/*
===========================================================================
The Nomad AGPL Source Code
Copyright (C) 2025 Noah Van Til

The Nomad Source Code is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

The Nomad Source Code is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with The Nomad Source Code.  If not, see <http://www.gnu.org/licenses/>.

If you have questions concerning this license or the applicable additional
terms, you may contact me via email at nyvantil@gmail.com.
===========================================================================
*/

using System;
using System.Collections.Generic;
using Godot;
using Steamworks;

namespace Steam {
	public partial class SteamManager : Node {
		public static CSteamID VIP_ID = (CSteamID)76561199403850315;

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

		private void LoadAppInfo() {
			GameLanguage = SteamApps.GetCurrentGameLanguage();
			SteamAppID = SteamUtils.GetAppID();
			SteamAppBuildID = SteamApps.GetAppBuildId();

			if ( SteamApps.BIsTimedTrial( out TimedTrialSecondsAllowed, out TimedTrialSecondsPlayed ) ) {
				if ( TimedTrialSecondsPlayed > TimedTrialSecondsAllowed ) {
					Console.PrintError( "[STEAM] Timed trial has expired" );
					GetTree().Quit();
				}
			}

			Console.PrintLine( string.Format( "...Language: {0}", GameLanguage ) );
			Console.PrintLine( string.Format( "...AppId: {0}", SteamAppID ) );
			Console.PrintLine( string.Format( "...AppBuildId: {0}", SteamAppBuildID ) );
		}
		private static void LoadUserInfo() {
			SteamID = SteamUser.GetSteamID();
			SteamUsername = SteamFriends.GetPersonaName();

			Console.PrintLine( string.Format( "...SteamUser.Id: {0}", SteamID ) );
			Console.PrintLine( string.Format( "...SteamUser.UserName: {0}", SteamUsername ) );
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
				Console.PrintError( "Steam isn't running, not initializing SteamAPI" );
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

			ProcessMode = ProcessModeEnum.Always;
		}
		public override void _Process( double delta ) {
			if ( ( Engine.GetProcessFrames() % 180 ) != 0 ) {
				return;
			}
			SteamAPI.RunCallbacks();
		}

		private void SteamAPIDebugTextCallback( int nSeverity, System.Text.StringBuilder debugText ) {
			Console.PrintLine( string.Format( "[STEAM] {0}", debugText.ToString() ) );
		}

		public static void SaveCloudFile( string path ) {
			if ( !SteamRemoteStorage.IsCloudEnabledForAccount() || !SteamRemoteStorage.IsCloudEnabledForApp() ) {
				Console.PrintLine( "SteamCloud isn't enabled for this application or the account" );
				return;
			}

			string realpath = ProjectSettings.GlobalizePath( "user://" + path );
			using var stream = new System.IO.FileStream( realpath, System.IO.FileMode.Open );

			stream.Seek( 0, System.IO.SeekOrigin.End );
			long length = stream.Position;
			stream.Seek( 0, System.IO.SeekOrigin.Begin );

			byte[] buffer = new byte[ length ];
			stream.ReadExactly( buffer );

			Console.PrintLine( string.Format( "Saving file \"{0}\" to SteamCloud...", path ) );
			SteamRemoteStorage.FileWrite( path, buffer, buffer.Length );
		}
	};
};