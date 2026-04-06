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
using Nomad.Game.Domain.Data.Player;
using Nomad.Game.Domain.Events.Player;
using Nomad.Game.Domain.Interfaces.Player;

namespace Nomad.Game.Application.Gameplay.Player {
	/*
	===================================================================================
	
	PlayerSpawnService
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	internal sealed class PlayerSpawnService : IPlayerSpawnService {
		public IGameEvent<PlayerSpawnResultEventArgs> SpawnResultsReady => _spawnResultsReady;
		private readonly IGameEvent<PlayerSpawnResultEventArgs> _spawnResultsReady = default;

		private readonly ISubscriptionHandle _spawnRequested;
		private readonly PlayerRepository _repository;

		private bool _isDisposed = false;
		
		/*
		===============
		PlayerSpawnService
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="eventFactory"></param>
		/// <param name="repository"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public PlayerSpawnService( IGameEventRegistryService eventFactory, PlayerRepository repository ) {
			_spawnResultsReady = eventFactory.GetEvent<PlayerSpawnResultEventArgs>( EventNames.PLAYER_SPAWN_RESULT_READY, EventNames.NAMESPACE );

			_repository = repository ?? throw new ArgumentNullException( nameof( repository ) );

			_spawnRequested = eventFactory.
				GetEvent<PlayerSpawnRequestedEventArgs>( EventNames.PLAYER_SPAWN_REQUESTED, EventNames.NAMESPACE )
				.Subscribe( OnSpawnRequested );
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
				_spawnRequested?.Dispose();
			}
			GC.SuppressFinalize( this );
			_isDisposed = true;
		}

		/*
		===============
		OnSpawnRequested
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnSpawnRequested( in PlayerSpawnRequestedEventArgs args ) {
			var player = _repository.CreatePlayer( in args );
			_spawnResultsReady.Publish( new PlayerSpawnResultEventArgs( args.RequestId, player.Id, true ) );
		}
	};
};