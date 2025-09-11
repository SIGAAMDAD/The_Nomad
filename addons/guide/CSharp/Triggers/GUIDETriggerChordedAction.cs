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
	
	GUIDETriggerChordedAction
	
	===================================================================================
	*/
	/// <summary>
	/// Fires when the given action is currently triggering. This trigger is implicit,
	/// so it will prevent the action from triggering even if other triggers are successful.
	/// </summary>
	
	[Tool]
	public sealed partial class GUIDETriggerChordedAction : GUIDETrigger {
		[Export]
		public GUIDEAction Action;

		/*
		===============
		IsSameAs
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public override bool IsSameAs( GUIDETrigger other ) {
			if ( other is GUIDETriggerChordedAction chordedAction ) {
				return Action == chordedAction.Action;
			}
			return false;
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
			if ( Action == null ) {
				Console.PrintWarning( "GUIDETriggerChordedAction.UpdateState: chorded trigger without action will never trigger" );
				return GUIDETriggerState.None;
			}
			return Action.IsTriggered() ? GUIDETriggerState.Triggered : GUIDETriggerState.None;
		}

		/*
		===============
		EditorName
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public override string EditorName() {
			return "Chorded Action";
		}

		/*
		===============
		EditorDescription
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public override string EditorDescription() {
			return "Fires, when the given action is currently triggering. This trigger is implicit,\nso it will prevent the action from triggering even if other triggers are successful.";
		}
	};
};