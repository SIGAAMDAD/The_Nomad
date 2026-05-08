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
using Nomad.Core.Events;
using Nomad.Core.OnlineServices;
using Nomad.Game.Domain.Data.Gameplay;
using Nomad.Game.Domain.Events.Gameplay;
using Nomad.Game.Domain.Interfaces.Gameplay;
using Nomad.Game.Domain.Interfaces.Player;

namespace Nomad.Game.Application.Gameplay.World
{
	/*
	===================================================================================
	
	WorldBootstrapper
	
	===================================================================================
	*/
	/// <summary>
	/// Responsible for initializing and bootstrapping game worlds based on the specified mode.
	/// Handles the creation of new worlds, loading existing ones, and setting up multiplayer sessions.
	/// Publishes success or failure events depending on the outcome of the bootstrap process.
	/// </summary>

	internal sealed class WorldBootstrapper : IWorldBootstrapper
	{
		private readonly IWorldLoader _worldLoader;
		private readonly IPlayerSpawnService _spawnService;
		private readonly INetworkSessionService _networkSessionService;

		public IGameEvent<WorldBootstrapFailureEventArgs> BootstrapFailure => _bootstrapFailed;
		private readonly IGameEvent<WorldBootstrapFailureEventArgs> _bootstrapFailed;

		public IGameEvent<WorldBootstrapSucceededEventArgs> BootstrapSucceeded => _bootstrapSucceeded;
		private readonly IGameEvent<WorldBootstrapSucceededEventArgs> _bootstrapSucceeded;

		public IGameEvent<WorldBootstrapRequestEventArgs> BootstrapRequest => _bootstrapRequest;
		private readonly IGameEvent<WorldBootstrapRequestEventArgs> _bootstrapRequest;

		/*
		===============
		WorldBootstrapper
		===============
		*/
		/// <summary>
		/// Initializes a new instance of the <see cref="WorldBootstrapper"/> class.
		/// </summary>
		/// <param name="eventFactory">The service used to register and publish game events.</param>
		/// <param name="loader">The service responsible for loading world data.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="loader"/> is null.</exception>
		public WorldBootstrapper( IGameEventRegistryService eventFactory, IWorldLoader loader )
		{
			ArgumentGuard.ThrowIfNull( eventFactory, nameof( eventFactory ) );

			_worldLoader = loader ?? throw new ArgumentNullException( nameof( loader ) );
			_bootstrapFailed = eventFactory.GetEvent<WorldBootstrapFailureEventArgs>( WorldBootstrapFailureEventArgs.Name, WorldBootstrapFailureEventArgs.NameSpace );
			_bootstrapSucceeded = eventFactory.GetEvent<WorldBootstrapSucceededEventArgs>( WorldBootstrapSucceededEventArgs.Name, WorldBootstrapSucceededEventArgs.NameSpace );
			_bootstrapRequest = eventFactory.GetEvent<WorldBootstrapRequestEventArgs>( WorldBootstrapRequestEventArgs.Name, WorldBootstrapRequestEventArgs.NameSpace );
		}

		/*
		===============
		Bootstrap
		===============
		*/
		/// <summary>
		/// Bootstraps a game world based on the provided request parameters.
		/// Validates the request, loads the world, and handles mode-specific initialization.
		/// Publishes a success event on successful bootstrap or a failure event if an error occurs.
		/// </summary>
		/// <param name="request">The bootstrap request containing mode, world ID, and other parameters.</param>
		/// <returns>A <see cref="WorldBootstrapResult"/> indicating success or failure with details.</returns>
		private void OnBootstrapRequested( in WorldBootstrapRequestEventArgs args )
		{
			try {
				Validate( in args );

				Guid? resolvedLobbyId = null;

				switch ( args.Mode ) {
					case WorldBootstrapMode.SinglePlayerNewGame:
						break;
					case WorldBootstrapMode.SinglePlayerLoadGame:
						break;
					case WorldBootstrapMode.MultiplayerHost:
						break;
					case WorldBootstrapMode.MultiplayerClient:
						break;
					default:
						break;
				}
				IWorldHandle world = LoadWorld( args.WorldId );

				_bootstrapSucceeded.Publish( new WorldBootstrapSucceededEventArgs(
					args.RequestId,
					args.Mode,
					args.WorldId,
					world.Id,
					resolvedLobbyId
				) );
			} catch ( Exception e ) {
				_bootstrapFailed.Publish( new WorldBootstrapFailureEventArgs(
					args.RequestId,
					args.Mode,
					args.WorldId,
					WorldBootstrapFailureReason.Unknown,
					$"{e.Message}\n{e.StackTrace}"
				) );
			}
		}

		private static void Validate( in WorldBootstrapRequestEventArgs request )
		{
			if ( request.RequestId == Guid.Empty ) {
				throw new InvalidOperationException( "Bootstrap request id cannot be empty." );
			}
			if ( string.IsNullOrWhiteSpace( request.WorldId ) ) {
				throw new InvalidOperationException( "World id is required." );
			}
		}

		private IWorldHandle LoadWorld( string worldId )
		{
			try {
				return _worldLoader.Load( worldId );
			} catch ( Exception e ) {
				throw new InvalidOperationException( $"Failed to load world '{worldId}': {e.Message}" );
			}
		}
	};
};
