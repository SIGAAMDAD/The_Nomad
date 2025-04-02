using System.Collections.Generic;
using Godot;

public class ResourceCache {
#region Mob Sound Effects
	public static AudioStream[] TargetSpotted;
	public static AudioStream[] HeavyDead;
	public static AudioStream[] Ceasefire;
	public static AudioStream[] Alert;
	public static AudioStream[] Confusion;
	public static AudioStream[] ManDown;
	public static AudioStream[] Curse;
	public static AudioStream[] TargetPinned;
	public static AudioStream[] TargetRunning;
	public static AudioStream[] OutOfTheWay;
	public static AudioStream[] CheckItOut;
	public static AudioStream[] Quiet;
	public static AudioStream ManDown2;
	public static AudioStream ManDown3;
	public static AudioStream Deaf;
	public static AudioStream SquadWiped;
	public static AudioStream[] NeedBackup;
	public static AudioStream Unstoppable;
	
	public static AudioStream[] Help;
	public static AudioStream[] RepeatPlease;
#endregion

	public static AudioStream NoAmmoSfx;

	public static AudioStream Fire;
	public static AudioStream[] BulletShell;
	public static AudioStream[] ShotgunShell;
	public static AudioStream[] MoveGravelSfx;
	public static AudioStream[] MoveWaterSfx;

	public static AudioStream ActivatedCheckpointSfx;
	public static AudioStream CampfireAmbienceSfx;

#region Player Sound Effects
	public static AudioStream LeapOfFaithSfx;
	public static AudioStream ChangeWeaponSfx;
	public static AudioStream[] PlayerPainSfx;
	public static AudioStream[] PlayerDieSfx;
	public static AudioStream[] PlayerDeathSfx;
	public static AudioStream[] DashSfx;
	public static AudioStream[] SlideSfx;
	public static AudioStream DashExplosion;
	public static AudioStream SlowMoBeginSfx;
	public static AudioStream SlowMoEndSfx;
#endregion

	private static Dictionary<string, AudioStream> AudioCache = new Dictionary<string, AudioStream>();
	private static Dictionary<string, Texture2D> TextureCache = new Dictionary<string, Texture2D>();
	private static Dictionary<string, PackedScene> SceneCache = new Dictionary<string, PackedScene>();

	public static bool Initialized = false;

	public static AudioStream GetSound( string key ) {
		if ( AudioCache.TryGetValue( key, out AudioStream value ) ) {
			return value;
		}
		value = ResourceLoader.Load<AudioStream>( key );
		AudioCache.Add( key, value );
		return value;
	}
	public static Texture2D GetTexture( string key ) {
		if ( TextureCache.TryGetValue( key, out Texture2D value ) ) {
			return value;
		}
		value = ResourceLoader.Load<Texture2D>( key );
		TextureCache.Add( key, value );
		return value;
	}
	public static PackedScene GetScene( string key ) {
		if ( SceneCache.TryGetValue( key, out PackedScene value ) ) {
			return value;
		}
		value = ResourceLoader.Load<PackedScene>( key );
		SceneCache.Add( key, value );
		return value;
	}

	public static void Cache( Node world ) {
		Console.PrintLine( "Loading sound effects..." );

		TargetSpotted = new AudioStream[ 16 ];
		TargetSpotted[0] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/21198.mp3" );
		TargetSpotted[1] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/21199.mp3" );
		TargetSpotted[2] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/21167.mp3" );
		TargetSpotted[3] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/21200.mp3" );
		TargetSpotted[4] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/21201.mp3" );
		TargetSpotted[5] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/21202.mp3" );
		TargetSpotted[6] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/21203.mp3" );
		TargetSpotted[7] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/21204.mp3" );
		TargetSpotted[8] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/21205.mp3" );
		TargetSpotted[9] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/21207.mp3" );
		TargetSpotted[10] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/21208.mp3" );
		TargetSpotted[11] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/21209.mp3" );
		TargetSpotted[12] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/21210.mp3" );
		TargetSpotted[13] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/21211.mp3" );
		TargetSpotted[14] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/21212.mp3" );
		TargetSpotted[15] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/21213.mp3" );

		Quiet = new AudioStream[ 5 ];
		Quiet[0] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/quiet_cmd_0.mp3" );
		Quiet[1] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/quiet_cmd_1.mp3" );
		Quiet[2] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/quiet_cmd_2.mp3" );
		Quiet[3] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/quiet_cmd_3.mp3" );
		Quiet[4] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/quiet_cmd_4.mp3" );

		TargetPinned = new AudioStream[ 4 ];
		TargetPinned[0] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/21161.mp3" );
		TargetPinned[1] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/21162.mp3" );
		TargetPinned[2] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/21163.mp3" );
		TargetPinned[3] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/21186.mp3" );

		TargetRunning = new AudioStream[ 4 ];
		TargetRunning[0] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/21156.mp3" );
		TargetRunning[1] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/21157.mp3" );
		TargetRunning[2] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/21159.mp3" );
		TargetRunning[3] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/21160.mp3" );

		Ceasefire = new AudioStream[ 4 ];
		Ceasefire[0] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/ceasefire_cmd_0.mp3" );
		Ceasefire[1] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/ceasefire_cmd_2.mp3" );
		Ceasefire[2] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/ceasefire_cmd_3.mp3" );
		Ceasefire[3] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/ceasefire_cmd_4.mp3" );

		OutOfTheWay = new AudioStream[ 6 ];
		OutOfTheWay[0] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/21376.mp3" );
		OutOfTheWay[1] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/21377.mp3" );
		OutOfTheWay[2] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/21381.mp3" );
		OutOfTheWay[3] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/get_down_cmd_0.mp3" );
		OutOfTheWay[5] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/get_down_cmd_1.mp3" );

		Curse = new AudioStream[ 3 ];
		Curse[0] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/21009.mp3" );
		Curse[1] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/21010.mp3" );
		Curse[2] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/21011.mp3" );

		Alert = new AudioStream[ 9 ];
		Alert[0] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/21164.mp3" );
		Alert[1] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/21170.mp3" );
		Alert[2] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/21169.mp3" );
		Alert[3] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/21028.mp3" );
		Alert[4] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/21029.mp3" );
		Alert[5] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/21030.mp3" );
		Alert[6] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/21033.mp3" );
		Alert[7] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/21034.mp3" );
		Alert[8] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/21026.mp3" );

		CheckItOut = new AudioStream[ 4 ];
		CheckItOut[0] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/21100.mp3" );
		CheckItOut[1] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/21172.mp3" );
		CheckItOut[2] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/21175.mp3" );
		CheckItOut[3] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/search_area_cmd_0.mp3" );

		Confusion = new AudioStream[ 6 ];
		Confusion[0] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/21164.mp3" );
		Confusion[1] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/21165.mp3" );
		Confusion[2] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/21168.mp3" );
		Confusion[3] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/21169.mp3" );
		Confusion[4] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/21170.mp3" );
		Confusion[5] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/21171.mp3" );

		HeavyDead = new AudioStream[ 3 ];
		HeavyDead[0] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/14859.mp3" );
		HeavyDead[1] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/14860.mp3" );
		HeavyDead[2] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/14861.mp3" );

		ManDown = new AudioStream[ 2 ];
		ManDown[0] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/21348.mp3" );
		ManDown[1] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/21359.mp3" );

		ManDown2 = ResourceLoader.Load<AudioStream>( "res://sounds/barks/man_down_2_callout_0.mp3" );
		ManDown2 = ResourceLoader.Load<AudioStream>( "res://sounds/barks/men_down_3_callout_0.mp3" );
		Deaf = ResourceLoader.Load<AudioStream>( "res://sounds/barks/deaf_callout.mp3" );
		SquadWiped = ResourceLoader.Load<AudioStream>( "res://sounds/barks/squad_wiped_callout_0.mp3" );

		NeedBackup = new AudioStream[ 5 ];
		NeedBackup[0] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/21193.mp3" );
		NeedBackup[1] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/21194.mp3" );
		NeedBackup[2] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/21195.mp3" );
		NeedBackup[3] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/21196.mp3" );
		NeedBackup[4] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/21197.mp3" );

		Help = new AudioStream[ 2 ];
		Help[0] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/21189.mp3" );
		Help[1] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/21190.mp3" );

		RepeatPlease = new AudioStream[ 2 ];
		RepeatPlease[0] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/14865.mp3" );
		RepeatPlease[1] = ResourceLoader.Load<AudioStream>( "res://sounds/barks/14866.mp3" );

		BulletShell = new AudioStream[ 2 ];
		BulletShell[0] = ResourceLoader.Load<AudioStream>( "res://sounds/env/bullet_shell_0.ogg" );
		BulletShell[1] = ResourceLoader.Load<AudioStream>( "res://sounds/env/bullet_shell_1.ogg" );

		ShotgunShell = new AudioStream[ 2 ];
		ShotgunShell[0] = ResourceLoader.Load<AudioStream>( "res://sounds/env/shotgun_shell_0.ogg" );
		ShotgunShell[1] = ResourceLoader.Load<AudioStream>( "res://sounds/env/shotgun_shell_1.ogg" );
		
		Fire = ResourceLoader.Load<AudioStream>( "res://sounds/env/fire.ogg" );

		ChangeWeaponSfx = ResourceLoader.Load<AudioStream>( "res://sounds/player/change_weapon.ogg" );

		NoAmmoSfx = ResourceLoader.Load<AudioStream>( "res://sounds/weapons/noammo.wav" );

		PlayerPainSfx = new AudioStream[ 3 ];
		PlayerPainSfx[0] = ResourceLoader.Load<AudioStream>( "res://sounds/player/pain0.ogg" );
		PlayerPainSfx[1] = ResourceLoader.Load<AudioStream>( "res://sounds/player/pain1.ogg" );
		PlayerPainSfx[2] = ResourceLoader.Load<AudioStream>( "res://sounds/player/pain2.ogg" );
		
		PlayerDieSfx = new AudioStream[ 3 ];
		PlayerDieSfx[0] = ResourceLoader.Load<AudioStream>( "res://sounds/player/death1.ogg" );
		PlayerDieSfx[1] = ResourceLoader.Load<AudioStream>( "res://sounds/player/death2.ogg" );
		PlayerDieSfx[2] = ResourceLoader.Load<AudioStream>( "res://sounds/player/death3.ogg" );

		PlayerDeathSfx = new AudioStream[3];
		PlayerDeathSfx[0] = ResourceLoader.Load<AudioStream>( "res://sounds/player/dying_0.ogg" );
		PlayerDeathSfx[1] = ResourceLoader.Load<AudioStream>( "res://sounds/player/dying_1.ogg" );
		PlayerDeathSfx[2] = ResourceLoader.Load<AudioStream>( "res://sounds/player/dying_2.wav" );

		MoveGravelSfx = new AudioStream[ 4 ];
		MoveGravelSfx[0] = ResourceLoader.Load<AudioStream>( "res://sounds/env/moveGravel0.ogg" );
		MoveGravelSfx[1] = ResourceLoader.Load<AudioStream>( "res://sounds/env/moveGravel1.ogg" );
		MoveGravelSfx[2] = ResourceLoader.Load<AudioStream>( "res://sounds/env/moveGravel2.ogg" );
		MoveGravelSfx[3] = ResourceLoader.Load<AudioStream>( "res://sounds/env/moveGravel3.ogg" );

		MoveWaterSfx = new AudioStream[ 4 ];
		MoveWaterSfx[0] = ResourceLoader.Load<AudioStream>( "res://sounds/env/moveWater0.ogg" );
		MoveWaterSfx[1] = ResourceLoader.Load<AudioStream>( "res://sounds/env/moveWater1.ogg" );

		DashSfx = new AudioStream[ 2 ];
		DashSfx[0] = ResourceLoader.Load<AudioStream>( "res://sounds/player/jumpjet_burn_v2_m_01.wav" );
		DashSfx[1] = ResourceLoader.Load<AudioStream>( "res://sounds/player/jumpjet_burn_v2_m_02.wav" );
		
		SlideSfx = new AudioStream[ 2 ];
		SlideSfx[0] = ResourceLoader.Load<AudioStream>( "res://sounds/player/slide0.ogg" );
		SlideSfx[1] = ResourceLoader.Load<AudioStream>( "res://sounds/player/slide1.ogg" );

		SlowMoBeginSfx = ResourceLoader.Load<AudioStream>( "res://sounds/player/slowmo_begin.ogg" );
		SlowMoEndSfx = ResourceLoader.Load<AudioStream>( "res://sounds/player/slowmo_end.ogg" );
		DashExplosion = ResourceLoader.Load<AudioStream>( "res://sounds/player/dash_explosion.mp3" );

		LeapOfFaithSfx = ResourceLoader.Load<AudioStream>( "res://sounds/player/leap_of_faith.ogg" );

		ActivatedCheckpointSfx = ResourceLoader.Load<AudioStream>( "res://sounds/env/bonfire_create.ogg" );
		CampfireAmbienceSfx = ResourceLoader.Load<AudioStream>( "res://sounds/env/campfire.ogg" );

		world.CallDeferred( "emit_signal", "ResourcesLoadingFinished" );
	}
};