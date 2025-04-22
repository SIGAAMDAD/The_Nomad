using Godot;
using Steamworks;

public partial class SettingsMenu : Control {
	private OptionButton VSync;
	private HSlider ImageSharpening;
	private OptionButton WindowModeOption;
	private OptionButton AntiAliasingOption;
	private OptionButton ShadowQuality;
	private OptionButton SunShadowQuality;
	private CheckBox SunLightEnabled;
	private CheckBox ShowFPS;

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

	private AudioStreamPlayer UIChannel;

	private Label RestartToActivateLabel;

	private TabBar ControlsTabBar;
	private TabBar VideoTabBar;
	private TabBar AudioTabBar;
	private TabBar AccessibilityTabBar;
	private TabBar NetworkTabBar;

	private void UpdateWindowScale() {
		Godot.Vector2I centerScreen = DisplayServer.ScreenGetPosition() + DisplayServer.ScreenGetSize() / 2;
		Godot.Vector2I windowSize = GetWindow().GetSizeWithDecorations();
		GetWindow().SetImePosition( centerScreen - windowSize / 2 );
	}
	private void ApplyVideoSettings() {
		switch ( VSync.Selected ) {
		case 0:
			DisplayServer.WindowSetVsyncMode( DisplayServer.VSyncMode.Disabled );
			break;
		case 1:
			DisplayServer.WindowSetVsyncMode( DisplayServer.VSyncMode.Adaptive );
			break;
		case 2:
			DisplayServer.WindowSetVsyncMode( DisplayServer.VSyncMode.Enabled );
			break;
		};

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
			DisplayServer.WindowSetSize( new Godot.Vector2I( 640, 480 ) );
			break;
		case (int)WindowMode.BorderlessWindowed:
			DisplayServer.WindowSetMode( DisplayServer.WindowMode.Windowed );
			DisplayServer.WindowSetFlag( DisplayServer.WindowFlags.Borderless, true );
			DisplayServer.WindowSetSize( new Godot.Vector2I( 640, 480 ) );
			break;
		case (int)WindowMode.Fullscreen:
			DisplayServer.WindowSetMode( DisplayServer.WindowMode.Fullscreen );
			DisplayServer.WindowSetFlag( DisplayServer.WindowFlags.Borderless, false );
			DisplayServer.WindowSetSize( new Godot.Vector2I( 640, 480 ) );
			break;
		case (int)WindowMode.BorderlessFullscreen:
			DisplayServer.WindowSetMode( DisplayServer.WindowMode.Fullscreen );
			DisplayServer.WindowSetFlag( DisplayServer.WindowFlags.Borderless, true );
			DisplayServer.WindowSetSize( new Godot.Vector2I( 640, 480 ) );
			break;
		};

//		UpdateWindowScale();
	}
	private void OnSaveSettingsButtonPressed() {
		UIChannel.Stream = UISfxManager.ButtonPressed;
		UIChannel.Play();

		SettingsData.SetVSync( (DisplayServer.VSyncMode)VSync.Selected );
		SettingsData.SetWindowMode( (WindowMode)WindowModeOption.Selected );
		SettingsData.SetAntiAliasing( (AntiAliasing)AntiAliasingOption.Selected );
		SettingsData.SetSunLightEnabled( SunLightEnabled.ButtonPressed );
		SettingsData.SetSunShadowQuality( (ShadowQuality)SunShadowQuality.Selected );
		SettingsData.SetShowFPS( ShowFPS.ButtonPressed );

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

	private void OnButtonFocused() {
		UIChannel.Stream = UISfxManager.ButtonFocused;
		UIChannel.Play();
	}
	private void OnButtonPressed() {
		UIChannel.Stream = UISfxManager.ButtonPressed;
		UIChannel.Play();
	}

	// TODO: make this more controller friendly
	public override void _Ready() {
		base._Ready();

		VSync = GetNode<OptionButton>( "TabContainer/Video/VBoxContainer/VSyncList/VSyncOptionButton" );
		VSync.SetProcess( false );
		VSync.SetProcessInternal( false );
		VSync.Connect( "pressed", Callable.From( OnButtonPressed ) );
		VSync.Connect( "item_selected", Callable.From( ( int index ) => { OnButtonPressed(); } ) );
		VSync.Connect( "mouse_entered", Callable.From( OnButtonFocused ) );

		WindowModeOption = GetNode<OptionButton>( "TabContainer/Video/VBoxContainer/WindowModeList/WindowModeOptionButton" );
		WindowModeOption.SetProcess( false );
		WindowModeOption.SetProcessInternal( false );
		WindowModeOption.Connect( "pressed", Callable.From( OnButtonPressed ) );
		WindowModeOption.Connect( "item_selected", Callable.From( ( int index ) => { OnButtonPressed(); } ) );
		WindowModeOption.Connect( "mouse_entered", Callable.From( OnButtonFocused ) );

		AntiAliasingOption = GetNode<OptionButton>( "TabContainer/Video/VBoxContainer/AntiAliasingList/AntiAliasingOptionButton" );
		AntiAliasingOption.SetProcess( false );
		AntiAliasingOption.SetProcessInternal( false );
		AntiAliasingOption.Connect( "pressed", Callable.From( OnButtonPressed ) );
		AntiAliasingOption.Connect( "item_selected", Callable.From( ( int index ) => { OnButtonPressed(); } ) );
		AntiAliasingOption.Connect( "mouse_entered", Callable.From( OnButtonFocused ) );

		ShadowQuality = GetNode<OptionButton>( "TabContainer/Video/VBoxContainer/ShadowQualityList/ShadowQualityOptionButton" );
		ShadowQuality.SetProcess( false );
		ShadowQuality.SetProcessInternal( false );
		ShadowQuality.Connect( "pressed", Callable.From( OnButtonPressed ) );
		ShadowQuality.Connect( "item_selected", Callable.From( ( int index ) => { OnButtonPressed(); } ) );
		ShadowQuality.Connect( "mouse_entered", Callable.From( OnButtonFocused ) );

		SunShadowQuality = GetNode<OptionButton>( "TabContainer/Video/VBoxContainer/SunShadowQualityList/SunShadowQualityOptionButton" );
		SunShadowQuality.SetProcess( false );
		SunShadowQuality.SetProcessInternal( false );
		SunShadowQuality.Connect( "pressed", Callable.From( OnButtonPressed ) );
		SunShadowQuality.Connect( "item_selected", Callable.From( ( int index ) => { OnButtonPressed(); } ) );
		SunShadowQuality.Connect( "mouse_entered", Callable.From( OnButtonFocused ) );

		SunLightEnabled = GetNode<CheckBox>( "TabContainer/Video/VBoxContainer/SunLightEnabledButton/SunLightEnabledCheckBox" );
		SunLightEnabled.SetProcess( false );
		SunLightEnabled.SetProcessInternal( false );
		SunLightEnabled.Connect( "pressed", Callable.From( OnButtonPressed ) );
		SunLightEnabled.Connect( "mouse_entered", Callable.From( OnButtonFocused ) );

		ShowFPS = GetNode<CheckBox>( "TabContainer/Video/VBoxContainer/ShowFPSButton/ShowFPSCheckBox" );
		ShowFPS.SetProcess( false );
		ShowFPS.SetProcessInternal( false );
		ShowFPS.Connect( "pressed", Callable.From( OnButtonPressed ) );
		ShowFPS.Connect( "mouse_entered", Callable.From( OnButtonFocused ) );

		EffectsOn = GetNode<CheckBox>( "TabContainer/Audio/VBoxContainer/EffectsOnButton/EffectsOnCheckBox" );
		EffectsOn.SetProcess( false );
		EffectsOn.SetProcessInternal( false );
		EffectsOn.Connect( "pressed", Callable.From( OnButtonPressed ) );
		EffectsOn.Connect( "mouse_entered", Callable.From( OnButtonFocused ) );

		EffectsVolume = GetNode<HSlider>( "TabContainer/Audio/VBoxContainer/EffectsVolumeSlider/EffectsVolumeHSlider" );
		EffectsVolume.SetProcess( false );
		EffectsVolume.SetProcessInternal( false );
		EffectsVolume.Connect( "changed", Callable.From( OnButtonPressed ) );
		EffectsVolume.Connect( "mouse_entered", Callable.From( OnButtonFocused ) );

		MusicOn = GetNode<CheckBox>( "TabContainer/Audio/VBoxContainer/MusicOnButton/MusicOnCheckBox" );
		MusicOn.SetProcess( false );
		MusicOn.SetProcessInternal( false );
		MusicOn.Connect( "pressed", Callable.From( OnButtonPressed ) );
		MusicOn.Connect( "mouse_entered", Callable.From( OnButtonFocused ) );

		MusicVolume = GetNode<HSlider>( "TabContainer/Audio/VBoxContainer/MusicVolumeSlider/MusicVolumeHSlider" );
		MusicVolume.SetProcess( false );
		MusicVolume.SetProcessInternal( false );
		MusicVolume.Connect( "changed", Callable.From( OnButtonPressed ) );
		MusicVolume.Connect( "mouse_entered", Callable.From( OnButtonFocused ) );

		MuteUnfocused = GetNode<CheckBox>( "TabContainer/Audio/VBoxContainer/MuteOnUnfocusedButton/MuteOnUnfocusedCheckBox" );
		MuteUnfocused.SetProcess( false );
		MuteUnfocused.SetProcessInternal( false );
		MuteUnfocused.Connect( "pressed", Callable.From( OnButtonPressed ) );
		MuteUnfocused.Connect( "mouse_entered", Callable.From( OnButtonFocused ) );

		HapticEnabled = GetNode<CheckBox>( "TabContainer/Accessibility/VBoxContainer/HapticFeedbackButton/HapticFeedbackCheckbox" );
		HapticEnabled.SetProcess( false );
		HapticEnabled.SetProcessInternal( false );
		HapticEnabled.Connect( "pressed", Callable.From( OnButtonPressed ) );
		HapticEnabled.Connect( "mouse_entered", Callable.From( OnButtonFocused ) );
		
		HapticStrength = GetNode<HSlider>( "TabContainer/Accessibility/VBoxContainer/HapticStrengthSlider/HapticStrengthSlider" );
		HapticStrength.Connect( "changed", Callable.From( OnButtonPressed ) );
		HapticStrength.Connect( "mouse_entered", Callable.From( OnButtonFocused ) );

		AutoAimEnabled = GetNode<CheckBox>( "TabContainer/Accessibility/VBoxContainer/AutoAimButton/AutoAimCheckbox" );
		AutoAimEnabled.SetProcess( false );
		AutoAimEnabled.SetProcessInternal( false );
		AutoAimEnabled.Connect( "pressed", Callable.From( OnButtonPressed ) );
		AutoAimEnabled.Connect( "mouse_entered", Callable.From( OnButtonFocused ) );

		DyslexiaMode = GetNode<CheckBox>( "TabContainer/Accessibility/VBoxContainer/DyslexiaModeButton/DyslexiaCheckbox" );
		DyslexiaMode.SetProcess( false );
		DyslexiaMode.SetProcessInternal( false );
		DyslexiaMode.Connect( "pressed", Callable.From( OnButtonPressed ) );
		DyslexiaMode.Connect( "mouse_entered", Callable.From( OnButtonFocused ) );

		NetworkingEnabled = GetNode<CheckBox>( "TabContainer/Network/VBoxContainer/EnableNetworkingButton/EnableNetworkingCheckbox" );
		NetworkingEnabled.SetProcess( false );
		NetworkingEnabled.SetProcessInternal( false );
		NetworkingEnabled.Connect( "pressed", Callable.From( OnButtonPressed ) );
		NetworkingEnabled.Connect( "mouse_entered", Callable.From( OnButtonFocused ) );

		FriendsOnly = GetNode<CheckBox>( "TabContainer/Network/VBoxContainer/FriendsOnlyButton/FriendsOnlyCheckbox" );
		FriendsOnly.SetProcess( false );
		FriendsOnly.SetProcessInternal( false );
		FriendsOnly.Connect( "pressed", Callable.From( OnButtonPressed ) );
		FriendsOnly.Connect( "mouse_entered", Callable.From( OnButtonFocused ) );

		TabContainer TabContainer = GetNode<TabContainer>( "TabContainer" );
		TabContainer.Connect( "tab_clicked", Callable.From(
				( int tab ) => {
					OnButtonPressed();
					switch ( tab ) {
					case 0:
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
					case 1:
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
					case 2:
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
					case 3:
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
					case 4:
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
					};
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
		AntiAliasingOption.Selected = (int)SettingsData.GetAntiAliasing();
		SunLightEnabled.ButtonPressed = SettingsData.GetSunLightEnabled();
		SunShadowQuality.Selected = (int)SettingsData.GetSunShadowQuality();
		ShowFPS.ButtonPressed = SettingsData.GetShowFPS();

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

		ApplyVideoSettings();

		Button SaveSettingsButton = GetNode<Button>( "SaveSettingsButton" );
		SaveSettingsButton.Connect( "focus_entered", Callable.From( OnButtonFocused ) );
		SaveSettingsButton.Connect( "mouse_entered", Callable.From( OnButtonFocused ) );
		SaveSettingsButton.Connect( "pressed", Callable.From( OnSaveSettingsButtonPressed ) );

		UIChannel = GetNode<AudioStreamPlayer>( "../UIChannel" );
		UIChannel.SetProcess( false );
		UIChannel.SetProcessInternal( false );
	}
};