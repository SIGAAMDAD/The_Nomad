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

using Nomad.Core.ServiceRegistry.Globals;
using Nomad.EngineUtils.Settings.Services;

namespace Nomad.Game.Presentation.Screens.SettingsMenu
{
	/*
	===================================================================================
	
	AudioSettingsContainerPresenter
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	internal sealed class AudioSettingsContainerPresenter : ISettingsSectionPresenter
	{
		private readonly AudioSettingsContainerView _view;
		private readonly AudioSettingsContainerModel _model;

		public AudioSettingsContainerPresenter( AudioSettingsContainerView view, AudioSettingsContainerModel model )
		{
			_view = view;
			_model = model;

			_view.MusicVolumeChanged += _model.SetMusicVolume;
			_view.MusicOnChanged += _model.SetMusicOn;
			_view.EffectsVolumeChanged += _model.SetEffectsVolume;
			_view.EffectsOnChanged += _model.SetEffectsOn;
			_view.OutputDeviceChanged += _model.SetOutputDevice;
			_view.AudioDriverChanged += _model.SetAudioDriverAPI;

			SyncView();
		}

		public void Reset()
		{
			_model.Reset();
		}

		public void Save()
		{
			_model.Save();
		}

		/*
		===============
		SyncView
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		public void SyncView()
		{
			_view.SetMusicVolume( _model.MusicVolume );
			_view.SetMusicVolumeLimits( _model.MusicMinVolume, _model.MusicMaxVolume );
			_view.SetMusicOn( _model.MusicOn );

			_view.SetEffectsVolume( _model.EffectsVolume );
			_view.SetEffectsVolumeLimits( _model.EffectsMinVolume, _model.EffectsMaxVolume );
			_view.SetEffectsOn( _model.EffectsOn );

			_view.SetAudioDrivers( _model.AudioDrivers );
			_view.SetAudioDriver( _model.AudioDriverAPI );

			_view.SetOutputDevices( _model.OutputDevices );
			_view.SetOutputDevice( _model.OutputDeviceIndex );
		}
	};
};
