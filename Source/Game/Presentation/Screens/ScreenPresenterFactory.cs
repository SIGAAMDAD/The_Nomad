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
using Nomad.Game.Sdk.Gameplay;
using Nomad.Game.Sdk.Multiplayer;
using Nomad.Game.Presentation.Screens.ExtrasMenu;
using Nomad.Game.Presentation.Screens.LobbyCreationMenu;
using Nomad.Game.Presentation.Screens.LobbyWaitingRoom;
using Nomad.Game.Presentation.Screens.MainMenu;
using Nomad.Game.Presentation.Screens.MultiplayerMenu;
using Nomad.Game.Presentation.Screens.PauseMenu;
using Nomad.Game.Presentation.Screens.SettingsMenu;
using Nomad.Game.Presentation.Screens.LoadGameMenu;
using Nomad.Save.Services;
using Nomad.Game.Presentation.Screens.NewGameMenu;
using Nomad.Networking.Session;
using Nomad.Game.Presentation.Screens.LoadingScreen;

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
		public static MainMenuPresenter CreateMainMenuPresenter( IMainMenuView view )
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
		public static SettingsMenuPresenter CreateSettingsMenuPresenter( SettingsMenuGodotView view )
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
		public static ExtrasMenuPresenter CreateExtrasMenuPresenter( IExtrasMenuView view )
		{
			var eventFactory = _locator.GetService<IGameEventRegistryService>();

			return new ExtrasMenuPresenter(
				view,
				eventFactory
			);
		}

		/*
		===============
		CreateLoadGameMenuPresenter
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="view"></param>
		/// <returns></returns>
		public static LoadGameMenuPresenter CreateLoadGameMenuPresenter( ILoadGameMenuView view )
		{
			var saveDataProvider = _locator.GetService<ISaveDataProvider>();
			var gameSessionService = _locator.GetService<IGameSessionService>();
			var eventFactory = _locator.GetService<IGameEventRegistryService>();

			return new LoadGameMenuPresenter(
				view,
				saveDataProvider,
				gameSessionService,
				eventFactory
			);
		}

		/*
		===============
		CreateNewGameMenuPresenter
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="view"></param>
		/// <returns></returns>
		public static NewGameMenuPresenter CreateNewGameMenuPresenter( INewGameMenuView view )
		{
			var gameSessionService = _locator.GetService<IGameSessionService>();
			var eventFactory = _locator.GetService<IGameEventRegistryService>();

			return new NewGameMenuPresenter(
				view,
				gameSessionService,
				eventFactory
			);
		}

		/*
		===============
		CreatePauseMenuPresenter
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="view"></param>
		/// <returns></returns>
		public static PauseMenuPresenter CreatePauseMenuPresenter( IPauseMenuView view )
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

		public static LobbyCreationMenuPresenter CreateLobbyCreationMenuPresenter( ILobbyCreationMenuView view )
		{
			var sessionService = _locator.GetService<INetworkSessionService>();
			var eventFactory = _locator.GetService<IGameEventRegistryService>();

			return new LobbyCreationMenuPresenter(
				view,
				new LobbyCreationMenuModel(),
				sessionService,
				eventFactory
			);
		}

		public static MultiplayerMenuPresenter CreateMultiplayerMenuPresenter( IMultiplayerMenuView view )
		{
			var eventFactory = _locator.GetService<IGameEventRegistryService>();

			return new MultiplayerMenuPresenter(
				view,
				new MultiplayerMenuModel(),
				eventFactory
			);
		}

		public static LobbyWaitingRoomPresenter CreateLobbyWaitingRoomPresenter( ILobbyWaitingRoomView view )
		{
			var sessionService = _locator.GetService<INetworkSessionService>();
			var votingService = _locator.GetService<IVotingService>();
			var waitingRoomService = _locator.GetService<ILobbyWaitingRoomService>();

			return new LobbyWaitingRoomPresenter(
				view,
				new LobbyWaitingRoomModel( sessionService ),
				sessionService,
				votingService,
				waitingRoomService
			);
		}

		public static LoadingScreenPresenter CreateLoadingScreenPresenter( ILoadingScreenView view )
		{
			var eventFactory = _locator.GetService<IGameEventRegistryService>();

			return new LoadingScreenPresenter(
				view,
				eventFactory
			);
		}
	};
};
