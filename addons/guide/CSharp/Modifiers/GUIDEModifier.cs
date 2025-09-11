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
	[Tool]
	[Icon( "res://addons/guide/modifiers/guide_modifier.svg" )]
	public partial class GUIDEModifier : Resource {
		/*
		===============
		IsSameAs
		===============
		*/
		/// <summary>
		/// Returns whether this modifier is the same as the other modifier.
		/// This is used to determine if a modifier can be reused during context switching.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool IsSameAs( GUIDEModifier other ) {
			return this == other;
		}

		/*
		===============
		BeginUsage
		===============
		*/
		/// <summary>
		/// Called when the modifier is started to be used by GUIDE. Can be used to perform
		/// initializations.
		/// </summary>
		public virtual void BeginUsage() {
		}

		/*
		===============
		EndUsage
		===============
		*/
		/// <summary>
		/// Called when the modifier is no longer used by GUIDE. Can be used to perform
		/// cleanup.
		/// </summary>
		public virtual void EndUsage() {
		}

		/*
		===============
		ModifyInput
		===============
		*/
		/// <summary>
		/// Called to modify the input value before it is passed to the triggers.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="delta"></param>
		/// <param name="valueType"></param>
		/// <returns></returns>
		public virtual Vector3 ModifyInput( Vector3 input, float delta, GUIDEAction.GUIDEActionValueType valueType ) {
			return input;
		}

		/*
		===============
		EditorName
		===============
		*/
		/// <summary>
		/// The name as it should be displayed in the editor
		/// </summary>
		/// <returns></returns>
		public virtual string EditorName() {
			return "";
		}

		/*
		===============
		EditorDescription
		===============
		*/
		/// <summary>
		/// The description as it should be displayed in the editor
		/// </summary>
		/// <returns></returns>
		public virtual string EditorDescription() {
			return "";
		}
	};
};