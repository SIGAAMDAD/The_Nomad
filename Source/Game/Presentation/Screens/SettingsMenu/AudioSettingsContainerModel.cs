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
using System.Collections.Generic;
using Nomad.Audio.Interfaces;
using Nomad.Audio.ValueObjects;
using Nomad.Core.Compatibility.Guards;
using Nomad.Core.CVars;
using Nomad.EngineUtils.Settings.Services;

namespace Nomad.Game.Presentation.Screens.SettingsMenu
{
	/*
	===================================================================================

	AudioSettingsContainerModel

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed class AudioSettingsContainerModel
	{
		public float MasterVolume => _service.Config.MasterVolume;
		public float MasterMinVolume { get; private set; } = 0.0f;
		public float MasterMaxVolume { get; private set; } = 100.0f;
		public float MusicVolume => _service.Config.MusicVolume;
		public float MusicMinVolume { get; private set; } = 0.0f;
		public float MusicMaxVolume { get; private set; } = 100.0f;
		public bool MusicOn => _service.Config.MusicOn;
		public float EffectsVolume => _service.Config.SoundEffectsVolume;
		public float EffectsMinVolume { get; private set; } = 0.0f;
		public float EffectsMaxVolume { get; private set; } = 100.0f;
		public bool EffectsOn => _service.Config.SoundEffectsOn;
		public int OutputDeviceIndex => _service.Config.OutputDeviceIndex;
		public int AudioDriverAPI => _service.Config.AudioDriver;
		public SpeakerMode SpeakerMode => _service.Config.SpeakerMode;

		public IReadOnlyList<string> OutputDevices => _device.OutputDevices;
		public IReadOnlyList<string> AudioDrivers => _device.AudioDrivers;
		public IReadOnlyList<string> SpeakerModes => null;

		public event Action Changed;

		private readonly IAudioDevice _device;
		private readonly AudioSettingsService _service;

		/*
		===============
		AudioSettingsContainerModel
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="device"></param>
		/// <param name="service"></param>
		/// <param name="cvarSystem"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public AudioSettingsContainerModel( IAudioDevice device, AudioSettingsService service, ICVarSystemService cvarSystem )
		{
			ArgumentGuard.ThrowIfNull( cvarSystem, nameof( cvarSystem ) );

			_device = device ?? throw new ArgumentNullException( nameof( device ) );
			_service = service ?? throw new ArgumentNullException( nameof( service ) );

			string driverAPI = device.AudioDriver;
			var drivers = device.AudioDrivers;
			for ( int i = 0; i < drivers.Count; i++ ) {
				if ( driverAPI.Equals( drivers[i], StringComparison.InvariantCulture ) ) {
					_service.Config.AudioDriver = i;
					break;
				}
			}

			MusicMinVolume = 0.0f;
			MusicMaxVolume = 100.0f;
			MasterMinVolume = 0.0f;
			MasterMaxVolume = 100.0f;
			EffectsMinVolume = 0.0f;
			EffectsMaxVolume = 100.0f;
		}

		public void Reset()
		{
			_service.Reset();
		}

		public void Save()
		{
			_service.Save();
		}

		public void SetMusicVolume( float value )
		{
			_service.Config.MusicVolume = value;
		}

		public void SetMusicOn( bool value )
		{
			_service.Config.MusicOn = value;
		}

		public void SetEffectsVolume( float value )
		{
			_service.Config.SoundEffectsVolume = value;
		}

		public void SetEffectsOn( bool value )
		{
			_service.Config.SoundEffectsOn = value;
		}

		public void SetMasterVolume( float volume )
		{
			_service.Config.MasterVolume = volume;
		}

		public void SetSpeakerMode( int value )
		{
			_service.Config.SpeakerMode = (SpeakerMode)value;
		}

		public void SetOutputDevice( int value )
		{
			_service.Config.OutputDeviceIndex = value;
		}

		public void SetAudioDriverAPI( int value )
		{
			_service.Config.AudioDriver = value;
		}
	};
};
