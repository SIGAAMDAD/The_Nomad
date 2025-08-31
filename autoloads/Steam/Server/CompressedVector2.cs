/*
===========================================================================
Copyright (C) 2023-2025 Noah Van Til

This file is part of The Nomad source code.

The Nomad source code is free software; you can redistribute it
and/or modify it under the terms of the GNU Affero General Public License as
published by the Free Software Foundation; either version 3 of the License,
or (at your option) any later version.

The Nomad source code is distributed in the hope that it will be
useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with The Nomad source code; if not, write to the Free Software
Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301 USA
===========================================================================
*/

using Godot;
using System;
using System.Runtime.CompilerServices;

namespace Steam.Server {
	/*
	===================================================================================
	
	CompressedVector2
	
	===================================================================================
	*/
	
	public struct CompressedVector2 {
		public enum Axis {
			X,
			Y
		};

		public sbyte X;
		public sbyte Y;

		public static readonly float GRID_STEP = 16.0f;

		public sbyte this[ int index ] {
			readonly get {
				return index switch {
					0 => X,
					1 => Y,
					_ => throw new ArgumentOutOfRangeException( nameof( index ) ),
				};
			}
			set {
				switch ( index ) {
					case 0:
						X = value;
						break;
					case 1:
						Y = value;
						break;
					default:
						throw new ArgumentOutOfRangeException( nameof( index ) );
				}
			}
		}

		public CompressedVector2( float x, float y ) {
			Vector2I rounded = new Vector2I(
				(int)Mathf.Round( x / GRID_STEP ),
				(int)Mathf.Round( y / GRID_STEP )
			);

			X = (sbyte)Mathf.Clamp( rounded.X, -127, 127 );
			Y = (sbyte)Mathf.Clamp( rounded.Y, -127, 127 );
		}
		
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static explicit operator short( CompressedVector2 value ) {
			return (short)( ( value.X << 8 ) | ( value.Y & 0xFF ) );
		}
	};
};