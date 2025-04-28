using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Godot;
using Steamworks;

namespace ChallengeMode {
	public class ChallengeCache {
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

		public static Dictionary<ChallengeMap, ChallengeScore> Scores =  new Dictionary<ChallengeMap, ChallengeScore>();
		public static List<ChallengeMap> MapList = null;

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
			score.Score = Score;
			score.TimeMinutes = TimeMinutes;
			score.TimeSeconds = TimeSeconds;
			score.TimeMilliseconds = TimeMilliseconds;

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
			}

			Console.PrintLine( "Loading challenge mode scores..." );
			LoadScores();
		}
	};
};