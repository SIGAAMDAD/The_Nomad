/*
===========================================================================
The Nomad MPLv2 Source Code
Copyright (C) 2025-2026 Noah Van Til

This Source Code Form is subject to the terms of the Mozilla Public
License, v2. If a copy of the MPL was not distributed with this
file, You can obtain one at https://mozilla.org/MPL/2.0/.

This software is provided "as is", without warranty of any kind,
express or implied, including but not limited to the warranties
of merchantability, fitness for a particular purpose and noninfringement.
===========================================================================
*/

using Nomad.Game.Application.UI;
using Nomad.Game.Application.UI.Menus.Events;
using Nomad.Core.Events;
using Nomad.Events.Globals;
using Nomad.UI;
using Nomad.Game.Infrastructure.UI.Nodes.OptionList;
using Nomad.Game.Infrastructure.UI.Nodes.OptionSlider;
using Nomad.CVars.Global;
using Nomad.CVars;
using Nomad.Core.Engine.Windowing;
using Nomad.Core.Engine.Rendering;
using Nomad.Core.ServiceRegistry.Globals;
using Nomad.Core.Engine.Services;
using Nomad.Game.Application.UI.Menus;
using System;
using Nomad.EngineUtils.Settings.Services;
using Nomad.Core.CVars;
using Nomad.Core.FileSystem;
using Nomad.Audio.Interfaces;
using Nomad.Game.Infrastructure.UI.Nodes.OptionCheckbox;
using Nomad.Core.Input;
using Nomad.Input.Interfaces;

namespace Nomad.Game.Presentation.Screens.SettingsMenu {
	/*
	===================================================================================
	
	SettingsMenu
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public partial class SettingsMenu : EnginePanel {
		private ISubscriptionGroup _buttonGroup;
		private DisplaySettingsService _displaySettings;
		private AudioSettingsService _audioSettings;

		/*
		===============
		OnInit
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		protected override void OnInit() {
			_buttonGroup = GameEventRegistry.GetGroup( nameof( SettingsMenu ) );

			_buttonGroup.Add( FindChild<EngineButton>( "BottomContainer/ButtonContainer/BackButton" ).Clicked, OnBackPressed );
			_buttonGroup.Add( FindChild<EngineButton>( "BottomContainer/ButtonContainer/ResetButton" ).Clicked, OnResetPressed );
			_buttonGroup.Add( FindChild<EngineButton>( "BottomContainer/ButtonContainer/SaveButton" ).Clicked, OnSavePressed );

			var cvarSystem = ServiceLocator.GetService<ICVarSystemService>();

			_displaySettings ??= new DisplaySettingsService(
				ServiceLocator.GetService<IDisplayService>(),
				cvarSystem
			);
			_audioSettings ??= new AudioSettingsService(
				cvarSystem
			);

			InitBasicDisplayOptions();
			InitAdvancedDisplayOptions();
			InitAudioOptions();
		}

		/*
		===============
		OnShutdown
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		protected override void OnShutdown() {
			_buttonGroup?.Dispose();
		}

		/*
		===============
		InitControlOptions
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		private void InitControlOptions() {
			var container = FindChild<EngineVerticalContainer>( "TabContainer/Controls" );

			var bindResolver = ServiceLocator.GetService<IBindResolver>();
			
			var keyboardBinds = bindResolver.GetBindMapping( "KeyboardAndMouse" );
			
			var gamepadBinds = bindResolver.GetBindMapping( "Gamepad" );
		}

		/*
		===============
		InitAudioOptions
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		private void InitAudioOptions() {
			var container = FindChild<EngineVerticalContainer>( "TabContainer/Audio" );

			var cvarSystem = CVarSystem.Instance;
			var audioDevice = ServiceLocator.GetService<IAudioDevice>();

			var outputDeviceList = container.FindChild<OptionList>( "OutputDeviceList" );
			outputDeviceList.SetOptions( audioDevice.GetOutputDevices() );
			outputDeviceList.Value = cvarSystem.GetCVarOrThrow<int>( Core.Constants.CVars.EngineUtils.Audio.OUTPUT_DEVICE_INDEX ).Value;

			var audioDriver = container.FindChild<OptionList>( "DriverAPIList" );
			var driverList = audioDevice.GetAudioDrivers();
			audioDriver.SetOptions( driverList );
			string currentDriver = cvarSystem.GetCVarOrThrow<string>( Core.Constants.CVars.EngineUtils.Audio.AUDIO_DRIVER ).Value;
			for ( int i = 0; i < driverList.Count; i++ ) {
				if ( driverList[i].Equals( currentDriver, StringComparison.InvariantCulture ) ) {
					audioDriver.Value = i;
					break;
				}
			}

			var masterVolume = container.FindChild<OptionSlider>( "MasterVolumeSlider" );
			masterVolume.Value = cvarSystem.GetCVarOrThrow<float>( Core.Constants.CVars.EngineUtils.Audio.MASTER_VOLUME ).Value;

			var musicVolume = container.FindChild<OptionSlider>( "MusicVolumeSlider" );
			musicVolume.Value = cvarSystem.GetCVarOrThrow<float>( Core.Constants.CVars.EngineUtils.Audio.MUSIC_VOLUME ).Value;

			var musicOn = container.FindChild<OptionCheckbox>( "MusicOnCheckbox" );
			musicOn.Value = cvarSystem.GetCVarOrThrow<bool>( Core.Constants.CVars.EngineUtils.Audio.MUSIC_ON ).Value;

			var effectsVolume = container.FindChild<OptionSlider>( "EffectsVolumeSlider" );
			effectsVolume.Value = cvarSystem.GetCVarOrThrow<float>( Core.Constants.CVars.EngineUtils.Audio.EFFECTS_VOLUME ).Value;

			var effectsOn = container.FindChild<OptionCheckbox>( "EffectsOnCheckbox" );
			effectsOn.Value = cvarSystem.GetCVarOrThrow<bool>( Core.Constants.CVars.EngineUtils.Audio.EFFECTS_ON ).Value;
		}

		/*
		===============
		InitBasicDisplayOptions
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		private void InitBasicDisplayOptions() {
			var container = FindChild<EngineVerticalContainer>( "TabContainer/Display/Basic" );

			var cvarSystem = CVarSystem.Instance;
			var windowService = ServiceLocator.GetService<IWindowService>();

			var monitors = new string[windowService.Monitors.Count];
			for ( int i = 0; i < monitors.Length; i++ ) {
				monitors[i] = i.ToString();
			}

			var monitorIndex = container.FindChild<OptionList>( "MonitorList" );
			monitorIndex.SetOptions( monitors );
			monitorIndex.Value = cvarSystem.GetCVarOrThrow<int>( Core.Constants.CVars.EngineUtils.Display.MONITOR ).Value;

			var windowModes = new string[] {
				"Windowed",
				"Borderless Windowed",
				"Fullscreen"
			};
			var windowMode = container.FindChild<OptionList>( "WindowModeList" );
			windowMode.SetOptions( windowModes );
			windowMode.Value = cvarSystem.GetCVarOrThrow<WindowMode>( Core.Constants.CVars.EngineUtils.Display.WINDOW_MODE ).Value switch {
				WindowMode.Windowed => 0,
				WindowMode.BorderlessWindowed => 1,
				WindowMode.ExclusiveFullscreen => 2
			};

			var supportedResolutions = windowService.GetSupportedResolutions( monitorIndex.Value );
			var windowResolutions = new string[supportedResolutions.Count];
			for ( int i = 0; i < windowResolutions.Length; i++ ) {
				windowResolutions[i] = supportedResolutions[i].ToDisplayString();
			}
			var windowResolution = container.FindChild<OptionList>( "WindowResolutionList" );
			windowResolution.SetOptions( windowResolutions );
			windowResolution.Value = (int)cvarSystem.GetCVarOrThrow<WindowResolution>( Core.Constants.CVars.EngineUtils.Display.WINDOW_RESOLUTION ).Value;

			var vsyncModes = new string[(int)VSyncMode.Count];
			for ( int i = 0; i < vsyncModes.Length; i++ ) {
				var mode = (VSyncMode)((int)VSyncMode.Enabled + i);
				vsyncModes[i] = mode.ToDisplayString();
			}

			var vsyncList = container.FindChild<OptionList>( "VSyncList" );
			vsyncList.SetOptions( vsyncModes );
			vsyncList.Value = (int)cvarSystem.GetCVarOrThrow<VSyncMode>( Core.Constants.CVars.EngineUtils.Display.VSYNC_MODE ).Value;

			var maximumFramerate = container.FindChild<OptionSlider>( "MaxFpsSlider" );
			maximumFramerate.Value = cvarSystem.GetCVarOrThrow<int>( Core.Constants.CVars.EngineUtils.Display.MAX_FPS ).Value;

			var aspectRatios = new string[(int)AspectRatio.Count];
			for ( int i = 0; i < aspectRatios.Length; i++ ) {
				var ratio = (AspectRatio)((int)AspectRatio.Aspect_4_3 + i);
				aspectRatios[i] = ratio.ToDisplayString();
			}
			var aspectRatio = container.FindChild<OptionList>( "AspectRatioList" );
			aspectRatio.SetOptions( aspectRatios );
			aspectRatio.Value = (int)cvarSystem.GetCVarOrThrow<AspectRatio>( Core.Constants.CVars.EngineUtils.Display.ASPECT_RATIO ).Value;
		}

		/*
		===============
		InitAdvancedDisplayOptions
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		private void InitAdvancedDisplayOptions() {
			var container = FindChild<EngineVerticalContainer>( "TabContainer/Display/Advanced" );

			var cvarSystem = CVarSystem.Instance;
			var windowService = ServiceLocator.GetService<IWindowService>();
			var displayService = ServiceLocator.GetService<IDisplayService>();

			var antiAliasing = container.FindChild<OptionList>( "AntiAliasingList" );
			antiAliasing.SetOptions( _displaySettings.AntiAliasingModes );
			antiAliasing.Value = (int)cvarSystem.GetCVarOrThrow<AntiAliasingMode>( Core.Constants.CVars.EngineUtils.Display.ANTI_ALIASING ).Value;
		}

		/*
		===============
		OnBackPressed
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnBackPressed( in EmptyEventArgs args ) {
			GameEventRegistry.GetEvent<MenuTransitionRequestedEventArgs>( UIConstants.MENU_TRANSITION_REQUESTED_EVENT, UIConstants.NAMESPACE ).Publish( new MenuTransitionRequestedEventArgs( MenuState.Settings, MenuState.Main ) );
		}

		/*
		===============
		OnResetPressed
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnResetPressed( in EmptyEventArgs args ) {
		}

		/*
		===============
		OnSavePressed
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnSavePressed( in EmptyEventArgs args ) {
			{
				var basicContainer = FindChild<EngineVerticalContainer>( "TabContainer/Display/Basic" );
				var monitorIndex = basicContainer.FindChild<OptionList>( "MonitorList" );
				var windowMode = basicContainer.FindChild<OptionList>( "WindowModeList" );
				var vsyncList = basicContainer.FindChild<OptionList>( "VSyncList" );
				var maximumFramerate = basicContainer.FindChild<OptionSlider>( "MaxFpsSlider" );
				var windowResolution = basicContainer.FindChild<OptionList>( "WindowResolutionList" );
				var aspectRatio = basicContainer.FindChild<OptionList>( "AspectRatioList" );

				_displaySettings.Config.MaximumFrameRate = (int)maximumFramerate.Value;
				_displaySettings.Config.WindowMode = windowMode.Value switch {
					0 => WindowMode.Windowed,
					1 => WindowMode.BorderlessWindowed,
					2 => WindowMode.ExclusiveFullscreen,
					_ => throw new ArgumentOutOfRangeException( nameof( windowMode ) )
				};
				_displaySettings.Config.Resolution = (WindowResolution)windowResolution.Value;
				_displaySettings.Config.VSyncMode = (VSyncMode)vsyncList.Value;
				_displaySettings.Config.MonitorIndex = monitorIndex.Value;
			}
			{
				var container = FindChild<EngineVerticalContainer>( "TabContainer/Audio" );

				var outputDeviceList = container.FindChild<OptionList>( "OutputDeviceList" );
				_audioSettings.Config.OutputDeviceIndex = outputDeviceList.Value;

				var audioDriver = container.FindChild<OptionList>( "DriverAPIList" );
				_audioSettings.Config.AudioDriver = audioDriver.Values[ audioDriver.Value ];
				
				var masterVolume = container.FindChild<OptionSlider>( "MasterVolume" );
				
				var musicVolume = container.FindChild<OptionSlider>( "MusicVolumeSlider" );
				_audioSettings.Config.MusicVolume = musicVolume.Value;

				var musicOn = container.FindChild<OptionCheckbox>( "MusicOnCheckbox" );
				_audioSettings.Config.MusicOn = musicOn.Value;

				var effectsVolume = container.FindChild<OptionSlider>( "EffectsVolumeSlider" );
				_audioSettings.Config.SoundEffectsVolume = effectsVolume.Value;

				var effectsOn = container.FindChild<OptionCheckbox>( "EffectsOnCheckbox" );
				_audioSettings.Config.SoundEffectsOn = effectsOn.Value;
			}

			_displaySettings.Save();
			_audioSettings.Save();

			var fileSystem = ServiceLocator.GetService<IFileSystem>();
			CVarSystem.Save( fileSystem, $"{fileSystem.GetUserDataPath()}/UserConfig.ini" );
		}
	};
};
