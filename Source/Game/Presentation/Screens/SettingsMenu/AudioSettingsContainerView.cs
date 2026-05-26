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
using Godot;
using Nomad.Game.Presentation.Widgets.OptionCheckbox;
using Nomad.Game.Presentation.Widgets.OptionList;
using Nomad.Game.Presentation.Widgets.OptionSlider;
using Nomad.Game.Presentation.Screens.SettingsMenu;

namespace Nomad.Game.Presentation.Screens.SettingsMenu
{
	/*
	===================================================================================

	AudioSettingsContainerView

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed partial class AudioSettingsContainerView : VBoxContainer
	{
		public event Action<int> AudioDriverChanged;
		public event Action<int> OutputDeviceChanged;
		public event Action<float> MasterVolumeChanged;
		public event Action<float> MusicVolumeChanged;
		public event Action<bool> MusicOnChanged;
		public event Action<float> EffectsVolumeChanged;
		public event Action<bool> EffectsOnChanged;

		private OptionSlider _masterVolume;
		private OptionSlider _musicVolume;
		private OptionCheckbox _musicOn;
		private OptionSlider _effectsVolume;
		private OptionCheckbox _effectsOn;
		private OptionList _driverAPI;
		private OptionList _outputDevice;
		private OptionList _speakerMode;

		public void SetMusicVolumeLimits( float min, float max )
		{
			_musicVolume.Min = min;
			_musicVolume.Max = max;
		}

		public void SetMusicVolume( float value )
		{
			_musicVolume.Value = value;
			MusicVolumeChanged?.Invoke( value );
		}

		public void SetMusicOn( bool value )
		{
			_musicOn.Value = value;
			MusicOnChanged?.Invoke( value );
		}

		public void SetEffectsVolumeLimits( float min, float max )
		{
			_effectsVolume.Min = min;
			_effectsVolume.Max = max;
		}

		public void SetEffectsVolume( float value )
		{
			_effectsVolume.Value = value;
			EffectsVolumeChanged?.Invoke( value );
		}

		public void SetEffectsOn( bool value )
		{
			_effectsOn.Value = value;
			EffectsOnChanged?.Invoke( value );
		}

		public void SetOutputDevice( int value )
		{
			_outputDevice.Value = value;
			OutputDeviceChanged?.Invoke( value );
		}

		public void SetOutputDevices( IReadOnlyList<string> items )
		{
			_outputDevice.SetOptions( items );
		}

		public void SetAudioDriver( int value )
		{
			_driverAPI.Value = value;
			AudioDriverChanged?.Invoke( value );
		}

		public void SetAudioDrivers( IReadOnlyList<string> items )
		{
			_driverAPI.SetOptions( items );
		}

		/*
		===============
		_Ready
		===============
		*/
		/// <summary>
		///
		/// </summary>
		public override void _Ready()
		{
			base._Ready();

			_driverAPI = GetNode<OptionList>( "DriverAPIList" );
			_driverAPI.ValueSet.Subscribe( ( in value ) => AudioDriverChanged?.Invoke( value.Value ) );

			_outputDevice = GetNode<OptionList>( "OutputDeviceList" );
			_outputDevice.ValueSet.Subscribe( ( in value ) => OutputDeviceChanged?.Invoke( value.Value ) );

			_masterVolume = GetNode<OptionSlider>( "MasterVolumeSlider" );
			_masterVolume.ValueChanged.Subscribe( ( in value ) => MasterVolumeChanged?.Invoke( value.Value ) );

			_musicVolume = GetNode<OptionSlider>( "MusicVolumeSlider" );
			_musicVolume.ValueChanged.Subscribe( ( in value ) => MusicVolumeChanged?.Invoke( value.Value ) );

			_musicOn = GetNode<OptionCheckbox>( "MusicOnCheckbox" );
			_musicOn.Toggled.Subscribe( ( in value ) => MusicOnChanged?.Invoke( value.Value ) );

			_effectsVolume = GetNode<OptionSlider>( "EffectsVolumeSlider" );
			_effectsVolume.ValueChanged.Subscribe( ( in value ) => EffectsVolumeChanged?.Invoke( value.Value ) );

			_effectsOn = GetNode<OptionCheckbox>( "EffectsOnCheckbox" );
			_effectsOn.Toggled.Subscribe( ( in value ) => EffectsOnChanged?.Invoke( value.Value ) );
		}
	};
};
