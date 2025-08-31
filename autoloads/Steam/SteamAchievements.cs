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

using Godot;
using Steamworks;
using System.Collections.Concurrent;

namespace Steam {
	/*
	===================================================================================
	
	SteamAchievements
	
	===================================================================================
	*/
	/// <summary>
	/// The main manager behind Steam achievements and stats
	/// </summary>
	/// <remarks>
	/// Ideally shouldn't be called into from mods, maybe official DLC (but I hate DLC-dependent achievements)
	/// </remarks>

	[GlobalClass]
	public partial class SteamAchievements : Node {
		// don't judge me...
		public enum AchievementID {
			R_U_Cheating = 0,
			Complete_Domination,
			Geneva_Suggestion,
			Pyromaniac,
			Massacre,
			Just_A_Minor_Inconvenience,
			Jack_The_Ripper,
			Explosion_Connoisseuir,
			God_of_War,
			Boom_Headshot,
			Respectful,
			A_Legend_is_Born,
			Building_the_Legend,
			Same_Shit_Different_Day,
			Shut_The_FUCK_Up,
			This_Is_SPARTA,
			Unearthed_Arcana,
			Awaken_The_Ancients,
			Its_High_Noon,
			Death_From_Above,
			Kombatant,
			Laughing_In_Deaths_Face,
			Right_Back_at_U,
			Bitch_Slap,
			Chefs_Special,
			Knuckle_Sandwich,
			Brother_In_Arms,
			BRU,
			GIT_PWNED,
			Silent_Death,
			Well_Done_Weeb,
			Its_Treason_Then,
			Heartless,
			Bushido,
			Maximus_The_Merciful,
			Cheer_Up_Love_The_Cavalrys_Here,
			Worse_Than_Death,
			Looks_Like_Meats_Back_On_Our_Menu_Boys,
			GYAT,
			To_The_Slaughter,
			One_Man_Army,
			You_Call_That_A_KNIFE,
			AMERICA_FUCK_YEA,
			Sussy,
			Live_To_Fight_Another_Day,
			Remember_Us,
			Zandutsu_That_Shit,
			MORE,
			Edgelord,
			That_Actually_WORKED,
			NANOMACHINES_SON,
			Cool_Guys_Dont_Look_At_Explosions,
			Double_Take,
			Triple_Threat,
			DAYUUM_I_Aint_Gonna_Sugarcoat_It,
			Back_From_The_Brink,
			Dance_DANCE_DANCE,
			Bang_Bang_I_Shot_Em_Down,
			BOP_IT,
			Just_A_Leap_of_Faith,
			Send_Them_To_Jesus,
			Rizzlord,
			AHHH_GAHHH_HAAAAAAA,
			Absolutely_Necessary_Precautions,
			Stop_Hitting_Yourself,
			Fuck_This_Shit_Im_Out,
			Smoke_Break,
			Forever_Alone_Forever_Forlorn,
			One_In_A_Million,
			Touch_Grass,
			And_So_It_Begins,
			Not_Like_Us,
			John_Wick_Mode,
			The_Nomad,
			DNA_Of_The_Soul,
			Mercenary_Of_Many_Skills,
			Master_Of_The_Wastes,
			Sands_of_Time,
			Sweaty,
			A_Childish_Wish,
			Endless_Indulgence,
			Cold_Reality,
			Whispers_Of_The_Past,
			Lose_Yourself,

			Count
		};

		public struct SteamAchievement {
			public readonly AchievementID Id { get; }
			public readonly string IdString { get; }
			public readonly string Name { get; }
			public readonly object? MaxValue { get; } = null;
			public bool Achieved { get; private set; } = false;
			public object? Value { get; private set; } = null;

			public SteamAchievement( AchievementID id, string name ) {
				Id = id;
				IdString = "ACH_" + id.ToString().ToUpper();
				Name = name;
				Value = 0;
			}
			public SteamAchievement( AchievementID id, string name, uint maxValue ) {
				Id = id;
				IdString = $"ACH_{id.ToString().ToUpper()}";
				Name = name;
				Value = 0;
				MaxValue = maxValue;
			}
			public SteamAchievement( AchievementID id, string name, float maxValue ) {
				Id = id;
				IdString = $"ACH_{id.ToString().ToUpper()}";
				Name = name;
				Value = 0.0f;
				MaxValue = maxValue;
			}

			/*
			===============
			SetAchieved
			===============
			*/
			public void SetAchieved( bool bAchieved ) {
				Achieved = bAchieved;
			}

			/*
			===============
			SetFloatValue
			===============
			*/
			public void SetFloatValue( float value ) {
				if ( Value is float ) {
					Value = value;
					return;
				}
				Console.PrintError( string.Format( "[STEAM] Attempted to set achievement statistic data for {0} to invalid data type (float)", Name ) );
			}

			/*
			===============
			SetIntValue
			===============
			*/
			public void SetIntValue( int value ) {
				if ( Value is int ) {
					Value = value;
					return;
				}
				Console.PrintError( string.Format( "[STEAM] Attempted to set achievement statistic data for {0} to invalid data type (int)", Name ) );
			}
		};

		private Callback<UserStatsReceived_t> UserStatsReceived;
		private Callback<UserStatsStored_t> UserStatsStored;
		private Callback<UserAchievementStored_t> UserAchievementStored;

		private CallResult<UserStatsReceived_t> UserStatsReceivedCallResult;

		public static ConcurrentDictionary<string, SteamAchievement> AchievementTable;
		private bool SteamStatsReceived = false;

		/*
		===============
		_Ready
		===============
		*/
		/// <summary>
		/// godot initialization override
		/// </summary>
		public override void _Ready() {
			AchievementTable = new ConcurrentDictionary<string, SteamAchievement>();

			for ( uint i = 0; i < (uint)AchievementID.Count; i++ ) {
				AchievementID id = (AchievementID)i;
				AchievementTable.TryAdd( $"ACH_{id.ToString().ToUpper()}", new SteamAchievement( id, id.ToString() ) );
			}

			UserStatsReceived = Callback<UserStatsReceived_t>.Create( OnUserStatsReceived );
			UserStatsStored = Callback<UserStatsStored_t>.Create( OnUserStatsStored );
			UserAchievementStored = Callback<UserAchievementStored_t>.Create( OnUserAchievementStored );

			UserStatsReceivedCallResult = CallResult<UserStatsReceived_t>.Create( OnUserStatsReceived );

			if ( !SteamUserStats.RequestCurrentStats() ) {
				Console.PrintError( "[STEAM] Error fetching Steam stats!" );
			}
		}

		/*
		===============
		OnUserAchievementStored
		===============
		*/
		private void OnUserAchievementStored( UserAchievementStored_t pCallback ) {
			if ( pCallback.m_nGameID != (ulong)SteamManager.GetAppID() ) {
				Console.PrintError( "[STEAM] Incorrect AppID!" );
				return;
			}

			Console.PrintLine( string.Format( "[STEAM] Stored achievement data for {0}, progress: {1}/{2}", pCallback.m_rgchAchievementName, pCallback.m_nCurProgress, pCallback.m_nMaxProgress ) );
		}

		/*
		===============
		OnUserStatsReceived
		===============
		*/
		private void OnUserStatsReceived( UserStatsReceived_t pCallback, bool bIOFailure ) {
			if ( pCallback.m_eResult != EResult.k_EResultOK ) {
				Console.PrintError( "[STEAM] Error fetching steam user statistics" );
				return;
			}
			if ( pCallback.m_nGameID != (ulong)SteamManager.GetAppID() ) {
				Console.PrintError( "[STEAM] Incorrect AppID!" );
				return;
			}

			int count = 0;
			foreach ( var achievement in AchievementTable ) {
				if ( !SteamUserStats.GetAchievement( achievement.Value.IdString, out bool bAchieved ) ) {
					Console.PrintError( string.Format( "[STEAM] Error getting achievement data for {0}", achievement.Value.IdString ) );
					continue;
				}
				if ( bAchieved ) {
					count++;
				}
				Console.PrintLine( "Got achievement data for " + achievement.Value.IdString + ", status: " + bAchieved.ToString() );
				AchievementTable[ achievement.Key ].SetAchieved( bAchieved );
			}
			if ( count == AchievementTable.Count ) {
				ActivateAchievement( "ACH_THE_MAN_THE_MYTH_THE_LEGEND" );
			}
		}
		
		/*
		===============
		OnUserStatsReceived
		===============
		*/
		private void OnUserStatsReceived( UserStatsReceived_t pCallback ) {
			if ( pCallback.m_eResult != EResult.k_EResultOK ) {
				Console.PrintError( "[STEAM] Error fetching steam user statistics" );
				return;
			}
			if ( pCallback.m_nGameID != (ulong)SteamManager.GetAppID() ) {
				Console.PrintError( "[STEAM] Incorrect AppID!" );
				return;
			}

			Console.PrintLine( "Got local player statistics & achievements." );
			SteamStatsReceived = true;
		}

		/*
		===============
		OnUserStatsStored
		===============
		*/
		private void OnUserStatsStored( UserStatsStored_t pCallback ) {
			if ( pCallback.m_eResult != EResult.k_EResultOK ) {
				Console.PrintError( string.Format( "[STEAM] Couldn't store stats: {0}", pCallback.m_eResult ) );
			}
			if ( pCallback.m_nGameID != (ulong)SteamManager.GetAppID() ) {
				Console.PrintError( "[STEAM] Incorrect AppID!" );
			}
			Console.PrintLine( "[STEAM] Stored steam stats." );
		}

		/*
		===============
		SetAchievementProgress
		===============
		*/
		public static void SetAchievementProgress( string id, string statName, int nValue ) {
			if ( !AchievementTable.TryGetValue( id, out SteamAchievement achievement ) ) {
				Console.PrintError( string.Format( "[STEAM] Achievement {0} doesn't exist!", id ) );
				return;
			}

			achievement.SetIntValue( nValue );
			if ( !SteamUserStats.SetStat( statName, nValue ) ) {
				Console.PrintError( string.Format( "[STEAM] Error setting statistic \"{0}\" for achievement {0}!", statName, id ) );
				return;
			}
			while ( !SteamUserStats.StoreStats() ) {
				Console.PrintError( "[STEAM] Steam couldn't store stats!" );
			}
		}

		/*
		===============
		SetAchievementProgress
		===============
		*/
		public static void SetAchievementProgress( string id, string statName, float nValue ) {
			if ( !AchievementTable.TryGetValue( id, out SteamAchievement achievement ) ) {
				Console.PrintError( string.Format( "[STEAM] Achievement {0} doesn't exist!", id ) );
				return;
			}

			achievement.SetFloatValue( nValue );
			if ( !SteamUserStats.SetStat( statName, nValue ) ) {
				Console.PrintError( $"[STEAM] Error setting statistic \"{statName}\" for achievement {id}!" );
				return;
			}
			while ( !SteamUserStats.StoreStats() ) {
				Console.PrintError( "[STEAM] Steam couldn't store stats!" );
			}
		}

		/*
		===============
		ActivateAchievement
		===============
		*/
		public static void ActivateAchievement( string id ) {
			if ( !AchievementTable.TryGetValue( id, out SteamAchievement achievement ) ) {
				Console.PrintError( $"[STEAM] Achievement {id} doesn't exist!" );
				return;
			}

			achievement.SetAchieved( true );

			if ( !SteamUserStats.SetAchievement( id ) ) {
				Console.PrintError( "[STEAM] Error activating achievement!" );
				return;
			}
			while ( !SteamUserStats.StoreStats() ) {
				Console.PrintError( "[STEAM] Steam couldn't store stats!" );
			}
		}
	};
};