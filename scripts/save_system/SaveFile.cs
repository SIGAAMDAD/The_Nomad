using Godot;

public class SaveFile {
	private const uint MAGIC = 0x387AFFA3;

	private uint VersionMajor;
	private uint VersionMinor;
	private uint VersionPatch;

	public SaveFile() {
	}

	private void Load( FileAccess file ) {
		uint Magic = file.Get32();

		if ( Magic != MAGIC ) {
			GD.PushError( "SaveFile::Load: file doesn't have correct magic!" );
			return;
		}

		VersionMajor = file.Get32();
		VersionMinor = file.Get32();
		VersionPatch = file.Get32();
	}
};