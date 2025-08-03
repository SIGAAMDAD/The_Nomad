using Godot;
using Steamworks;

public partial class SettingsMenu : Control {
	private struct SettingsConfig {
		//
		// video options
		//
		public WindowMode WindowMode;
		public DRSPreset DRSPreset;
		public AspectRatio AspectRatio;
		public Resolution Resolution;
		public ShadowQuality ShadowQuality;
		public ShadowFilterQuality ShadowFilterQuality;
		public ParticleQuality ParticleQuality;
		public bool VSyncMode;
		public AntiAliasing AntiAliasing;
		public int MaxFps;
		public int DRSTargetFrames;
		public bool BloomEnabled;
		public bool ShowFPS;
		public bool ShowBlood;

		//
		// accessibility options
		//
		public float HapticStrength;
		public bool HapticEnabled;
		public bool QuicktimeAutocomplete;
		public int ColorblindMode;
		public bool AutoAimEnabled;
		public bool DyslexiaMode;
		public float UIScale;

		//
		// audio options
		//
		public bool EffectsOn;
		public float EffectsVolume;
		public bool MusicOn;
		public float MusicVolume;

		//
		// gameplay options
		//
		public bool EquipWeaponOnPickup;
		public bool HellbreakerEnabled;
		public bool HellbreakerRevanents;
		public bool EnableTutorials;
		public bool ExpertUI;

		//
		// network options
		//
		public bool EnableNetworking;
		public bool FriendsOnlyNetworking;

		public SettingsConfig( DefaultSettings settings ) {	
			WindowMode = settings.WindowMode;
			AspectRatio = settings.AspectRatio;
			Resolution = settings.Resolution;
			ShadowQuality = settings.ShadowQuality;
			ShadowFilterQuality = settings.ShadowFilterQuality;
			ParticleQuality = settings.ParticleQuality;
			VSyncMode = settings.Vsync;
			AntiAliasing = settings.AntiAliasing;
			MaxFps = settings.MaxFps;
			BloomEnabled = settings.BloomEnabled;
			ShowFPS = settings.ShowFps;
			ShowBlood = settings.ShowBlood;

			HapticStrength = settings.HapticStrength;
			HapticEnabled = settings.HapticFeedback;
			QuicktimeAutocomplete = settings.QuicktimeAutocomplete;
			ColorblindMode = settings.ColorblindMode;
			AutoAimEnabled = settings.AutoAim;
			DyslexiaMode = settings.DyslexiaMode;
			EnableTutorials = settings.EnableTutorials;
			ExpertUI = settings.ExpertUI;

			EffectsOn = settings.SoundEffectsOn;
			EffectsVolume = settings.SoundEffectsVolume;
			MusicOn = settings.MusicOn;
			MusicVolume = settings.MusicVolume;

			EquipWeaponOnPickup = settings.EquipWeaponOnPickup;
			HellbreakerEnabled = settings.Hellbreaker;
			HellbreakerRevanents = settings.HellbreakerRevanents;

			EnableNetworking = settings.NetworkingEnabled;
			FriendsOnlyNetworking = settings.FriendsOnly;
		}
	};

	private readonly SettingsConfig Default;
	private SettingsConfig Temp;

	private HBoxContainer VSync;
	private HBoxContainer ImageSharpening;
	private HBoxContainer ResolutionOption;
	private HBoxContainer WindowModeOption;
	private HBoxContainer AntiAliasingOption;
	private HBoxContainer ParticleQuality;
	private HBoxContainer ShadowQuality;
	private HBoxContainer ShadowFilterQuality;
	private HBoxContainer MaxFps;
	private HBoxContainer ShowFPS;
	private HBoxContainer ShowBlood;

	private HBoxContainer EffectsOn;
	private HBoxContainer EffectsVolume;
	private HBoxContainer MusicOn;
	private HBoxContainer MusicVolume;

	private HBoxContainer HapticEnabled;
	private HBoxContainer HapticStrength;
	private HBoxContainer AutoAimEnabled;
	private HBoxContainer DyslexiaMode;

	private HBoxContainer ExpertUI;
	private HBoxContainer EnableTutorials;

	private HBoxContainer NetworkingEnabled;
	private HBoxContainer FriendsOnly;

	private TabContainer TabContainer;
	private TabBar ControlsTabBar;
	private TabBar VideoTabBar;
	private TabBar AudioTabBar;
	private TabBar AccessibilityTabBar;
	private TabBar GameplayTabBar;
	private TabBar NetworkTabBar;

	public enum CategoryTabBar {
		Video,
		Audio,
		Accessibility,
		Gameplay,
		Network,
		Controls
	};

	private void OnSaveSettingsButtonPressed() {
		UIAudioManager.OnButtonPressed();

		SettingsData.SetVSync( VSync.Call( "get_value" ).AsBool() );
		SettingsData.SetShadowQuality( (ShadowQuality)ShadowQuality.Call( "get_value" ).AsInt32() );
		SettingsData.SetShadowFilterQuality( (ShadowFilterQuality)ShadowFilterQuality.Call( "get_value" ).AsInt32() );
		SettingsData.SetWindowMode( (WindowMode)WindowModeOption.Call( "get_value" ).AsInt32() );
		SettingsData.SetResolution( (Resolution)ResolutionOption.Call( "get_value" ).AsInt32() );
		SettingsData.SetAntiAliasing( (AntiAliasing)AntiAliasingOption.Call( "get_value" ).AsInt32() );
		SettingsData.SetShowFPS( ShowFPS.Call( "get_value" ).AsBool() );
		SettingsData.SetShowBlood( ShowBlood.Call( "get_value" ).AsBool() );
		switch ( MaxFps.Call( "get_value" ).AsInt32() ) {
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

		SettingsData.SetEffectsOn( EffectsOn.Call( "get_value" ).AsBool() );
		SettingsData.SetEffectsVolume( EffectsVolume.Call( "get_value" ).AsSingle() );
		SettingsData.SetMusicOn( MusicOn.Call( "get_value" ).AsBool() );
		SettingsData.SetMusicVolume( MusicVolume.Call( "get_value" ).AsSingle() );

		SettingsData.SetHapticEnabled( HapticEnabled.Call( "get_value" ).AsBool() );
		SettingsData.SetHapticStrength( HapticStrength.Call( "get_value" ).AsSingle() );
		SettingsData.SetAutoAimEnabled( AutoAimEnabled.Call( "get_value" ).AsBool() );
		SettingsData.SetDyslexiaMode( DyslexiaMode.Call( "get_value" ).AsBool() );

		SettingsData.SetTutorialsEnabled( EnableTutorials.Call( "get_value" ).AsBool() );
		SettingsData.SetExpertUI( ExpertUI.Call( "get_value" ).AsBool() );

		SettingsData.SetNetworkingEnabled( NetworkingEnabled.Call( "get_value" ).AsBool() );
		SettingsData.SetFriendsOnlyNetworking( FriendsOnly.Call( "get_value" ).AsBool() );

		SettingsData.ApplyVideoSettings();
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

		SetProcess( true );

		VSync = GetNode<HBoxContainer>( "TabContainer/Video/VBoxContainer/VSyncButton" );
		VSync.Connect( HBoxContainer.SignalName.MouseEntered, Callable.From( UIAudioManager.OnButtonFocused ) );

		WindowModeOption = GetNode<HBoxContainer>( "TabContainer/Video/VBoxContainer/WindowModeList" );
		WindowModeOption.Connect( HBoxContainer.SignalName.MouseEntered, Callable.From( UIAudioManager.OnButtonFocused ) );

		ResolutionOption = GetNode<HBoxContainer>( "TabContainer/Video/VBoxContainer/ResolutionList" );
		ResolutionOption.Connect( HBoxContainer.SignalName.MouseEntered, Callable.From( UIAudioManager.OnButtonFocused ) );

		ParticleQuality = GetNode<HBoxContainer>( "TabContainer/Video/VBoxContainer/ParticleQualityList" );
		ParticleQuality.Connect( HBoxContainer.SignalName.MouseEntered, Callable.From( UIAudioManager.OnButtonFocused ) );

		MaxFps = GetNode<HBoxContainer>( "TabContainer/Video/VBoxContainer/MaxFpsList" );
		MaxFps.Connect( HBoxContainer.SignalName.MouseEntered, Callable.From( UIAudioManager.OnButtonFocused ) );

		AntiAliasingOption = GetNode<HBoxContainer>( "TabContainer/Video/VBoxContainer/AntiAliasingList" );
		AntiAliasingOption.Connect( HBoxContainer.SignalName.MouseEntered, Callable.From( UIAudioManager.OnButtonFocused ) );

		ShadowQuality = GetNode<HBoxContainer>( "TabContainer/Video/VBoxContainer/ShadowQualityList" );
		ShadowQuality.Connect( HBoxContainer.SignalName.MouseEntered, Callable.From( UIAudioManager.OnButtonFocused ) );

		ShadowFilterQuality = GetNode<HBoxContainer>( "TabContainer/Video/VBoxContainer/ShadowFilterQualityList" );
		ShadowFilterQuality.Connect( HBoxContainer.SignalName.MouseEntered, Callable.From( UIAudioManager.OnButtonFocused ) );

		ShowFPS = GetNode<HBoxContainer>( "TabContainer/Video/VBoxContainer/ShowFPSButton" );
		ShowFPS.Connect( HBoxContainer.SignalName.MouseEntered, Callable.From( UIAudioManager.OnButtonFocused ) );

		ShowBlood = GetNode<HBoxContainer>( "TabContainer/Video/VBoxContainer/ShowBloodButton" );
		ShowBlood.Connect( HBoxContainer.SignalName.MouseEntered, Callable.From( UIAudioManager.OnButtonFocused ) );

		EffectsOn = GetNode<HBoxContainer>( "TabContainer/Audio/VBoxContainer/EffectsOnButton" );
		EffectsOn.Connect( HBoxContainer.SignalName.MouseEntered, Callable.From( UIAudioManager.OnButtonFocused ) );

		EffectsVolume = GetNode<HBoxContainer>( "TabContainer/Audio/VBoxContainer/EffectsVolumeSlider" );
		EffectsVolume.Connect( HBoxContainer.SignalName.MouseEntered, Callable.From( UIAudioManager.OnButtonFocused ) );

		MusicOn = GetNode<HBoxContainer>( "TabContainer/Audio/VBoxContainer/MusicOnButton" );
		MusicOn.Connect( HBoxContainer.SignalName.MouseEntered, Callable.From( UIAudioManager.OnButtonFocused ) );

		MusicVolume = GetNode<HBoxContainer>( "TabContainer/Audio/VBoxContainer/MusicVolumeSlider" );
		MusicVolume.Connect( HBoxContainer.SignalName.MouseEntered, Callable.From( UIAudioManager.OnButtonFocused ) );

		HapticEnabled = GetNode<HBoxContainer>( "TabContainer/Accessibility/VBoxContainer/HapticFeedbackButton" );
		HapticEnabled.Connect( HBoxContainer.SignalName.MouseEntered, Callable.From( UIAudioManager.OnButtonFocused ) );

		HapticStrength = GetNode<HBoxContainer>( "TabContainer/Accessibility/VBoxContainer/HapticStrengthSlider" );
		HapticStrength.Connect( HBoxContainer.SignalName.MouseEntered, Callable.From( UIAudioManager.OnButtonFocused ) );

		AutoAimEnabled = GetNode<HBoxContainer>( "TabContainer/Accessibility/VBoxContainer/AutoAimButton" );
		AutoAimEnabled.Connect( HBoxContainer.SignalName.MouseEntered, Callable.From( UIAudioManager.OnButtonFocused ) );

		DyslexiaMode = GetNode<HBoxContainer>( "TabContainer/Accessibility/VBoxContainer/DyslexiaModeButton" );
		DyslexiaMode.Connect( HBoxContainer.SignalName.MouseEntered, Callable.From( UIAudioManager.OnButtonFocused ) );

		EnableTutorials = GetNode<HBoxContainer>( "TabContainer/Gameplay/VBoxContainer/EnableTutorialsButton" );
		EnableTutorials.Connect( HBoxContainer.SignalName.MouseEntered, Callable.From( UIAudioManager.OnButtonFocused ) );

		ExpertUI = GetNode<HBoxContainer>( "TabContainer/Gameplay/VBoxContainer/ExpertUIButton" );
		ExpertUI.Connect( HBoxContainer.SignalName.MouseEntered, Callable.From( UIAudioManager.OnButtonFocused ) );

		NetworkingEnabled = GetNode<HBoxContainer>( "TabContainer/Network/VBoxContainer/EnableNetworkingButton" );
		NetworkingEnabled.Connect( HBoxContainer.SignalName.MouseEntered, Callable.From( UIAudioManager.OnButtonFocused ) );

		FriendsOnly = GetNode<HBoxContainer>( "TabContainer/Network/VBoxContainer/FriendsOnlyButton" );
		FriendsOnly.Connect( HBoxContainer.SignalName.MouseEntered, Callable.From( UIAudioManager.OnButtonFocused ) );

		TabContainer = GetNode<TabContainer>( "TabContainer" );
		TabContainer.Connect( TabContainer.SignalName.TabClicked, Callable.From(
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

						GameplayTabBar.SetProcess( false );
						GameplayTabBar.SetProcessInternal( false );
						GameplayTabBar.ProcessMode = ProcessModeEnum.Disabled;

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

						GameplayTabBar.SetProcess( false );
						GameplayTabBar.SetProcessInternal( false );
						GameplayTabBar.ProcessMode = ProcessModeEnum.Disabled;

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

						GameplayTabBar.SetProcess( false );
						GameplayTabBar.SetProcessInternal( false );
						GameplayTabBar.ProcessMode = ProcessModeEnum.Disabled;

						ControlsTabBar.SetProcess( false );
						ControlsTabBar.SetProcessInternal( false );
						ControlsTabBar.ProcessMode = ProcessModeEnum.Disabled;

						NetworkTabBar.SetProcess( false );
						NetworkTabBar.SetProcessInternal( false );
						NetworkTabBar.ProcessMode = ProcessModeEnum.Disabled;
						break;
					case CategoryTabBar.Gameplay:
						VideoTabBar.SetProcess( false );
						VideoTabBar.SetProcessInternal( false );
						VideoTabBar.ProcessMode = ProcessModeEnum.Disabled;

						AudioTabBar.SetProcess( false );
						AudioTabBar.SetProcessInternal( false );
						AudioTabBar.ProcessMode = ProcessModeEnum.Disabled;

						AccessibilityTabBar.SetProcess( false );
						AccessibilityTabBar.SetProcessInternal( false );
						AccessibilityTabBar.ProcessMode = ProcessModeEnum.Disabled;

						GameplayTabBar.SetProcess( true );
						GameplayTabBar.SetProcessInternal( true );
						GameplayTabBar.ProcessMode = ProcessModeEnum.Always;

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

						GameplayTabBar.SetProcess( false );
						GameplayTabBar.SetProcessInternal( false );
						GameplayTabBar.ProcessMode = ProcessModeEnum.Disabled;

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

						GameplayTabBar.SetProcess( false );
						GameplayTabBar.SetProcessInternal( false );
						GameplayTabBar.ProcessMode = ProcessModeEnum.Disabled;

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

		GameplayTabBar = GetNode<TabBar>( "TabContainer/Gameplay" );

		ControlsTabBar = GetNode<TabBar>( "TabContainer/Controls" );
		ControlsTabBar.ProcessMode = ProcessModeEnum.Disabled;

		NetworkTabBar = GetNode<TabBar>( "TabContainer/Network" );
		NetworkTabBar.ProcessMode = ProcessModeEnum.Disabled;

		VSync.Call( "set_value", SettingsData.GetVSync() );
		WindowModeOption.Call( "set_value", (int)SettingsData.GetWindowMode() );
		ResolutionOption.Call( "set_value", (int)SettingsData.GetResolution() );
		AntiAliasingOption.Call( "set_value", (int)SettingsData.GetAntiAliasing() );
		ShadowQuality.Call( "set_value", (int)SettingsData.GetShadowQuality() );
		switch ( SettingsData.GetMaxFps() ) {
		case 0:
			MaxFps.Call( "set_value", 0 );
			break;
		case 30:
			MaxFps.Call( "set_value", 1 );
			break;
		case 45:
			MaxFps.Call( "set_value", 2 );
			break;
		case 60:
			MaxFps.Call( "set_value", 3 );
			break;
		case 90:
			MaxFps.Call( "set_value", 4 );
			break;
		case 125:
			MaxFps.Call( "set_value", 5 );
			break;
		case 225:
			MaxFps.Call( "set_value", 6 );
			break;
		default: // custom, dev setting, or someone's fucking with the .ini file
			Console.PrintLine( "Custom FPS set." );
			break;
		};
		ShadowFilterQuality.Call( "set_value", (int)SettingsData.GetShadowFilterQuality() );
		ShowFPS.Call( "set_value", SettingsData.GetShowFPS() );
		ShowBlood.Call( "set_value", SettingsData.GetShowBlood() );

		EffectsOn.Call( "set_value", SettingsData.GetEffectsOn() );
		EffectsVolume.Call( "set_value", SettingsData.GetEffectsVolume() );
		MusicOn.Call( "set_value", SettingsData.GetMusicOn() );
		MusicVolume.Call( "set_value", SettingsData.GetMusicVolume() );

		HapticEnabled.Call( "set_value", SettingsData.GetHapticEnabled() );
		HapticStrength.Call( "set_value", SettingsData.GetHapticStrength() );
		AutoAimEnabled.Call( "set_value", SettingsData.GetAutoAimEnabled() );
		DyslexiaMode.Call( "set_value", SettingsData.GetDyslexiaMode() );

		EnableTutorials.Call( "set_value", SettingsData.GetTutorialsEnabled() );
		ExpertUI.Call( "set_value", SettingsData.GetExpertUI() );

		NetworkingEnabled.Call( "set_value", SettingsData.GetNetworkingEnabled() );
		FriendsOnly.Call( "set_value", SettingsData.GetFriendsOnlyNetworking() );

		/*
		Button RemoveSteamSyncButton = GetNode<Button>( "RemoveSteamSyncButton" );
		RemoveSteamSyncButton.Connect( "focus_entered", Callable.From( UIAudioManager.OnButtonFocused ) );
		RemoveSteamSyncButton.Connect( HBoxContainer.SignalName.MouseEntered, Callable.From( UIAudioManager.OnButtonFocused ) );
		RemoveSteamSyncButton.Connect( "pressed", Callable.From( OnRemoveSteamSyncButtonPressed ) );

		Button SyncToSteamButton = GetNode<Button>( "SyncToSteamButton" );
		SyncToSteamButton.Connect( "focus_entered", Callable.From( UIAudioManager.OnButtonFocused ) );
		SyncToSteamButton.Connect( HBoxContainer.SignalName.MouseEntered, Callable.From( UIAudioManager.OnButtonFocused ) );
		SyncToSteamButton.Connect( "pressed", Callable.From( OnSyncToSteamButtonPressed ) );
		*/

		Button SaveSettingsButton = GetNode<Button>( "SaveSettingsButton" );
		SaveSettingsButton.Connect( Button.SignalName.FocusEntered, Callable.From( UIAudioManager.OnButtonFocused ) );
		SaveSettingsButton.Connect( Button.SignalName.MouseEntered, Callable.From( UIAudioManager.OnButtonFocused ) );
		SaveSettingsButton.Connect( Button.SignalName.Pressed, Callable.From( OnSaveSettingsButtonPressed ) );
	}
	public override void _UnhandledInput( InputEvent @event ) {
		if ( Input.IsActionJustPressed( "menu_settings_next_category" ) ) {
			switch ( (CategoryTabBar)TabContainer.CurrentTab ) {
			case CategoryTabBar.Video:
				AudioTabBar.Show();
				TabContainer.EmitSignal( TabContainer.SignalName.TabClicked, (int)CategoryTabBar.Audio );
				break;
			case CategoryTabBar.Audio:
				AccessibilityTabBar.Show();
				TabContainer.EmitSignal( TabContainer.SignalName.TabClicked, (int)CategoryTabBar.Accessibility );
				break;
			case CategoryTabBar.Accessibility:
				GameplayTabBar.Show();
				TabContainer.EmitSignal( TabContainer.SignalName.TabClicked, (int)CategoryTabBar.Gameplay );
				break;
			case CategoryTabBar.Gameplay:
				NetworkTabBar.Show();
				TabContainer.EmitSignal( TabContainer.SignalName.TabClicked, (int)CategoryTabBar.Network );
				break;
			case CategoryTabBar.Network:
				ControlsTabBar.Show();
				TabContainer.EmitSignal( TabContainer.SignalName.TabClicked, (int)CategoryTabBar.Controls );
				break;
			case CategoryTabBar.Controls:
				VideoTabBar.Show();
				TabContainer.EmitSignal( TabContainer.SignalName.TabClicked, (int)CategoryTabBar.Video );
				break;
			}
			;
		}
		if ( Input.IsActionJustPressed( "menu_settings_prev_category" ) ) {
			switch ( (CategoryTabBar)TabContainer.CurrentTab ) {
			case CategoryTabBar.Video:
				ControlsTabBar.Show();
				TabContainer.EmitSignal( TabContainer.SignalName.TabClicked, (int)CategoryTabBar.Controls );
				break;
			case CategoryTabBar.Audio:
				VideoTabBar.Show();
				TabContainer.EmitSignal( TabContainer.SignalName.TabClicked, (int)CategoryTabBar.Video );
				break;
			case CategoryTabBar.Accessibility:
				AudioTabBar.Show();
				TabContainer.EmitSignal( TabContainer.SignalName.TabClicked, (int)CategoryTabBar.Audio );
				break;
			case CategoryTabBar.Gameplay:
				AccessibilityTabBar.Show();
				TabContainer.EmitSignal( TabContainer.SignalName.TabClicked, (int)CategoryTabBar.Accessibility );
				break;
			case CategoryTabBar.Network:
				GameplayTabBar.Show();
				TabContainer.EmitSignal( TabContainer.SignalName.TabClicked, (int)CategoryTabBar.Gameplay );
				break;
			case CategoryTabBar.Controls:
				NetworkTabBar.Show();
				TabContainer.EmitSignal( TabContainer.SignalName.TabClicked, (int)CategoryTabBar.Network );
				break;
			};
		}
	}
};