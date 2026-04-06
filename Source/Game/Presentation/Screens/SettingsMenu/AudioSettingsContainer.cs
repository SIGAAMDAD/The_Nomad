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

using System;
using Nomad.Audio.Interfaces;
using Nomad.Core.ServiceRegistry.Globals;
using Nomad.CVars;
using Nomad.CVars.Global;
using Nomad.Game.Infrastructure.UI.Nodes.OptionCheckbox;
using Nomad.Game.Infrastructure.UI.Nodes.OptionList;
using Nomad.Game.Infrastructure.UI.Nodes.OptionSlider;
using Nomad.UI;

namespace Nomad.Game.Presentation.Screens.SettingsMenu {
	/*
	===================================================================================
	
	AudioSettingsContainer
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public sealed partial class AudioSettingsContainer : EngineVerticalContainer {
		/*
		===============
		OnInit
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		protected override void OnInit() {
			base.OnInit();

			var cvarSystem = CVarSystem.Instance;
			var audioDevice = ServiceLocator.GetService<IAudioDevice>();

			var outputDeviceList = FindChild<OptionList>( "OutputDeviceList" );
			outputDeviceList.SetOptions( audioDevice.GetOutputDevices() );
			outputDeviceList.Value = cvarSystem.GetCVarOrThrow<int>( Core.Constants.CVars.EngineUtils.Audio.OUTPUT_DEVICE_INDEX ).Value;

			var audioDriver = FindChild<OptionList>( "DriverAPIList" );
			var driverList = audioDevice.GetAudioDrivers();
			audioDriver.SetOptions( driverList );
			string currentDriver = cvarSystem.GetCVarOrThrow<string>( Core.Constants.CVars.EngineUtils.Audio.AUDIO_DRIVER ).Value;
			for ( int i = 0; i < driverList.Count; i++ ) {
				if ( driverList[i].Equals( currentDriver, StringComparison.InvariantCulture ) ) {
					audioDriver.Value = i;
					break;
				}
			}

			var masterVolume = FindChild<OptionSlider>( "MasterVolumeSlider" );
			masterVolume.Value = cvarSystem.GetCVarOrThrow<float>( Core.Constants.CVars.EngineUtils.Audio.MASTER_VOLUME ).Value;

			var musicVolume = FindChild<OptionSlider>( "MusicVolumeSlider" );
			musicVolume.Min = 0.0f;
			musicVolume.Max = 100.0f;
			musicVolume.Value = cvarSystem.GetCVarOrThrow<float>( Core.Constants.CVars.EngineUtils.Audio.MUSIC_VOLUME ).Value;

			var musicOn = FindChild<OptionCheckbox>( "MusicOnCheckbox" );
			musicOn.Value = cvarSystem.GetCVarOrThrow<bool>( Core.Constants.CVars.EngineUtils.Audio.MUSIC_ON ).Value;

			var effectsVolume = FindChild<OptionSlider>( "EffectsVolumeSlider" );
			effectsVolume.Min = 0.0f;
			effectsVolume.Max = 100.0f;
			effectsVolume.Value = cvarSystem.GetCVarOrThrow<float>( Core.Constants.CVars.EngineUtils.Audio.EFFECTS_VOLUME ).Value;

			var effectsOn = FindChild<OptionCheckbox>( "EffectsOnCheckbox" );
			effectsOn.Value = cvarSystem.GetCVarOrThrow<bool>( Core.Constants.CVars.EngineUtils.Audio.EFFECTS_ON ).Value;
		}
	};
};
