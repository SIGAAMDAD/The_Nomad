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
using System;
using System.Runtime.CompilerServices;

namespace GUIDE {
	/*
	===================================================================================
	
	GUIDEModifierPositiveNegative
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public sealed partial class GUIDEModifierPositiveNegative : GUIDEModifier {
		public enum LimitRange : uint {
			Positive = 1,
			Negative = 2
		};

		[Export]
		private LimitRange Range = LimitRange.Positive;
		[Export]
		private bool X {
			get => _x;
			set {
				if ( _x == value ) {
					return;
				}
				_x = value;
				EmitChanged();
			}
		}
		private bool _x = true;

		[Export]
		private bool Y {
			get => _y;
			set {
				if ( _y == value ) {
					return;
				}
				_y = value;
				EmitChanged();
			}
		}
		private bool _y = true;

		[Export]
		private bool Z {
			get => _z;
			set {
				if ( _z == value ) {
					return;
				}
				_z = value;
				EmitChanged();
			}
		}
		private bool _z = true;

		public override string? EditorName => "Positive/Negative";
		public override string? EditorDescription => "Clamps the input ot positive or negative values.";

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
			return other is GUIDEModifierPositiveNegative positiveNegative && Equals( positiveNegative );
		}

		/*
		===============
		ModifyInput
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="input"></param>
		/// <param name="delta"></param>
		/// <param name="valueType"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException"></exception>
		public override Vector3 ModifyInput( Vector3 input, float delta, GUIDEAction.GUIDEActionValueType valueType ) {
			if ( !input.IsFinite() ) {
				return Vector3.Inf;
			}

			return Range switch {
				LimitRange.Positive => new Vector3( GetMax( _x, input.X ), GetMax( _y, input.Y ), GetMax( _z, input.Z ) ),
				LimitRange.Negative => new Vector3( GetMin( _x, input.X ), GetMin( _y, input.Y ), GetMin( _z, input.Z ) ),
				_ => throw new InvalidOperationException( "Range isn't valid!" )
			};
		}

		/*
		===============
		GetMax
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <param name="input"></param>
		/// <returns></returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private float GetMax( bool value, float input ) {
			return value ? Mathf.Max( 0.0f, input ) : input;
		}
		
		/*
		===============
		GetMin
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <param name="input"></param>
		/// <returns></returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private float GetMin( bool value, float input ) {
			return value ? Mathf.Min( 0.0f, input ) : input;
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
		public bool Equals( GUIDEModifierPositiveNegative other ) {
			if ( Range != other.Range ) {
				return false;
			}
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
	};
};