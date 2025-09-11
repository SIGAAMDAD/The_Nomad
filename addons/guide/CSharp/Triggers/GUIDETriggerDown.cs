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
	
	GUIDETriggerDown
	
	===================================================================================
	*/
	/// <summary>
	/// Fires when the input exceeds the actuation threshold. This is
	/// the default trigger when no trigger is specified
	/// </summary>
	
	[Tool]
	public sealed partial class GUIDETriggerDown : GUIDETrigger {
		/*
		===============
		IsSameAs
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public override bool IsSameAs( GUIDETrigger other ) {
			return other is GUIDETriggerDown;
		}

		/*
		===============
		UpdateState
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public override GUIDETriggerState UpdateState( Vector3 input, float delta, GUIDEAction.GUIDEActionValueType valueType ) {
			return IsActuated( input, valueType ) ? GUIDETriggerState.Triggered : GUIDETriggerState.None;
		}

		/*
		===============
		EditorName
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public override string EditorName() {
			return "Down";
		}

		/*
		===============
		EditorDescription
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public override string EditorDescription() {
			return "Fires, when the input exceeds the actuation threshold. This is\nthe default trigger when no trigger is specified";
		}
	};
};