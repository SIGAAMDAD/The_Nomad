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
using System.Threading;
using System.Threading.Tasks;
using Nomad.Core.Logger;
using Nomad.Core.OnlineServices;
using Nomad.Game.Sdk.Multiplayer.Profile;
using Nomad.Game.Sdk.Multiplayer;

namespace Nomad.Game.Multiplayer.Profile
{
	internal sealed class RemotePlayerStatsFetchService : IRemotePlayerStatsFetchService, IDisposable
	{
		private readonly IStatsService _statsService;
		private readonly ILoggerCategory _category;
		private bool _isDisposed;

		public RemotePlayerStatsFetchService( IStatsService statsService, ILoggerService logger )
		{
			_statsService = statsService ?? throw new ArgumentNullException( nameof( statsService ) );
			_category = logger?.CreateCategory( nameof( RemotePlayerStatsFetchService ), LogLevel.Info, true );
		}

		public async ValueTask<PlayerStatsRecord> FetchStatsAsync( PeerId peerId, CancellationToken ct = default )
		{
			if ( !peerId.IsValid ) {
				_category?.PrintWarning( "FetchStatsAsync: invalid PeerId; returning empty stats." );
				return new PlayerStatsRecord();
			}

			PlayerStatsRecord stats = new PlayerStatsRecord();
			foreach ( PlayerProfileStatDescriptor stat in PlayerProfileStatsSchema.Stats ) {
				ct.ThrowIfCancellationRequested();

				int onlineValue = await _statsService.GetUserStatInt( peerId, stat.StatKey, ct );
				stats = stat.Write( stats, PlayerProfileStatsSchema.FromOnlineValue( onlineValue ) );
			}

			_category?.PrintDebug( $"FetchStatsAsync: fetched {PlayerProfileStatsSchema.Stats.Length} stats for PeerId='{peerId}'." );
			return stats;
		}

		public void Dispose()
		{
			if ( !_isDisposed ) {
				_category?.Dispose();
			}
			_isDisposed = true;
			GC.SuppressFinalize( this );
		}
	};
};
