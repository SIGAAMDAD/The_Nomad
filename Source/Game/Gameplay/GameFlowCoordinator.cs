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
using Nomad.Core.Compatibility.Guards;
using Nomad.Core.CVars;
using Nomad.Core.Events;
using Nomad.Core.Logger;
using Nomad.CVars;
using Nomad.Game.Sdk.Gameplay;
using Nomad.Game.Sdk.Events.Gameplay;

namespace Nomad.Game.Gameplay
{
	/*
	===================================================================================
	
	GameFlowCoordinator
	
	===================================================================================
	*/
	/// <summary>
	/// A coordination service meant to intercept gameplay state changes, load scenes, and tell the program
	/// what to do when certain game modes are applied.
	/// </summary>

	internal sealed class GameFlowCoordinator : IGameFlowCoordinator
	{
		private readonly ILoggerCategory _category;
		private readonly IGameStateService _gameStateService;
		private readonly IGameEventRegistryService _eventFactory;

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
		/// <param name="gameStateService"></param>
		/// <param name="cvarSystem"></param>
		/// <param name="logger"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public GameFlowCoordinator( IGameEventRegistryService eventFactory, IGameStateService gameStateService, ICVarSystemService cvarSystem, ILoggerService logger )
		{
			ArgumentGuard.ThrowIfNull( logger, nameof( logger ) );
			ArgumentGuard.ThrowIfNull( cvarSystem, nameof( cvarSystem ) );

			_eventFactory = eventFactory ?? throw new ArgumentNullException( nameof( eventFactory ) );

			_eventFactory
				.GetEvent<WorldBootstrapFailureEventArgs>( WorldBootstrapFailureEventArgs.Name, WorldBootstrapFailureEventArgs.NameSpace )
				.Subscribe( OnWorldBootstrapFailed );

			_eventFactory
				.GetEvent<WorldBootstrapSucceededEventArgs>( WorldBootstrapSucceededEventArgs.Name, WorldBootstrapSucceededEventArgs.NameSpace )
				.Subscribe( OnWorldBootstrapSucceeded );

			_gameStateService = gameStateService ?? throw new ArgumentNullException( nameof( gameStateService ) );
			_category = logger.CreateCategory( nameof( GameFlowCoordinator ), LogLevel.Info, true );
			_gameMode = cvarSystem.GetCVarOrThrow( GameplayCVarRegistry.Mode );
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

			_eventFactory
				.GetEvent<WorldBootstrapFailureEventArgs>( WorldBootstrapFailureEventArgs.Name, WorldBootstrapFailureEventArgs.NameSpace )
				.Unsubscribe( OnWorldBootstrapFailed );

			_eventFactory
				.GetEvent<WorldBootstrapSucceededEventArgs>( WorldBootstrapSucceededEventArgs.Name, WorldBootstrapSucceededEventArgs.NameSpace )
				.Unsubscribe( OnWorldBootstrapSucceeded );

			_category?.Dispose();

			GC.SuppressFinalize( this );
			_isDisposed = true;
		}

		/*
		===============
		OnWorldBootstrapFailed
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnWorldBootstrapFailed( in WorldBootstrapFailureEventArgs args )
		{
			_category.PrintError(
				$"World bootstrap failed for '{args.WorldId}'. Reason='{args.Reason}', Detail='{args.Detail}'"
			);
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
			_gameMode.Value = MapGameplayMode( args.Mode );
			_gameStateService.Current = GameState.Level;
			_category.PrintLine(
				$"World bootstrap succeeded for '{args.WorldId}' (WorldInstanceId={args.WorldInstanceId})"
			);
		}

		/*
		===============
		MapGameplayMode
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="mode"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		private static GameplayMode MapGameplayMode( WorldBootstrapMode mode )
		{
			return mode switch {
				WorldBootstrapMode.SinglePlayerNewGame => GameplayMode.Single,
				WorldBootstrapMode.SinglePlayerLoadGame => GameplayMode.Single,
				WorldBootstrapMode.MultiplayerHost => GameplayMode.Network,
				WorldBootstrapMode.MultiplayerClient => GameplayMode.Network,
				_ => throw new ArgumentOutOfRangeException( nameof( mode ), mode, null )
			};
		}
	};
};
