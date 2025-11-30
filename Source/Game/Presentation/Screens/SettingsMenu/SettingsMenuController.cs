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
using Game.Infrastructure.UI.NomadUI.SelectionNodes.Interfaces;
using Game.Infrastructure.UI.NomadUI.SelectionNodes.OptionCheckbox;
using Game.Infrastructure.UI.NomadUI.SelectionNodes.OptionList;
using Godot;
using NomadCore.Abstractions.Services;

namespace Game.Presentation.Screens.SettingsMenu {
	/*
	===================================================================================
	
	SettingsMenuController
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public sealed class SettingsMenuController : ISettingsScreenController {
		private readonly IGraphicsService _graphicsService;
		private readonly IDisplayService _displayService;
		private readonly ICVarSystemService _cvarSystem;

		private OptionListController _windowModeController;
		private OptionListController _windowResolutionController;

		/*
		===============
		SettingsMenuController
		===============
		*/
		public SettingsMenuController( ICVarSystemService cvarSystem, IGraphicsService graphicsService, IDisplayService displayService ) {
			_cvarSystem = cvarSystem;
			_graphicsService = graphicsService;
			_displayService = displayService;
		}

		public void InitializeAntiAliasing( OptionList basicList, OptionList advancedList ) {
			throw new System.NotImplementedException();
		}

		public void InitializeMaxFps( OptionList basicList, OptionList advancedList ) {
			throw new System.NotImplementedException();
		}

		public void InitializeSeparateRenderingThread( OptionCheckbox checkbox ) {
			throw new System.NotImplementedException();
		}

		public void InitializeVSyncModes( OptionList basicList, OptionList advancedList ) {
			throw new System.NotImplementedException();
		}

		public void InitializeWindowModes( OptionList basicList, OptionList advancedList ) {
			throw new System.NotImplementedException();
		}

		/*
		===============
		InitializeWindowResolutions
		===============
		*/
		public void InitializeWindowResolutions( OptionList resolutionList ) {
			string[] resolutions = _displayService.GetSupportedResolutions();
			int current = WindowResolution.Res_640x480 - _displayService.GetCurrentConfig().WindowResolution;

			_windowResolutionController = new OptionListController(
				resolutionList,
				(IOptionListView)resolutionList.View,
				resolutions,
				current
			);
		}
	};
};