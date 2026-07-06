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

using Nomad.Core.Events;
using Nomad.Core.Engine.SceneManagement;
using Nomad.Core.Engine.Globals;
using Nomad.Core.Engine.Services;
using System;
using System.Collections.Generic;
using Godot;
using Nomad.Game.Sdk.Gameplay;
using Nomad.Game.Sdk.Events.Gameplay;
using Nomad.EngineUtils;
using Nomad.Core.Logger;

namespace Nomad.Game.Presentation.Menus
{
	/*
	===================================================================================

	MenuManager

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed class MenuManager : IDisposable
	{
		private const float FADE_TIME = 0.75f;
		private const string SHADER_VARIABLE_PROGRESS_NAME = "progress";

		private MenuState _currentState = MenuState.None;
		private MenuState _previousState = MenuState.None;

		private readonly Dictionary<MenuState, string> _scenePaths = new Dictionary<MenuState, string>() {
			[MenuState.Splash] = EngineService.GetStoragePath( "Source/Game/Presentation/Screens/SplashScreen/SplashScreen.tscn", StorageScope.Install ),
			[MenuState.Main] = EngineService.GetStoragePath( "Source/Game/Presentation/Screens/MainMenu/MainMenu.tscn", StorageScope.Install ),
			[MenuState.Extras] = EngineService.GetStoragePath( "Source/Game/Presentation/Screens/ExtrasMenu/ExtrasMenu.tscn", StorageScope.Install ),
			[MenuState.Multiplayer] = EngineService.GetStoragePath( "Source/Game/Presentation/Screens/MultiplayerMenu/MultiplayerMenu.tscn", StorageScope.Install ),
			[MenuState.LobbyWaitingRoom] = EngineService.GetStoragePath( "Source/Game/Presentation/Screens/LobbyWaitingRoom/LobbyWaitingRoom.tscn", StorageScope.Install ),
			[MenuState.Loading] = EngineService.GetStoragePath( "Source/Game/Presentation/Screens/LoadingScreen/LoadingScreen.tscn", StorageScope.Install ),
			[MenuState.Settings] = EngineService.GetStoragePath( "Source/Game/Presentation/Screens/SettingsMenu/SettingsMenu.tscn", StorageScope.Install ),
			[MenuState.NewGame] = EngineService.GetStoragePath( "Source/Game/Presentation/Screens/NewGameMenu/NewGameMenu.tscn", StorageScope.Install ),
			[MenuState.LoadGame] = EngineService.GetStoragePath( "Source/Game/Presentation/Screens/LoadGameMenu/LoadGameMenu.tscn", StorageScope.Install ),
			[MenuState.Pause] = EngineService.GetStoragePath( "Source/Game/Presentation/Screens/PauseMenu/PauseMenu.tscn", StorageScope.Install )
		};

		private readonly IGameEventRegistryService _eventFactory;
		private readonly ISceneManager _sceneManager;
		private readonly IGameStateService _gameStateService;
		private readonly ILoggerService _logger;

		[Event( nameSpace: "Nomad.Game.Presentation.Menus" )]
		[EventPayload( "PreviousState", typeof( MenuState ), Order = 1 )]
		[EventPayload( "CurrentState", typeof( MenuState ), Order = 2 )]
		public IGameEvent<MenuTransitionCompletedEventArgs> MenuTransitionCompleted => _menuTransitionCompleted;
		private readonly IGameEvent<MenuTransitionCompletedEventArgs> _menuTransitionCompleted = null;

		[Event( nameSpace: "Nomad.Game.Presentation.Menus" )]
		[EventPayload( "FromState", typeof( MenuState ), Order = 1 )]
		[EventPayload( "ToState", typeof( MenuState ), Order = 2 )]
		public IGameEvent<MenuTransitionRequestedEventArgs> MenuTransitionRequested => _menuTransitionRequested;
		private readonly IGameEvent<MenuTransitionRequestedEventArgs> _menuTransitionRequested = null;

		private IScene? _currentMenu;
		private readonly IScene _menuHubPrefab;

		private bool _isDisposed = false;

		/*
		===============
		MenuManager
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="sceneManager"></param>
		/// <param name="gameStateService"></param>
		/// <param name="eventFactory"></param>
		/// <param name="logger"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public MenuManager( ISceneManager sceneManager, IGameStateService gameStateService, IGameEventRegistryService eventFactory, ILoggerService logger )
		{
			_sceneManager = sceneManager ?? throw new ArgumentNullException( nameof( sceneManager ) );
			_eventFactory = eventFactory ?? throw new ArgumentNullException( nameof( eventFactory ) );
			_gameStateService = gameStateService ?? throw new ArgumentNullException( nameof( gameStateService ) );
			_logger = logger ?? throw new ArgumentNullException( nameof( logger ) );

			_menuTransitionRequested = _eventFactory
				.GetEvent<MenuTransitionRequestedEventArgs>(
					MenuTransitionRequestedEventArgs.Name,
					MenuTransitionRequestedEventArgs.NameSpace
				);
			_menuTransitionRequested.Subscribe( OnMenuTransitionRequested );

			_menuTransitionCompleted = _eventFactory
				.GetEvent<MenuTransitionCompletedEventArgs>(
					MenuTransitionCompletedEventArgs.Name,
					MenuTransitionCompletedEventArgs.NameSpace
				);

			_eventFactory
				.GetEvent<WorldBootstrapSucceededEventArgs>(
					WorldBootstrapSucceededEventArgs.Name,
					WorldBootstrapSucceededEventArgs.NameSpace
				)
				.Subscribe( OnWorldBootstrapSucceeded );

			_menuHubPrefab = _sceneManager.LoadPrefab( EngineService.GetStoragePath( "Source/Game/Presentation/Screens/MenuHub/MenuHub.tscn", StorageScope.Install ) );
			_gameStateService.StateChanged.Subscribe( OnGameStateChanged );

			ActivateStandaloneMenuHub();
			TransitionToMenu( MenuState.Splash );
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

			_menuTransitionRequested.Unsubscribe( OnMenuTransitionRequested );

			_eventFactory
				.GetEvent<WorldBootstrapSucceededEventArgs>(
					WorldBootstrapSucceededEventArgs.Name,
					WorldBootstrapSucceededEventArgs.NameSpace
				)
				.Unsubscribe( OnWorldBootstrapSucceeded );

			GC.SuppressFinalize( this );
			_isDisposed = true;
		}

		/*
		===============
		TransitionToMenu
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="newState"></param>
		/// <returns></returns>
		public void TransitionToMenu( MenuState newState )
		{
			newState = ResolveRequestedState( newState );

			if ( newState == MenuState.None ) {
				return;
			}
			if ( _currentState == newState ) {
				return;
			}
			if ( !_scenePaths.ContainsKey( newState ) ) {
				_logger.PrintError( $"Menu state '{newState}' does not have a registered scene path yet." );
				return;
			}

			_previousState = _currentState;
			TransitionFrom( newState );
			_currentState = newState;

			_menuTransitionCompleted.Publish( new MenuTransitionCompletedEventArgs( _currentState, _previousState ) );
		}

		/*
		===============
		OnMenuTransitionRequested
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnMenuTransitionRequested( in MenuTransitionRequestedEventArgs args )
		{
			var toState = args.ToState;

			// we might be calling this from a non ui thread, so make it deferred.
			Callable.From( () => TransitionToMenu( toState ) ).CallDeferred();
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
			switch ( args.CurrentState ) {
				case GameState.Paused:
					AttachMenuHubToActiveScene();
					SetMenuHubVisible( true );
					TransitionToMenu( MenuState.Pause );
					break;
				case GameState.Menu:
					ActivateStandaloneMenuHub();
					SetMenuHubVisible( true );
					TransitionToMenu( MenuState.Main );
					break;
				case GameState.Loading:
					SetMenuHubVisible( true );
					TransitionToMenu( MenuState.Loading );
					break;
				case GameState.Level:
					ClearCurrentMenu( resetState: true );
					SetMenuHubVisible( false );
					break;
			}
		}

		/*
		===============
		OnWorldBootstrapSucceeded
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnWorldBootstrapSucceeded( in WorldBootstrapSucceededEventArgs args )
		{
			AttachMenuHubToActiveScene();
			SetMenuHubVisible( false );
		}

		/*
		===============
		TransitionFrom
		===============
		*/
		/// <summary>
		/// Animates the transition between two menu screens.
		/// </summary>
		/// <param name="newState">The menu state to transition from.</param>
		private void TransitionFrom( MenuState newState )
		{
			if ( _currentMenu == null ) {
				TransitionTo( newState );
				return;
			}

			IScene oldMenu = _currentMenu;
			if ( TryAnimateMenu( oldMenu, 1.0f, 0.0f, () => {
				CloseMenuScene( oldMenu );
				TransitionTo( newState );
			} ) ) {
				return;
			}

			CloseMenuScene( oldMenu );
			TransitionTo( newState );
		}

		/*
		===============
		TransitionTo
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="newState"></param>
		private void TransitionTo( MenuState newState )
		{
			var toScene = _sceneManager.LoadPrefab( _scenePaths[newState] );
			var root = GetSceneRootNode( toScene );

			AttachNodeToMenuHub( root );
			_currentMenu = toScene;
			SetMenuHubVisible( true );

			TryAnimateMenu( toScene, 0.0f, 1.0f, null );
		}

		private MenuState ResolveRequestedState( MenuState requestedState )
		{
			if ( requestedState != MenuState.None ) {
				return requestedState;
			}
			if ( _previousState != MenuState.None ) {
				return _previousState;
			}

			return _gameStateService.Current == GameState.Paused ? MenuState.Pause : MenuState.Main;
		}

		private void ActivateStandaloneMenuHub()
		{
			_sceneManager.SetActiveScene( _menuHubPrefab );
		}

		private void AttachMenuHubToActiveScene()
		{
			if ( _sceneManager.ActiveScene == null ) {
				return;
			}

			AttachMenuHubTo( GetSceneRootNode( _sceneManager.ActiveScene ) );
		}

		private void AttachMenuHubTo( Node parent )
		{
			Node menuHubRoot = GetSceneRootNode( _menuHubPrefab );
			Node? currentParent = menuHubRoot.GetParent();

			if ( currentParent == parent ) {
				return;
			}

			currentParent?.RemoveChild( menuHubRoot );
			parent.AddChild( menuHubRoot );
		}

		private void AttachNodeToMenuHub( Node child )
		{
			Node menuHubRoot = GetSceneRootNode( _menuHubPrefab );
			Node? currentParent = child.GetParent();

			if ( currentParent == menuHubRoot ) {
				return;
			}

			currentParent?.RemoveChild( child );
			menuHubRoot.AddChild( child );
		}

		private void ClearCurrentMenu( bool resetState )
		{
			if ( _currentMenu != null ) {
				CloseMenuScene( _currentMenu );
			}

			if ( resetState ) {
				_previousState = _currentState;
				_currentState = MenuState.None;
			}
		}

		private void CloseMenuScene( IScene scene )
		{
			Node root = GetSceneRootNode( scene );
			Node? parent = root.GetParent();

			parent?.RemoveChild( root );
			root.QueueFree();
			scene.Dispose();

			if ( ReferenceEquals( _currentMenu, scene ) ) {
				_currentMenu = null;
			}
		}

		private void SetMenuHubVisible( bool visible )
		{
			if ( GetSceneRootNode( _menuHubPrefab ) is CanvasItem canvasItem ) {
				canvasItem.Visible = visible;
			}
		}

		private static bool TryAnimateMenu( IScene scene, float fromValue, float toValue, Action? onFinished )
		{
			if ( FindTransitionSurface( GetSceneRootNode( scene ) ) is not CanvasItem surface ) {
				return false;
			}
			if ( surface.Material is not ShaderMaterial shader ) {
				return false;
			}

			try {
				shader.SetShaderParameter( SHADER_VARIABLE_PROGRESS_NAME, fromValue );
			} catch ( Exception ) {
				return false;
			}

			Tween tween = surface.CreateTween();
			tween.TweenMethod(
				Callable.From<float>( value => shader.SetShaderParameter( SHADER_VARIABLE_PROGRESS_NAME, value ) ),
				fromValue,
				toValue,
				FADE_TIME
			)
			.SetTrans( Tween.TransitionType.Linear );

			if ( onFinished != null ) {
				tween.Connect( Tween.SignalName.Finished, Callable.From( onFinished ) );
			}

			return true;
		}

		private static CanvasItem? FindTransitionSurface( Node node )
		{
			if ( node is CanvasItem canvasItem && canvasItem.Material is ShaderMaterial ) {
				return canvasItem;
			}

			var children = node.GetChildren();
			for ( int i = 0; i < children.Count; i++ ) {
				if ( children[i] is not Node childNode ) {
					continue;
				}

				CanvasItem? result = FindTransitionSurface( childNode );
				if ( result != null ) {
					return result;
				}
			}

			return null;
		}

		private static Node GetSceneRootNode( IScene scene )
		{
			if ( scene.Root is not GodotGameObject root ) {
				throw new InvalidCastException( $"Expected a Godot scene root for '{scene.Name}'." );
			}

			return root.Node;
		}
	};
};
