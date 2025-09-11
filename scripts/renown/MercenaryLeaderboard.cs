/*
===========================================================================
Copyright (C) 2023-2025 Noah Van Til

This file is part of The Nomad source code.

The Nomad source code is free software; you can redistribute it
and/or modify it under the terms of the GNU Affero General Public License as
published by the Free Software Foundation; either version 2 of the License,
or (at your option) any later version.

The Nomad source code is distributed in the hope that it will be
useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with The Nomad source code; if not, write to the Free Software
Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301 USA
===========================================================================
*/

using Steamworks;
using System.Collections.Generic;

// NOTE: implement bounty hunt mechanic for top merc

namespace Renown {
	/*
	===================================================================================
	
	MercenaryLeaderboard
	
	===================================================================================
	*/
	
	public class MercenaryLeaderboard {
		public readonly struct LeaderboardEntry {
			public readonly CSteamID UserId = CSteamID.Nil;
			public readonly int Bounty = 0;
			public readonly int Ranking = 0;

			public LeaderboardEntry( LeaderboardEntry_t entry, int[] details ) {
				UserId = entry.m_steamIDUser;
				Bounty = entry.m_nScore;
				Ranking = entry.m_nGlobalRank;
			}
		};

		public static Dictionary<int, LeaderboardEntry> LeaderboardData { get; private set; } = null;
		public static int LeaderboardEntryCount { get; private set; } = 0;
		private static SteamLeaderboardEntries_t LeaderboardEntries;
		private static SteamLeaderboard_t Leaderboard;

		private static CallResult<LeaderboardFindResult_t> OnLeaderboardFindResult;
		private static CallResult<LeaderboardScoreUploaded_t> OnLeaderboardScoreUploaded;
		private static CallResult<LeaderboardScoresDownloaded_t> OnLeaderboardScoresDownloaded;

		/*
		===============
		OnFindLeaderboard
		===============
		*/
		private static void OnFindLeaderboard( LeaderboardFindResult_t pCallback, bool bIOFailure ) {
			if ( pCallback.m_bLeaderboardFound == 0 ) {
				Console.PrintError( "[STEAM] Error finding leaderboard!" );
				return;
			}
			Leaderboard = pCallback.m_hSteamLeaderboard;

			FetchLeaderboardData();
		}

		/*
		===============
		OnScoreUploaded
		===============
		*/
		private static void OnScoreUploaded( LeaderboardScoreUploaded_t pCallback, bool bIOFailure ) {
			if ( pCallback.m_bSuccess == 0 ) {
				Console.PrintError( "[STEAM] Error uploading stats to global leaderboards!" );
				return;
			}
		}

		/*
		===============
		OnScoreDownloaded
		===============
		*/
		private static void OnScoreDownloaded( LeaderboardScoresDownloaded_t pCallback, bool bIOFailure ) {
			int[] details = new int[ 4 ];
			LeaderboardEntries = pCallback.m_hSteamLeaderboardEntries;

			for ( int i = 0; i < pCallback.m_cEntryCount; i++ ) {
				if ( !SteamUserStats.GetDownloadedLeaderboardEntry( LeaderboardEntries, i, out LeaderboardEntry_t entry, details, details.Length ) ) {
					Console.PrintError( "[STEAM] Error fetching downloaded leaderboard entry!" );
					continue;
				}
				LeaderboardData.Add( entry.m_nGlobalRank, new LeaderboardEntry( entry, details ) );
			}
		}

		/*
		===============
		FetchLeaderboardData
		===============
		*/
		private static void FetchLeaderboardData() {
			Console.PrintLine( "Found leaderboard." );

			LeaderboardEntryCount = SteamUserStats.GetLeaderboardEntryCount( Leaderboard );
			LeaderboardData = new Dictionary<int, LeaderboardEntry>( LeaderboardEntryCount );

			Console.PrintLine( $"Downloading {LeaderboardEntryCount} results..." );

			SteamAPICall_t handle = SteamUserStats.DownloadLeaderboardEntries( Leaderboard, ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal, 0, LeaderboardEntryCount );
			OnLeaderboardScoresDownloaded.Set( handle );
		}

		/*
		===============
		Init
		===============
		*/
		public static void Init() {
			Console.PrintLine( "Initializing Global Mercenary Leaderboard..." );

			OnLeaderboardFindResult = CallResult<LeaderboardFindResult_t>.Create( OnFindLeaderboard );
			OnLeaderboardScoreUploaded = CallResult<LeaderboardScoreUploaded_t>.Create( OnScoreUploaded );
			OnLeaderboardScoresDownloaded = CallResult<LeaderboardScoresDownloaded_t>.Create( OnScoreDownloaded );

			SteamAPICall_t handle = SteamUserStats.FindLeaderboard( "GlobalScoreboard" );
			OnLeaderboardFindResult.Set( handle );
		}

		/*
		===============
		UploadData
		===============
		*/
		/// <summary>
		/// The score with all the bonuses should be calculated before calling this
		/// </summary>
		/// <param name="type"></param>
		/// <param name="timeMinutes"></param>
		/// <param name="timeSeconds"></param>
		/// <param name="timeMilliseconds"></param>
		/// <param name="score"></param>
		public static void UploadData( Contracts.Type type, int timeMinutes, int timeSeconds, int timeMilliseconds, int score ) {
			if ( !SteamAPI.IsSteamRunning() ) {
				return;
			}

			Console.PrintLine( "[STEAM] Uploading local statistics to global leaderboards..." );

			int[] details = [
				(int)type,
				timeMinutes,
				timeSeconds,
				timeMilliseconds
			];

			SteamUserStats.UploadLeaderboardScore(
				Leaderboard,
				ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest,
				score,
				details,
				details.Length
			);
		}

		/*
		===============
		GetLeaderboardEntries
		===============
		*/
		public static LeaderboardEntry[] GetLeaderboardEntries() {
			LeaderboardEntry[] entries = new LeaderboardEntry[ LeaderboardData.Count ];
			for ( int i = 0; i < LeaderboardData.Count; i++ ) {
				entries[ i ] = LeaderboardData[ i ];
			}
			System.Array.Sort( entries );
			return entries;
		}

		/*
		===============
		StartBountyHunt
		===============
		*/
		/// <summary>
		/// Allows the local player to "invade" the top mercenary's world and collect the bounty on their head
		/// </summary>
		/// <param name="targetId"></param>
		public static void StartBountyHunt( CSteamID targetId ) {
			if ( targetId == CSteamID.Nil ) {
				Console.PrintError( "MercenaryLeaderboard.StartBountyHunt: invalid targetId (nil)" );
				return;
			}

			LeaderboardEntry target;
			bool bFound = false;

			foreach ( var entry in LeaderboardData ) {
				if ( entry.Value.UserId == targetId ) {
					target = entry.Value;
					bFound = true;
					break;
				}
			}
			if ( !bFound ) {
				Console.PrintWarning( $"StartBountyHunt: no leaderboard entry for player ID {targetId}" );
				return;
			}
		}
	};
};
