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
	
	GUIDEModifierMapRange
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	[Tool]
	public sealed partial class GUIDEModifierMapRange : GUIDEModifier {
		[Export]
		private bool ApplyClamp = true;
		[Export]
		private float InputMin = 0.0f;
		[Export]
		private float InputMax = 1.0f;
		[Export]
		private float OutputMin = 0.0f;
		[Export]
		private float OutputMax = 1.0f;
		[Export]
		private bool X = true;
		[Export]
		private bool Y = true;
		[Export]
		private bool Z = true;

		public override string? EditorName => "Map Range";
		public override string? EditorDescription => "Maps an input range to an output range and optionally clamps the output";

		private float _omin = 0.0f;
		private float _omax = 0.0f;

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
			return other is GUIDEModifierMapRange mapRange && Equals( mapRange );
		}

		/*
		===============
		BeginUsage
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		public override void BeginUsage() {
			_omin = Mathf.Min( OutputMin, OutputMax );
			_omax = Mathf.Max( OutputMin, OutputMax );
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
		public override Vector3 ModifyInput( Vector3 input, float delta, GUIDEAction.GUIDEActionValueType valueType ) {
			if ( !input.IsFinite() ) {
				return Vector3.Inf;
			}

			return new Vector3(
				MapValue( X, input.X ),
				MapValue( Y, input.Y ),
				MapValue( Z, input.Z )
			);
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
		public bool Equals( GUIDEModifierMapRange other ) {
			if ( ApplyClamp != other.ApplyClamp ) {
				return false;
			}
			if ( X != other.X ) {
				return false;
			}
			if ( Y != other.Y ) {
				return false;
			}
			if ( Z != other.Z ) {
				return false;
			}
			if ( Mathf.IsEqualApprox( InputMin, other.InputMin ) ) {
				return false;
			}
			if ( Mathf.IsEqualApprox( InputMax, other.InputMax ) ) {
				return false;
			}
			if ( Mathf.IsEqualApprox( OutputMin, other.OutputMin ) ) {
				return false;
			}
			if ( Mathf.IsEqualApprox( OutputMax, other.OutputMax ) ) {
				return false;
			}
			return true;
		}

		/*
		===============
		MapValue
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="final"></param>
		/// <param name="input"></param>
		/// <returns></returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private float MapValue( bool final, float input ) {
			float value = Mathf.Remap( input, InputMin, InputMax, OutputMin, OutputMax );
			if ( ApplyClamp ) {
				value = Mathf.Clamp( value, _omin, _omax );
			}
			return final ? value : input;
		}
	};
};