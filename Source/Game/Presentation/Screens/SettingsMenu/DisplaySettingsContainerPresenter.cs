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

using Nomad.Game.Presentation.Screens.SettingsMenu;

namespace Nomad.Game.Presentation.Screens.SettingsMenu
{
	/*
	===================================================================================

	DisplaySettingsContainerPresenter

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed class DisplaySettingsContainerPresenter : ISettingsSectionPresenter
	{
		private readonly DisplaySettingsContainerModel _model;
		private readonly DisplaySettingsContainerView _view;

		/*
		===============
		DisplaySettingsContainerPresenter
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="view"></param>
		/// <param name="model"></param>
		public DisplaySettingsContainerPresenter( DisplaySettingsContainerView view, DisplaySettingsContainerModel model )
		{
			_model = model;
			_view = view;

			_view.MonitorChanged += _model.SetMonitorIndex;
			_view.WindowResolutionChanged += _model.SetWindowResolution;
			_view.WindowModeChanged += _model.SetWindowMode;
			_view.VSyncModeChanged += _model.SetVSyncMode;
			_view.MaximumFramerateChanged += _model.SetMaximumFramerate;

			SyncView();
		}

		/*
		===============
		Reset
		===============
		*/
		/// <summary>
		///
		/// </summary>
		public void Reset()
		{
			_model.Reset();
			SyncView();
		}

		/*
		===============
		Save
		===============
		*/
		/// <summary>
		///
		/// </summary>
		public void Save()
		{
			_model.Save();
			SyncView();
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
			_view.SetWindowModes( _model.WindowModes );
			_view.SetWindowMode( _model.WindowMode );

			_view.SetWindowResolutions( _model.WindowResolutions );
			_view.SetWindowResolution( _model.WindowResolution );

			_view.SetVSyncModes( _model.VSyncModes );
			_view.SetVSyncMode( _model.VSyncMode );

			_view.SetMaximumFramerate( _model.MaximumFramerate );
			_view.SetMaximumFramerateLimits( _model.MaximumFramerateMin, _model.MaximumFramerateMax );

			_view.SetMonitors( _model.Monitors );
			_view.SetMonitorIndex( _model.MonitorIndex );
		}
	};
};
