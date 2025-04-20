using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
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
	public static AudioStream[] Pain;
	public static AudioStream[] Die;
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

	public static Texture2D Light;

	public static Resource ItemDatabase;

	public static Resource KeyboardInputMappings;
	public static Resource GamepadInputMappings;

	private static ConcurrentDictionary<string, AudioStream> AudioCache = new ConcurrentDictionary<string, AudioStream>( 1024, 1024 );
	private static ConcurrentDictionary<string, Texture2D> TextureCache = new ConcurrentDictionary<string, Texture2D>( 1024, 1024 );
	private static ConcurrentDictionary<string, PackedScene> SceneCache = new ConcurrentDictionary<string, PackedScene>( 1024, 1024 );

	public static bool Initialized = false;

	public static AudioStream GetSound( string key ) {
		if ( AudioCache.TryGetValue( key, out AudioStream value ) ) {
			return value;
		}
		value = ResourceLoader.Load<AudioStream>( key );
		AudioCache.TryAdd( key, value );
		return value;
	}
	public static Texture2D GetTexture( string key ) {
		if ( TextureCache.TryGetValue( key, out Texture2D value ) ) {
			return value;
		}
		value = ResourceLoader.Load<Texture2D>( key );
		TextureCache.TryAdd( key, value );
		return value;
	}
	public static PackedScene GetScene( string key ) {
		if ( SceneCache.TryGetValue( key, out PackedScene value ) ) {
			return value;
		}
		value = ResourceLoader.Load<PackedScene>( key );
		SceneCache.TryAdd( key, value );
		return value;
	}

	public static void Cache( Node world, System.Threading.Thread SceneLoadThread ) {
		Console.PrintLine( "Loading sound effects..." );

		SceneLoadThread?.Start();

		TargetSpotted = [
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21198.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21199.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21167.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21200.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21201.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21202.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21203.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21204.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21205.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21207.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21208.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21209.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21210.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21211.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21212.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21213.mp3" ),
		];

		Quiet = [
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/quiet_cmd_0.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/quiet_cmd_1.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/quiet_cmd_2.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/quiet_cmd_3.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/quiet_cmd_4.mp3" ),
		];

		TargetPinned = [
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21161.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21162.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21163.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21186.mp3" )
		];

		TargetRunning = [
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21156.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21157.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21159.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21160.mp3" ),
		];

		Ceasefire = [
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/ceasefire_cmd_0.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/ceasefire_cmd_2.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/ceasefire_cmd_3.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/ceasefire_cmd_4.mp3" ),
		];

		OutOfTheWay = [
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21376.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21377.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21381.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/get_down_cmd_0.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/get_down_cmd_1.mp3" ),
		];

		Curse = [
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21009.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21010.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21011.mp3" ),
		];

		Alert = [
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21164.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21170.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21169.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21028.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21029.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21030.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21033.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21034.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21026.mp3" ),
		];

		CheckItOut = [
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21100.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21172.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21175.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/search_area_cmd_0.mp3" ),
		];

		Confusion = [
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21164.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21165.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21168.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21169.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21170.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21171.mp3" ),
		];

		HeavyDead = [
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/14859.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/14860.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/14861.mp3" ),
		];

		ManDown = [
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21348.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21359.mp3" ),
		];

		ManDown2 = ResourceLoader.Load<AudioStream>( "res://sounds/barks/man_down_2_callout_0.mp3" );
		ManDown2 = ResourceLoader.Load<AudioStream>( "res://sounds/barks/men_down_3_callout_0.mp3" );
		Deaf = ResourceLoader.Load<AudioStream>( "res://sounds/barks/deaf_callout.mp3" );
		SquadWiped = ResourceLoader.Load<AudioStream>( "res://sounds/barks/squad_wiped_callout_0.mp3" );

		Pain = [
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21304a.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21304c.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21304d.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21304e.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21304f.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21305a.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21305b.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21306a.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21306b.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21307a.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21307b.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21307c.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21307d.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21307e.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21307f.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21307g.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21307h.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21307i.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21308a.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21308b.mp3" ),
		];

		NeedBackup = [
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21193.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21194.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21195.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21196.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21197.mp3" ),
		];

		Help = [
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21189.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21190.mp3" ),
		];

		RepeatPlease = [
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/14865.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/14866.mp3" ),
		];

		BulletShell = [
			ResourceLoader.Load<AudioStream>( "res://sounds/env/bullet_shell_0.ogg" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/env/bullet_shell_1.ogg" ),
		];

		ShotgunShell = [
			ResourceLoader.Load<AudioStream>( "res://sounds/env/shotgun_shell_0.ogg" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/env/shotgun_shell_1.ogg" ),
		];
		
		Fire = ResourceLoader.Load<AudioStream>( "res://sounds/env/fire.ogg" );

		ChangeWeaponSfx = ResourceLoader.Load<AudioStream>( "res://sounds/player/change_weapon.ogg" );

		NoAmmoSfx = ResourceLoader.Load<AudioStream>( "res://sounds/weapons/noammo.wav" );

		PlayerPainSfx = [
			ResourceLoader.Load<AudioStream>( "res://sounds/player/pain0.ogg" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/player/pain1.ogg" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/player/pain2.ogg" ),
		];
		
		PlayerDieSfx = [
			ResourceLoader.Load<AudioStream>( "res://sounds/player/death1.ogg" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/player/death2.ogg" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/player/death3.ogg" ),
		];

		PlayerDeathSfx = [
			ResourceLoader.Load<AudioStream>( "res://sounds/player/dying_0.ogg" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/player/dying_1.ogg" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/player/dying_2.wav" ),
		];

		MoveGravelSfx = [
			ResourceLoader.Load<AudioStream>( "res://sounds/env/moveGravel0.wav" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/env/moveGravel1.wav" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/env/moveGravel2.wav" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/env/moveGravel3.wav" ),
		];

		MoveWaterSfx = [
			ResourceLoader.Load<AudioStream>( "res://sounds/env/moveWater0.ogg" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/env/moveWater1.ogg" ),
		];

		DashSfx = [
			ResourceLoader.Load<AudioStream>( "res://sounds/player/jumpjet_burn_v2_m_01.wav" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/player/jumpjet_burn_v2_m_02.wav" ),
		];
		
		SlideSfx = [
			ResourceLoader.Load<AudioStream>( "res://sounds/player/slide0.ogg" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/player/slide1.ogg" ),
		];

		SlowMoBeginSfx = ResourceLoader.Load<AudioStream>( "res://sounds/player/slowmo_begin.ogg" );
		SlowMoEndSfx = ResourceLoader.Load<AudioStream>( "res://sounds/player/slowmo_end.ogg" );
		DashExplosion = ResourceLoader.Load<AudioStream>( "res://sounds/player/dash_explosion.mp3" );

		LeapOfFaithSfx = ResourceLoader.Load<AudioStream>( "res://sounds/player/leap_of_faith.ogg" );

		ActivatedCheckpointSfx = ResourceLoader.Load<AudioStream>( "res://sounds/env/bonfire_create.ogg" );
		CampfireAmbienceSfx = ResourceLoader.Load<AudioStream>( "res://sounds/env/campfire.wav" );

		Light = ResourceLoader.Load<Texture2D>( "res://textures/point_light.dds" );

		SceneLoadThread?.Join();

		world.CallDeferred( "emit_signal", "ResourcesLoadingFinished" );
	}
};