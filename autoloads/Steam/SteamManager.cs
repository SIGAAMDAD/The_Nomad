using Godot;
using Steamworks;

[GlobalClass]
public partial class SteamManager : Node {
	private static ulong VIP_ID = 76561199403850315;

	private static AppId_t SteamAppID;
	private static int SteamAppBuildID;
	private static string GameLanguage;

	private static CSteamID SteamID = CSteamID.Nil;
	private static string SteamUsername;

	private static uint TimedTrialSecondsAllowed;
	private static uint TimedTrialSecondsPlayed;

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

		GD.Print( "Language: " + GameLanguage );
		GD.Print( "AppId: " + SteamAppID );
		GD.Print( "AppBuildId: " + SteamAppBuildID );
	}
	private static void LoadUserInfo() {
		SteamID = SteamUser.GetSteamID();
		SteamUsername = SteamFriends.GetPersonaName();

		GD.Print( "SteamUser.Id: " + SteamID );
		GD.Print( "SteamUser.UserName: " + SteamUsername );
	}

    public override void _Ready() {
		/*
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
		*/

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
	}

	private void SteamAPIDebugTextCallback( int nSeverity, System.Text.StringBuilder debugText ) {
		GetNode( "/root/Console" ).Call( "print_line", "[STEAM] " + debugText.ToString() );
	}

	public override void _Process( double delta ) {
		SteamAPI.RunCallbacks();
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