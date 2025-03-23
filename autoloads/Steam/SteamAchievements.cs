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
		
		Count
	};

	public class SteamAchievement {
		private AchievementID Id;
		private string IdString;
		private string Name;
		private bool Achieved = false;
		private object Value = null;
		private object MaxValue = null;

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

		public object GetValue() {
			return Value;
		}
		public object GetMaxValue() {
			return MaxValue;
		}
		public string GetName() {
			return Name;
		}
		public string GetIdString() {
			return IdString;
		}
		public void SetAchieved( bool bAchieved ) {
			Achieved = bAchieved;
		}
    };

	private Callback<UserStatsReceived_t> UserStatsReceived;
	private Callback<UserStatsStored_t> UserStatsStored;
	private Callback<UserAchievementStored_t> UserAchievementStored;

	private CallResult<UserStatsReceived_t> UserStatsReceivedCallResult;
	
	private static System.Collections.Generic.Dictionary<string, SteamAchievement> AchievementTable;
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
			GD.PushError( "[STEAM] Error fetching Steam stats!" );
		}
	}

    private void OnUserAchievementStored( UserAchievementStored_t pCallback ) {
		if ( pCallback.m_nGameID != (ulong)SteamManager.GetAppID() ) {
			GD.PushError( "[STEAM] Incorrect AppID!" );
			return;
		}

		GD.Print( "[STEAM] Stored achievement data for " + pCallback.m_rgchAchievementName + ", progress: " + pCallback.m_nCurProgress + "/" + pCallback.m_nMaxProgress );
	}
	private void OnUserStatsReceived( UserStatsReceived_t pCallback, bool bIOFailure ) {
		if ( pCallback.m_eResult != EResult.k_EResultOK ) {
			GD.PushError( "[STEAM] Error fetching steam user statistics" );
			return;
		}
		if ( pCallback.m_nGameID != (ulong)SteamManager.GetAppID() ) {
			GD.PushError( "[STEAM] Incorrect AppID!" );
			return;
		}

		foreach ( var achievement in AchievementTable ) {
			bool bAchieved;
			if ( !SteamUserStats.GetAchievement( achievement.Value.GetIdString(), out bAchieved ) ) {
				GD.PushError( "[STEAM] Error getting achievement data for " + achievement.Value.GetIdString() );
				continue;
			}
			GD.Print( "Got achievement data for " + achievement.Value.GetIdString() + ", status: " + bAchieved.ToString() );
			AchievementTable[ achievement.Key ].SetAchieved( bAchieved );
		}
	}
	private void OnUserStatsReceived( UserStatsReceived_t pCallback ) {
		if ( pCallback.m_eResult != EResult.k_EResultOK ) {
			GD.PushError( "[STEAM] Error fetching steam user statistics" );
			return;
		}
		if ( pCallback.m_nGameID != (ulong)SteamManager.GetAppID() ) {
			GD.PushError( "[STEAM] Incorrect AppID!" );
			return;
		}

		GetNode( "/root/Console" ).Call( "print_line", "Got local player statistics & achievements.", true );
		SteamStatsReceived = true;
	}
	private void OnUserStatsStored( UserStatsStored_t pCallback ) {
		if ( pCallback.m_eResult != EResult.k_EResultOK ) {
			GD.PushError( "[STEAM] Couldn't store stats: " + pCallback.m_eResult );
		}
		if ( pCallback.m_nGameID != (ulong)SteamManager.GetAppID() ) {
			GD.PushError( "[STEAM] Incorrect AppID!" );
		}
	}

	public static void SetAchievementProgress( string id, string statName, int nValue ) {
		if ( !AchievementTable.ContainsKey( id ) ) {
			GD.PushError( "[STEAM] Achievement " + id + " doesn't exist!" );
			return;
		}

		SteamUserStats.SetStat( statName, nValue );
		while ( !SteamUserStats.StoreStats() ) {
			GD.PushError( "[STEAM] Steam couldn't store stats!" );
		}
	}
	public static void SetAchievementProgress( string id, string statName, float nValue ) {
		if ( !AchievementTable.ContainsKey( id ) ) {
			GD.PushError( "[STEAM] Achievement " + id + " doesn't exist!" );
			return;
		}

		SteamUserStats.SetStat( statName, nValue );
		while ( !SteamUserStats.StoreStats() ) {
			GD.PushError( "[STEAM] Steam couldn't store stats!" );
		}
	}
	public static void ActivateAchievement( string id ) {
		if ( !AchievementTable.ContainsKey( id ) ) {
			GD.PushError( "[STEAM] Achievement " + id + " doesn't exist!" );
			return;
		}

		AchievementTable[ id ].SetAchieved( true );

		if ( !SteamUserStats.SetAchievement( id ) ) {
			GD.PushError( "[STEAM] Error activating achievement!" );
			return;
		}
		while ( !SteamUserStats.StoreStats() ) {
			GD.PushError( "[STEAM] Steam couldn't store stats!" );
		}
	}
};