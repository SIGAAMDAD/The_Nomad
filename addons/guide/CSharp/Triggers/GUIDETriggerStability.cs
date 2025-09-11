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

namespace GUIDE {
	/*
	===================================================================================
	
	GUIDETriggerStability

	===================================================================================
	*/
	/// <summary>
	/// ## Triggers depending on whether the input changes while actuated. This trigger is
	/// is implicit, so it must succeed for all other triggers to succeed.
	/// </summary>
	
	[Tool]
	public sealed partial class GUIDETriggerStability : GUIDETrigger {
		public enum TriggerWhen : uint {
			/// <summary>
			/// Input must be stable
			/// </summary>
			InputIsStable,

			/// <summary>
			/// Input must change
			/// </summary>
			InputChanges
		};

		/// <summary>
		/// The maximum amount that the input can change after actuation before it is
		/// considered "changed"
		/// </summary>
		[Export]
		public float MaxDeviation = 1.0f;

		/// <summary>
		/// When should the trigger... trigger?
		/// </summary>
		[Export]
		public TriggerWhen Trigger = TriggerWhen.InputIsStable;

		private Vector3 InitialValue = Vector3.Zero;
		private bool Deviated = false;

		/*
		===============
		IsSameAs
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public override bool IsSameAs( GUIDETrigger other ) {
			return other is GUIDETriggerStability stability ?
					Trigger == stability.Trigger && Mathf.IsEqualApprox( MaxDeviation, stability.MaxDeviation )
				:
					false;
		}

		/*
		===============
		GetTriggerType
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public override GUIDETriggerType GetTriggerType() {
			return GUIDETriggerType.Implicit;
		}

		/*
		===============
		UpdateState
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public override GUIDETriggerState UpdateState( Vector3 input, float delta, GUIDEAction.GUIDEActionValueType valueType ) {
			if ( IsActuated( input, valueType ) ) {
				if ( Deviated ) {
					return Trigger == TriggerWhen.InputIsStable ? GUIDETriggerState.None : GUIDETriggerState.Triggered;
				}
				if ( !IsActuated( LastValue, valueType ) ) {
					InitialValue = input;
					return Trigger == TriggerWhen.InputIsStable ? GUIDETriggerState.Triggered : GUIDETriggerState.Ongoing;
				}
				if ( InitialValue.DistanceSquaredTo( input ) > ( MaxDeviation * MaxDeviation ) ) {
					Deviated = true;
					return Trigger == TriggerWhen.InputIsStable ? GUIDETriggerState.None : GUIDETriggerState.Triggered;
				}
				return Trigger == TriggerWhen.InputIsStable ? GUIDETriggerState.Triggered : GUIDETriggerState.Ongoing;
			}
			Deviated = false;
			return GUIDETriggerState.None;
		}

		/*
		===============
		EditorName
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public override string EditorName() {
			return "Stability";
		}

		/*
		===============
		EditorDescription
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public override string EditorDescription() {
			return "Triggers depending on whether the input changes while actuated. This trigger\nis implicit, so it must succeed for all other triggers to succeed.";
		}
	};
};