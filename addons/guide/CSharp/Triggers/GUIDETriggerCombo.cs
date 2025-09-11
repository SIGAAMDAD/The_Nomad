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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.Marshalling;

namespace GUIDE {
	/*
	===================================================================================
	
	GUIDETriggerCombo
	
	===================================================================================
	*/
	
	[Tool]
	public sealed partial class GUIDETriggerCombo : GUIDETrigger {
		public enum ActionEventType : uint {
			Triggered = 1,
			Started = 2,
			Ongoing = 4,
			Cancelled = 8,
			Completed = 16
		};

		/// <summary>
		/// If set to true, the combo trigger will print information
		/// about state changes to the console
		/// </summary>
		[Export]
		public bool EnableDebugPrint = false;
		[Export]
		public GUIDETriggerComboStep[] Steps;
		[Export]
		public GUIDETriggerComboCancelAction[] CancellationActions;

		private int CurrentStep = -1;
		private float RemainingTime = 0.0f;

		/*
		===============
		IsSameAs
		===============
		*/
		public override bool IsSameAs( GUIDETrigger other ) {
			if ( other is not GUIDETriggerCombo combo ) {
				return false;
			} else if ( Steps.Length != combo.Steps.Length ) {
				return false;
			} else if ( CancellationActions.Length != combo.CancellationActions.Length ) {
				return false;
			}

			for ( int i = 0; i < Steps.Length; i++ ) {
				if ( !Steps[ i ].IsSameAs( combo.Steps[ i ] ) ) {
					return false;
				}
			}
			for ( int i = 0; i < CancellationActions.Length; i++ ) {
				if ( !CancellationActions[ i ].IsSameAs( combo.CancellationActions[ i ] ) ) {
					return false;
				}
			}
			return true;
		}

		/*
		===============
		UpdateState
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public override GUIDETriggerState UpdateState( Vector3 input, float delta, GUIDEAction.GUIDEActionValueType valueType ) {
			if ( Steps.Length == 0 ) {
				Console.PrintWarning( "GUIDETriggerCombo.UpdateState: combo with no steps will never fire" );
				return GUIDETriggerState.None;
			}

			int i;
			if ( CurrentStep == -1 ) {
				for ( i = 0; i < Steps.Length; i++ ) {
					Steps[ i ].Prepare();
				}
				for ( i = 0; i < CancellationActions.Length; i++ ) {
					CancellationActions[ i ].Prepare();
				}
				Reset();
			}

			GUIDEAction currentAction = Steps[ CurrentStep ].Action;
			if ( currentAction == null ) {
				Console.PrintWarning( $"GUIDETriggerCombo.UpdateState: step {CurrentStep} has no action {ResourcePath}" );
				return GUIDETriggerState.None;
			}

			for ( i = 0; i < CancellationActions.Length; i++ ) {
				if ( CancellationActions[ i ].Action == currentAction ) {
					continue;
				}
				if ( CancellationActions[ i ].HasFired ) {
					if ( EnableDebugPrint ) {
						Console.PrintDebug( $"GUIDETriggerCombo.UpdateState: combo cancelled by action '{CancellationActions[ i ].Action.EditorName()}'" );
					}
					Reset();
					return GUIDETriggerState.None;
				}
			}
			for ( i = 0; i < Steps.Length; i++ ) {
				if ( Steps[ i ].Action == currentAction ) {
					continue;
				}
				if ( Steps[ i ].HasFired ) {
					if ( EnableDebugPrint ) {
						Console.PrintDebug( $"GUIDETriggerCombo.UpdateState: combo out of order step by action '{Steps[ i ].Action.EditorName()}'" );
					}
					Reset();
					return GUIDETriggerState.None;
				}
			}

			// check if we took too long (unless we're in the first step)
			if ( CurrentStep > 0 ) {
				RemainingTime -= delta;
				if ( RemainingTime <= 0.0f ) {
					if ( EnableDebugPrint ) {
						Console.PrintDebug( $"GUIDETriggerCombo.UpdateState: step time for step {CurrentStep} exceeded" );
					}
					Reset();
					return GUIDETriggerState.None;
				}
			}

			if ( Steps[ CurrentStep ].HasFired ) {
				Steps[ CurrentStep ].HasFired = false;
				if ( CurrentStep + 1 >= Steps.Length ) {
					if ( EnableDebugPrint ) {
						Console.PrintDebug( $"GUIDETriggerCombo.UpdateState: combo fired" );
					}
					Reset();
					return GUIDETriggerState.Triggered;
				}

				CurrentStep++;
				if ( EnableDebugPrint ) {
					Console.PrintDebug( $"GUIDETriggerCombo.UpdateState: combo advanced to step {CurrentStep}." );
				}
				RemainingTime = Steps[ CurrentStep ].TimeToActuate;

				for ( i = 0; i < Steps.Length; i++ ) {
					Steps[ i ].HasFired = false;
				}
				for ( i = 0; i < CancellationActions.Length; i++ ) {
					CancellationActions[ i ].HasFired = false;
				}
			}

			// we're still processing
			return GUIDETriggerState.Ongoing;
		}

		/*
		===============
		Reset
		===============
		*/
		private void Reset() {
			if ( EnableDebugPrint ) {
				Console.PrintDebug( "GUIDETriggerCombo.Reset: combo reset" );
			}
			CurrentStep = 0;
			RemainingTime = Steps[ 0 ].TimeToActuate;
			for ( int i = 0; i < Steps.Length; i++ ) {
				Steps[ i ].HasFired = false;
			}
			for ( int i = 0; i < CancellationActions.Length; i++ ) {
				CancellationActions[ i ].HasFired = false;
			}
		}

		/*
		===============
		EditorName
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public override string EditorName() {
			return "Combo";
		}

		/*
		===============
		EditorDescription
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public override string EditorDescription() {
			return "Fires, when the input exceeds the actuation threshold.";
		}
	};
};