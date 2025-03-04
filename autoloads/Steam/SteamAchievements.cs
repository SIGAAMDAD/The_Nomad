using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Godot;
using Godot.Collections;
using GodotSteam;

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
		
		Count
	};

	public class SteamAchievement {
		private AchievementID Id;
		private string IdString;
		private string Name;
		private bool Achieved = false;
		private Variant Value = new Variant();

		public SteamAchievement( AchievementID id, string name ) {
			Id = id;
			IdString = "ACH_" + nameof( id ).ToUpper();
			Name = name;

			GD.Print( "Added SteamAPI Achievement " + IdString + "/\"" + Name + "\"" );
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

	private static System.Collections.Generic.Dictionary<string, SteamAchievement> AchievementTable;

	public override void _Ready() {
		AchievementTable = new System.Collections.Generic.Dictionary<string, SteamAchievement>();

		for ( uint i = 0; i < (uint)AchievementID.Count; i++ ) {
			AchievementID id = (AchievementID)i;
			AchievementTable.Add( "ACH_" + id.ToString().ToUpper(), new SteamAchievement( id, id.ToString() ) );
		}

		Steam.UserAchievementStored += ( gameId, groupAchieve, achievementName, currentProgress, maxProgress ) => {
		};

		Steam.UserStatsReceived += ( gameId, result, userId ) => {
			GD.Print( "Got local player statistics & achievements" );

			if ( userId != SteamManager.GetSteamID() ) {
				GD.PushError( "Not this user, aborting." );
				return;
			}
			if ( gameId != SteamManager.GetAppID() ) {
				GD.PushError( "Not this game, aborting." );
				return;
			}

			if ( result != 1 ) {
				GD.PushError( "Steam couldn't fetch stats: " + result.ToString() );
				return;
			}
			GD.Print( "Fetched user statistics." );

			foreach ( var achievement in AchievementTable ) {
				Dictionary steamStatus = Steam.GetAchievement( achievement.Value.GetIdString() );
				if ( !(bool)steamStatus[ "ret" ] ) {
					continue;
				}
				AchievementTable[ achievement.Key ].SetAchieved( (bool)steamStatus[ "achieved" ] );
			}
		};

		Steam.RequestUserStats( SteamManager.GetSteamID() );
	}

	public static void ActivateAchievement( string id ) {
		if ( !AchievementTable.ContainsKey( id ) ) {
			GD.PushError( "[STEAM] Achievement " + id + " doesn't exist!" );
			return;
		}

		Dictionary achievement = Steam.GetAchievement( id );
		if ( (bool)achievement[ "achieved" ] ) {
			return;
		}

		Steam.SetAchievement( id );
		Steam.StoreStats();
	}
};