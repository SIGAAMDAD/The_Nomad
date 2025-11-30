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
using Game.Application.Configuration;
using Game.Application.Configuration.Services;
using Game.Infrastructure.Configuration;
using Game.Infrastructure.Configuration.Godot;
using Game.Infrastructure.Configuration.Interfaces;
using Game.Infrastructure.UI.NomadUI.Menus;
using Game.Infrastructure.UI.NomadUI.SelectionNodes;
using Game.Infrastructure.UI.NomadUI.SelectionNodes.OptionList;
using NomadCore.Abstractions.Services;
using NomadCore.Infrastructure;
using NomadCore.Interfaces.EventSystem;

namespace Game.Presentation.Screens.SettingsMenu {
	/*
	===================================================================================
	
	SettingsMenu
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public sealed partial class SettingsMenu : BaseMenu {
		private readonly ISettingsPresetService _presetService = new SettingsPresetService(
			ServiceRegistry.Get<ICVarSystemService>(),
			new SettingsPresetRepository()
		);

		private readonly IGraphicsService _graphicsService;
		private readonly IDisplayService _displayService;
		private readonly ISettingsScreenController _controller;
		private readonly ICVarSystemService _cvarSystem;
		private readonly IDisplayConfig _displayConfig;
		private readonly ILoggerService _logger;

		/*
		===============
		SettingsMenu
		===============
		*/
		public SettingsMenu() {
			_cvarSystem = ServiceRegistry.Get<ICVarSystemService>() ?? throw new System.Exception( "CVar system isn't initialized yet" );
			_logger = ServiceRegistry.Get<ILoggerService>() ?? throw new System.Exception( "Logger system isn't initialized yet" );

			_displayConfig = new GodotDisplay( _cvarSystem, _logger );
			_graphicsService = new GraphicsService( _cvarSystem );
			_displayService = new DisplayService( _cvarSystem, _displayConfig );
			_controller = new SettingsMenuController( _cvarSystem, _graphicsService, _displayService );
		}

		/*
		===============
		OnSaveButtonPressed
		===============
		*/
		private void OnSaveButtonPressed( in UIEvent eventData, in IEventArgs args ) {
		}

		/*
		===============
		_Ready
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		public override void _Ready() {
			base._Ready();

			_controller.InitializeWindowResolutions( GetNode<OptionList>( "%WindowResolutionList" ) );
			_controller.InitializeAntiAliasing( GetNode<OptionList>( "%AntiAliasingListBasic" ), GetNode<OptionList>( "%AntiAliasingList" ) );
			_controller.InitializeMaxFps( GetNode<OptionList>( "%MaxFpsListBasic" ), GetNode<OptionList>( "%MaxFpsList" ) );
		}
	};
};