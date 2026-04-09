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
using Nomad.Game.Infrastructure.UI.Nodes.OptionCheckbox;

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
		private ISubscriptionGroup _eventGroup;

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
			_eventGroup = GameEventRegistry.GetGroup( nameof( SettingsMenu ) );
			_eventGroup.Add( FindChild<EngineButton>( "BottomContainer/ButtonContainer/BackButton" ).Clicked, OnBackPressed );
			_eventGroup.Add( FindChild<EngineButton>( "BottomContainer/ButtonContainer/ResetButton" ).Clicked, OnResetPressed );
			_eventGroup.Add( FindChild<EngineButton>( "BottomContainer/ButtonContainer/SaveButton" ).Clicked, OnSavePressed );

			var cvarSystem = ServiceLocator.GetService<ICVarSystemService>();

			_displaySettings ??= new DisplaySettingsService(
				ServiceLocator.GetService<IDisplayService>(),
				cvarSystem
			);
			_audioSettings ??= new AudioSettingsService(
				cvarSystem
			);

			InitControlOptions();
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
			base.OnShutdown();

			_eventGroup?.Dispose();
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
			var container = FindChild<EngineVerticalContainer>( "TabContainer/Controls/Bindings" );

			container.FindChild<BindingButton>( "MoveBind" ).SetBind( "KeyboardAndMouse", "Move" );
			container.FindChild<BindingButton>( "DashBind" ).SetBind( "KeyboardAndMouse", "Dash" );
			container.FindChild<BindingButton>( "ParryBind" ).SetBind( "KeyboardAndMouse", "Parry" );
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
			/*
			{
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
			*/
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
