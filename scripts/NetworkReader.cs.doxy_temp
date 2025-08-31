using System.IO;

public class NetworkReader {
	private BinaryReader Reader = null;

	public NetworkReader() {
	}

	public void BeginRead( BinaryReader stream ) => Reader = stream;

	public Godot.Vector2 ReadVector2() {
		Godot.Vector2 value = Godot.Vector2.Zero;
		value.X = Reader.Read();
		value.Y = Reader.Read();
		return value;
	}
	public ulong ReadUInt64() => Reader.ReadUInt64();
	public uint ReadUInt32() => Reader.ReadUInt32();
	public float ReadFloat() => (float)Reader.ReadDouble();
	public byte ReadByte() => Reader.ReadByte();
	public sbyte ReadSByte() => Reader.ReadSByte();
	public int ReadInt32() => Reader.ReadInt32();
	public string ReadString() => Reader.ReadString();
	public bool ReadBoolean() => Reader.ReadBoolean();
};