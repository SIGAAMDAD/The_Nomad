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
using Nomad.Core.Logger;
using Nomad.Core.Compatibility.Guards;
using Nomad.Game.Domain.Events.Player;
using Nomad.Game.Domain.Interfaces.Player;

namespace Nomad.Game.Application.Gameplay.Player
{
	/*
	===================================================================================

	PlayerSpawnService

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed class PlayerSpawnService : IPlayerSpawnService
	{
		private readonly ILoggerCategory _category;
		private readonly IGameEventRegistryService _eventFactory;

		private readonly PlayerRepository _repository;
		private readonly IPlayerSpawnApplicator _spawnApplicator;
		private readonly IPlayerSpawnResolver _profileResolver;

		private bool _isDisposed = false;

		public IGameEvent<PlayerSpawnResultEventArgs> SpawnResultsReady => _spawnResultsReady;
		private readonly IGameEvent<PlayerSpawnResultEventArgs> _spawnResultsReady = default;

		public IGameEvent<PlayerSpawnRequestedEventArgs> SpawnRequested => _spawnRequested;
		private readonly IGameEvent<PlayerSpawnRequestedEventArgs> _spawnRequested = default;

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
		/// <param name="spawnApplicator"></param>
		/// <param name="profileResolver"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public PlayerSpawnService(
			IGameEventRegistryService eventFactory,
			PlayerRepository repository,
			IPlayerSpawnApplicator spawnApplicator,
			IPlayerSpawnResolver profileResolver,
			ILoggerService logger
		)
		{
			ArgumentGuard.ThrowIfNull( eventFactory, nameof( eventFactory ) );

			_repository = repository ?? throw new ArgumentNullException( nameof( repository ) );
			_profileResolver = profileResolver ?? throw new ArgumentNullException( nameof( profileResolver ) );
			_spawnApplicator = spawnApplicator ?? throw new ArgumentNullException( nameof( spawnApplicator ) );

			_eventFactory = eventFactory ?? throw new ArgumentNullException( nameof( eventFactory ) );
			_category = logger.CreateCategory( nameof( PlayerSpawnService ), LogLevel.Info, true );

			_spawnResultsReady = eventFactory
				.GetEvent<PlayerSpawnResultEventArgs>(
					PlayerSpawnResultEventArgs.Name,
					PlayerSpawnResultEventArgs.NameSpace
				);

			_spawnRequested = eventFactory
				.GetEvent<PlayerSpawnRequestedEventArgs>(
					PlayerSpawnRequestedEventArgs.Name,
					PlayerSpawnRequestedEventArgs.NameSpace
				);
			_spawnRequested.Subscribe( OnSpawnRequested );
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
			_spawnRequested.Dispose();
			_spawnResultsReady.Dispose();
			_category.Dispose();

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
		private void OnSpawnRequested( in PlayerSpawnRequestedEventArgs args )
		{
			try {
				var player = _repository.CreatePlayer( in args );

				var profile = _profileResolver.Resolve( in args.Context );
				player.ApplySpawnProfile( _spawnApplicator, profile, in args.Context );

				_spawnResultsReady.Publish( new PlayerSpawnResultEventArgs( args.RequestId, player.Id, true ) );
			} catch ( Exception e ) {
				_category.PrintError( $"Exception thrown when spawning player: {e}" );
				throw;
			}
		}
	};
};
