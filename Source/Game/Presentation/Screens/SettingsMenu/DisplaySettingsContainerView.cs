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
using Nomad.Core.Engine.Rendering;
using Nomad.Core.Engine.Windowing;
using Nomad.Game.Presentation.Widgets.OptionList;
using Nomad.Game.Presentation.Widgets.OptionSlider;
using Nomad.Game.Presentation.Screens.SettingsMenu;

namespace Nomad.Game.Presentation.Screens.SettingsMenu
{
	/*
	===================================================================================

	DisplaySettingsContainerView

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed partial class DisplaySettingsContainerView : TabContainer
	{
		public event Action<int> MonitorChanged;
		public event Action<WindowMode> WindowModeChanged;
		public event Action<WindowResolution> WindowResolutionChanged;
		public event Action<int> MaximumFramerateChanged;
		public event Action<VSyncMode> VSyncModeChanged;
		public event Action<float> BrightnessChanged;

		private OptionList _monitorIndex;
		private OptionList _windowMode;
		private OptionList _windowResolution;
		private OptionList _vsyncList;
		private OptionSlider _maximumFramerate;

		public void SetMonitorIndex( int value )
		{
			_monitorIndex.Value = value;
			MonitorChanged?.Invoke( value );
		}

		public void SetMonitors( IReadOnlyList<string> items )
		{
			_monitorIndex.SetOptions( items );
		}

		public void SetWindowMode( WindowMode value )
		{
			_windowMode.Value = (int)value;
			WindowModeChanged?.Invoke( value );
		}

		public void SetWindowModes( IReadOnlyList<string> items )
		{
			_windowMode.SetOptions( items );
		}

		public void SetWindowResolution( WindowResolution value )
		{
			_windowResolution.Value = (int)value;
			WindowResolutionChanged?.Invoke( value );
		}

		public void SetWindowResolutions( IReadOnlyList<string> items )
		{
			_windowResolution.SetOptions( items );
		}

		public void SetVSyncMode( VSyncMode value )
		{
			_vsyncList.Value = (int)value;
			VSyncModeChanged?.Invoke( value );
		}

		public void SetVSyncModes( IReadOnlyList<string> items )
		{
			_vsyncList.SetOptions( items );
		}

		public void SetMaximumFramerate( int value )
		{
			_maximumFramerate.Value = value;
			MaximumFramerateChanged?.Invoke( value );
		}

		public void SetMaximumFramerateLimits( float min, float max )
		{
			_maximumFramerate.Min = min;
			_maximumFramerate.Max = max;
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

			_monitorIndex = GetNode<OptionList>( "Basic/MonitorList" );
			_monitorIndex.ValueSet.Subscribe( ( in value ) => MonitorChanged?.Invoke( value.Value ) );

			_windowMode = GetNode<OptionList>( "Basic/WindowModeList" );
			_windowMode.ValueSet.Subscribe( ( in value ) => WindowModeChanged?.Invoke( (WindowMode)value.Value ) );

			_windowResolution = GetNode<OptionList>( "Basic/WindowResolutionList" );
			_windowResolution.ValueSet.Subscribe( ( in value ) => WindowResolutionChanged?.Invoke( (WindowResolution)value.Value ) );

			_vsyncList = GetNode<OptionList>( "Basic/VSyncList" );
			_vsyncList.ValueSet.Subscribe( ( in value ) => VSyncModeChanged?.Invoke( (VSyncMode)value.Value ) );

			_maximumFramerate = GetNode<OptionSlider>( "Basic/MaxFpsSlider" );
			_maximumFramerate.ValueChanged.Subscribe( ( in value ) => MaximumFramerateChanged?.Invoke( (int)value.Value ) );
		}
	};
};
