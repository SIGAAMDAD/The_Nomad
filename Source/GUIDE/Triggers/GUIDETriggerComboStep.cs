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
	
	GUIDETriggerComboStep
	
	===================================================================================
	*/

	[Tool]
	[Icon( "res://addons/guide/guide_internal.svg" )]
	public sealed partial class GUIDETriggerComboStep : Resource {
		[Export]
		public GUIDEAction Action;
		[Export( PropertyHint.Flags, "Triggered:1,Started:2,Ongoing:4,Cancelled:8,Completed:16" )]
		public GUIDETriggerCombo.ActionEventType CompletionEvents = GUIDETriggerCombo.ActionEventType.Triggered;
		[Export]
		public float TimeToActuate = 0.5f;

		public bool HasFired = false;

		/*
		===============
		IsSameAs
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public bool IsSameAs( GUIDETriggerComboStep other ) {
			return Action == other.Action && CompletionEvents == other.CompletionEvents && Mathf.IsEqualApprox( TimeToActuate, other.TimeToActuate );
		}

		/*
		===============
		Prepare
		===============
		*/
		public void Prepare() {
			if ( ( CompletionEvents & GUIDETriggerCombo.ActionEventType.Triggered ) != 0 ) {
				Action.Triggered += Fired;
			} else if ( ( CompletionEvents & GUIDETriggerCombo.ActionEventType.Started ) != 0 ) {
				Action.Started += Fired;
			} else if ( ( CompletionEvents & GUIDETriggerCombo.ActionEventType.Ongoing ) != 0 ) {
				Action.Ongoing += Fired;
			} else if ( ( CompletionEvents & GUIDETriggerCombo.ActionEventType.Cancelled ) != 0 ) {
				Action.Cancelled += Fired;
			} else if ( ( CompletionEvents & GUIDETriggerCombo.ActionEventType.Completed ) != 0 ) {
				Action.Completed += Fired;
			}
			HasFired = false;
		}

		/*
		===============
		Fired
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private void Fired() {
			HasFired = true;
		}
	};
};