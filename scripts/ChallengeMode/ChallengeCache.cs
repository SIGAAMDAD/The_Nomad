/*
===========================================================================
The Nomad AGPL Source Code
Copyright (C) 2025 Noah Van Til

The Nomad Source Code is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

The Nomad Source Code is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with The Nomad Source Code.  If not, see <http://www.gnu.org/licenses/>.

If you have questions concerning this license or the applicable additional
terms, you may contact me via email at nyvantil@gmail.com.
===========================================================================
*/

using System.Collections.Generic;
using Steam;
using Godot;
using Steamworks;
using System.Runtime.CompilerServices;

namespace ChallengeMode {
	/*
	===================================================================================
	
	ChallengeCache
	
	===================================================================================
	*/
	/// <summary>
	/// Manages challenge mode data
	/// </summary>

	public class ChallengeCache {
		/// <summary>
		/// A leaderboard entry for a player's ChallengeMap score, stored on steam
		/// </summary>
		public readonly struct LeaderboardEntry {
			public readonly int Score = 0;
			public readonly int TimeCompletedMinutes = 0;
			public readonly int TimeCompletedSeconds = 0;
			public readonly int TimeCompletedMillseconds = 0;
			public readonly CSteamID UserID = CSteamID.Nil;

			public LeaderboardEntry( LeaderboardEntry_t entry, int[] details ) {
				UserID = entry.m_steamIDUser;
				Score = entry.m_nScore;

				TimeCompletedMinutes = details[ 0 ];
				TimeCompletedSeconds = details[ 1 ];
				TimeCompletedMillseconds = details[ 2 ];
			}
		};

		/// <summary>
		/// Local ChallengeMap score data
		/// </summary>
		public struct ChallengeScore {
			public int MapIndex = 0;
			public int TimeMinutes = 0;
			public int TimeSeconds = 0;
			public int TimeMilliseconds = 0;
			public int Score = 0;

			public ChallengeScore( int mapIndex, int timeMinutes, int timeSeconds, int timeMilliseconds, int totalScore ) {
				MapIndex = mapIndex;
				TimeMinutes = timeMinutes;
				TimeSeconds = timeSeconds;
				TimeMilliseconds = timeMilliseconds;
				Score = totalScore;
			}
		};

		/// <summary>
		/// data for the currently selected map
		/// </summary>
		public static Dictionary<int, LeaderboardEntry>? LeaderboardData { get; private set; } = null;

		/// <summary>
		/// Steamworks API leaderboard handle
		/// </summary>
		public static SteamLeaderboardEntries_t LeaderboardEntries { get; private set; }

		/// <summary>
		/// A cache of all the leaderboards related to ChallengeMode
		/// </summary>
		public static Dictionary<StringName, SteamLeaderboard_t>? Leaderboards { get; private set; } = null;

		private static CallResult<LeaderboardFindResult_t> OnLeaderboardFindResult;
		private static CallResult<LeaderboardScoresDownloaded_t> OnLeaderboardScoresDownloaded;
		private static CallResult<LeaderboardScoreUploaded_t> OnLeaderboardScoreUploaded;

		private static System.Action<Dictionary<int, LeaderboardEntry>> ScoresDownloadedCallback;

		/// <summary>
		/// Local score storage cache
		/// </summary>
		public static Dictionary<ChallengeMap, ChallengeScore>? Scores { get; private set; } = null;

		/// <summary>
		/// Cache of all the ChallengeMaps found in res://resources/challenge_maps
		/// </summary>
		public static List<ChallengeMap>? MapList { get; private set; } = null;

		public static readonly int ScoreMultiplier_HeadShot = 3;

		public static int CurrentMap { get; private set; } = 0;
		public static Resource? ActiveQuest { get; private set; }

		/*
		===============
		GetCurrentLeaderboard
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static SteamLeaderboard_t GetCurrentLeaderboard() {
			return Leaderboards[ $"ChallengeMap{CurrentMap}" ];
		}

		/*
		===============
		SetQuestData
		===============
		*/
		public static void SetQuestData( Resource quest ) {
			ActiveQuest = quest;
		}

		/*
		===============
		SetCurrentMap
		===============
		*/
		public static void SetCurrentMap( int currentMap ) {
			CurrentMap = currentMap;
		}

		/*
		===============
		FetchLeaderboardData
		===============
		*/
		public static void FetchLeaderboardData( int mapIndex ) {
			SteamAPICall_t handle;
			if ( !Leaderboards.ContainsKey( string.Format( "Challenge{0}", mapIndex ) ) ) {
				handle = SteamUserStats.FindOrCreateLeaderboard( string.Format( "Challenge{0}", mapIndex ), ELeaderboardSortMethod.k_ELeaderboardSortMethodDescending, ELeaderboardDisplayType.k_ELeaderboardDisplayTypeNumeric );
				OnLeaderboardFindResult.Set( handle );
				return;
			}
			SteamLeaderboard_t hLeaderboard = Leaderboards[ string.Format( "Challenge{0}", mapIndex ) ];

			Console.PrintLine( string.Format( "...Found leaderboard {0}", SteamUserStats.GetLeaderboardName( hLeaderboard ) ) );

			int entryCount = SteamUserStats.GetLeaderboardEntryCount( hLeaderboard );

			handle = SteamUserStats.DownloadLeaderboardEntries( hLeaderboard, ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal, 1, entryCount );
			OnLeaderboardScoresDownloaded.Set( handle );
		}

		/*
		===============
		OnFindLeaderboard
		===============
		*/
		/// <summary>
		/// Steamworks.NET API callback
		/// </summary>
		/// <param name="pCallback"></param>
		/// <param name="bIOFailure"></param>
		private static void OnFindLeaderboard( LeaderboardFindResult_t pCallback, bool bIOFailure ) {
			if ( pCallback.m_bLeaderboardFound == 0 ) {
				Console.PrintError( "[STEAM] Error finding leaderboard!" );
				return;
			}
			Console.PrintLine( string.Format( "[STEAM] Successfully found leaderboard {0}", SteamUserStats.GetLeaderboardName( pCallback.m_hSteamLeaderboard ) ) );
			Leaderboards.Add( SteamUserStats.GetLeaderboardName( pCallback.m_hSteamLeaderboard ), pCallback.m_hSteamLeaderboard );
		}

		/*
		===============
		OnScoreUploaded
		===============
		*/
		/// <summary>
		/// Steamworks.NET API callback
		/// </summary>
		/// <param name="pCallback"></param>
		/// <param name="bIOFailure"></param>
		private static void OnScoreUploaded( LeaderboardScoreUploaded_t pCallback, bool bIOFailure ) {
			if ( pCallback.m_bSuccess == 0 ) {
				Console.PrintError( "[STEAM] Error uploading stats to steam leaderboards!" );
				return;
			}
		}

		/*
		===============
		OnScoreDownloaded
		===============
		*/
		/// <summary>
		/// Steamworks.NET API callbacks
		/// </summary>
		/// <param name="pCallback"></param>
		/// <param name="bIOFailure"></param>
		private static void OnScoreDownloaded( LeaderboardScoresDownloaded_t pCallback, bool bIOFailure ) {
			Console.PrintLine( "Downloading scores..." );

			int[] details = new int[ 3 ];

			LeaderboardEntries = pCallback.m_hSteamLeaderboardEntries;

			LeaderboardData.Clear();
			for ( int i = 0; i < pCallback.m_cEntryCount; i++ ) {
				if ( !SteamUserStats.GetDownloadedLeaderboardEntry( LeaderboardEntries, i, out LeaderboardEntry_t entry, details, details.Length ) ) {
					Console.PrintError( "[STEAM] Error fetching downloaded leaderboard entry!" );
					continue;
				}
				LeaderboardData.Add( entry.m_nGlobalRank, new LeaderboardEntry( entry, details ) );
			}

			ScoresDownloadedCallback?.Invoke( LeaderboardData );
		}

		/*
		===============
		LoadMapList
		===============
		*/
		/// <summary>
		/// Loads all ChallengeMap data from the path provided
		/// </summary>
		/// <param name="path">The directory to load maps from</param>
		/// <param name="list">The list of map file paths to add to</param>
		private static void LoadMapList( string path, List<string> list ) {
			DirAccess dir = DirAccess.Open( path );
			if ( dir != null ) {
				dir.ListDirBegin();
				string fileName = dir.GetNext();
				while ( fileName.Length > 0 ) {
					if ( fileName.GetExtension() != "tres" ) {
						fileName = dir.GetNext();
						continue;
					}
					list.Add( dir.GetCurrentDir() + "/" + fileName );
					fileName = dir.GetNext();
				}
			} else {
				Console.PrintError( $"An error occurred when trying to access path \"{path}\"" );
			}
		}

		/*
		===============
		SaveScores
		===============
		*/
		/// <summary>
		/// Saves all local ChallengeMode scores to user://SaveData/ChallengeData.dat and syncs with SteamRemoteStorage
		/// </summary>
		private static void SaveScores() {
			Error result = DirAccess.MakeDirRecursiveAbsolute( "user://SaveData/" );
			if ( result != Error.Ok ) {
				Console.PrintError( $"ChallengeCache.SaveScores: failed to create directory {ArchiveSystem.SaveDirectory}" );
				return;
			}
			
			string path = ProjectSettings.GlobalizePath( "user://SaveData/ChallengeData.dat" );

			try {
				using ( var stream = new System.IO.FileStream( path, System.IO.FileMode.Create ) ) {
					if ( stream == null ) {
						Console.PrintError( "Error creating SaveData/ChallengeData.dat!" );
						return;
					}
					using var writer = new System.IO.BinaryWriter( stream );
					writer.Write( Scores.Count );
					foreach ( var score in Scores ) {
						writer.Write( score.Value.MapIndex );
						writer.Write( score.Value.Score );
						writer.Write( score.Value.TimeMinutes );
						writer.Write( score.Value.TimeSeconds );
						writer.Write( score.Value.TimeMilliseconds );
						Console.PrintLine( $"Saving score for {score.Value.MapIndex}, {score.Value.TimeMinutes}:{score.Value.TimeSeconds}.{score.Value.TimeMilliseconds}..." );
					}
					writer.Flush();
				}
				SteamManager.SaveCloudFile( "SaveData/ChallengeData.dat" );
			} catch ( System.Exception e ) {
				
			}
		}

		/*
		===============
		UpdateScore
		===============
		*/
		public static void UpdateScore( SteamLeaderboard_t leaderboard, int mapIndex, int score, int timeMinutes, int timeSeconds, int timeMilliseconds ) {
			if ( Scores.TryGetValue( MapList[ mapIndex ], out ChallengeScore data ) ) {
				data = Scores[ MapList[ mapIndex ] ];

				data.MapIndex = mapIndex;

				if ( score > data.Score ) {
					data.Score = score;
				}
				if ( timeMinutes < data.TimeMinutes ) {
					data.TimeMinutes = timeMinutes;
				}
				if ( timeSeconds < data.TimeSeconds ) {
					data.TimeSeconds = timeSeconds;
				}
				if ( timeMilliseconds < data.TimeMilliseconds ) {
					data.TimeMilliseconds = timeMilliseconds;
				}
			} else {
				data = new ChallengeScore();
				data.Score = score;
				data.TimeMinutes = timeMinutes;
				data.TimeSeconds = timeSeconds;
				data.TimeMilliseconds = timeMilliseconds;
				Scores.Add( MapList[ mapIndex ], data );
			}

			SteamUserStats.UploadLeaderboardScore(
				leaderboard,
				ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest,
				score,
				[ timeMinutes, timeSeconds, timeMilliseconds ],
				3
			);

			Console.PrintLine( $"Saving score for {MapList[ mapIndex ].MapName}, {data.Score} {data.TimeMinutes}:{data.TimeSeconds}.{data.TimeMilliseconds}..." );

			SaveScores();

			if ( MapList[ mapIndex ].MapName == "golden_gate_massacre" ) {
				if ( timeMinutes < 5 ) {
					SteamAchievements.ActivateAchievement( "ACH_A_LEGEND_IS_BORN" );
				}
			}

			//
			// check achievements
			//
			int count = 0;
			foreach ( var challenge in Scores ) {
				if ( challenge.Value.Score > 0 ) {
					count++;
				}
			}
			if ( count == Scores.Count ) {
				SteamAchievements.ActivateAchievement( "ACH_COMPLETE_DOMINATION" );
			}
		}

		/*
		===============
		GetScore
		===============
		*/
		public static void GetScore( int mapIndex, out int totalScore, out int timeMinutes, out int timeSeconds, out int timeMilliseconds, System.Action<Dictionary<int, LeaderboardEntry>> callback ) {
			ScoresDownloadedCallback = callback;
			FetchLeaderboardData( mapIndex );

			if ( Scores.TryGetValue( MapList[ mapIndex ], out ChallengeScore score ) ) {
				Console.PrintLine( $"Loading map scores for {mapIndex}..." );
				totalScore = score.Score;
				timeMinutes = score.TimeMinutes;
				timeSeconds = score.TimeSeconds;
				timeMilliseconds = score.TimeMilliseconds;
			} else {
				timeMinutes = 0;
				timeSeconds = 0;
				timeMilliseconds = 0;
				totalScore = 0;
			}
		}

		/*
		===============
		LoadScores
		===============
		*/
		private static void LoadScores() {
			// TODO: make the save path a global static
			string path = ProjectSettings.GlobalizePath( "user://SaveData/ChallengeData.dat" );

			Scores = new Dictionary<ChallengeMap, ChallengeScore>();

			System.IO.FileStream stream;
			try {
				stream = new System.IO.FileStream( path, System.IO.FileMode.Open );
			} catch ( System.Exception exception ) {
				if ( !FileAccess.FileExists( "user://SaveData/ChallengeData.dat" ) ) {
					// not present
					return;
				}
				Console.PrintError( $"Error loading SaveData/ChallengeData.dat even though it exists: {exception.Message}" );
				return;
			}

			using ( var reader = new System.IO.BinaryReader( stream ) ) {
				int count = reader.ReadInt32();
				for ( int i = 0; i < count; i++ ) {
					ChallengeScore score = new ChallengeScore();
					score.MapIndex = reader.ReadInt32();
					score.Score = reader.ReadInt32();
					score.TimeMinutes = reader.ReadInt32();
					score.TimeSeconds = reader.ReadInt32();
					score.TimeMilliseconds = reader.ReadInt32();
					Scores.Add( MapList[ score.MapIndex ], score );

					Console.PrintLine( $"...Loaded score for {i}, {score.TimeMinutes}:{score.TimeSeconds}.{score.TimeMilliseconds}" );
				}
			}
			Console.PrintLine( "...Loaded challenge mode scores." );
		}

		/*
		===============
		Init
		===============
		*/
		public static void Init() {
			Console.PrintLine( "Loading challenge maps..." );

			Leaderboards = new Dictionary<StringName, SteamLeaderboard_t>();
			Scores = new Dictionary<ChallengeMap, ChallengeScore>();

			OnLeaderboardFindResult = CallResult<LeaderboardFindResult_t>.Create( OnFindLeaderboard );
			OnLeaderboardScoreUploaded = CallResult<LeaderboardScoreUploaded_t>.Create( OnScoreUploaded );
			OnLeaderboardScoresDownloaded = CallResult<LeaderboardScoresDownloaded_t>.Create( OnScoreDownloaded );

			List<string> mapList = new List<string>();
			LoadMapList( "res://resources/challenge_maps", mapList );
			mapList.Sort();

			Console.PrintLine( "Loading challenge mode scores..." );

			MapList = new List<ChallengeMap>( mapList.Count );
			for ( int i = 0; i < mapList.Count; i++ ) {
				Console.PrintLine( $"...found map {mapList[ i ]}" );
				Resource map = ResourceLoader.Load( mapList[ i ], "", ResourceLoader.CacheMode.Replace );
				if ( map != null ) {
					MapList.Add( (ChallengeMap)map );
				} else {
					Console.PrintError( $"Error loading challenge map {mapList[ i ]}!" );
				}
			}

			LoadScores();
		}
	};
};