/*
===========================================================================
The Nomad AGPL Source Code
Copyright (C) 2025 Noah Van Til

The Nomad Source Code is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

The Nomad Source Code is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with The Nomad Source Code.  If not, see <http://www.gnu.org/licenses/>.

If you have questions concerning this license or the applicable additional
terms, you may contact me via email at nyvantil@gmail.com.
===========================================================================
*/

using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace GUIDE {
	public sealed partial class GUIDERemapper : RefCounted {
		public struct ConfigItem {
			public event Action<ConfigItem, GUIDEInput> Changed;

			public GUIDEMappingContext Context { get; private set; }
			public readonly GUIDEAction Action;
			public readonly int Index;

			private readonly GUIDEInputMapping InputMapping;

			public readonly string DisplayCategory {
				get => GetEffectiveDisplayCategory( Action, InputMapping );
			}
			public readonly string DisplayName {
				get => GetEffectiveDisplayName( Action, InputMapping );
			}
			public readonly bool IsRemappable {
				get => IsEffectivelyRemappable( Action, InputMapping );
			}
			public readonly GUIDEAction.GUIDEActionValueType ValueType {
				get => GetEffectiveValueType( Action, InputMapping );
			}

			/*
			===============
			ConfigItem
			===============
			*/
			public ConfigItem( GUIDEMappingContext context, GUIDEAction action, int index, GUIDEInputMapping inputMapping ) {
				Context = context;
				Action = action;
				Index = index;
				InputMapping = inputMapping;
			}

			/*
			===============
			EmitSignalChanged
			===============
			*/
			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			public void EmitSignalChanged( ConfigItem item, GUIDEInput input ) {
				Changed?.Invoke( item, input );
			}

			/*
			===============
			IsSameAs
			===============
			*/
			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			public bool IsSameAs( ConfigItem other ) {
				return Context == other.Context && Action == other.Action && Index == other.Index;
			}

			/*
			===============
			ItemChanged
			===============
			*/
			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			public void ItemChanged( ConfigItem item, GUIDEInput input ) {
				if ( IsSameAs( item ) ) {
					Changed.Invoke( item, input );
				}
			}
		};

		public GUIDERemappingConfig RemappingConfig = new GUIDERemappingConfig();
		public List<GUIDEMappingContext> MappingContexts = new List<GUIDEMappingContext>();

		public event Action<ConfigItem, GUIDEInput> ItemChanged;

		/*
		===============
		Initialize
		===============
		*/
		public void Initialize( GUIDEMappingContext[] mappingContexts, GUIDERemappingConfig remappingConfig ) {
			RemappingConfig = remappingConfig != null ? (GUIDERemappingConfig)remappingConfig.Duplicate() : new GUIDERemappingConfig();
			MappingContexts.Clear();

			for ( int i = 0; i < mappingContexts.Length; i++ ) {
				if ( !IsInstanceValid( mappingContexts[ i ] ) ) {
					Console.PrintError( $"GUIDERemapper.Initialize: cannot add null mapping context, ignoring" );
					return;
				}
				MappingContexts.Add( mappingContexts[ i ] );
			}
		}

		/*
		===============
		GetMappingConfig
		===============
		*/
		public GUIDERemappingConfig GetRemappingConfig() {
			return (GUIDERemappingConfig)RemappingConfig.Duplicate();
		}

		/*
		===============
		SetCustomData
		===============
		*/
		public void SetCustomData( Variant key, Variant value ) {
			RemappingConfig.CustomData[ key ] = value;
		}

		/*
		===============
		GetCustomData
		===============
		*/
		public Variant GetCustomData( Variant key ) {
			return RemappingConfig.CustomData[ key ];
		}

		/*
		===============
		RemoveCustomData
		===============
		*/
		public void RemoveCustomData( Variant key ) {
			RemappingConfig.CustomData.Remove( key );
		}

		/*
		===============
		GetRemappableItems
		===============
		*/
		public List<ConfigItem> GetRemappableItems( GUIDEMappingContext context = null, string displayCategory = "", GUIDEAction action = null ) {
			if ( action != null && !action.IsRemappable ) {
				Console.PrintWarning( $"GUIDERemapper.GetRemappableItems: action filter was set but filtered action is not remappable" );
				return new List<ConfigItem>();
			} else if ( context == null ) {
				return new List<ConfigItem>();
			}

			List<ConfigItem> items = new List<ConfigItem>();
			for ( int i = 0; i < MappingContexts.Count; i++ ) {
				if ( MappingContexts[ i ] != context ) {
					continue;
				}
				for ( int a = 0; a < MappingContexts[ i ].Mappings.Length; a++ ) {
					GUIDEAction mappedAction = MappingContexts[ i ].Mappings[ a ].Action;
					if ( !mappedAction.IsRemappable ) {
						continue;
					} else if ( action != null && action != mappedAction ) {
						continue;
					}

					for ( int index = 0; index < MappingContexts[ i ].Mappings[ a ].InputMappings.Length; index++ ) {
						GUIDEInputMapping inputMapping = MappingContexts[ i ].Mappings[ a ].InputMappings[ index ];
						if ( inputMapping.OverrideActionSettings && !inputMapping.IsRemappable ) {
							continue;
						}

						string effectiveDisplayCategory = GetEffectiveDisplayCategory( mappedAction, inputMapping );
						if ( displayCategory.Length > 0 && effectiveDisplayCategory != displayCategory ) {
							continue;
						}

						ConfigItem item = new ConfigItem( MappingContexts[ i ], MappingContexts[ i ].Mappings[ a ].Action, index, inputMapping );
						ItemChanged += item.EmitSignalChanged;
						items.Add( item );
					}
				}
			}
			return items;
		}

		/*
		===============
		GetEffectiveDisplayCategory
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private static string GetEffectiveDisplayCategory( GUIDEAction action, GUIDEInputMapping inputMapping ) {
			if ( inputMapping.OverrideActionSettings ) {
				return inputMapping.DisplayCategory.Length > 0 ? inputMapping.DisplayCategory : "";
			}
			return action.DisplayCategory;
		}

		/*
		===============
		GetEffectiveDisplayName
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private static string GetEffectiveDisplayName( GUIDEAction action, GUIDEInputMapping inputMapping ) {
			if ( inputMapping.OverrideActionSettings ) {
				return inputMapping.DisplayName.Length > 0 ? inputMapping.DisplayName : "";
			}
			return action.DisplayName;
		}

		/*
		===============
		IsEffectivelyRemappable
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private static bool IsEffectivelyRemappable( GUIDEAction action, GUIDEInputMapping inputMapping ) {
			return action.IsRemappable && ( !inputMapping.OverrideActionSettings || inputMapping.IsRemappable );
		}

		/*
		===============
		GetEffectiveValueType
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private static GUIDEAction.GUIDEActionValueType GetEffectiveValueType( GUIDEAction action, GUIDEInputMapping inputMapping ) {
			return inputMapping.OverrideActionSettings && inputMapping.Input != null ? inputMapping.Input.NativeValueType() : action.ActionValueType;
		}

		/*
		===============
		GetInputCollisions
		===============
		*/
		public List<ConfigItem> GetInputCollisions( ConfigItem item, GUIDEInput input ) {
			if ( !CheckItem( item ) ) {
				return new List<ConfigItem>();
			}

			List<ConfigItem> items = new List<ConfigItem>();
			if ( input == null ) {
				return items;
			}

			for ( int i = 0; i < MappingContexts.Count; i++ ) {
				for ( int a = 0; a < MappingContexts[ i ].Mappings.Length; a++ ) {
					for ( int index = 0; index < MappingContexts[ i ].Mappings[ a ].InputMappings.Length; index++ ) {
						GUIDEAction action = MappingContexts[ i ].Mappings[ a ].Action;
						if ( MappingContexts[ i ] == item.Context && action == item.Action && index == item.Index ) {
							continue; // we're allowed to hit ourselves
						}

						GUIDEInputMapping inputMapping = MappingContexts[ i ].Mappings[ a ].InputMappings[ index ];
						GUIDEInput boundInput = inputMapping.Input;
						if ( RemappingConfig.Has( MappingContexts[ i ], action, index ) ) {
							boundInput = RemappingConfig.GetBoundInputOrNull( MappingContexts[ i ], action, index );
						}

						// we have a collision
						if ( boundInput != null && boundInput.IsSameAs( input ) ) {
							ConfigItem collisionItem = new ConfigItem( MappingContexts[ i ], action, index, inputMapping );
							ItemChanged += collisionItem.EmitSignalChanged;
							items.Add( collisionItem );
						}
					}
				}
			}

			return items;
		}

		/*
		===============
		GetBoundInputOrNull
		===============
		*/
		public GUIDEInput GetBoundInputOrNull( ConfigItem item ) {
			if ( !CheckItem( item ) ) {
				return null;
			}
			if ( RemappingConfig.Has( item.Context, item.Action, item.Index ) ) {
				return RemappingConfig.GetBoundInputOrNull( item.Context, item.Action, item.Index );
			}

			for ( int i = 0; i < item.Context.Mappings.Length; i++ ) {
				if ( item.Context.Mappings[ i ].Action == item.Action ) {
					if ( item.Context.Mappings[ i ].InputMappings.Length > item.Index ) {
						return item.Context.Mappings[ i ].InputMappings[ item.Index ].Input;
					} else {
						Console.PrintError( $"GUIDERemapper.GetBoundInputOrNull: action mapping does not have an index of {item.Index}" );
					}
				}
			}
			return null;
		}

		/*
		===============
		SetBoundInput
		===============
		*/
		public void SetBoundInput( ConfigItem item, GUIDEInput input ) {
			if ( !CheckItem( item ) ) {
				return;
			}

			RemappingConfig.Clear( item.Context, item.Action, item.Index );

			GUIDEInput boundInput = GetBoundInputOrNull( item );
			if ( boundInput == null ) {
				if ( input != null ) {
					RemappingConfig.Bind( item.Context, item.Action, input, item.Index );
				}
				ItemChanged?.Invoke( item, input );
				return;
			} else if ( boundInput != null && input != null && boundInput.IsSameAs( input ) ) {
				ItemChanged?.Invoke( item, input );
				return;
			}
			RemappingConfig.Bind( item.Context, item.Action, input, item.Index );
			ItemChanged?.Invoke( item, input );
		}

		/*
		===============
		GetDefaultInput
		===============
		*/
		public GUIDEInput GetDefaultInput( ConfigItem item ) {
			if ( !CheckItem( item ) ) {
				return null;
			}
			for ( int i = 0; i < item.Context.Mappings.Length; i++ ) {
				if ( item.Context.Mappings[ i ].Action == item.Action ) {
					return item.Context.Mappings[ i ].InputMappings[ item.Index ].Input;
				}
			}
			return null;
		}

		/*
		===============
		RestoreDefaultFor
		===============
		*/
		public void RestoreDefaultFor( ConfigItem item ) {
			if ( !CheckItem( item ) ) {
				return;
			}
			RemappingConfig.Clear( item.Context, item.Action, item.Index );
			ItemChanged?.Invoke( item, GetBoundInputOrNull( item ) );
		}

		/*
		===============
		CheckItem
		===============
		*/
		private bool CheckItem( ConfigItem item ) {
			if ( !MappingContexts.Contains( item.Context ) ) {
				Console.PrintError( $"GUIDERemapper.CheckItem: given context is not known to this mapper. Did you call Initialize()?" );
				return false;
			}

			bool actionFound = false;
			bool sizeOk = false;
			for ( int i = 0; i < item.Context.Mappings.Length; i++ ) {
				if ( item.Context.Mappings[ i ].Action == item.Action ) {
					actionFound = true;
					if ( item.Context.Mappings[ i ].InputMappings.Length > item.Index && item.Index >= 0 ) {
						sizeOk = true;
					}
					break;
				}
			}
			if ( !actionFound ) {
				Console.PrintError( "GUIDERemapper.CheckItem: given action does not belong to the given context" );
				return false;
			}
			if ( !sizeOk ) {
				Console.PrintError( "GUIDERemapper.CheckItem: given index does not exist for the given action's input binding" );
				return false;
			}
			if ( !item.Action.IsRemappable ) {
				Console.PrintError( "GUIDERemapper.CheckItem: given action is not remappable" );
				return false;
			}

			return true;
		}
	};
};