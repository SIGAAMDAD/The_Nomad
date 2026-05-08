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
using Nomad.Core.Engine.Services;
using Nomad.Core.Events;
using Nomad.Game.Application.UI.Menus;

namespace Nomad.Game.Presentation.Screens.MainMenu
{
	internal sealed class MainMenuPresenter
	{
		private readonly IGameEventRegistryService _eventFactory;
		private readonly IEngineService _engineService;

		public MainMenuPresenter( MainMenuView view, IEngineService engineService, IGameEventRegistryService eventFactory )
		{
			_eventFactory = eventFactory ?? throw new ArgumentNullException( nameof( eventFactory ) );
			_engineService = engineService ?? throw new ArgumentNullException( nameof( engineService ) );

			view.NewGame += OnNewGamePressed;
			view.LoadGame += OnLoadGamePressed;
			view.Extras += OnExtrasMenuPressed;
			view.Settings += OnSettingsMenuPressed;
			view.QuitGame += OnQuitGamePressed;
		}

		private void OnNewGamePressed()
		{
			_eventFactory
				.GetEvent<MenuTransitionRequestedEventArgs>( MenuTransitionRequestedEventArgs.Name, MenuTransitionRequestedEventArgs.NameSpace )
				.Publish( new MenuTransitionRequestedEventArgs( MenuState.Main, MenuState.NewGame ) );
		}

		private void OnLoadGamePressed()
		{
			_eventFactory
				.GetEvent<MenuTransitionRequestedEventArgs>( MenuTransitionRequestedEventArgs.Name, MenuTransitionRequestedEventArgs.NameSpace )
				.Publish( new MenuTransitionRequestedEventArgs( MenuState.Main, MenuState.LoadGame ) );
		}

		private void OnSettingsMenuPressed()
		{
			_eventFactory
				.GetEvent<MenuTransitionRequestedEventArgs>( MenuTransitionRequestedEventArgs.Name, MenuTransitionRequestedEventArgs.NameSpace )
				.Publish( new MenuTransitionRequestedEventArgs( MenuState.Main, MenuState.Settings ) );
		}

		private void OnExtrasMenuPressed()
		{
			_eventFactory
				.GetEvent<MenuTransitionRequestedEventArgs>( MenuTransitionRequestedEventArgs.Name, MenuTransitionRequestedEventArgs.NameSpace )
				.Publish( new MenuTransitionRequestedEventArgs( MenuState.Main, MenuState.Extras ) );
		}

		private void OnQuitGamePressed()
		{
			_engineService.Quit();
		}
	};
};
