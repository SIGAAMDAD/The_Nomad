using Godot;
using System;
using PlayerSystem;
using System.Collections.Generic;
using Steamworks;
using System.Runtime.CompilerServices;
using Renown;
using GDExtension.Wrappers;
using System.Xml;

public partial class Player : Entity {
	public enum Hands : byte {
		Left,
		Right,
		Both
	};

	public enum PlayerFlags : uint {
		Sliding			= 0x00000001,
		Crouching		= 0x00000002,
		BulletTime		= 0x00000004,
		Dashing			= 0x00000008,
		DemonRage		= 0x00000010,
		UsedMana		= 0x00000020,
		DemonSight		= 0x00000040,
		OnHorse			= 0x00000080,
		IdleAnimation	= 0x00001000,
		Checkpoint		= 0x00002000,
		BlockedInput	= 0x00004000,
		UsingWeapon		= 0x00008000,
		Inventory		= 0x00010000,
	};

	public enum AnimationState : byte {
		Idle,
		Move,
		Melee
	};

	public class WarpPoint {
		private Checkpoint Location;
		private Renown.World.Biome Biome;
		private Texture2D Icon;

		public WarpPoint( Checkpoint checkpoint ) {
			Location = checkpoint;
		}

		public Texture2D GetIcon() {
			return Icon;
		}
		public Checkpoint GetLocation() {
			return Location;
		}
		public Renown.World.Biome GetBiome() {
			return Biome;
		}
	};

	private readonly WeaponEntity.Properties[] WeaponModeList = [
		WeaponEntity.Properties.IsOneHanded | WeaponEntity.Properties.IsBladed,
		WeaponEntity.Properties.IsOneHanded | WeaponEntity.Properties.IsBlunt,
		WeaponEntity.Properties.IsOneHanded | WeaponEntity.Properties.IsFirearm,

		WeaponEntity.Properties.IsTwoHanded | WeaponEntity.Properties.IsBladed,
		WeaponEntity.Properties.IsTwoHanded | WeaponEntity.Properties.IsBlunt,
		WeaponEntity.Properties.IsTwoHanded | WeaponEntity.Properties.IsFirearm
	];

	private static readonly float PunchRange = 40.0f;
	private static readonly int MAX_WEAPON_SLOTS = 4;
	private static readonly Color AimingAtTarget = new Color( 1.0f, 0.0f, 0.0f, 1.0f );
	private static readonly Color AimingAtNull = new Color( 0.5f, 0.5f, 0.0f, 1.0f );

	public static bool InCombat = false;
	public static int NumTargets = 0;

	private const float ACCEL = 900.0f;
	private const float FRICTION = 1400.0f;
	private const float MAX_SPEED = 240.0f;
	private const float JUMP_VELOCITY = -400.0f;

	private Random RandomFactory = new Random( System.DateTime.Now.Year + System.DateTime.Now.Month + System.DateTime.Now.Day );
	private readonly Godot.Vector2 JumpkitSparksOffset = new Godot.Vector2( 255.0f, 0.0f );

	private static Godot.Vector2I ScreenSize = Godot.Vector2I.Zero;

	private Resource CurrentMappingContext;

	private Resource SwitchToKeyboard;
	private Resource SwitchToGamepad;

	private Resource MoveAction;
	private Resource DashAction;
	private Resource SlideAction;
	private Resource MeleeAction;
	private Resource UseWeaponAction;
	private Resource SwitchWeaponModeAction;
	private Resource NextWeaponAction;
	private Resource PrevWeaponAction;
	private Resource OpenInventoryAction;
	private Resource BulletTimeAction;
	public readonly Resource InteractionAction;

	private Resource MoveActionGamepad;
	private Resource DashActionGamepad;
	private Resource SlideActionGamepad;
	private Resource MeleeActionGamepad;
	private Resource UseWeaponActionGamepad;
	private Resource SwitchWeaponModeActionGamepad;
	private Resource NextWeaponActionGamepad;
	private Resource PrevWeaponActionGamepad;
	private Resource OpenInventoryActionGamepad;
	private Resource BulletTimeActionGamepad;
	private Resource ArmAngleAction;
	public readonly Resource InteractionActionGamepad;

	private Resource MoveActionKeyboard;
	private Resource DashActionKeyboard;
	private Resource SlideActionKeyboard;
	private Resource MeleeActionKeyboard;
	private Resource UseWeaponActionKeyboard;
	private Resource SwitchWeaponModeActionKeyboard;
	private Resource NextWeaponActionKeyboard;
	private Resource PrevWeaponActionKeyboard;
	private Resource OpenInventoryActionKeyboard;
	private Resource BulletTimeActionKeyboard;
	public readonly Resource InteractionActionKeyboard;

	private GpuParticles2D WalkEffect;
	private GpuParticles2D SlideEffect;
	private GpuParticles2D DashEffect;
	private PointLight2D DashLight;
	private AnimatedSprite2D JumpkitSparks;

	private PackedScene BloodSplatter;

	private Node Animations;
	private AnimatedSprite2D TorsoAnimation;
	private AnimatedSprite2D LegAnimation;
	private AnimatedSprite2D IdleAnimation;

	private Timer IdleTimer;

	private Timer DashTime;
	private Timer SlideTime;
	private Timer DashCooldownTime;

	[Export]
	private Node Inventory;
	[Export]
	private Arm ArmLeft;
	[Export]
	private Arm ArmRight;
	[Export]
	private HeadsUpDisplay HUD;

	private static bool TutorialCompleted = false;

	private WeaponSlot[] WeaponSlots = new WeaponSlot[ MAX_WEAPON_SLOTS ];

	private float Rage = 60.0f;
	private PlayerFlags Flags = 0;
	private int CurrentWeapon = 0;

	// multiplayer data
	public CSteamID MultiplayerId = CSteamID.Nil;
	public string MultiplayerUsername;
	public uint MultiplayerKills = 0;
	public uint MultiplayerDeaths = 0;
	public uint MultiplayerFlagCaptures = 0;
	public uint MultiplayerFlagReturns = 0;
	public float MultiplayerHilltime = 0.0f;

	// aim reticle
	private Line2D AimLine = null;
	private RayCast2D AimRayCast = null;

	private bool NetworkNeedsSync = false;
	private PlayerAnimationState LeftArmAnimationState;
	private PlayerAnimationState RightArmAnimationState;
	private PlayerAnimationState TorsoAnimationState;
    private PlayerAnimationState LegAnimationState;

	private AudioStreamPlayer2D AudioChannel;
	private AudioStreamPlayer2D DashChannel;

	private FootSteps FootSteps;

	private NetworkWriter SyncObject = new NetworkWriter( 1024 );

	private Dictionary<int, WeaponEntity> WeaponsStack = new Dictionary<int, WeaponEntity>();
	private Dictionary<int, ConsumableStack> ConsumableStacks = new Dictionary<int, ConsumableStack>();
	private Dictionary<int, AmmoStack> AmmoStacks = new Dictionary<int, AmmoStack>();

	private Hands HandsUsed = Hands.Right;
	private Arm LastUsedArm;
	private float ArmAngle = 0.0f;
	private float DashBurnout = 0.0f;
	private int InputDevice = 0;
	private float FrameDamage = 0.0f;
	private int Hellbreaks = 0;
	private bool SplitScreen = false;
	private float SoundLevel = 0.0f;
	private Godot.Vector2 DashDirection = Godot.Vector2.Zero;
	private Godot.Vector2 InputVelocity = Godot.Vector2.Zero;
	private Godot.Vector2 LastMousePosition = Godot.Vector2.Zero;

	private CircleShape2D SoundArea;

	// used when LastCheckpoint is null
	private Godot.Vector2 StartingPosition = Godot.Vector2.Zero;
	private Checkpoint LastCheckpoint;

	private int TileMapLevel = 0;

	[Signal]
	public delegate void SwitchedWeaponEventHandler( WeaponEntity weapon );
	[Signal]
	public delegate void SwitchedWeaponModeEventHandler( WeaponEntity weapon, WeaponEntity.Properties useMode );

	public static bool IsTutorialActive() {
		return !TutorialCompleted;
	}

	public void SetTileMapFloorLevel( int nLevel ) => TileMapLevel = nLevel;
	public void SetSoundLevel( float nSoundLevel ) {
		if ( nSoundLevel > SoundLevel ) {
			SoundLevel = nSoundLevel;
		}
	}
	
	/*
	public override void PlaySound( AudioStreamPlayer2D channel, AudioStream stream ) {
		channel ??= MiscChannel;
		channel.Stream = stream;
		channel.VolumeDb = Mathf.LinearToDb( 100.0f / SettingsData.GetEffectsVolume() );
		channel.Play();
	}
	*/

	public override void Save() {
		using ( var writer = new SaveSystem.SaveSectionWriter( "Player" ) ) {
			int stackIndex;

			writer.SaveFloat( "health", Health );
			writer.SaveFloat( "rage", Rage );
			writer.SaveInt( "hellbreaks", Hellbreaks );
			writer.SaveInt( "current_weapon", CurrentWeapon );
			writer.SaveUInt( "hands_used", (uint)HandsUsed );

			writer.SaveVector2( "position", GlobalPosition );

			writer.SaveInt( "arm_left_slot", ArmLeft.GetSlot() );
			writer.SaveInt( "arm_right_slot", ArmRight.GetSlot() );

			writer.SaveInt( "ammo_stacks_count", AmmoStacks.Count );
			stackIndex = 0;
			foreach ( var stack in AmmoStacks ) {
				writer.SaveInt( "ammo_stacks_amount_" + stackIndex.ToString(), stack.Value.Amount );
				writer.SaveString( "ammo_stacks_type_" + stackIndex.ToString(), (string)stack.Value.AmmoType.Get( "id" ) );
				stackIndex++;
			}

			writer.SaveInt( "weapon_stacks_count", WeaponsStack.Count );
			stackIndex = 0;
			foreach ( var stack in WeaponsStack ) {
				writer.SaveString( "weapon_stacks_type_" + stackIndex.ToString(), (string)stack.Value.Data.Get( "id" ) );
				if ( ( stack.Value.GetProperties() & WeaponEntity.Properties.IsFirearm ) != 0 ) {
					writer.SaveInt( "weapon_stacks_bullet_count_" + stackIndex.ToString(), stack.Value.GetBulletCount() );
				}
				stackIndex++;
			}

			writer.SaveInt( "max_weapon_slots", MAX_WEAPON_SLOTS );
			for ( int i = 0; i < WeaponSlots.Length; i++ ) {
				writer.SaveBool( "weapon_slot_used_" + i.ToString(), WeaponSlots[i].IsUsed() );
				if ( WeaponSlots[i].IsUsed() ) {
					string weaponId = "";
					foreach ( var stack in WeaponsStack ) {
						if ( stack.Value == WeaponSlots[ i ].GetWeapon() ) {
							weaponId = (string)stack.Value.Data.Get( "id" );
							break;
						}
					}
					writer.SaveString( "weapon_slot_id_" + i.ToString(), weaponId );
					writer.SaveUInt( "weapon_slot_mode_" + i.ToString(), (uint)WeaponSlots[i].GetMode() );
				}
			}

			writer.SaveInt( "consumable_stacks_count", ConsumableStacks.Count );
			stackIndex = 0;
			foreach ( var stack in ConsumableStacks ) {
				writer.SaveInt( "consumable_stacks_amount_" + stackIndex.ToString(), stack.Value.Amount );
				writer.SaveString( "consumable_stacks_type_" + stackIndex.ToString(), (string)stack.Value.ItemType.Get( "id" ) );
			}
		}
	}
	public override void Load() {
		SaveSystem.SaveSectionReader reader = ArchiveSystem.GetSection( "Player" );

		Health = reader.LoadFloat( "health" );
		Rage = reader.LoadFloat( "rage" );
		Hellbreaks = reader.LoadInt( "hellbreaks" );
		CurrentWeapon = reader.LoadInt( "current_weapon" );
		HandsUsed = (Hands)reader.LoadUInt( "hands_used" );

		GlobalPosition = reader.LoadVector2( "position" );

		ArmLeft.SetWeapon( reader.LoadInt( "arm_left_slot" ) );
		ArmRight.SetWeapon( reader.LoadInt( "arm_right_slot" ) );
		switch ( HandsUsed ) {
		case Hands.Left:
			LastUsedArm = ArmLeft;
			break;
		case Hands.Right:
		case Hands.Both:
			LastUsedArm = ArmRight;
			break;
		};

		AmmoStacks.Clear();
		int numAmmoStacks = reader.LoadInt( "ammo_stacks_count" );
		for ( int i = 0; i < numAmmoStacks; i++ ) {
			AmmoStack stack = new AmmoStack();
			stack.Amount = reader.LoadInt( "ammo_stacks_amount_" + i.ToString() );
			string id = reader.LoadString( "ammo_stacks_type_" + i.ToString() );
			stack.AmmoType = (Resource)( (Resource)Inventory.Get( "database" ) ).Call( "get_item", id );
			AmmoStacks.Add( id.GetHashCode(), stack );
		}

		WeaponsStack.Clear();
		int numWeapons = reader.LoadInt( "weapon_stacks_count" );
		for ( int i = 0; i < numWeapons; i++ ) {
			WeaponEntity weapon = new WeaponEntity();
			string id = reader.LoadString( "weapon_stacks_type_" + i.ToString() );
			weapon.Data = (Resource)( (Resource)Inventory.Get( "database" ) ).Call( "get_item", id );
			weapon.SetOwner( this );
			WeaponsStack.Add( id.GetHashCode(), weapon );
			weapon.OverrideRayCast( AimRayCast );
			AddChild( weapon );

			if ( ( weapon.GetProperties() & WeaponEntity.Properties.IsFirearm ) != 0 ) {
				int nBulletCount = reader.LoadInt( "weapon_stacks_bullet_count_" + i.ToString() );
				if ( nBulletCount == 0 ) {
					weapon.SetWeaponState( WeaponEntity.WeaponState.Reload );
				} else {
					weapon.SetWeaponState( WeaponEntity.WeaponState.Idle );
				}
				weapon.SetBulletCount( nBulletCount );
			} else {
				weapon.SetWeaponState( WeaponEntity.WeaponState.Idle );
			}

			AmmoStack stack = null;
			foreach ( var it in AmmoStacks ) {
				if ( (int)( (Godot.Collections.Dictionary)it.Value.AmmoType.Get( "properties" ) )[ "type" ] ==
					(int)weapon.GetAmmoType() )
				{
					stack = it.Value;
					break;
				}
			}
			if ( stack != null ) {
				weapon.SetReserve( stack );
				weapon.SetAmmo( stack.AmmoType );
			}
		}

		int maxSlots = reader.LoadInt( "max_weapon_slots" );
		for ( int i = 0; i < maxSlots; i++ ) {
			if ( reader.LoadBoolean( "weapon_slot_used_" + i.ToString() ) ) {
				WeaponEntity weapon = WeaponsStack[ reader.LoadString( "weapon_slot_id_" + i.ToString() ).GetHashCode() ];
				weapon.SetEquippedState( true );
				WeaponSlots[i].SetWeapon( weapon );
				WeaponSlots[i].SetMode( (WeaponEntity.Properties)reader.LoadUInt( "weapon_slot_mode_" + i.ToString() ) );
			}
		}

		ConsumableStacks.Clear();
		int numConsumableStacks = reader.LoadInt( "consumable_stacks_count" );
		for ( int i = 0; i < numConsumableStacks; i++ ) {
			ConsumableStack stack = new ConsumableStack();
			stack.Amount = reader.LoadInt( "consumable_stacks_amount_" + i.ToString() );
			string id = reader.LoadString( "consumable_stacks_type_" + i.ToString() );
			stack.ItemType = (Resource)( (Resource)Inventory.Get( "database" ) ).Call( "get_item", id );
			ConsumableStacks.Add( id.GetHashCode(), stack );
		}

		EmitSignalSwitchedWeapon( WeaponSlots[ CurrentWeapon ].GetWeapon() );
	}

	public void SendPacket() {
		if ( !SettingsData.GetNetworkingEnabled() ) {
			return;
		}

		SyncObject.Write( (byte)SteamLobby.MessageType.ClientData );
		SyncObject.Write( (sbyte)CurrentWeapon );
		if ( CurrentWeapon != -1 ) {
			SyncObject.Write( (uint)WeaponSlots[ CurrentWeapon ].GetMode() );
			SyncObject.Write( WeaponSlots[ CurrentWeapon ].IsUsed() );
			if ( WeaponSlots[ CurrentWeapon ].IsUsed() ) {
				SyncObject.Write( (string)WeaponSlots[ CurrentWeapon ].GetWeapon().Data.Get( "id" ) );
			}
		}
		SyncObject.Write( GlobalPosition.X );
		SyncObject.Write( GlobalPosition.Y );
		SyncObject.Write( (byte)TorsoAnimationState );
		SyncObject.Write( ArmLeft.Rotation );
		SyncObject.Write( (byte)LeftArmAnimationState );
		SyncObject.Write( ArmRight.Rotation );
		SyncObject.Write( (byte)RightArmAnimationState );
		SyncObject.Write( (byte)LegAnimationState );
		SyncObject.Write( (byte)TorsoAnimationState );
		SyncObject.Write( (byte)HandsUsed );
		SyncObject.Write( (uint)Flags );
		SyncObject.Sync();
	}
	
	private void OnSoundAreaShape2DEntered( Rid bodyRid, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		if ( body is Renown.Thinkers.MobBase mob && mob != null ) {
			mob.Alert( this );
		}
	}
	private void OnSoundAreaShape2DExited( Rid bodyRid, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public AnimatedSprite2D GetTorsoAnimation() => TorsoAnimation;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public AnimatedSprite2D GetLegsAnimation() => LegAnimation;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public AnimatedSprite2D GetLeftArmAnimation() => ArmLeft.Animations;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public AnimatedSprite2D GetRightArmAnimation() => ArmRight.Animations;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Resource GetCurrentMappingContext() => CurrentMappingContext;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public WeaponSlot[] GetWeaponSlots() => WeaponSlots;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Dictionary<int, WeaponEntity> GetWeaponStack() => WeaponsStack;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Dictionary<int, AmmoStack> GetAmmoStacks() => AmmoStacks;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Node GetInventory() => Inventory;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Arm GetWeaponHand( WeaponEntity weapon ) {
		if ( ArmLeft.GetSlot() != WeaponSlot.INVALID && WeaponSlots[ ArmLeft.GetSlot() ].GetWeapon() == weapon ) {
			return ArmLeft;
		} else if ( ArmRight.GetSlot() != WeaponSlot.INVALID && WeaponSlots[ ArmRight.GetSlot() ].GetWeapon() == weapon ) {
			return ArmRight;
		}
		return null;
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Godot.Vector2 GetInputVelocity() => InputVelocity;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public WeaponSlot GetSlot( int nSlot ) => WeaponSlots[ nSlot ];
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float GetArmAngle() {
		if ( ( Flags & PlayerFlags.BlockedInput ) != 0 ) {
			return 0.0f;
		}
		if ( CurrentMappingContext == ResourceCache.KeyboardInputMappings ) {
			Godot.Vector2 mousePosition;

			if ( (int)SettingsData.GetWindowMode() >= 2 ) {
				mousePosition = DisplayServer.MouseGetPosition();
			} else {
				mousePosition = GetViewport().GetMousePosition();
			}

			float y = mousePosition.Y;
			float x = mousePosition.X;
			int width = ScreenSize.X;
			int height = ScreenSize.Y;

//			if ( LastMousePosition != mousePosition ) {
//				LastMousePosition = mousePosition;
//				IdleReset();
//			}

			ArmAngle = GetGlobalMousePosition().Angle();
//			ArmAngle = (float)Math.Atan2( y - ( height / 2.0f ), x - ( width / 2.0f ) );
			AimLine.GlobalRotation = ArmAngle;
			AimRayCast.TargetPosition = Godot.Vector2.Right.Rotated( Mathf.DegToRad( ArmAngle ) ) * AimLine.Points[1].X;
			if ( x > width * 0.5f ) {
				FlipSpriteRight();
			} else if ( x < width * 0.5f ) {
				FlipSpriteLeft();
			}
		}
		return ArmAngle;
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetArmAngle( float nAngle ) => ArmAngle = nAngle;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Arm GetLastUsedArm() => LastUsedArm;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetLastUsedArm( Arm arm ) => LastUsedArm = arm;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float GetSoundLevel() => SoundLevel;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetHealth( float nHealth ) {
		Health = nHealth;
		HUD.GetHealthBar().SetHealth( Health );
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float GetRage() => Rage;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetRage( float nRage ) {
		Rage = nRage;
		HUD.GetRageBar().Value = Rage;
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public PlayerFlags GetFlags() => Flags;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetFlags( PlayerFlags flags ) => Flags = flags;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Hands GetHandsUsed() => HandsUsed;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetHandsUsed( Hands hands ) => HandsUsed = hands;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public WeaponSlot[] GetSlots() => WeaponSlots;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetSlots( WeaponSlot[] slots ) => WeaponSlots = slots;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Arm GetLeftArm() => ArmLeft;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Arm GetRightArm() => ArmRight;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetCurrentWeapon() => CurrentWeapon;

	private void IdleReset() {
		IdleTimer.Start();
		IdleAnimation.Hide();
		IdleAnimation.Stop();

		LegAnimation.Show();
		TorsoAnimation.Show();
		ArmRight.Animations.Show();
		ArmLeft.Animations.Show();
	}

	public override void PlaySound( AudioStreamPlayer2D channel, AudioStream stream ) {
		channel ??= AudioChannel;
		channel.Stream = stream;
		channel.Play();
	}

	public void SetupSplitScreen( int nInputIndex ) {
		if ( Input.GetConnectedJoypads().Count > 0 ) {
			SplitScreen = true;
		}
		InputDevice = nInputIndex;
	}

	public override void _ExitTree() {
		base._ExitTree();

		for ( int i = 0; i < AmmoStacks.Count; i++ ) {
			AmmoStacks[i].QueueFree();
		}
		AmmoStacks.Clear();

		for ( int i = 0; i < ConsumableStacks.Count; i++ ) {
			ConsumableStacks[i].QueueFree();
		}
		ConsumableStacks.Clear();

		for ( int i = 0; i < WeaponsStack.Count; i++ ) {
			RemoveChild( WeaponsStack[i] );
			WeaponsStack[i].QueueFree();
		}
		WeaponsStack.Clear();

		for ( int i = 0; i < WeaponSlots.Length; i++ ) {
			WeaponSlots[i] = null;
		}

		/*
		MoveChannel.QueueFree();
		DashChannel.QueueFree();
		SlideChannel.QueueFree();
		SlowMoChannel.QueueFree();
		ActionChannel.QueueFree();
		PainChannel.QueueFree();
		*/
//		DashChannel.QueueFree();
//		MoveChannel.QueueFree();
//		MiscChannel.QueueFree();

		DashTime.QueueFree();
		SlideTime.QueueFree();
		DashCooldownTime.QueueFree();

		WalkEffect.QueueFree();
		DashEffect.QueueFree();
		SlideEffect.QueueFree();
		JumpkitSparks.QueueFree();

		IdleTimer.QueueFree();
		IdleAnimation.QueueFree();

		TorsoAnimation.QueueFree();
		LegAnimation.QueueFree();
		ArmLeft.Animations.QueueFree();
		ArmRight.Animations.QueueFree();
		ArmLeft.QueueFree();
		ArmRight.QueueFree();

		IdleTimer.QueueFree();
		IdleAnimation.QueueFree();
		Inventory.QueueFree();
		HUD.QueueFree();

		QueueFree();
	}

	private void Respawn() {
		GetNode<CanvasLayer>( "/root/TransitionScreen" ).Connect( "transition_finished", Callable.From( OnRespawnFinished ) );
		GetNode<CanvasLayer>( "/root/TransitionScreen" ).Call( "transition" );

		ArchiveSystem.SaveGame( null, 0 );
	}

	private void OnRespawnFinished() {
		GetNode<CanvasLayer>( "/root/TransitionScreen" ).Disconnect( "transition_finished", Callable.From( OnRespawnFinished ) );

		if ( LastCheckpoint == null ) {
			GlobalPosition = StartingPosition;
		} else {
			GlobalPosition = LastCheckpoint.GlobalPosition;
			BeginInteraction( LastCheckpoint );
		}

		Health = 100.0f;
		Rage = 60.0f;

		HUD.GetHealthBar().SetHealth( Health );
		HUD.GetRageBar().Value = Rage;
	}
	private void OnDeath( Entity attacker ) {
		EmitSignalDie( attacker, this );
		LegAnimation.Hide();
		ArmLeft.Animations.Hide();
		ArmRight.Animations.Hide();

		TorsoAnimation.Play( "death" );

		PlaySound( AudioChannel, ResourceCache.PlayerDieSfx[ RandomFactory.Next( 0, ResourceCache.PlayerDieSfx.Length - 1 ) ] );

		SetProcessUnhandledInput( true );
		SetProcess( false );

		//
		// respawn
		//
		Respawn();
	}

	public override void Damage( Entity attacker, float nAmount ) {
		if ( ( Flags & PlayerFlags.Dashing ) != 0 ) {
			return; // iframes
		}

		Health -= nAmount;
		Rage += nAmount;

		BloodParticleFactory.Create( attacker.GlobalPosition, GlobalPosition );

		if ( Health <= 0.0f ) {
			OnDeath( attacker );
		} else {
			PlaySound( AudioChannel, ResourceCache.PlayerPainSfx[ RandomFactory.Next( 0, ResourceCache.PlayerPainSfx.Length - 1 ) ] );
		}

		EmitSignalDamaged( attacker, this, nAmount );
	}

	public void BlockInput( bool bBlocked ) {
		if ( bBlocked ) {
			Flags |= PlayerFlags.BlockedInput;
		} else {
			Flags &= ~PlayerFlags.BlockedInput;
		}
	}

	public void EndInteraction() {
		HUD.HideInteraction();
		Flags &= ~PlayerFlags.Checkpoint;
	}
	public void BeginInteraction( InteractionItem item ) {
		HUD.ShowInteraction( item );

		switch ( item.GetInteractionType() ) {
		case InteractionType.Checkpoint:
			Flags |= PlayerFlags.Checkpoint;
			LastCheckpoint = item as Checkpoint;
			break;
		};
	}

	private void OnIdleAnimationTimerTimeout() {
		if ( IdleAnimation.IsPlaying() ) {
			return;
		}

		TorsoAnimation.Hide();
		ArmLeft.Animations.Hide();
		ArmRight.Animations.Hide();
		LegAnimation.Hide();
		IdleAnimation.Show();
		IdleAnimation.Play( "start" );

		TorsoAnimationState = PlayerAnimationState.TrueIdleStart;
		LegAnimationState = PlayerAnimationState.TrueIdleStart;
		LeftArmAnimationState = PlayerAnimationState.TrueIdleStart;
		RightArmAnimationState = PlayerAnimationState.TrueIdleStart;

		SteamAchievements.ActivateAchievement( "ACH_SMOKE_BREAK" );
	}
	private void OnIdleAnimationAnimationFinished()	{
		TorsoAnimationState = PlayerAnimationState.TrueIdleLoop;
		LegAnimationState = PlayerAnimationState.TrueIdleLoop;
		LeftArmAnimationState = PlayerAnimationState.TrueIdleLoop;
		RightArmAnimationState = PlayerAnimationState.TrueIdleLoop;

		IdleAnimation.Play( "loop" );
	}
	private void OnLegsAnimationLooped() {
		if ( LinearVelocity != Godot.Vector2.Zero ) {
			PlaySound( AudioChannel, ResourceCache.MoveGravelSfx[ RandomFactory.Next( 0, ResourceCache.MoveGravelSfx.Length - 1 ) ] );
			FootSteps.AddStep( GlobalPosition );
			SetSoundLevel( 14.0f );
		}
	}
	private void OnDashTimeTimeout() {
		HUD.GetDashOverlay().Hide();
		DashLight.Hide();
		DashEffect.Emitting = false;
		Flags &= ~PlayerFlags.Dashing;
		if ( LegAnimation.FlipH ) {
			JumpkitSparks.FlipH = false;
			JumpkitSparks.Offset = JumpkitSparksOffset;
		} else {
			JumpkitSparks.FlipH = false;
			JumpkitSparks.Offset = Godot.Vector2.Zero;
		}
		DashCooldownTime.Start();
	}
	private void OnSlideTimeout() {
		SlideEffect.Emitting = false;
		Flags &= ~PlayerFlags.Sliding;
	}

	private bool IsInputBlocked( bool bIsInventory = false ) => ( Flags & PlayerFlags.BlockedInput ) != 0 || ( !bIsInventory && ( Flags & PlayerFlags.Inventory ) != 0 );
	private void OnDash() {
		if ( IsInputBlocked() ) {
			return;
		}

		// TODO: upgradable dash burnout?
		if ( DashBurnout >= 1.0f ) {
			PlaySound( AudioChannel, ResourceCache.DashExplosion );

			DashBurnout = 0.0f;

			// extension of recharge time
			DashCooldownTime.WaitTime = 2.50f;
			return;
		}

		IdleReset();
		Flags |= PlayerFlags.Dashing;
		DashTime.Start();
		DashChannel.PitchScale = 1.0f + DashBurnout;
		PlaySound( DashChannel, ResourceCache.DashSfx[ RandomFactory.Next( 0, ResourceCache.DashSfx.Length - 1 ) ] );
		DashLight.Show();
		DashEffect.Emitting = true;
		DashDirection = LinearVelocity;
		HUD.GetDashOverlay().Show();

		DashBurnout += 0.25f;
		DashCooldownTime.WaitTime = 0.80f;
		DashCooldownTime.Start();
	}
	private void OnSlide() {
		if ( IsInputBlocked() ) {
			return;
		} else if ( ( Flags & PlayerFlags.Sliding ) != 0 ) {
			return;
		}
		IdleReset();
		Flags |= PlayerFlags.Sliding;
		SlideTime.Start();
		SlideEffect.Emitting = true;

		PlaySound( AudioChannel, ResourceCache.SlideSfx[ RandomFactory.Next( 0, ResourceCache.SlideSfx.Length - 1 ) ] );

		LegAnimationState = PlayerAnimationState.Sliding;
		TorsoAnimationState = PlayerAnimationState.Sliding;

		LegAnimation.Play( "slide" );
	}
	private void OnUseWeapon() {
		if ( IsInputBlocked() ) {
			return;
		}

		IdleReset();

		int slot = LastUsedArm.GetSlot();
		if ( slot == WeaponSlot.INVALID ) {
			return; // nothing equipped
		}
		if ( WeaponSlots[ slot ].IsUsed() ) {
			float soundLevel;
			FrameDamage += WeaponSlots[ slot ].GetWeapon().Use( WeaponSlots[ slot ].GetWeapon().GetLastUsedMode(), out soundLevel, ( Flags & PlayerFlags.UsingWeapon ) != 0 );
			Flags |= PlayerFlags.UsingWeapon;
			SetSoundLevel( soundLevel );
		}
	}
	private void OnArmAngleChanged() {
		if ( IsInputBlocked() ) {
			return;
		}
		
		ArmAngle = (float)ArmAngleAction.Get( "value_axis_1d" );
		AimLine.GlobalRotation = ArmAngle;
		AimRayCast.GlobalRotation = ArmAngle;
	}
	private void OnPrevWeapon() {
		if ( IsInputBlocked() ) {
			return;
		}

		int index = CurrentWeapon <= 0 ? MAX_WEAPON_SLOTS - 1 : CurrentWeapon - 1;
		while ( index != -1 ) {
			if ( WeaponSlots[ index ].IsUsed() ) {
				break;
			}
			index--;
		}

		if ( index == MAX_WEAPON_SLOTS ) {
			index = -1;
		}

		// adjust arm state
		if ( index != WeaponSlot.INVALID ) {
			WeaponEntity weapon;
			Arm otherArm;

			if ( LastUsedArm == ArmLeft ) {
				otherArm = ArmRight;
				HandsUsed = Hands.Left;
			} else if ( LastUsedArm == ArmRight ) {
				otherArm = ArmLeft;
				HandsUsed = Hands.Right;
			} else {
				GD.PushError( "OnNextWeapon: invalid LastUsedArm" );
				LastUsedArm = ArmRight;
				return;
			}

			weapon = WeaponSlots[ index ].GetWeapon();
			if ( ( weapon.GetLastUsedMode() & WeaponEntity.Properties.IsTwoHanded ) != 0 ) {
				otherArm.SetWeapon( WeaponSlot.INVALID );
				HandsUsed = Hands.Both;
			}

			EmitSignalSwitchedWeapon( weapon );
		} else {
			EmitSignalSwitchedWeapon( null );
		}
		PlaySound( AudioChannel, ResourceCache.ChangeWeaponSfx );

		CurrentWeapon = index;
		LastUsedArm.SetWeapon( CurrentWeapon );
	}
	private void OnNextWeapon() {
		if ( IsInputBlocked() ) {
			return;
		}

		int index = CurrentWeapon == MAX_WEAPON_SLOTS - 1 ? 0 : CurrentWeapon + 1;
		while ( index < MAX_WEAPON_SLOTS ) {
			if ( WeaponSlots[ index ].IsUsed() ) {
				break;
			}
			index++;
		}

		if ( index == MAX_WEAPON_SLOTS ) {
			index = -1;
		}

		// adjust arm state
		if ( index != WeaponSlot.INVALID ) {
			WeaponEntity weapon;
			Arm otherArm;

			if ( LastUsedArm == ArmLeft ) {
				otherArm = ArmRight;
				HandsUsed = Hands.Left;
			} else if ( LastUsedArm == ArmRight ) {
				otherArm = ArmLeft;
				HandsUsed = Hands.Right;
			} else {
				GD.PushError( "OnNextWeapon: invalid LastUsedArm" );
				LastUsedArm = ArmRight;
				return;
			}

			weapon = WeaponSlots[ index ].GetWeapon();
			if ( ( weapon.GetLastUsedMode() & WeaponEntity.Properties.IsTwoHanded ) != 0 ) {
				otherArm.SetWeapon( WeaponSlot.INVALID );
				HandsUsed = Hands.Both;
			}

			EmitSignalSwitchedWeapon( weapon );
		} else {
			EmitSignalSwitchedWeapon( null );
		}
		PlaySound( AudioChannel, ResourceCache.ChangeWeaponSfx );

		CurrentWeapon = index;
		LastUsedArm.SetWeapon( CurrentWeapon );
	}
	private void OnBulletTime() {
		if ( IsInputBlocked() ) {
			return;
		}

		IdleReset();
		if ( ( Flags & PlayerFlags.BulletTime ) != 0 ) {
			ExitBulletTime();
		} else {
			PlaySound( AudioChannel, ResourceCache.SlowMoBeginSfx );

			Flags |= PlayerFlags.BulletTime;
			Engine.TimeScale = 0.40f;
		}
	}
	private void OnToggleInventory() {
		if ( IsInputBlocked( true ) ) {
			return;
		}

		if ( ( Flags & PlayerFlags.Inventory ) == 0 ) {
			Flags |= PlayerFlags.Inventory;
		} else {
			Flags &= ~PlayerFlags.Inventory;
		}
		HUD.OnShowInventory();
	}
	private void SwitchWeaponWielding() {
		if ( IsInputBlocked() ) {
			return;
		}

		Arm src;
		Arm dst;

		switch ( HandsUsed ) {
		case Hands.Left:
			src = ArmLeft;
			dst = ArmRight;
			break;
		case Hands.Right:
			src = ArmRight;
			dst = ArmLeft;
			break;
		case Hands.Both:
		default:
			src = LastUsedArm;
			dst = src;
			break;
		};

		if ( src.GetSlot() == WeaponSlot.INVALID ) {
			// nothing in the source hand, deny
			return;
		}

		LastUsedArm = dst;

		WeaponEntity srcWeapon = WeaponSlots[ src.GetSlot() ].GetWeapon();
		if ( ( srcWeapon.GetLastUsedMode() & WeaponEntity.Properties.IsTwoHanded ) != 0 && ( srcWeapon.GetLastUsedMode() & WeaponEntity.Properties.IsOneHanded ) == 0 ) {
			// cannot change hands, no one-handing allowed
			return;
		}
		
		// check if the destination hand has something in it, if true, then swap
		if ( dst.GetSlot() != WeaponSlot.INVALID ) {
			int tmp = dst.GetSlot();

			dst.SetWeapon( src.GetSlot() );
			src.SetWeapon( tmp );
		} else {
			// if we have nothing in the destination hand, then just clear the source hand
			int tmp = src.GetSlot();

			src.SetWeapon( WeaponSlot.INVALID );
			dst.SetWeapon( tmp );
		}
		EmitSignalSwitchedWeaponMode( srcWeapon, srcWeapon.GetLastUsedMode() );
	}
	private void SwitchWeaponHand() {
		if ( IsInputBlocked() ) {
			return;
		}
		// TODO: implement use both hands
		
		switch ( HandsUsed ) {
		case Hands.Left:
			HandsUsed = Hands.Right; // set to right
			LastUsedArm = ArmRight;
			break;
		case Hands.Right:
			HandsUsed = Hands.Left; // set to right
			LastUsedArm = ArmLeft;
			break;
		// can't switch if we're using both hands for one weapon
		case Hands.Both:
		default:
			Console.PrintError( "SwitchWeaponHand: invalid hand, setting to default of right" );
			HandsUsed = Hands.Right;
			break;
		};
		if ( LastUsedArm.GetSlot() != WeaponSlot.INVALID ) {
			EquipSlot( LastUsedArm.GetSlot() );
		}
	}
	private void SwitchWeaponMode() {
		if ( IsInputBlocked() ) {
			return;
		}
		// weapon mode order (default)
		// blade -> blunt -> firearm

		WeaponSlot slot;

		switch ( HandsUsed ) {
		case Hands.Left: {
			int index = ArmLeft.GetSlot();
			if ( index == WeaponSlot.INVALID ) {
				return;
			}
			slot = WeaponSlots[ index ];
			break; }
		case Hands.Right: {
			int index = ArmRight.GetSlot();
			if ( index == WeaponSlot.INVALID ) {
				return;
			}
			slot = WeaponSlots[ index ];
			break; }
		case Hands.Both:
		default:
			slot = WeaponSlots[ CurrentWeapon ];
			break;
		};

		WeaponEntity.Properties mode = slot.GetMode();
		WeaponEntity weapon = slot.GetWeapon();

		slot.SetMode( WeaponEntity.Properties.None ); // clear the modes

		// find the next most suitable mode
		WeaponEntity.Properties props = weapon.GetProperties();
		bool IsOneHanded = ( props & WeaponEntity.Properties.IsOneHanded ) != 0;
		bool IsTwoHanded = ( props & WeaponEntity.Properties.IsTwoHanded ) != 0;
		const WeaponEntity.Properties hands = ~(  WeaponEntity.Properties.IsOneHanded | WeaponEntity.Properties.IsTwoHanded );

		for ( int i = 0; i < WeaponModeList.Length; i++ ) {
			WeaponEntity.Properties current = WeaponModeList[i];
			if ( mode == current ) {
				// same mode, don't switch
				continue;
			}
			if ( ( props & ( current & hands ) ) != 0 ) {
				// apply some filters
				if ( ( current & WeaponEntity.Properties.IsOneHanded ) != 0 && !IsOneHanded ) {
					// one handing not possible, deny
					continue;
				}
				if ( ( current & WeaponEntity.Properties.IsTwoHanded ) != 0 && IsTwoHanded ) {
					// two handing not possible, deny
					continue;
				}
				
				// we've got a match!
				slot.SetMode( current );
				slot.GetWeapon().SetUseMode( current );
			}
		}
	}

	public void EquipSlot( int nSlot ) {
		CurrentWeapon = nSlot;

		WeaponEntity weapon = WeaponSlots[ nSlot ].GetWeapon();
		if ( weapon != null ) {
			// apply rules of various weapon properties
			if ( ( weapon.GetLastUsedMode() & WeaponEntity.Properties.IsTwoHanded ) != 0 )	{
				ArmLeft.SetWeapon( CurrentWeapon );
				ArmRight.SetWeapon( CurrentWeapon );

				// this will automatically override any other modes
				WeaponSlots[ ArmLeft.GetSlot() ].SetMode( weapon.GetDefaultMode() );
			}

			WeaponSlots[ LastUsedArm.GetSlot() ].SetMode( weapon.GetProperties() );
		}
		else {
			ArmLeft.SetWeapon( -1 );
			ArmRight.SetWeapon( -1 );
		}

		// update hand data
		LastUsedArm.SetWeapon( CurrentWeapon );

		EmitSignalSwitchedWeapon( weapon );
	}
	private void OnStoppedUsingWeapon() => Flags &= ~PlayerFlags.UsingWeapon;

	public void SwitchInputMode( Resource InputContext ) {
		GetNode( "/root/GUIDE" ).Call( "enable_mapping_context", InputContext );

		Console.PrintLine( "Remapping input..." );

		if ( GameConfiguration.GameMode == GameMode.LocalCoop2 ) {
			LoadGamepadBinds();
		}
		
		SwitchWeaponModeAction?.Disconnect( "triggered", Callable.From( SwitchWeaponMode ) );
		BulletTimeAction?.Disconnect( "triggered", Callable.From( OnBulletTime ) );
		NextWeaponAction?.Disconnect( "triggered", Callable.From( OnNextWeapon ) );
		PrevWeaponAction?.Disconnect( "triggered", Callable.From( OnPrevWeapon ) );
		DashAction?.Disconnect( "triggered", Callable.From( OnDash ) );
		SlideAction?.Disconnect( "triggered", Callable.From( OnSlide ) );
		UseWeaponAction?.Disconnect( "triggered", Callable.From( OnUseWeapon ) );
		UseWeaponAction?.Disconnect( "completed", Callable.From( OnStoppedUsingWeapon ) );
		OpenInventoryAction?.Disconnect( "triggered", Callable.From( OnToggleInventory ) );

		if ( InputContext == ResourceCache.KeyboardInputMappings ) {
			MoveAction = MoveActionKeyboard;
			DashAction = DashActionKeyboard;
			SlideAction = SlideActionKeyboard;
			BulletTimeAction = BulletTimeActionKeyboard;
			PrevWeaponAction = PrevWeaponActionKeyboard;
			NextWeaponAction = NextWeaponActionKeyboard;
			SwitchWeaponModeAction = SwitchWeaponModeActionKeyboard;
			OpenInventoryAction = OpenInventoryActionKeyboard;
			UseWeaponAction = UseWeaponActionKeyboard;

			CurrentMappingContext = ResourceCache.KeyboardInputMappings;
		} else {
			MoveAction = MoveActionGamepad;
			DashAction = DashActionGamepad;
			SlideAction = SlideActionGamepad;
			BulletTimeAction = BulletTimeActionGamepad;
			PrevWeaponAction = PrevWeaponActionGamepad;
			NextWeaponAction = NextWeaponActionGamepad;
			SwitchWeaponModeAction = SwitchWeaponModeActionGamepad;
			OpenInventoryAction = OpenInventoryActionGamepad;
			UseWeaponAction = UseWeaponActionGamepad;

			CurrentMappingContext = ResourceCache.GamepadInputMappings;
		}

		SwitchWeaponModeAction.Connect( "triggered", Callable.From( SwitchWeaponMode ) );
		BulletTimeAction.Connect( "triggered", Callable.From( OnBulletTime ) );
		NextWeaponAction.Connect( "triggered", Callable.From( OnNextWeapon ) );
		PrevWeaponAction.Connect( "triggered", Callable.From( OnPrevWeapon ) );
		DashAction.Connect( "triggered", Callable.From( OnDash ) );
		SlideAction.Connect( "triggered", Callable.From( OnSlide ) );
		UseWeaponAction.Connect( "triggered", Callable.From( OnUseWeapon ) );
		UseWeaponAction.Connect( "completed", Callable.From( OnStoppedUsingWeapon ) );
		OpenInventoryAction.Connect( "triggered", Callable.From( OnToggleInventory ) );
	}

	public override void _UnhandledInput( InputEvent @event ) {
		base._UnhandledInput( @event );
		
		if ( Health <= 0.0f ) {
			// dead
			SetHealth( 100.0f );
			SetRage( 0.0f );

			SetProcess( true );
			SetPhysicsProcess( true );
			SetProcessUnhandledInput( false );

			TorsoAnimation.Play( "default" );
			LegAnimation.Visible = true;
			ArmLeft.Animations.Visible = true;
			ArmRight.Animations.Visible = true;
		}
	}

	private void FlipSpriteLeft() {
		LegAnimation.FlipH = true;
		TorsoAnimation.FlipH = true;
		ArmLeft.Flip = true;
		ArmRight.Flip = true;
	}
	private void FlipSpriteRight() {
		LegAnimation.FlipH = false;
		TorsoAnimation.FlipH = false;
		ArmLeft.Flip = false;
		ArmRight.Flip = false;
	}

	private void OnSlowMoSfxFinished() {
//		if ( MiscChannel.Stream == ResourceCache.SlowMoBeginSfx ) {
//			// only start lagging audio playback after the slowmo begin finishes
//			AudioServer.PlaybackSpeedScale = 0.50f;
//		}
	}

	public void LoadGamepadBinds() {
		MoveActionGamepad = ResourceLoader.Load( "res://resources/binds/actions/gamepad/move_player" + InputDevice.ToString() + ".tres" );
		DashActionGamepad = ResourceLoader.Load( "res://resources/binds/actions/gamepad/dash_player" + InputDevice.ToString() + ".tres" );
		SlideActionGamepad = ResourceLoader.Load( "res://resources/binds/actions/gamepad/slide_player" + InputDevice.ToString() + ".tres" );
		UseWeaponActionGamepad = ResourceLoader.Load( "res://resources/binds/actions/gamepad/use_weapon_player" + InputDevice.ToString() + ".tres" );
		NextWeaponActionGamepad = ResourceLoader.Load( "res://resources/binds/actions/gamepad/next_weapon_player" + InputDevice.ToString() + ".tres" );
		PrevWeaponActionGamepad = ResourceLoader.Load( "res://resources/binds/actions/gamepad/prev_weapon_player" + InputDevice.ToString() + ".tres" );
		SwitchWeaponModeActionGamepad = ResourceLoader.Load( "res://resources/binds/actions/gamepad/switch_weapon_mode_player" + InputDevice.ToString() + ".tres" );
		BulletTimeActionGamepad = ResourceLoader.Load( "res://resources/binds/actions/gamepad/bullet_time_player" + InputDevice.ToString() + ".tres" );
		OpenInventoryActionGamepad = ResourceLoader.Load( "res://resources/binds/actions/gamepad/open_inventory_player" + InputDevice.ToString() + ".tres" );
		ArmAngleAction = ResourceLoader.Load( "res://resources/binds/actions/gamepad/arm_angle_player" + InputDevice.ToString() + ".tres" );
	}
	private void LoadKeyboardBinds() {
		MoveActionKeyboard = ResourceLoader.Load( "res://resources/binds/actions/keyboard/move.tres" );
		DashActionKeyboard = ResourceLoader.Load( "res://resources/binds/actions/keyboard/dash.tres" );
		SlideActionKeyboard = ResourceLoader.Load( "res://resources/binds/actions/keyboard/slide.tres" );
		UseWeaponActionKeyboard = ResourceLoader.Load( "res://resources/binds/actions/keyboard/use_weapon.tres" );
		NextWeaponActionKeyboard = ResourceLoader.Load( "res://resources/binds/actions/keyboard/next_weapon.tres" );
		PrevWeaponActionKeyboard = ResourceLoader.Load( "res://resources/binds/actions/keyboard/prev_weapon.tres" );
		SwitchWeaponModeActionKeyboard = ResourceLoader.Load( "res://resources/binds/actions/keyboard/switch_weapon_mode.tres" );
		BulletTimeActionKeyboard = ResourceLoader.Load( "res://resources/binds/actions/keyboard/bullet_time.tres" );
		OpenInventoryActionKeyboard = ResourceLoader.Load( "res://resources/binds/actions/keyboard/open_inventory.tres" );
	}
	private void LoadSfx() {
		/*
		ActionChannel = GetNode<AudioStreamPlayer2D>( "ActionChannel" );
		PainChannel = GetNode<AudioStreamPlayer2D>( "PainChannel" );
		SlowMoChannel = GetNode<AudioStreamPlayer2D>( "SlowMoChannel" );
		SlowMoChannel.Connect( "finished", Callable.From( OnSlowMoSfxFinished ) );

		DashChannel = GetNode<AudioStreamPlayer2D>( "DashChannel" );
		SlideChannel = GetNode<AudioStreamPlayer2D>( "SlideChannel" );
		MoveChannel = GetNode<AudioStreamPlayer2D>( "MoveChannel" );
		*/
		/*
		MoveChannel = GetNode<AudioStreamPlayer2D>( "MoveChannel" );
		MoveChannel.SetProcess( false );
		MoveChannel.SetProcessInternal( false );

		DashChannel = GetNode<AudioStreamPlayer2D>( "DashChannel" );
		DashChannel.SetProcess( false );
		DashChannel.SetProcessInternal( false );

		MiscChannel = GetNode<AudioStreamPlayer2D>( "MiscChannel" );
		MiscChannel.SetProcess( false );
		MiscChannel.SetProcessInternal( false );
		*/
	}

	private void ExitBulletTime() {
		Flags &= ~PlayerFlags.BulletTime;
		HUD.GetReflexOverlay().Hide();
		Engine.TimeScale = 1.0f;
		AudioServer.PlaybackSpeedScale = 1.0f;
		PlaySound( AudioChannel, ResourceCache.SlowMoEndSfx );
	}

	private void CmdSuicide() {
		Health = 0.0f;
		OnDeath( this );
	}

	private float LitValue = 0.0f;
	private Texture2D LastSpriteTexture = null;
	private Rect2I LastCropRect = new Rect2I();

	private void OnViewportFramePreDraw() {
		Texture2D spriteTexture = TorsoAnimation.SpriteFrames.GetFrameTexture( TorsoAnimation.Animation, TorsoAnimation.Frame );
		Image image = spriteTexture.GetImage();

		image.GetPixel( 0, 0 );

		LastSpriteTexture = TorsoAnimation.SpriteFrames.GetFrameTexture( TorsoAnimation.Animation, TorsoAnimation.Frame );
		Godot.Vector2 viewportScale = TorsoAnimation.GetViewportTransform().Scale;
		Godot.Vector2I screenPos = (Godot.Vector2I)( TorsoAnimation.GetScreenTransform().Origin * viewportScale );
		LastCropRect = new Godot.Rect2I( screenPos - ( ( (Godot.Vector2I)LastSpriteTexture.GetSize() / new Godot.Vector2I( 2, 2 ) ) * (Godot.Vector2I)viewportScale ), (Godot.Vector2I)LastSpriteTexture.GetSize() * (Godot.Vector2I)viewportScale );
	}
	private void OnViewportFramePostDraw() {
		if ( LastSpriteTexture == null ) {
			return;
		}
		Image viewImage = GetViewport().GetTexture().GetImage();
		Image spriteImage = LastSpriteTexture.GetImage();

		viewImage = viewImage.GetRegion( LastCropRect );
		viewImage.Resize( (int)LastSpriteTexture.GetSize().X, (int)LastSpriteTexture.GetSize().Y );
		viewImage.Convert( spriteImage.GetFormat() );

		Image finalImage = Image.CreateEmpty( (int)LastSpriteTexture.GetSize().X, (int)LastSpriteTexture.GetSize().Y, false, Image.Format.Rgba8 );
		finalImage.BlitRectMask( viewImage, spriteImage, new Rect2I( Vector2I.Zero, (Vector2I)LastSpriteTexture.GetSize() ), Vector2I.Zero );
		finalImage.FixAlphaEdges();
		finalImage.Resize( 1, 1, Image.Interpolation.Lanczos );
		spriteImage.Resize( 1, 1, Image.Interpolation.Lanczos );

		Color finalColor = finalImage.GetPixel( 0, 0 );
		Color baseColor = spriteImage.GetPixel( 0, 0 );
		if ( baseColor.A != 0.0f ) {
			finalColor = finalColor / finalColor.A;
			baseColor = baseColor / baseColor.A;
			LitValue = finalColor.Luminance - baseColor.Luminance;
		}
	}

	private void OnScreenSizeChanged() {
		ScreenSize = DisplayServer.WindowGetSize();
	}
	public override void _Ready() {
		base._Ready();

		StartingPosition = GlobalPosition;

		AimLine = GetNode<Line2D>( "AimAssist/AimLine" );
		AimRayCast = GetNode<RayCast2D>( "AimAssist/AimLine/RayCast2D" );

		AimLine.Points[1].X = AimLine.Points[0].X * PunchRange;
		AimRayCast.TargetPosition = GlobalPosition * PunchRange;

		Input.SetCustomMouseCursor( ResourceCache.GetTexture( "res://textures/hud/crosshairs/crosshairi.tga" ) );

		Health = 100.0f;
		Rage = 60.0f;

		if ( GameConfiguration.GameMode != GameMode.LocalCoop2 ) {
			LoadKeyboardBinds();
			LoadGamepadBinds();
		}
		LoadSfx();

		SetProcessUnhandledInput( false );

		if ( !Renown.Constants.StartingQuestPath.IsEmpty && GetTree().CurrentScene.Name == "World" ) {
			QuestState.StartContract( ResourceLoader.Load( Renown.Constants.StartingQuestPath ), Renown.Constants.StartingQuestFlags, Renown.Constants.StartingQuestState );
		}

		ResourceCache.KeyboardInputMappings = ResourceLoader.Load( "res://resources/binds/binds_keyboard.tres" );
		ResourceCache.GamepadInputMappings = ResourceLoader.Load( "res://resources/binds/binds_gamepad.tres" );
		ResourceCache.ItemDatabase = ResourceLoader.Load( "res://resources/ItemDatabase.tres" );

		//
		// initialize input context
		//
		SwitchToGamepad = ResourceLoader.Load( "res://resources/binds/actions/keyboard/switch_to_gamepad.tres" );
		SwitchToKeyboard = ResourceLoader.Load( "res://resources/binds/actions/gamepad/switch_to_keyboard.tres" );

		AudioChannel = GetNode<AudioStreamPlayer2D>( "AudioChannel" );
		AudioChannel.VolumeDb = SettingsData.GetEffectsVolumeLinear();

		DashChannel = GetNode<AudioStreamPlayer2D>( "DashChannel" );
		DashChannel.VolumeDb = SettingsData.GetEffectsVolumeLinear();

		CollisionShape2D SoundBounds = GetNode<CollisionShape2D>( "SoundArea/CollisionShape2D" );
		SoundArea = SoundBounds.Shape as CircleShape2D;

		Area2D Area = GetNode<Area2D>( "SoundArea" );
		Area.Connect( "body_shape_entered", Callable.From<Rid, Node2D, int, int>( OnSoundAreaShape2DEntered ) );

		FootSteps = GetNode<FootSteps>( "FootSteps" );

//		SwitchToGamepad.Connect( "triggered", Callable.From<Resource>( SwitchInputMode ) );
//		SwitchToKeyboard.Connect( "triggered", Callable.From<Resource>( SwitchInputMode ) );
//		SwitchToKeyboard.Connect( "triggered", Callable.From( () => { SwitchInputMode( KeyboardInputMappings ); } ) );
//		SwitchToGamepad.Connect( "triggered", Callable.From( () => { SwitchInputMode( GamepadInputMappings ); } ) );

//		if ( Input.GetConnectedJoypads().Count > 0 ) {
//			SwitchInputMode( GamepadInputMappings );
//		} else {
		if ( GameConfiguration.GameMode != GameMode.LocalCoop2 ) {
			SwitchInputMode( ResourceCache.KeyboardInputMappings );
		}
//		}

		DashTime = GetNode<Timer>( "Timers/DashTime" );
		DashTime.SetProcess( false );
		DashTime.SetProcessInternal( false );
		DashTime.Connect( "timeout", Callable.From( OnDashTimeTimeout ) );

		IdleTimer = GetNode<Timer>( "IdleAnimationTimer" );
		IdleTimer.SetProcess( false );
		IdleTimer.SetProcessInternal( false );
		IdleTimer.Connect( "timeout", Callable.From( OnIdleAnimationTimerTimeout ) );

		IdleAnimation = GetNode<AnimatedSprite2D>( "Animations/Idle" );
		IdleAnimation.SetProcess( false );
		IdleAnimation.Connect( "animation_finished", Callable.From( OnIdleAnimationAnimationFinished ) );

		LegAnimation = GetNode<AnimatedSprite2D>( "Animations/Legs" );
		LegAnimation.SetProcess( false );
		LegAnimation.Connect( "animation_looped", Callable.From( OnLegsAnimationLooped ) );

		TorsoAnimation = GetNode<AnimatedSprite2D>( "Animations/Torso" );
		Animations = GetNode( "Animations" );

		WalkEffect = GetNode<GpuParticles2D>( "Animations/DustPuff" );
		WalkEffect.SetProcess( false );
		WalkEffect.SetProcessInternal( false );

		SlideEffect = GetNode<GpuParticles2D>( "Animations/SlidePuff" );
		SlideEffect.SetProcess( false );
		SlideEffect.SetProcessInternal( false );

		DashEffect = GetNode<GpuParticles2D>( "Animations/DashEffect" );
		DashEffect.SetProcess( false );
		DashEffect.SetProcessInternal( false );

		DashLight = GetNode<PointLight2D>( "Animations/DashEffect/PointLight2D" );
		DashLight.SetProcess( false );
		DashLight.SetProcessInternal( false );

		JumpkitSparks = GetNode<AnimatedSprite2D>( "Animations/JumpkitSparks" );
		JumpkitSparks.SetProcess( false );
		JumpkitSparks.SetProcessInternal( false );

		SlideTime = GetNode<Timer>( "Timers/SlideTime" );
		SlideTime.Connect( "timeout", Callable.From( OnSlideTimeout ) );
		DashCooldownTime = GetNode<Timer>( "Timers/DashCooldownTime" );

		for ( int i = 0; i < MAX_WEAPON_SLOTS; i++ ) {
			WeaponSlots[i] = new WeaponSlot();
			WeaponSlots[i].SetIndex( i );
		}

		LastUsedArm = ArmRight;

		ScreenSize = DisplayServer.WindowGetSize();

		GetTree().Root.SizeChanged += OnScreenSizeChanged;

//		RenderingServer.FramePostDraw += () => OnViewportFramePostDraw();
//		RenderingServer.FramePreDraw += () => OnViewportFramePreDraw();

		Console.AddCommand( "suicide", Callable.From( CmdSuicide ), null, 0 );

		if ( SettingsData.GetNetworkingEnabled() ) {
			SteamLobby.Instance.AddPlayer( SteamUser.GetSteamID(),
				new SteamLobby.NetworkNode( this, SendPacket, null ) );
		}

		if ( ArchiveSystem.Instance.IsLoaded() ) {
			Load();
		}
	}

	public override void _PhysicsProcess( double delta ) {
		base._PhysicsProcess( delta );

		AimRayCast.ForceRaycastUpdate();
		if ( AimRayCast.GetCollider() is Entity entity && entity != null && entity.GetFaction() != Faction ) {
			AimLine.DefaultColor = AimingAtTarget;
		} else {
			AimLine.DefaultColor = AimingAtNull;
		}

		float speed = MAX_SPEED;
		if ( ( Flags & PlayerFlags.Dashing ) != 0 ) {
			speed += 1800;
		}
		if ( ( Flags & PlayerFlags.Sliding ) != 0 ) {
			speed += 400;
		}

		InputVelocity = (Godot.Vector2)MoveAction.Get( "value_axis_2d" );
		if ( InputVelocity != Godot.Vector2.Zero ) {
			LinearVelocity = LinearVelocity.MoveToward( InputVelocity * speed, (float)delta * ACCEL );
		} else {
			LinearVelocity = LinearVelocity.MoveToward( Godot.Vector2.Zero, (float)delta * FRICTION );
		}

		GlobalRotation = 0.0f;

		if ( LinearVelocity != Godot.Vector2.Zero ) {
			IdleReset();
		}

		// cool down the jet engine if applicable
		if ( DashBurnout > 0.0f && DashCooldownTime.TimeLeft == 0.0f ) {
			DashBurnout -= 0.10f;
			if ( DashBurnout < 0.0f ) {
				DashBurnout = 0.0f;
			}
		}

		CheckStatus( (float)delta );
		if ( ( Flags & PlayerFlags.BulletTime ) != 0 ) {
			Rage -= 0.05f * (float)delta;
		}
    }
	
	[MethodImpl(MethodImplOptions.AggressiveOptimization)]
	private void CalcArmAnimation( Arm arm, out PlayerAnimationState animState ) {
		arm.Animations.GlobalRotation = ArmAngle;
		arm.Animations.FlipV = TorsoAnimation.FlipH;
		arm.Animations.SpriteFrames = arm.GetAnimationSet();

		if ( arm.GetSlot() == WeaponSlot.INVALID ) {
			arm.Animations.Play( InputVelocity != Godot.Vector2.Zero ? "run" : "idle" );
			animState = InputVelocity != Godot.Vector2.Zero ? PlayerAnimationState.Running :
				PlayerAnimationState.Idle;
		} else {
			WeaponEntity weapon = WeaponSlots[ arm.GetSlot() ].GetWeapon();
			string animationName;
			switch ( weapon.GetCurrentState() ) {
			default:
			case WeaponEntity.WeaponState.Idle:
				animationName = "idle";
				animState = PlayerAnimationState.WeaponIdle;
				break;
			case WeaponEntity.WeaponState.Reload:
				animationName = "reload";
				animState = PlayerAnimationState.WeaponReload;
				break;
			case WeaponEntity.WeaponState.Use:
				animationName = "use";
				animState = PlayerAnimationState.WeaponUse;
				break;
			case WeaponEntity.WeaponState.Empty:
				animationName = "empty";
				animState = PlayerAnimationState.WeaponEmpty;
				break;
			};
			if ( ( weapon.GetLastUsedMode() & WeaponEntity.Properties.IsOneHanded ) != 0 ) {
				if ( ( arm == ArmLeft && !arm.Animations.FlipH )
					|| ( arm == ArmRight && arm.Animations.FlipH ) ) {
					animationName += "_flip";
				}
			}
			if ( ( weapon.GetLastUsedMode() & WeaponEntity.Properties.IsFirearm ) != 0 ) {
				AimLine.Points[1].X = (float)( (Godot.Collections.Dictionary)weapon.GetReserve().AmmoType.Get( "properties" ) )[ "range" ];
			}
			arm.Animations.Play( animationName );
		}
	}
	private void CheckStatus( float delta ) {
		if ( Rage < 100.0f ) {
			if ( ( Flags & PlayerFlags.UsedMana ) != 0 ) {
			}
		}
		if ( FrameDamage > 0.0f ) {
			Rage += FrameDamage * delta;
			FrameDamage = 0.0f;
			Flags |= PlayerFlags.UsedMana;
			HUD.GetRageBar().Show();
		}
		FrameDamage = 0.0f;
		if ( Health < 100.0f ) {
			Health += 0.075f * delta;
			Rage -= 0.5f * delta;
			// mana conversion ratio to health is extremely inefficient

			Flags |= PlayerFlags.UsedMana;
			HUD.GetHealthBar().Show();
		}
		if ( Rage > 100.0f ) {
			Rage = 100.0f;
		} else if ( Rage < 0.0f ) {
			Rage = 0.0f;
		}
	}
	[MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public override void _Process( double delta ) {
		if ( InputVelocity != Godot.Vector2.Zero ) {
			if ( ( Flags & PlayerFlags.Sliding ) == 0 && ( Flags & PlayerFlags.OnHorse ) == 0 ) {
				LegAnimation.Play( "run" );
				LegAnimationState = PlayerAnimationState.Running;
				WalkEffect.Emitting = true;
			}
		} else {
			LegAnimation.Play( "idle" );
			LegAnimationState = PlayerAnimationState.Idle;
			WalkEffect.Emitting = false;
			SlideEffect.Emitting = false;
		}

		if ( !IdleAnimation.IsPlaying() ) {
			TorsoAnimationState = PlayerAnimationState.Idle;
		}

		base._Process( delta );

		if ( SoundLevel > 0.0f ) {
			SoundLevel -= 1.0f;
			if ( SoundLevel < 0.0f ) {
				SoundLevel = 0.0f;
			}
		}
		SoundArea.Radius = SoundLevel;

		GetArmAngle();

		ArmLeft.Animations.Show();
		ArmRight.Animations.Show();

		Arm back;
		Arm front;
		if ( TorsoAnimation.FlipH ) {
			back = ArmRight;
			front = ArmLeft;
		} else {
			back = ArmLeft;
			front = ArmRight;
		}

		if ( HandsUsed == Hands.Both ) {
			if ( TorsoAnimation.FlipH ) {
				Arm tmp = back;
				back = front;
				front = tmp;
			} else {
				front = LastUsedArm;
			}
			back.Animations.Hide();
		} else {
			back.Animations.Show();
		}

		front.Show();

		Animations.MoveChild( back.Animations, 0 );
		Animations.MoveChild( front.Animations, 3 );

		CalcArmAnimation( ArmLeft, out LeftArmAnimationState );
		CalcArmAnimation( ArmRight, out RightArmAnimationState );
	}

	private void OnWeaponModeChanged( WeaponEntity source, WeaponEntity.Properties useMode ) => EmitSignalSwitchedWeaponMode( source, useMode );

	public void PickupAmmo( AmmoEntity ammo ) {
		AmmoStack stack = null;
		bool found = false;
		
		foreach ( var it in AmmoStacks ) {
			if ( ammo.Data == it.Value.AmmoType ) {
				found = true;
				stack = it.Value;
				break;
			}
		}
		if ( !found ) {
			stack = new AmmoStack();
			stack.SetType( ammo );
			AmmoStacks.Add( ( (string)ammo.Data.Get( "id" ) ).GetHashCode(), stack );
		}
		stack.AddItems( (int)( (Godot.Collections.Dictionary)ammo.Data.Get( "properties" ) )[ "stack_add_amount" ] );

		PlaySound( AudioChannel, ammo.GetPickupSound() );

		for ( int i = 0; i < MAX_WEAPON_SLOTS; i++ ) {
			WeaponSlot slot = WeaponSlots[i];
			if ( slot.IsUsed() && (int)slot.GetWeapon().GetAmmoType()
				== (int)( (Godot.Collections.Dictionary)ammo.Data.Get( "properties" ) )[ "type" ] )
			{
				slot.GetWeapon().SetReserve( stack );
				slot.GetWeapon().SetAmmo( ammo.Data );
			}
		}

		LastUsedArm = ArmRight;
	}
	public void PickupWeapon( WeaponEntity weapon ) {
		for ( int i = 0; i < MAX_WEAPON_SLOTS; i++ ) {
			if ( !WeaponSlots[i].IsUsed() ) {
				WeaponSlots[i].SetWeapon( weapon );
				CurrentWeapon = i;
				break;
			}
		}
		
		WeaponsStack.Add( ( (string)weapon.Data.Get( "id ") ).GetHashCode(), weapon );

		TorsoAnimation.FlipH = false;
		LegAnimation.FlipH = false;

		weapon.Connect( "ModeChanged", Callable.From<WeaponEntity, WeaponEntity.Properties>( OnWeaponModeChanged ) );
		weapon.OverrideRayCast( AimRayCast );

		AmmoStack stack = null;
		foreach ( var ammo in AmmoStacks ) {
			if ( (int)( (Godot.Collections.Dictionary)ammo.Value.AmmoType.Get( "properties" ) )[ "type" ] ==
				(int)weapon.GetAmmoType() )
			{
				stack = ammo.Value;
				break;
			}
		}
		if ( stack != null ) {
			weapon.SetReserve( stack );
			weapon.SetAmmo( stack.AmmoType );
		}
		if ( SettingsData.GetEquipWeaponOnPickup() ) {
			// apply rules of various weapon properties
			if ( ( weapon.GetDefaultMode() & WeaponEntity.Properties.IsTwoHanded ) != 0 ) {
				ArmLeft.SetWeapon( CurrentWeapon );
				ArmRight.SetWeapon( CurrentWeapon );

				HandsUsed = Hands.Both;
				LastUsedArm = ArmRight;

				// this will automatically overwrite any other modes
				WeaponSlots[ ArmLeft.GetSlot() ].SetMode( weapon.GetDefaultMode() );
				WeaponSlots[ ArmRight.GetSlot() ].SetMode( weapon.GetDefaultMode() );
			}
			else if ( ( weapon.GetDefaultMode() & WeaponEntity.Properties.IsOneHanded ) != 0 ) {
				LastUsedArm ??= ArmRight;
				LastUsedArm.SetWeapon( CurrentWeapon );

				if ( LastUsedArm == ArmRight ) {
					HandsUsed = Hands.Right;
				} else if ( LastUsedArm == ArmLeft ) {
					HandsUsed = Hands.Left;
				}

				WeaponSlots[ LastUsedArm.GetSlot() ].SetMode( weapon.GetDefaultMode() );
			}

			// update the hand data
			LastUsedArm.SetWeapon( CurrentWeapon );
			WeaponSlots[ LastUsedArm.GetSlot() ].SetMode( weapon.GetProperties() );
			weapon.SetUseMode( weapon.GetDefaultMode() );
			
			EmitSignalSwitchedWeapon( weapon );
		}
	}
};