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

using System.Runtime.CompilerServices;
using System.IO;
using System;
using System.Buffers;
using Godot;
using Steamworks;
using Steam;

namespace Multiplayer {
	/*
	===================================================================================
	
	NetworkWriter
	
	===================================================================================
	*/
	/// <summary>
	/// A struct meant for writing to a network stream
	/// </summary>

	public readonly struct NetworkWriter : IDisposable {
		public static readonly int MAX_PACKET_SIZE = 4096;
		
		private readonly MemoryStream? Stream = null;
		private readonly BinaryWriter? Writer = null;
		private readonly int SendType = Constants.k_nSteamNetworkingSend_Unreliable;

		/*
		===============
		NetworkWriter
		===============
		*/
		/// <summary>
		/// The base constructor for a NetworkWriter
		/// </summary>
		/// <param name="packetSize">The length in bytes of the packet to be sent over a connection</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="sendType"/> is invalid, or if <paramref name="packetSize"/> is greater than 4 KiB</exception>
		public NetworkWriter( int packetSize, int sendType ) {
			ArgumentOutOfRangeException.ThrowIfLessThanOrEqual( packetSize, 0 );
			if ( sendType < Constants.k_nSteamNetworkingSend_Unreliable || sendType > Constants.k_nSteamNetworkingSend_ReliableNoNagle ) {
				throw new ArgumentOutOfRangeException( $"invalid sendType {sendType}" );
			}
			if ( packetSize > MAX_PACKET_SIZE ) {
				throw new ArgumentOutOfRangeException( $"packetSize ({packetSize}) is larger than 4 KiB" );
			}

			SendType = sendType;

			Stream = new MemoryStream( ArrayPool<byte>.Shared.Rent( packetSize ) );
			Writer = new BinaryWriter( Stream );
		}

		/*
		===============
		Sync
		===============
		*/
		/// <summary>
		/// Sends the written packet over a connection to all other peers
		/// </summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private void Sync() {
			ArgumentNullException.ThrowIfNull( Stream );

			// send the packet
			SteamLobby.Instance.SendP2PPacket( Stream.GetBuffer(), SendType );
		}

		/*
		===============
		Dispose
		===============
		*/
		/// <summary>
		/// Returns the MemoryStream's buffer to the shared ArrayPool and sends the packet
		/// </summary>
		/// <remarks>
		/// The packet is only sent if it's been written to
		/// </remarks>
		public void Dispose() {
			ArgumentNullException.ThrowIfNull( Stream );

			if ( Stream.Length > 0 ) {
				Sync();
			}

			ArrayPool<byte>.Shared.Return( Stream.GetBuffer() );
		}

		/*
		===============
		WriteVector2
		===============
		*/
		/// <summary>
		/// Write a Godot.Vector2 to the network stream
		/// </summary>
		/// <remarks>
		/// Quantization occurs to reduce bandwitdth usage, the Vector2 is converted to
		/// a packed short (1 byte for each coordinate)
		/// </remarks>
		/// <param name="value">The value to write</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void WriteVector2( Godot.Vector2 value ) {
			ArgumentNullException.ThrowIfNull( Writer );

			Godot.Vector2I position = new Godot.Vector2I(
				(int)Mathf.Round( value.X / NetworkSyncObject.GRID_STEP ),
				(int)Mathf.Round( value.Y / NetworkSyncObject.GRID_STEP )
			);

			sbyte x = (sbyte)Mathf.Clamp( position.X, -128, 127 );
			sbyte y = (sbyte)Mathf.Clamp( position.Y, -128, 127 );

			Writer.Write( (short)( ( x << 8 ) | ( y & 0xFF ) ) );
		}

		/*
		===============
		WriteUInt8
		===============
		*/
		/// <summary>
		/// Writes an 8-bit integer to the network stream
		/// </summary>
		/// <param name="value">The value to write</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void WriteUInt8( byte value ) {
			ArgumentNullException.ThrowIfNull( Writer );
			Writer.Write( value );
		}

		/*
		===============
		WriteUInt16
		===============
		*/
		/// <summary>
		/// Writes a 16-bit integer to the network stream
		/// </summary>
		/// <param name="value">The value to write</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void WriteUInt16( ushort value ) {
			ArgumentNullException.ThrowIfNull( Writer );
			Writer.Write( value );
		}

		/*
		===============
		WriteUInt32
		===============
		*/
		/// <summary>
		/// Writes a 32-bit integer to the network stream
		/// </summary>
		/// <param name="value">The value to write</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void WriteUInt32( uint value ) {
			ArgumentNullException.ThrowIfNull( Writer );
			Writer.Write( value );
		}

		/*
		===============
		WriteULong
		===============
		*/
		/// <summary>
		/// Writes
		/// </summary>
		/// <param name="value"></param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void WriteULong( ulong value ) {
			ArgumentNullException.ThrowIfNull( Writer );
			Writer.Write( value );
		}

		/*
		===============
		WriteInt8
		===============
		*/
		/// <summary>
		/// Writes an 8-bit integer to the network stream
		/// </summary>
		/// <param name="value">The value to write</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void Write( sbyte value ) {
			ArgumentNullException.ThrowIfNull( Writer );
			Writer.Write( value );
		}

		/*
		===============
		WriteInt16
		===============
		*/
		/// <summary>
		/// Writes a 16-bit integer to the network stream
		/// </summary>
		/// <param name="value">The value to write</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void WriteInt16( short value ) {
			ArgumentNullException.ThrowIfNull( Writer );
			Writer.Write( value );
		}

		/*
		===============
		WriteInt32
		===============
		*/
		/// <summary>
		/// Writes a 32-bit integer to the network stream
		/// </summary>
		/// <param name="value">The value to write</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void WriteInt32( int value ) {
			ArgumentNullException.ThrowIfNull( Writer );
			Writer.Write( value );
		}

		/*
		===============
		WriteInt64
		===============
		*/
		/// <summary>
		/// Writes a 64-bit integer to the network stream
		/// </summary>
		/// <param name="value">The value to write</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void WriteInt64( long value ) {
			ArgumentNullException.ThrowIfNull( Writer );
			Writer.Write( value );
		}

		/*
		===============
		WriteString
		===============
		*/
		/// <summary>
		/// Writes a string to the network stream
		/// </summary>
		/// <param name="value">The value to write</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void WriteString( string value ) {
			ArgumentNullException.ThrowIfNull( Writer );
			Writer.Write( value );
		}

		/*
		===============
		WriteBoolean
		===============
		*/
		/// <summary>
		/// Writes a boolean to the network stream
		/// </summary>
		/// <param name="value">The value to write</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void WriteBoolean( bool value ) {
			ArgumentNullException.ThrowIfNull( Writer );
			Writer.Write( value );
		}

		/*
		===============
		WriteFloat
		===============
		*/
		/// <summary>
		/// Writes a 32-bit floating point value to the network stream
		/// </summary>
		/// <remarks>
		/// The 32-bit float will always be converted to a half (16-bit floating point)
		/// to reduce bandwidth usage
		/// </remarks>
		/// <param name="value">The value to write</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void WriteFloat( float value ) {
			ArgumentNullException.ThrowIfNull( Writer );
			Writer.Write( (Half)value );
		}

		/*
		===============
		WriteByteArray
		===============
		*/
		/// <summary>
		/// Writes a byte array to the network stream
		/// </summary>
		/// <param name="value">The value to write</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void WriteByteArray( byte[] value ) {
			ArgumentNullException.ThrowIfNull( value );
			ArgumentNullException.ThrowIfNull( Writer );
			Writer.Write( value );
		}

		/*
		===============
		WritePackedInt
		===============
		*/
		/// <summary>
		/// Writes a 32-bit value as a 7-bit encoded/packed value to the network stream
		/// </summary>
		/// <param name="value">The value to write</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void WritePackedInt( int value ) {
			ArgumentNullException.ThrowIfNull( Writer );
			Writer.Write7BitEncodedInt( value );
		}
	};
};