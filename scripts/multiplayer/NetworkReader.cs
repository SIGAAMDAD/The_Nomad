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
using System.IO;
using System.Runtime.CompilerServices;

namespace Multiplayer {
	/*
	===================================================================================
	
	NetworkReader
	
	===================================================================================
	*/
	/// <summary>
	/// A struct meant for reading from a network stream
	/// </summary>

	public readonly struct NetworkReader : IDisposable {
		private readonly BinaryReader? Reader;

		/*
		===============
		NetworkReader
		===============
		*/
		/// <summary>
		/// Constructs a NetworkReader with the provided <paramref name="reader"/> as the stream
		/// </summary>
		/// <param name="reader">The stream to read from</param>
		public NetworkReader( BinaryReader reader ) {
			Reader = reader;
		}

		/*
		===============
		Dispose
		===============
		*/
		public void Dispose() {
		}

		/*
		===============
		ReadVector2
		===============
		*/
		/// <summary>
		/// Reads a Godot.Vector2 from the network stream
		/// </summary>
		/// <returns>The resulting Godot.Vector2 after quantization</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public Godot.Vector2 ReadVector2() {
			ArgumentNullException.ThrowIfNull( Reader );

			short packed = Reader.ReadInt16();
			sbyte x = (sbyte)( packed >> 8 );
			sbyte y = (sbyte)( packed & 0xFF );

			Godot.Vector2I value = new Godot.Vector2I( x, y );

			return new Godot.Vector2(
				value.X * NetworkSyncObject.GRID_STEP,
				value.Y * NetworkSyncObject.GRID_STEP
			);
		}

		/*
		===============
		ReadUInt8
		===============
		*/
		/// <summary>
		/// Reads an 8-bit unsigned integer value from the network stream
		/// </summary>
		/// <returns>The value read from the stream</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public byte ReadUInt8() {
			ArgumentNullException.ThrowIfNull( Reader );
			return Reader.ReadByte();
		}

		/*
		===============
		ReadUInt16
		===============
		*/
		/// <summary>
		/// Reads a 16-bit unsigned integer value from the network stream
		/// </summary>
		/// <returns>The value read from the stream</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public ushort ReadUInt16() {
			ArgumentNullException.ThrowIfNull( Reader );
			return Reader.ReadUInt16();
		}

		/*
		===============
		ReadUInt32
		===============
		*/
		/// <summary>
		/// Reads a 32-bit unsigned integer value from the network stream
		/// </summary>
		/// <returns>The value read from the stream</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public uint ReadUInt32() {
			ArgumentNullException.ThrowIfNull( Reader );
			return Reader.ReadUInt32();
		}

		/*
		===============
		ReadUInt64
		===============
		*/
		/// <summary>
		/// Reads a 64-bit unsigned integer value from the network stream
		/// </summary>
		/// <returns>The value read from the stream</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public ulong ReadUInt64() {
			ArgumentNullException.ThrowIfNull( Reader );
			return Reader.ReadUInt64();
		}

		/*
		===============
		ReadInt8
		===============
		*/
		/// <summary>
		/// Reads an 8-bit signed integer value from the network stream
		/// </summary>
		/// <returns>The value read from the stream</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public sbyte ReadInt8() {
			ArgumentNullException.ThrowIfNull( Reader );
			return Reader.ReadSByte();
		}

		/*
		===============
		ReadInt16
		===============
		*/
		/// <summary>
		/// Reads a 16-bit signed integer value from the network stream
		/// </summary>
		/// <returns>The value read from the stream</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public short ReadInt16() {
			ArgumentNullException.ThrowIfNull( Reader );
			return Reader.ReadInt16();
		}

		/*
		===============
		ReadInt32
		===============
		*/
		/// <summary>
		/// Reads a 32-bit signed integer value from the network stream
		/// </summary>
		/// <returns>The value read from the stream</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public int ReadInt32() {
			ArgumentNullException.ThrowIfNull( Reader );
			return Reader.ReadInt32();
		}

		/*
		===============
		ReadInt64
		===============
		*/
		/// <summary>
		/// Reads a 64-bit signed integer value from the network stream
		/// </summary>
		/// <returns>The value read from the stream</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public long ReadInt64() {
			ArgumentNullException.ThrowIfNull( Reader );
			return Reader.ReadInt64();
		}

		/*
		===============
		ReadFloat
		===============
		*/
		/// <summary>
		/// Reads a 32-bit floating point value from the network stream
		/// </summary>
		/// <remarks>
		/// Converted from a half (16-bit floating value) in the network stream
		/// to a 32-bit floating point value
		/// </remarks>
		/// <returns>The value read from the stream</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public float ReadFloat() {
			ArgumentNullException.ThrowIfNull( Reader );
			return (float)Reader.ReadHalf();
		}

		/*
		===============
		ReadString
		===============
		*/
		/// <summary>
		/// Reads a string value from the network stream
		/// </summary>
		/// <returns>The value read from the stream</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public string ReadString() {
			ArgumentNullException.ThrowIfNull( Reader );
			return Reader.ReadString();
		}

		/*
		===============
		ReadBoolean
		===============
		*/
		/// <summary>
		/// Reads a boolean value from the network stream
		/// </summary>
		/// <returns>The value read from the stream</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public bool ReadBoolean() {
			ArgumentNullException.ThrowIfNull( Reader );
			return Reader.ReadBoolean();
		}

		/*
		===============
		ReadPackedInt
		===============
		*/
		/// <summary>
		/// Reads a 7-bit encoded/packed value from the network stream
		/// </summary>
		/// <returns>The value read from the stream</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public int ReadPackedInt() {
			ArgumentNullException.ThrowIfNull( Reader );
			return Reader.Read7BitEncodedInt();
		}
	};
};