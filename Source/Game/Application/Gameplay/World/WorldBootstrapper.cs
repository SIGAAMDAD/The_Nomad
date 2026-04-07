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
using Nomad.Game.Domain.Data.Gameplay;
using Nomad.Game.Domain.Events.Gameplay;
using Nomad.Game.Domain.Interfaces.Gameplay;
using Nomad.Game.Domain.Interfaces.Multiplayer;
using Nomad.Game.Domain.Interfaces.Player;

namespace Nomad.Game.Application.Gameplay.World {
	internal sealed class WorldBootstrapper : IWorldBootstrapper {
		private readonly IWorldLoader _worldLoader;
		private readonly IPlayerSpawnService _spawnService;
		private readonly INetworkSessionService _networkSessionService;

		private readonly IGameEvent<WorldBootstrapFailureEventArgs> _bootstrapFailed;
		private readonly IGameEvent<WorldBootstrapSucceededEventArgs> _bootstrapSucceeded;

		public WorldBootstrapper( IGameEventRegistryService eventFactory, IWorldLoader loader ) {
			_worldLoader = loader ?? throw new ArgumentNullException( nameof( loader ) );
			_bootstrapFailed = eventFactory.GetEvent<WorldBootstrapFailureEventArgs>( EventNames.WORLD_BOOTSTRAP_FAILURE, EventNames.NAMESPACE );
			_bootstrapSucceeded = eventFactory.GetEvent<WorldBootstrapSucceededEventArgs>( EventNames.WORLD_BOOTSTRAP_SUCCEEDED, EventNames.NAMESPACE );
		}

		public WorldBootstrapResult Bootstrap( WorldBootstrapRequestEventArgs request ) {
			try {
				Validate( in request );

				Guid? resolvedLobbyId = null;

				switch ( request.Mode ) {
					case WorldBootstrapMode.SinglePlayer:
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