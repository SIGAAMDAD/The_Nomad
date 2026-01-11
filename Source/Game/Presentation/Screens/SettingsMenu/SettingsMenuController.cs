using Game.Application.Common.Models.Interfaces;
using Game.Application.Configuration.Enums;
using Game.Application.UI;
using Game.Application.UI.Menus;
using Game.Application.UI.Menus.Events;
using Game.Domain.Configuration.Interfaces;
using Game.Domain.Events.UI;
using Game.Infrastructure;
using Game.Infrastructure.Configuration;
using Game.Infrastructure.UI.NomadUI.SelectionNodes.OptionCheckbox;
using Game.Infrastructure.UI.NomadUI.SelectionNodes.OptionList;
using Game.Infrastructure.UI.NomadUI.SelectionNodes.OptionSlider;
using Godot;
using Nomad.Core;
using Nomad.Core.Events;
using Nomad.Core.Logger;
using Nomad.Core.Memory;
using Nomad.Core.Util;
using Nomad.CVars;
using System;
using System.Collections.Generic;

namespace Game.Presentation.Screens.SettingsMenu {
	/*
	===================================================================================
	
	SettingsMenuController

	FIXME: this does too much
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public sealed class SettingsMenuController : IDisposable {
		private enum State : int {
			Display,
			Graphics,
			Audio,
			Accessibility,
			Controls
		};

		private readonly OptionListController _windowModeController;
		private readonly OptionListController _monitorController;
		private readonly OptionListController _antiAliasingController;
		private readonly OptionListController _vsyncModesController;
		private readonly OptionListController _windowResolutionController;
		private readonly OptionListController _maxFpsController;
		private readonly OptionListController _aspectRatioController;
		private readonly OptionSliderController _brightnessController;

		private readonly OptionListController _audioDriverController;
		private readonly OptionListController _outputDeviceController;
		private readonly OptionCheckboxController _effectsOnController;
		private readonly OptionSliderController _effectsVolumeController;
		private readonly OptionCheckboxController _musicOnController;
		private readonly OptionSliderController _musicVolumeController;
		private readonly OptionSliderController _masterVolumeController;

		private readonly SettingsMenuView _view;
		private readonly ISettingsDataRepository _dataRepository;

		private readonly IDisplaySettingsService _displaySettings;
		private readonly IGraphicsSettingsService _graphicsSettings;
		private readonly IAudioSettingsService _audioSettings;
		private readonly ICVarSystemService _cvarSystem;

		private readonly MenuStateMachine<State> _stateMachine;

		private readonly ILoggerService _logger;

		private InternString _menuId => StringPool.Intern( nameof( SettingsMenu ) );

		/*
		===============
		SettingsMenuController
		===============
		*/
		/// <summary>
		/// Creates a SettingsMenuController.
		/// </summary>
		/// <param name="view"></param>
		public SettingsMenuController( SettingsMenuView view ) {
			_view = view;

			var serviceLocator = view.Owner.GetNode<NomadBootstrapper>( "/root/NomadBootstrapper" ).ServiceLocator;
			var eventBus = serviceLocator.GetService<IGodotEventBusService>();
			var eventFactory = serviceLocator.GetService<IGameEventRegistryService>();

			_cvarSystem = serviceLocator.GetService<ICVarSystemService>();
			_logger = serviceLocator.GetService<ILoggerService>();

			_displaySettings = serviceLocator.GetService<IDisplaySettingsService>();
			_graphicsSettings = serviceLocator.GetService<IGraphicsSettingsService>();
			_audioSettings = serviceLocator.GetService<IAudioSettingsService>();

			_dataRepository = new SettingsDataRepository( serviceLocator.GetService<ICVarSystemService>() );
			_stateMachine = new MenuStateMachine<State>( _menuId, State.Display, eventFactory,
				new Dictionary<State, Control?> {
					[ State.Display ] = _view.DisplayOptionsContainer,
					[ State.Graphics ] = _view.GraphicsOptionsContainer,
					[ State.Audio ] = _view.AudioOptionsContainer,
					[ State.Accessibility ] = _view.AccessibilityOptionsContainer,
					[ State.Controls ] = _view.ControlsOptionContainer
				}
			);

			_monitorController = new OptionListController( eventBus, view.MonitorList );
			_aspectRatioController = new OptionListController( eventBus, view.AspectRatioList );
			_windowModeController = new OptionListController( eventBus, view.WindowModeList );
			_antiAliasingController = new OptionListController( eventBus, view.AntiAliasingList );
			_windowResolutionController = new OptionListController( eventBus, view.WindowResolutionList );
			_vsyncModesController = new OptionListController( eventBus, view.VSyncList );
			_brightnessController = new OptionSliderController( eventBus, view.BrightnessSlider );
			_maxFpsController = new OptionListController( eventBus, view.MaxFpsList );

			_audioDriverController = new OptionListController( eventBus, view.AudioDriverList );
			_outputDeviceController = new OptionListController( eventBus, view.OutputDeviceList );
			_masterVolumeController = new OptionSliderController( eventBus, view.MasterVolumeSlider );
			_musicVolumeController = new OptionSliderController( eventBus, view.MusicVolumeSlider );
			_musicOnController = new OptionCheckboxController( eventBus, view.MusicOnToggle );
			_effectsVolumeController = new OptionSliderController( eventBus, view.EffectsVolumeSlider );
			_effectsOnController = new OptionCheckboxController( eventBus, view.EffectsOnToggle );

			InitDisplayOptions();
			InitGraphicsOptions();
			InitAudioOptions();

			UIEventHelper.SubscribeToUIEvent<OptionListValueSetEventArgs>( eventFactory, this, UIConstants.OPTION_LIST_VALUE_SET_EVENT, OnOptionListValueChanged );
			UIEventHelper.SubscribeToUIEvent<OptionSliderValueChangedEventArgs>( eventFactory, this, UIConstants.OPTION_SLIDER_VALUE_CHANGED_EVENT, OnOptionSliderValueChanged );
			UIEventHelper.SubscribeToUIEvent<OptionCheckboxValueChangedEventArgs>( eventFactory, this, UIConstants.OPTION_CHECKBOX_TOGGLED_EVENT, OnOptionCheckboxValueChanged );
			UIEventHelper.SubscribeToUIEvent<ButtonClickedEventArgs>( eventFactory, this, UIConstants.BUTTON_CLICKED_EVENT, OnButtonClicked );
		}

		/*
		===============
		Dispose
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		public void Dispose() {
			var eventFactory = _view.Owner.GetNode<NomadBootstrapper>( "/root/NomadBootstrapper" ).ServiceLocator.GetService<IGameEventRegistryService>();

			UIEventHelper.UnsubscribeFromUIEvent<OptionListValueSetEventArgs>( eventFactory, this, UIConstants.OPTION_LIST_VALUE_SET_EVENT, OnOptionListValueChanged );
			UIEventHelper.UnsubscribeFromUIEvent<OptionSliderValueChangedEventArgs>( eventFactory, this, UIConstants.OPTION_SLIDER_VALUE_CHANGED_EVENT, OnOptionSliderValueChanged );
			UIEventHelper.UnsubscribeFromUIEvent<OptionCheckboxValueChangedEventArgs>( eventFactory, this, UIConstants.OPTION_CHECKBOX_TOGGLED_EVENT, OnOptionCheckboxValueChanged );
			UIEventHelper.UnsubscribeFromUIEvent<ButtonClickedEventArgs>( eventFactory, this, UIConstants.BUTTON_CLICKED_EVENT, OnButtonClicked );
		}

		/*
		===============
		InitDisplayOptions
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="serviceLocator"></param>
		private void InitDisplayOptions() {
			var config = _displaySettings.Config;

			_antiAliasingController.SetOptions( _displaySettings.GetSupportedAntiAliasingModes() );
			_antiAliasingController.SetValue( (int)config.AntiAliasing );

			_windowResolutionController.SetOptions( _displaySettings.GetSupportedResolutions() );
			_windowResolutionController.SetValue( (int)config.WindowResolution );

			_windowModeController.SetOptions( _displaySettings.GetSupportedDisplayModes() );
			_windowModeController.SetValue( (int)config.WindowMode );

			_vsyncModesController.SetOptions( _displaySettings.GetSupportedVSyncModes() );
			_vsyncModesController.SetValue( (int)config.VSyncMode );

			_maxFpsController.SetOptions( _displaySettings.GetSupportedFrameLimits() );
			_maxFpsController.SetValue( (int)config.MaxFps );

			_brightnessController.Value = config.Brightness;

			int monitorCount = _displaySettings.MonitorCount;
			var monitors = new string[ monitorCount ];
			for ( int i = 0; i < monitorCount; i++ ) {
				monitors[ i ] = i.ToString();
			}
			_monitorController.SetOptions( monitors );
			_monitorController.SetValue( config.Monitor );

			_aspectRatioController.SetOptions( _displaySettings.GetSupportedAspectRatios() );
			_aspectRatioController.SetValue( (int)config.AspectRatio );
		}

		/*
		===============
		InitGraphicsOptions
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="serviceLocator"></param>
		private void InitGraphicsOptions() {
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
			IReadOnlyList<string> audioDrivers = [ .. _audioSettings.GetAudioDrivers() ];
			_audioDriverController.SetOptions( audioDrivers );

			var config = _audioSettings.Config;

			int audioDriverIndex = -1;
			for ( int i = 0; i < audioDrivers.Count; i++ ) {
				if ( audioDrivers[ i ].Equals( config.AudioDriver ) ) {
					audioDriverIndex = i;
					break;
				}
			}
			if ( audioDriverIndex == -1 ) {
				throw new Exception( $"Invalid audio driver!" );
			}
			_audioDriverController.SetValue( audioDriverIndex );

			_outputDeviceController.SetOptions( [ .. _audioSettings.GetAudioDevices() ] );
			_outputDeviceController.SetValue( config.OutputAudioDevice );

			_musicOnController.Value = config.MusicOn;
			_musicVolumeController.Value = config.MusicVolume;

			_effectsOnController.Value = config.EffectsOn;
			_effectsVolumeController.Value = config.EffectsVolume;

			_masterVolumeController.Value = config.MasterVolume;
		}

		/*
		===============
		OnSaveSettings
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		private void OnSaveSettings() {
			SaveDisplaySettings();
			SaveAudioSettings();
			_cvarSystem.Save( FilePath.FromUserPath( "user://settings.ini" ).OSPath );
		}

		/*
		===============
		SaveDisplaySettings
		===============
		*/
		private void SaveDisplaySettings() {
			var config = _displaySettings.Config;

			_dataRepository.SetValue( Constants.CVars.Display.MONITOR, config.Monitor );
			_dataRepository.SetValue( Constants.CVars.Display.ANTI_ALIASING, config.AntiAliasing );
			_dataRepository.SetValue( Constants.CVars.Display.ASPECT_RATIO, config.AspectRatio );
			_dataRepository.SetValue( Constants.CVars.Display.WINDOW_RESOLUTION, config.WindowResolution );
			_dataRepository.SetValue( Constants.CVars.Display.WINDOW_MODE, config.WindowMode );
			_dataRepository.SetValue( Constants.CVars.Display.BRIGHTNESS, config.Brightness );
			_dataRepository.SetValue( Constants.CVars.Display.VSYNC_MODE, config.VSyncMode );
			_dataRepository.SetValue( Constants.CVars.Display.MAX_FPS, config.MaxFps );
		}

		/*
		===============
		SaveGraphicsSettings
		===============
		*/
		private void SaveGraphicsSettings() {
		}

		/*
		===============
		SaveAudioSettings
		===============
		*/
		private void SaveAudioSettings() {
			var config = _audioSettings.Config;

			_dataRepository.SetValue( Constants.CVars.Audio.OUTPUT_DEVICE_INDEX, config.OutputAudioDevice );
			_dataRepository.SetValue( Constants.CVars.Audio.EFFECTS_ON, config.EffectsOn );
			_dataRepository.SetValue( Constants.CVars.Audio.EFFECTS_VOLUME, config.EffectsVolume );
			_dataRepository.SetValue( Constants.CVars.Audio.MUSIC_ON, config.MusicOn );
			_dataRepository.SetValue( Constants.CVars.Audio.MUSIC_VOLUME, config.MusicVolume );
		}

		/*
		===============
		OnResetSettings
		===============
		*/
		private void OnResetSettings() {
		}

		/*
		===============
		OnButtonClicked
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		/// <exception cref="Exception"></exception>
		private void OnButtonClicked( in ButtonClickedEventArgs args ) {
			if ( args.ButtonId == _view.SaveButton.ButtonId ) {
				OnSaveSettings();
			} else if ( args.ButtonId == _view.ResetButton.ButtonId ) {
				OnResetSettings();
			} else if ( args.ButtonId == _view.QuitButton.ButtonId ) {
				var eventFactory = _view.Owner.GetNode<NomadBootstrapper>( "/root/NomadBootstrapper" ).ServiceLocator.GetService<IGameEventRegistryService>();
				UIEventHelper.PublishUIEvent( eventFactory, UIConstants.MENU_TRANSITION_REQUESTED_EVENT, new MenuTransitionRequestedEventArgs( MenuState.Settings, MenuState.Main ) );
			} else {
				throw new Exception( $"Invalid ButtonId for SettingsMenu - {args.ButtonId}" );
			}
		}

		/*
		===============
		OnOptionFocused
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnOptionFocused( in OptionListFocusedEventArgs args ) {
		}

		/*
		===============
		OnOptionListValueChanged
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnOptionListValueChanged( in OptionListValueSetEventArgs args ) {
			if ( args.ListId == _windowModeController.ListId ) {
				_displaySettings.Config.WindowMode = ( (WindowModeBasic)args.Value ).AsRealMode();
			} else if ( args.ListId == _windowResolutionController.ListId ) {
				_displaySettings.Config.WindowResolution = (WindowResolution)args.Value;
			} else if ( args.ListId == _antiAliasingController.ListId ) {
				_displaySettings.Config.AntiAliasing = (AntiAliasing)args.Value;
			} else if ( args.ListId == _monitorController.ListId ) {
				_displaySettings.Config.Monitor = args.Value;
			} else if ( args.ListId == _vsyncModesController.ListId ) {
				_displaySettings.Config.VSyncMode = (VSyncMode)args.Value;
			} else if ( args.ListId == _maxFpsController.ListId ) {
				_displaySettings.Config.MaxFps = (MaxFps)args.Value;
			} else if ( args.ListId == _aspectRatioController.ListId ) {
				_displaySettings.Config.AspectRatio = (AspectRatio)args.Value;
			}
		}

		/*
		===============
		OnOptionCheckboxValueChanged
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnOptionCheckboxValueChanged( in OptionCheckboxValueChangedEventArgs args ) {
			if ( args.CheckboxId == _musicOnController.CheckboxId ) {
				_audioSettings.Config.MusicOn = args.Value;
			} else if ( args.CheckboxId == _effectsOnController.CheckboxId ) {
				_audioSettings.Config.EffectsOn = args.Value;
			}
		}

		/*
		===============
		OnOptionSliderValueChanged
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnOptionSliderValueChanged( in OptionSliderValueChangedEventArgs args ) {
			if ( args.SliderId == _brightnessController.SliderId ) {
				_displaySettings.Config.Brightness = args.Value;
			} else if ( args.SliderId == _musicVolumeController.SliderId ) {
				_audioSettings.Config.MusicVolume = args.Value;
			} else if ( args.SliderId == _effectsVolumeController.SliderId ) {
				_audioSettings.Config.EffectsVolume = args.Value;
			}
		}
	};
};