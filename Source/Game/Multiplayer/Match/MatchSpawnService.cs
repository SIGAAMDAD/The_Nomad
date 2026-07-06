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
using Nomad.Game.Sdk.Events.Player;
using Nomad.Game.Sdk.Player;

namespace Nomad.Game.Multiplayer.Match
{
	internal sealed class MatchSpawnService : IPlayerSpawnService
	{
		private bool _isDisposed = false;

		public IGameEvent<PlayerSpawnResultEventArgs> SpawnResultsReady => _spawnResultsReady;
		private readonly IGameEvent<PlayerSpawnResultEventArgs> _spawnResultsReady = null;

		public IGameEvent<PlayerSpawnRequestedEventArgs> SpawnRequested => _spawnRequested;
		private readonly IGameEvent<PlayerSpawnRequestedEventArgs> _spawnRequested = null;

		public MatchSpawnService( IGameEventRegistryService eventFactory )
		{
			ArgumentGuard.ThrowIfNull( eventFactory, nameof( eventFactory ) );

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
		}

		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			_spawnResultsReady.Dispose();
			_spawnRequested.Dispose();

			GC.SuppressFinalize( this );
			_isDisposed = true;
		}
	};
};
