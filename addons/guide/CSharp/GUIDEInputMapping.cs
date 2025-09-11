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
using System.Collections.Generic;

namespace GUIDE {
	[Tool]
	[Icon( "res://addons/guide/guide_internal.svg" )]
	public sealed partial class GUIDEInputMapping : Resource {
		/// <summary>
		/// Whether the remapping configuration in this input mapping
		/// should override the configuration of the bound action. Enable
		/// this, to give a key a custom name or category for remapping.
		/// </summary>
		[Export]
		public bool OverrideActionSettings {
			get => _overrideActionSettings;
			set {
				if ( _overrideActionSettings == value ) {
					return;
				}
				_overrideActionSettings = value;
				EmitChanged();
			}
		}

		/// <summary>
		/// If true, players can remap this input mapping. Note that the
		/// action to which this input is bound also needs to be remappable
		/// for this setting to have an effect.
		/// </summary>
		[Export]
		public bool IsRemappable {
			get => _isRemappable;
			set {
				if ( _isRemappable == value ) {
					return;
				}
				_isRemappable = value;
				EmitChanged();
			}
		}

		/// <summary>
		/// The display name of the input mapping shown to the player. If empty,
		/// the display name of the action is used.
		/// </summary>
		[Export]
		public string? DisplayName {
			get => _displayName;
			set {
				if ( _displayName == value ) {
					return;
				}
				_displayName = value;
				EmitChanged();
			}
		}

		/// <summary>
		/// The display category of the input mapping. If empty, the display name of the
		/// action is used.
		/// </summary>
		[Export]
		public string? DisplayCategory {
			get => _displayCategory;
			set {
				if ( _displayCategory == value ) {
					return;
				}
				_displayCategory = value;
				EmitChanged();
			}
		}

		/// <summary>
		/// The input to be actuated
		/// </summary>
		[Export]
		public GUIDEInput? Input {
			get => _input;
			set {
				if ( _input == value ) {
					return;
				}
				_input = value;
				EmitChanged();
			}
		}

		/// <summary>
		/// A list of modifiers that preprocess the actuated input before
		/// it is fed to the triggers.
		/// </summary>
		[Export]
		public Godot.Collections.Array<GUIDEModifier> Modifiers {
			get => _modifiers;
			set {
				if ( _modifiers == value ) {
					return;
				}
				_modifiers = value;
				EmitChanged();
			}
		}

		/// <summary>
		/// A list of triggers that could trigger the mapped action
		/// </summary>
		[Export]
		public Godot.Collections.Array<GUIDETrigger> Triggers {
			get => _triggers;
			set {
				if ( _triggers == value ) {
					return;
				}
				_triggers = value;
				EmitChanged();
			}
		}

		private bool _overrideActionSettings = false;
		private bool _isRemappable = false;
		private string? _displayName = "";
		private string? _displayCategory = "";
		private GUIDEInput? _input;
		private Godot.Collections.Array<GUIDEModifier> _modifiers = new Godot.Collections.Array<GUIDEModifier>();
		private Godot.Collections.Array<GUIDETrigger> _triggers = new Godot.Collections.Array<GUIDETrigger>();

		/// <summary>
		/// Hint for how long the input must remain actuated (in seconds) before the mapping triggers.
		/// If the mapping has no hold trigger it will be -1. If it has multiple hold triggers
		/// the shortest hold time will be used
		/// </summary>
		private float TriggerHoldThreshold = -1.0f;

		private GUIDETrigger.GUIDETriggerState State = GUIDETrigger.GUIDETriggerState.None;
		private Vector3 Value = Vector3.Zero;

		private List<GUIDETrigger> TriggerList = new List<GUIDETrigger>();
		private int ImplicitCount = 0;
		private int ExplicitCount = 0;

		/*
		===============
		Initialize
		===============
		*/
		/// <summary>
		/// Called when the mapping is started to be used by GUIDE. Calculates
		/// the number of implicit and explicit triggers so we don't need to do this
		/// per frame. Also creates a default trigger when none is set.
		/// Finally initializes the LastValue of all triggers to the current
		/// state of the input.
		/// </summary>
		/// <param name="valueType"></param>
		public void Initialize( GUIDEAction.GUIDEActionValueType valueType ) {
			TriggerList.Clear();

			ImplicitCount = 0;
			ExplicitCount = 0;
			TriggerHoldThreshold = -1.0f;

			if ( Triggers.Count == 0 ) {
				// make a default trigger and use that
				return;
			}
		}
	};
};