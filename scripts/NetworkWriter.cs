using System;
using System.IO;
using Steamworks;

public class NetworkWriter {
	private byte[] Packet = null;
	private MemoryStream Stream = null;
	private BinaryWriter Writer = null;

	public NetworkWriter( int nPacketMaxSize ) {
		Packet = new byte[ nPacketMaxSize ];
		Stream = new MemoryStream( Packet );
		Writer = new BinaryWriter( Stream );
	}

	public void WritePosition( Godot.Vector2 value ) {
		const float PRECISION = 0.01f; // 1cm precision
		const float MAX_VALUE = 1000.0f;

		ushort x = (ushort)( ( value.X + MAX_VALUE ) / PRECISION );
		ushort y = (ushort)( ( value.Y + MAX_VALUE ) / PRECISION );

		byte[] data = new byte[ 4 ];
		Buffer.BlockCopy( BitConverter.GetBytes( x ), 0, data, 0, 2 );
		Buffer.BlockCopy( BitConverter.GetBytes( y ), 0, data, 2, 2 );
		Writer.Write( data );
	}
	public void Write( Godot.Vector2 value ) {
		Writer.Write( (Half)value.X );
		Writer.Write( (Half)value.Y );
	}
	public void Write( ulong value ) => Writer.Write( value );
	public void Write( uint value ) => Writer.Write( value );
	public void Write( float value ) => Writer.Write( (Half)value );
	public void Write( byte value ) => Writer.Write( value );
	public void Write( sbyte value ) => Writer.Write( value );
	public void Write( int value ) => Writer.Write( value );
	public void Write( string value ) => Writer.Write( value );
	public void Write( bool value ) => Writer.Write( value );

	public void Sync( CSteamID target ) {
		// send the packet
		SteamLobby.Instance.SendMessage( target, Packet );

		// rewind
		Stream.Seek( 0, SeekOrigin.Begin );
	}
	public void Sync() {
		// send the packet
		SteamLobby.Instance.SendP2PPacket( Packet );

		// rewind
		Stream.Seek( 0, SeekOrigin.Begin );
	}
};