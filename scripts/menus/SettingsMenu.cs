using Godot;
using Steamworks;

public partial class SettingsMenu : Control {
	private OptionButton VSync;
	private HSlider ImageSharpening;
	private OptionButton ResolutionOption;
	private OptionButton WindowModeOption;
	private OptionButton AntiAliasingOption;
	private OptionButton ShadowQuality;
	private OptionButton ShadowFilterQuality;
	private OptionButton MaxFps;
	private CheckBox ShowFPS;
	private CheckBox ShowBlood;

	private CheckBox EffectsOn;
	private HSlider EffectsVolume;
	private CheckBox MusicOn;
	private HSlider MusicVolume;
	private CheckBox MuteUnfocused;

	private CheckBox HapticEnabled;
	private HSlider HapticStrength;
	private CheckBox AutoAimEnabled;
	private CheckBox DyslexiaMode;

	private CheckBox NetworkingEnabled;
	private CheckBox FriendsOnly;

	private Label RestartToActivateLabel;

	private TabContainer TabContainer;
	private TabBar ControlsTabBar;
	private TabBar VideoTabBar;
	private TabBar AudioTabBar;
	private TabBar AccessibilityTabBar;
	private TabBar NetworkTabBar;

	public enum CategoryTabBar {
		Video,
		Audio,
		Accessibility,
		Network,
		Controls
	};

	private void UpdateWindowScale() {
		Godot.Vector2I centerScreen = DisplayServer.ScreenGetPosition() + DisplayServer.ScreenGetSize() / 2;
		Godot.Vector2I windowSize = GetWindow().GetSizeWithDecorations();
		GetWindow().SetImePosition( centerScreen - windowSize / 2 );
	}
	private void ApplyVideoSettings() {
		switch ( VSync.Selected ) {
		case 0:
			DisplayServer.WindowSetVsyncMode( DisplayServer.VSyncMode.Disabled );
			ProjectSettings.SetSetting(
				"rendering/rendering_device/vsync/swapchain_image_count",
				2
			);
			break;
		case 1:
			DisplayServer.WindowSetVsyncMode( DisplayServer.VSyncMode.Adaptive );
			ProjectSettings.SetSetting(
				"rendering/rendering_device/vsync/swapchain_image_count",
				2
			);
			break;
		case 2:
			DisplayServer.WindowSetVsyncMode( DisplayServer.VSyncMode.Enabled );
			ProjectSettings.SetSetting(
				"rendering/rendering_device/vsync/swapchain_image_count",
				2
			);
			break;
		case 3:
			DisplayServer.WindowSetVsyncMode( DisplayServer.VSyncMode.Mailbox );
			ProjectSettings.SetSetting(
				"rendering/rendering_device/vsync/swapchain_image_count",
				3
			);
			break;
		};

		switch ( MaxFps.Selected ) {
		case 0:
			Engine.MaxFps = 0;
			break;
		case 1:
			Engine.MaxFps = 30;
			break;
		case 2:
			Engine.MaxFps = 45;
			break;
		case 3:
			Engine.MaxFps = 60;
			break;
		case 4:
			Engine.MaxFps = 90;
			break;
		case 5:
			Engine.MaxFps = 125;
			break;
		case 6:
			Engine.MaxFps = 225;
			break;
		case 7:
			break;
		};

		Godot.Vector2I windowSize = Godot.Vector2I.Zero;
		switch ( (Resolution)ResolutionOption.Selected ) {
		case Resolution.Res_640x480:
			windowSize = new Godot.Vector2I( 640, 480 );
			break;
		case Resolution.Res_800x600:
			windowSize = new Godot.Vector2I( 800, 600 );
			break;
		case Resolution.Res_1280x768:
			windowSize = new Godot.Vector2I( 1280, 768 );
			break;
		case Resolution.Res_1920x1080:
			windowSize = new Godot.Vector2I( 1920, 1080 );
			break;
		case Resolution.Res_1600x900:
			windowSize = new Godot.Vector2I( 1600, 900 );
			break;
		default:
			windowSize = new Godot.Vector2I( 640, 480 );
			break;
		};
		DisplayServer.WindowSetSize( windowSize );

		Rid viewport = GetTree().Root.GetViewportRid();
		switch ( (AntiAliasing)AntiAliasingOption.Selected ) {
		case AntiAliasing.None:
			RenderingServer.ViewportSetUseTaa( viewport, false );
			RenderingServer.ViewportSetScreenSpaceAA( viewport, RenderingServer.ViewportScreenSpaceAA.Disabled );
			RenderingServer.ViewportSetMsaa2D( viewport, RenderingServer.ViewportMsaa.Disabled );
			break;
		case AntiAliasing.FXAA:
			RenderingServer.ViewportSetUseTaa( viewport, false );
			RenderingServer.ViewportSetScreenSpaceAA( viewport, RenderingServer.ViewportScreenSpaceAA.Fxaa );
			RenderingServer.ViewportSetMsaa2D( viewport, RenderingServer.ViewportMsaa.Disabled );
			break;
		case AntiAliasing.MSAA_2x:
			RenderingServer.ViewportSetUseTaa( viewport, false );
			RenderingServer.ViewportSetScreenSpaceAA( viewport, RenderingServer.ViewportScreenSpaceAA.Disabled );
			RenderingServer.ViewportSetMsaa2D( viewport, RenderingServer.ViewportMsaa.Msaa2X );
			break;
		case AntiAliasing.MSAA_4x:
			RenderingServer.ViewportSetUseTaa( viewport, false );
			RenderingServer.ViewportSetScreenSpaceAA( viewport, RenderingServer.ViewportScreenSpaceAA.Disabled );
			RenderingServer.ViewportSetMsaa2D( viewport, RenderingServer.ViewportMsaa.Msaa4X );
			break;
		case AntiAliasing.MSAA_8x:
			RenderingServer.ViewportSetUseTaa( viewport, false );
			RenderingServer.ViewportSetScreenSpaceAA( viewport, RenderingServer.ViewportScreenSpaceAA.Disabled );
			RenderingServer.ViewportSetMsaa2D( viewport, RenderingServer.ViewportMsaa.Msaa8X );
			break;
		case AntiAliasing.TAA:
			RenderingServer.ViewportSetUseTaa( viewport, true );
			RenderingServer.ViewportSetScreenSpaceAA( viewport, RenderingServer.ViewportScreenSpaceAA.Disabled );
			RenderingServer.ViewportSetMsaa2D( viewport, RenderingServer.ViewportMsaa.Disabled );
			break;
		case AntiAliasing.FXAA_and_TAA:
			RenderingServer.ViewportSetUseTaa( viewport, true );
			RenderingServer.ViewportSetScreenSpaceAA( viewport, RenderingServer.ViewportScreenSpaceAA.Fxaa );
			RenderingServer.ViewportSetMsaa2D( viewport, RenderingServer.ViewportMsaa.Disabled );
			break;
		};

		switch ( WindowModeOption.Selected ) {
		case (int)WindowMode.Windowed:
			DisplayServer.WindowSetMode( DisplayServer.WindowMode.Windowed );
			DisplayServer.WindowSetFlag( DisplayServer.WindowFlags.Borderless, false );
			break;
		case (int)WindowMode.BorderlessWindowed:
			DisplayServer.WindowSetMode( DisplayServer.WindowMode.Windowed );
			DisplayServer.WindowSetFlag( DisplayServer.WindowFlags.Borderless, true );
			break;
		case (int)WindowMode.Fullscreen:
			DisplayServer.WindowSetMode( DisplayServer.WindowMode.Fullscreen );
			DisplayServer.WindowSetFlag( DisplayServer.WindowFlags.Borderless, false );
			break;
		case (int)WindowMode.BorderlessFullscreen:
			DisplayServer.WindowSetMode( DisplayServer.WindowMode.Fullscreen );
			DisplayServer.WindowSetFlag( DisplayServer.WindowFlags.Borderless, true );
			break;
		case (int)WindowMode.ExclusiveFullscreen:
			DisplayServer.WindowSetMode( DisplayServer.WindowMode.ExclusiveFullscreen );
			DisplayServer.WindowSetFlag( DisplayServer.WindowFlags.Borderless, true );
			break;
		};

		UpdateWindowScale();
	}
	private void OnSaveSettingsButtonPressed() {
		UIAudioManager.OnButtonPressed();

		SettingsData.SetVSync( (VSyncMode)VSync.Selected );
		SettingsData.SetShadowQuality( (ShadowQuality)ShadowQuality.Selected );
		SettingsData.SetShadowFilterQuality( (ShadowFilterQuality)ShadowFilterQuality.Selected );
		SettingsData.SetWindowMode( (WindowMode)WindowModeOption.Selected );
		SettingsData.SetResolution( (Resolution)ResolutionOption.Selected );
		SettingsData.SetAntiAliasing( (AntiAliasing)AntiAliasingOption.Selected );
		SettingsData.SetShowFPS( ShowFPS.ButtonPressed );
		SettingsData.SetShowBlood( ShowBlood.ButtonPressed );
		switch ( MaxFps.Selected ) {
		case 0:
			SettingsData.SetMaxFps( 0 );
			break;
		case 1:
			SettingsData.SetMaxFps( 30 );
			break;
		case 2:
			SettingsData.SetMaxFps( 45 );
			break;
		case 3:
			SettingsData.SetMaxFps( 60 );
			break;
		case 4:
			SettingsData.SetMaxFps( 90 );
			break;
		case 5:
			SettingsData.SetMaxFps( 125 );
			break;
		case 6:
			SettingsData.SetMaxFps( 225 );
			break;
		case 7:
			break;
		};

		SettingsData.SetEffectsOn( EffectsOn.ButtonPressed );
		SettingsData.SetEffectsVolume( (float)EffectsVolume.Value );
		SettingsData.SetMusicOn( MusicOn.ButtonPressed );
		SettingsData.SetMusicVolume( (float)MusicVolume.Value );
		SettingsData.SetMuteUnfocused( MuteUnfocused.ButtonPressed );

		SettingsData.SetHapticEnabled( HapticEnabled.ButtonPressed );
		SettingsData.SetHapticStrength( (float)HapticStrength.Value );
		SettingsData.SetAutoAimEnabled( AutoAimEnabled.ButtonPressed );
		SettingsData.SetDyslexiaMode( DyslexiaMode.ButtonPressed );

		SettingsData.SetNetworkingEnabled( NetworkingEnabled.ButtonPressed );
		SettingsData.SetFriendsOnlyNetworking( FriendsOnly.ButtonPressed );

		ApplyVideoSettings();

		SettingsData.Save();
	}

	private void OnSyncToSteamButtonPressed() {
		Console.PrintLine( "Synchronizing settings to SteamCloud..." );
		SteamManager.SaveCloudFile( "user://settings.ini" );
	}
	private void OnRemoveSteamSyncButtonPressed() {
		SteamRemoteStorage.FileDelete( "settings.ini" );
	}

	// TODO: make this more controller friendly
	public override void _Ready() {
		base._Ready();

		VSync = GetNode<OptionButton>( "TabContainer/Video/VBoxContainer/VSyncList/VSyncOptionButton" );
		VSync.Connect( "pressed", Callable.From( UIAudioManager.OnButtonPressed ) );
		VSync.Connect( "item_selected", Callable.From( ( int index ) => UIAudioManager.OnButtonPressed() ) );
		VSync.Connect( "mouse_entered", Callable.From( UIAudioManager.OnButtonFocused ) );

		WindowModeOption = GetNode<OptionButton>( "TabContainer/Video/VBoxContainer/WindowModeList/WindowModeOptionButton" );
		WindowModeOption.Connect( "pressed", Callable.From( UIAudioManager.OnButtonPressed ) );
		WindowModeOption.Connect( "item_selected", Callable.From( ( int index ) => UIAudioManager.OnButtonPressed() ) );
		WindowModeOption.Connect( "mouse_entered", Callable.From( UIAudioManager.OnButtonFocused ) );

		ResolutionOption = GetNode<OptionButton>( "TabContainer/Video/VBoxContainer/ResolutionList/ResolutionOptionButton" );
		ResolutionOption.Connect( "pressed", Callable.From( UIAudioManager.OnButtonPressed ) );
		ResolutionOption.Connect( "item_selected", Callable.From( ( int index ) => UIAudioManager.OnButtonPressed() ) );
		ResolutionOption.Connect( "mouse_entered", Callable.From( UIAudioManager.OnButtonFocused ) );

		MaxFps = GetNode<OptionButton>( "TabContainer/Video/VBoxContainer/MaxFpsList/MaxFpsOptionButton" );
		MaxFps.Connect( "pressed", Callable.From( UIAudioManager.OnButtonPressed ) );
		MaxFps.Connect( "item_selected", Callable.From( ( int index ) => UIAudioManager.OnButtonPressed() ) );
		MaxFps.Connect( "mouse_entered", Callable.From( UIAudioManager.OnButtonFocused ) );

		AntiAliasingOption = GetNode<OptionButton>( "TabContainer/Video/VBoxContainer/AntiAliasingList/AntiAliasingOptionButton" );
		AntiAliasingOption.Connect( "pressed", Callable.From( UIAudioManager.OnButtonPressed ) );
		AntiAliasingOption.Connect( "item_selected", Callable.From( ( int index ) => UIAudioManager.OnButtonPressed() ) );
		AntiAliasingOption.Connect( "mouse_entered", Callable.From( UIAudioManager.OnButtonFocused ) );

		ShadowQuality = GetNode<OptionButton>( "TabContainer/Video/VBoxContainer/ShadowQualityList/ShadowQualityOptionButton" );
		ShadowQuality.Connect( "pressed", Callable.From( UIAudioManager.OnButtonPressed ) );
		ShadowQuality.Connect( "item_selected", Callable.From( ( int index ) => UIAudioManager.OnButtonPressed() ) );
		ShadowQuality.Connect( "mouse_entered", Callable.From( UIAudioManager.OnButtonFocused ) );

		ShadowFilterQuality = GetNode<OptionButton>( "TabContainer/Video/VBoxContainer/ShadowFilterQualityList/ShadowFilterQualityOptionButton" );
		ShadowFilterQuality.Connect( "pressed", Callable.From( UIAudioManager.OnButtonPressed ) );
		ShadowFilterQuality.Connect( "item_selected", Callable.From( ( int index ) => UIAudioManager.OnButtonPressed() ) );
		ShadowFilterQuality.Connect( "mouse_entered", Callable.From( UIAudioManager.OnButtonFocused ) );

		ShowFPS = GetNode<CheckBox>( "TabContainer/Video/VBoxContainer/ShowFPSButton/ShowFPSCheckBox" );
		ShowFPS.Connect( "pressed", Callable.From( UIAudioManager.OnButtonPressed ) );
		ShowFPS.Connect( "mouse_entered", Callable.From( UIAudioManager.OnButtonFocused ) );

		ShowBlood = GetNode<CheckBox>( "TabContainer/Video/VBoxContainer/ShowBloodButton/ShowBloodCheckBox" );
		ShowBlood.Connect( "pressed", Callable.From( UIAudioManager.OnButtonPressed ) );
		ShowBlood.Connect( "mouse_entered", Callable.From( UIAudioManager.OnButtonFocused ) );

		EffectsOn = GetNode<CheckBox>( "TabContainer/Audio/VBoxContainer/EffectsOnButton/EffectsOnCheckBox" );
		EffectsOn.Connect( "pressed", Callable.From( UIAudioManager.OnButtonPressed ) );
		EffectsOn.Connect( "mouse_entered", Callable.From( UIAudioManager.OnButtonFocused ) );

		EffectsVolume = GetNode<HSlider>( "TabContainer/Audio/VBoxContainer/EffectsVolumeSlider/EffectsVolumeHSlider" );
		EffectsVolume.Connect( "changed", Callable.From( UIAudioManager.OnButtonPressed ) );
		EffectsVolume.Connect( "mouse_entered", Callable.From( UIAudioManager.OnButtonFocused ) );

		MusicOn = GetNode<CheckBox>( "TabContainer/Audio/VBoxContainer/MusicOnButton/MusicOnCheckBox" );
		MusicOn.Connect( "pressed", Callable.From( UIAudioManager.OnButtonPressed ) );
		MusicOn.Connect( "mouse_entered", Callable.From( UIAudioManager.OnButtonFocused ) );

		MusicVolume = GetNode<HSlider>( "TabContainer/Audio/VBoxContainer/MusicVolumeSlider/MusicVolumeHSlider" );
		MusicVolume.Connect( "changed", Callable.From( UIAudioManager.OnButtonPressed ) );
		MusicVolume.Connect( "mouse_entered", Callable.From( UIAudioManager.OnButtonFocused ) );

		MuteUnfocused = GetNode<CheckBox>( "TabContainer/Audio/VBoxContainer/MuteOnUnfocusedButton/MuteOnUnfocusedCheckBox" );
		MuteUnfocused.Connect( "pressed", Callable.From( UIAudioManager.OnButtonPressed ) );
		MuteUnfocused.Connect( "mouse_entered", Callable.From( UIAudioManager.OnButtonFocused ) );

		HapticEnabled = GetNode<CheckBox>( "TabContainer/Accessibility/VBoxContainer/HapticFeedbackButton/HapticFeedbackCheckbox" );
		HapticEnabled.Connect( "pressed", Callable.From( UIAudioManager.OnButtonPressed ) );
		HapticEnabled.Connect( "mouse_entered", Callable.From( UIAudioManager.OnButtonFocused ) );

		HapticStrength = GetNode<HSlider>( "TabContainer/Accessibility/VBoxContainer/HapticStrengthSlider/HapticStrengthSlider" );
		HapticStrength.Connect( "changed", Callable.From( UIAudioManager.OnButtonPressed ) );
		HapticStrength.Connect( "mouse_entered", Callable.From( UIAudioManager.OnButtonFocused ) );

		AutoAimEnabled = GetNode<CheckBox>( "TabContainer/Accessibility/VBoxContainer/AutoAimButton/AutoAimCheckbox" );
		AutoAimEnabled.Connect( "pressed", Callable.From( UIAudioManager.OnButtonPressed ) );
		AutoAimEnabled.Connect( "mouse_entered", Callable.From( UIAudioManager.OnButtonFocused ) );

		DyslexiaMode = GetNode<CheckBox>( "TabContainer/Accessibility/VBoxContainer/DyslexiaModeButton/DyslexiaCheckbox" );
		DyslexiaMode.Connect( "pressed", Callable.From( UIAudioManager.OnButtonPressed ) );
		DyslexiaMode.Connect( "mouse_entered", Callable.From( UIAudioManager.OnButtonFocused ) );

		NetworkingEnabled = GetNode<CheckBox>( "TabContainer/Network/VBoxContainer/EnableNetworkingButton/EnableNetworkingCheckbox" );
		NetworkingEnabled.Connect( "pressed", Callable.From( UIAudioManager.OnButtonPressed ) );
		NetworkingEnabled.Connect( "mouse_entered", Callable.From( UIAudioManager.OnButtonFocused ) );

		FriendsOnly = GetNode<CheckBox>( "TabContainer/Network/VBoxContainer/FriendsOnlyButton/FriendsOnlyCheckbox" );
		FriendsOnly.Connect( "pressed", Callable.From( UIAudioManager.OnButtonPressed ) );
		FriendsOnly.Connect( "mouse_entered", Callable.From( UIAudioManager.OnButtonFocused ) );

		TabContainer = GetNode<TabContainer>( "TabContainer" );
		TabContainer.Connect( "tab_clicked", Callable.From(
				( int tab ) => {
					UIAudioManager.OnButtonPressed();
					switch ( (CategoryTabBar)tab ) {
					case CategoryTabBar.Video:
						VideoTabBar.SetProcess( true );
						VideoTabBar.SetProcessInternal( true );
						VideoTabBar.ProcessMode = ProcessModeEnum.Always;

						AudioTabBar.SetProcess( false );
						AudioTabBar.SetProcessInternal( false );
						AudioTabBar.ProcessMode = ProcessModeEnum.Disabled;

						AccessibilityTabBar.SetProcess( false );
						AccessibilityTabBar.SetProcessInternal( false );
						AccessibilityTabBar.ProcessMode = ProcessModeEnum.Disabled;

						ControlsTabBar.SetProcess( false );
						ControlsTabBar.SetProcessInternal( false );
						ControlsTabBar.ProcessMode = ProcessModeEnum.Disabled;

						NetworkTabBar.SetProcess( false );
						NetworkTabBar.SetProcessInternal( false );
						NetworkTabBar.ProcessMode = ProcessModeEnum.Disabled;
						break;
					case CategoryTabBar.Audio:
						VideoTabBar.SetProcess( false );
						VideoTabBar.SetProcessInternal( false );
						VideoTabBar.ProcessMode = ProcessModeEnum.Disabled;

						AudioTabBar.SetProcess( true );
						AudioTabBar.SetProcessInternal( true );
						AudioTabBar.ProcessMode = ProcessModeEnum.Always;

						AccessibilityTabBar.SetProcess( false );
						AccessibilityTabBar.SetProcessInternal( false );
						AccessibilityTabBar.ProcessMode = ProcessModeEnum.Disabled;

						ControlsTabBar.SetProcess( false );
						ControlsTabBar.SetProcessInternal( false );
						ControlsTabBar.ProcessMode = ProcessModeEnum.Disabled;

						NetworkTabBar.SetProcess( false );
						NetworkTabBar.SetProcessInternal( false );
						NetworkTabBar.ProcessMode = ProcessModeEnum.Disabled;
						break;
					case CategoryTabBar.Accessibility:
						VideoTabBar.SetProcess( false );
						VideoTabBar.SetProcessInternal( false );
						VideoTabBar.ProcessMode = ProcessModeEnum.Disabled;

						AudioTabBar.SetProcess( false );
						AudioTabBar.SetProcessInternal( false );
						AudioTabBar.ProcessMode = ProcessModeEnum.Disabled;

						AccessibilityTabBar.SetProcess( true );
						AccessibilityTabBar.SetProcessInternal( true );
						AccessibilityTabBar.ProcessMode = ProcessModeEnum.Always;

						ControlsTabBar.SetProcess( false );
						ControlsTabBar.SetProcessInternal( false );
						ControlsTabBar.ProcessMode = ProcessModeEnum.Disabled;

						NetworkTabBar.SetProcess( false );
						NetworkTabBar.SetProcessInternal( false );
						NetworkTabBar.ProcessMode = ProcessModeEnum.Disabled;
						break;
					case CategoryTabBar.Controls:
						VideoTabBar.SetProcess( false );
						VideoTabBar.SetProcessInternal( false );
						VideoTabBar.ProcessMode = ProcessModeEnum.Disabled;

						AudioTabBar.SetProcess( false );
						AudioTabBar.SetProcessInternal( false );
						AudioTabBar.ProcessMode = ProcessModeEnum.Disabled;

						AccessibilityTabBar.SetProcess( false );
						AccessibilityTabBar.SetProcessInternal( false );
						AccessibilityTabBar.ProcessMode = ProcessModeEnum.Disabled;

						ControlsTabBar.SetProcess( true );
						ControlsTabBar.SetProcessInternal( true );
						ControlsTabBar.ProcessMode = ProcessModeEnum.Always;

						NetworkTabBar.SetProcess( false );
						NetworkTabBar.SetProcessInternal( false );
						NetworkTabBar.ProcessMode = ProcessModeEnum.Disabled;
						break;
					case CategoryTabBar.Network:
						VideoTabBar.SetProcess( false );
						VideoTabBar.SetProcessInternal( false );
						VideoTabBar.ProcessMode = ProcessModeEnum.Disabled;

						AudioTabBar.SetProcess( false );
						AudioTabBar.SetProcessInternal( false );
						AudioTabBar.ProcessMode = ProcessModeEnum.Disabled;

						AccessibilityTabBar.SetProcess( false );
						AccessibilityTabBar.SetProcessInternal( false );
						AccessibilityTabBar.ProcessMode = ProcessModeEnum.Disabled;

						ControlsTabBar.SetProcess( false );
						ControlsTabBar.SetProcessInternal( false );
						ControlsTabBar.ProcessMode = ProcessModeEnum.Disabled;

						NetworkTabBar.SetProcess( true );
						NetworkTabBar.SetProcessInternal( true );
						NetworkTabBar.ProcessMode = ProcessModeEnum.Always;
						break;
					}
					;
				}
			)
		);

		VideoTabBar = GetNode<TabBar>( "TabContainer/Video" );

		AudioTabBar = GetNode<TabBar>( "TabContainer/Audio" );
		AudioTabBar.ProcessMode = ProcessModeEnum.Disabled;

		AccessibilityTabBar = GetNode<TabBar>( "TabContainer/Accessibility" );
		AccessibilityTabBar.ProcessMode = ProcessModeEnum.Disabled;

		ControlsTabBar = GetNode<TabBar>( "TabContainer/Controls" );
		ControlsTabBar.ProcessMode = ProcessModeEnum.Disabled;

		NetworkTabBar = GetNode<TabBar>( "TabContainer/Network" );
		NetworkTabBar.ProcessMode = ProcessModeEnum.Disabled;

		VSync.Selected = (int)SettingsData.GetVSync();
		WindowModeOption.Selected = (int)SettingsData.GetWindowMode();
		ResolutionOption.Selected = (int)SettingsData.GetResolution();
		AntiAliasingOption.Selected = (int)SettingsData.GetAntiAliasing();
		ShadowQuality.Selected = (int)SettingsData.GetShadowQuality();
		switch ( SettingsData.GetMaxFps() ) {
		case 0:
			MaxFps.Selected = 0;
			break;
		case 30:
			MaxFps.Selected = 1;
			break;
		case 45:
			MaxFps.Selected = 2;
			break;
		case 60:
			MaxFps.Selected = 3;
			break;
		case 90:
			MaxFps.Selected = 4;
			break;
		case 125:
			MaxFps.Selected = 5;
			break;
		case 225:
			MaxFps.Selected = 6;
			break;
		default: // custom, dev setting, or someone's fucking with the .ini file
			Console.PrintLine( "Custom FPS set." );
			MaxFps.AddItem( "CUSTOM", 7 );
			MaxFps.Selected = 7;
			break;
		}
		;
		ShadowFilterQuality.Selected = (int)SettingsData.GetShadowFilterQuality();
		ShowFPS.ButtonPressed = SettingsData.GetShowFPS();
		ShowBlood.ButtonPressed = SettingsData.GetShowBlood();

		EffectsOn.ButtonPressed = SettingsData.GetEffectsOn();
		EffectsVolume.Value = SettingsData.GetEffectsVolume();
		MusicOn.ButtonPressed = SettingsData.GetMusicOn();
		MusicVolume.Value = SettingsData.GetMusicVolume();
		MuteUnfocused.ButtonPressed = SettingsData.GetMuteUnfocused();

		HapticEnabled.ButtonPressed = SettingsData.GetHapticEnabled();
		HapticStrength.Value = SettingsData.GetHapticStrength();
		AutoAimEnabled.ButtonPressed = SettingsData.GetAutoAimEnabled();
		DyslexiaMode.ButtonPressed = SettingsData.GetDyslexiaMode();

		NetworkingEnabled.ButtonPressed = SettingsData.GetNetworkingEnabled();
		FriendsOnly.ButtonPressed = SettingsData.GetFriendsOnlyNetworking();

		/*
		Button RemoveSteamSyncButton = GetNode<Button>( "RemoveSteamSyncButton" );
		RemoveSteamSyncButton.Connect( "focus_entered", Callable.From( UIAudioManager.OnButtonFocused ) );
		RemoveSteamSyncButton.Connect( "mouse_entered", Callable.From( UIAudioManager.OnButtonFocused ) );
		RemoveSteamSyncButton.Connect( "pressed", Callable.From( OnRemoveSteamSyncButtonPressed ) );

		Button SyncToSteamButton = GetNode<Button>( "SyncToSteamButton" );
		SyncToSteamButton.Connect( "focus_entered", Callable.From( UIAudioManager.OnButtonFocused ) );
		SyncToSteamButton.Connect( "mouse_entered", Callable.From( UIAudioManager.OnButtonFocused ) );
		SyncToSteamButton.Connect( "pressed", Callable.From( OnSyncToSteamButtonPressed ) );
		*/

		Button SaveSettingsButton = GetNode<Button>( "SaveSettingsButton" );
		SaveSettingsButton.Connect( "focus_entered", Callable.From( UIAudioManager.OnButtonFocused ) );
		SaveSettingsButton.Connect( "mouse_entered", Callable.From( UIAudioManager.OnButtonFocused ) );
		SaveSettingsButton.Connect( "pressed", Callable.From( OnSaveSettingsButtonPressed ) );
	}
	public override void _UnhandledInput( InputEvent @event ) {
		base._UnhandledInput( @event );

		if ( Input.IsActionJustPressed( "menu_settings_next_category" ) ) {
			switch ( (CategoryTabBar)TabContainer.CurrentTab ) {
			case CategoryTabBar.Video:
				AudioTabBar.Show();
				TabContainer.EmitSignal( "tab_clicked", (int)CategoryTabBar.Audio );
				break;
			case CategoryTabBar.Audio:
				AccessibilityTabBar.Show();
				TabContainer.EmitSignal( "tab_clicked", (int)CategoryTabBar.Accessibility );
				break;
			case CategoryTabBar.Accessibility:
				NetworkTabBar.Show();
				TabContainer.EmitSignal( "tab_clicked", (int)CategoryTabBar.Network );
				break;
			case CategoryTabBar.Network:
				ControlsTabBar.Show();
				TabContainer.EmitSignal( "tab_clicked", (int)CategoryTabBar.Controls );
				break;
			case CategoryTabBar.Controls:
				VideoTabBar.Show();
				TabContainer.EmitSignal( "tab_clicked", (int)CategoryTabBar.Video );
				break;
			};
		}
		if ( Input.IsActionJustPressed( "menu_settings_prev_category" ) ) {
			switch ( (CategoryTabBar)TabContainer.CurrentTab ) {
			case CategoryTabBar.Video:
				ControlsTabBar.Show();
				TabContainer.EmitSignal( "tab_clicked", (int)CategoryTabBar.Controls );
				break;
			case CategoryTabBar.Audio:
				VideoTabBar.Show();
				TabContainer.EmitSignal( "tab_clicked", (int)CategoryTabBar.Video );
				break;
			case CategoryTabBar.Accessibility:
				AudioTabBar.Show();
				TabContainer.EmitSignal( "tab_clicked", (int)CategoryTabBar.Audio );
				break;
			case CategoryTabBar.Network:
				AccessibilityTabBar.Show();
				TabContainer.EmitSignal( "tab_clicked", (int)CategoryTabBar.Accessibility );
				break;
			case CategoryTabBar.Controls:
				NetworkTabBar.Show();
				TabContainer.EmitSignal( "tab_clicked", (int)CategoryTabBar.Network );
				break;
			};
		}
	}
};