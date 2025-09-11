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

using Godot;
using ResourceCache;
using Utils;

namespace Menus {
	/*
	===================================================================================

	PauseMenu

	===================================================================================
	*/

	public partial class PauseMenu : CanvasLayer {
		private enum TabSelect {
			Backpack,
			Journal,
			Equipment,
			Options
		};

		private ConfirmationDialog ConfirmExitDlg;
		private ConfirmationDialog ConfirmQuitDlg;

		private AudioStreamPlayer UIChannel;

		private TabContainer TabContainer;

		private SettingsMenu SettingsMenu;
		private Button ExitSettingsMenuButton;

		[Signal]
		public delegate void GamePausedEventHandler();
		[Signal]
		public delegate void LeaveLobbyEventHandler();

		/*
		================
		OnConfirmExitConfirmed
		===============
		*/
		private void OnConfirmExitConfirmed() {
			GameConfiguration.Paused = false;

			if ( GameConfiguration.GameMode == GameMode.SinglePlayer || GameConfiguration.GameMode == GameMode.Online ) {
				ArchiveSystem.SaveGame( SettingsData.LastSaveSlot );
			}

			GetTree().Paused = false;
			Engine.TimeScale = 1.0f;
			ArchiveSystem.Clear();

			Steam.SteamLobby.Instance.SetPhysicsProcess( false );

			if ( GameConfiguration.GameMode == GameMode.Multiplayer || GameConfiguration.GameMode == GameMode.Online ) {
				EmitSignalLeaveLobby();
			}
			GetTree().ChangeSceneToFile( "res://scenes/main_menu.tscn" );
		}

		/*
		================
		OnToggled
		===============
		*/
		private void OnToggled() {
			Visible = !Visible;
		}

		/*
		================
		OnSettingsMenuButtonPressed
		===============
		*/
		private void OnSettingsMenuButtonPressed() {
			AddChild( SettingsMenu );
			ExitSettingsMenuButton.Show();
		}

		/*
		================
		OnExitSettingsMenuButtonPressed
		===============
		*/
		private void OnExitSettingsMenuButtonPressed() {
			RemoveChild( SettingsMenu );
			ExitSettingsMenuButton.Hide();
		}

		/*
		================
		OnTabContainerTabClicked
		===============
		*/
		private void OnTabContainerTabClicked( int tabSelected ) {
			UIAudioManager.OnButtonPressed();
			bool paused = tabSelected == (int)TabSelect.Options;

			if ( GameConfiguration.GameMode != GameMode.Multiplayer ) {
				GetTree().Paused = paused;
				Engine.TimeScale = paused ? 0.0f : 1.0f;
			}

			if ( paused ) {
				Input.SetCustomMouseCursor( TextureCache.GetTexture( "res://cursor_n.png" ) );
				EmitSignalGamePaused();
			} else {
				Input.SetCustomMouseCursor( TextureCache.GetTexture( "res://textures/hud/crosshairs/crosshairi.tga" ) );
			}
		}

		/*
		================
		OnConfirmQuitDlgConfirmed
		===============
		*/
		private void OnConfirmQuitDlgConfirmed() {
			if ( GameConfiguration.GameMode == GameMode.SinglePlayer ) {
				ArchiveSystem.SaveGame( SettingsData.LastSaveSlot );
			}

			GetTree().Quit();
		}

		/*
		================
		OnJoyConnectionChanged
		===============
		*/
		private void OnJoyConnectionChanged( long device, bool connected ) {
			TabContainer.CallDeferred( MethodName.EmitSignal, TabContainer.SignalName.TabSelected, (int)TabSelect.Options );
		}

		/*
		================
		_Ready

		godot initialization override
		===============
		*/
		public override void _Ready() {
			base._Ready();

			SettingsMenu = SceneCache.GetScene( "res://scenes/menus/settings_menu.tscn" ).Instantiate<SettingsMenu>();

			ExitSettingsMenuButton = GetNode<Button>( "ExitButton" );
			GameEventBus.ConnectSignal( ExitSettingsMenuButton, Button.SignalName.Pressed, this, OnExitSettingsMenuButtonPressed );

			ConfirmExitDlg = GetNode<ConfirmationDialog>( "ConfirmExit" );
			GameEventBus.ConnectSignal( ConfirmExitDlg, ConfirmationDialog.SignalName.Confirmed, this, OnConfirmExitConfirmed );
			GameEventBus.ConnectSignal( ConfirmExitDlg, ConfirmationDialog.SignalName.Canceled, this, ConfirmExitDlg.Hide );

			TabContainer = GetNode<TabContainer>( "MarginContainer/TabContainer" );
			GameEventBus.ConnectSignal( TabContainer, TabContainer.SignalName.TabClicked, this, Callable.From<int>( OnTabContainerTabClicked ) );

			ConfirmQuitDlg = GetNode<ConfirmationDialog>( "ConfirmQuit" );
			GameEventBus.ConnectSignal( ConfirmQuitDlg, ConfirmationDialog.SignalName.Confirmed, this, OnConfirmQuitDlgConfirmed );
			GameEventBus.ConnectSignal( ConfirmQuitDlg, ConfirmationDialog.SignalName.Canceled, this, ConfirmQuitDlg.Hide );

			Methods.ConnectMenuButton(
				GetNode<Button>( "MarginContainer/TabContainer/Options/VBoxContainer/ResumeButton" ),
				this,
				OnToggled
			);

			Methods.ConnectMenuButton(
				GetNode<Button>( "MarginContainer/TabContainer/Options/VBoxContainer/SettingsMenuButton" ),
				this,
				OnSettingsMenuButtonPressed
			);

			Methods.ConnectMenuButton(
				GetNode<Button>( "MarginContainer/TabContainer/Options/VBoxContainer/ExitToMainMenuButton" ),
				this,
				ConfirmExitDlg.Show
			);

			ProcessMode = ProcessModeEnum.Always;

			switch ( GameConfiguration.GameMode ) {
				case GameMode.SinglePlayer:
				case GameMode.Online:
				case GameMode.JohnWick:
				case GameMode.ChallengeMode:
				case GameMode.LocalCoop2:
				case GameMode.LocalCoop3:
				case GameMode.LocalCoop4:
					ConfirmExitDlg.Set( ConfirmationDialog.PropertyName.DialogText, "Are you sure you want to quit?" );
					ConfirmQuitDlg.Set( ConfirmationDialog.PropertyName.DialogText, "Are you sure you want to quit?" );
					break;
				case GameMode.Multiplayer:
					ConfirmExitDlg.Set( ConfirmationDialog.PropertyName.DialogText, "Are you sure?" );
					ConfirmQuitDlg.Set( ConfirmationDialog.PropertyName.DialogText, "Are you sure?" );
					break;
				default:
					break;
			}

			// ensure that whenever there's a connection status change, we automatically pause
			// so that someone isn't getting ganked
			GameEventBus.Subscribe<Input.JoyConnectionChangedEventHandler>( this, OnJoyConnectionChanged );
		}

		/*
		================
		_UnhandledInput
		===============
		*/
		public override void _UnhandledInput( InputEvent @event ) {
			base._UnhandledInput( @event );

			if ( Input.IsActionJustReleased( "ui_exit" ) ) {
				CallDeferred( MethodName.OnToggled );
			}
		}
	};
};