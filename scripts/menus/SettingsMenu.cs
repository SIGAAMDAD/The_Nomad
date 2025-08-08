using Godot;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Utilities;
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
		public LightingQuality LightingQuality;
		public AnimationQuality AnimationQuality;
		public VSyncMode VSyncMode;
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
		public ColorblindMode ColorblindMode;
		public AutoAimMode AutoAimMode;
		public bool DyslexiaMode;
		public float UIScale;
		public bool TextToSpeech;

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
		public HUDPreset HUDPreset;

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
			LightingQuality = settings.LightingQuality;
			ParticleQuality = settings.ParticleQuality;
			AnimationQuality = settings.AnimationQuality;
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
			AutoAimMode = settings.AutoAim;
			DyslexiaMode = settings.DyslexiaMode;
			EnableTutorials = settings.EnableTutorials;
			HUDPreset = settings.HUDPreset;
			TextToSpeech = settings.TextToSpeech;
			UIScale = settings.UIScale;

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

		public readonly override bool Equals( object obj ) => false;
		public readonly override int GetHashCode() => base.GetHashCode();

		// memcmp less painful?
		public static bool operator ==( SettingsConfig a, SettingsConfig b ) {
			if ( a.WindowMode != b.WindowMode ) {
				return false;
			}
			if ( a.VSyncMode != b.VSyncMode ) {
				return false;
			}
			if ( a.AntiAliasing != b.AntiAliasing ) {
				return false;
			}
			if ( a.BloomEnabled != b.BloomEnabled ) {
				return false;
			}
			if ( a.MaxFps != b.MaxFps ) {
				return false;
			}
			if ( a.ShowBlood != b.ShowBlood ) {
				return false;
			}
			if ( a.ShowFPS != b.ShowFPS ) {
				return false;
			}
			if ( a.Resolution != b.Resolution ) {
				return false;
			}
			if ( a.AnimationQuality != b.AnimationQuality ) {
				return false;
			}
			if ( a.ParticleQuality != b.ParticleQuality ) {
				return false;
			}
			if ( a.LightingQuality != b.LightingQuality ) {
				return false;
			}
			if ( a.ShadowQuality != b.ShadowQuality ) {
				return false;
			}
			if ( a.ShadowFilterQuality != b.ShadowFilterQuality ) {
				return false;
			}

			if ( a.EffectsOn != b.EffectsOn ) {
				return false;
			}
			if ( a.EffectsVolume != b.EffectsVolume ) {
				return false;
			}
			if ( a.MusicOn != b.MusicOn ) {
				return false;
			}
			if ( a.MusicVolume != b.MusicVolume ) {
				return false;
			}

			if ( a.HapticEnabled != b.HapticEnabled ) {
				return false;
			}
			if ( a.HapticStrength != b.HapticStrength ) {
				return false;
			}
			if ( a.ColorblindMode != b.ColorblindMode ) {
				return false;
			}
			if ( a.AutoAimMode != b.AutoAimMode ) {
				return false;
			}
			if ( a.DyslexiaMode != b.DyslexiaMode ) {
				return false;
			}
			if ( a.UIScale != b.UIScale ) {
				return false;
			}

			if ( a.EnableTutorials != b.EnableTutorials ) {
				return false;
			}
			if ( a.HUDPreset != b.HUDPreset ) {
				return false;
			}
			return true;
		}
		public static bool operator !=( SettingsConfig a, SettingsConfig b ) {
			return !( a == b );
		}
	};

	private readonly SettingsConfig Default;
	private SettingsConfig Temp;

	private RichTextLabel DescriptionLabel;

	private HBoxContainer VSync;
	private HBoxContainer ImageSharpening;
	private HBoxContainer ResolutionOption;
	private HBoxContainer WindowModeOption;
	private HBoxContainer AntiAliasingOption;
	private HBoxContainer ParticleQuality;
	private HBoxContainer ShadowQuality;
	private HBoxContainer ShadowFilterQuality;
	private HBoxContainer LightingQuality;
	private HBoxContainer AnimationQuality;
	private HBoxContainer MaxFps;
	private HBoxContainer ShowFPS;
	private HBoxContainer ShowBlood;

	private HBoxContainer EffectsOn;
	private HBoxContainer EffectsVolume;
	private HBoxContainer MusicOn;
	private HBoxContainer MusicVolume;

	private HBoxContainer HapticEnabled;
	private HBoxContainer HapticStrength;
	private HBoxContainer AutoAimMode;
	private HBoxContainer DyslexiaMode;
	private HBoxContainer GhostlyGuide;
	private HBoxContainer ColorblindMode;
	private HBoxContainer DisableFlashes;
	private HBoxContainer TextToSpeech;
	private HBoxContainer HUDPreset;
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

	private void OnDefaultSettingsButtonPressed() {
		UIAudioManager.OnButtonPressed();

		
	}
	private void OnSaveSettingsButtonPressed() {
		UIAudioManager.OnButtonPressed();

		SettingsData.SetVSync( (VSyncMode)VSync.Call( "get_value" ).AsInt32() );
		SettingsData.SetShadowQuality( (ShadowQuality)ShadowQuality.Call( "get_value" ).AsInt32() );
		SettingsData.SetShadowFilterQuality( (ShadowFilterQuality)ShadowFilterQuality.Call( "get_value" ).AsInt32() );
		SettingsData.SetLightingQuality( (LightingQuality)LightingQuality.Call( "get_value" ).AsInt32() );
		SettingsData.SetParticleQuality( (ParticleQuality)ParticleQuality.Call( "get_value" ).AsInt32() );
		SettingsData.SetAnimationQuality( (AnimationQuality)AnimationQuality.Call( "get_value" ).AsInt32() );
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
		}
		;

		SettingsData.SetEffectsOn( EffectsOn.Call( "get_value" ).AsBool() );
		SettingsData.SetEffectsVolume( EffectsVolume.Call( "get_value" ).AsSingle() );
		SettingsData.SetMusicOn( MusicOn.Call( "get_value" ).AsBool() );
		SettingsData.SetMusicVolume( MusicVolume.Call( "get_value" ).AsSingle() );

		SettingsData.SetHapticEnabled( HapticEnabled.Call( "get_value" ).AsBool() );
		SettingsData.SetHapticStrength( HapticStrength.Call( "get_value" ).AsSingle() );
		SettingsData.SetAutoAimMode( (AutoAimMode)AutoAimMode.Call( "get_value" ).AsInt32() );
		SettingsData.SetDyslexiaMode( DyslexiaMode.Call( "get_value" ).AsBool() );
		SettingsData.SetTutorialsEnabled( EnableTutorials.Call( "get_value" ).AsBool() );
		SettingsData.SetTextToSpeech( TextToSpeech.Call( "get_value" ).AsBool() );
		SettingsData.SetHUDPreset( (HUDPreset)HUDPreset.Call( "get_value" ).AsInt32() );

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
	private void OnOptionFocused( HBoxContainer option ) {
		DescriptionLabel.ParseBbcode( option.Call( "get_description" ).AsString() );
		UIAudioManager.OnButtonFocused();
	}

	// TODO: make this more controller friendly
	public override void _Ready() {
		base._Ready();

		SetProcess( true );

		DescriptionLabel = GetNode<RichTextLabel>( "TabContainer/Video/MainContainer/Description" );

		VSync = GetNode<HBoxContainer>( "TabContainer/Video/MainContainer/VBoxContainer/VSyncList" );
		VSync.Connect( HBoxContainer.SignalName.FocusEntered, Callable.From( () => OnOptionFocused( VSync ) ) );
		VSync.Connect( HBoxContainer.SignalName.MouseEntered, Callable.From( () => OnOptionFocused( VSync ) ) );

		WindowModeOption = GetNode<HBoxContainer>( "TabContainer/Video/MainContainer/VBoxContainer/WindowModeList" );
		WindowModeOption.Connect( HBoxContainer.SignalName.FocusEntered, Callable.From( () => OnOptionFocused( WindowModeOption ) ) );
		WindowModeOption.Connect( HBoxContainer.SignalName.MouseEntered, Callable.From( () => OnOptionFocused( WindowModeOption ) ) );

		ResolutionOption = GetNode<HBoxContainer>( "TabContainer/Video/MainContainer/VBoxContainer/ResolutionList" );
		ResolutionOption.Connect( HBoxContainer.SignalName.FocusEntered, Callable.From( () => OnOptionFocused( ResolutionOption ) ) );
		ResolutionOption.Connect( HBoxContainer.SignalName.MouseEntered, Callable.From( () => OnOptionFocused( ResolutionOption ) ) );

		AnimationQuality = GetNode<HBoxContainer>( "TabContainer/Video/MainContainer/AdvancedContainer/AnimationQualityList" );
		AnimationQuality.Connect( HBoxContainer.SignalName.FocusEntered, Callable.From( () => OnOptionFocused( AnimationQuality ) ) );
		AnimationQuality.Connect( HBoxContainer.SignalName.MouseEntered, Callable.From( () => OnOptionFocused( AnimationQuality ) ) );

		ParticleQuality = GetNode<HBoxContainer>( "TabContainer/Video/MainContainer/AdvancedContainer/ParticleQualityList" );
		ParticleQuality.Connect( HBoxContainer.SignalName.FocusEntered, Callable.From( () => OnOptionFocused( ParticleQuality ) ) );
		ParticleQuality.Connect( HBoxContainer.SignalName.MouseEntered, Callable.From( () => OnOptionFocused( ParticleQuality ) ) );

		MaxFps = GetNode<HBoxContainer>( "TabContainer/Video/MainContainer/VBoxContainer/MaxFpsList" );
		MaxFps.Connect( HBoxContainer.SignalName.FocusEntered, Callable.From( () => OnOptionFocused( MaxFps ) ) );
		MaxFps.Connect( HBoxContainer.SignalName.MouseEntered, Callable.From( () => OnOptionFocused( MaxFps ) ) );

		AntiAliasingOption = GetNode<HBoxContainer>( "TabContainer/Video/MainContainer/VBoxContainer/AntiAliasingList" );
		AntiAliasingOption.Connect( HBoxContainer.SignalName.FocusEntered, Callable.From( () => OnOptionFocused( AntiAliasingOption ) ) );
		AntiAliasingOption.Connect( HBoxContainer.SignalName.MouseEntered, Callable.From( () => OnOptionFocused( AntiAliasingOption ) ) );

		ShadowQuality = GetNode<HBoxContainer>( "TabContainer/Video/MainContainer/AdvancedContainer/ShadowQualityList" );
		ShadowQuality.Connect( HBoxContainer.SignalName.FocusEntered, Callable.From( () => OnOptionFocused( ShadowQuality ) ) );
		ShadowQuality.Connect( HBoxContainer.SignalName.MouseEntered, Callable.From( () => OnOptionFocused( ShadowQuality ) ) );

		ShadowFilterQuality = GetNode<HBoxContainer>( "TabContainer/Video/MainContainer/AdvancedContainer/ShadowFilterQualityList" );
		ShadowFilterQuality.Connect( HBoxContainer.SignalName.FocusEntered, Callable.From( () => OnOptionFocused( ShadowFilterQuality ) ) );
		ShadowFilterQuality.Connect( HBoxContainer.SignalName.MouseEntered, Callable.From( () => OnOptionFocused( ShadowFilterQuality ) ) );

		LightingQuality = GetNode<HBoxContainer>( "TabContainer/Video/MainContainer/AdvancedContainer/LightingQualityList" );
		LightingQuality.Connect( HBoxContainer.SignalName.FocusEntered, Callable.From( () => OnOptionFocused( LightingQuality ) ) );
		LightingQuality.Connect( HBoxContainer.SignalName.MouseEntered, Callable.From( () => OnOptionFocused( LightingQuality ) ) );

		ShowFPS = GetNode<HBoxContainer>( "TabContainer/Video/MainContainer/VBoxContainer/ShowFPSButton" );
		ShowFPS.Connect( HBoxContainer.SignalName.FocusEntered, Callable.From( () => OnOptionFocused( ShowFPS ) ) );
		ShowFPS.Connect( HBoxContainer.SignalName.MouseEntered, Callable.From( () => OnOptionFocused( ShowFPS ) ) );

		ShowBlood = GetNode<HBoxContainer>( "TabContainer/Video/MainContainer/VBoxContainer/ShowBloodButton" );
		ShowBlood.Connect( HBoxContainer.SignalName.FocusEntered, Callable.From( () => OnOptionFocused( ShowBlood ) ) );
		ShowBlood.Connect( HBoxContainer.SignalName.MouseEntered, Callable.From( () => OnOptionFocused( ShowBlood ) ) );

		EffectsOn = GetNode<HBoxContainer>( "TabContainer/Audio/VBoxContainer/EffectsOnButton" );
		EffectsOn.Connect( HBoxContainer.SignalName.FocusEntered, Callable.From( () => OnOptionFocused( EffectsOn ) ) );
		EffectsOn.Connect( HBoxContainer.SignalName.MouseEntered, Callable.From( () => OnOptionFocused( EffectsOn ) ) );

		EffectsVolume = GetNode<HBoxContainer>( "TabContainer/Audio/VBoxContainer/EffectsVolumeSlider" );
		EffectsVolume.Connect( HBoxContainer.SignalName.FocusEntered, Callable.From( () => OnOptionFocused( EffectsVolume ) ) );
		EffectsVolume.Connect( HBoxContainer.SignalName.MouseEntered, Callable.From( () => OnOptionFocused( EffectsVolume ) ) );

		MusicOn = GetNode<HBoxContainer>( "TabContainer/Audio/VBoxContainer/MusicOnButton" );
		MusicOn.Connect( HBoxContainer.SignalName.FocusEntered, Callable.From( () => OnOptionFocused( MusicOn ) ) );
		MusicOn.Connect( HBoxContainer.SignalName.MouseEntered, Callable.From( () => OnOptionFocused( MusicOn ) ) );

		MusicVolume = GetNode<HBoxContainer>( "TabContainer/Audio/VBoxContainer/MusicVolumeSlider" );
		MusicVolume.Connect( HBoxContainer.SignalName.FocusEntered, Callable.From( () => OnOptionFocused( MusicVolume ) ) );
		MusicVolume.Connect( HBoxContainer.SignalName.MouseEntered, Callable.From( () => OnOptionFocused( MusicVolume ) ) );

		HapticEnabled = GetNode<HBoxContainer>( "TabContainer/Accessibility/VBoxContainer/HapticFeedbackButton" );
		HapticEnabled.Connect( HBoxContainer.SignalName.FocusEntered, Callable.From( () => OnOptionFocused( HapticEnabled ) ) );
		HapticEnabled.Connect( HBoxContainer.SignalName.MouseEntered, Callable.From( () => OnOptionFocused( HapticEnabled ) ) );

		HapticStrength = GetNode<HBoxContainer>( "TabContainer/Accessibility/VBoxContainer/HapticStrengthSlider" );
		HapticStrength.Connect( HBoxContainer.SignalName.FocusEntered, Callable.From( () => OnOptionFocused( HapticStrength ) ) );
		HapticStrength.Connect( HBoxContainer.SignalName.MouseEntered, Callable.From( () => OnOptionFocused( HapticStrength ) ) );

		AutoAimMode = GetNode<HBoxContainer>( "TabContainer/Accessibility/VBoxContainer/AutoAimList" );
		AutoAimMode.Connect( HBoxContainer.SignalName.FocusEntered, Callable.From( () => OnOptionFocused( AutoAimMode ) ) );
		AutoAimMode.Connect( HBoxContainer.SignalName.MouseEntered, Callable.From( () => OnOptionFocused( AutoAimMode ) ) );

		ColorblindMode = GetNode<HBoxContainer>( "TabContainer/Accessibility/VBoxContainer/ColorblindModeList" );
		ColorblindMode.Connect( HBoxContainer.SignalName.FocusEntered, Callable.From( () => OnOptionFocused( ColorblindMode ) ) );
		ColorblindMode.Connect( HBoxContainer.SignalName.MouseEntered, Callable.From( () => OnOptionFocused( ColorblindMode ) ) );

		DyslexiaMode = GetNode<HBoxContainer>( "TabContainer/Accessibility/VBoxContainer/DyslexiaModeButton" );
		DyslexiaMode.Connect( HBoxContainer.SignalName.FocusEntered, Callable.From( () => OnOptionFocused( DyslexiaMode ) ) );
		DyslexiaMode.Connect( HBoxContainer.SignalName.MouseEntered, Callable.From( () => OnOptionFocused( DyslexiaMode ) ) );

		TextToSpeech = GetNode<HBoxContainer>( "TabContainer/Accessibility/VBoxContainer/TextToSpeechButton" );
		TextToSpeech.Connect( HBoxContainer.SignalName.FocusEntered, Callable.From( () => OnOptionFocused( TextToSpeech ) ) );
		TextToSpeech.Connect( HBoxContainer.SignalName.MouseEntered, Callable.From( () => OnOptionFocused( TextToSpeech ) ) );

		EnableTutorials = GetNode<HBoxContainer>( "TabContainer/Gameplay/VBoxContainer/EnableTutorialsButton" );
		EnableTutorials.Connect( HBoxContainer.SignalName.FocusEntered, Callable.From( () => OnOptionFocused( EnableTutorials ) ) );
		EnableTutorials.Connect( HBoxContainer.SignalName.MouseEntered, Callable.From( () => OnOptionFocused( EnableTutorials ) ) );

		HUDPreset = GetNode<HBoxContainer>( "TabContainer/Gameplay/VBoxContainer/HUDPresetList" );
		HUDPreset.Connect( HBoxContainer.SignalName.FocusEntered, Callable.From( () => OnOptionFocused( HUDPreset ) ) );
		HUDPreset.Connect( HBoxContainer.SignalName.MouseEntered, Callable.From( () => OnOptionFocused( HUDPreset ) ) );

		NetworkingEnabled = GetNode<HBoxContainer>( "TabContainer/Network/VBoxContainer/EnableNetworkingButton" );
		NetworkingEnabled.Connect( HBoxContainer.SignalName.FocusEntered, Callable.From( () => OnOptionFocused( NetworkingEnabled ) ) );
		NetworkingEnabled.Connect( HBoxContainer.SignalName.MouseEntered, Callable.From( () => OnOptionFocused( NetworkingEnabled ) ) );

		FriendsOnly = GetNode<HBoxContainer>( "TabContainer/Network/VBoxContainer/FriendsOnlyButton" );
		FriendsOnly.Connect( HBoxContainer.SignalName.FocusEntered, Callable.From( () => OnOptionFocused( FriendsOnly ) ) );
		FriendsOnly.Connect( HBoxContainer.SignalName.MouseEntered, Callable.From( () => OnOptionFocused( FriendsOnly ) ) );

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

						DescriptionLabel = GetNode<RichTextLabel>( "TabContainer/Video/MainContainer/Description" );
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

						DescriptionLabel = GetNode<RichTextLabel>( "TabContainer/Audio/VBoxContainer/Description" );
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

						DescriptionLabel = GetNode<RichTextLabel>( "TabContainer/Accessibility/VBoxContainer/Description" );
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

						DescriptionLabel = GetNode<RichTextLabel>( "TabContainer/Gameplay/VBoxContainer/Description" );
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

						DescriptionLabel = GetNode<RichTextLabel>( "TabContainer/Network/VBoxContainer/Description" );
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

		GameplayTabBar = GetNode<TabBar>( "TabContainer/Gameplay" );
		GameplayTabBar.ProcessMode = ProcessModeEnum.Disabled;

		ControlsTabBar = GetNode<TabBar>( "TabContainer/Controls" );
		ControlsTabBar.ProcessMode = ProcessModeEnum.Disabled;

		NetworkTabBar = GetNode<TabBar>( "TabContainer/Network" );
		NetworkTabBar.ProcessMode = ProcessModeEnum.Disabled;

		VSync.Call( "set_value", (int)SettingsData.GetVSync() );
		WindowModeOption.Call( "set_value", (int)SettingsData.GetWindowMode() );
		ResolutionOption.Call( "set_value", (int)SettingsData.GetResolution() );
		AntiAliasingOption.Call( "set_value", (int)SettingsData.GetAntiAliasing() );
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
		default: // custom, dev setting, or someone's messing with the .ini file
			Console.PrintLine( "Custom FPS set." );
			break;
		};
		ShadowQuality.Call( "set_value", (int)SettingsData.GetShadowQuality() );
		ShadowFilterQuality.Call( "set_value", (int)SettingsData.GetShadowFilterQuality() );
		AnimationQuality.Call( "set_value", (int)SettingsData.GetAnimationQuality() );
		ParticleQuality.Call( "set_value", (int)SettingsData.GetParticleQuality() );
		LightingQuality.Call( "set_value", (int)SettingsData.GetLightingQuality() );
		ShowFPS.Call( "set_value", SettingsData.GetShowFPS() );
		ShowBlood.Call( "set_value", SettingsData.GetShowBlood() );

		EffectsOn.Call( "set_value", SettingsData.GetEffectsOn() );
		EffectsVolume.Call( "set_value", SettingsData.GetEffectsVolume() );
		MusicOn.Call( "set_value", SettingsData.GetMusicOn() );
		MusicVolume.Call( "set_value", SettingsData.GetMusicVolume() );

		HapticEnabled.Call( "set_value", SettingsData.GetHapticEnabled() );
		HapticStrength.Call( "set_value", SettingsData.GetHapticStrength() );
		ColorblindMode.Call( "set_value", (int)SettingsData.GetColorblindMode() );
		AutoAimMode.Call( "set_value", (int)SettingsData.GetAutoAimMode() );
		DyslexiaMode.Call( "set_value", SettingsData.GetDyslexiaMode() );
		TextToSpeech.Call( "set_value", SettingsData.GetTextToSpeech() );
		EnableTutorials.Call( "set_value", SettingsData.GetTutorialsEnabled() );
		HUDPreset.Call( "set_value", (int)SettingsData.GetHUDPreset() );

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

		Button DefaultSettingsButton = GetNode<Button>( "DefaultSettingsButton" );
		DefaultSettingsButton.Connect( Button.SignalName.FocusEntered, Callable.From( UIAudioManager.OnButtonFocused ) );
		DefaultSettingsButton.Connect( Button.SignalName.FocusEntered, Callable.From( UIAudioManager.OnButtonFocused ) );
		DefaultSettingsButton.Connect( Button.SignalName.Pressed, Callable.From( OnDefaultSettingsButtonPressed ) );

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