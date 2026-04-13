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

using Nomad.Game.Application.UI.Menus.Events;
using Nomad.Core.Events;
using Nomad.Core.Engine.SceneManagement;
using Nomad.Core.Engine.Globals;
using Nomad.Core.Engine.Services;
using System;
using System.Collections.Generic;
using Godot;
using Nomad.UI;

namespace Nomad.Game.Application.UI.Menus {
	/*
	===================================================================================
	
	MenuManager
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public sealed class MenuManager : IDisposable {
		private const float FADE_TIME = 0.75f;
		private static readonly StringName @ShaderVariableProgressName = "progress";

		private MenuState _currentState = MenuState.None;
		private MenuState _previousState = MenuState.None;

		private readonly Dictionary<MenuState, string> _scenePaths = new Dictionary<MenuState, string>() {
			[MenuState.Splash] = EngineService.GetStoragePath( "Prefabs/SplashScreen/SplashScreen.tscn", StorageScope.StreamingAssets ),
			[MenuState.Main] = EngineService.GetStoragePath( "Source/Game/Presentation/Screens/MainMenu/MainMenu.tscn", StorageScope.Install ),
			[MenuState.Extras] = EngineService.GetStoragePath( "Source/Game/Presentation/Screens/ExtrasMenu/ExtrasMenu.tscn", StorageScope.Install ),
			[MenuState.Loading] = EngineService.GetStoragePath( "Source/Game/Presentation/Screens/LoadingScreen/LoadingScreen.tscn", StorageScope.Install ),
			[MenuState.Settings] = EngineService.GetStoragePath( "Source/Game/Presentation/Screens/SettingsMenu/SettingsMenu.tscn", StorageScope.Install ),
			[MenuState.NewGame] = EngineService.GetStoragePath( "Source/Game/Presentation/Screens/NewGameMenu/NewGameMenu.tscn", StorageScope.Install )
		};

		private readonly IGameEventRegistryService _eventRegistry;

		private IScene? _currentScene;
		private readonly ISceneManager _sceneManager;

		private readonly Callable _setShaderProgressValue;

		private bool _isDiposed = false;

		/*
		===============
		MenuManager
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="sceneManager"></param>
		/// <param name="eventFactory"></param>
		public MenuManager( ISceneManager sceneManager, IGameEventRegistryService eventFactory ) {
			_sceneManager = sceneManager;
			_eventRegistry = eventFactory;

			eventFactory
				.GetEvent<MenuTransitionRequestedEventArgs>( UIConstants.MENU_TRANSITION_REQUESTED_EVENT, UIConstants.NAMESPACE )
				.Subscribe( OnMenuTransitionRequested );

			sceneManager.LoadScene( EngineService.GetStoragePath( "Source/Game/Presentation/Screens/MenuHub/MenuHub.tscn", StorageScope.Install ), LoadSceneMode.Single );
		}

		/*
		===============
		Dispose
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		public void Dispose() {
			if ( !_isDiposed ) {
				_eventRegistry
					.GetEvent<MenuTransitionRequestedEventArgs>( UIConstants.MENU_TRANSITION_REQUESTED_EVENT, UIConstants.NAMESPACE )
					.Subscribe( OnMenuTransitionRequested );
			}
			GC.SuppressFinalize( this );
			_isDiposed = true;
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
		public void TransitionToMenu( MenuState newState ) {
			if ( _currentState == newState ) {
				return;
			}

			_previousState = _currentState;
			TransitionFrom( newState );
			_currentState = newState;

			_eventRegistry
				.GetEvent<MenuTransitionCompletedEventArgs>( UIConstants.MENU_TRANSITION_COMPLETED_EVENT, UIConstants.NAMESPACE )
				.Publish( new MenuTransitionCompletedEventArgs( _currentState, _previousState ) );
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
		private void OnMenuTransitionRequested( in MenuTransitionRequestedEventArgs args ) {
			TransitionToMenu( args.ToState );
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
		private void TransitionFrom( MenuState newState ) {
			if ( _currentScene == null ) {
				// in the case that we are initializing just skip straight to the splashscreen.
				TransitionTo( MenuState.Splash );
				return;
			}
			var root = _currentScene.Root.CastAs<EnginePanel>();
			Tween tween = root.CreateTween();
			ShaderMaterial shader = root.Material as ShaderMaterial;

			shader.SetShaderParameter( ShaderVariableProgressName, 1.0f );
			tween.TweenMethod(
				Callable.From<float>( value => shader.SetShaderParameter( ShaderVariableProgressName, value ) ),
				1.0f,
				0.0f,
				FADE_TIME
			)
			.SetTrans( Tween.TransitionType.Linear );
			tween.Connect( Tween.SignalName.Finished, Callable.From( () => {
				_sceneManager.UnloadScene( _currentScene );
				TransitionTo( newState );
		 	} ) );
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
		private void TransitionTo( MenuState newState ) {
			var toScene = _sceneManager.LoadScene( _scenePaths[newState], LoadSceneMode.Additive );
			var root = toScene.Root.CastAs<EnginePanel>();
			Tween tween = root.CreateTween();
			ShaderMaterial shader = root.Material as ShaderMaterial;

			_currentScene = toScene;

			shader.SetShaderParameter( ShaderVariableProgressName, 0.0f );
			tween.TweenMethod(
				Callable.From<float>( value => shader.SetShaderParameter( ShaderVariableProgressName, value ) ),
				0.0f,
				1.0f,
				FADE_TIME
			)
			.SetTrans( Tween.TransitionType.Linear );
		}
	};
};
