using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DialogueManagerRuntime;
using Godot;
using Renown.World;

public class ResourceCache {
#region Mob Sound Effects
	public static AudioStream[] TargetSpotted { get; private set; }
	public static AudioStream[] HeavyDead { get; private set; }
	public static AudioStream[] Ceasefire { get; private set; }
	public static AudioStream[] Alert { get; private set; }
	public static AudioStream[] Confusion { get; private set; }
	public static AudioStream[] ManDown { get; private set; }
	public static AudioStream[] Curse { get; private set; }
	public static AudioStream[] TargetPinned { get; private set; }
	public static AudioStream[] TargetRunning { get; private set; }
	public static AudioStream[] OutOfTheWay { get; private set; }
	public static AudioStream[] CheckItOut { get; private set; }
	public static AudioStream[] Quiet { get; private set; }
	public static AudioStream[] Pain { get; private set; }
	public static AudioStream[] Die { get; private set; }
	public static AudioStream ManDown2 { get; private set; }
	public static AudioStream ManDown3 { get; private set; }
	public static AudioStream Deaf { get; private set; }
	public static AudioStream SquadWiped { get; private set; }
	public static AudioStream[] NeedBackup { get; private set; }
	public static AudioStream Unstoppable { get; private set; }
	
	public static AudioStream[] Help { get; private set; }
	public static AudioStream[] RepeatPlease { get; private set; }
#endregion

	public static AudioStream NoAmmoSfx { get; private set; }

	public static AudioStream Fire { get; private set; }
	public static AudioStream[] BulletShell { get; private set; }
	public static AudioStream[] ShotgunShell { get; private set; }
	public static AudioStream[] MoveGravelSfx { get; private set; }
	public static AudioStream[] MoveSandSfx { get; private set; }
	public static AudioStream[] MoveStoneSfx { get; private set; }
	public static AudioStream[] MoveWoodSfx { get; private set; }
	public static AudioStream[] MoveWaterSfx { get; private set; }

	public static AudioStream ActivatedCheckpointSfx { get; private set; }
	public static AudioStream CampfireAmbienceSfx { get; private set; }

#region Player Sound Effects
	public static AudioStream LeapOfFaithSfx { get; private set; }
	public static AudioStream ChangeWeaponSfx { get; private set; }
	public static AudioStream[] PlayerPainSfx { get; private set; }
	public static AudioStream[] PlayerDieSfx { get; private set; }
	public static AudioStream[] PlayerDeathSfx { get; private set; }
	public static AudioStream[] DashSfx { get; private set; }
	public static AudioStream[] SlideSfx { get; private set; }
	public static AudioStream DashExplosion { get; private set; }
	public static AudioStream SlowMoBeginSfx { get; private set; }
	public static AudioStream SlowMoEndSfx { get; private set; }
#endregion

	public static Texture2D Light { get; private set; }

	public static Resource ItemDatabase { get; private set; }

	public static Resource[] MoveActionGamepad { get; private set; }
	public static Resource[] DashActionGamepad { get; private set; }
	public static Resource[] SlideActionGamepad { get; private set; }
	public static Resource[] MeleeActionGamepad { get; private set; }
	public static Resource[] UseWeaponActionGamepad { get; private set; }
	public static Resource[] SwitchWeaponModeActionGamepad { get; private set; }
	public static Resource[] NextWeaponActionGamepad { get; private set; }
	public static Resource[] PrevWeaponActionGamepad { get; private set; }
	public static Resource[] OpenInventoryActionGamepad { get; private set; }
	public static Resource[] BulletTimeActionGamepad { get; private set; }
	public static Resource[] ArmAngleActionGamepad { get; private set; }
	public static Resource[] UseBothHandsActionsGamepad { get; private set; }

	public static Resource MoveActionKeyboard { get; private set; }
	public static Resource DashActionKeyboard { get; private set; }
	public static Resource SlideActionKeyboard { get; private set; }
	public static Resource MeleeActionKeyboard { get; private set; }
	public static Resource UseWeaponActionKeyboard { get; private set; }
	public static Resource SwitchWeaponModeActionKeyboard { get; private set; }
	public static Resource NextWeaponActionKeyboard { get; private set; }
	public static Resource PrevWeaponActionKeyboard { get; private set; }
	public static Resource OpenInventoryActionKeyboard { get; private set; }
	public static Resource BulletTimeActionKeyboard { get; private set; }
	public static Resource ArmAngleActionKeyboard { get; private set; }
	public static Resource UseBothHandsActionsKeyboard { get; private set; }

	public static Resource KeyboardInputMappings { get; private set; }
	public static Resource GamepadInputMappings { get; private set; }

	private static ConcurrentDictionary<string, Resource> DialogueCache = new ConcurrentDictionary<string, Resource>( 1024, 256 );
	private static ConcurrentDictionary<string, AudioStream> AudioCache = new ConcurrentDictionary<string, AudioStream>( 1024, 256 );
	private static ConcurrentDictionary<string, Texture2D> TextureCache = new ConcurrentDictionary<string, Texture2D>( 1024, 256 );
	private static ConcurrentDictionary<string, PackedScene> SceneCache = new ConcurrentDictionary<string, PackedScene>( 1024, 256 );
	private static Dictionary<string, SpriteFrames> SpriteFramesCache = new Dictionary<string, SpriteFrames>( 256 );

	public static bool Initialized = false;

	public static SpriteFrames GetSpriteFrames( string key ) {
		if ( SpriteFramesCache.TryGetValue( key, out SpriteFrames value ) ) {
			return value;
		}
		value = ResourceLoader.Load<SpriteFrames>( key );
		SpriteFramesCache.TryAdd( key, value );
		return value;
	}
	public static Resource CreateDialogue( string key, string text ) {
		if ( DialogueCache.TryGetValue( key, out Resource value ) ) {
			return value;
		}
		value = DialogueManager.CreateResourceFromText( text );
		DialogueCache.TryAdd( key, value );
		return value;
	}
	public static Resource GetDialogue( string key ) {
		if ( DialogueCache.TryGetValue( key, out Resource value ) ) {
			return value;
		}
		value = ResourceLoader.Load( "res://resources/dialogue/" + key + ".dialogue" );
		DialogueCache.TryAdd( key, value );
		return value;
	}
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

	public static void LoadGamepadBinds() {
		MoveActionGamepad ??= [
			ResourceLoader.Load( "res://resources/binds/actions/gamepad/move_player0.tres" ),
			ResourceLoader.Load( "res://resources/binds/actions/gamepad/move_player1.tres" ),
			ResourceLoader.Load( "res://resources/binds/actions/gamepad/move_player2.tres" ),
			ResourceLoader.Load( "res://resources/binds/actions/gamepad/move_player3.tres" )
		];
		DashActionGamepad ??= [
			ResourceLoader.Load( "res://resources/binds/actions/gamepad/dash_player0.tres" ),
			ResourceLoader.Load( "res://resources/binds/actions/gamepad/dash_player1.tres" ),
			ResourceLoader.Load( "res://resources/binds/actions/gamepad/dash_player2.tres" ),
			ResourceLoader.Load( "res://resources/binds/actions/gamepad/dash_player3.tres" )
		];
		SlideActionGamepad ??= [
			ResourceLoader.Load( "res://resources/binds/actions/gamepad/slide_player0.tres" ),
			ResourceLoader.Load( "res://resources/binds/actions/gamepad/slide_player1.tres" ),
			ResourceLoader.Load( "res://resources/binds/actions/gamepad/slide_player2.tres" ),
			ResourceLoader.Load( "res://resources/binds/actions/gamepad/slide_player3.tres" )
		];
		UseWeaponActionGamepad ??= [
			ResourceLoader.Load( "res://resources/binds/actions/gamepad/use_weapon_player0.tres" ),
			ResourceLoader.Load( "res://resources/binds/actions/gamepad/use_weapon_player1.tres" ),
			ResourceLoader.Load( "res://resources/binds/actions/gamepad/use_weapon_player2.tres" ),
			ResourceLoader.Load( "res://resources/binds/actions/gamepad/use_weapon_player3.tres" )
		];
		NextWeaponActionGamepad ??= [
			ResourceLoader.Load( "res://resources/binds/actions/gamepad/next_weapon_player0.tres" ),
			ResourceLoader.Load( "res://resources/binds/actions/gamepad/next_weapon_player1.tres" ),
			ResourceLoader.Load( "res://resources/binds/actions/gamepad/next_weapon_player2.tres" ),
			ResourceLoader.Load( "res://resources/binds/actions/gamepad/next_weapon_player3.tres" )
		];
		PrevWeaponActionGamepad ??= [
			ResourceLoader.Load( "res://resources/binds/actions/gamepad/prev_weapon_player0.tres" ),
			ResourceLoader.Load( "res://resources/binds/actions/gamepad/prev_weapon_player1.tres" ),
			ResourceLoader.Load( "res://resources/binds/actions/gamepad/prev_weapon_player2.tres" ),
			ResourceLoader.Load( "res://resources/binds/actions/gamepad/prev_weapon_player3.tres" )
		];
		//		SwitchWeaponModeActionGamepad = [
		//			ResourceLoader.Load( "res://resources/binds/actions/gamepad/switch_weapon_mode_player0.tres" ),
		//			ResourceLoader.Load( "res://resources/binds/actions/gamepad/switch_weapon_mode_player1.tres" ),
		//			ResourceLoader.Load( "res://resources/binds/actions/gamepad/switch_weapon_mode_player2.tres" ),
		//			ResourceLoader.Load( "res://resources/binds/actions/gamepad/switch_weapon_mode_player3.tres" )
		//		];
		BulletTimeActionGamepad ??= [
			ResourceLoader.Load( "res://resources/binds/actions/gamepad/bullet_time_player0.tres" ),
			ResourceLoader.Load( "res://resources/binds/actions/gamepad/bullet_time_player1.tres" ),
			ResourceLoader.Load( "res://resources/binds/actions/gamepad/bullet_time_player2.tres" ),
			ResourceLoader.Load( "res://resources/binds/actions/gamepad/bullet_time_player3.tres" )
		];
		OpenInventoryActionGamepad ??= [
			ResourceLoader.Load( "res://resources/binds/actions/gamepad/open_inventory_player0.tres" ),
			ResourceLoader.Load( "res://resources/binds/actions/gamepad/open_inventory_player1.tres" ),
			ResourceLoader.Load( "res://resources/binds/actions/gamepad/open_inventory_player2.tres" ),
			ResourceLoader.Load( "res://resources/binds/actions/gamepad/open_inventory_player3.tres" )
		];
		ArmAngleActionGamepad ??= [
			ResourceLoader.Load( "res://resources/binds/actions/gamepad/arm_angle_player0.tres" ),
			ResourceLoader.Load( "res://resources/binds/actions/gamepad/arm_angle_player1.tres" ),
			ResourceLoader.Load( "res://resources/binds/actions/gamepad/arm_angle_player2.tres" ),
			ResourceLoader.Load( "res://resources/binds/actions/gamepad/arm_angle_player3.tres" )
		];
		MeleeActionGamepad ??= [
			ResourceLoader.Load( "res://resources/binds/actions/gamepad/melee_player0.tres" ),
			ResourceLoader.Load( "res://resources/binds/actions/gamepad/melee_player1.tres" ),
			ResourceLoader.Load( "res://resources/binds/actions/gamepad/melee_player2.tres" ),
			ResourceLoader.Load( "res://resources/binds/actions/gamepad/melee_player3.tres" )
		];
		//		UseBothHandsActionsGamepad = [
		//			ResourceLoader.Load( "res://resources/binds/actions/gamepad/use_both_hands_player0.tres" ),
		//			ResourceLoader.Load( "res://resources/binds/actions/gamepad/use_both_hands_player1.tres" ),
		//			ResourceLoader.Load( "res://resources/binds/actions/gamepad/use_both_hands_player2.tres" ),
		//			ResourceLoader.Load( "res://resources/binds/actions/gamepad/use_both_hands_player3.tres" )
		//		];
	}
	public static void LoadKeyboardBinds() {
		MoveActionKeyboard ??= ResourceLoader.Load( "res://resources/binds/actions/keyboard/move.tres" );
		DashActionKeyboard ??= ResourceLoader.Load( "res://resources/binds/actions/keyboard/dash.tres" );
		SlideActionKeyboard ??= ResourceLoader.Load( "res://resources/binds/actions/keyboard/slide.tres" );
		UseWeaponActionKeyboard ??= ResourceLoader.Load( "res://resources/binds/actions/keyboard/use_weapon.tres" );
		NextWeaponActionKeyboard ??= ResourceLoader.Load( "res://resources/binds/actions/keyboard/next_weapon.tres" );
		PrevWeaponActionKeyboard ??= ResourceLoader.Load( "res://resources/binds/actions/keyboard/prev_weapon.tres" );
		SwitchWeaponModeActionKeyboard ??= ResourceLoader.Load( "res://resources/binds/actions/keyboard/switch_weapon_mode.tres" );
		BulletTimeActionKeyboard ??= ResourceLoader.Load( "res://resources/binds/actions/keyboard/bullet_time.tres" );
		OpenInventoryActionKeyboard ??= ResourceLoader.Load( "res://resources/binds/actions/keyboard/open_inventory.tres" );
		ArmAngleActionKeyboard ??= ResourceLoader.Load( "res://resources/binds/actions/keyboard/arm_angle.tres" );
		MeleeActionKeyboard ??= ResourceLoader.Load( "res://resources/binds/actions/keyboard/melee.tres" );
		UseBothHandsActionsKeyboard ??= ResourceLoader.Load( "res://resources/binds/actions/keyboard/use_both_hands.tres" );
	}
	public static void LoadBinds() {
		KeyboardInputMappings ??= ResourceLoader.Load( "res://resources/binds/binds_keyboard.tres" );
		GamepadInputMappings ??= ResourceLoader.Load( "res://resources/binds/binds_gamepad.tres" );
	}

	public static void Cache( Node world, Thread SceneLoadThread ) {
		Console.PrintLine( "Loading sound effects..." );

		SceneLoadThread?.Start();

		long[] WorkerThreads = [
			WorkerThreadPool.AddTask( Callable.From( () => { world.CallDeferred( "ApplyShadowQuality" ); } ) ),
			WorkerThreadPool.AddTask( Callable.From( () => {
				ItemDatabase = ResourceLoader.Load( "res://resources/ItemDatabase.tres" );
			} ) ),
			WorkerThreadPool.AddTask( Callable.From( () => {
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
			} ) ),
			WorkerThreadPool.AddTask( Callable.From( () => {
				Quiet = [
					ResourceLoader.Load<AudioStream>( "res://sounds/barks/quiet_cmd_0.mp3" ),
					ResourceLoader.Load<AudioStream>( "res://sounds/barks/quiet_cmd_1.mp3" ),
					ResourceLoader.Load<AudioStream>( "res://sounds/barks/quiet_cmd_2.mp3" ),
					ResourceLoader.Load<AudioStream>( "res://sounds/barks/quiet_cmd_3.mp3" ),
					ResourceLoader.Load<AudioStream>( "res://sounds/barks/quiet_cmd_4.mp3" ),
				];
			} ) ),
			WorkerThreadPool.AddTask( Callable.From( () => {
				TargetPinned = [
					ResourceLoader.Load<AudioStream>( "res://sounds/barks/21161.mp3" ),
					ResourceLoader.Load<AudioStream>( "res://sounds/barks/21162.mp3" ),
					ResourceLoader.Load<AudioStream>( "res://sounds/barks/21163.mp3" ),
					ResourceLoader.Load<AudioStream>( "res://sounds/barks/21186.mp3" )
				];
			} ) ),
			WorkerThreadPool.AddTask( Callable.From( () => {
				TargetRunning = [
					ResourceLoader.Load<AudioStream>( "res://sounds/barks/21156.mp3" ),
					ResourceLoader.Load<AudioStream>( "res://sounds/barks/21157.mp3" ),
					ResourceLoader.Load<AudioStream>( "res://sounds/barks/21159.mp3" ),
					ResourceLoader.Load<AudioStream>( "res://sounds/barks/21160.mp3" ),
				];
			} ) ),
			WorkerThreadPool.AddTask( Callable.From( () => {
				Ceasefire = [
					ResourceLoader.Load<AudioStream>( "res://sounds/barks/ceasefire_cmd_0.mp3" ),
					ResourceLoader.Load<AudioStream>( "res://sounds/barks/ceasefire_cmd_2.mp3" ),
					ResourceLoader.Load<AudioStream>( "res://sounds/barks/ceasefire_cmd_3.mp3" ),
					ResourceLoader.Load<AudioStream>( "res://sounds/barks/ceasefire_cmd_4.mp3" ),
				];
			} ) ),
			WorkerThreadPool.AddTask( Callable.From( () => {
				OutOfTheWay = [
					ResourceLoader.Load<AudioStream>( "res://sounds/barks/21376.mp3" ),
					ResourceLoader.Load<AudioStream>( "res://sounds/barks/21377.mp3" ),
					ResourceLoader.Load<AudioStream>( "res://sounds/barks/21381.mp3" ),
					ResourceLoader.Load<AudioStream>( "res://sounds/barks/get_down_cmd_0.mp3" ),
					ResourceLoader.Load<AudioStream>( "res://sounds/barks/get_down_cmd_1.mp3" ),
				];
			} ) ),
			WorkerThreadPool.AddTask( Callable.From( () => {
				Curse = [
					ResourceLoader.Load<AudioStream>( "res://sounds/barks/21009.mp3" ),
					ResourceLoader.Load<AudioStream>( "res://sounds/barks/21010.mp3" ),
					ResourceLoader.Load<AudioStream>( "res://sounds/barks/21011.mp3" ),
				];
			} ) ),
			WorkerThreadPool.AddTask( Callable.From( () => {
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
			} ) ),
			WorkerThreadPool.AddTask( Callable.From( () => {
				CheckItOut = [
					ResourceLoader.Load<AudioStream>( "res://sounds/barks/21100.mp3" ),
					ResourceLoader.Load<AudioStream>( "res://sounds/barks/21172.mp3" ),
					ResourceLoader.Load<AudioStream>( "res://sounds/barks/21175.mp3" ),
					ResourceLoader.Load<AudioStream>( "res://sounds/barks/search_area_cmd_0.mp3" ),
				];
			} ) ),
			WorkerThreadPool.AddTask( Callable.From( () => {
				Confusion = [
					ResourceLoader.Load<AudioStream>( "res://sounds/barks/21164.mp3" ),
					ResourceLoader.Load<AudioStream>( "res://sounds/barks/21165.mp3" ),
					ResourceLoader.Load<AudioStream>( "res://sounds/barks/21168.mp3" ),
					ResourceLoader.Load<AudioStream>( "res://sounds/barks/21169.mp3" ),
					ResourceLoader.Load<AudioStream>( "res://sounds/barks/21170.mp3" ),
					ResourceLoader.Load<AudioStream>( "res://sounds/barks/21171.mp3" ),
				];
			} ) ),
			WorkerThreadPool.AddTask( Callable.From( () => {
				HeavyDead = [
					ResourceLoader.Load<AudioStream>( "res://sounds/barks/14859.mp3" ),
					ResourceLoader.Load<AudioStream>( "res://sounds/barks/14860.mp3" ),
					ResourceLoader.Load<AudioStream>( "res://sounds/barks/14861.mp3" ),
				];
			} ) ),
			WorkerThreadPool.AddTask( Callable.From( () => {
				ManDown = [
					ResourceLoader.Load<AudioStream>( "res://sounds/barks/21348.mp3" ),
					ResourceLoader.Load<AudioStream>( "res://sounds/barks/21359.mp3" ),
				];
			} ) ),
			WorkerThreadPool.AddTask( Callable.From( () => {
				ManDown2 = ResourceLoader.Load<AudioStream>( "res://sounds/barks/man_down_2_callout_0.mp3" );
				ManDown2 = ResourceLoader.Load<AudioStream>( "res://sounds/barks/men_down_3_callout_0.mp3" );
				Deaf = ResourceLoader.Load<AudioStream>( "res://sounds/barks/deaf_callout.mp3" );
				SquadWiped = ResourceLoader.Load<AudioStream>( "res://sounds/barks/squad_wiped_callout_0.mp3" );
			} ) ),
			WorkerThreadPool.AddTask( Callable.From( () => {
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
			} ) ),
			WorkerThreadPool.AddTask( Callable.From( () => {
				NeedBackup = [
					ResourceLoader.Load<AudioStream>( "res://sounds/barks/21193.mp3" ),
					ResourceLoader.Load<AudioStream>( "res://sounds/barks/21194.mp3" ),
					ResourceLoader.Load<AudioStream>( "res://sounds/barks/21195.mp3" ),
					ResourceLoader.Load<AudioStream>( "res://sounds/barks/21196.mp3" ),
					ResourceLoader.Load<AudioStream>( "res://sounds/barks/21197.mp3" ),
				];
			} ) ),
			WorkerThreadPool.AddTask( Callable.From( () => {
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
			} ) ),
			WorkerThreadPool.AddTask( Callable.From( () => {
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
				MoveSandSfx = [
					ResourceLoader.Load<AudioStream>( "res://sounds/env/move_sand_0.ogg" ),
					ResourceLoader.Load<AudioStream>( "res://sounds/env/move_sand_1.ogg" ),
					ResourceLoader.Load<AudioStream>( "res://sounds/env/move_sand_2.ogg" ),
					ResourceLoader.Load<AudioStream>( "res://sounds/env/move_sand_3.ogg" ),
				];
				MoveStoneSfx = [
					ResourceLoader.Load<AudioStream>( "res://sounds/env/move_stone_0.ogg" ),
					ResourceLoader.Load<AudioStream>( "res://sounds/env/move_stone_1.ogg" ),
					ResourceLoader.Load<AudioStream>( "res://sounds/env/move_stone_2.ogg" ),
					ResourceLoader.Load<AudioStream>( "res://sounds/env/move_stone_3.ogg" ),
				];
				MoveWoodSfx = [
					ResourceLoader.Load<AudioStream>( "res://sounds/env/move_wood_0.ogg" ),
					ResourceLoader.Load<AudioStream>( "res://sounds/env/move_wood_1.ogg" ),
					ResourceLoader.Load<AudioStream>( "res://sounds/env/move_wood_2.ogg" ),
					ResourceLoader.Load<AudioStream>( "res://sounds/env/move_wood_3.ogg" ),
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
			} ) )
		];

		Light = ResourceLoader.Load<Texture2D>( "res://textures/point_light.dds" );

		SceneLoadThread?.Join();

		for ( int i = 0; i < WorkerThreads.Length; i++ ) {
			WorkerThreadPool.WaitForTaskCompletion( WorkerThreads[ i ] );
		}

		if ( GameConfiguration.GameMode != GameMode.Multiplayer && GameConfiguration.GameMode != GameMode.ChallengeMode && GameConfiguration.GameMode != GameMode.JohnWick ) {
			GlobalEconomy.Init();
		}

		world.CallDeferred( "emit_signal", "ResourcesLoadingFinished" );
	}
};