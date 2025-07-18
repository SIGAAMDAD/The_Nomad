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

public partial class MainMenu : Control {
	private enum IndexedButton : int {
		Story,
		Extras,
		Settings,
		Mods,
		Credits,
		Quit,

		Count
	};

	public static Color Selected = new Color( 1.0f, 0.0f, 0.0f, 1.0f );
	public static Color Unselected = new Color( 1.0f, 1.0f, 1.0f, 1.0f );
	public static bool Loaded = false;
	private Button[] ButtonList = null;
	private int ButtonIndex = 0;

	[Signal]
	public delegate void StoryMenuEventHandler();
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

	/*
	private void OnBeginGameFinished() {
		GetNode<CanvasLayer>( "/root/TransitionScreen" ).Disconnect( "transition_finished", Callable.From( OnBeginGameFinished ) );
		QueueFree();
		GetTree().ChangeSceneToFile( "res://scenes/menus/poem.tscn" );
	}
	private void OnContinueGameFinished() {
		GetNode<CanvasLayer>( "/root/TransitionScreen" ).Disconnect( "transition_finished", Callable.From(  OnContinueGameFinished ) );

		Hide();
		GetNode<CanvasLayer>( "/root/LoadingScreen" ).Call( "FadeIn" );

		Console.PrintLine( "Loading game..." );

		if ( SettingsData.GetNetworkingEnabled() ) {
			Console.PrintLine( "Networking enabled, creating co-op lobby..." );

			GameConfiguration.GameMode = GameMode.Online;

			SteamLobby.Instance.SetMaxMembers( 4 );
			string name = SteamManager.GetSteamName();
			if ( name[ name.Length - 1  ] == 's' ) {
				SteamLobby.Instance.SetLobbyName( string.Format( "{0}' Lobby", name ) );
			} else {
				SteamLobby.Instance.SetLobbyName( string.Format( "{0}'s Lobby", name ) );
			}

			SteamLobby.Instance.CreateLobby();
		} else {
			GameConfiguration.GameMode = GameMode.SinglePlayer;
		}

		FinishedLoading += () => {
			LoadThread.Join();
			GetTree().ChangeSceneToPacked( LoadedWorld );
		};
		LoadThread = new Thread( () => {
			ArchiveSystem.LoadGame( SettingsData.GetSaveSlot() );
			LoadedWorld = ResourceLoader.Load<PackedScene>( "res://levels/world.tscn" );
			CallDeferred( "emit_signal", "FinishedLoading" );
		} );
		LoadThread.Start();
	}
	private void OnContinueGameButtonPressed() {
		if ( Loaded ) {
			return;
		}
		Loaded = true;

		EmitSignalBeginGame();
		UIChannel.Stream = UISfxManager.BeginGame;
		UIChannel.Play();

		Hide();
		GetNode<CanvasLayer>( "/root/TransitionScreen" ).Connect( "transition_finished", Callable.From(  OnContinueGameFinished ) );
		GetNode<CanvasLayer>( "/root/TransitionScreen" ).Call( "transition" );

		AudioFade = GetTree().Root.CreateTween();
		AudioFade.TweenProperty( GetTree().CurrentScene.GetNode( "Theme" ), "volume_db", -20.0f, 2.5f );
		AudioFade.Connect( "finished", Callable.From( OnAudioFadeFinished ) );
	}
	private void OnNewGameButtonPressed() {
		if ( Loaded ) {
			return;
		}
		Loaded = true;

		int SlotIndex = 0;
		while ( ArchiveSystem.SlotExists( SlotIndex ) ) {
			SlotIndex++;
		}
		SettingsData.SetSaveSlot( SlotIndex );

		EmitSignalBeginGame();

		AudioFade = GetTree().Root.CreateTween();
		AudioFade.TweenProperty( GetTree().CurrentScene.GetNode( "Theme" ), "volume_db", -20.0f, 1.5f );
		AudioFade.Connect( "finished", Callable.From( OnAudioFadeFinished ) );

		UIChannel.Stream = UISfxManager.BeginGame;
		UIChannel.Play();
		GetNode<CanvasLayer>( "/root/TransitionScreen" ).Connect( "transition_finished", Callable.From(  OnBeginGameFinished ) );
		GetNode<CanvasLayer>( "/root/TransitionScreen" ).Call( "transition" );
	}
	*/

	private void OnButtonPressed( System.Action signalCallback ) {
		if ( Loaded ) {
			return;
		}
		UIAudioManager.OnButtonPressed();
		signalCallback();
	}
	private void OnButtonFocused( int nButtonIndex ) {
		UIAudioManager.OnButtonFocused();

		if ( ButtonIndex != nButtonIndex ) {
			OnButtonUnfocused( ButtonIndex );
		}

		ButtonIndex = nButtonIndex;
		ButtonList[ ButtonIndex ].Modulate = Selected;
	}
	private void OnButtonUnfocused( int nButtonIndex ) {
		ButtonList[ ButtonIndex ].Modulate = Unselected;
	}

	public override void _Ready() {
		PhysicsServer2D.SetActive( false );

		if ( SettingsData.GetDyslexiaMode() ) {
			Theme = AccessibilityManager.DyslexiaTheme;
		} else {
			Theme = AccessibilityManager.DefaultTheme;
		}

		UIAudioManager.PlayTheme();
		Loaded = false;

		MultiplayerMapManager.Init();
		ArchiveSystem.Instance.CheckSaveData();

		ProcessMode = ProcessModeEnum.Always;

		Button StoryModeButton = GetNode<Button>( "VBoxContainer/StoryModeButton" );
		StoryModeButton.Connect( "mouse_entered", Callable.From( () => OnButtonFocused( 0 ) ) );
		StoryModeButton.Connect( "mouse_exited", Callable.From( () => OnButtonUnfocused( 0 ) ) );
		StoryModeButton.Connect( "focus_entered", Callable.From( () => OnButtonFocused( 0 ) ) );
		StoryModeButton.Connect( "focus_exited", Callable.From( () => OnButtonUnfocused( 0 ) ) );
		StoryModeButton.Connect( "pressed", Callable.From( () => OnButtonPressed( EmitSignalStoryMenu ) ) );

		Button ExtrasButton = GetNode<Button>( "VBoxContainer/ExtrasButton" );
		ExtrasButton.Connect( "mouse_entered", Callable.From( () => OnButtonFocused( (int)IndexedButton.Extras ) ) );
		ExtrasButton.Connect( "mouse_exited", Callable.From( () => OnButtonUnfocused( (int)IndexedButton.Extras ) ) );
		ExtrasButton.Connect( "focus_entered", Callable.From( () => OnButtonFocused( (int)IndexedButton.Extras ) ) );
		ExtrasButton.Connect( "focus_exited", Callable.From( () => OnButtonUnfocused( (int)IndexedButton.Extras ) ) );
		ExtrasButton.Connect( "pressed", Callable.From( () => OnButtonPressed( EmitSignalExtrasMenu ) ) );

		Button SettingsButton = GetNode<Button>( "VBoxContainer/SettingsButton" );
		SettingsButton.Connect( "mouse_entered", Callable.From( () => OnButtonFocused( (int)IndexedButton.Settings ) ) );
		SettingsButton.Connect( "mouse_exited", Callable.From( () => OnButtonUnfocused( (int)IndexedButton.Settings ) ) );
		SettingsButton.Connect( "focus_entered", Callable.From( () => OnButtonFocused( (int)IndexedButton.Settings ) ) );
		SettingsButton.Connect( "focus_exited", Callable.From( () => OnButtonUnfocused( (int)IndexedButton.Settings ) ) );
		SettingsButton.Connect( "pressed", Callable.From( () => OnButtonPressed( EmitSignalSettingsMenu ) ) );

		Button ModsButton = GetNode<Button>( "VBoxContainer/TalesAroundTheCampfireButton" );
		ModsButton.Connect( "mouse_entered", Callable.From( () => OnButtonFocused( (int)IndexedButton.Mods ) ) );
		ModsButton.Connect( "mouse_exited", Callable.From( () => OnButtonUnfocused( (int)IndexedButton.Mods ) ) );
		ModsButton.Connect( "focus_entered", Callable.From( () => OnButtonFocused( (int)IndexedButton.Mods ) ) );
		ModsButton.Connect( "focus_exited", Callable.From( () => OnButtonUnfocused( (int)IndexedButton.Mods ) ) );
		ModsButton.Connect( "pressed", Callable.From( () => OnButtonPressed( EmitSignalModsMenu ) ) );

		Button CreditsButton = GetNode<Button>( "VBoxContainer/CreditsButton" );
		CreditsButton.Connect( "mouse_entered", Callable.From( () => OnButtonFocused( (int)IndexedButton.Credits ) ) );
		CreditsButton.Connect( "mouse_exited", Callable.From( () => OnButtonUnfocused( (int)IndexedButton.Credits ) ) );
		CreditsButton.Connect( "focus_entered", Callable.From( () => OnButtonFocused( (int)IndexedButton.Credits ) ) );
		CreditsButton.Connect( "focus_exited", Callable.From( () => OnButtonUnfocused( (int)IndexedButton.Credits ) ) );
		CreditsButton.Connect( "pressed", Callable.From( () => OnButtonPressed( EmitSignalCreditsMenu ) ) );

		Button ExitButton = GetNode<Button>( "VBoxContainer/QuitGameButton" );
		ExitButton.Connect( "mouse_entered", Callable.From( () => OnButtonFocused( (int)IndexedButton.Quit ) ) );
		ExitButton.Connect( "mouse_exited", Callable.From( () => OnButtonUnfocused( (int)IndexedButton.Quit ) ) );
		ExitButton.Connect( "focus_entered", Callable.From( () => OnButtonFocused( (int)IndexedButton.Quit ) ) );
		ExitButton.Connect( "focus_exited", Callable.From( () => OnButtonUnfocused( (int)IndexedButton.Quit ) ) );
		ExitButton.Connect( "pressed", Callable.From( () => OnButtonPressed( () => GetTree().Quit() ) ) );

		Label AppVersion = GetNode<Label>( "AppVersion" );
		AppVersion.Text = "App Version " + (string)ProjectSettings.GetSetting( "application/config/version" );
		AppVersion.ProcessMode = ProcessModeEnum.Disabled;

		ButtonList = [
			StoryModeButton,
			ExtrasButton,
			SettingsButton,
			ModsButton,
			CreditsButton,
			ExitButton
		];

		StoryModeButton.Modulate = Selected;
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
		}
		else if ( Input.IsActionJustPressed( "ui_up" ) ) {
			ButtonList[ ButtonIndex ].EmitSignal( Button.SignalName.FocusExited );
			if ( ButtonIndex == 0 ) {
				ButtonIndex = ButtonList.Length - 1;
			} else {
				ButtonIndex--;
			}
			ButtonList[ ButtonIndex ].EmitSignal( Button.SignalName.FocusEntered );
		}
		else if ( Input.IsActionJustPressed( "ui_accept" ) || Input.IsActionJustPressed( "ui_enter" ) ) {
			ButtonList[ ButtonIndex ].EmitSignal( Button.SignalName.FocusEntered );
			ButtonList[ ButtonIndex ].CallDeferred( MethodName.EmitSignal, Button.SignalName.Pressed );
		}
	}
};
