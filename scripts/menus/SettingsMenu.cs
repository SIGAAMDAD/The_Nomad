using System.Runtime.InteropServices.Marshalling;
using Godot;

public partial class SettingsMenu : Control {
	private CheckBox VSync;
	private HSlider ImageSharpening;
	private OptionButton WindowModeOption;
	private OptionButton AntiAliasingOption;
	private OptionButton ShadowQuality;

	private CheckBox EffectsOn;
	private HSlider EffectsVolume;
	private CheckBox MusicOn;
	private HSlider MusicVolume;
	private CheckBox MuteUnfocused;

	private CheckBox HapticEnabled;
	private HSlider HapticStrength;
	private CheckBox AutoAimEnabled;
	private CheckBox DyslexiaMode;

	private AudioStreamPlayer UIChannel;

	private Label RestartToActivateLabel;

	private TabBar ControlsTabBar;
	private TabBar VideoTabBar;
	private TabBar AudioTabBar;
	private TabBar AccessibilityTabBar;

	private void UpdateWindowScale() {
		Godot.Vector2I centerScreen = DisplayServer.ScreenGetPosition() + DisplayServer.ScreenGetSize() / 2;
		Godot.Vector2I windowSize = GetWindow().GetSizeWithDecorations();
		GetWindow().SetImePosition( centerScreen - windowSize / 2 );
	}
	private void ApplyVideoSettings() {
		if ( VSync.ButtonPressed ) {
			DisplayServer.WindowSetVsyncMode( DisplayServer.VSyncMode.Adaptive );
		} else {
			DisplayServer.WindowSetVsyncMode( DisplayServer.VSyncMode.Disabled );
		}

		Rid viewport = GetTree().Root.GetViewportRid();
		switch ( AntiAliasingOption.Selected ) {
		case (int)AntiAliasing.None:
			RenderingServer.ViewportSetScreenSpaceAA( viewport, RenderingServer.ViewportScreenSpaceAA.Disabled );
			RenderingServer.ViewportSetMsaa2D( viewport, RenderingServer.ViewportMsaa.Disabled );
			break;
		case (int)AntiAliasing.FXAA:
			RenderingServer.ViewportSetScreenSpaceAA( viewport, RenderingServer.ViewportScreenSpaceAA.Fxaa );
			RenderingServer.ViewportSetMsaa2D( viewport, RenderingServer.ViewportMsaa.Disabled );
			break;
		case (int)AntiAliasing.MSAA_2x:
			RenderingServer.ViewportSetScreenSpaceAA( viewport, RenderingServer.ViewportScreenSpaceAA.Disabled );
			RenderingServer.ViewportSetMsaa2D( viewport, RenderingServer.ViewportMsaa.Msaa2X );
			break;
		case (int)AntiAliasing.MSAA_4x:
			RenderingServer.ViewportSetScreenSpaceAA( viewport, RenderingServer.ViewportScreenSpaceAA.Disabled );
			RenderingServer.ViewportSetMsaa2D( viewport, RenderingServer.ViewportMsaa.Msaa4X );
			break;
		case (int)AntiAliasing.MSAA_8x:
			RenderingServer.ViewportSetScreenSpaceAA( viewport, RenderingServer.ViewportScreenSpaceAA.Disabled );
			RenderingServer.ViewportSetMsaa2D( viewport, RenderingServer.ViewportMsaa.Msaa8X );
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
		};

		UpdateWindowScale();
	}
	private void OnSaveSettingsButtonPressed() {
		UIChannel.Stream = UISfxManager.ButtonPressed;
		UIChannel.Play();

		SettingsData.SetVSync( VSync.ButtonPressed );
		SettingsData.SetWindowMode( (WindowMode)WindowModeOption.Selected );
		SettingsData.SetAntiAliasing( (AntiAliasing)AntiAliasingOption.Selected );

		SettingsData.SetEffectsOn( EffectsOn.ButtonPressed );
		SettingsData.SetEffectsVolume( (float)EffectsVolume.Value );
		SettingsData.SetMusicOn( MusicOn.ButtonPressed );
		SettingsData.SetMusicVolume( (float)MusicVolume.Value );
		SettingsData.SetMuteUnfocused( MuteUnfocused.ButtonPressed );

		SettingsData.SetHapticEnabled( HapticEnabled.ButtonPressed );
		SettingsData.SetHapticStrength( (float)HapticStrength.Value );
		SettingsData.SetAutoAimEnabled( AutoAimEnabled.ButtonPressed );
		SettingsData.SetDyslexiaMode( DyslexiaMode.ButtonPressed );

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

		if ( SettingsData.GetDyslexiaMode() ) {
			Theme = AccessibilityManager.DyslexiaTheme;
		} else {
			Theme = AccessibilityManager.DefaultTheme;
		}

		VSync = GetNode<CheckBox>( "TabContainer/Video/VBoxContainer/VSyncButton/VSyncCheckBox" );
		VSync.Connect( "pressed", Callable.From( OnButtonPressed ) );
		VSync.Connect( "mouse_entered", Callable.From( OnButtonFocused ) );

		WindowModeOption = GetNode<OptionButton>( "TabContainer/Video/VBoxContainer/WindowModeList/WindowModeOptionButton" );
		WindowModeOption.Connect( "pressed", Callable.From( OnButtonPressed ) );
		WindowModeOption.Connect( "item_selected", Callable.From( ( int index ) => { OnButtonPressed(); } ) );
		WindowModeOption.Connect( "mouse_entered", Callable.From( OnButtonFocused ) );

		AntiAliasingOption = GetNode<OptionButton>( "TabContainer/Video/VBoxContainer/AntiAliasingList/AntiAliasingOptionButton" );
		AntiAliasingOption.Connect( "pressed", Callable.From( OnButtonPressed ) );
		AntiAliasingOption.Connect( "item_selected", Callable.From( ( int index ) => { OnButtonPressed(); } ) );
		AntiAliasingOption.Connect( "mouse_entered", Callable.From( OnButtonFocused ) );

		EffectsOn = GetNode<CheckBox>( "TabContainer/Audio/VBoxContainer/EffectsOnButton/EffectsOnCheckBox" );
		EffectsOn.Connect( "pressed", Callable.From( OnButtonPressed ) );
		EffectsOn.Connect( "mouse_entered", Callable.From( OnButtonFocused ) );

		EffectsVolume = GetNode<HSlider>( "TabContainer/Audio/VBoxContainer/EffectsVolumeSlider/EffectsVolumeHSlider" );
		EffectsVolume.Connect( "changed", Callable.From( OnButtonPressed ) );
		EffectsVolume.Connect( "mouse_entered", Callable.From( OnButtonFocused ) );

		MusicOn = GetNode<CheckBox>( "TabContainer/Audio/VBoxContainer/MusicOnButton/MusicOnCheckBox" );
		MusicOn.Connect( "pressed", Callable.From( OnButtonPressed ) );
		MusicOn.Connect( "mouse_entered", Callable.From( OnButtonFocused ) );

		MusicVolume = GetNode<HSlider>( "TabContainer/Audio/VBoxContainer/MusicVolumeSlider/MusicVolumeHSlider" );
		MusicVolume.Connect( "changed", Callable.From( OnButtonPressed ) );
		MusicVolume.Connect( "mouse_entered", Callable.From( OnButtonFocused ) );

		MuteUnfocused = GetNode<CheckBox>( "TabContainer/Audio/VBoxContainer/MuteOnUnfocusedButton/MuteOnUnfocusedCheckBox" );
		MuteUnfocused.Connect( "pressed", Callable.From( OnButtonPressed ) );
		MuteUnfocused.Connect( "mouse_entered", Callable.From( OnButtonFocused ) );

		HapticEnabled = GetNode<CheckBox>( "TabContainer/Accessibility/VBoxContainer/HapticFeedbackButton/HapticFeedbackCheckbox" );
		HapticEnabled.Connect( "pressed", Callable.From( OnButtonPressed ) );
		HapticEnabled.Connect( "mouse_entered", Callable.From( OnButtonFocused ) );
		
		HapticStrength = GetNode<HSlider>( "TabContainer/Accessibility/VBoxContainer/HapticStrengthSlider/HapticStrengthSlider" );
		HapticStrength.Connect( "changed", Callable.From( OnButtonPressed ) );
		HapticStrength.Connect( "mouse_entered", Callable.From( OnButtonFocused ) );

		AutoAimEnabled = GetNode<CheckBox>( "TabContainer/Accessibility/VBoxContainer/AutoAimButton/AutoAimCheckbox" );
		AutoAimEnabled.Connect( "pressed", Callable.From( OnButtonPressed ) );
		AutoAimEnabled.Connect( "mouse_entered", Callable.From( OnButtonFocused ) );

		DyslexiaMode = GetNode<CheckBox>( "TabContainer/Accessibility/VBoxContainer/DyslexiaModeButton/DyslexiaCheckbox" );
		DyslexiaMode.Connect( "pressed", Callable.From( OnButtonPressed ) );
		DyslexiaMode.Connect( "mouse_entered", Callable.From( OnButtonFocused ) );

		VideoTabBar = GetNode<TabBar>( "TabContainer/Video" );
		VideoTabBar.Connect( "tab_button_pressed", Callable.From( ( int tab ) => { OnButtonPressed(); } ) );

		AudioTabBar = GetNode<TabBar>( "TabContainer/Audio" );
		AudioTabBar.Connect( "tab_button_pressed", Callable.From( ( int tab ) => { OnButtonPressed(); } ) );

		AccessibilityTabBar = GetNode<TabBar>( "TabContainer/Accessibility" );
		AccessibilityTabBar.Connect( "tab_button_pressed", Callable.From( ( int tab ) => { OnButtonPressed(); } ) );

		ControlsTabBar = GetNode<TabBar>( "TabContainer/Controls" );
		ControlsTabBar.Connect( "tab_button_pressed", Callable.From( ( int tab ) => { OnButtonPressed(); } ) );

		VSync.ButtonPressed = SettingsData.GetVSync();
		WindowModeOption.Selected = (int)SettingsData.GetWindowMode();
		AntiAliasingOption.Selected = (int)SettingsData.GetAntiAliasing();

		EffectsOn.ButtonPressed = SettingsData.GetEffectsOn();
		EffectsVolume.Value = SettingsData.GetEffectsVolume();
		MusicOn.ButtonPressed = SettingsData.GetMusicOn();
		MusicVolume.Value = SettingsData.GetMusicVolume();
		MuteUnfocused.ButtonPressed = SettingsData.GetMuteUnfocused();

		HapticEnabled.ButtonPressed = SettingsData.GetHapticEnabled();
		HapticStrength.Value = SettingsData.GetHapticStrength();
		AutoAimEnabled.ButtonPressed = SettingsData.GetAutoAimEnabled();
		DyslexiaMode.ButtonPressed = SettingsData.GetDyslexiaMode();

		ApplyVideoSettings();

		Button SaveSettingsButton = GetNode<Button>( "SaveSettingsButton" );
		SaveSettingsButton.Connect( "focus_entered", Callable.From( OnButtonFocused ) );
		SaveSettingsButton.Connect( "mouse_entered", Callable.From( OnButtonFocused ) );
		SaveSettingsButton.Connect( "pressed", Callable.From( OnSaveSettingsButtonPressed ) );

		UIChannel = GetNode<AudioStreamPlayer>( "../UIChannel" );
	}
	public override void _Process( double delta ) {
		base._Process( delta );
	}
};