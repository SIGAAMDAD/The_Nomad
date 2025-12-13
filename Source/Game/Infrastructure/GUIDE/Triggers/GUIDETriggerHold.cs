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
	
	GUIDETriggerHold
	
	===================================================================================
	*/
	/// <summary>
	/// A trigger that activates when the input is held down for a certain amount of time
	/// </summary>

	[Tool]
	public sealed partial class GUIDETriggerHold : GUIDETrigger {
		/// <summary>
		/// A trigger that activates when the input is held down for a certain amount
		/// of time
		/// </summary>
		[Export]
		public float HoldThreshold = 1.0f;

		/// <summary>
		/// If true, the trigger will only fire once until the input is released.
		/// Otherwise the trigger will fire every frame.
		/// </summary>
		[Export]
		public bool IsOneShot = false;

		private float AccumulatedTime = 0.0f;
		private bool DidShoot = false;

		/*
		===============
		IsSameAs
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public override bool IsSameAs( GUIDETrigger other ) {
			if ( other is GUIDETriggerHold hold ) {
				return IsOneShot == hold.IsOneShot && Mathf.IsEqualApprox( AccumulatedTime, hold.AccumulatedTime );
			}
			return false;
		}

		/*
		===============
		UpdateState
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public override GUIDETriggerState UpdateState( Vector3 input, float delta, GUIDEAction.GUIDEActionValueType valueType ) {
			if ( IsActuated( input, valueType ) ) {
				AccumulatedTime += delta;

				return AccumulatedTime >= HoldThreshold ?
					IsOneShot && DidShoot ? GUIDETriggerState.None : GUIDETriggerState.Triggered
					:
					GUIDETriggerState.Ongoing;
			}
			AccumulatedTime = 0.0f;
			DidShoot = false;
			return GUIDETriggerState.None;
		}

		/*
		===============
		EditorName
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public override string EditorName() {
			return "Hold";
		}

		/*
		===============
		EditorDescription
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public override string EditorDescription() {
			return "Fires, once the input has remained actuated for hold_threshold seconds.\nMight fire once or repeatedly.";
		}
	};
};