using Godot;
using System;
using PlayerSystem;
using System.Collections.Generic;
using Steamworks;
using System.Runtime.CompilerServices;
using Renown;
using Renown.World;
using System.Diagnostics;

public enum WeaponSlotIndex : int {
	Primary,
	HeavyPrimary,
	Sidearm,
	HeavySidearm
};

// TODO: dash i-frames duration and speed should decrease if we spam it

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
		IdleAnimation	= 0x00000100,
		Checkpoint		= 0x00000200,
		BlockedInput	= 0x00000400,
		UsingWeapon		= 0x00000800,
		Inventory		= 0x00001000,
		Resting			= 0x00002000,
		UsingMelee		= 0x00004000,
		Parrying		= 0x00008000,
		Encumbured		= 0x00010000,
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

	private static readonly WeaponEntity.Properties[] WeaponModeList = [
		WeaponEntity.Properties.IsOneHanded | WeaponEntity.Properties.IsBladed,
		WeaponEntity.Properties.IsOneHanded | WeaponEntity.Properties.IsBlunt,
		WeaponEntity.Properties.IsOneHanded | WeaponEntity.Properties.IsFirearm,

		WeaponEntity.Properties.IsTwoHanded | WeaponEntity.Properties.IsBladed,
		WeaponEntity.Properties.IsTwoHanded | WeaponEntity.Properties.IsBlunt,
		WeaponEntity.Properties.IsTwoHanded | WeaponEntity.Properties.IsFirearm
	];

	private Godot.Vector2 LastNetworkPosition = Godot.Vector2.Zero;
	private PlayerAnimationState LastLeftArmAnimationState;
	private PlayerAnimationState LastRightArmAnimationState;
	private PlayerAnimationState LastLegArmAnimationState;
	private PlayerAnimationState LastTorsoArmAnimationState;
	private float LastNetworkAimAngle = 0.0f;
	private uint LastNetworkFlags = 0;
	private float LastNetworkBloodAmount = 0.0f;
	private WeaponEntity.Properties LastNetworkUseMode = WeaponEntity.Properties.None;
	private ulong LastNetworkWeaponRID = 0;

	private static readonly float PunchRange = 40.0f;
	private static readonly int MAX_WEAPON_SLOTS = 4;
	private static readonly Color AimingAtTarget = new Color( 1.0f, 0.0f, 0.0f, 1.0f );
	private static readonly Color AimingAtNull = new Color( 0.5f, 0.5f, 0.0f, 1.0f );

	public static bool InCombat = false;
	public static int NumTargets = 0;

	public static readonly float ACCEL = 1600.0f;
	public static readonly float FRICTION = 1400.0f;
	public static readonly float MAX_SPEED = 440.0f;
	public static readonly float JUMP_VELOCITY = -400.0f;

	public static Godot.Vector2I ScreenSize = Godot.Vector2I.Zero;

	[Export]
	private Node Inventory;
	[Export]
	private Arm ArmLeft;
	[Export]
	private Arm ArmRight;
	[Export]
	private HeadsUpDisplay HUD;

	[ExportCategory( "Start" )]
	[Export]
	private PlayerFlags Flags = 0;

	[ExportCategory( "Camera Shake" )]
	[Export]
	private float RandomStrength = 36.0f;
	[Export]
	private float ShakeFade = 5.0f;

	private Area2D ParryArea;
	private CollisionShape2D ParryBox;
	private Area2D ParryDamageArea;
	private CollisionShape2D ParryDamageBox;

	/// <summary>
	/// The maximum amount of weight the player is currently allowed to carry
	/// </summary>
	public float MaximumInventoryWeight {
		get;
		private set;
	} = 500.0f;

	/// <summary>
	/// The current total weight of inventory items
	/// </summary>
	public float TotalInventoryWeight {
		get;
		private set;
	} = 0.0f;

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
	private Resource ArmAngleAction;
	private Resource UseBothHandsAction;
	public readonly Resource InteractionAction;

	public readonly Resource InteractionActionGamepad;

	public readonly Resource InteractionActionKeyboard;

	private GpuParticles2D WalkEffect;
	private GpuParticles2D SlideEffect;

	private Area2D DashFlameArea;
	private GpuParticles2D DashEffect;
	private PointLight2D DashLight;

	private Godot.Vector2 PhysicsPosition = Godot.Vector2.Zero;

	private Node Animations;
	private SpriteFrames DefaultLeftArmAnimations;
	private AnimatedSprite2D TorsoAnimation;
	private AnimatedSprite2D LegAnimation;
	private AnimatedSprite2D IdleAnimation;

	private Timer IdleTimer;

	private Timer CheckpointDrinkTimer;

	private Timer DashTime;
	private Timer SlideTime;
	private Timer DashBurnoutCooldownTimer;
	private Timer DashCooldownTime;

	public GroundMaterialType GroundType {
		get;
		private set;
	}

	// how much blood we're covered in
	private float BloodAmount = 0.0f;
	private Timer BloodDropTimer;
	private ShaderMaterial BloodMaterial;

	private static float ShakeStrength = 0.0f;

	private static bool TutorialCompleted = false;

	private WeaponSlot[] WeaponSlots = new WeaponSlot[ MAX_WEAPON_SLOTS ];

	private float Rage = 60.0f;
	private int CurrentWeapon = WeaponSlot.INVALID;

	private Camera2D Viewpoint;

	//
	// multiplayer data
	//
	public Multiplayer.PlayerData.MultiplayerMetadata MultiplayerData;

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
	private AudioStreamPlayer2D MiscChannel;

	private FootSteps FootSteps;

	private NetworkSyncObject SyncObject = new NetworkSyncObject( 1024 );

	private TileMapFloor Floor;

	private Dictionary<int, WeaponEntity> WeaponsStack = new Dictionary<int, WeaponEntity>();
	private Dictionary<int, ConsumableStack> ConsumableStacks = new Dictionary<int, ConsumableStack>();
	private Dictionary<int, AmmoStack> AmmoStacks = new Dictionary<int, AmmoStack>();

	private Hands HandsUsed = Hands.Right;
	private Arm LastUsedArm;
	public static int ComboCounter = 0;
	private float ArmAngle = 0.0f;
	private float DashBurnout = 0.0f;
	private float DashTimer = 0.0f;
	private int InputDevice = 0;
	private float FrameDamage = 0.0f;
	private int Hellbreaks = 0;
	private bool SplitScreen = false;
	private float SoundLevel = 0.1f;
	private Godot.Vector2 DashDirection = Godot.Vector2.Zero;
	private Godot.Vector2 InputVelocity = Godot.Vector2.Zero;
	private Godot.Vector2 LastMousePosition = Godot.Vector2.Zero;

	private CircleShape2D SoundArea;

	// used when LastCheckpoint is null
	private Godot.Vector2 StartingPosition = Godot.Vector2.Zero;
	private Checkpoint LastCheckpoint;

	private int TileMapLevel = 0;

	private WorldArea Waypoint;
	private List<Resource> Quests;

	[Signal]
	public delegate void SwitchedWeaponEventHandler( WeaponEntity weapon );
	[Signal]
	public delegate void SwitchHandUsedEventHandler( Hands hands );
	[Signal]
	public delegate void UsedWeaponEventHandler( WeaponEntity source );
	[Signal]
	public delegate void WeaponPickedUpEventHandler( WeaponEntity source );
	[Signal]
	public delegate void AmmoPickedUpEventHandler( AmmoEntity source );
	[Signal]
	public delegate void LocationChangedEventHandler( WorldArea location );
	[Signal]
	public delegate void HandsStatusUpdatedEventHandler( Hands handsUsed );
	[Signal]
	public delegate void WeaponStatusUpdatedEventHandler( WeaponEntity source, WeaponEntity.Properties useMode );

	private void OnFlameAreaBodyShape2DEntered( Rid bodyRid, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		if ( body is Entity entity && entity != null ) {
			entity.AddStatusEffect( "status_burning" );
		}
	}

	public override void AddStatusEffect( string effectName ) {
		base.AddStatusEffect( effectName );

		HUD.AddStatusEffect( effectName, StatusEffects[ effectName ] );
	}

	public static bool IsTutorialActive() {
		return !TutorialCompleted;
	}

	public void SetTileMapFloorLevel( int nLevel ) => TileMapLevel = nLevel;
	public int GetTileMapFloorLevel() => TileMapLevel;

	public void SetSoundLevel( float nSoundLevel ) {
		if ( nSoundLevel > SoundLevel ) {
			SoundLevel = nSoundLevel;
		}
	}

	private void IncreaseInventoryWeight( float nAmount ) {
		TotalInventoryWeight += nAmount;
		if ( TotalInventoryWeight >= MaximumInventoryWeight * 0.80f ) {
			Flags |= PlayerFlags.Encumbured;
		}
	}
	private void DecreaseInventoryWeight( float nAmount ) {
		TotalInventoryWeight -= nAmount;
		if ( TotalInventoryWeight < MaximumInventoryWeight * 0.80f ) {
			Flags &= ~PlayerFlags.Encumbured;
		}
	}

	#region Load & Save
	public override void Save() {
		using ( var writer = new SaveSystem.SaveSectionWriter( "SaveState" ) ) {
			writer.SaveString( "Location", Location.GetAreaName() );
			writer.SaveUInt( "TimeYear", WorldTimeManager.Year );
			writer.SaveUInt( "TimeMonth", WorldTimeManager.Month );
			writer.SaveUInt( "TimeDay", WorldTimeManager.Day );
		}

		using ( var writer = new SaveSystem.SaveSectionWriter( "Player" ) ) {
			int stackIndex;

			writer.SaveFloat( "Health", Health );
			writer.SaveFloat( "Rage", Rage );
			writer.SaveInt( "Hellbreaks", Hellbreaks );
			writer.SaveInt( "CurrentWeapon", CurrentWeapon );
			writer.SaveUInt( "HandsUsed", (uint)HandsUsed );

			writer.SaveVector2( "Position", GlobalPosition );

			writer.SaveInt( "ArmLeftSlot", ArmLeft.Slot );
			writer.SaveInt( "ArmRightSlot", ArmRight.Slot );

			writer.SaveInt( "AmmoStacksCount", AmmoStacks.Count );
			stackIndex = 0;
			foreach ( var stack in AmmoStacks ) {
				// TODO: FIXME!
				writer.SaveInt( string.Format( "AmmoStacksAmount{0}", stackIndex ), stack.Value.Amount );
				writer.SaveString( string.Format( "AmmoStacksType{0}", stackIndex ), (string)stack.Value.AmmoType.Data.Get( "id" ) );
				stackIndex++;
			}

			writer.SaveInt( "WeaponStacksCount", WeaponsStack.Count );
			stackIndex = 0;
			foreach ( var stack in WeaponsStack ) {
				writer.SaveString( string.Format( "WeaponStacksPath{0}", stackIndex ), (string)stack.Value.InitialPath );
				if ( ( stack.Value.PropertyBits & WeaponEntity.Properties.IsFirearm ) != 0 ) {
					writer.SaveInt( string.Format( "WeaponStacksBulletCount{0}", stackIndex ), stack.Value.BulletsLeft );
				}
				stackIndex++;
			}

			writer.SaveInt( "MaxWeaponSlots", MAX_WEAPON_SLOTS );
			for ( int i = 0; i < WeaponSlots.Length; i++ ) {
				writer.SaveBool( string.Format( "WeaponSlotUsed{0}", i ), WeaponSlots[ i ].IsUsed() );
				if ( WeaponSlots[ i ].IsUsed() ) {
					NodePath weaponId = "";
					foreach ( var stack in WeaponsStack ) {
						if ( stack.Value == WeaponSlots[ i ].GetWeapon() ) {
							weaponId = stack.Value.InitialPath;
							break;
						}
					}
					writer.SaveString( string.Format( "WeaponSlotHash{0}", i ), weaponId );
					writer.SaveUInt( string.Format( "WeaponSlotMode{0}", i ), (uint)WeaponSlots[ i ].GetMode() );
				}
			}

			writer.SaveInt( "ConsumableStacksCount", ConsumableStacks.Count );
			stackIndex = 0;
			foreach ( var stack in ConsumableStacks ) {
				writer.SaveInt( string.Format( "ConsumableStacksAmount{0}", stackIndex ), stack.Value.Amount );
				writer.SaveString( string.Format( "ConsumableStacksType{0}", stackIndex ), (string)stack.Value.ItemType.Get( "id" ) );
				stackIndex++;
			}
		}
	}
	public override void Load() {
		SaveSystem.SaveSectionReader reader = ArchiveSystem.GetSection( "Player" );

		Health = reader.LoadFloat( "Health" );
		Rage = reader.LoadFloat( "Rage" );
		Hellbreaks = reader.LoadInt( "Hellbreaks" );
		CurrentWeapon = reader.LoadInt( "CurrentWeapon" );
		HandsUsed = (Hands)reader.LoadUInt( "HandsUsed" );

		GlobalPosition = reader.LoadVector2( "Position" );

		ArmLeft.Slot = reader.LoadInt( "ArmLeftSlot" );
		ArmRight.Slot = reader.LoadInt( "ArmRightSlot" );
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
		int numAmmoStacks = reader.LoadInt( "AmmoStacksCount" );
		for ( int i = 0; i < numAmmoStacks; i++ ) {
			AmmoStack stack = new AmmoStack();
			stack.Amount = reader.LoadInt( string.Format( "AmmoStacksAmount{0}", i ) );
			string id = reader.LoadString( string.Format( "AmmoStacksType{0}", i ) );
			stack.AmmoType = new AmmoEntity();
			stack.AmmoType.Data = (Resource)( (Resource)Inventory.Get( "database" ) ).Call( "get_item", id );
			AmmoStacks.Add( id.GetHashCode(), stack );
			CallDeferred( "add_child", stack.AmmoType );
		}

		WeaponsStack.Clear();
		int numWeapons = reader.LoadInt( "WeaponStacksCount" );
		for ( int i = 0; i < numWeapons; i++ ) {
			CallDeferred( "LoadWeapon", reader.LoadString( string.Format( "WeaponStacksPath{0}", i ) ), reader.LoadInt( string.Format( "WeaponStacksBulletCount{0}", i ) ) );
		}

		int maxSlots = reader.LoadInt( "MaxWeaponSlots" );
		for ( int i = 0; i < maxSlots; i++ ) {
			if ( reader.LoadBoolean( string.Format( "WeaponSlotUsed{0}", i ) ) ) {
				CallDeferred( "InitWeaponSlot", i, reader.LoadString( string.Format( "WeaponSlotHash{0}", i ) ), reader.LoadUInt( string.Format( "WeaponSlotMode{0}", i ) ) );
			}
		}

		ConsumableStacks.Clear();
		int numConsumableStacks = reader.LoadInt( "ConsumableStacksCount" );
		for ( int i = 0; i < numConsumableStacks; i++ ) {
			ConsumableStack stack = new ConsumableStack();
			stack.Amount = reader.LoadInt( string.Format( "ConsumableStacksAmount{0}", i ) );
			string id = reader.LoadString( string.Format( "ConsumableStacksType{0}", i ) );
			stack.ItemType = (Resource)( (Resource)Inventory.Get( "database" ) ).Call( "get_item", id );
			ConsumableStacks.Add( id.GetHashCode(), stack );
		}

		CallDeferred( "emit_signal", "SwitchedWeapon", WeaponSlots[ CurrentWeapon ].GetWeapon() );
	}

	public void SetTileMapFloor( TileMapFloor floor ) => Floor = floor;
	public TileMapFloor GetTileMapFloor() => Floor;

	public void ThoughtBubble( string text ) {
		HeadsUpDisplay.StartThoughtBubble( text );
	}

	private void InitWeaponSlot( int nSlot, NodePath path, uint mode ) {
		WeaponEntity weapon = WeaponsStack[ path.GetHashCode() ];
		weapon.SetEquippedState( true );
		WeaponSlots[ nSlot ].SetWeapon( weapon );
		WeaponSlots[ nSlot ].SetMode( (WeaponEntity.Properties)mode );
	}
	private void LoadWeapon( NodePath nodePath, int bulletCount ) {
		WeaponEntity weapon = GetNode<WeaponEntity>( nodePath );

		weapon._Owner = this;
		weapon.OverrideRayCast( AimRayCast );

		weapon.OwnerLoad( bulletCount );

		AmmoStack stack = null;
		foreach ( var it in AmmoStacks ) {
			if ( (int)( (Godot.Collections.Dictionary)it.Value.AmmoType.Data.Get( "properties" ) )[ "type" ] ==
				(int)weapon.Ammunition ) {
				stack = it.Value;
				break;
			}
		}
		if ( stack != null ) {
			weapon.SetReserve( stack );
			weapon.SetAmmo( stack.AmmoType );
		}
	}
	#endregion

	private void ReceivePacket( System.IO.BinaryReader reader ) {
		SyncObject.BeginRead( reader );

		PlayerUpdateType type = (PlayerUpdateType)SyncObject.ReadByte();
		switch ( type ) {
		case PlayerUpdateType.Damage: {
				switch ( (PlayerDamageSource)SyncObject.ReadByte() ) {
				case PlayerDamageSource.Player:
					Damage( SteamLobby.Instance.GetPlayer( (CSteamID)SyncObject.ReadUInt64() ), SyncObject.ReadFloat() );
					break;
				case PlayerDamageSource.NPC:
					break;
				case PlayerDamageSource.Environment:
					break;
				};
				break;
			}
		case PlayerUpdateType.Count:
		default:
			Console.PrintError( string.Format( "Player.ReceivePacket: invalid PlayerUpdateType {0}", (byte)type ) );
			break;
		};
	}

	private void SendPacket() {
		if ( GameConfiguration.GameMode != GameMode.Online && GameConfiguration.GameMode != GameMode.Multiplayer ) {
			return;
		}
		SyncObject.Write( (byte)SteamLobby.MessageType.ClientData );
		SyncObject.Write( TorsoAnimation.FlipH );

		SyncObject.Write( GlobalPosition );

		if ( (uint)Flags != LastNetworkFlags ) {
			SyncObject.Write( true );
			SyncObject.Write( (uint)Flags );
			LastNetworkFlags = (uint)Flags;
		} else {
			SyncObject.Write( false );
		}

		if ( LastNetworkAimAngle != AimLine.GlobalRotation ) {
			SyncObject.Write( true );
			LastNetworkAimAngle = AimLine.GlobalRotation;
			SyncObject.Write( LastNetworkAimAngle );
		} else {
			SyncObject.Write( false );
		}

		if ( LastNetworkBloodAmount != BloodAmount ) {
			SyncObject.Write( true );
			SyncObject.Write( BloodAmount );
			LastNetworkBloodAmount = BloodAmount;
		} else {
			SyncObject.Write( false );
		}
		if ( CurrentWeapon == WeaponSlot.INVALID ) {
			SyncObject.Write( false );
			SyncObject.Write( false );
		} else {
			WeaponEntity weapon = WeaponSlots[ CurrentWeapon ].GetWeapon();
			if ( weapon == null ) {
				SyncObject.Write( false );
				SyncObject.Write( false );
			} else {
				ulong RID = weapon.Data.GetRid().Id;
				if ( RID != LastNetworkWeaponRID ) {
					SyncObject.Write( true );
					SyncObject.Write( (string)weapon.Data.Get( "id" ) );
					LastNetworkWeaponRID = RID;
				}
				if ( weapon.LastUsedMode != LastNetworkUseMode ) {
					SyncObject.Write( true );
					SyncObject.Write( (uint)weapon.LastUsedMode );
					LastNetworkUseMode = weapon.LastUsedMode;
				}
			}
		}

		SyncObject.Write( (byte)LeftArmAnimationState );
		SyncObject.Write( (byte)RightArmAnimationState );
		SyncObject.Write( (byte)LegAnimationState );
		SyncObject.Write( (byte)TorsoAnimationState );

		SyncObject.Sync( Steamworks.Constants.k_nSteamNetworkingSend_Unreliable );
		/*

		SyncObject.Write( TorsoAnimation.FlipH );

		SyncObject.Write( GlobalPosition );

		SyncObject.Write( ArmLeft.Animations.GlobalRotation );
		SyncObject.Write( ArmRight.Animations.GlobalRotation );

		SyncObject.Write( (byte)LeftArmAnimationState );
		SyncObject.Write( (byte)RightArmAnimationState );
		SyncObject.Write( (byte)LegAnimationState );
		SyncObject.Write( (byte)TorsoAnimationState );

		SyncObject.Write( (byte)HandsUsed );

		SyncObject.Write( (sbyte)CurrentWeapon );
		if ( CurrentWeapon != WeaponSlot.INVALID ) {
			SyncObject.Write( (uint)WeaponSlots[ CurrentWeapon ].GetMode() );
			SyncObject.Write( WeaponSlots[ CurrentWeapon ].IsUsed() );
			if ( WeaponSlots[ CurrentWeapon ].IsUsed() ) {
				SyncObject.Write( ( (string)WeaponSlots[ CurrentWeapon ].GetWeapon().Data.Get( "id" ) ).GetHashCode() );
			}
		}
		SyncObject.Sync();
		*/
	}

	private void OnSoundAreaShape2DEntered( Rid bodyRid, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		if ( body is Renown.Thinkers.Thinker mob && mob != null ) {
			if ( mob.GetTileMapFloor() == Floor ) {
				mob.Alert( this );
			}
		}
	}
	private void OnSoundAreaShape2DExited( Rid bodyRid, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
	}

	private void IncreaseBlood( float nAmount ) {
		if ( !SettingsData.GetShowBlood() ) {
			return;
		}
		BloodAmount += nAmount;
		BloodMaterial.SetShaderParameter( "blood_coef", BloodAmount );
		BloodDropTimer.Start();
	}
	private void OnBloodDropTimerTimeout() {
		if ( !SettingsData.GetShowBlood() ) {
			return;
		}
		BloodAmount -= 0.001f;
		BloodMaterial.SetShaderParameter( "blood_coef", BloodAmount );

		if ( BloodAmount < 0.0f ) {
			BloodAmount = 0.0f;
			BloodDropTimer.Stop();
		}
	}

	public override void SetLocation( in WorldArea location ) {
		if ( location != Location ) {
			EmitSignalLocationChanged( location );
		}

		base.SetLocation( location );
	}

	public AnimatedSprite2D GetTorsoAnimation() => TorsoAnimation;
	public AnimatedSprite2D GetLegsAnimation() => LegAnimation;
	public AnimatedSprite2D GetLeftArmAnimation() => ArmLeft.Animations;
	public AnimatedSprite2D GetRightArmAnimation() => ArmRight.Animations;

	public WeaponSlot GetPrimaryWeapon() => WeaponSlots[ (int)WeaponSlotIndex.Primary ];
	public WeaponSlot GetHeavyPrimaryWeapon() => WeaponSlots[ (int)WeaponSlotIndex.HeavyPrimary ];
	public WeaponSlot GetSidearmWeapon() => WeaponSlots[ (int)WeaponSlotIndex.Sidearm ];
	public WeaponSlot GetHeavySidearmWeapon() => WeaponSlots[ (int)WeaponSlotIndex.HeavySidearm ];

	public Resource GetCurrentMappingContext() => CurrentMappingContext;
	public WeaponSlot[] GetWeaponSlots() => WeaponSlots;
	public Dictionary<int, WeaponEntity> GetWeaponStack() => WeaponsStack;
	public Dictionary<int, AmmoStack> GetAmmoStacks() => AmmoStacks;
	public Node GetInventory() => Inventory;
	public Arm GetWeaponHand( WeaponEntity weapon ) {
		if ( ArmLeft.Slot != WeaponSlot.INVALID && WeaponSlots[ ArmLeft.Slot ].GetWeapon() == weapon ) {
			return ArmLeft;
		} else if ( ArmRight.Slot != WeaponSlot.INVALID && WeaponSlots[ ArmRight.Slot ].GetWeapon() == weapon ) {
			return ArmRight;
		}
		return null;
	}
	public Godot.Vector2 GetInputVelocity() => InputVelocity;
	public WeaponSlot GetSlot( int nSlot ) => WeaponSlots[ nSlot ];
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

			if ( LastMousePosition != mousePosition ) {
				LastMousePosition = mousePosition;
				IdleReset();
			}

			ArmAngle = GetLocalMousePosition().Angle();
			AimLine.GlobalRotation = ArmAngle;
			AimRayCast.TargetPosition = AimLine.Points[ 1 ];
			if ( mousePosition.X >= ScreenSize.X / 2.0f ) {
				FlipSpriteRight();
			} else if ( mousePosition.X <= ScreenSize.X / 2.0f ) {
				FlipSpriteLeft();
			}
		}
		return ArmAngle;
	}
	public void SetArmAngle( float nAngle ) => ArmAngle = nAngle;
	public Arm GetLastUsedArm() => LastUsedArm;
	public void SetLastUsedArm( in Arm arm ) => LastUsedArm = arm;
	public float GetSoundLevel() => SoundLevel;
	public void SetHealth( float nHealth ) {
		Health = nHealth;
		if ( Health > 100.0f ) {
			Health = 100.0f;
		}
		HUD.GetHealthBar().SetHealth( Health );
	}
	public float GetRage() => Rage;
	public void SetRage( float nRage ) {
		Rage = nRage;
		HUD.GetRageBar().Value = Rage;
	}
	public PlayerFlags GetFlags() => Flags;
	public void SetFlags( PlayerFlags flags ) => Flags = flags;
	public Hands GetHandsUsed() => HandsUsed;
	public void SetHandsUsed( Hands hands ) => HandsUsed = hands;
	public WeaponSlot[] GetSlots() => WeaponSlots;
	public void SetSlots( WeaponSlot[] slots ) => WeaponSlots = slots;

	public void SetGroundMaterial( GroundMaterialType nType ) => GroundType = nType;

	public Arm GetLeftArm() => ArmLeft;
	public Arm GetRightArm() => ArmRight;
	public int GetCurrentWeapon() => CurrentWeapon;

	public static void ShakeCamera( float nAmount ) {
		ShakeStrength += nAmount;
	}

	private void IdleReset() {
		IdleTimer.Start();
		IdleAnimation.Hide();
		IdleAnimation.Stop();

		LegAnimation.Show();
		TorsoAnimation.Show();
		ArmRight.Animations.Show();
		ArmLeft.Animations.Show();
	}

	public override void PlaySound( in AudioStreamPlayer2D channel, in AudioStream stream ) {
		if ( channel == null ) {
			AudioChannel.Stream = stream;
			AudioChannel.Play();
		} else {
			channel.Stream = stream;
			channel.Play();
		}
	}

	public void SetupSplitScreen( int nInputIndex ) {
		if ( Input.GetConnectedJoypads().Count > 0 ) {
			SplitScreen = true;
		}
		InputDevice = nInputIndex;
	}

	private void Respawn() {
		Console.PrintLine( "Respawning player..." );

		if ( LastCheckpoint == null ) {
			GlobalPosition = StartingPosition;
		} else {
			GlobalPosition = LastCheckpoint.GlobalPosition;
			BeginInteraction( LastCheckpoint );
		}

		Flags &= ~PlayerFlags.Dashing;
		BlockInput( false );

		Health = 100.0f;
		Rage = 60.0f;

		TorsoAnimation.Play( "default" );
		LegAnimation.Play( "idle" );
		ArmLeft.Animations.Play( "idle" );
		ArmRight.Animations.Play( "idle" );

		TorsoAnimation.Show();
		LegAnimation.Show();
		ArmLeft.Animations.Show();
		ArmRight.Animations.Show();

		HUD.GetHealthBar().SetHealth( Health );
		HUD.GetRageBar().Value = Rage;

		if ( ( GameConfiguration.GameMode == GameMode.Online && SteamLobby.Instance.IsOwner() ) || GameConfiguration.GameMode == GameMode.SinglePlayer ) {
			ArchiveSystem.SaveGame( SettingsData.GetSaveSlot() );
		}
	}

	private void OnDeath( Entity attacker ) {
		EmitSignalDie( attacker, this );

		PlaySound( AudioChannel, ResourceCache.PlayerDieSfx[ RNJesus.IntRange( 0, ResourceCache.PlayerDieSfx.Length - 1 ) ] );

		// check if hellbreaker is an option
		if ( GameConfiguration.GameMode == GameMode.ChallengeMode && SettingsData.GetHellbreakerEnabled() && Hellbreaker.CanActivate() ) {
			return;
		}

		// make sure we're not calling this over and over
		Flags |= PlayerFlags.Dashing;
		BlockInput( true );

		LegAnimation.Hide();
		ArmLeft.Animations.Hide();
		ArmRight.Animations.Hide();

		Velocity = Godot.Vector2.Zero;

		TorsoAnimation.Play( "death" );

		SetProcessUnhandledInput( true );
		SetProcess( false );

		EmitSignalDie( attacker, this );
	}

	public override void Damage( in Entity attacker, float nAmount ) {
		if ( ( Flags & PlayerFlags.Dashing ) != 0 ) {
			return; // iframes
		}
		ShakeCamera( nAmount );

		FreeFlow.EndCombo();

		System.Threading.Interlocked.Exchange( ref Health, Health - ( nAmount * 0.75f ) );
		System.Threading.Interlocked.Exchange( ref Rage, Rage + ( nAmount * 0.01f ) );
		if ( Rage > 100.0f ) {
			System.Threading.Interlocked.Exchange( ref Rage, 100.0f );
		}

		if ( attacker != null ) {
			BloodParticleFactory.Create( attacker.GlobalPosition, GlobalPosition );
		}

		if ( Health <= 0.0f ) {
			CallDeferred( "OnDeath", attacker );
		} else {
			CallDeferred( "PlaySound", AudioChannel, ResourceCache.PlayerPainSfx[ RNJesus.IntRange( 0, ResourceCache.PlayerPainSfx.Length - 1 ) ] );
		}

		CallDeferred( "emit_signal", "Damaged", attacker, this, nAmount );
	}

	public void OnParry( Bullet from, float damage ) {
		float distance = from.GlobalPosition.DistanceTo( from.GlobalPosition );

		// we punch the bullet or object so hard it creates a shrapnel cloud
		AimRayCast.TargetPosition = Godot.Vector2.Right.Rotated( ArmAngle ) * distance;
		if ( AimRayCast.GetCollider() is GodotObject collision && collision != null ) {
			if ( collision is Entity entity && entity != null ) {
				entity.Damage( this, damage );
				entity.AddStatusEffect( "status_burning" );
			}
		}
	}
	public void OnParry( RayCast2D from, float damage ) {
		//		float distance = from.GlobalPosition.DistanceTo( from.TargetPosition );

		if ( ( Flags & PlayerFlags.Parrying ) == 0 ) {
			return;
		}

		PlaySound( MiscChannel, ResourceCache.GetSound( "res://sounds/env/RICOCHE2.wav" ) );

		HUD.GetParryOverlay().Show();

		// we punch the bullet or object so hard it creates a shrapnel cloud
		Godot.Collections.Array<Node2D> entities = ParryDamageArea.GetOverlappingBodies();
		for ( int i = 0; i < entities.Count; i++ ) {
			if ( entities[ i ] is Entity entity && entity != null ) {
				entity.Damage( this, damage );
				entity.AddStatusEffect( "status_burning" );
			}
		}
		Godot.Collections.Array<Area2D> areas = ParryDamageArea.GetOverlappingAreas();
		for ( int i = 0; i < areas.Count; i++ ) {
			if ( areas[ i ] is Hitbox hitbox && hitbox != null ) {
				hitbox.OnHit( this );
			}
		}

		Rage -= 35.0f;

		// create a freeze frame
		float timeScale = (float)Engine.TimeScale;
		Engine.TimeScale = 0.25f;
		ToSignal( GetTree().CreateTimer( 0.30f, false, false, true ), SceneTreeTimer.SignalName.Timeout ).OnCompleted( new Action( () => {
			Engine.TimeScale = timeScale;

			ParryDamageArea.SetDeferred( "monitoring", false );
			HUD.GetParryOverlay().Hide();
		} ) );
	}

	public void BlockInput( bool bBlocked ) {
		if ( bBlocked ) {
			Flags |= PlayerFlags.BlockedInput;
		} else {
			Flags &= ~PlayerFlags.BlockedInput;
		}
		Velocity = Godot.Vector2.Zero;
	}

	private void OnCheckpointRestBegin() {
		TorsoAnimationState = PlayerAnimationState.CheckpointIdle;
		LegAnimationState = PlayerAnimationState.CheckpointIdle;
		LeftArmAnimationState = PlayerAnimationState.CheckpointIdle;
		RightArmAnimationState = PlayerAnimationState.CheckpointIdle;

		TorsoAnimation.Hide();
		LegAnimation.Hide();
		ArmLeft.Animations.Hide();
		ArmRight.Animations.Hide();

		IdleAnimation.Show();
		IdleAnimation.Play( "checkpoint_idle" );

		Vector2 direction = GlobalPosition.DirectionTo( HUD.GetCurrentCheckpoint().GlobalPosition );
		if ( direction.X < 0.0f ) {
			IdleAnimation.FlipH = true;
		} else {
			IdleAnimation.FlipH = false;
		}

		GetNode<CanvasLayer>( "/root/TransitionScreen" ).Disconnect( "transition_finished", Callable.From( OnCheckpointRestBegin ) );
		CheckpointDrinkTimer.ProcessMode = ProcessModeEnum.Pausable;
		CheckpointDrinkTimer.Start();
	}
	private void OnCheckpointExitEnd() {
		if ( ( Flags & PlayerFlags.Resting ) == 0 ) {
			return;
		}
		Debug.Assert( ( Flags & PlayerFlags.Resting ) != 0 );

		IdleTimer.Start();
		IdleAnimation.FlipH = false;
		IdleAnimation.Hide();
		IdleAnimation.Disconnect( "animation_finished", Callable.From( OnCheckpointExitEnd ) );
		BlockInput( false );
		SetProcessUnhandledInput( false );

		Flags &= ~PlayerFlags.Resting;
	}
	public void LeaveCampfire() {
		CheckpointDrinkTimer.Stop();
		CheckpointDrinkTimer.ProcessMode = ProcessModeEnum.Disabled;
		IdleAnimation.Play( "checkpoint_exit" );
		IdleAnimation.Connect( "animation_finished", Callable.From( OnCheckpointExitEnd ) );
	}
	public void RestAtCampfire() {
		if ( ( Flags & PlayerFlags.Resting ) != 0 ) {
			return;
		}
		Debug.Assert( ( Flags & PlayerFlags.Resting ) == 0 );

		IdleTimer.Stop();

		// clean all the blood off us
		BloodAmount = 0.0f;
		BloodMaterial.SetShaderParameter( "blood_coef", BloodAmount );

		GetNode<CanvasLayer>( "/root/TransitionScreen" ).Connect( "transition_finished", Callable.From( OnCheckpointRestBegin ) );
		GetNode<CanvasLayer>( "/root/TransitionScreen" ).Call( "transition" );

		Flags |= PlayerFlags.Resting;

		SetProcessUnhandledInput( true );
		BlockInput( true );
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
		if ( IdleAnimation.IsPlaying() || ( Flags & PlayerFlags.Resting ) != 0 ) {
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
	private void OnIdleAnimationAnimationFinished() {
		if ( ( Flags & PlayerFlags.Resting ) != 0 ) {
			return;
		}
		TorsoAnimationState = PlayerAnimationState.TrueIdleLoop;
		LegAnimationState = PlayerAnimationState.TrueIdleLoop;
		LeftArmAnimationState = PlayerAnimationState.TrueIdleLoop;
		RightArmAnimationState = PlayerAnimationState.TrueIdleLoop;

		IdleAnimation.Play( "loop" );
	}
	private void OnLegsAnimationLooped() {
		if ( Velocity != Godot.Vector2.Zero ) {
			FootSteps.AddStep( Velocity, GlobalPosition, GroundType );
			SetSoundLevel( 24.0f );
		}
	}
	private void OnDashTimeTimeout() {
		HUD.GetDashOverlay().Hide();
		DashLight.Hide();
		DashEffect.Emitting = false;
		DashFlameArea.Monitoring = false;
		Flags &= ~PlayerFlags.Dashing;
		DashCooldownTime.Start();
	}
	private void OnDashBurnoutCooldownTimerTimeout() {
		DashBurnout = 0.0f;
		DashTimer = 0.6f;

		DashTime.WaitTime = DashTimer;

		PlaySound( DashChannel, ResourceCache.GetSound( "res://sounds/player/dash_chargeup.ogg" ) );
	}
	private void OnSlideTimeout() {
		SlideEffect.Emitting = false;
		Flags &= ~PlayerFlags.Sliding;
	}

	private bool IsInputBlocked( bool bIsInventory = false ) => ( Flags & PlayerFlags.BlockedInput ) != 0 || ( !bIsInventory && ( Flags & PlayerFlags.Inventory ) != 0 );
	private void OnDash() {
		if ( IsInputBlocked() || DashBurnoutCooldownTimer.TimeLeft > 0.0f ) {
			return;
		}

		// TODO: upgradable dash burnout?
		if ( DashBurnout >= 1.0f ) {
			PlaySound( AudioChannel, ResourceCache.DashExplosion );

			AddChild( ResourceCache.GetScene( "res://scenes/effects/explosion.tscn" ).Instantiate<Explosion>() );

			Flags &= ~PlayerFlags.Dashing;

			DashBurnoutCooldownTimer.Start();

			Damage( this, 20.0f );
			AddStatusEffect( "status_burning" );
			ShakeCamera( 50.0f );

			return;
		}

		DashFlameArea.Monitoring = true;

		IdleReset();
		Flags |= PlayerFlags.Dashing;
		DashTime.WaitTime = DashTimer;
		DashTime.Start();
		DashChannel.PitchScale = 1.0f + DashBurnout;
		PlaySound( DashChannel, ResourceCache.DashSfx[ RNJesus.IntRange( 0, ResourceCache.DashSfx.Length - 1 ) ] );
		DashLight.Show();
		DashEffect.Emitting = true;
		DashDirection = Velocity;
		HUD.GetDashOverlay().Show();

		DashBurnout += 0.25f;
		if ( DashTimer >= 0.10f ) {
			DashTimer -= 0.05f;
		}
		DashCooldownTime.WaitTime = 0.80f;
		DashCooldownTime.Start();
	}
	private void OnSlide() {
		if ( IsInputBlocked() || ( Flags & PlayerFlags.Dashing ) != 0 ) {
			return;
		}
		IdleReset();
		Flags |= PlayerFlags.Sliding;
		SlideTime.Start();
		SlideEffect.Emitting = true;

		PlaySound( AudioChannel, ResourceCache.SlideSfx[ RNJesus.IntRange( 0, ResourceCache.SlideSfx.Length - 1 ) ] );

		LegAnimationState = PlayerAnimationState.Sliding;
		TorsoAnimationState = PlayerAnimationState.Sliding;

		LegAnimation.Play( "slide" );
	}
	private void OnUseWeapon() {
		if ( IsInputBlocked() ) {
			return;
		}

		IdleReset();

		int slot = LastUsedArm.Slot;
		if ( slot == WeaponSlot.INVALID ) {
			return; // nothing equipped
		}
		if ( WeaponSlots[ slot ].IsUsed() ) {
			WeaponEntity weapon = WeaponSlots[ slot ].GetWeapon();
			weapon.SetAttackAngle( ArmAngle );
			if ( weapon.IsBladed() && ( Flags & PlayerFlags.UsingMelee ) == 0 ) {
				Flags |= PlayerFlags.UsingMelee;
			}

			EmitSignalUsedWeapon( weapon );
			//			AimRayCast.CollisionMask = (uint)( PhysicsLayer.Player | PhysicsLayer.SpriteEntity | PhysicsLayer.SpecialHitboxes );
			FrameDamage += weapon.Use( weapon.LastUsedMode, out float soundLevel, ( Flags & PlayerFlags.UsingWeapon ) != 0 );
			if ( FrameDamage > 0.0f ) {
				IncreaseBlood( FrameDamage * 0.0001f );
				ComboCounter++;
			}
			Flags |= PlayerFlags.UsingWeapon;
			SetSoundLevel( soundLevel );
		}
	}
	private void OnArmAngleChanged() {
		if ( IsInputBlocked() ) {
			return;
		}

		if ( CurrentMappingContext == ResourceCache.KeyboardInputMappings ) {
			GetArmAngle();
		} else {
			ArmAngle = (float)ArmAngleAction.Get( "value_axis_1d" );
		}
		AimLine.GlobalRotation = ArmAngle;
		AimRayCast.TargetPosition = Godot.Vector2.Right.Rotated( Mathf.RadToDeg( ArmAngle ) ) * AimLine.Points[ 1 ].X;
	}
	private void OnPrevWeapon() {
		if ( IsInputBlocked() ) {
			return;
		}

		int index = CurrentWeapon < 0 ? MAX_WEAPON_SLOTS - 1 : CurrentWeapon - 1;
		while ( index != -1 ) {
			if ( WeaponSlots[ index ].IsUsed() ) {
				break;
			}
			index--;
		}

		if ( index == -1 || !WeaponSlots[ index ].IsUsed() ) {
			index = WeaponSlot.INVALID;
		}

		Arm otherArm;
		if ( LastUsedArm == ArmLeft ) {
			otherArm = ArmRight;
			HandsUsed = Hands.Left;
		} else if ( LastUsedArm == ArmRight ) {
			otherArm = ArmLeft;
			HandsUsed = Hands.Right;
		} else {
			Console.PrintError( "OnNextWeapon: invalid LastUsedArm" );
			LastUsedArm = ArmRight;
			return;
		}

		// adjust arm state
		if ( index != WeaponSlot.INVALID ) {
			WeaponEntity weapon = WeaponSlots[ index ].GetWeapon();
			if ( ( weapon.LastUsedMode & WeaponEntity.Properties.IsTwoHanded ) != 0 ) {
				otherArm.Slot = WeaponSlot.INVALID;
				HandsUsed = Hands.Both;
			}

			EmitSignalSwitchedWeapon( weapon );
			WeaponSlots[ index ].SetMode( WeaponSlots[ index ].GetWeapon().LastUsedMode );
		} else {
			EmitSignalSwitchedWeapon( null );
		}
		PlaySound( MiscChannel, ResourceCache.ChangeWeaponSfx );

		CurrentWeapon = index;
		LastUsedArm.Slot = CurrentWeapon;

		EmitSignalHandsStatusUpdated( HandsUsed );
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

		if ( index == MAX_WEAPON_SLOTS || !WeaponSlots[ index ].IsUsed() ) {
			index = -1;
		}

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

		// adjust arm state
		if ( index != WeaponSlot.INVALID ) {
			WeaponEntity weapon = WeaponSlots[ index ].GetWeapon();
			if ( ( weapon.LastUsedMode & WeaponEntity.Properties.IsTwoHanded ) != 0 ) {
				otherArm.Slot = WeaponSlot.INVALID;
				HandsUsed = Hands.Both;
			}

			EmitSignalSwitchedWeapon( weapon );
			WeaponSlots[ index ].SetMode( WeaponSlots[ index ].GetWeapon().LastUsedMode );
		} else {
			EmitSignalSwitchedWeapon( null );
		}
		PlaySound( MiscChannel, ResourceCache.ChangeWeaponSfx );

		CurrentWeapon = index;
		LastUsedArm.Slot = CurrentWeapon;

		EmitSignalHandsStatusUpdated( HandsUsed );
	}
	private void OnBulletTime() {
		if ( IsInputBlocked() || Rage <= 0.0f ) {
			return;
		}

		IdleReset();
		if ( ( Flags & PlayerFlags.BulletTime ) != 0 ) {
			ExitBulletTime();
		} else {
			PlaySound( MiscChannel, ResourceCache.SlowMoBeginSfx );

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
	private void OnUseBothHands() {
		if ( IsInputBlocked() ) {
			return;
		}
		HandsUsed = Hands.Both;
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

		if ( src.Slot == WeaponSlot.INVALID ) {
			// nothing in the source hand, deny
			return;
		}

		LastUsedArm = dst;

		WeaponEntity srcWeapon = WeaponSlots[ src.Slot ].GetWeapon();
		if ( ( srcWeapon.LastUsedMode & WeaponEntity.Properties.IsTwoHanded ) != 0 && ( srcWeapon.LastUsedMode & WeaponEntity.Properties.IsOneHanded ) == 0 ) {
			// cannot change hands, no one-handing allowed
			return;
		}

		// check if the destination hand has something in it, if true, then swap
		if ( dst.Slot != WeaponSlot.INVALID ) {
			int tmp = dst.Slot;

			dst.Slot = src.Slot;
			src.Slot = tmp;
		} else {
			// if we have nothing in the destination hand, then just clear the source hand
			int tmp = src.Slot;

			src.Slot = WeaponSlot.INVALID;
			dst.Slot = tmp;
		}
		EmitSignalWeaponStatusUpdated( srcWeapon, srcWeapon.LastUsedMode );
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
		case Hands.Both:
			break;
		default:
			Console.PrintError( "SwitchWeaponHand: invalid hand, setting to default of right" );
			HandsUsed = Hands.Right;
			break;
		};
		if ( LastUsedArm.Slot != WeaponSlot.INVALID ) {
			EquipSlot( LastUsedArm.Slot );
		}

		EmitSignalSwitchHandUsed( HandsUsed );
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
			int index = ArmLeft.Slot;
			if ( index == WeaponSlot.INVALID ) {
				return;
			}
			slot = WeaponSlots[ index ];
			break; }
		case Hands.Right: {
			int index = ArmRight.Slot;
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
		WeaponEntity.Properties props = weapon.PropertyBits;
		bool IsOneHanded = ( props & WeaponEntity.Properties.IsOneHanded ) != 0;
		bool IsTwoHanded = ( props & WeaponEntity.Properties.IsTwoHanded ) != 0;
		const WeaponEntity.Properties hands = ~( WeaponEntity.Properties.IsOneHanded | WeaponEntity.Properties.IsTwoHanded );

		for ( int i = 0; i < WeaponModeList.Length; i++ ) {
			WeaponEntity.Properties current = WeaponModeList[ i ];
			if ( mode == current ) {
				// same mode, don't switch
				continue;
			}
			if ( ( props & ( current & hands ) ) != 0 ) {
				// apply some filters
				if ( ( current & WeaponEntity.Properties.IsOneHanded ) != 0 && IsOneHanded ) {
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
		EmitSignalWeaponStatusUpdated( slot.GetWeapon(), slot.GetMode() );
	}

	public void EquipSlot( int nSlot ) {
		CurrentWeapon = nSlot;

		WeaponEntity weapon = WeaponSlots[ nSlot ].GetWeapon();
		if ( weapon != null ) {
			// apply rules of various weapon properties
			if ( ( weapon.LastUsedMode & WeaponEntity.Properties.IsTwoHanded ) != 0 ) {
				ArmLeft.Slot = CurrentWeapon;
				ArmRight.Slot = CurrentWeapon;

				// this will automatically override any other modes
				WeaponSlots[ ArmLeft.Slot ].SetMode( weapon.DefaultMode );
			}

			WeaponSlots[ LastUsedArm.Slot ].SetMode( weapon.PropertyBits );
		} else {
			ArmLeft.Slot = WeaponSlot.INVALID;
			ArmRight.Slot = WeaponSlot.INVALID;
		}

		// update hand data
		LastUsedArm.Slot = CurrentWeapon;

		EmitSignalSwitchedWeapon( weapon );
	}
	private void OnStoppedUsingWeapon() {
		Flags &= ~( PlayerFlags.UsingWeapon | PlayerFlags.UsingMelee );
	}

	private void OnMeleeFinished() {
		ArmLeft.Animations.AnimationFinished -= OnMeleeFinished;
		Flags &= ~PlayerFlags.Parrying;
		BlockInput( false );

		ParryDamageArea.SetDeferred( "monitoring", false );
		ParryArea.SetDeferred( "monitoring", false );
	}
	private void OnMelee() {
		if ( IsInputBlocked() ) {
			return;
		}

		HandsUsed = Hands.Right;

		ParryArea.SetDeferred( "monitoring", true );
		ParryDamageArea.SetDeferred( "monitoring", true );

		// force the player to commit to the parry
		Flags |= PlayerFlags.Parrying;
		BlockInput( true );
		ArmLeft.Animations.SpriteFrames = DefaultLeftArmAnimations;
		ArmLeft.Animations.AnimationFinished += OnMeleeFinished;
		ArmLeft.Animations.CallDeferred( "play", "melee" );
		PlaySound( MiscChannel, ResourceCache.GetSound( "res://sounds/player/melee.wav" ) );
	}

	public void SwitchInputMode( Resource InputContext ) {
		GetNode( "/root/GUIDE" ).Call( "enable_mapping_context", InputContext );

		Console.PrintLine( "Remapping input..." );

		if ( GameConfiguration.GameMode == GameMode.LocalCoop2 ) {
			ConnectGamepadBinds();
		} else {
			ConnectKeyboardBinds();
		}

		if ( InputContext == ResourceCache.KeyboardInputMappings ) {
			MoveAction = ResourceCache.MoveActionKeyboard;
			DashAction = ResourceCache.DashActionKeyboard;
			SlideAction = ResourceCache.SlideActionKeyboard;
			BulletTimeAction = ResourceCache.BulletTimeActionKeyboard;
			PrevWeaponAction = ResourceCache.PrevWeaponActionKeyboard;
			NextWeaponAction = ResourceCache.NextWeaponActionKeyboard;
			SwitchWeaponModeAction = ResourceCache.SwitchWeaponModeActionKeyboard;
			OpenInventoryAction = ResourceCache.OpenInventoryActionKeyboard;
			UseWeaponAction = ResourceCache.UseWeaponActionKeyboard;

			CurrentMappingContext = ResourceCache.KeyboardInputMappings;
		} else {
			MoveAction = ResourceCache.MoveActionGamepad[ InputDevice ];
			DashAction = ResourceCache.DashActionGamepad[ InputDevice ];
			SlideAction = ResourceCache.SlideActionGamepad[ InputDevice ];
			BulletTimeAction = ResourceCache.BulletTimeActionGamepad[ InputDevice ];
			PrevWeaponAction = ResourceCache.PrevWeaponActionGamepad[ InputDevice ];
			NextWeaponAction = ResourceCache.NextWeaponActionGamepad[ InputDevice ];
			SwitchWeaponModeAction = ResourceCache.SwitchWeaponModeActionGamepad[ InputDevice ];
			OpenInventoryAction = ResourceCache.OpenInventoryActionGamepad[ InputDevice ];
			UseWeaponAction = ResourceCache.UseWeaponActionGamepad[ InputDevice ];

			CurrentMappingContext = ResourceCache.GamepadInputMappings;
		}
	}

	public override void _UnhandledInput( InputEvent @event ) {
		base._UnhandledInput( @event );

		switch ( GameConfiguration.GameMode ) {
		case GameMode.SinglePlayer:
		case GameMode.ChallengeMode:
		case GameMode.Online:
		case GameMode.LocalCoop2:
		case GameMode.LocalCoop3:
		case GameMode.LocalCoop4:
			break;
		case GameMode.JohnWick:
			return;
		case GameMode.Multiplayer:
			break;
		};
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
		if ( MiscChannel.Stream == ResourceCache.SlowMoBeginSfx ) {
			// only start lagging audio playback after the slowmo begin finishes
			AudioServer.PlaybackSpeedScale = 0.50f;
		}
	}

	public void ConnectGamepadBinds() {
		ResourceCache.MeleeActionGamepad[ InputDevice ].Connect( "triggered", Callable.From( OnMelee ) );
//		ResourceCache.SwitchWeaponModeActionGamepad[ InputDevice ].Connect( "triggered", Callable.From( SwitchWeaponMode ) );
		ResourceCache.BulletTimeActionGamepad[ InputDevice ].Connect( "triggered", Callable.From( OnBulletTime ) );
		ResourceCache.NextWeaponActionGamepad[ InputDevice ].Connect( "triggered", Callable.From( OnNextWeapon ) );
		ResourceCache.PrevWeaponActionGamepad[ InputDevice ].Connect( "triggered", Callable.From( OnPrevWeapon ) );
		ResourceCache.DashActionGamepad[ InputDevice ].Connect( "triggered", Callable.From( OnDash ) );
		ResourceCache.SlideActionGamepad[ InputDevice ].Connect( "triggered", Callable.From( OnSlide ) );
		ResourceCache.UseWeaponActionGamepad[ InputDevice ].Connect( "triggered", Callable.From( OnUseWeapon ) );
		ResourceCache.UseWeaponActionGamepad[ InputDevice ].Connect( "completed", Callable.From( OnStoppedUsingWeapon ) );
		ResourceCache.OpenInventoryActionGamepad[ InputDevice ].Connect( "triggered", Callable.From( OnToggleInventory ) );
	}
	private void ConnectKeyboardBinds() {
		ResourceCache.UseBothHandsActionsKeyboard.Connect( "triggered", Callable.From( OnUseBothHands ) );
		ResourceCache.MeleeActionKeyboard.Connect( "triggered", Callable.From( OnMelee ) );
		ResourceCache.SwitchWeaponModeActionKeyboard.Connect( "triggered", Callable.From( SwitchWeaponMode ) );
		ResourceCache.BulletTimeActionKeyboard.Connect( "triggered", Callable.From( OnBulletTime ) );
		ResourceCache.NextWeaponActionKeyboard.Connect( "triggered", Callable.From( OnNextWeapon ) );
		ResourceCache.PrevWeaponActionKeyboard.Connect( "triggered", Callable.From( OnPrevWeapon ) );
		ResourceCache.DashActionKeyboard.Connect( "triggered", Callable.From( OnDash ) );
		ResourceCache.SlideActionKeyboard.Connect( "triggered", Callable.From( OnSlide ) );
		ResourceCache.UseWeaponActionKeyboard.Connect( "triggered", Callable.From( OnUseWeapon ) );
		ResourceCache.UseWeaponActionKeyboard.Connect( "completed", Callable.From( OnStoppedUsingWeapon ) );
		ResourceCache.OpenInventoryActionKeyboard.Connect( "triggered", Callable.From( OnToggleInventory ) );
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
		if ( ( Flags & PlayerFlags.BulletTime ) == 0 ) {
			return;
		}
		Flags &= ~PlayerFlags.BulletTime;
		HUD.GetReflexOverlay().Hide();
		Engine.TimeScale = 1.0f;
		AudioServer.PlaybackSpeedScale = 1.0f;
		PlaySound( MiscChannel, ResourceCache.SlowMoEndSfx );
	}

	private void CmdSuicide() {
		Health = 0.0f;
		OnDeath( this );
	}
	private void CmdTeleport( string locationId ) {
		Console.PrintLine( string.Format( "Teleporing player to {0}...", locationId ) );

		Godot.Collections.Array<Node> checkpoints = GetTree().GetNodesInGroup( "Checkpoints" );
		for ( int i = 0; i < checkpoints.Count; i++ ) {
			if ( checkpoints[ i ] is Checkpoint checkpoint && checkpoint != null && checkpoint.Name == locationId ) {
				GlobalPosition = checkpoint.GlobalPosition;
				return;
			}
		}
		Console.PrintWarning( string.Format( "No such checkpoint \"{0}\"", locationId ) );
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

		ScreenSize = DisplayServer.WindowGetSize();

		LevelData.Instance.PlayerRespawn += Respawn;

		// don't allow keybind input when we're in the console
		Console.Control.VisibilityChanged += () => {
			BlockInput( Console.Control.Visible );
		};

		StartingPosition = GlobalPosition;

		AimLine = GetNode<Line2D>( "AimAssist/AimLine" );
		AimRayCast = GetNode<RayCast2D>( "AimAssist/AimLine/RayCast2D" );

		AimLine.Points[ 1 ].X = AimLine.Points[ 0 ].X * ( ScreenSize.X / 2.0f );
		AimRayCast.TargetPosition = GlobalPosition * PunchRange;

		Input.SetCustomMouseCursor( ResourceCache.GetTexture( "res://textures/hud/crosshairs/crosshairi.tga" ), Input.CursorShape.Arrow );

		Health = 100.0f;
		Rage = 60.0f;

		if ( GameConfiguration.GameMode != GameMode.LocalCoop2 ) {
			ResourceCache.LoadKeyboardBinds();
			ResourceCache.LoadGamepadBinds();
		}
		LoadSfx();

		Resource screenshot = ResourceLoader.Load( "res://resources/binds/actions/keyboard/screenshot.tres" );
		screenshot.Connect( "triggered", Callable.From( () => {
			HUD.Hide();

			RenderingServer.ForceDraw();
			DirAccess.MakeDirRecursiveAbsolute( "user://screenshots" );
			GetViewport().GetTexture().GetImage().SavePng(
				string.Format( "user://screenshots/screenshot{0}.png", DateTime.Now )
			);

			HUD.Show();
		} ) );

		SetProcessUnhandledInput( false );

		if ( !Renown.Constants.StartingQuestPath.IsEmpty && GetTree().CurrentScene.Name == "World" ) {
			QuestState.StartContract( ResourceLoader.Load( Renown.Constants.StartingQuestPath ), Renown.Constants.StartingQuestFlags, Renown.Constants.StartingQuestState );
		}

		ResourceCache.LoadBinds();

		//
		// initialize input context
		//
		SwitchToGamepad = ResourceLoader.Load( "res://resources/binds/actions/keyboard/switch_to_gamepad.tres" );
		SwitchToKeyboard = ResourceLoader.Load( "res://resources/binds/actions/gamepad/switch_to_keyboard.tres" );

		Viewpoint = GetNode<Camera2D>( "Camera2D" );

		AudioChannel = GetNode<AudioStreamPlayer2D>( "AudioChannel" );
		AudioChannel.VolumeDb = SettingsData.GetEffectsVolumeLinear();

		MiscChannel = GetNode<AudioStreamPlayer2D>( "MiscChannel" );
		MiscChannel.Connect( "finished", Callable.From( OnSlowMoSfxFinished ) );
		MiscChannel.VolumeDb = SettingsData.GetEffectsVolumeLinear();

		DashFlameArea = GetNode<Area2D>( "Animations/DashEffect/FlameArea" );
		DashFlameArea.Monitoring = false;
		DashFlameArea.Connect( "body_shape_entered", Callable.From<Rid, Node2D, int, int>( OnFlameAreaBodyShape2DEntered ) );

		DashChannel = GetNode<AudioStreamPlayer2D>( "DashChannel" );
		DashChannel.VolumeDb = SettingsData.GetEffectsVolumeLinear();

		CollisionShape2D SoundBounds = GetNode<CollisionShape2D>( "SoundArea/CollisionShape2D" );
		SoundArea = SoundBounds.Shape as CircleShape2D;

		ParryArea = GetNode<Area2D>( "AimAssist/AimLine/ParryArea" );
		ParryArea.SetMeta( "Owner", this );

		ParryBox = ParryArea.GetNode<CollisionShape2D>( "CollisionShape2D" );

		ParryDamageArea = GetNode<Area2D>( "AimAssist/AimLine/ParryDamageArea" );
		ParryDamageBox = GetNode<CollisionShape2D>( "AimAssist/AimLine/ParryDamageArea/CollisionShape2D" );

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

		DefaultLeftArmAnimations = ArmLeft.Animations.SpriteFrames;

		DashTime = GetNode<Timer>( "Timers/DashTime" );
		DashTimer = (float)DashTime.WaitTime;
		DashTime.Connect( "timeout", Callable.From( OnDashTimeTimeout ) );

		DashBurnoutCooldownTimer = GetNode<Timer>( "Timers/DashBurnoutCooldownTimer" );
		DashBurnoutCooldownTimer.Connect( "timeout", Callable.From( OnDashBurnoutCooldownTimerTimeout ) );

		IdleTimer = GetNode<Timer>( "IdleAnimationTimer" );
		IdleTimer.Connect( "timeout", Callable.From( OnIdleAnimationTimerTimeout ) );

		IdleAnimation = GetNode<AnimatedSprite2D>( "Animations/Idle" );
		IdleAnimation.Connect( "animation_finished", Callable.From( OnIdleAnimationAnimationFinished ) );

		LegAnimation = GetNode<AnimatedSprite2D>( "Animations/Legs" );
		LegAnimation.Connect( "animation_looped", Callable.From( OnLegsAnimationLooped ) );

		TorsoAnimation = GetNode<AnimatedSprite2D>( "Animations/Torso" );
		Animations = GetNode( "Animations" );

		WalkEffect = GetNode<GpuParticles2D>( "Animations/DustPuff" );
		SlideEffect = GetNode<GpuParticles2D>( "Animations/SlidePuff" );
		DashEffect = GetNode<GpuParticles2D>( "Animations/DashEffect" );
		DashLight = GetNode<PointLight2D>( "Animations/DashEffect/PointLight2D" );

		SlideTime = GetNode<Timer>( "Timers/SlideTime" );
		SlideTime.Connect( "timeout", Callable.From( OnSlideTimeout ) );
		DashCooldownTime = GetNode<Timer>( "Timers/DashCooldownTime" );

		CheckpointDrinkTimer = new Timer();
		CheckpointDrinkTimer.Name = "CheckpointDrinkTimer";
		CheckpointDrinkTimer.WaitTime = 10.5f;
		CheckpointDrinkTimer.ProcessMode = ProcessModeEnum.Disabled;
		CheckpointDrinkTimer.Connect( "timeout", Callable.From( () => {
			IdleAnimation.CallDeferred( "play", "checkpoint_drink" );
			TorsoAnimationState = PlayerAnimationState.CheckpointDrinking;
			LegAnimationState = PlayerAnimationState.CheckpointDrinking;
			LeftArmAnimationState = PlayerAnimationState.CheckpointDrinking;
			RightArmAnimationState = PlayerAnimationState.CheckpointDrinking;
		} ) );
		AddChild( CheckpointDrinkTimer );

		for ( int i = 0; i < MAX_WEAPON_SLOTS; i++ ) {
			WeaponSlots[ i ] = new WeaponSlot();
			WeaponSlots[ i ].SetIndex( i );
		}

		LastUsedArm = ArmRight;

		GetTree().Root.SizeChanged += OnScreenSizeChanged;

		//		RenderingServer.FramePostDraw += () => OnViewportFramePostDraw();
		//		RenderingServer.FramePreDraw += () => OnViewportFramePreDraw();

		Console.AddCommand( "suicide", Callable.From( CmdSuicide ), null, 0, "it's in the name" );
		Console.AddCommand( "teleport", Callable.From<string>( CmdTeleport ), new[] { "checkpoint" }, 1, "teleports the player to the specified location" );

		if ( SettingsData.GetNetworkingEnabled() && GameConfiguration.GameMode != GameMode.ChallengeMode ) {
			SteamLobby.Instance.AddPlayer( SteamUser.GetSteamID(),
				new SteamLobby.NetworkNode( this, SendPacket, ReceivePacket ) );
		}

		ProcessMode = ProcessModeEnum.Pausable;

		if ( SettingsData.GetShowBlood() ) {
			BloodDropTimer = new Timer();
			BloodDropTimer.Name = "BloodDropTimer";
			BloodDropTimer.WaitTime = 30.0f;
			BloodDropTimer.Connect( "timeout", Callable.From( OnBloodDropTimerTimeout ) );
			AddChild( BloodDropTimer );

			BloodMaterial = ResourceLoader.Load<ShaderMaterial>( "res://resources/materials/covered_in_blood.tres" );
		} else {
			TorsoAnimation.Material = null;
			LegAnimation.Material = null;
			ArmLeft.Animations.Material = null;
			ArmRight.Animations.Material = null;
		}

		if ( ArchiveSystem.Instance.IsLoaded() ) {
			Load();
		}

		Input.JoyConnectionChanged += ( device, connected ) => { if ( connected ) { SwitchInputMode( ResourceCache.GamepadInputMappings ); } };
	}

	public override void _PhysicsProcess( double delta ) {
		base._PhysicsProcess( delta );

		GodotObject collision = AimRayCast.GetCollider();
		if ( collision != null && collision.HasMeta( "Faction" ) && (Faction)collision.GetMeta( "Faction" ) != Faction ) {
			AimLine.DefaultColor = AimingAtTarget;
		} else if ( collision is Hitbox hitbox && hitbox != null && ( (Node2D)hitbox.GetMeta( "Owner" ) as Entity ).GetFaction() != Faction ) {
			AimLine.DefaultColor = AimingAtTarget;
		} else {
			AimLine.DefaultColor = AimingAtNull;
		}

		// cool down the jet engine if applicable
		if ( DashBurnout > 0.0f && DashCooldownTime.TimeLeft == 0.0f ) {
			DashBurnout -= 0.10f;
			DashTimer += 0.05f;
			if ( DashBurnout < 0.0f ) {
				DashBurnout = 0.0f;
			}
		}
		CheckStatus( (float)delta );

		if ( ( Flags & PlayerFlags.BlockedInput ) != 0 ) {
			return;
		}

		float speed = MAX_SPEED;
		// encumbured
		if ( TotalInventoryWeight >= MaximumInventoryWeight * 0.85f ) {

		}

		if ( ( Flags & PlayerFlags.Dashing ) != 0 ) {
			speed += 1800;
		}
		if ( ( Flags & PlayerFlags.Sliding ) != 0 ) {
			speed += 400;
			LeftArmAnimationState = PlayerAnimationState.Sliding;
		}

		Godot.Vector2 velocity = Velocity;

		InputVelocity = (Godot.Vector2)MoveAction.Get( "value_axis_2d" );
		if ( InputVelocity != Godot.Vector2.Zero ) {
			velocity = velocity.MoveToward( InputVelocity * speed, (float)delta * ACCEL );
			TorsoAnimationState = PlayerAnimationState.Running;
			LegAnimationState = PlayerAnimationState.Running;
		} else {
			velocity = velocity.MoveToward( Godot.Vector2.Zero, (float)delta * FRICTION );
			TorsoAnimationState = PlayerAnimationState.Idle;
			LegAnimationState = PlayerAnimationState.Idle;
		}

		if ( velocity != Godot.Vector2.Zero ) {
			IdleReset();
		}

		Velocity = velocity;
		MoveAndSlide();
	}

	[MethodImpl( MethodImplOptions.AggressiveOptimization )]
	private void CalcArmAnimation( Arm arm, out PlayerAnimationState animState ) {
		arm.Animations.GlobalRotation = ArmAngle;
		arm.Animations.FlipV = TorsoAnimation.FlipH;
		arm.Animations.SpriteFrames = arm.GetAnimationSet();

		if ( arm.Slot == WeaponSlot.INVALID ) {
			arm.Animations.Play( InputVelocity != Godot.Vector2.Zero ? "run" : "idle" );
			animState = InputVelocity != Godot.Vector2.Zero ? PlayerAnimationState.Running :
				PlayerAnimationState.Idle;
		} else {
			WeaponEntity weapon = WeaponSlots[ arm.Slot ].GetWeapon();
			string animationName;
			switch ( weapon.CurrentState ) {
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

			if ( ( weapon.LastUsedMode & WeaponEntity.Properties.IsOneHanded ) != 0 ) {
				if ( ( arm == ArmLeft && !arm.Animations.FlipH )
					|| ( arm == ArmRight && arm.Animations.FlipH ) ) {
					animationName += "_flip";
				}
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
			// the more attacks we chain together without taking a hit, the more rage we get
			Rage += FrameDamage * ComboCounter * delta;
			FrameDamage = 0.0f;
			Flags |= PlayerFlags.UsedMana;
			HUD.GetRageBar().Rage = Rage;
		}
		FrameDamage = 0.0f;
		if ( Health < 100.0f && Rage > 0.0f ) {
			Health += 3.0f * delta;
			Rage -= 10.0f * delta;
			// mana conversion ratio to health is extremely inefficient

			Flags |= PlayerFlags.UsedMana;
			HUD.GetHealthBar().SetHealth( Health );
			HUD.GetRageBar().Rage = Rage;
		}
		if ( ( Flags & PlayerFlags.BulletTime ) != 0 ) {
			Rage -= 20.0f * delta;
			HUD.GetRageBar().Rage = Rage;
		}
		if ( Rage > 100.0f ) {
			Rage = 100.0f;
		} else if ( Rage < 0.0f ) {
			Rage = 0.0f;
			ExitBulletTime();
		}
	}
	[MethodImpl( MethodImplOptions.AggressiveOptimization )]
	public override void _Process( double delta ) {
		base._Process( delta );

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

		if ( Health < 30.0f ) {
			ShakeStrength = 5.0f;
		}
		if ( Health < 15.0f ) {
			ShakeStrength = 10.0f;
		}

		if ( ShakeStrength > 0.0f ) {
			ShakeStrength = Mathf.Lerp( ShakeStrength, 0.0f, ShakeFade * (float)delta );
			Viewpoint.Offset = new Vector2( RNJesus.FloatRange( -ShakeStrength, ShakeStrength ), RNJesus.FloatRange( -ShakeStrength, ShakeStrength ) );
		}

		if ( SoundLevel > 0.1f ) {
			SoundLevel -= 1024.0f * (float)delta;
			if ( SoundLevel < 0.0f ) {
				SoundLevel = 0.1f;
			}
		}
		SoundArea.Radius = SoundLevel;

		if ( ( Flags & PlayerFlags.Resting ) != 0 ) {
			return;
		}

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
				( front, back ) = ( back, front );
			} else {
				front = LastUsedArm;
			}
		}
		back.Animations.Visible = HandsUsed != Hands.Both;

		front.Animations.Show();

		Animations.MoveChild( back.Animations, 0 );
		Animations.MoveChild( front.Animations, 3 );

		if ( ( Flags & PlayerFlags.Parrying ) == 0 ) {
			CalcArmAnimation( ArmLeft, out LeftArmAnimationState );
			CalcArmAnimation( ArmRight, out RightArmAnimationState );
		}
	}

	private void OnWeaponModeChanged( WeaponEntity source, WeaponEntity.Properties useMode ) => EmitSignalWeaponStatusUpdated( source, useMode );

	public void PickupAmmo( in AmmoEntity ammo ) {
		AmmoStack stack = null;
		bool found = false;

		EmitSignalAmmoPickedUp( ammo );

		foreach ( var it in AmmoStacks ) {
			if ( (string)ammo.Data.Get( "id" ) == (string)it.Value.AmmoType.Data.Get( "id" ) ) {
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

		PlaySound( MiscChannel, ammo.GetPickupSound() );

		for ( int i = 0; i < MAX_WEAPON_SLOTS; i++ ) {
			WeaponSlot slot = WeaponSlots[ i ];
			if ( slot.IsUsed() && (int)slot.GetWeapon().Ammunition
				== (int)( (Godot.Collections.Dictionary)ammo.Data.Get( "properties" ) )[ "type" ] )
			{
				slot.GetWeapon().SetReserve( stack );
				slot.GetWeapon().SetAmmo( ammo );
				if ( LastUsedArm.Slot == i ) {
					EmitSignalWeaponStatusUpdated( WeaponSlots[ i ].GetWeapon(), WeaponSlots[ i ].GetMode() );
				}
			}
		}
	}
	public override void PickupWeapon( in WeaponEntity weapon ) {
		int index = WeaponSlot.INVALID;

		EmitSignalWeaponPickedUp( weapon );

		Godot.Collections.Array<Resource> categories = (Godot.Collections.Array<Resource>)weapon.Data.Get( "categories" );

		for ( int i = 0; i < categories.Count; i++ ) {
			switch ( (string)categories[ i ].Get( "id" ) ) {
			case "WEAPON_CATEGORY_PRIMARY":
				index = (int)WeaponSlotIndex.Primary;
				break;
			case "WEAPON_CATEGORY_HEAVY_PRIMARY":
				index = (int)WeaponSlotIndex.HeavyPrimary;
				break;
			case "WEAPON_CATEGORY_SIDEARM":
				index = (int)WeaponSlotIndex.Sidearm;
				break;
			case "WEAPON_CATEGORY_HEAVY_SIDEARM":
				index = (int)WeaponSlotIndex.HeavySidearm;
				break;
			default:
				break;
			};
		}
		if ( index == WeaponSlot.INVALID ) {
			Console.PrintError( string.Format( "Player.PickupWeapon: weapon {0} has invalid equipment category", (string)weapon.Data.Get( "id" ) ) );
		} else if ( !WeaponSlots[ index ].IsUsed() ) {
			WeaponSlots[ index ].SetWeapon( weapon );
			CurrentWeapon = index;
		}

		WeaponsStack.Add( weapon.GetHashCode(), weapon );
		TotalInventoryWeight += weapon.Weight;

		TorsoAnimation.FlipH = false;
		LegAnimation.FlipH = false;

		weapon.Connect( "ModeChanged", Callable.From<WeaponEntity, WeaponEntity.Properties>( OnWeaponModeChanged ) );
		weapon.OverrideRayCast( AimRayCast );

		AmmoStack stack = null;
		foreach ( var ammo in AmmoStacks ) {
			if ( (int)( (Godot.Collections.Dictionary)ammo.Value.AmmoType.Data.Get( "properties" ) )[ "type" ] ==
				(int)weapon.Ammunition ) {
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
			if ( ( weapon.DefaultMode & WeaponEntity.Properties.IsTwoHanded ) != 0 ) {
				HandsUsed = Hands.Both;

				ArmLeft.Slot = WeaponSlot.INVALID;

				LastUsedArm = ArmRight;
				LastUsedArm.Slot = index;

				// this will automatically overwrite any other modes
				WeaponSlots[ LastUsedArm.Slot ].SetMode( weapon.DefaultMode );
			} else if ( ( weapon.DefaultMode & WeaponEntity.Properties.IsOneHanded ) != 0 ) {
				LastUsedArm ??= ArmRight;
				LastUsedArm.Slot = CurrentWeapon;

				if ( LastUsedArm == ArmRight ) {
					HandsUsed = Hands.Right;
				} else if ( LastUsedArm == ArmLeft ) {
					HandsUsed = Hands.Left;
				}

				WeaponSlots[ LastUsedArm.Slot ].SetMode( weapon.DefaultMode );
			}

			// update the hand data
			LastUsedArm.Slot = CurrentWeapon;
			WeaponSlots[ LastUsedArm.Slot ].SetMode( weapon.PropertyBits );
			weapon.SetUseMode( weapon.DefaultMode );

			EmitSignalSwitchedWeapon( weapon );
			EmitSignalHandsStatusUpdated( HandsUsed );
		}
	}
};