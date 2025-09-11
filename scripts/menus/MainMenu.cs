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
using System;
using Utils;

namespace Menus {
	/*
	===================================================================================

	MainMenu

	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public partial class MainMenu : Control {
		private enum IndexedButton : int {
			ContinueGame,
			NewGame,
			LoadGame,
			Extras,
			Settings,
			Mods,
			Credits,
			Quit,

			Count
		};

		/// <summary>
		/// The color of a button that cannot be interacted with
		/// </summary>
		private static readonly Color DISABLED_COLOR = new Color( 0.50f, 0.50f, 0.50f, 0.75f );

		/// <summary>
		/// Set to true when one of the following is done: "New Game" is pressed,
		/// "Continue Game" is pressed, or a save slot in the SaveSlotsMenu is pressed
		/// </summary>
		public static bool Loaded { get; set; } = false;

		private ConfirmationDialog TutorialsPopup;

		[Signal]
		public delegate void SaveSlotsMenuEventHandler();
		[Signal]
		public delegate void SettingsMenuEventHandler();
		[Signal]
		public delegate void HelpMenuEventHandler();
		[Signal]
		public delegate void ExtrasMenuEventHandler();
		[Signal]
		public delegate void ModsMenuEventHandler();
		[Signal]
		public delegate void CreditsMenuEventHandler();

		[Signal]
		public delegate void BeginGameEventHandler();

		/*
		===============
		OnContinueGameFinished
		===============
		*/
		private void OnContinueGameFinished() {
			TransitionScreen.TransitionFinished -= OnContinueGameFinished;

			Hide();
			GetNode<LoadingScreen>( "/root/LoadingScreen" ).FadeIn( "res://levels/world.tscn", () => ArchiveSystem.LoadGame( SettingsData.LastSaveSlot ) );

			Console.PrintLine( "Loading game..." );

			GameConfiguration.SetGameMode( GameMode.SinglePlayer );

			World.LoadTime = System.Diagnostics.Stopwatch.StartNew();
		}

		/*
		===============
		OnContinueGameButtonPressed
		===============
		*/
		private void OnContinueGameButtonPressed() {
			if ( MainMenu.Loaded ) {
				return;
			}
			MainMenu.Loaded = true;

			EmitSignalBeginGame();

			UIAudioManager.OnActivate();

			Hide();
			TransitionScreen.TransitionFinished += OnContinueGameFinished;
			TransitionScreen.Transition();

			UIAudioManager.FadeMusic();
		}

		/*
		===============
		OnBeginGameFinished
		===============
		*/
		private void OnBeginGameFinished() {
			TransitionScreen.TransitionFinished -= OnBeginGameFinished;
			QueueFree();
			GetTree().ChangeSceneToFile( "res://scenes/menus/poem.tscn" );
		}

		/*
		===============
		OnNewGameButtonPressed
		===============
		*/
		/// <summary>
		/// Creates a new save slot and asks the user if they want tutorials for a new game save
		/// </summary>
		private void OnNewGameButtonPressed() {
			if ( MainMenu.Loaded ) {
				return;
			}
			MainMenu.Loaded = true;

			int slotIndex = 0;
			while ( ArchiveSystem.SlotExists( slotIndex ) ) {
				slotIndex++;
			}
			SettingsData.SetSaveSlot( slotIndex );

			TutorialsPopup.Show();
		}

		/*
		===============
		OnButtonPressed
		===============
		*/
		private static void OnButtonPressed( Action signalCallback ) {
			if ( Loaded ) {
				return;
			}
			UIAudioManager.OnButtonPressed();
			signalCallback();
		}

		/*
		===============
		OnTutorialOptionSelected
		===============
		*/
		private void OnTutorialOptionSelected( bool tutorials ) {
			SettingsData.SetTutorialsEnabled( tutorials );

			EmitSignalBeginGame();

			UIAudioManager.OnActivate();

			TransitionScreen.TransitionFinished += OnBeginGameFinished;
			TransitionScreen.Transition();
		}

		/*
		===============
		InitNodes
		===============
		*/
		/// <summary>
		/// Binds node references and signals for the Main Menu
		/// </summary>
		private void InitNodes() {
			bool hasSaveData = DirAccess.DirExistsAbsolute( ProjectSettings.GlobalizePath( "user://SaveData" ) );

			SelectionNodes.Button continueGameButton = GetNode<SelectionNodes.Button>( "VBoxContainer/ContinueGameButton" );
			if ( hasSaveData ) {
				continueGameButton.Pressed += OnContinueGameButtonPressed;
				continueGameButton.GrabFocus();
			} else {
				continueGameButton.Modulate = DISABLED_COLOR;
			}

			SelectionNodes.Button loadGameButton = GetNode<SelectionNodes.Button>( "VBoxContainer/LoadGameButton" );
			if ( hasSaveData ) {
				loadGameButton.Pressed += EmitSignalSaveSlotsMenu;
			} else {
				loadGameButton.Modulate = DISABLED_COLOR;
			}

			SelectionNodes.Button newGameButton = GetNode<SelectionNodes.Button>( "VBoxContainer/NewGameButton" );
			newGameButton.Pressed += OnNewGameButtonPressed;

			TutorialsPopup = GetNode<ConfirmationDialog>( "TutorialsPopup" );
			GameEventBus.ConnectSignal( TutorialsPopup, ConfirmationDialog.SignalName.Canceled, this, () => OnTutorialOptionSelected( false ) );
			GameEventBus.ConnectSignal( TutorialsPopup, ConfirmationDialog.SignalName.CloseRequested, this, TutorialsPopup.Hide );
			GameEventBus.ConnectSignal( TutorialsPopup, ConfirmationDialog.SignalName.Confirmed, this, () => OnTutorialOptionSelected( true ) );

			GetNode<SelectionNodes.Button>( "VBoxContainer/ExtrasButton" ).Pressed += () => OnButtonPressed( EmitSignalExtrasMenu );
			GetNode<SelectionNodes.Button>( "VBoxContainer/SettingsButton" ).Pressed += () => OnButtonPressed( EmitSignalSettingsMenu );
			GetNode<SelectionNodes.Button>( "VBoxContainer/TalesAroundTheCampfireButton" ).Pressed += () => OnButtonPressed( EmitSignalModsMenu );

			SelectionNodes.Button exitButton = GetNode<SelectionNodes.Button>( "VBoxContainer/QuitGameButton" );
			exitButton.Pressed += () => GetTree().Quit();

			Label appVersion = GetNode<Label>( "AppVersion" );
			appVersion.Text = ProjectSettings.GetSetting( "application/config/version" ).AsString();
			appVersion.ProcessMode = ProcessModeEnum.Disabled;

			if ( !hasSaveData ) {
				newGameButton.FocusNeighborTop = exitButton.GetPath();
				exitButton.FocusNeighborBottom = newGameButton.GetPath();

//				newGameButton.GrabFocus();
			}
		}

		/*
		===============
		_Ready
		===============
		*/
		public override void _Ready() {
			base._Ready();

			// we don't need physics in a menu
			PhysicsServer2D.SetActive( false );

			if ( SettingsData.DyslexiaMode ) {
				Theme = AccessibilityManager.DyslexiaTheme;
			} else {
				Theme = AccessibilityManager.DefaultTheme;
			}

			Loaded = false;

			MultiplayerMapManager.Init();
			ArchiveSystem.CheckSaveData();

			ProcessMode = ProcessModeEnum.Always;

			InitNodes();
		}
	};
};