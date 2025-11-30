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

using Game.Application.Common.Interfaces;
using Game.Application.Configuration.Enums;
using Game.Infrastructure.UI.NomadUI.Events;
using Game.Infrastructure.UI.NomadUI.SelectionNodes;
using Game.Infrastructure.UI.NomadUI.SelectionNodes.OptionList;
using Godot;
using NomadCore.Systems.EventSystem.Common;
using System.Collections.Generic;

namespace Game.Presentation.Screens.SettingsMenu {
	/*
	===================================================================================
	
	SettingsView
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public class SettingsView : ISettingsView {
		private readonly Control _owner;

		private readonly OptionList _vsyncModeBasic;
		private readonly OptionList _windowModeBasic;
		private readonly OptionList _antiAliasingBasic;
		private readonly OptionList _maxFpsBasic;

		private readonly OptionList _monitor;
		private readonly OptionList _vsyncMode;
		private readonly OptionList _windowMode;
		private readonly OptionList _antiAliasing;
		private readonly OptionList _maxFps;
		private readonly OptionList _performanceOverlay;

		private readonly OptionList _windowResolutionList;

		public SettingsView( Control owner ) {
			_owner = owner;

			_vsyncModeBasic = owner.GetNode<OptionList>( "%VSyncModeListBasic" );
			_windowModeBasic = owner.GetNode<OptionList>( "%WindowModeListBasic" );
			_antiAliasingBasic = owner.GetNode<OptionList>( "%AntiAliasingListBasic" );
			_maxFpsBasic = owner.GetNode<OptionList>( "%MaxFpsListBasic" );

			_monitor = owner.GetNode<OptionList>( "%MonitorList" );
			_vsyncMode = owner.GetNode<OptionList>( "%VSyncModeList" );
			_windowMode = owner.GetNode<OptionList>( "%WindowModeList" );
			_antiAliasing = owner.GetNode<OptionList>( "%AntiAliasingList" );
			_maxFps = owner.GetNode<OptionList>( "%MaxFpsList" );
			_performanceOverlay = owner.GetNode<OptionList>( "%PerformanceOverlayList" );

			_windowResolutionList = owner.GetNode<OptionList>( "%WindowResolutionList" );
		}

		private void OnMonitorChanged() {

		}
	};
};