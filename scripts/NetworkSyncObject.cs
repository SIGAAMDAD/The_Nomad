using System.IO;
using System.Runtime.CompilerServices;
using Steamworks;

public class NetworkSyncObject {
	private readonly byte[] Packet = null;
	private readonly MemoryStream Stream = null;
	private readonly BinaryWriter Writer = null;
	private readonly BinaryReader Reader = null;

	public NetworkSyncObject( int nPacketSize ) {
		Packet = new byte[ nPacketSize ];
		Stream = new MemoryStream( Packet );
		Writer = new BinaryWriter( Stream );
		Reader = new BinaryReader( Stream );
	}

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public Godot.Vector2 ReadVector2() {
		Godot.Vector2 value = Godot.Vector2.Zero;
		value.X = Reader.Read();
		value.Y = Reader.Read();
		return value;
	}
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public ulong ReadUInt64() => Reader.ReadUInt64();
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public uint ReadUInt32() => Reader.ReadUInt32();
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public float ReadFloat() => (float)Reader.ReadDouble();
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public byte ReadByte() => Reader.ReadByte();
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public sbyte ReadSByte() => Reader.ReadSByte();
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public int ReadInt32() => Reader.ReadInt32();
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public string ReadString() => Reader.ReadString();
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public bool ReadBoolean() => Reader.ReadBoolean();

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public void Write( Godot.Vector2 value ) {
		Writer.Write( (double)value.X );
		Writer.Write( (double)value.Y );
	}
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public void Write( ulong value ) => Writer.Write( value );
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public void Write( uint value ) => Writer.Write( value );
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public void Write( float value ) => Writer.Write( (double)value );
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
	public void Sync( CSteamID target ) {
		// send the packet
		SteamLobby.Instance.SendTargetPacket( target, Packet );

		// rewind
		Stream.Seek( 0, SeekOrigin.Begin );
	}
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public void Sync() {
		// send the packet
		SteamLobby.Instance.SendP2PPacket( Packet );

		// rewind
		Stream.Seek( 0, SeekOrigin.Begin );
	}
};