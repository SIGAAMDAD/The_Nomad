using System;
using Godot;
using Steamworks;

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
		
		Count
	};

	public struct SteamAchievement {
		private readonly AchievementID Id;
		private readonly string IdString;
		private readonly string Name;
		private readonly object MaxValue = null;
		private bool Achieved = false;
		private object Value = null;

		public SteamAchievement( AchievementID id, string name ) {
			Id = id;
			IdString = "ACH_" + id.ToString().ToUpper();
			Name = name;
			Value = 0;

			GD.Print( "Added SteamAPI Achievement " + IdString + "/\"" + Name + "\"" );
		}
		public SteamAchievement( AchievementID id, string name, uint maxValue ) {
			Id = id;
			IdString = "ACH_" + id.ToString().ToUpper();
			Name = name;
			Value = 0;
			MaxValue = maxValue;

			GD.Print( "Added SteamAPI Achievement " + IdString + "/\"" + Name + "\"" );
		}
		public SteamAchievement( AchievementID id, string name, float maxValue ) {
			Id = id;
			IdString = "ACH_" + id.ToString().ToUpper();
			Name = name;
			Value = 0.0f;
			MaxValue = maxValue;

			GD.Print( "Added SteamAPI Achievement " + IdString + "/\"" + Name + "\"" );
		}

		public readonly object GetValue() => Value;
		public readonly object GetMaxValue() => MaxValue;
		public readonly string GetName() => Name;
		public readonly string GetIdString() => IdString;
		public readonly bool GetAchieved() => Achieved;
		public void SetAchieved( bool bAchieved ) => Achieved = bAchieved;
		public void SetFloatValue( float value ) {
			if ( Value is float floatValue ) {
				Value = value;
				return;
			}
			Console.PrintError( string.Format( "[STEAM] Attempted to set achievement statistic data for {0} to invalid data type (float)", Name ) );
		}
		public void SetIntValue( int value ) {
			if ( Value is int floatValue ) {
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
	
	public static System.Collections.Generic.Dictionary<string, SteamAchievement> AchievementTable;
	private bool SteamStatsReceived = false;

	public override void _Ready() {
		AchievementTable = new System.Collections.Generic.Dictionary<string, SteamAchievement>();

		for ( uint i = 0; i < (uint)AchievementID.Count; i++ ) {
			AchievementID id = (AchievementID)i;
			AchievementTable.Add( "ACH_" + id.ToString().ToUpper(), new SteamAchievement( id, id.ToString() ) );
		}

		UserStatsReceived = Callback<UserStatsReceived_t>.Create( OnUserStatsReceived );
		UserStatsStored = Callback<UserStatsStored_t>.Create( OnUserStatsStored );
		UserAchievementStored = Callback<UserAchievementStored_t>.Create( OnUserAchievementStored );

		UserStatsReceivedCallResult = CallResult<UserStatsReceived_t>.Create( OnUserStatsReceived );

		if ( !SteamUserStats.RequestCurrentStats() ) {
			Console.PrintError( "[STEAM] Error fetching Steam stats!" );
		}
	}

    private void OnUserAchievementStored( UserAchievementStored_t pCallback ) {
		if ( pCallback.m_nGameID != (ulong)SteamManager.GetAppID() ) {
			Console.PrintError( "[STEAM] Incorrect AppID!" );
			return;
		}

		Console.PrintLine( string.Format( "[STEAM] Stored achievement data for {0}, progress: {1}/{2}", pCallback.m_rgchAchievementName, pCallback.m_nCurProgress, pCallback.m_nMaxProgress ) );
	}
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
			if ( !SteamUserStats.GetAchievement( achievement.Value.GetIdString(), out bool bAchieved ) ) {
				Console.PrintError( string.Format( "[STEAM] Error getting achievement data for {0}", achievement.Value.GetIdString() ) );
				continue;
			}
			if ( bAchieved ) {
				count++;
			}
			Console.PrintLine( "Got achievement data for " + achievement.Value.GetIdString() + ", status: " + bAchieved.ToString() );
			AchievementTable[ achievement.Key ].SetAchieved( bAchieved );
		}
		if ( count == AchievementTable.Count ) {
			ActivateAchievement( "ACH_THE_MAN_THE_MYTH_THE_LEGEND" );
		}
	}
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
	private void OnUserStatsStored( UserStatsStored_t pCallback ) {
		if ( pCallback.m_eResult != EResult.k_EResultOK ) {
			Console.PrintError( string.Format( "[STEAM] Couldn't store stats: {0}", pCallback.m_eResult ) );
		}
		if ( pCallback.m_nGameID != (ulong)SteamManager.GetAppID() ) {
			Console.PrintError( "[STEAM] Incorrect AppID!" );
		}
		Console.PrintLine( "[STEAM] Stored steam stats." );
	}

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
	public static void SetAchievementProgress( string id, string statName, float nValue ) {
		if ( !AchievementTable.TryGetValue( id, out SteamAchievement achievement ) ) {
			Console.PrintError( string.Format( "[STEAM] Achievement {0} doesn't exist!", id ) );
			return;
		}

		achievement.SetFloatValue( nValue );
		if ( !SteamUserStats.SetStat( statName, nValue ) ) {
			Console.PrintError( string.Format( "[STEAM] Error setting statistic \"{0}\" for achievement {0}!", statName, id ) );
			return;
		}
		while ( !SteamUserStats.StoreStats() ) {
			Console.PrintError( "[STEAM] Steam couldn't store stats!" );
		}
	}
	public static void ActivateAchievement( string id ) {
		if ( !AchievementTable.TryGetValue( id, out SteamAchievement achievement ) ) {
			Console.PrintError( string.Format( "[STEAM] Achievement {0} doesn't exist!", id ) );
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