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
using Nomad.Core.Events;
using Nomad.Core.OnlineServices;
using Nomad.Game.Domain.Data.Gameplay;
using Nomad.Game.Domain.Events.Gameplay;
using Nomad.Game.Domain.Interfaces.Gameplay;
using Nomad.Game.Domain.Interfaces.Player;

namespace Nomad.Game.Application.Gameplay.World {
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
	
	internal sealed class WorldBootstrapper : IWorldBootstrapper {
		private readonly IWorldLoader _worldLoader;
		private readonly IPlayerSpawnService _spawnService;
		private readonly INetworkSessionService _networkSessionService;

		private readonly IGameEvent<WorldBootstrapFailureEventArgs> _bootstrapFailed;
		private readonly IGameEvent<WorldBootstrapSucceededEventArgs> _bootstrapSucceeded;

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
		public WorldBootstrapper( IGameEventRegistryService eventFactory, IWorldLoader loader ) {
			_worldLoader = loader ?? throw new ArgumentNullException( nameof( loader ) );
			_bootstrapFailed = eventFactory.GetEvent<WorldBootstrapFailureEventArgs>( EventNames.WORLD_BOOTSTRAP_FAILURE, EventNames.NAMESPACE );
			_bootstrapSucceeded = eventFactory.GetEvent<WorldBootstrapSucceededEventArgs>( EventNames.WORLD_BOOTSTRAP_SUCCEEDED, EventNames.NAMESPACE );
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
		public WorldBootstrapResult Bootstrap( in WorldBootstrapRequestEventArgs request ) {
			try {
				Validate( in request );

				Guid? resolvedLobbyId = null;

				switch ( request.Mode ) {
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
				IWorldHandle world = LoadWorld( request.WorldId );

				var succeeded = new WorldBootstrapSucceededEventArgs(
					request.RequestId,
					request.Mode,
					request.WorldId,
					world.Id,
					resolvedLobbyId
				);

				_bootstrapSucceeded.Publish( in succeeded );

				return new WorldBootstrapSuccess {
					RequestId = succeeded.RequestId,
					Mode = succeeded.Mode,
					WorldId = succeeded.WorldId,
					WorldInstanceId = succeeded.WorldInstanceId,
					LobbyId = succeeded.LobbyId
				};
			} catch ( Exception e ) {
				return PublishFailure( request, WorldBootstrapFailureReason.Unknown, $"{e.Message}\n{e.StackTrace}" );
			}
		}

		private static void Validate( in WorldBootstrapRequestEventArgs request ) {
			if ( request.RequestId == Guid.Empty ) {
				throw new InvalidOperationException( "Bootstrap request id cannot be empty." );
			}
			if ( string.IsNullOrWhiteSpace( request.WorldId ) ) {
				throw new InvalidOperationException( "World id is required." );
			}
		}

		private IWorldHandle LoadWorld( string worldId ) {
			try {
				return _worldLoader.Load( worldId );
			} catch ( Exception e ) {
				throw new InvalidOperationException( $"Failed to load world '{worldId}': {e.Message}" );
			}
		}

		private WorldBootstrapFailure PublishFailure( in WorldBootstrapRequestEventArgs request, WorldBootstrapFailureReason reason, string? detail ) {
			var failed = new WorldBootstrapFailureEventArgs(
				request.RequestId,
				request.Mode,
				request.WorldId,
				reason,
				detail
			);

			_bootstrapFailed.Publish( in failed );

			return new WorldBootstrapFailure {
				RequestId = failed.RequestId,
				Mode = failed.Mode,
				WorldId = failed.WorldId,
				Reason = failed.Reason,
				Detail = failed.Detail
			};
		}
	};
};