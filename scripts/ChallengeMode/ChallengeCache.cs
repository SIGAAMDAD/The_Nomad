using System.Collections.Generic;
using Godot;
using Steamworks;

namespace ChallengeMode {
	public class ChallengeCache {
		private struct LeaderboardEntry {
			public readonly int Score = 0;
			public readonly int TimeCompletedMinutes = 0;
			public readonly int TimeCompletedSeconds = 0;
			public readonly int TimeCompletedMillseconds = 0;
			public readonly CSteamID UserID = CSteamID.Nil;

			public LeaderboardEntry( LeaderboardEntry_t entry, int[] details ) {
				UserID = entry.m_steamIDUser;
				Score = entry.m_nScore;

				if ( details.Length != 3 ) {
					Console.PrintError( "[STEAM] Invalid leaderboard entry data!" );
					return;
				}

				TimeCompletedMinutes = details[0];
				TimeCompletedSeconds = details[1];
				TimeCompletedMillseconds = details[2];
			}
		};
		public struct ChallengeScore {
			public int MapIndex;
			public int TimeMinutes;
			public int TimeSeconds;
			public int TimeMilliseconds;
			public int Score;

			public ChallengeScore( int MapIndex, int TimeMinutes, int TimeSeconds, int TimeMilliseconds, int TotalScore ) {
				this.MapIndex = MapIndex;
				this.TimeMinutes = TimeMinutes;
				this.TimeSeconds = TimeSeconds;
				this.TimeMilliseconds = TimeMilliseconds;
				Score = TotalScore;
			}
		};

		/// <summary>
		/// data for the currently selected map
		/// </summary>
		private static Dictionary<int, LeaderboardEntry> LeaderboardData = new Dictionary<int, LeaderboardEntry>();
		private static SteamLeaderboardEntries_t LeaderboardEntries;	

		private static Dictionary<StringName, SteamLeaderboard_t> Leaderboards;
		private static CallResult<LeaderboardFindResult_t> OnLeaderboardFindResult;
		private static CallResult<LeaderboardScoresDownloaded_t> OnLeaderboardScoresDownloaded;
		private static CallResult<LeaderboardScoreUploaded_t> OnLeaderboardScoreUploaded;

		public static Dictionary<ChallengeMap, ChallengeScore> Scores =  new Dictionary<ChallengeMap, ChallengeScore>();
		public static List<ChallengeMap> MapList = null;

		private static int CurrentMap = 0;

		public static SteamLeaderboard_t GetCurrentLeaderboard() => Leaderboards[ MapList[ CurrentMap ].MapName ];

		public static int GetCurrentMap() => CurrentMap;
		public static void SetCurrentMap( int nCurrentMap ) => CurrentMap = nCurrentMap;

		public static void FetchLeaderboardData( int nMapIndex ) {
			SteamLeaderboard_t hLeaderboard = Leaderboards[ string.Format( "Challenge{0}", nMapIndex ) ];

			Console.PrintLine( string.Format( "...Found leaderboard {0}", SteamUserStats.GetLeaderboardName( hLeaderboard ) ) );

			int entryCount = SteamUserStats.GetLeaderboardEntryCount( hLeaderboard );

			SteamAPICall_t handle = SteamUserStats.DownloadLeaderboardEntries( hLeaderboard, ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal, 0, entryCount );
			OnLeaderboardScoresDownloaded.Set( handle );
		}

		private static void OnFindLeaderboard( LeaderboardFindResult_t pCallback, bool bIOFailure ) {
			if ( pCallback.m_bLeaderboardFound == 0 ) {
				Console.PrintError( "[STEAM] Error finding leaderboard!" );
				return;
			}
			Leaderboards.Add( SteamUserStats.GetLeaderboardName( pCallback.m_hSteamLeaderboard ), pCallback.m_hSteamLeaderboard );
		}
		private static void OnScoreUploaded( LeaderboardScoreUploaded_t pCallback, bool bIOFailure ) {
			if ( pCallback.m_bSuccess == 0 ) {
				Console.PrintError( "[STEAM] Error uploading stats to steam leaderboards!" );
				return;
			}
		}
		private static void OnScoreDownloaded( LeaderboardScoresDownloaded_t pCallback, bool bIOFailure ) {
			int[] details = new int[4];
			LeaderboardEntries = pCallback.m_hSteamLeaderboardEntries;

			LeaderboardData.Clear();
			for ( int i = 0; i < pCallback.m_cEntryCount; i++ ) {
				LeaderboardEntry_t entry;
				if ( !SteamUserStats.GetDownloadedLeaderboardEntry( LeaderboardEntries, i, out entry, details, details.Length ) ) {
					Console.PrintError( "[STEAM] Error fetching downloaded leaderboard entry!" );
					continue;
				}
				LeaderboardData.Add( entry.m_nGlobalRank, new LeaderboardEntry( entry, details ) );
			}
		}

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
				Console.PrintError( string.Format( "An error occurred when trying to access path \"{0}\"", path ) );
			}
		}
		private static void SaveScores() {
			string path = ProjectSettings.GlobalizePath( "user://SaveData/ChallengeData.dat" );
			using ( var stream = new System.IO.FileStream( path, System.IO.FileMode.Create ) ) {
				if ( stream == null ) {
					Console.PrintError( "Error creating SaveData/ChallengeData.dat!" );
					return;
				}
				using ( var writer = new System.IO.BinaryWriter( stream ) ) {
					writer.Write( Scores.Count );
					foreach ( var score in Scores ) {
						writer.Write( score.Value.MapIndex );
						writer.Write( score.Value.Score );
						writer.Write( score.Value.TimeMinutes );
						writer.Write( score.Value.TimeSeconds );
						writer.Write( score.Value.TimeMilliseconds );
					}
				}
			}
			SteamManager.SaveCloudFile( "user://SaveData/ChallengeData.dat" );
		}
		public static void UpdateScore( SteamLeaderboard_t hLeaderboard, int MapIndex, int Score, int TimeMinutes, int TimeSeconds, int TimeMilliseconds ) {
			ChallengeScore score = Scores[ MapList[ MapIndex ] ];
			score.MapIndex = MapIndex;

			if ( Score > score.Score ) {
				score.Score = Score;
			}
			if ( TimeMinutes < score.TimeMinutes ) {
				score.TimeMinutes = TimeMinutes;
			}
			if ( TimeSeconds < score.TimeSeconds ) {
				score.TimeSeconds = TimeSeconds;
			}
			if ( TimeMilliseconds < score.TimeMilliseconds ) {
				score.TimeMilliseconds = TimeMilliseconds;
			}

			SteamUserStats.UploadLeaderboardScore(
				hLeaderboard,
				ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest,
				Score,
				[ TimeMinutes, TimeSeconds, TimeMilliseconds ],
				3
			);

			SaveScores();
		}
		public static void GetScore( int MapIndex, out int Score, out int TimeMinutes, out int TimeSeconds, out int TimeMilliseconds ) {
			ChallengeScore score = Scores[ MapList[ MapIndex ] ];

			Score = score.Score;
			TimeMinutes = score.TimeMilliseconds;
			TimeSeconds = score.TimeSeconds;
			TimeMilliseconds = score.TimeMilliseconds;

			FetchLeaderboardData( MapIndex );
		}
		private static void LoadScores() {
			string path = ProjectSettings.GlobalizePath( "user://SaveData/ChallengeData.dat" );

			System.IO.FileStream stream;
			try {
				stream = new System.IO.FileStream( path, System.IO.FileMode.Open );
			} catch ( System.Exception exception ) {
				if ( !FileAccess.FileExists( "user://SaveData/ChallengeData.dat" ) ) {
					// not present
					return;
				}
				Console.PrintError( string.Format( "Error loading SaveData/ChallengeData.dat even though it exists: {0}", exception.Message ) );
				return;
			}

			using ( var reader = new System.IO.BinaryReader( stream ) ) {
				int count = reader.ReadInt32();
				for ( int i = 0; i < count; i++ ) {
					int mapIndex = reader.ReadInt32();
					int score = reader.ReadInt32();
					int minutes = reader.ReadInt32();
					int seconds = reader.ReadInt32();
					int milliseconds = reader.ReadInt32();

					if ( mapIndex < 0 || mapIndex >= MapList.Count ) {
						Console.PrintWarning( string.Format( "ChallengeCache.LoadScores: invalid mapIndex {0}", mapIndex ) );
						continue;
					}

					Scores.Add( MapList[ mapIndex ], new ChallengeScore( mapIndex, minutes, seconds, milliseconds, score ) );
				}
			}
		}
		public static void Init() {
			Console.PrintLine( "Loading challenge maps..." );

			OnLeaderboardFindResult = CallResult<LeaderboardFindResult_t>.Create( OnFindLeaderboard );
			OnLeaderboardScoreUploaded = CallResult<LeaderboardScoreUploaded_t>.Create( OnScoreUploaded );
			OnLeaderboardScoresDownloaded = CallResult<LeaderboardScoresDownloaded_t>.Create( OnScoreDownloaded );

			List<string> mapList = new List<string>();
			LoadMapList( "res://resources/challenge_maps", mapList );

			MapList = new List<ChallengeMap>( mapList.Count );
			for ( int i = 0; i < mapList.Count; i++ ) {
				Console.PrintLine( string.Format( "...found map {0}", mapList[i] ) );
				Resource map = ResourceLoader.Load( mapList[i], "", ResourceLoader.CacheMode.Replace );
				if ( map != null ) {
					MapList.Add( (ChallengeMap)map );
				} else {
					Console.PrintError( string.Format( "Error loading challenge map {0}!", mapList[i] ) );
				}
				SteamAPICall_t handle = SteamUserStats.FindLeaderboard( string.Format( "Challenge{0}", i ) );
				OnLeaderboardFindResult.Set( handle );
			}

			Console.PrintLine( "Loading challenge mode scores..." );
			LoadScores();
		}
	};
};