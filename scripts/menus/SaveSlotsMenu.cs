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
using System.Collections.Generic;

namespace Menus {
	public partial class SaveSlotsMenu : Control {
		private Button DeleteSaveButton;
		private VBoxContainer SlotsContainer;

		private Button[] ButtonList;
		private int ButtonIndex = 0;

		[Signal]
		public delegate void BeginGameEventHandler();

		private void OnContinueGameFinished() {
			GetNode<CanvasLayer>( "/root/TransitionScreen" ).Disconnect( "transition_finished", Callable.From( OnContinueGameFinished ) );

			Hide();
			GetNode<LoadingScreen>( "/root/LoadingScreen" ).Call( "FadeIn" );

			Console.PrintLine( "Loading game..." );

			/*
			if ( SettingsData.GetNetworkingEnabled() ) {
				Console.PrintLine( "Networking enabled, creating co-op lobby..." );

				GameConfiguration.GameMode = GameMode.Online;

				SteamLobby.Instance.SetMaxMembers( 4 );
				string name = SteamManager.GetSteamName();
				if ( name[ name.Length - 1 ] == 's' ) {
					SteamLobby.Instance.SetLobbyName( string.Format( "{0}' Lobby", name ) );
				} else {
					SteamLobby.Instance.SetLobbyName( string.Format( "{0}'s Lobby", name ) );
				}

				SteamLobby.Instance.CreateLobby();
			} else {
			}
			*/
			GameConfiguration.GameMode = GameMode.SinglePlayer;

			World.LoadTime = System.Diagnostics.Stopwatch.StartNew();
		}
		private void OnContinueGameButtonPressed() {
			if ( MainMenu.Loaded ) {
				return;
			}
			MainMenu.Loaded = true;

			EmitSignalBeginGame();

			UIAudioManager.OnActivate();

			Hide();
			GetNode<CanvasLayer>( "/root/TransitionScreen" ).Connect( "transition_finished", Callable.From( OnContinueGameFinished ) );
			GetNode<CanvasLayer>( "/root/TransitionScreen" ).Call( "transition" );

			UIAudioManager.FadeMusic();
		}

		private void OnSaveSlotButtonPressed( Button SaveSlot ) {
			if ( MainMenu.Loaded ) {
				return;
			}
			MainMenu.Loaded = true;

			EmitSignalBeginGame();

			UIAudioManager.OnActivate();

			SettingsData.SetSaveSlot( (int)SaveSlot.GetMeta( "index" ) );

			Hide();
			GetNode<CanvasLayer>( "/root/TransitionScreen" ).Connect( "transition_finished", Callable.From( OnContinueGameFinished ) );
			GetNode<CanvasLayer>( "/root/TransitionScreen" ).Call( "transition" );

			UIAudioManager.FadeMusic();
		}

		private List<string> GetSaveSlotList( string directory ) {
			List<string> saveSlotList = new List<string>();

			DirAccess dir = DirAccess.Open( directory );
			if ( dir != null ) {
				dir.ListDirBegin();
				string fileName = dir.GetNext();
				while ( fileName.Length > 0 ) {
					if ( fileName.GetExtension() != "ngd" ) {
						fileName = dir.GetNext();
						continue;
					}
					saveSlotList.Add( fileName );
					fileName = dir.GetNext();
				}
			} else {
				Console.PrintError( string.Format( "An error occurred when trying to access path \"{0}\"", directory ) );
			}

			return saveSlotList;
		}
		private void LoadSaveSlots() {
			List<string> saveSlots = GetSaveSlotList( "user://SaveData" );
			Console.PrintLine( string.Format( "Found {0} save slots.", saveSlots.Count ) );

			// clear out the children first
			for ( int i = 0; i < SlotsContainer.GetChildCount(); i++ ) {
				SlotsContainer.GetChild( i ).CallDeferred( MethodName.QueueFree );
				SlotsContainer.RemoveChild( SlotsContainer.GetChild( i ) );
			}

			ButtonIndex = 0;

			ButtonList = new Button[ saveSlots.Count ];
			for ( int i = 0; i < saveSlots.Count; i++ ) {
				Button label = new Button();

				label.Text = saveSlots[ i ];
				label.Connect( Button.SignalName.FocusEntered, Callable.From( () => OnButtonFocused( label ) ) );
				label.Connect( Button.SignalName.MouseEntered, Callable.From( () => OnButtonFocused( label ) ) );
				label.Connect( Button.SignalName.Pressed, Callable.From( () => OnSaveSlotButtonPressed( label ) ) );
				label.SetMeta( "index", i );
				label.Theme = DeleteSaveButton.Theme;
				label.Show();
				SlotsContainer.AddChild( label );

				ButtonList[ i ] = label;
			}
		}

		private void OnDeleteSaveButtonPressed() {
			ArchiveSystem.DeleteSave( ButtonIndex );
			ButtonIndex = 0;

			LoadSaveSlots();
		}

		private void OnButtonFocused( Button button ) {
			UIAudioManager.OnButtonFocused();

			if ( ButtonList[ ButtonIndex ] != button ) {
				ButtonIndex = (int)button.GetMeta( "index" );
			}
		}

		public override void _Ready() {
			base._Ready();

			Theme = SettingsData.DyslexiaMode ? AccessibilityManager.DyslexiaTheme : AccessibilityManager.DefaultTheme;

			ConfirmationDialog DeleteSaveConfirm = GetNode<ConfirmationDialog>( "DeleteSaveConfirmation" );
			DeleteSaveConfirm.Connect( ConfirmationDialog.SignalName.Canceled, Callable.From( Hide ) );
			DeleteSaveConfirm.Connect( ConfirmationDialog.SignalName.Confirmed, Callable.From( OnDeleteSaveButtonPressed ) );

			DeleteSaveButton = GetNode<Button>( "DeleteSaveButton" );
			DeleteSaveButton.Connect( Button.SignalName.FocusEntered, Callable.From( () => OnButtonFocused( DeleteSaveButton ) ) );
			DeleteSaveButton.Connect( Button.SignalName.Pressed, Callable.From( DeleteSaveConfirm.Show ) );

			SlotsContainer = GetNode<VBoxContainer>( "VScrollBar/SaveSlotsContainer" );

			LoadSaveSlots();
		}
		public override void _UnhandledInput( InputEvent @event ) {
			base._UnhandledInput( @event );

			if ( Input.IsActionJustPressed( "ui_down" ) ) {
				ButtonList[ ButtonIndex ].EmitSignal( Button.SignalName.FocusExited );
				if ( ButtonIndex == ButtonList.Length - 1 ) {
					ButtonIndex = 0;
				} else {
					ButtonIndex++;
				}
				ButtonList[ ButtonIndex ].EmitSignal( Button.SignalName.FocusEntered );
			} else if ( Input.IsActionJustPressed( "ui_up" ) ) {
				ButtonList[ ButtonIndex ].EmitSignal( Button.SignalName.FocusExited );
				if ( ButtonIndex == 0 ) {
					ButtonIndex = ButtonList.Length - 1;
				} else {
					ButtonIndex--;
				}
				ButtonList[ ButtonIndex ].EmitSignal( Button.SignalName.FocusEntered );
			} else if ( Input.IsActionJustPressed( "ui_accept" ) || Input.IsActionJustPressed( "ui_enter" ) ) {
				ButtonList[ ButtonIndex ].EmitSignal( Button.SignalName.FocusEntered );
				ButtonList[ ButtonIndex ].CallDeferred( Button.MethodName.EmitSignal, Button.SignalName.Pressed );
			}
		}
	};
};