using System.Collections.Generic;
using System.Threading;
using Godot;
using Steamworks;

[GlobalClass]
public partial class SteamManager : Node {
	private static ulong VIP_ID = 76561199403850315;

	private static List<string> DlcList = null;

	private static AppId_t SteamAppID;
	private static int SteamAppBuildID;
	private static string GameLanguage;

	private static CSteamID SteamID = CSteamID.Nil;
	private static string SteamUsername;

	private static uint TimedTrialSecondsAllowed;
	private static uint TimedTrialSecondsPlayed;

	private bool Quit = false;
	private Thread APIThread = null;

	private SteamAPIWarningMessageHook_t DebugMessageHook;

	private bool IsMe = false;

	public static AppId_t GetAppID() {
		return SteamAppID;
	}
	public static CSteamID GetSteamID() {
		return SteamID;
	}
	public static string GetSteamName() {
		return SteamUsername;
	}

	private static void LoadAppInfo() {
		GameLanguage = SteamApps.GetCurrentGameLanguage();
		SteamAppID = SteamUtils.GetAppID();
		SteamAppBuildID = SteamApps.GetAppBuildId();

		bool bIsTimedTrial;
		if ( bIsTimedTrial = SteamApps.BIsTimedTrial( out TimedTrialSecondsAllowed, out TimedTrialSecondsPlayed ) ) {
			if ( TimedTrialSecondsPlayed > TimedTrialSecondsAllowed ) {
				GD.PushError( "[STEAM] Timed trial has expired" );
			}
		}

		GD.Print( "...Language: " + GameLanguage );
		GD.Print( "...AppId: " + SteamAppID );
		GD.Print( "...AppBuildId: " + SteamAppBuildID );
	}
	private static void LoadUserInfo() {
		SteamID = SteamUser.GetSteamID();
		SteamUsername = SteamFriends.GetPersonaName();

		GD.Print( "...SteamUser.Id: " + SteamID );
		GD.Print( "...SteamUser.UserName: " + SteamUsername );
	}
	private void LoadDLCInfo() {
		GetNode( "/root/Console" ).Call( "print_line", "Loading Steam DLC information...", true );

		int dlcCount = SteamApps.GetDLCCount();
		if ( dlcCount == 0 ) {
			GetNode( "/root/Console" ).Call( "print_line", "...None installed", true );
			return;
		}

		GetNode( "/root/Console" ).Call( "print_line", "...Found " + dlcCount + " DLC packets", true );
		for ( int i = 0; i < dlcCount; i++ ) {
			bool available;
			string name;
			AppId_t dlcId;

			if ( SteamApps.BGetDLCDataByIndex( i, out dlcId, out available, out name, 256 ) ) {
				GetNode( "/root/Console" ).Call( "print_error", "...Couldn't load info for " + i, true );
				continue;
			}
			GetNode( "/root/Console" ).Call( "print_line", "...Got DLC Packet \"" + name + "\", status: " + available.ToString(), true );
		}
	}

	public override void _ExitTree() {
		base._ExitTree();
		Quit = true;
		APIThread.Join();
	}
    public override void _Ready() {
		if ( !SteamAPI.IsSteamRunning() ) {
			GD.Print( "Steam isn't running, not initializing SteamAPI" );
			return;
		}
		
		string errorMessage;
		
		if ( SteamAPI.InitEx( out errorMessage ) != ESteamAPIInitResult.k_ESteamAPIInitResult_OK ) {
			GD.PushError( "[STEAM] Error initializing SteamAPI: " + errorMessage );
			return;
		}

		DebugMessageHook = new SteamAPIWarningMessageHook_t( SteamAPIDebugTextCallback );
		SteamClient.SetWarningMessageHook( DebugMessageHook );

		LoadAppInfo();
		LoadUserInfo();
		LoadDLCInfo();

		APIThread = new Thread( () => {
			while ( !Quit ) {
				Thread.Sleep( 1500 );
				SteamAPI.RunCallbacks();
			}
		} );
		APIThread.Start();
	}

    private void SteamAPIDebugTextCallback( int nSeverity, System.Text.StringBuilder debugText ) {
		GetNode( "/root/Console" ).Call( "print_line", "[STEAM] " + debugText.ToString(), true );
	}

    public static void SaveCloudFile( string path ) {
		if ( !SteamRemoteStorage.IsCloudEnabledForAccount() || !SteamRemoteStorage.IsCloudEnabledForApp() ) {
			GD.Print( "SteamCloud isn't enabled for this application or the account" );
			return;
		}

		string realpath = ProjectSettings.GlobalizePath( "user://" + path );
		System.IO.FileStream stream = new System.IO.FileStream( realpath, System.IO.FileMode.Open );

		stream.Seek( 0, System.IO.SeekOrigin.End );
		long length = stream.Position;
		stream.Seek( 0, System.IO.SeekOrigin.Begin );

		byte[] buffer = new byte[ length ];
		stream.Read( buffer );

		GD.Print( "Saving file \"" + path + "\" to SteamCloud..." );
		SteamRemoteStorage.FileWrite( path, buffer, buffer.Length );
	}
};