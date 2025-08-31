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

namespace Menus;
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

	public static bool Loaded { get; set; } = false;

	private Button[] ButtonList = null;
	private int ButtonIndex = 0;

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
		GetNode<CanvasLayer>( "/root/TransitionScreen" ).Disconnect( "transition_finished", Callable.From( OnContinueGameFinished ) );

		Hide();
		GetNode<LoadingScreen>( "/root/LoadingScreen" ).FadeIn( "res://levels/world.tscn", () => ArchiveSystem.LoadGame( SettingsData.LastSaveSlot ) );

		Console.PrintLine( "Loading game..." );

		GameConfiguration.GameMode = GameMode.SinglePlayer;

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
		GetNode<CanvasLayer>( "/root/TransitionScreen" ).Connect( "transition_finished", Callable.From( OnContinueGameFinished ) );
		GetNode<CanvasLayer>( "/root/TransitionScreen" ).Call( "transition" );

		UIAudioManager.FadeMusic();
	}

	/*
	===============
	OnBeginGameFinished
	===============
	*/
	private void OnBeginGameFinished() {
		GetNode<CanvasLayer>( "/root/TransitionScreen" ).Disconnect( "transition_finished", Callable.From( OnBeginGameFinished ) );
		QueueFree();
		GetTree().ChangeSceneToFile( "res://scenes/menus/poem.tscn" );
	}

	/*
	===============
	OnNewGameButtonPressed
	===============
	*/
	private void OnNewGameButtonPressed() {
		if ( MainMenu.Loaded ) {
			return;
		}
		MainMenu.Loaded = true;

		int SlotIndex = 0;
		while ( ArchiveSystem.SlotExists( SlotIndex ) ) {
			SlotIndex++;
		}
		SettingsData.SetSaveSlot( SlotIndex );

		TutorialsPopup.Show();
	}

	/*
	===============
	OnButtonFocused
	===============
	*/
	private void OnButtonPressed( System.Action signalCallback ) {
		if ( Loaded ) {
			return;
		}
		UIAudioManager.OnButtonPressed();
		signalCallback();
	}

	/*
	===============
	OnButtonFocused
	===============
	*/
	private void OnButtonFocused( int buttonIndex ) {
		UIAudioManager.OnButtonFocused();
		ButtonIndex = buttonIndex;

		// throws null exception if its ButtonList[ ButtonIndex ]?.GrabFocus(), I did try... I did try
		// even VSCode is tweaking
		if ( ButtonList != null ) {
			ButtonList[ ButtonIndex ].GrabFocus();
		}
	}

	/*
	===============
	_Ready
	===============
	*/
	public override void _Ready() {
		PhysicsServer2D.SetActive( false );

		if ( SettingsData.DyslexiaMode ) {
			Theme = AccessibilityManager.DyslexiaTheme;
		} else {
			Theme = AccessibilityManager.DefaultTheme;
		}

		bool hasSaveData = DirAccess.DirExistsAbsolute( ProjectSettings.GlobalizePath( "user://SaveData" ) );

		UIAudioManager.PlayTheme();
		Loaded = false;

		MultiplayerMapManager.Init();
		ArchiveSystem.CheckSaveData();

		ProcessMode = ProcessModeEnum.Always;

		Button continueGameButton = GetNode<Button>( "VBoxContainer/ContinueGameButton" );
		if ( hasSaveData ) {
			continueGameButton.Connect( Button.SignalName.FocusEntered, Callable.From( () => OnButtonFocused( (int)IndexedButton.ContinueGame ) ) );
			continueGameButton.Connect( Button.SignalName.MouseEntered, Callable.From( () => OnButtonFocused( (int)IndexedButton.ContinueGame ) ) );
			continueGameButton.Connect( Button.SignalName.Pressed, Callable.From( OnContinueGameButtonPressed ) );

			continueGameButton.GrabFocus();
		} else {
			continueGameButton.Modulate = new Color( 0.50f, 0.50f, 0.50f, 0.75f );
		}
		continueGameButton.SetMeta( "index", 0 );

		Button loadGameButton = GetNode<Button>( "VBoxContainer/LoadGameButton" );
		if ( hasSaveData ) {
			loadGameButton.Connect( Button.SignalName.FocusEntered, Callable.From( () => OnButtonFocused( (int)IndexedButton.LoadGame ) ) );
			loadGameButton.Connect( Button.SignalName.MouseEntered, Callable.From( () => OnButtonFocused( (int)IndexedButton.LoadGame ) ) );
			loadGameButton.Connect( Button.SignalName.Pressed, Callable.From( EmitSignalSaveSlotsMenu ) );
		} else {
			loadGameButton.Modulate = new Color( 0.60f, 0.60f, 0.60f, 1.0f );
		}
		loadGameButton.SetMeta( "index", 1 );

		Button newGameButton = GetNode<Button>( "VBoxContainer/NewGameButton" );
		newGameButton.Connect( Button.SignalName.FocusEntered, Callable.From( () => OnButtonFocused( (int)IndexedButton.NewGame ) ) );
		newGameButton.Connect( Button.SignalName.MouseEntered, Callable.From( () => OnButtonFocused( (int)IndexedButton.NewGame ) ) );
		newGameButton.Connect( Button.SignalName.Pressed, Callable.From( OnNewGameButtonPressed ) );
		newGameButton.SetMeta( "index", 2 );
		if ( !hasSaveData ) {
			newGameButton.GrabFocus();
		}

		TutorialsPopup = GetNode<ConfirmationDialog>( "TutorialsPopup" );
		TutorialsPopup.Canceled += () => {
			SettingsData.SetTutorialsEnabled( false );

			EmitSignalBeginGame();

			UIAudioManager.OnActivate();

			GetNode<CanvasLayer>( "/root/TransitionScreen" ).Connect( "transition_finished", Callable.From( OnBeginGameFinished ) );
			GetNode<CanvasLayer>( "/root/TransitionScreen" ).Call( "transition" );
		};
		TutorialsPopup.CloseRequested += () => {
			TutorialsPopup.Hide();
		};
		TutorialsPopup.Confirmed += () => {
			SettingsData.SetTutorialsEnabled( true );

			EmitSignalBeginGame();

			UIAudioManager.OnActivate();

			GetNode<CanvasLayer>( "/root/TransitionScreen" ).Connect( "transition_finished", Callable.From( OnBeginGameFinished ) );
			GetNode<CanvasLayer>( "/root/TransitionScreen" ).Call( "transition" );
		};

		Button extrasButton = GetNode<Button>( "VBoxContainer/ExtrasButton" );
		extrasButton.Connect( Button.SignalName.MouseEntered, Callable.From( () => OnButtonFocused( (int)IndexedButton.Extras ) ) );
		extrasButton.Connect( Button.SignalName.FocusEntered, Callable.From( () => OnButtonFocused( (int)IndexedButton.Extras ) ) );
		extrasButton.Connect( Button.SignalName.Pressed, Callable.From( () => OnButtonPressed( EmitSignalExtrasMenu ) ) );

		Button settingsButton = GetNode<Button>( "VBoxContainer/SettingsButton" );
		settingsButton.Connect( Button.SignalName.MouseEntered, Callable.From( () => OnButtonFocused( (int)IndexedButton.Settings ) ) );
		settingsButton.Connect( Button.SignalName.FocusEntered, Callable.From( () => OnButtonFocused( (int)IndexedButton.Settings ) ) );
		settingsButton.Connect( Button.SignalName.Pressed, Callable.From( () => OnButtonPressed( EmitSignalSettingsMenu ) ) );

		Button modsButton = GetNode<Button>( "VBoxContainer/TalesAroundTheCampfireButton" );
		modsButton.Connect( Button.SignalName.MouseEntered, Callable.From( () => OnButtonFocused( (int)IndexedButton.Mods ) ) );
		modsButton.Connect( Button.SignalName.FocusEntered, Callable.From( () => OnButtonFocused( (int)IndexedButton.Mods ) ) );
		modsButton.Connect( Button.SignalName.Pressed, Callable.From( () => OnButtonPressed( EmitSignalModsMenu ) ) );

		Button creditsButton = GetNode<Button>( "VBoxContainer/CreditsButton" );
		creditsButton.Connect( Button.SignalName.MouseEntered, Callable.From( () => OnButtonFocused( (int)IndexedButton.Credits ) ) );
		creditsButton.Connect( Button.SignalName.FocusEntered, Callable.From( () => OnButtonFocused( (int)IndexedButton.Credits ) ) );
		creditsButton.Connect( Button.SignalName.Pressed, Callable.From( () => OnButtonPressed( EmitSignalCreditsMenu ) ) );

		Button exitButton = GetNode<Button>( "VBoxContainer/QuitGameButton" );
		exitButton.Connect( Button.SignalName.MouseEntered, Callable.From( () => OnButtonFocused( (int)IndexedButton.Quit ) ) );
		exitButton.Connect( Button.SignalName.FocusEntered, Callable.From( () => OnButtonFocused( (int)IndexedButton.Quit ) ) );
		exitButton.Connect( Button.SignalName.Pressed, Callable.From( () => OnButtonPressed( () => GetTree().Quit() ) ) );

		Label appVersion = GetNode<Label>( "AppVersion" );
		appVersion.Text = ProjectSettings.GetSetting( "application/config/version" ).AsString();
		appVersion.ProcessMode = ProcessModeEnum.Disabled;

		ButtonList = [
			continueGameButton,
				newGameButton,
				loadGameButton,
				extrasButton,
				settingsButton,
				modsButton,
				creditsButton,
				exitButton
		];
	}
	/*
	public override void _UnhandledInput( InputEvent @event ) {
		base._UnhandledInput( @event );

		if ( Input.IsActionJustPressed( "ui_down" ) ) {
			ButtonList[ ButtonIndex ].EmitSignal( Button.SignalName.FocusExited );
			ButtonList[ ButtonIndex ].EmitSignal( Button.SignalName.MouseExited );
			if ( ButtonIndex == ButtonList.Length - 1 ) {
				ButtonIndex = 0;
			} else {
				ButtonIndex++;
			}
			ButtonList[ ButtonIndex ].EmitSignal( Button.SignalName.FocusEntered );
			ButtonList[ ButtonIndex ].EmitSignal( Button.SignalName.MouseEntered );
		} else if ( Input.IsActionJustPressed( "ui_up" ) ) {
			ButtonList[ ButtonIndex ].EmitSignal( Button.SignalName.FocusExited );
			ButtonList[ ButtonIndex ].EmitSignal( Button.SignalName.MouseExited );
			if ( ButtonIndex == 0 ) {
				ButtonIndex = ButtonList.Length - 1;
			} else {
				ButtonIndex--;
			}
			ButtonList[ ButtonIndex ].EmitSignal( Button.SignalName.FocusEntered );
			ButtonList[ ButtonIndex ].EmitSignal( Button.SignalName.MouseEntered );
		} else if ( Input.IsActionJustPressed( "ui_accept" ) || Input.IsActionJustPressed( "ui_enter" ) ) {
			ButtonList[ ButtonIndex ].EmitSignal( Button.SignalName.FocusEntered );
			ButtonList[ ButtonIndex ].EmitSignal( Button.SignalName.MouseEntered );
			ButtonList[ ButtonIndex ].CallDeferred( MethodName.EmitSignal, Button.SignalName.Pressed );
		}
	}
	*/
};