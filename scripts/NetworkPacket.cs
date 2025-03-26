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

	public void Write( uint value ) => Writer.Write( value );
	public void Write( float value ) => Writer.Write( (double)value );
	public void Write( byte value ) => Writer.Write( value );
	public void Write( sbyte value ) => Writer.Write( value );
	public void Write( int value ) => Writer.Write( value );
	public void Write( string value ) => Writer.Write( value );
	public void Write( bool value ) => Writer.Write( value );

	public void Sync( CSteamID target ) {
		// send the packet
		SteamLobby.Instance.SendP2PPacket( target, Packet );

		// rewind
		Stream.Seek( 0, SeekOrigin.Begin );
	}
	public void Sync() {
		Sync( CSteamID.Nil );
	}
};