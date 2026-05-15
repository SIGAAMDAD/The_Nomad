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
	internal sealed class MainMenuPresenter : IDisposable
	{
		private readonly IGameEventRegistryService _eventFactory;
		private readonly IEngineService _engineService;

		private readonly MainMenuView _view;

		private bool _isDisposed = false;

		public MainMenuPresenter( MainMenuView view, IEngineService engineService, IGameEventRegistryService eventFactory )
		{
			_view = view ?? throw new ArgumentNullException( nameof( view ) );
			_eventFactory = eventFactory ?? throw new ArgumentNullException( nameof( eventFactory ) );
			_engineService = engineService ?? throw new ArgumentNullException( nameof( engineService ) );

			_view.NewGame += OnNewGamePressed;
			_view.LoadGame += OnLoadGamePressed;
			_view.Extras += OnExtrasMenuPressed;
			_view.Settings += OnSettingsMenuPressed;
			_view.QuitGame += OnQuitGamePressed;
		}

		/*
		===============
		Dispose
		===============
		*/
		/// <summary>
		///
		/// </summary>
		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			_view.NewGame += OnNewGamePressed;
			_view.LoadGame += OnLoadGamePressed;
			_view.Extras += OnExtrasMenuPressed;
			_view.Settings += OnSettingsMenuPressed;
			_view.QuitGame += OnQuitGamePressed;

			GC.SuppressFinalize( this );
			_isDisposed = true;
		}

		/*
		===============
		OnNewGamePressed
		===============
		*/
		/// <summary>
		///
		/// </summary>
		private void OnNewGamePressed()
		{
			_eventFactory
				.GetEvent<MenuTransitionRequestedEventArgs>( MenuTransitionRequestedEventArgs.Name, MenuTransitionRequestedEventArgs.NameSpace )
				.Publish( new MenuTransitionRequestedEventArgs( MenuState.Main, MenuState.NewGame ) );
		}

		/*
		===============
		OnLoadGamePressed
		===============
		*/
		/// <summary>
		///
		/// </summary>
		private void OnLoadGamePressed()
		{
			_eventFactory
				.GetEvent<MenuTransitionRequestedEventArgs>( MenuTransitionRequestedEventArgs.Name, MenuTransitionRequestedEventArgs.NameSpace )
				.Publish( new MenuTransitionRequestedEventArgs( MenuState.Main, MenuState.LoadGame ) );
		}

		/*
		===============
		OnSettingsMenuPressed
		===============
		*/
		/// <summary>
		///
		/// </summary>
		private void OnSettingsMenuPressed()
		{
			_eventFactory
				.GetEvent<MenuTransitionRequestedEventArgs>( MenuTransitionRequestedEventArgs.Name, MenuTransitionRequestedEventArgs.NameSpace )
				.Publish( new MenuTransitionRequestedEventArgs( MenuState.Main, MenuState.Settings ) );
		}

		/*
		===============
		OnExtrasMenuPressed
		===============
		*/
		/// <summary>
		///
		/// </summary>
		private void OnExtrasMenuPressed()
		{
			_eventFactory
				.GetEvent<MenuTransitionRequestedEventArgs>( MenuTransitionRequestedEventArgs.Name, MenuTransitionRequestedEventArgs.NameSpace )
				.Publish( new MenuTransitionRequestedEventArgs( MenuState.Main, MenuState.Extras ) );
		}

		/*
		===============
		OnQuitGamePressed
		===============
		*/
		/// <summary>
		///
		/// </summary>
		private void OnQuitGamePressed()
		{
			_engineService.Quit();
		}
	};
};
