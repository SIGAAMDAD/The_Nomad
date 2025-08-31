using System.Collections.Generic;
using System.Linq;
using Godot;
using Steamworks;

namespace ChallengeMode {
	public class ChallengeCache {
		public struct LeaderboardEntry {
			public readonly int Score = 0;
			public readonly int TimeCompletedMinutes = 0;
			public readonly int TimeCompletedSeconds = 0;
			public readonly int TimeCompletedMillseconds = 0;
			public readonly CSteamID UserID = CSteamID.Nil;

			public LeaderboardEntry( LeaderboardEntry_t entry, int[] details ) {
				UserID = entry.m_steamIDUser;
				Score = entry.m_nScore;

				TimeCompletedMinutes = details[0];
				TimeCompletedSeconds = details[1];
				TimeCompletedMillseconds = details[2];
			}
		};
		public struct ChallengeScore {
			public int MapIndex = 0;
			public int TimeMinutes = 0;
			public int TimeSeconds = 0;
			public int TimeMilliseconds = 0;
			public int Score = 0;

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

		private static Dictionary<StringName, SteamLeaderboard_t> Leaderboards = new Dictionary<StringName, SteamLeaderboard_t>();
		private static CallResult<LeaderboardFindResult_t> OnLeaderboardFindResult;
		private static CallResult<LeaderboardScoresDownloaded_t> OnLeaderboardScoresDownloaded;
		private static CallResult<LeaderboardScoreUploaded_t> OnLeaderboardScoreUploaded;

		private static System.Action<Dictionary<int, LeaderboardEntry>> ScoresDownloadedCallback;

		public static Dictionary<ChallengeMap, ChallengeScore> Scores = null;
		public static List<ChallengeMap> MapList = null;

		public static readonly int ScoreMultiplier_HeadShot = 3;

		private static int CurrentMap = 0;
		private static Resource ActiveQuest = null;

		public static Dictionary<int, LeaderboardEntry> GetLeaderboardData() => LeaderboardData;
		public static SteamLeaderboard_t GetCurrentLeaderboard() => Leaderboards[ string.Format( "Challenge{0}", CurrentMap ) ];

		public static Resource GetQuestData() => ActiveQuest;
		public static void SetQuestData( Resource quest ) => ActiveQuest = quest;

		public static int GetCurrentMap() => CurrentMap;
		public static void SetCurrentMap( int nCurrentMap ) => CurrentMap = nCurrentMap;

		public static void FetchLeaderboardData( int nMapIndex ) {
			SteamAPICall_t handle;
			if ( !Leaderboards.ContainsKey( string.Format( "Challenge{0}", nMapIndex ) ) ) {
				handle = SteamUserStats.FindOrCreateLeaderboard( string.Format( "Challenge{0}", nMapIndex ), ELeaderboardSortMethod.k_ELeaderboardSortMethodDescending, ELeaderboardDisplayType.k_ELeaderboardDisplayTypeNumeric );
				OnLeaderboardFindResult.Set( handle );
				return;
			}
			SteamLeaderboard_t hLeaderboard = Leaderboards[ string.Format( "Challenge{0}", nMapIndex ) ];

			Console.PrintLine( string.Format( "...Found leaderboard {0}", SteamUserStats.GetLeaderboardName( hLeaderboard ) ) );

			int entryCount = SteamUserStats.GetLeaderboardEntryCount( hLeaderboard );

			handle = SteamUserStats.DownloadLeaderboardEntries( hLeaderboard, ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal, 1, entryCount );
			OnLeaderboardScoresDownloaded.Set( handle );
		}

		private static void OnFindLeaderboard( LeaderboardFindResult_t pCallback, bool bIOFailure ) {
			if ( pCallback.m_bLeaderboardFound == 0 ) {
				Console.PrintError( "[STEAM] Error finding leaderboard!" );
				return;
			}
			Console.PrintLine( string.Format( "[STEAM] Successfully found leaderboard {0}", SteamUserStats.GetLeaderboardName( pCallback.m_hSteamLeaderboard ) ) );
			Leaderboards.Add( SteamUserStats.GetLeaderboardName( pCallback.m_hSteamLeaderboard ), pCallback.m_hSteamLeaderboard );
		}
		private static void OnScoreUploaded( LeaderboardScoreUploaded_t pCallback, bool bIOFailure ) {
			if ( pCallback.m_bSuccess == 0 ) {
				Console.PrintError( "[STEAM] Error uploading stats to steam leaderboards!" );
				return;
			}
		}
		private static void OnScoreDownloaded( LeaderboardScoresDownloaded_t pCallback, bool bIOFailure ) {
			Console.PrintLine( "Downloading scores..." );

			int[] details = new int[3];

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
			DirAccess.MakeDirRecursiveAbsolute( "user://SaveData/" );
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
						Console.PrintLine( string.Format( "Saving score for {0}, {1}:{2}.{3}...", score.Value.MapIndex, score.Value.TimeMinutes, score.Value.TimeSeconds, score.Value.TimeMilliseconds ) );
					}
					writer.Flush();
				}
			}
			SteamManager.SaveCloudFile( "SaveData/ChallengeData.dat" );
		}
		public static void UpdateScore( SteamLeaderboard_t hLeaderboard, int MapIndex, int Score, int TimeMinutes, int TimeSeconds, int TimeMilliseconds ) {
			ChallengeScore score;
			if ( Scores.ContainsKey( MapList[ MapIndex ] ) ) {
				score = Scores[ MapList[ MapIndex ] ];

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
			} else {
				score = new ChallengeScore();
				score.Score = Score;
				score.TimeMinutes = TimeMinutes;
				score.TimeSeconds = TimeSeconds;
				score.TimeMilliseconds = TimeMilliseconds;
				Scores.Add( MapList[ MapIndex ], score );
			}

			SteamUserStats.UploadLeaderboardScore(
				hLeaderboard,
				ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest,
				Score,
				[ TimeMinutes, TimeSeconds, TimeMilliseconds ],
				3
			);

			Console.PrintLine( string.Format( "Saving score for {0}, {1} {2}:{3}.{4}...", MapList[ MapIndex ].MapName, score.Score, score.TimeMinutes, score.TimeSeconds, score.TimeMilliseconds ) );

			SaveScores();

			if ( MapList[ MapIndex ].MapName == "golden_gate_massacre" ) {
				if ( TimeMinutes < 5 ) {
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
		public static void GetScore( int MapIndex, out int Score, out int TimeMinutes, out int TimeSeconds, out int TimeMilliseconds, System.Action<Dictionary<int, LeaderboardEntry>> callback ) {
			ScoresDownloadedCallback = callback;
			FetchLeaderboardData( MapIndex );

			if ( Scores.TryGetValue( MapList[ MapIndex ], out ChallengeScore score ) ) {
				Console.PrintLine( string.Format( "Loading map scores for {0}...", MapIndex ) );
				Score = score.Score;
				TimeMinutes = score.TimeMinutes;
				TimeSeconds = score.TimeSeconds;
				TimeMilliseconds = score.TimeMilliseconds;
			} else {
				TimeMinutes = 0;
				TimeSeconds = 0;
				TimeMilliseconds = 0;
				Score = 0;
			}
		}
		private static void LoadScores() {
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
				Console.PrintError( string.Format( "Error loading SaveData/ChallengeData.dat even though it exists: {0}", exception.Message ) );
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

					Console.PrintLine( string.Format( "...Loaded score for {0}, {1}:{2}.{3}", i, score.TimeMinutes, score.TimeSeconds, score.TimeMilliseconds ) );
				}
			}
			Console.PrintLine( "...Loaded challenge mode scores." );
		}
		public static void Init() {
			Console.PrintLine( "Loading challenge maps..." );

			OnLeaderboardFindResult = CallResult<LeaderboardFindResult_t>.Create( OnFindLeaderboard );
			OnLeaderboardScoreUploaded = CallResult<LeaderboardScoreUploaded_t>.Create( OnScoreUploaded );
			OnLeaderboardScoresDownloaded = CallResult<LeaderboardScoresDownloaded_t>.Create( OnScoreDownloaded );

			List<string> mapList = new List<string>();
			LoadMapList( "res://resources/challenge_maps", mapList );
			mapList.Sort();

			Console.PrintLine( "Loading challenge mode scores..." );

			MapList = new List<ChallengeMap>( mapList.Count );
			for ( int i = 0; i < mapList.Count; i++ ) {
				Console.PrintLine( string.Format( "...found map {0}", mapList[i] ) );
				Resource map = ResourceLoader.Load( mapList[i], "", ResourceLoader.CacheMode.Replace );
				if ( map != null ) {
					MapList.Add( (ChallengeMap)map );
				} else {
					Console.PrintError( string.Format( "Error loading challenge map {0}!", mapList[i] ) );
				}
			}

			LoadScores();
		}
	};
};