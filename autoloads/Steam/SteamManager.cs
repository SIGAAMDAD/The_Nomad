using System;
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

	private SteamAPIWarningMessageHook_t DebugMessageHook;

	private Callback<GameOverlayActivated_t> GameOverlayActivated;

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
		Console.PrintLine( "Loading Steam DLC information..." );

		int dlcCount = SteamApps.GetDLCCount();
		if ( dlcCount == 0 ) {
			Console.PrintLine( "...None installed" );
			return;
		}

		Console.PrintLine( string.Format( "...Found {0} DLC packets", dlcCount ) );
		for ( int i = 0; i < dlcCount; i++ ) {
			bool available;
			string name;
			AppId_t dlcId;

			if ( SteamApps.BGetDLCDataByIndex( i, out dlcId, out available, out name, 256 ) ) {
				Console.PrintError( string.Format( "...Couldn't load info for {0}", i ) );
				continue;
			}
			Console.PrintLine( string.Format( "...Got DLC Packet \"{0}\", status: {1}", name, available.ToString() ) );
		}
	}

	private void OnGameOverlayActived( GameOverlayActivated_t pCallback ) {
		if ( pCallback.m_nAppID != SteamAppID ) {
			Console.PrintError( "[STEAM] Invalid GameOverlay AppID!" );
			return;
		}

		Console.PrintLine( "[STEAM] In-Game overlay activated." );
		
		if ( Convert.ToBoolean( pCallback.m_bActive ) ) {
			// feed a pause event
			InputEventKey keyEvent = new InputEventKey();
			keyEvent.PhysicalKeycode = Key.Escape;
			Input.ParseInputEvent( keyEvent );
		}
	}

	public override void _ExitTree() {
		base._ExitTree();

		SteamAPI.Shutdown();
	}
    public override void _Ready() {
		if ( !SteamAPI.IsSteamRunning() ) {
			GD.Print( "Steam isn't running, not initializing SteamAPI" );
			return;
		}
		
		string errorMessage;
		
		if ( SteamAPI.InitEx( out errorMessage ) != ESteamAPIInitResult.k_ESteamAPIInitResult_OK ) {
			Console.PrintError( string.Format( "[STEAM] Error initializing SteamAPI: {0}", errorMessage ) );
			return;
		}

		DebugMessageHook = new SteamAPIWarningMessageHook_t( SteamAPIDebugTextCallback );
		SteamClient.SetWarningMessageHook( DebugMessageHook );

		LoadAppInfo();
		LoadUserInfo();
		LoadDLCInfo();

		GameOverlayActivated = Callback<GameOverlayActivated_t>.Create( OnGameOverlayActived );
	}
	public override void _Process( double delta ) {
		base._Process( delta );

		SteamAPI.RunCallbacks();
	}

    private void SteamAPIDebugTextCallback( int nSeverity, System.Text.StringBuilder debugText ) {
		Console.PrintLine( string.Format( "[STEAM] {0}", debugText.ToString() ) );
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