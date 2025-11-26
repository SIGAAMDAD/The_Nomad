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
	
	GUIDEModifierNegate
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	[Tool]
	public sealed partial class GUIDEModifierNegate : GUIDEModifier {
		[Export]
		private bool X {
			get => _x;
			set {
				if ( _x == value ) {
					return;
				}
				_x = value;
				UpdateCaches();
				EmitChanged();
			}
		}
		private bool _x;

		[Export]
		private bool Y {
			get => _y;
			set {
				if ( _y == value ) {
					return;
				}
				_y = value;
				UpdateCaches();
				EmitChanged();
			}
		}
		private bool _y;

		[Export]
		private bool Z {
			get => _z;
			set {
				if ( _z == value ) {
					return;
				}
				_z = value;
				UpdateCaches();
				EmitChanged();
			}
		}
		private bool _z;

		public override string? EditorName => "Negate";
		public override string? EditorDescription => "Inverts input per axis.";

		private Vector3 Multiplier = Vector3.One * -1.0f;

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
			return other is GUIDEModifierNegate negate && Equals( negate );
		}

		/*
		===============
		ModifyInput
		===============
		*/
		public override Vector3 ModifyInput( Vector3 input, float delta, GUIDEAction.GUIDEActionValueType valueType ) {
			if ( !input.IsFinite() ) {
				return Vector3.Inf;
			}
			return input * Multiplier;
		}

		/*
		===============
		Equals
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool Equals( GUIDEModifierNegate other ) {
			if ( _x != other._x ) {
				return false;
			}
			if ( _y != other._y ) {
				return false;
			}
			if ( _z != other._z ) {
				return false;
			}
			return true;
		}

		private void UpdateCaches() {
			Multiplier.X = _x ? -1.0f : 1.0f;
			Multiplier.Y = _y ? -1.0f : 1.0f;
			Multiplier.Z = _z ? -1.0f : 1.0f;
		}
	};
};