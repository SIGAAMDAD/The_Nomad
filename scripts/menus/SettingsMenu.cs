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
using Steamworks;
using System;
using Utils;

namespace Menus {
	/*
	===================================================================================

	SettingsMenu

	===================================================================================
	*/

	public partial class SettingsMenu : Control {
		public enum CategoryTabBar : uint {
			Video,
			Audio,
			Accessibility,
			Gameplay,
			Network,
			Controls
		};

		private static readonly StringName @ValueChangedSignalName = "value_changed";
		private static readonly StringName @FocusNodeMetaName = "focus_node";
		private static readonly StringName @DescriptionNodeMetaName = "description_node";

		private TabBar[] TabBars;

		private Settings.SettingsConfig Default;
		private Settings.SettingsConfig Temp;

		private RichTextLabel DescriptionLabel;

		private SelectionNodes.OptionList VSync;
		private SelectionNodes.OptionList ImageSharpening;
		private SelectionNodes.OptionList ResolutionOption;
		private SelectionNodes.OptionList WindowModeOption;
		private SelectionNodes.OptionList AntiAliasingOption;
		private SelectionNodes.OptionList ParticleQuality;
		private SelectionNodes.OptionList ShadowQuality;
		private SelectionNodes.OptionList ShadowFilterQuality;
		private SelectionNodes.OptionList LightingQuality;
		private SelectionNodes.OptionList AnimationQuality;
		private SelectionNodes.OptionList MaxFps;
		private SelectionNodes.OptionList PerformanceOverlay;
		private SelectionNodes.OptionCheckbox ShowBlood;

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
		private HBoxContainer CODLobbies;
		private HBoxContainer BountyHuntEnabled;

		private TabContainer TabContainer;
		private TabBar ControlsTabBar;
		private TabBar VideoTabBar;
		private TabBar AudioTabBar;
		private TabBar AccessibilityTabBar;
		private TabBar GameplayTabBar;
		private TabBar NetworkTabBar;

		[Signal]
		public delegate void ExitMenuEventHandler();

		/*
		===============
		OnDefaultSettingsButtonPressed
		===============
		*/
		private void OnDefaultSettingsButtonPressed() {
			UIAudioManager.OnButtonPressed();

			VSync.Call( "set_value", (int)Default.VSyncMode );
			WindowModeOption.Call( "set_value", (int)Default.WindowMode );
			ResolutionOption.Call( "set_value", (int)Default.Resolution );
			AntiAliasingOption.Call( "set_value", (int)Default.AntiAliasing );
			switch ( Default.MaxFps ) {
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
			}
			ShadowQuality.Call( "set_value", (int)Default.ShadowQuality );
			ShadowFilterQuality.Call( "set_value", (int)Default.ShadowFilterQuality );
			AnimationQuality.Call( "set_value", (int)Default.AnimationQuality );
			ParticleQuality.Call( "set_value", (int)Default.ParticleQuality );
			LightingQuality.Call( "set_value", (int)Default.LightingQuality );
			PerformanceOverlay.Call( "set_value", (int)Default.PerformanceOverlay );
			ShowBlood.Call( "set_value", Default.ShowBlood );

			EffectsOn.Call( "set_value", Default.EffectsOn );
			EffectsVolume.Call( "set_value", Default.EffectsVolume );
			MusicOn.Call( "set_value", Default.MusicOn );
			MusicVolume.Call( "set_value", Default.MusicVolume );

			HapticEnabled.Call( "set_value", Default.HapticEnabled );
			HapticStrength.Call( "set_value", Default.HapticStrength );
			ColorblindMode.Call( "set_value", (int)Default.ColorblindMode );
			AutoAimMode.Call( "set_value", (int)Default.AutoAimMode );
			DyslexiaMode.Call( "set_value", Default.DyslexiaMode );
			TextToSpeech.Call( "set_value", Default.TextToSpeech );
			EnableTutorials.Call( "set_value", Default.EnableTutorials );
			HUDPreset.Call( "set_value", (int)Default.HUDPreset );

			NetworkingEnabled.Call( "set_value", Default.EnableNetworking );
			BountyHuntEnabled.Call( "set_value", Default.BountyHuntEnabled );
			CODLobbies.Call( "set_value", Default.CODLobbies );
		}

		/*
		===============
		OnSaveSettingsButtonPressed
		===============
		*/
		private void OnSaveSettingsButtonPressed() {
			UIAudioManager.OnButtonPressed();

			SettingsData.SetVSync( (VSyncMode)VSync.Call( "get_value" ).AsInt32() );
			SettingsData.SetShadowQuality( (ShadowQuality)ShadowQuality.Call( "get_value" ).AsInt32() );
			SettingsData.SetShadowFilterQuality( (ShadowFilterQuality)ShadowFilterQuality.Call( "get_value" ).AsInt32() );
			SettingsData.SetLightingQuality( (LightingQuality)LightingQuality.Call( "get_value" ).AsInt32() );
			SettingsData.SetParticleQuality( (ParticleQuality)ParticleQuality.Call( "get_value" ).AsInt32() );
			SettingsData.SetAnimationQuality( (AnimationQuality)AnimationQuality.Call( "get_value" ).AsInt32() );
			SettingsData.SetWindowMode( (WindowMode)WindowModeOption.Call( "get_value" ).AsInt32() );
			SettingsData.SetResolution( (WindowResolution)ResolutionOption.Call( "get_value" ).AsInt32() );
			SettingsData.SetAntiAliasing( (AntiAliasing)AntiAliasingOption.Call( "get_value" ).AsInt32() );
			SettingsData.SetPerformanceOverlay( (PerformanceOverlayPreset)PerformanceOverlay.Call( "get_value" ).AsInt32() );
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
			SettingsData.SetCODLobbyEnabled( CODLobbies.Call( "get_value" ).AsBool() );
			SettingsData.SetBountyHuntEnabled( BountyHuntEnabled.Call( "get_value" ).AsBool() );

			SettingsData.ApplyVideoSettings();
			SettingsData.Save();
		}

		/*
		===============
		OnSyncToSteamButtonPressed
		===============
		*/
		private void OnSyncToSteamButtonPressed() {
			Console.PrintLine( "Synchronizing settings to SteamCloud..." );
			Steam.SteamManager.SaveCloudFile( "user://settings.ini" );
		}

		/*
		===============
		OnRemoveSteamSyncButtonPressed
		===============
		*/
		private void OnRemoveSteamSyncButtonPressed() {
			SteamRemoteStorage.FileDelete( "settings.ini" );
		}

		/*
		===============
		OnOptionFocused
		===============
		*/
		private void OnOptionFocused( HBoxContainer option ) {
			DescriptionLabel.ParseBbcode( option.Call( "get_description" ).AsString() );
			UIAudioManager.OnButtonFocused( option.GetNode<Control>( "Title" ) );
		}

		/*
		===============
		OnOptionMouseFocused
		===============
		*/
		/// <summary>
		/// Here as a wrapper to prevent focus and mouse proximity from having two strobing options
		/// </summary>
		/// <param name="option"></param>
		private void OnOptionMouseFocused( HBoxContainer option ) {
			Control focusOwner = GetViewport().GuiGetFocusOwner();
			if ( focusOwner != null ) {
				OnOptionUnfocused( focusOwner as HBoxContainer );
			}
			OnOptionFocused( option );
		}

		/*
		===============
		OnOptionUnfocused
		===============
		*/
		private void OnOptionUnfocused( HBoxContainer option ) {
			UIAudioManager.OnButtonUnfocused( option.GetNode<Control>( "Title" ) );
		}


		/*
		===============
		ConnectButton
		===============
		*/
		private void ConnectButton<[MustBeVariant] T>( HBoxContainer button, Action<T> valueChangedCallback ) {
			GameEventBus.ConnectSignal( button, HBoxContainer.SignalName.FocusEntered, this, Callable.From( () => OnOptionFocused( button ) ) );
			GameEventBus.ConnectSignal( button, HBoxContainer.SignalName.MouseEntered, this, Callable.From( () => OnOptionMouseFocused( button ) ) );
			GameEventBus.ConnectSignal( button, HBoxContainer.SignalName.FocusExited, this, Callable.From( () => OnOptionUnfocused( button ) ) );
			GameEventBus.ConnectSignal( button, HBoxContainer.SignalName.MouseExited, this, Callable.From( () => OnOptionUnfocused( button ) ) );
			GameEventBus.ConnectSignal( button, ValueChangedSignalName, this, Callable.From<T>( ( value ) => valueChangedCallback( value ) ) );
		}

		private void SetActiveTab( int tabIndex, in TabBar tab ) {
			DescriptionLabel = (RichTextLabel)tab.GetMeta( DescriptionNodeMetaName );
			( (Control)tab.GetMeta( FocusNodeMetaName ) ).GrabFocus();
			for ( int i = 0; i < TabBars.Length; i++ ) {
				TabBars[ i ].ProcessMode = i == tabIndex ? ProcessModeEnum.Always : ProcessModeEnum.Disabled;
			}
		}

		private void BindNodes() {
			DescriptionLabel = GetNode<RichTextLabel>( "TabContainer/Video/MainContainer/Description" );

			VSync = GetNode<SelectionNodes.OptionList>( "TabContainer/Video/MainContainer/VBoxContainer/VSyncList" );
			VSync.ValueChanged += ( value ) => Temp.VSyncMode = (VSyncMode)value;

			WindowModeOption = GetNode<SelectionNodes.OptionList>( "TabContainer/Video/MainContainer/VBoxContainer/WindowModeList" );
			WindowModeOption.ValueChanged += ( value ) => Temp.WindowMode = (WindowMode)value;

			ResolutionOption = GetNode<SelectionNodes.OptionList>( "TabContainer/Video/MainContainer/VBoxContainer/ResolutionList" );
			ResolutionOption.ValueChanged += ( value ) => Temp.Resolution = (WindowResolution)value;

			AnimationQuality = GetNode<SelectionNodes.OptionList>( "TabContainer/Video/MainContainer/AdvancedContainer/AnimationQualityList" );
			AnimationQuality.ValueChanged += ( value ) => Temp.AnimationQuality = (AnimationQuality)value;

			ParticleQuality = GetNode<SelectionNodes.OptionList>( "TabContainer/Video/MainContainer/AdvancedContainer/ParticleQualityList" );
			ParticleQuality.ValueChanged += ( value ) => Temp.ParticleQuality = (ParticleQuality)value;

			MaxFps = GetNode<SelectionNodes.OptionList>( "TabContainer/Video/MainContainer/VBoxContainer/MaxFpsList" );
			MaxFps.ValueChanged += ( value ) => Temp.MaxFps = value;

			AntiAliasingOption = GetNode<SelectionNodes.OptionList>( "TabContainer/Video/MainContainer/VBoxContainer/AntiAliasingList" );
			AntiAliasingOption.ValueChanged += ( value ) => Temp.AntiAliasing = (AntiAliasing)value;

			ShadowQuality = GetNode<SelectionNodes.OptionList>( "TabContainer/Video/MainContainer/AdvancedContainer/ShadowQualityList" );
			ShadowQuality.ValueChanged += ( value ) => Temp.ShadowQuality = (ShadowQuality)value;

			ShadowFilterQuality = GetNode<SelectionNodes.OptionList>( "TabContainer/Video/MainContainer/AdvancedContainer/ShadowFilterQualityList" );
			ShadowFilterQuality.ValueChanged += ( value ) => Temp.ShadowFilterQuality = (ShadowFilterQuality)value;

			LightingQuality = GetNode<SelectionNodes.OptionList>( "TabContainer/Video/MainContainer/AdvancedContainer/LightingQualityList" );
			LightingQuality.ValueChanged += ( value ) => Temp.LightingQuality = (LightingQuality)value;

			PerformanceOverlay = GetNode<SelectionNodes.OptionList>( "TabContainer/Video/MainContainer/VBoxContainer/PerformanceOverlayList" );
			PerformanceOverlay.ValueChanged += ( value ) => Temp.PerformanceOverlay = (PerformanceOverlayPreset)value;

			ShowBlood = GetNode<SelectionNodes.OptionCheckbox>( "TabContainer/Video/MainContainer/VBoxContainer/ShowBloodButton" );
			ShowBlood.ValueChanged += ( value ) => Temp.ShowBlood = value;

			EffectsOn = GetNode<HBoxContainer>( "TabContainer/Audio/VBoxContainer/EffectsOnButton" );
			ConnectButton<bool>( EffectsOn, ( value ) => Temp.EffectsOn = value );

			EffectsVolume = GetNode<HBoxContainer>( "TabContainer/Audio/VBoxContainer/EffectsVolumeSlider" );
			ConnectButton<float>( EffectsVolume, ( value ) => Temp.EffectsVolume = value );

			MusicOn = GetNode<HBoxContainer>( "TabContainer/Audio/VBoxContainer/MusicOnButton" );
			ConnectButton<bool>( MusicOn, ( value ) => Temp.MusicOn = value );

			MusicVolume = GetNode<HBoxContainer>( "TabContainer/Audio/VBoxContainer/MusicVolumeSlider" );
			ConnectButton<float>( MusicVolume, ( value ) => Temp.MusicVolume = value );

			HapticEnabled = GetNode<HBoxContainer>( "TabContainer/Accessibility/VBoxContainer/HapticFeedbackButton" );
			ConnectButton<bool>( HapticEnabled, ( value ) => Temp.HapticEnabled = value );

			HapticStrength = GetNode<HBoxContainer>( "TabContainer/Accessibility/VBoxContainer/HapticStrengthSlider" );
			ConnectButton<float>( HapticStrength, ( value ) => Temp.HapticStrength = value );

			AutoAimMode = GetNode<HBoxContainer>( "TabContainer/Accessibility/VBoxContainer/AutoAimList" );
			ConnectButton<int>( AutoAimMode, ( value ) => Temp.AutoAimMode = (AutoAimMode)value );

			ColorblindMode = GetNode<HBoxContainer>( "TabContainer/Accessibility/VBoxContainer/ColorblindModeList" );
			ConnectButton<int>( ColorblindMode, ( value ) => Temp.ColorblindMode = (ColorblindMode)value );

			DyslexiaMode = GetNode<HBoxContainer>( "TabContainer/Accessibility/VBoxContainer/DyslexiaModeButton" );
			ConnectButton<bool>( DyslexiaMode, ( value ) => Temp.DyslexiaMode = value );

			TextToSpeech = GetNode<HBoxContainer>( "TabContainer/Accessibility/VBoxContainer/TextToSpeechButton" );
			ConnectButton<bool>( TextToSpeech, ( value ) => Temp.TextToSpeech = value );

			EnableTutorials = GetNode<HBoxContainer>( "TabContainer/Gameplay/VBoxContainer/EnableTutorialsButton" );
			ConnectButton<bool>( EnableTutorials, ( value ) => Temp.EnableTutorials = value );

			HUDPreset = GetNode<HBoxContainer>( "TabContainer/Gameplay/VBoxContainer/HUDPresetList" );
			ConnectButton<int>( HUDPreset, ( value ) => Temp.HUDPreset = (HUDPreset)value );

			NetworkingEnabled = GetNode<HBoxContainer>( "TabContainer/Network/VBoxContainer/EnableNetworkingButton" );
			ConnectButton<bool>( NetworkingEnabled, ( value ) => Temp.EnableNetworking = value );

			BountyHuntEnabled = GetNode<HBoxContainer>( "TabContainer/Network/VBoxContainer/BountyHuntEnabled" );
			ConnectButton<bool>( BountyHuntEnabled, ( value ) => Temp.BountyHuntEnabled = value );

			CODLobbies = GetNode<HBoxContainer>( "TabContainer/Network/VBoxContainer/CODLobbies" );
			ConnectButton<bool>( CODLobbies, ( value ) => Temp.CODLobbies = value );

			TabContainer = GetNode<TabContainer>( "TabContainer" );
			GameEventBus.ConnectSignal( TabContainer, TabContainer.SignalName.TabClicked, this, Callable.From(
					( int tab ) => {
						UIAudioManager.OnButtonPressed();
						SetActiveTab( tab, in TabBars[ tab ] );
					}
				)
			);

			VideoTabBar = GetNode<TabBar>( "TabContainer/Video" );
			VideoTabBar.SetMeta( DescriptionNodeMetaName, GetNode( "TabContainer/Video/MainContainer/Description" ) );
			VideoTabBar.SetMeta( FocusNodeMetaName, VSync );

			AudioTabBar = GetNode<TabBar>( "TabContainer/Audio" );
			AudioTabBar.SetMeta( DescriptionNodeMetaName, GetNode( "TabContainer/Audio/VBoxContainer/Description" ) );
			AudioTabBar.SetMeta( FocusNodeMetaName, EffectsOn );
			AudioTabBar.ProcessMode = ProcessModeEnum.Disabled;

			AccessibilityTabBar = GetNode<TabBar>( "TabContainer/Accessibility" );
			AccessibilityTabBar.SetMeta( DescriptionNodeMetaName, GetNode( "TabContainer/Accessibility/VBoxContainer/Description" ) );
			AccessibilityTabBar.SetMeta( FocusNodeMetaName, HapticEnabled );
			AccessibilityTabBar.ProcessMode = ProcessModeEnum.Disabled;

			GameplayTabBar = GetNode<TabBar>( "TabContainer/Gameplay" );
			GameplayTabBar.SetMeta( DescriptionNodeMetaName, GetNode( "TabContainer/Gameplay/VBoxContainer/Description" ) );
			GameplayTabBar.SetMeta( FocusNodeMetaName, EnableTutorials );
			GameplayTabBar.ProcessMode = ProcessModeEnum.Disabled;

			ControlsTabBar = GetNode<TabBar>( "TabContainer/Controls" );
			ControlsTabBar.SetMeta( DescriptionNodeMetaName, GetNode( "TabContainer/Controls/VBoxContainer/Description" ) );
			ControlsTabBar.SetMeta( FocusNodeMetaName, GetNode( "TabContainer/Controls/VBoxContainer/MarginContainer/HBoxContainer/MovementContainer/MoveNorthBind" ) );
			ControlsTabBar.ProcessMode = ProcessModeEnum.Disabled;

			NetworkTabBar = GetNode<TabBar>( "TabContainer/Network" );
			NetworkTabBar.SetMeta( DescriptionNodeMetaName, GetNode( "TabContainer/Network/VBoxContainer/Description" ) );
			NetworkTabBar.SetMeta( FocusNodeMetaName, NetworkingEnabled );
			NetworkTabBar.ProcessMode = ProcessModeEnum.Disabled;
		}

		/*
		===============
		_Ready
		===============
		*/
		/// <summary>
		/// godot initialization override
		/// </summary>
		/// <remarks>
		/// TODO: make this more controller friendly
		/// </remarks>
		public override void _Ready() {
			base._Ready();

			SetProcess( true );

			TextureRect leftIcon = GetNode<TextureRect>( "LeftIcon" );
			leftIcon.Texture = AccessibilityManager.GetPrevMenuButtonTexture();

			TextureRect rightIcon = GetNode<TextureRect>( "RightIcon" );
			rightIcon.Texture = AccessibilityManager.GetNextMenuButtonTexture();

			TextureRect saveSettingsIcon = GetNode<TextureRect>( "SaveSettingsIcon" );
			saveSettingsIcon.Texture = AccessibilityManager.GetSaveSettingsButtonTexture();

			TextureRect resetSettingsIcon = GetNode<TextureRect>( "ResetSettingsIcon" );
			resetSettingsIcon.Texture = AccessibilityManager.GetResetSettingsButtonTexture();

			GameEventBus.ConnectSignal( GetNode<Button>( "ExitButton" ), Button.SignalName.Pressed, this, Callable.From( EmitSignalExitMenu ) );
			GetNode<TextureRect>( "ExitIcon" ).Texture = AccessibilityManager.GetExitMenuButtonTexture();

			BindNodes();

			TabBars = [
				VideoTabBar,
				AudioTabBar,
				AccessibilityTabBar,
				GameplayTabBar,
				ControlsTabBar,
				NetworkTabBar
			];

			VSync.SetValue( (int)SettingsData.VSyncMode );
			WindowModeOption.SetValue( (int)SettingsData.WindowMode );
			ResolutionOption.SetValue( (int)SettingsData.Resolution );
			AntiAliasingOption.SetValue( (int)SettingsData.AntiAliasing );
			switch ( SettingsData.MaxFps ) {
				case 0:
					MaxFps.SetValue( 0 );
					break;
				case 30:
					MaxFps.SetValue( 1 );
					break;
				case 45:
					MaxFps.SetValue( 2 );
					break;
				case 60:
					MaxFps.SetValue( 3 );
					break;
				case 90:
					MaxFps.SetValue( 4 );
					break;
				case 125:
					MaxFps.SetValue( 5 );
					break;
				case 225:
					MaxFps.SetValue( 6 );
					break;
				default: // custom, dev setting, or someone's messing with the .ini file
					Console.PrintLine( "Custom FPS set." );
					break;
			}
			ShadowQuality.SetValue( (int)SettingsData.ShadowQuality );
			ShadowFilterQuality.SetValue( (int)SettingsData.ShadowFilterQuality );
			AnimationQuality.SetValue( (int)SettingsData.AnimationQuality );
			ParticleQuality.SetValue( (int)SettingsData.ParticleQuality );
			LightingQuality.SetValue( (int)SettingsData.LightingQuality );
			PerformanceOverlay.SetValue( (int)SettingsData.PerformanceOverlay );
			ShowBlood.SetValue( SettingsData.ShowBlood );

			EffectsOn.Call( "set_value", SettingsData.EffectsOn );
			EffectsVolume.Call( "set_value", SettingsData.EffectsVolume );
			MusicOn.Call( "set_value", SettingsData.MusicOn );
			MusicVolume.Call( "set_value", SettingsData.MusicVolume );

			HapticEnabled.Call( "set_value", SettingsData.HapticEnabled );
			HapticStrength.Call( "set_value", SettingsData.HapticStrength );
			ColorblindMode.Call( "set_value", (int)SettingsData.ColorblindMode );
			AutoAimMode.Call( "set_value", (int)SettingsData.AutoAimMode );
			DyslexiaMode.Call( "set_value", SettingsData.DyslexiaMode );
			TextToSpeech.Call( "set_value", SettingsData.TextToSpeech );
			EnableTutorials.Call( "set_value", SettingsData.EnableTutorials );
			HUDPreset.Call( "set_value", (int)SettingsData.HUDPreset );

			NetworkingEnabled.Call( "set_value", SettingsData.EnableNetworking );
			BountyHuntEnabled.Call( "set_value", SettingsData.BountyHuntEnabled );
			CODLobbies.Call( "set_value", SettingsData.CODLobbies );

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

			Button defaultSettingsButton = GetNode<Button>( "DefaultSettingsButton" );
			Methods.ConnectMenuButton( defaultSettingsButton, this, OnDefaultSettingsButtonPressed );

			Button saveSettingsButton = GetNode<Button>( "SaveSettingsButton" );
			Methods.ConnectMenuButton( saveSettingsButton, this, OnSaveSettingsButtonPressed );
		}

		private void ShowTab( int currentTab ) {
			TabBars[ currentTab ].Show();
			TabContainer.EmitSignal( TabContainer.SignalName.TabClicked, currentTab );
		}

		/*
		===============
		_UnhandledInput
		===============
		*/
		public override void _UnhandledInput( InputEvent @event ) {
			if ( Input.IsActionJustPressed( "menu_settings_next_category" ) ) {
				int currentTab = TabContainer.CurrentTab - 1;
				if ( currentTab < 0 ) {
					currentTab = TabContainer.GetTabCount();
				}
				ShowTab( currentTab );
			}
			if ( Input.IsActionJustPressed( "menu_settings_prev_category" ) ) {
				int currentTab = TabContainer.CurrentTab + 1;
				if ( currentTab >= TabContainer.GetTabCount() ) {
					currentTab = 0;
				}
				ShowTab ( currentTab );
			}
		}
	};
};