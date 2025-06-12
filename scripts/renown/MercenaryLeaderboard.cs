using Godot;
using Multiplayer;
using Steamworks;
using System;
using System.Collections.Generic;

// NOTE: implement bounty hunt mechanic for top merc

namespace Renown {
	public class MercenaryLeaderboard {
		public readonly struct LeaderboardEntry {
			public readonly CSteamID UserId = CSteamID.Nil;
			public readonly int Bounty = 0;
			public readonly int Ranking = 0;

			public LeaderboardEntry( LeaderboardEntry_t hEntry, int[] szDetails ) {
				UserId = hEntry.m_steamIDUser;
				Bounty = hEntry.m_nScore;
				Ranking = hEntry.m_nGlobalRank;
			}
		};
		
		private static Dictionary<int, LeaderboardEntry> LeaderboardData = null;
		private static int LeaderboardEntryCount = 0;
		private static SteamLeaderboardEntries_t LeaderboardEntries;
		private static SteamLeaderboard_t hLeaderboard;
		
		private static CallResult<LeaderboardFindResult_t> OnLeaderboardFindResult;
		private static CallResult<LeaderboardScoreUploaded_t> OnLeaderboardScoreUploaded;
		private static CallResult<LeaderboardScoresDownloaded_t> OnLeaderboardScoresDownloaded;
		
		private static void OnFindLeaderboard( LeaderboardFindResult_t pCallback, bool bIOFailure ) {
			if ( pCallback.m_bLeaderboardFound == 0 ) {
				Console.PrintError( "[STEAM] Error finding leaderboard!" );
				return;
			}
			hLeaderboard = pCallback.m_hSteamLeaderboard;

			FetchLeaderboardData();
		}
		private static void OnScoreUploaded( LeaderboardScoreUploaded_t pCallback, bool bIOFailure ) {
			if ( pCallback.m_bSuccess == 0 ) {
				Console.PrintError( "[STEAM] Error uploading stats to global leaderboards!" );
				return;
			}
		}
		private static void OnScoreDownloaded( LeaderboardScoresDownloaded_t pCallback, bool bIOFailure ) {
			int[] details = new int[ 4 ];
			LeaderboardEntries = pCallback.m_hSteamLeaderboardEntries;
			
			for ( int i = 0; i < pCallback.m_cEntryCount; i++ ) {
				if ( !SteamUserStats.GetDownloadedLeaderboardEntry( LeaderboardEntries, i, out LeaderboardEntry_t hEntry, details, details.Length ) ) {
					Console.PrintError( "[STEAM] Error fetching downloaded leaderboard entry!" );
					continue;
				}
				LeaderboardData.Add( hEntry.m_nGlobalRank, new LeaderboardEntry( hEntry, details ) );
			}
		}
		
		private static void FetchLeaderboardData() {
			Console.PrintLine( "Found leaderboard." );

			LeaderboardEntryCount = SteamUserStats.GetLeaderboardEntryCount( hLeaderboard );
			LeaderboardData = new Dictionary<int, LeaderboardEntry>( LeaderboardEntryCount );

			Console.PrintLine( string.Format( "Downloading {0} results...", LeaderboardEntryCount ) );

			SteamAPICall_t handle = SteamUserStats.DownloadLeaderboardEntries( hLeaderboard, ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal, 0, LeaderboardEntryCount );
			OnLeaderboardScoresDownloaded.Set( handle );
		}
		
		public static void Init() {
			Console.PrintLine( "Initializing Global Mercenary Leaderboard..." );

			OnLeaderboardFindResult = CallResult<LeaderboardFindResult_t>.Create( OnFindLeaderboard );
			OnLeaderboardScoreUploaded = CallResult<LeaderboardScoreUploaded_t>.Create( OnScoreUploaded );
			OnLeaderboardScoresDownloaded = CallResult<LeaderboardScoresDownloaded_t>.Create( OnScoreDownloaded );

			SteamAPICall_t handle = SteamUserStats.FindLeaderboard( "GlobalScoreboard" );
			OnLeaderboardFindResult.Set( handle );
		}

		/// <summary>
		/// the score with all the bonuses should be calculated before calling this
		/// </summary>
		public static void UploadData( ContractType nType, int TimeMinutes, int TimeSeconds, int TimeMilliseconds, int nScore ) {
			Console.PrintLine( "[STEAM] Uploading local statistics to global leaderboards..." );
			
			int[] details = [
				(int)nType,
				TimeMinutes,
				TimeSeconds,
				TimeMilliseconds
			];
			
			SteamUserStats.UploadLeaderboardScore(
				hLeaderboard,
				ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest,
				nScore,
				details,
				details.Length
			);
		}
		
		public static List<LeaderboardEntry> GetLeaderboardEntries() {
			List<LeaderboardEntry> entries = new List<LeaderboardEntry>( LeaderboardData.Count );
			for ( int i = 0; i < LeaderboardData.Count; i++ ) {
				entries.Add( LeaderboardData[i] );
			}
			entries.Sort();
			return entries;
		}

		/// <summary>
		/// allows the local player to "invade" the top mercenary's world and collect the bounty on their head
		/// </summary>
		public static void StartBountyHunt() {
		}
	};
};
