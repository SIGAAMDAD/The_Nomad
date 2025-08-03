/*
===========================================================================
Copyright (C) 2023-2025 Noah Van Til

This file is part of The Nomad source code.

The Nomad source code is free software; you can redistribute it
and/or modify it under the terms of the GNU General Public License as
published by the Free Software Foundation; either version 2 of the License,
or (at your option) any later version.

The Nomad source code is distributed in the hope that it will be
useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Foobar; if not, write to the Free Software
Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
===========================================================================
*/

using Godot;
using System;

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

	private Button ContinueGameButton;
	private Button LoadGameButton;
	private Button NewGameButton;

	public static bool Loaded = false;
	private Button[] ButtonList = null;
	private int ButtonIndex = 0;

	private System.Threading.Thread LoadThread;
	private PackedScene LoadedWorld;

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
	[Signal]
	public delegate void FinishedLoadingEventHandler();

	private void OnContinueGameFinished() {
		GetNode<CanvasLayer>( "/root/TransitionScreen" ).Disconnect( "transition_finished", Callable.From( OnContinueGameFinished ) );

		Hide();
		GetNode<LoadingScreen>( "/root/LoadingScreen" ).Call( "FadeIn" );

		Console.PrintLine( "Loading game..." );

		GameConfiguration.GameMode = GameMode.SinglePlayer;

		World.LoadTime = System.Diagnostics.Stopwatch.StartNew();

		FinishedLoading += () => {
			LoadThread.Join();
			GetTree().ChangeSceneToPacked( LoadedWorld );
		};
		LoadThread = new System.Threading.Thread( () => {
			ArchiveSystem.LoadGame( SettingsData.GetSaveSlot() );
			LoadedWorld = ResourceLoader.Load<PackedScene>( "res://levels/world.tscn" );
			CallDeferred( MethodName.EmitSignal, SignalName.FinishedLoading );
		} );
		LoadThread.Start();
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

	private void OnBeginGameFinished() {
		GetNode<CanvasLayer>( "/root/TransitionScreen" ).Disconnect( "transition_finished", Callable.From( OnBeginGameFinished ) );
		QueueFree();
		GetTree().ChangeSceneToFile( "res://scenes/menus/poem.tscn" );
	}
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

	private void OnButtonPressed( System.Action signalCallback ) {
		if ( Loaded ) {
			return;
		}
		UIAudioManager.OnButtonPressed();
		signalCallback();
	}
	private void OnButtonFocused( int nButtonIndex ) {
		UIAudioManager.OnButtonFocused();
		ButtonIndex = nButtonIndex;

		if ( ButtonList != null ) {
			ButtonList[ ButtonIndex ].GrabFocus();
		}
	}

	public override void _Ready() {
		PhysicsServer2D.SetActive( false );

		if ( SettingsData.GetDyslexiaMode() ) {
			Theme = AccessibilityManager.DyslexiaTheme;
		} else {
			Theme = AccessibilityManager.DefaultTheme;
		}

		bool bHasSaveData = DirAccess.DirExistsAbsolute( ProjectSettings.GlobalizePath( "user://SaveData" ) );

		UIAudioManager.PlayTheme();
		Loaded = false;

		MultiplayerMapManager.Init();
		ArchiveSystem.Instance.CheckSaveData();

		ProcessMode = ProcessModeEnum.Always;

		ContinueGameButton = GetNode<Button>( "VBoxContainer/ContinueGameButton" );
		if ( bHasSaveData ) {
			ContinueGameButton.Connect( Button.SignalName.FocusEntered, Callable.From( () => OnButtonFocused( (int)IndexedButton.ContinueGame ) ) );
			ContinueGameButton.Connect( Button.SignalName.MouseEntered, Callable.From( () => OnButtonFocused( (int)IndexedButton.ContinueGame ) ) );
			ContinueGameButton.Connect( Button.SignalName.Pressed, Callable.From( OnContinueGameButtonPressed ) );

			ContinueGameButton.GrabFocus();
		} else {
			ContinueGameButton.Modulate = new Color( 0.50f, 0.50f, 0.50f, 0.75f );
		}
		ContinueGameButton.SetMeta( "index", 0 );

		LoadGameButton = GetNode<Button>( "VBoxContainer/LoadGameButton" );
		if ( bHasSaveData ) {
			LoadGameButton.Connect( Button.SignalName.FocusEntered, Callable.From( () => OnButtonFocused( (int)IndexedButton.LoadGame ) ) );
			LoadGameButton.Connect( Button.SignalName.MouseEntered, Callable.From( () => OnButtonFocused( (int)IndexedButton.LoadGame ) ) );
			LoadGameButton.Connect( Button.SignalName.Pressed, Callable.From( EmitSignalSaveSlotsMenu ) );
		} else {
			LoadGameButton.Modulate = new Color( 0.60f, 0.60f, 0.60f, 1.0f );
		}
		LoadGameButton.SetMeta( "index", 1 );

		Button NewGameButton = GetNode<Button>( "VBoxContainer/NewGameButton" );
		NewGameButton.Connect( Button.SignalName.FocusEntered, Callable.From( () => OnButtonFocused( (int)IndexedButton.NewGame ) ) );
		NewGameButton.Connect( Button.SignalName.MouseEntered, Callable.From( () => OnButtonFocused( (int)IndexedButton.NewGame ) ) );
		NewGameButton.Connect( Button.SignalName.Pressed, Callable.From( OnNewGameButtonPressed ) );
		NewGameButton.SetMeta( "index", 2 );
		if ( !bHasSaveData ) {
			NewGameButton.GrabFocus();
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

		Button ExtrasButton = GetNode<Button>( "VBoxContainer/ExtrasButton" );
		ExtrasButton.Connect( Button.SignalName.MouseEntered, Callable.From( () => OnButtonFocused( (int)IndexedButton.Extras ) ) );
		ExtrasButton.Connect( Button.SignalName.FocusEntered, Callable.From( () => OnButtonFocused( (int)IndexedButton.Extras ) ) );
		ExtrasButton.Connect( Button.SignalName.Pressed, Callable.From( () => OnButtonPressed( EmitSignalExtrasMenu ) ) );

		Button SettingsButton = GetNode<Button>( "VBoxContainer/SettingsButton" );
		SettingsButton.Connect( Button.SignalName.MouseEntered, Callable.From( () => OnButtonFocused( (int)IndexedButton.Settings ) ) );
		SettingsButton.Connect( Button.SignalName.FocusEntered, Callable.From( () => OnButtonFocused( (int)IndexedButton.Settings ) ) );
		SettingsButton.Connect( Button.SignalName.Pressed, Callable.From( () => OnButtonPressed( EmitSignalSettingsMenu ) ) );

		Button ModsButton = GetNode<Button>( "VBoxContainer/TalesAroundTheCampfireButton" );
		ModsButton.Connect( Button.SignalName.MouseEntered, Callable.From( () => OnButtonFocused( (int)IndexedButton.Mods ) ) );
		ModsButton.Connect( Button.SignalName.FocusEntered, Callable.From( () => OnButtonFocused( (int)IndexedButton.Mods ) ) );
		ModsButton.Connect( Button.SignalName.Pressed, Callable.From( () => OnButtonPressed( EmitSignalModsMenu ) ) );

		Button CreditsButton = GetNode<Button>( "VBoxContainer/CreditsButton" );
		CreditsButton.Connect( Button.SignalName.MouseEntered, Callable.From( () => OnButtonFocused( (int)IndexedButton.Credits ) ) );
		CreditsButton.Connect( Button.SignalName.FocusEntered, Callable.From( () => OnButtonFocused( (int)IndexedButton.Credits ) ) );
		CreditsButton.Connect( Button.SignalName.Pressed, Callable.From( () => OnButtonPressed( EmitSignalCreditsMenu ) ) );

		Button ExitButton = GetNode<Button>( "VBoxContainer/QuitGameButton" );
		ExitButton.Connect( Button.SignalName.MouseEntered, Callable.From( () => OnButtonFocused( (int)IndexedButton.Quit ) ) );
		ExitButton.Connect( Button.SignalName.FocusEntered, Callable.From( () => OnButtonFocused( (int)IndexedButton.Quit ) ) );
		ExitButton.Connect( Button.SignalName.Pressed, Callable.From( () => OnButtonPressed( () => GetTree().Quit() ) ) );

		Label AppVersion = GetNode<Label>( "AppVersion" );
		AppVersion.Text = ProjectSettings.GetSetting( "application/config/version" ).AsString();
		AppVersion.ProcessMode = ProcessModeEnum.Disabled;

		ButtonList = [
			ContinueGameButton,
			NewGameButton,
			LoadGameButton,
			ExtrasButton,
			SettingsButton,
			ModsButton,
			CreditsButton,
			ExitButton
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
