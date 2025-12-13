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
	
	GUIDEInput
	
	===================================================================================
	*/
	/// <summary>
	/// A class representing some actuated input
	/// </summary>

	[Tool]
	[Icon( "res://addons/guide/inputs/guide_input.svg" )]
	public partial class GUIDEInput : Resource {
		protected Vector3 Value = Vector3.Zero;
		protected GUIDEInputState? State = null;

		/*
		===============
		NeedsReset
		===============
		*/
		public virtual bool NeedsReset() {
			return false;
		}

		/*
		===============
		Reeset
		===============
		*/
		public virtual void Reset() {
			Value = Vector3.Zero;
		}

		/*
		===============
		IsSameAs
		===============
		*/
		public virtual bool IsSameAs( GUIDEInput other ) {
			return false;
		}

		/*
		===============
		BeginUsage
		===============
		*/
		public virtual void BeginUsage() {
		}

		/*
		===============
		EndUsage
		===============
		*/
		public virtual void EndUsage() {
		}

		/*
		===============
		EditorName
		===============
		*/
		public virtual string EditorName() {
			return "";
		}

		/*
		===============
		EditorDescription
		===============
		*/
		public virtual string EditorDescription() {
			return "";
		}

		/*
		===============
		NativeValueType
		===============
		*/
		public virtual GUIDEAction.GUIDEActionValueType NativeValueType() {
			return GUIDEAction.GUIDEActionValueType.Invalid;
		}
	};
};