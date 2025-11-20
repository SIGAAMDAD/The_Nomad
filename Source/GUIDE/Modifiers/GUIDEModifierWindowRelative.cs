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

namespace GUIDE {
	/*
	===================================================================================
	
	GUIDEModifierWindowRelative
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public sealed partial class GUIDEModifierWindowRelative : GUIDEModifier {
		public override string? EditorName => "Window Relative";
		public override string? EditorDescription =>
			@"
			Converts the value of the input into window-relative units between 0 and 1.
			E.g. if a mouse cursor moves half a screen to the right and down, then
			this modifier will return (0.5, 0.5).
			";

		/*
		===============
		IsSameAs
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public override bool IsSameAs( GUIDEModifier other ) {
			return other is GUIDEModifierWindowRelative;
		}

		public override Vector3 ModifyInput( Vector3 input, float delta, GUIDEAction.GUIDEActionValueType valueType ) {
			if ( !input.IsFinite() ) {
				return Vector3.Inf;
			}
			Window window = (Window)Engine.GetMainLoop().Get( SceneTree.PropertyName.Root );
			Vector2 windowSize = window.GetScreenTransform().AffineInverse() * new Vector2( window.Size.X, window.Size.Y );
			return new Vector3( input.X / windowSize.X, input.Y / windowSize.Y, input.Z );
		}
	};
};