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
using Nomad.Core.Input;
using Nomad.Core.Input.ValueObjects;
using Nomad.Game.Application.UI.Menus;
using Nomad.Game.Domain.Data.Gameplay;
using Nomad.Game.Domain.Events.Gameplay;
using Nomad.Game.Domain.Interfaces.Gameplay;

namespace Nomad.Game.Presentation.Screens.PauseMenu
{
	/*
	===================================================================================

	PauseMenuPresenter

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed class PauseMenuPresenter : IDisposable
	{
		private readonly PauseMenuView _view;
		private readonly PauseMenuModel _model;

		private readonly IGameStateService _gameStateService;
		private readonly IEngineService _engineService;
		private readonly IGamePauseService _pauseService;
		private readonly IGameEventRegistryService _eventFactory;
		private readonly IGameEvent<MenuTransitionRequestedEventArgs> _menuTransitionRequested;
		private readonly IGameEvent<WorldBootstrapRequestEventArgs> _worldBootstrapRequested;
		private readonly IGameEvent<KeyboardEventArgs> _keyboardEvent;

		private bool _isDisposed = false;

		/*
		===============
		PauseMenuPresenter
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="view"></param>
		/// <param name="model"></param>
		/// <param name="engineService"></param>
		/// <param name="eventFactory"></param>
		/// <param name="gameStateService"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public PauseMenuPresenter(
			PauseMenuView view,
			PauseMenuModel model,
			IEngineService engineService,
			IGameEventRegistryService eventFactory,
			IGameStateService gameStateService,
			IGamePauseService pauseService
		)
		{
			_view = view ?? throw new ArgumentNullException( nameof( view ) );
			_model = model ?? throw new ArgumentNullException( nameof( model ) );
			_gameStateService = gameStateService ?? throw new ArgumentNullException( nameof( gameStateService ) );
			_engineService = engineService ?? throw new ArgumentNullException( nameof( engineService ) );
			_pauseService = pauseService ?? throw new ArgumentNullException( nameof( pauseService ) );
			_eventFactory = eventFactory ?? throw new ArgumentNullException( nameof( eventFactory ) );

			_menuTransitionRequested = eventFactory.GetEvent<MenuTransitionRequestedEventArgs>( MenuTransitionRequestedEventArgs.Name, MenuTransitionRequestedEventArgs.NameSpace );
			_worldBootstrapRequested = eventFactory.GetEvent<WorldBootstrapRequestEventArgs>( WorldBootstrapRequestEventArgs.Name, WorldBootstrapRequestEventArgs.NameSpace );
			_keyboardEvent = eventFactory.GetEvent<KeyboardEventArgs>( KeyboardEventArgs.Name, KeyboardEventArgs.NameSpace );

			_gameStateService.StateChanged.Subscribe( OnGameStateChanged );

			_view.Resume += OnResumeGame;
			_view.LoadGame += OnLoadGame;
			_view.SettingsMenu += OnOpenSettingsMenu;
			_view.QuitGame += OnQuitGame;
			_view.QuitToMainMenu += OnQuitToMainMenu;

			_keyboardEvent.Subscribe( OnKeyboardEvent );

			SyncFromGameState( _gameStateService.Current );
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

			_gameStateService.StateChanged.Unsubscribe( OnGameStateChanged );
			_keyboardEvent.Unsubscribe( OnKeyboardEvent );

			_view.Resume -= OnResumeGame;
			_view.LoadGame -= OnLoadGame;
			_view.SettingsMenu -= OnOpenSettingsMenu;
			_view.QuitGame -= OnQuitGame;
			_view.QuitToMainMenu -= OnQuitToMainMenu;

			GC.SuppressFinalize( this );
			_isDisposed = true;
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
			_view.SetVisibility( _model.IsPaused );
		}

		/*
		===============
		OnGameStateChanged
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnGameStateChanged( in GameStateChangedEventArgs args )
		{
			SyncFromGameState( args.CurrentState );
		}

		private void SyncFromGameState( GameState state )
		{
			bool isPaused = state == GameState.Paused;
			_model.SetPaused( isPaused );
			_pauseService.SetPaused( isPaused );
			SyncView();
		}

		/*
		===============
		OnResumeGame
		===============
		*/
		/// <summary>
		///
		/// </summary>
		private void OnResumeGame()
		{
			_gameStateService.SetState( GameState.Level );
		}

		/*
		===============
		OnLoadGame
		===============
		*/
		/// <summary>
		///
		/// </summary>
		private void OnLoadGame()
		{
			_menuTransitionRequested.Publish( new MenuTransitionRequestedEventArgs( MenuState.Pause, MenuState.LoadGame ) );
		}

		/*
		===============
		OnOpenSettingsMenu
		===============
		*/
		/// <summary>
		///
		/// </summary>
		private void OnOpenSettingsMenu()
		{
			_menuTransitionRequested.Publish( new MenuTransitionRequestedEventArgs( MenuState.Pause, MenuState.Settings ) );
		}

		/*
		===============
		OnQuitGame
		===============
		*/
		/// <summary>
		///
		/// </summary>
		private void OnQuitGame()
		{
			_engineService.Quit();
		}

		/*
		===============
		OnQuitToMainMenu
		===============
		*/
		/// <summary>
		///
		/// </summary>
		private void OnQuitToMainMenu()
		{
			_gameStateService.SetState( GameState.Menu );
		}

		/*
		===============
		OnKeyboardEvent
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnKeyboardEvent( in KeyboardEventArgs args )
		{
			if ( args.Pressed && args.KeyNum == KeyNum.Escape ) {
				_gameStateService.SetState( _model.IsPaused ? GameState.Level : GameState.Paused );
			}
		}
	};
};
