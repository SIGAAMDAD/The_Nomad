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
using Nomad.Core.CVars;
using Nomad.Core.Engine.Services;
using Nomad.Core.Events;
using Nomad.Core.FileSystem;
using Nomad.Core.ServiceRegistry.Interfaces;
using Nomad.EngineUtils.Settings.Services;
using Nomad.Game.Domain.Interfaces.Gameplay;
using Nomad.Game.Presentation.Screens.ExtrasMenu;
using Nomad.Game.Presentation.Screens.MainMenu;
using Nomad.Game.Presentation.Screens.PauseMenu;
using Nomad.Game.Presentation.Screens.SettingsMenu;

namespace Nomad.Game.Presentation.Screens
{
	/*
	===================================================================================

	ScreenPresenterFactory

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal static class ScreenPresenterFactory
	{
		private static IServiceLocator _locator;

		public static void Initialize( IServiceLocator locator )
		{
			_locator = locator ?? throw new ArgumentNullException( nameof( locator ) );
		}

		/*
		===============
		CreatMainMenuPresenter
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="view"></param>
		/// <returns></returns>
		public static MainMenuPresenter CreateMainMenuPresenter( MainMenuView view )
		{
			var engineService = _locator.GetService<IEngineService>();
			var eventFactory = _locator.GetService<IGameEventRegistryService>();

			return new MainMenuPresenter(
				view,
				engineService,
				eventFactory
			);
		}

		/*
		===============
		CreateSettingsMenuPresenter
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="view"></param>
		/// <returns></returns>
		public static SettingsMenuPresenter CreateSettingsMenuPresenter( SettingsMenuView view )
		{
			var cvarSystem = _locator.GetService<ICVarSystemService>();
			var eventFactory = _locator.GetService<IGameEventRegistryService>();
			var fileSystem = _locator.GetService<IFileSystem>();
			var audioDevice = _locator.GetService<IAudioDevice>();
			var windowService = _locator.GetService<IWindowService>();
			var displayService = _locator.GetService<IDisplayService>();

			var sections = new ISettingsSectionPresenter[] {
				new AudioSettingsContainerPresenter(
					view.AudioView,
					new AudioSettingsContainerModel(
						audioDevice,
						new AudioSettingsService( audioDevice, cvarSystem ),
						cvarSystem
					)
				),
				new DisplaySettingsContainerPresenter(
					view.DisplayView,
					new DisplaySettingsContainerModel(
						windowService,
						new DisplaySettingsService(
							displayService,
							cvarSystem
						)
					)
				)
			};

			return new SettingsMenuPresenter(
				view,
				cvarSystem,
				eventFactory,
				fileSystem,
				sections
			);
		}

		/*
		===============
		CreateExtrasMenuPresenter
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="view"></param>
		/// <returns></returns>
		public static ExtrasMenuPresenter CreateExtrasMenuPresenter( ExtrasMenuView view )
		{
			var eventFactory = _locator.GetService<IGameEventRegistryService>();

			return new ExtrasMenuPresenter(
				view,
				eventFactory
			);
		}

		/*
		===============
		CreatePauseMenuView
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="view"></param>
		/// <returns></returns>
		public static PauseMenuPresenter CreatePauseMenuPresenter( PauseMenuView view )
		{
			var emitterFactory = _locator.GetService<IEmitterFactory>();
			var engineService = _locator.GetService<IEngineService>();
			var gameStateService = _locator.GetService<IGameStateService>();
			var eventFactory = _locator.GetService<IGameEventRegistryService>();
			var pauseService = _locator.GetService<IGamePauseService>();

			return new PauseMenuPresenter(
				view,
				new PauseMenuModel( emitterFactory ),
				engineService,
				eventFactory,
				gameStateService,
				pauseService
			);
		}
	};
};
