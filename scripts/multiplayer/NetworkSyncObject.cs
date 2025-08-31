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

using System.IO;
using System.Runtime.CompilerServices;
using Steamworks;
using Steam;
using System;
using Godot;

namespace Multiplayer {
	/*
	===================================================================================

	NetworkSyncObject

	===================================================================================
	*/
	/// <summary>
	/// An abstraction wrapper around writing to a System.IO.MemoryStream for networking purposes
	/// </summary>
	public class NetworkSyncObject {
		public static readonly float GRID_STEP = 16.0f;

		private readonly byte[] Packet = null;
		private readonly MemoryStream Stream = null;
		private readonly BinaryWriter Writer = null;
		private BinaryReader Reader = null;

		/*
		===============
		NetworkSyncObject
		===============
		*/
		/// <summary>
		/// The base constructor for a NetworkSyncObject
		/// </summary>
		/// <param name="packetSize">The size of the packet</param>
		public NetworkSyncObject( int packetSize ) {
			Packet = new byte[ packetSize ];
			Stream = new MemoryStream( Packet );
			Writer = new BinaryWriter( Stream );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void BeginRead( BinaryReader reader ) => Reader = reader;
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public Godot.Vector2 ReadVector2() {
			short packed = Reader.ReadInt16();
			sbyte x = (sbyte)( packed >> 8 );
			sbyte y = (sbyte)( packed & 0xFF );

			Godot.Vector2I value = new Godot.Vector2I( x, y );

			return new Godot.Vector2(
				value.X * GRID_STEP,
				value.Y * GRID_STEP
			);
		}
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public ulong ReadUInt64() => Reader.ReadUInt64();
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public uint ReadUInt32() => Reader.ReadUInt32();
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public ushort ReadUInt16() => Reader.ReadUInt16();
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public float ReadFloat() => (float)Reader.ReadHalf();
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public byte ReadByte() => Reader.ReadByte();
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public sbyte ReadSByte() => Reader.ReadSByte();
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public int ReadInt32() => Reader.ReadInt32();
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public short ReadInt16() => Reader.ReadInt16();
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public string ReadString() => Reader.ReadString();
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public bool ReadBoolean() => Reader.ReadBoolean();
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public int ReadPackedInt() => Reader.Read7BitEncodedInt();

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void Write( Godot.Vector2 value ) {
			Godot.Vector2I position = new Godot.Vector2I(
				(int)Mathf.Round( value.X / GRID_STEP ),
				(int)Mathf.Round( value.Y / GRID_STEP )
			);

			sbyte x = (sbyte)Mathf.Clamp( position.X, -128, 127 );
			sbyte y = (sbyte)Mathf.Clamp( position.Y, -128, 127 );

			Writer.Write( (short)( ( x << 8 ) | ( y & 0xFF ) ) );
		}
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void Write( ulong value ) => Writer.Write( value );
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void Write( uint value ) => Writer.Write( value );
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void Write( float value ) => Writer.Write( (Half)value );
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void Write( byte value ) => Writer.Write( value );
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void Write( sbyte value ) => Writer.Write( value );
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void Write( int value ) => Writer.Write( value );
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void Write( string value ) => Writer.Write( value );
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void Write( bool value ) => Writer.Write( value );
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void Write( byte[] value ) => Writer.Write( value );
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void WritePackedInt( int value ) => Writer.Write7BitEncodedInt( value );

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void Sync( CSteamID target, bool safe = false ) {
			// send the packet
			SteamLobby.Instance.SendTargetPacket( target, Packet, Constants.k_nSteamNetworkingSend_Reliable, safe );

			// rewind
			Stream.Seek( 0, SeekOrigin.Begin );
		}
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void ServerSync() {
			// send the packet
			SteamLobby.Instance.SendTargetPacket( SteamLobby.Instance.LobbyOwnerId, Packet, Constants.k_nSteamNetworkingSend_Reliable );

			// rewind
			Stream.Seek( 0, SeekOrigin.Begin );
		}
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void Sync( int nSendType = Constants.k_nSteamNetworkingSend_Reliable ) {
			// send the packet
			SteamLobby.Instance.SendP2PPacket( Packet, nSendType );

			// rewind
			Stream.Seek( 0, SeekOrigin.Begin );
		}
	};
};