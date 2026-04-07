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
using System.Threading.Tasks;
using Nomad.Core.Compatibility.Guards;
using Nomad.Core.CVars;
using Nomad.Core.Engine.SceneManagement;
using Nomad.Core.Events;
using Nomad.Core.Logger;
using Nomad.CVars;
using Nomad.Game.Application.Configuration.Enums.Gameplay;
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
		private readonly ILoggerCategory _category;
		private readonly IGameStateService _gameStateService;
		private readonly IGameEventRegistryService _eventFactory;
		private readonly IWorldBootstrapper _worldBootstrapper;

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
		/// <param name="boostrapper"></param>
		/// <param name="eventFactory"></param>
		/// <param name="gameStateService"></param>
		/// <param name="cvarSystem"></param>
		/// <param name="logger"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public GameFlowCoordinator( IWorldBootstrapper boostrapper, IGameEventRegistryService eventFactory, IGameStateService gameStateService, ICVarSystemService cvarSystem, ILoggerService logger ) {
			ArgumentGuard.ThrowIfNull( logger );
			ArgumentGuard.ThrowIfNull( cvarSystem );

			_worldBootstrapper = boostrapper ?? throw new ArgumentNullException( nameof( boostrapper ) );
			_eventFactory = eventFactory ?? throw new ArgumentNullException( nameof( eventFactory ) );

			_eventFactory
				.GetEvent<WorldBootstrapRequestEventArgs>( EventNames.WORLD_BOOTSTRAP_REQUESTED, EventNames.NAMESPACE )
				.Subscribe( OnWorldBootstrapRequested );
			
			_gameStateService = gameStateService ?? throw new ArgumentNullException( nameof( gameStateService ) );
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
				_eventFactory
					.GetEvent<WorldBootstrapRequestEventArgs>( EventNames.WORLD_BOOTSTRAP_REQUESTED, EventNames.NAMESPACE )
					.Unsubscribe( OnWorldBootstrapRequested );
				
				_category?.Dispose();
			}
			GC.SuppressFinalize( this );
			_isDisposed = true;
		}

		private void OnWorldBootstrapRequested( in WorldBootstrapRequestEventArgs args ) {
			_category.PrintLine( "Loading world..." );
			HandleWorldBootstrapRequested( in args );
		}

		private void HandleWorldBootstrapRequested( in WorldBootstrapRequestEventArgs args ) {
			try {
				WorldBootstrapResult result = _worldBootstrapper.Bootstrap( args );
				switch ( result ) {
					case WorldBootstrapSuccess success:
						_gameMode.Value = MapGameplayMode( success.Mode );
						_gameStateService.Current = GameState.Level;
						_category.PrintLine(
							$"World bootstrap succeeded for '{success.WorldId}' (WorldInstanceId={success.WorldInstanceId})"
						);
						break;
					case WorldBootstrapFailure failure:
						_category.PrintError(
							$"World bootstrap failed for '{failure.WorldId}'. Reason='{failure.Reason}', Detail='{failure.Detail}'"
						);
						break;
					default:
						_category.PrintError( "Unknown world bootstrap result received." );
						break;
				}
			} catch ( Exception e ) {
				_category.PrintError( $"Unhandled bootstrap exception: {e}" );
				throw;
			}
		}

		private static GameplayMode MapGameplayMode( WorldBootstrapMode mode ) {
			return mode switch {
				WorldBootstrapMode.SinglePlayer => GameplayMode.Single,
				WorldBootstrapMode.MultiplayerHost => GameplayMode.Network,
				WorldBootstrapMode.MultiplayerClient => GameplayMode.Network,
				_ => throw new ArgumentOutOfRangeException( nameof( mode ), mode, null )
			};
		}
	};
};