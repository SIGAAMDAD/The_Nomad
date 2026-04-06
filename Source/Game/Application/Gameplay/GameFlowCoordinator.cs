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
using System.Collections.Generic;
using Nomad.Core.Compatibility.Guards;
using Nomad.Core.CVars;
using Nomad.Core.Engine.SceneManagement;
using Nomad.Core.Events;
using Nomad.Core.Logger;
using Nomad.CVars;
using Nomad.Game.Domain.Data.Gameplay;
using Nomad.Game.Domain.Events.Gameplay;
using Nomad.Game.Domain.Interfaces.Gameplay;

namespace Nomad.Game.Application.Gameplay {
	/*
	===================================================================================
	
	GameFlowCoordinator
	
	===================================================================================
	*/
	/// <summary>
	/// A coordination service meant to intercept gameplay state changes, load scenes, and tell the program
	/// what to do when certain game modes are applied.
	/// </summary>
	
	internal sealed class GameFlowCoordinator : IGameFlowCoordinator {
		private sealed record GameplayStartDescriptor(
			string ScenePath,
			GameState TargetState
		);

		private static readonly Dictionary<GameplayMode, GameplayStartDescriptor> _worldScenePaths = new Dictionary<GameplayMode, GameplayStartDescriptor> {
			[ GameplayMode.Single ] = new( "Assets/Prefabs/SingleWorld/SingleWorld.tscn", GameState.Level ),
			[ GameplayMode.Network ] = new( "Assets/Prefabs/NetworkWorld/NetworkWorld.tscn", GameState.Level ),
		};
		
		private readonly ISubscriptionHandle _beginGameRequested;

		private readonly ISceneManager _sceneManager;
		private readonly ILoggerCategory _category;
		private readonly IGameStateService _gameStateService;
		private readonly ICVar<GameplayMode> _gameMode;

		private bool _isDisposed = false;
		
		/*
		===============
		GameFlowCoordinator
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="eventFactory"></param>
		/// <param name="cvarSystem"></param>
		/// <param name="logger"></param>
		/// <param name="sceneManager"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public GameFlowCoordinator( IGameEventRegistryService eventFactory, IGameStateService gameStateService, ICVarSystemService cvarSystem, ILoggerService logger, ISceneManager sceneManager ) {
			ArgumentGuard.ThrowIfNull( logger );
			ArgumentGuard.ThrowIfNull( cvarSystem );

			_beginGameRequested = eventFactory.GetEvent<BeginGameEventArgs>( EventNames.BEGIN_GAME, EventNames.NAMESPACE )
				.Subscribe( OnBeginGameRequested );
			
			_gameStateService = gameStateService ?? throw new ArgumentNullException( nameof( gameStateService ) );	
			_sceneManager = sceneManager ?? throw new ArgumentNullException( nameof( sceneManager ) );
			_category = logger.CreateCategory( nameof( GameFlowCoordinator ), LogLevel.Info, true );
			_gameMode = cvarSystem.GetCVarOrThrow<GameplayMode>( "game.Mode" );
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
			if ( !_isDisposed ) {
				_beginGameRequested?.Dispose();
				_category?.Dispose();
			}
			GC.SuppressFinalize( this );
			_isDisposed = true;
		}

		/*
		===============
		OnBeginGameRequested
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnBeginGameRequested( in BeginGameEventArgs args ) {
			if ( !_worldScenePaths.TryGetValue( args.Mode, out var descriptor ) ) {
				_category.PrintError( $"BeginGame requested with invalid GameMode '{args.Mode}'!" );
				return;
			}

			_gameMode.Value = args.Mode;
			_sceneManager.LoadScene( descriptor.ScenePath );
			_gameStateService.Current = GameState.Level;
		}
	};
};