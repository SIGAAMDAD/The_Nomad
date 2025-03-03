using Godot;
using GodotSteam;

[GlobalClass]
public partial class SteamManager : Node {
	private static ulong VIP_ID = 76561199403850315;

	private static uint SteamAppID = 3512240;
	private static ulong SteamID = 0;
	private static string SteamUsername;

	private bool IsMe = false;

	public static uint GetAppID() {
		return SteamAppID;
	}
	public static uint GetSteamID() {
		return (uint)SteamID;
	}
	public static string GetSteamName() {
		return SteamUsername;
	}

    public override void _Ready() {
		if ( Engine.HasSingleton( "Steam" ) ) {
			OS.SetEnvironment( "SteamAppId", SteamAppID.ToString() );
			OS.SetEnvironment( "SteamGameId", SteamAppID.ToString() );

			Steam.SteamInitEx( false );

			bool isSteamRunning = Steam.IsSteamRunning();
			if ( !isSteamRunning ) {
				GD.PushError( "Steam is not running." );
				return;
			}

			SteamID = Steam.GetSteamID();
			SteamUsername = Steam.GetFriendPersonaName( SteamID );

			if ( SteamID == VIP_ID ) {
				IsMe = true;
				GD.Print( "'ello" );
			}

			GD.Print( "SteamAPI initialized with username " + SteamUsername );
		} else {
			SteamID = 0;
			SteamUsername = "";
		}
	}

	public override void _Process( double delta ) {
		Steam.RunCallbacks();
	}

	public static void SaveCloudFile( string path ) {
		if ( !Steam.IsCloudEnabledForAccount() || !Steam.IsCloudEnabledForApp() ) {
			GD.Print( "SteamCloud isn't enabled for this application or the account" );
			return;
		}

		FileAccess file = FileAccess.Open( "user://" + path, FileAccess.ModeFlags.Read );
		if ( file == null ) {
			GD.PushError( "Error opening file \"" + path + "\" in read mode!" );
			return;
		}

		file.SeekEnd();
		ulong length = file.GetPosition();
		file.Seek( 0 );
		byte[] data = file.GetBuffer( (long)length );

		GD.Print( "Saving file \"" + path + "\" to SteamCloud..." );
		Steam.FileWrite( path, data, (long)length );
	}
};