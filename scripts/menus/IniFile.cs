using System.IO;
using System.Runtime.InteropServices;
using System.Text;

public class IniFile {
	private string Path;

	[DllImport("kernel32", CharSet = CharSet.Unicode)]
	static extern long WritePrivateProfileString( string section, string key, string value, string filePath );
	[DllImport("kernel32", CharSet = CharSet.Unicode)]
	static extern int GetPrivateProfileString( string section, string key, string _default, StringBuilder retVal, int size, string filePath );

	public IniFile( string path ) {
		Path = new FileInfo( path ).FullName;
	}

	public string Read( string key, string section ) {
		StringBuilder ret = new StringBuilder( 255 );
		GetPrivateProfileString( section, key, "", ret, 255, Path );
		return ret.ToString();
	}
	public void Write( string key, string value, string section ) {
		WritePrivateProfileString( section, key, value, Path );
	}
	public void DeleteKey( string key, string section ) {
		Write( key, null, section );
	}
	public void DeleteSection( string section ) {
		Write( null, null, section );
	}
	public bool KeyExists( string key, string section ) {
		return Read( key, section ).Length > 0;
	}
};