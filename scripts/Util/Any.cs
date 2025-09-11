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

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Util {
	/*
	===================================================================================
	
	Any
	
	===================================================================================
	*/
	/// <summary>
	/// A more performant version of object, but can only store primitives
	/// </summary>

	public struct Any {
		[StructLayout( LayoutKind.Explicit )]
		private struct AnyData {
			[FieldOffset( 0 )] private sbyte Int8;
			[FieldOffset( 0 )] private short Int16;
			[FieldOffset( 0 )] private int Int32;
			[FieldOffset( 0 )] private long Int64;
			[FieldOffset( 0 )] private byte UInt8;
			[FieldOffset( 0 )] private ushort UInt16;
			[FieldOffset( 0 )] private uint UInt32;
			[FieldOffset( 0 )] private ulong UInt64;
			[FieldOffset( 0 )] private Half Half;
			[FieldOffset( 0 )] private float Float;
			[FieldOffset( 0 )] private double Double;
			[FieldOffset( 0 )] private string String;

			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			public void Set( sbyte data ) {
				Int8 = data;
			}

			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			public void Set( short data ) {
				Int16 = data;
			}

			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			public void Set( int data ) {
				Int32 = data;
			}

			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			public void Set( long data ) {
				Int64 = data;
			}

			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			public void Set( byte data ) {
				UInt8 = data;
			}

			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			public void Set( ushort data ) {
				UInt16 = data;
			}

			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			public void Set( uint data ) {
				UInt32 = data;
			}

			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			public void Set( ulong data ) {
				UInt64 = data;
			}

			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			public void Set( Half data ) {
				Half = data;
			}

			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			public void Set( float data ) {
				Float = data;
			}

			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			public void Set( double data ) {
				Double = data;
			}

			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			public void Set( string data ) {
				String = data;
			}
		};

		private AnyData Data { get; set; } = new AnyData();

		public Any( sbyte data ) {
			Data.Set( data );
		}
		public Any( short data ) {
			Data.Set( data );
		}
		public Any( int data ) {
			Data.Set( data );
		}
		public Any( long data ) {
			Data.Set( data );
		}
		public Any( byte data ) {
			Data.Set( data );
		}
		public Any( ushort data ) {
			Data.Set( data );
		}
		public Any( uint data ) {
			Data.Set( data );
		}
		public Any( ulong data ) {
			Data.Set( data );
		}
		public Any( Half data ) {
			Data.Set( data );
		}
		public Any( float data ) {
			Data.Set( data );
		}
		public Any( double data ) {
			Data.Set( data );
		}
		public Any( string data ) {
			Data.Set( data );
		}
	};
};