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

namespace Nomad.Game.Application.Multiplayer.Profile
{
	internal sealed class LocalPlayerProfileService : ILocalPlayerProfileService
	{
		public PeerId LocalPeerId => _profile.PlayerId;
		public PlayerProfileRecord CurrentProfile => _profile;
		public PlayerStatsRecord CurrentStats => _profile.Stats ?? new PlayerStatsRecord();
		public uint LocalRevision => _localRevision;

		private readonly IStatsService _statsService;
		private readonly ILoggerCategory _category;

		private PlayerProfileRecord _profile;
		private uint _localRevision;
		private bool _isDisposed;

		public LocalPlayerProfileService( PeerId peerId, string callsign, IStatsService statsService, ILoggerService logger )
		{
			_statsService = statsService ?? throw new ArgumentNullException( nameof( statsService ) );
			_category = logger?.CreateCategory( nameof( LocalPlayerProfileService ), LogLevel.Info, true );

			DateTimeOffset now = DateTimeOffset.UtcNow;
			_profile = new PlayerProfileRecord {
				PlayerId = peerId,
				Callsign = callsign ?? string.Empty,
				Stats = new PlayerStatsRecord(),
				CreatedAtUtc = now,
				LastSeenUtc = now
			};
		}

		public void SetLocalPlayer( PeerId peerId, string callsign )
		{
			DateTimeOffset now = DateTimeOffset.UtcNow;
			_profile = _profile with {
				PlayerId = peerId,
				Callsign = callsign ?? string.Empty,
				LastSeenUtc = now
			};
			_localRevision++;

			_category?.PrintDebug( $"SetLocalPlayer: PeerId='{peerId}', Revision={_localRevision}." );
		}

		public PlayerStatsRecord UpdateStats( Func<PlayerStatsRecord, PlayerStatsRecord> update )
		{
			if ( update == null ) {
				throw new ArgumentNullException( nameof( update ) );
			}

			PlayerStatsRecord stats = update( CurrentStats ) ?? new PlayerStatsRecord();
			SetStats( stats );
			return stats;
		}

		public void SetStats( PlayerStatsRecord stats )
		{
			_profile = _profile with {
				Stats = stats ?? new PlayerStatsRecord(),
				LastSeenUtc = DateTimeOffset.UtcNow
			};
			_localRevision++;

			_category?.PrintDebug( $"SetStats: updated local profile stats. PeerId='{LocalPeerId}', Revision={_localRevision}." );
		}

		public async ValueTask<PlayerProfileRecord> RefreshStatsFromOnlineAsync( CancellationToken ct = default )
		{
			PlayerStatsRecord stats = CurrentStats;
			foreach ( PlayerProfileStatDescriptor stat in PlayerProfileStatsSchema.Stats ) {
				ct.ThrowIfCancellationRequested();

				int onlineValue = await _statsService.GetUserStatInt( LocalPeerId, stat.StatKey, ct );
				stats = stat.Write( stats, PlayerProfileStatsSchema.FromOnlineValue( onlineValue ) );
			}

			SetStats( stats );
			_category?.PrintDebug( $"RefreshStatsFromOnlineAsync: refreshed {PlayerProfileStatsSchema.Stats.Length} stats for PeerId='{LocalPeerId}'." );
			return _profile;
		}

		public async ValueTask<bool> PushStatsToOnlineAsync( CancellationToken ct = default )
		{
			bool allSucceeded = true;
			PlayerStatsRecord stats = CurrentStats;

			foreach ( PlayerProfileStatDescriptor stat in PlayerProfileStatsSchema.Stats ) {
				ct.ThrowIfCancellationRequested();

				bool success = await _statsService.SetUserStatInt(
					LocalPeerId,
					stat.StatKey,
					PlayerProfileStatsSchema.ToOnlineValue( stat.Read( stats ) ),
					ct
				);
				allSucceeded &= success;
			}

			bool stored = _statsService.StoreStats();
			_category?.PrintDebug( $"PushStatsToOnlineAsync: pushed {PlayerProfileStatsSchema.Stats.Length} stats for PeerId='{LocalPeerId}'. Success={allSucceeded && stored}." );
			return allSucceeded && stored;
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
