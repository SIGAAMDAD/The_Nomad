using Godot;
using System;
using PlayerSystem;
using PlayerSystem.Perks;
using PlayerSystem.Runes;
using PlayerSystem.Totems;
using System.Collections.Generic;
using Steamworks;
using System.Runtime.CompilerServices;
using Renown;
using Renown.World;
using System.Diagnostics;
using DialogueManagerRuntime;
using PlayerSystem.ArmAttachments;
using PlayerSystem.Upgrades;
using System.Threading.Tasks;

public enum WeaponSlotIndex : int {
	Primary,
	HeavyPrimary,
	Sidearm,
	HeavySidearm,
	Count
};

// TODO: dash i-frames duration and speed should decrease if we spam it

public partial class Player : Entity {
	public enum Hands : byte {
		Left,
		Right,
		Both
	};

	public enum PlayerFlags : uint {
		Sliding = 0x00000001,
		Crouching = 0x00000002,
		BulletTime = 0x00000004,
		Dashing = 0x00000008,
		DemonRage = 0x00000010,
		UsedMana = 0x00000020,
		DemonSight = 0x00000040,
		OnHorse = 0x00000080,
		IdleAnimation = 0x00000100,
		Checkpoint = 0x00000200,
		BlockedInput = 0x00000400,
		UsingWeapon = 0x00000800,
		Inventory = 0x00001000,
		Resting = 0x00002000,
		UsingMelee = 0x00004000,
		LightParrying = 0x00008000,
		HeavyParrying = 0x00010000,
		Encumbured = 0x00020000,
		Emoting = 0x00040000,
		Sober = 0x00080000,
		Berserker = 0x00100000,
	};

	public enum AnimationState : byte {
		Idle,
		Move,
		Melee
	};

	private static readonly WeaponEntity.Properties[] WeaponModeList = [
		WeaponEntity.Properties.IsOneHanded | WeaponEntity.Properties.IsBladed,
		WeaponEntity.Properties.IsOneHanded | WeaponEntity.Properties.IsBlunt,
		WeaponEntity.Properties.IsOneHanded | WeaponEntity.Properties.IsFirearm,

		WeaponEntity.Properties.IsTwoHanded | WeaponEntity.Properties.IsBladed,
		WeaponEntity.Properties.IsTwoHanded | WeaponEntity.Properties.IsBlunt,
		WeaponEntity.Properties.IsTwoHanded | WeaponEntity.Properties.IsFirearm
	];

	public static readonly float PunchRange = 40.0f;
	public static readonly int MAX_WEAPON_SLOTS = 4;
	public static readonly Color AimingAtTarget = new Color( 1.0f, 0.0f, 0.0f, 1.0f );
	public static readonly Color AimingAtNull = new Color( 0.5f, 0.5f, 0.0f, 1.0f );

	//
	// constants that can be modified by runes, perks, etc.
	//
	public static readonly float BaseFastTravelRageCost = 20.0f;
	public static readonly float BaseFastTravelSanityCost = 0.0f;
	public static readonly float BaseEnemyDetectionSpeed = 1.0f;

	private static readonly float DirectionalInfluence = 0.7f;

	/// <summary>
	/// The maximum amount of weight the player is currently allowed to carry
	/// </summary>
	public static readonly int MaximumRuneSlots = 5;
	public static readonly int MaximumQuickAccessSlots = 4;

	public static readonly int MAX_RUNES = 5;
	public static readonly int MAX_PERKS = 1;
	public static readonly float ACCEL = 500.0f;
	public static readonly float FRICTION = 1000.0f;
	public static readonly float MAX_SPEED = 440.0f;
	public static readonly float JUMP_VELOCITY = -400.0f;

	public static Godot.Vector2I ScreenSize = Godot.Vector2I.Zero;

	//
	// networking
	//
	private struct NetworkState {
		public float LastNetworkAimAngle;
		public float LastNetworkBloodAmount;
		public uint LastNetworkFlags;
		public WeaponEntity.Properties LastNetworkUseMode;
		public string LastNetworkWeaponID;

		public NetworkState() {
			LastNetworkAimAngle = 0.0f;
			LastNetworkBloodAmount = 0.0f;
			LastNetworkFlags = 0;
			LastNetworkUseMode = WeaponEntity.Properties.None;
			LastNetworkWeaponID = "";
		}
	};

	private NetworkState LastSyncState;

	public const float MaximumInventoryWeight = 500.0f;
	private float Money = 0.0f;

	private Dictionary<TraitType, float> Traits = null;
	private HashSet<RenownValue> Relations = null;
	private HashSet<RenownValue> Debts = null;

	private BladeAttackType BladeAttackType;

	[Export]
	private Node Inventory;
	[Export]
	private Arm ArmLeft;
	[Export]
	private Arm ArmRight;
	[Export]
	private CanvasLayer HUD;
	[Export]
	private Checkpoint StartingCheckpoint;
	[Export]
	private ArmAttachment ArmAttachment;

	[ExportCategory( "Start" )]
	[Export]
	private PlayerFlags Flags = 0;

	[ExportCategory( "Camera Shake" )]
	[Export]
	private float RandomStrength = 36.0f;
	[Export]
	private float ShakeFade = 0.5f;

	private Area2D ParryArea;
	private CollisionShape2D ParryBox;
	private Area2D ParryDamageArea;
	private CollisionShape2D ParryDamageBox;

	/// <summary>
	/// The current total weight of inventory items
	/// </summary>
	public float TotalInventoryWeight { get; private set; } = 0.0f;

	private Resource CurrentMappingContext;

	private Resource SwitchToKeyboard;
	private Resource SwitchToGamepad;

	public Resource MoveAction;
	public Resource DashAction { get; private set; }
	public Resource SlideAction { get; private set; }
	public Resource MeleeAction { get; private set; }
	public Resource UseWeaponAction { get; private set; }
	public Resource SwitchWeaponModeAction { get; private set; }
	public Resource NextWeaponAction { get; private set; }
	public Resource PrevWeaponAction { get; private set; }
	public Resource OpenInventoryAction { get; private set; }
	public Resource BulletTimeAction { get; private set; }
	public Resource ArmAngleAction { get; private set; }
	public Resource UseBothHandsAction { get; private set; }
	public Resource AimAngleAction { get; private set; }
	public Resource InteractAction { get; private set; }

	private GpuParticles2D WalkEffect;
	private GpuParticles2D SlideEffect;

	private GpuParticles2D DashEffect;
	private PointLight2D DashLight;

	private Godot.Vector2 PhysicsPosition = Godot.Vector2.Zero;

	private Node Animations;
	private SpriteFrames DefaultLeftArmAnimations;
	private Sprite2D HeadAnimation;
	public AnimatedSprite2D TorsoAnimation { get; private set; }
	public AnimatedSprite2D LegAnimation { get; private set; }
	public AnimatedSprite2D IdleAnimation { get; private set; }

	private float WindUpDuration = 0.05f;
	private float WindUpProgress = 0.0f;
	private float IdleTime = 0.0f;
	private float NextShiftTime = 0.0f;

	private Node2D Shadows;
	private AnimatedSprite2D LeftArmShadowAnimation;
	private AnimatedSprite2D RightArmShadowAnimation;
	private AnimatedSprite2D TorsoShadowAnimation;
	private AnimatedSprite2D LegShadowAnimation;

	private Timer IdleTimer;
	private Timer CheckpointDrinkTimer;

	private Timer HealTimer;

	private Timer SlideTime;

	public GroundMaterialType GroundType {
		get;
		private set;
	}

	// how much blood we're covered in
	private float BloodAmount = 0.0f;
	private Timer BloodDropTimer;
	private ShaderMaterial BloodMaterial;

	private static float ShakeStrength = 0.0f;
	private static Godot.Vector2 JoltDirection = Godot.Vector2.Zero;

	private static bool TutorialCompleted = false;

	private Texture2D CurrentEmote;

	public WeaponSlot[] WeaponSlots { get; private set; } = new WeaponSlot[ MAX_WEAPON_SLOTS ];
	public int CurrentWeapon { get; private set; } = WeaponSlot.INVALID;

	private Camera2D Viewpoint;

	public PlayerStat<float> MaxHealth;
	public PlayerStat<float> MaxSanity;
	public PlayerStat<float> MaxRage;
	public PlayerStat<float> Speed;

	//
	// multiplayer data
	//
	public Multiplayer.PlayerData.MultiplayerMetadata MultiplayerData;

	//
	// aim reticle
	//
	private Line2D AimLine = null;

	private bool NetworkNeedsSync = false;
	private PlayerAnimationState LeftArmAnimationState;
	private PlayerAnimationState RightArmAnimationState;
	private PlayerAnimationState TorsoAnimationState;
	private PlayerAnimationState LegAnimationState;

	private AudioStreamPlayer2D AudioChannel;
	private AudioStreamPlayer2D DashChannel;
	private AudioStreamPlayer2D MiscChannel;
	private AudioStreamPlayer2D VoiceChannel;
	private AudioStreamPlayer2D WalkChannel;

	//
	// for the phantoms
	//
	private AudioStreamPlayer WhisperChannel;
	private Timer WhisperTimer;

	private FootSteps FootSteps;

	private NetworkSyncObject SyncObject;

	private TileMapFloor Floor;

	//
	// Inventory Management
	//
	private Godot.Collections.Dictionary<int, WeaponEntity> WeaponsStack = new Godot.Collections.Dictionary<int, WeaponEntity>();
	private Godot.Collections.Dictionary<int, ConsumableStack> ConsumableStacks = new Godot.Collections.Dictionary<int, ConsumableStack>();
	private Godot.Collections.Dictionary<int, AmmoStack> AmmoStacks = new Godot.Collections.Dictionary<int, AmmoStack>();
	private HashSet<Perk> UnlockedBoons;
	private HashSet<Rune> UnlockedRunes;
	public Totem Totem { get; private set; } = null;
	private Perk PerkSlot = null;
	private Rune[] RuneSlots = new Rune[ MaximumRuneSlots ];
	private ConsumableStack[] QuickAccessSlots = new ConsumableStack[ MaximumQuickAccessSlots ];
	private Dictionary<GodotObject, int> Storage = new Dictionary<GodotObject, int>();
	private Godot.Collections.Dictionary<string, string> JournalCache;
	private HashSet<WorldArea> DiscoveredAreas;

	private float Rage = 60.0f;
	private float Sanity = 60.0f;

	private DashKit DashKit;

	private Hands HandsUsed = Hands.Right;
	private Arm LastUsedArm;
	private float ArmAngle = 0.0f;
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
	private static Action<int> DialogueCallback;

	private Dictionary<string, object> AchievementData;

	private Resource CurrentQuest;

	private Node2D Waypoint;

	[Signal]
	public delegate void JoinFactionEventHandler( Faction faction, Entity entity );
	[Signal]
	public delegate void LeaveFactionEventHandler( Faction faction, Entity entity );
	[Signal]
	public delegate void FactionPromotionEventHandler( Faction faction, Entity entity );
	[Signal]
	public delegate void FactionDemotionEventHandler( Faction faction, Entity entity );
	[Signal]
	public delegate void GainMoneyEventHandler( Node entity, float nAmount );
	[Signal]
	public delegate void LoseMoneyEventHandler( Node entity, float nAmount );
//	[Signal]
//	public delegate void CommitWarCrimeEventHandler( Entity entity, WarCrimeType nType );
	[Signal]
	public delegate void StartContractEventHandler( Contract contract, Entity entity );
	[Signal]
	public delegate void CompleteContractEventHandler( Contract contract, Entity entity );
	[Signal]
	public delegate void FailedContractEventHandler( Contract contract, Entity entity );
	[Signal]
	public delegate void CanceledContractEventHandler( Contract contract, Entity entity );
	[Signal]
	public delegate void DecreaseTraitScoreEventHandler( Node self, TraitType nTrait, float nScore );
	[Signal]
	public delegate void IncreaseTraitScoreEventHandler( Node self, TraitType nTrait, float nScore );
	[Signal]
	public delegate void MeetEntityEventHandler( Entity other, Node self );
	[Signal]
	public delegate void MeetFactionEventHandler( Faction faction, Node self );
	[Signal]
	public delegate void IncreaseRelationEventHandler( Node other, Node self, float nAmount );
	[Signal]
	public delegate void DecreaseRelationEventHandler( Node other, Node self, float nAmount );
	[Signal]
	public delegate void IncreaseRenownEventHandler( Node self, int nAmount );

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
	[Signal]
	public delegate void InputMappingContextChangedEventHandler();
	[Signal]
	public delegate void InteractionEventHandler();

	//
	// HUD related signals
	//
	[Signal]
	public delegate void DashBurnoutChangedEventHandler( float nDashBurnout );
	[Signal]
	public delegate void RageChangedEventHandler( float nRage );
	[Signal]
	public delegate void HealthChangedEventHandler( float nHealth );
	[Signal]
	public delegate void StatusEffectTriggeredEventHandler( string effectName, StatusEffect effect );
	[Signal]
	public delegate void ParrySuccessEventHandler();
	[Signal]
	public delegate void HideInteractionEventHandler();
	[Signal]
	public delegate void ShowInteractionEventHandler( InteractionItem item );
	[Signal]
	public delegate void DashStartEventHandler();
	[Signal]
	public delegate void DashEndEventHandler();
	[Signal]
	public delegate void BulletTimeStartEventHandler();
	[Signal]
	public delegate void BulletTimeEndEventHandler();

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public virtual float GetMoney() => Money;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public virtual void DecreaseMoney( float nAmount ) {
		Money -= nAmount;
		EmitSignalLoseMoney( this, nAmount );
	}
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public virtual void IncreaseMoney( float nAmount ) {
		Money += nAmount;
		EmitSignalGainMoney( this, nAmount );
	}

	//	public int GetWarCrimeCount() => WarCrimeCount;
	//	public virtual void CommitWarCrime( WarCrimeType nType ) {
	//		EmitSignal( "CommitWarCrime", this, nType );
	//		WarCrimeCount++;
	//	}

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public Dictionary<TraitType, float> GetTraits() => Traits;

	public void SetFaction( Faction faction ) {
		// sanity
		if ( Faction == faction ) {
			Console.PrintWarning( "Player.SetFaction: same faction" );
			return;
		}
		if ( faction != null ) {
			EmitSignalJoinFaction( faction, this );
		} else {
			EmitSignalLeaveFaction( Faction, this );
		}
		Faction = faction;
	}

	public void DecreaseFactionImportance( int nAmount ) {
		// sanity
		if ( Faction == null ) {
			Console.PrintError( "Player.DecreaseFactionImportance: no faction" );
			return;
		}
		FactionImportance -= nAmount;
		EmitSignalFactionDemotion( Faction, this );
	}
	public void IncreaseFactionImportance( int nAmount ) {
		// sanity
		if ( Faction == null ) {
			Console.PrintError( "Player.IncreaseFactionImportance: no faction" );
			return;
		}
		FactionImportance += nAmount;
		EmitSignalFactionPromotion( Faction, this );
	}

	public override int GetRenownScore() => RenownScore;
	public void AddRenown( int nAmount ) {
		RenownScore += nAmount;
		EmitSignalIncreaseRenown( this, nAmount );
	}

	/// <summary>
	/// returns true if the entity has the given trait
	/// </summary>
	public bool HasTrait( TraitType trait ) {
		foreach ( var it in Traits ) {
			if ( it.Key == trait ) {
				return true;
			}
		}
		return false;
	}
	public float GetTraitScore( TraitType type ) {
		if ( Traits.TryGetValue( type, out float value ) ) {
			return value;
		}
		Console.PrintWarning( string.Format( "Player.GetTraitScore: invalid trait type {0}!", type ) );
		return 0.0f;
	}
	public void ChangeTraitScore( TraitType type, float nDelta ) {
		if ( nDelta < 0.0f ) {
			EmitSignalDecreaseTraitScore( this, type, nDelta );
		} else {
			EmitSignalIncreaseTraitScore( this, type, nDelta );
		}
		Traits[ type ] += nDelta;
	}

	public virtual void Meet( Renown.Object other ) {
		Relations.Add( new RenownValue( other ) );
		if ( other is Entity entity && entity != null ) {
			EmitSignalMeetEntity( entity, this );
		} else {
			if ( other is Faction faction && faction != null ) {
				EmitSignalMeetFaction( faction, this );
				return;
			}
			Console.PrintError( "Entity.Meet: node isn't an entity or faction!" );
		}
	}
	public virtual void RelationIncrease( Renown.Object other, float nAmount ) {
		if ( !Relations.TryGetValue( new RenownValue( other ), out RenownValue score ) ) {
			Meet( other );
		}
		score.Value += nAmount;
		EmitSignalIncreaseRelation( other as Node, this, nAmount );

		Console.PrintLine( string.Format( "Relation between {0} and {1} increased by {2}", this, other, nAmount ) );
	}
	public virtual void RelationDecrease( Renown.Object other, float nAmount ) {
		if ( !Relations.TryGetValue( new RenownValue( other ), out RenownValue score ) ) {
			Meet( other );
		}
		score.Value -= nAmount;
		EmitSignalDecreaseRelation( other as Node, this, nAmount );

		Console.PrintLine( string.Format( "Relation between {0} and {1} decreased by {2}", this, other, nAmount ) );
	}

	private void OnFlameAreaBodyShape2DEntered( Rid bodyRid, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		if ( body is Entity entity && entity != null ) {
			entity.AddStatusEffect( "status_burning" );
		}
	}

	public override void AddStatusEffect( string effectName ) {
		base.AddStatusEffect( effectName );

		EmitSignalStatusEffectTriggered( effectName, StatusEffects[ effectName ] );
	}

	public static void StartThoughtBubble( string text ) {
		Resource dialogue = DialogueManager.CreateResourceFromText( string.Format( "~ thought_bubble\n{0}", text ) );
		LevelData.Instance.ThisPlayer.Velocity = Godot.Vector2.Zero;
		DialogueManager.ShowDialogueBalloon( dialogue, "thought_bubble" );
	}
	public static void StartDialogue( Resource dialogueResource, string key, System.Action<int> callback ) {
		LevelData.Instance.ThisPlayer.Velocity = Godot.Vector2.Zero;
		DialogueManager.ShowDialogueBalloon( dialogueResource, key );
		DialogueCallback = callback;
	}
	public void ThoughtBubble( string text ) {
		StartThoughtBubble( text );
	}

	private void OnDialogueEnded( Resource dialogueResource ) {
	}
	private void OnDialogueStarted( Resource dialogueResource ) {
	}
	private void OnMutated( Godot.Collections.Dictionary mutation ) {
		DialogueCallback?.DynamicInvoke( DialogueGlobals.Get().PlayerChoice );
	}

	public static bool IsTutorialActive() {
		return !TutorialCompleted;
	}

	public void SetTileMapFloorLevel( int nLevel ) => TileMapLevel = nLevel;
	public int GetTileMapFloorLevel() => TileMapLevel;

	public void AddToJournal( Note note ) => JournalCache.TryAdd( TranslationServer.Translate( note.TextId + "_TITLE" ), TranslationServer.Translate( note.TextId ) );

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
				writer.SaveInt( string.Format( "AmmoStackAmount{0}", stackIndex ), stack.Value.Amount );
				writer.SaveString( string.Format( "AmmoStackNode{0}", stackIndex ), stack.Value.AmmoType.GetPath() );
				stackIndex++;
			}

			writer.SaveInt( "WeaponStacksCount", WeaponsStack.Count );
			stackIndex = 0;
			foreach ( var stack in WeaponsStack ) {
				writer.SaveString( string.Format( "WeaponStackNode{0}", stackIndex ), stack.Value.GetPath() );
				stackIndex++;
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
		using SaveSystem.SaveSectionReader reader = ArchiveSystem.GetSection( "Player" );

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
			stack.Amount = reader.LoadInt( string.Format( "AmmoStackAmount{0}", i ) );
			stack.AmmoType = new AmmoEntity();

			string path = reader.LoadString( string.Format( "AmmoStackNode{0}", i ) );
			stack.AmmoType.Load( path );
			AddChild( stack.AmmoType );

			AmmoStacks.Add( path.GetHashCode(), stack );
		}

		WeaponsStack.Clear();
		int numWeapons = reader.LoadInt( "WeaponStacksCount" );
		for ( int i = 0; i < numWeapons; i++ ) {
			WeaponEntity weapon = new WeaponEntity();
			weapon._Owner = this;
			weapon.Load( reader.LoadString( string.Format( "WeaponStackNode{0}", i ) ) );
			AddChild( weapon );
			WeaponsStack.Add( weapon.GetPath().GetHashCode(), weapon );
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

		if ( CurrentWeapon != WeaponSlot.INVALID ) {
			CallDeferred( MethodName.EmitSignal, SignalName.SwitchedWeapon, WeaponSlots[ CurrentWeapon ].GetWeapon() );
		}
	}

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public void SetTileMapFloor( TileMapFloor floor ) => Floor = floor;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public TileMapFloor GetTileMapFloor() => Floor;

	public Checkpoint GetCurrentCheckpoint() => LastCheckpoint;

	public void LoadWeapon( WeaponEntity weapon, string ammo, int slot ) {
		weapon._Owner = this;

		if ( slot != WeaponSlot.INVALID ) {
			weapon.SetEquippedState( true );
			WeaponSlots[ slot ].SetWeapon( weapon );
		}

		if ( ammo != null ) {
			AmmoStack stack = null;
			foreach ( var it in AmmoStacks ) {
				if ( it.Key == ammo.GetHashCode() ) {
					stack = it.Value;
					break;
				}
			}
			if ( stack != null ) {
				weapon.CallDeferred( WeaponEntity.MethodName.SetReserve, stack );
				weapon.CallDeferred( WeaponEntity.MethodName.SetAmmo, stack.AmmoType );
			} else {
				// TODO: error
			}
		}
	}
	#endregion

	private void ReceivePacket( ulong senderId, System.IO.BinaryReader reader ) {
		SyncObject.BeginRead( reader );

		PlayerUpdateType type = (PlayerUpdateType)SyncObject.ReadByte();
		switch ( type ) {
		case PlayerUpdateType.Damage: {
			switch ( (PlayerDamageSource)SyncObject.ReadByte() ) {
			case PlayerDamageSource.Player:
				float damage = SyncObject.ReadFloat();
				Damage( SteamLobby.Instance.GetPlayer( (CSteamID)senderId ), damage );
				break;
			case PlayerDamageSource.NPC:
				break;
			case PlayerDamageSource.Environment:
				break;
			};
			break; }
		case PlayerUpdateType.SetSpawn:
			// only ever called in team modes
			GlobalPosition = SyncObject.ReadVector2();
			break;
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
		SyncObject.Write( (byte)PlayerUpdateType.Update );
		SyncObject.Write( TorsoAnimation.FlipH );

		SyncObject.Write( GlobalPosition.X );
		SyncObject.Write( GlobalPosition.Y );

		if ( (uint)Flags != LastSyncState.LastNetworkFlags ) {
			SyncObject.Write( true );
			SyncObject.Write( (uint)Flags );
			LastSyncState.LastNetworkFlags = (uint)Flags;
		} else {
			SyncObject.Write( false );
		}

		if ( LastSyncState.LastNetworkAimAngle != AimLine.GlobalRotation ) {
			SyncObject.Write( true );
			LastSyncState.LastNetworkAimAngle = AimLine.GlobalRotation;
			SyncObject.Write( LastSyncState.LastNetworkAimAngle );
		} else {
			SyncObject.Write( false );
		}

		if ( LastSyncState.LastNetworkBloodAmount != BloodAmount ) {
			SyncObject.Write( true );
			SyncObject.Write( BloodAmount );
			LastSyncState.LastNetworkBloodAmount = BloodAmount;
		} else {
			SyncObject.Write( false );
		}

		if ( CurrentWeapon == WeaponSlot.INVALID ) {
			SyncObject.Write( false );
			SyncObject.Write( false );
		} else {
			WeaponEntity weapon = WeaponSlots[ CurrentWeapon ].GetWeapon();
			if ( LastSyncState.LastNetworkWeaponID != (string)weapon.Data.Get( "id" ) ) {
				SyncObject.Write( true );
				SyncObject.Write( (string)weapon.Data.Get( "id" ) );
				LastSyncState.LastNetworkWeaponID = (string)weapon.Data.Get( "id" );
			} else {
				SyncObject.Write( false );
			}
			if ( LastSyncState.LastNetworkUseMode != weapon.LastUsedMode ) {
				SyncObject.Write( true );
				SyncObject.Write( (uint)weapon.LastUsedMode );
				LastSyncState.LastNetworkUseMode = weapon.LastUsedMode;
			} else {
				SyncObject.Write( false );
			}
		}

		SyncObject.Write( (byte)LeftArmAnimationState );
		SyncObject.Write( (byte)RightArmAnimationState );
		SyncObject.Write( (byte)LegAnimationState );
		SyncObject.Write( (byte)TorsoAnimationState );

		SyncObject.Sync( Steamworks.Constants.k_nSteamNetworkingSend_Unreliable );
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

	public bool HasPerk( string name ) {
		foreach ( var perk in UnlockedBoons ) {
			if ( perk.Name == name ) {
				return true;
			}
		}
		return false;
	}
	public bool HasRune( string name ) {
		foreach ( var rune in UnlockedRunes ) {
			if ( rune.Name == name ) {
				return true;
			}
		}
		return false;
	}

	private void IncreaseBlood( float nAmount ) {
		if ( !SettingsData.GetShowBlood() ) {
			return;
		}

		// cover us with more blood if we're sober
		if ( ( Flags & PlayerFlags.Sober ) == 0 ) {
			BloodAmount += nAmount * 0.25f;
			BloodMaterial.SetShaderParameter( "blood_coef", BloodAmount );
			BloodDropTimer.Start();
		} else {
			BloodAmount += nAmount;
			BloodMaterial.SetShaderParameter( "blood_coef", BloodAmount );
			BloodDropTimer.Start();
		}
	}
	private void OnBloodDropTimerTimeout() {
		if ( !SettingsData.GetShowBlood() ) {
			return;
		}

		// release more blood if we're high
		if ( ( Flags & PlayerFlags.Sober ) == 0 ) {
			BloodAmount -= 0.01f;
		} else {
			BloodAmount -= 0.001f;
		}
		BloodMaterial.SetShaderParameter( "blood_coef", BloodAmount );

		if ( BloodAmount < 0.0f ) {
			BloodAmount = 0.0f;
			BloodDropTimer.Stop();
		}
	}

	public override void SetLocation( in WorldArea location ) {
		if ( location != Location ) {
			DiscoveredAreas.Add( location );
			EmitSignalLocationChanged( location );
		}
 
		base.SetLocation( location );
	}

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public AnimatedSprite2D GetTorsoAnimation() => TorsoAnimation;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public AnimatedSprite2D GetLegsAnimation() => LegAnimation;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public AnimatedSprite2D GetLeftArmAnimation() => ArmLeft.Animations;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public AnimatedSprite2D GetRightArmAnimation() => ArmRight.Animations;

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public void SetPrimaryWeapon( WeaponEntity weapon ) => WeaponSlots[ (int)WeaponSlotIndex.Primary ].SetWeapon( weapon );
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public void SetHeavyPrimaryWeapon( WeaponEntity weapon ) => WeaponSlots[ (int)WeaponSlotIndex.HeavyPrimary ].SetWeapon( weapon );
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public void SetSidearmWeapon( WeaponEntity weapon ) => WeaponSlots[ (int)WeaponSlotIndex.Sidearm ].SetWeapon( weapon );
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public void SetHeavySidearmWeapon( WeaponEntity weapon ) => WeaponSlots[ (int)WeaponSlotIndex.HeavySidearm ].SetWeapon( weapon );

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public WeaponSlot GetPrimaryWeapon() => WeaponSlots[ (int)WeaponSlotIndex.Primary ];
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public WeaponSlot GetHeavyPrimaryWeapon() => WeaponSlots[ (int)WeaponSlotIndex.HeavyPrimary ];
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public WeaponSlot GetSidearmWeapon() => WeaponSlots[ (int)WeaponSlotIndex.Sidearm ];
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public WeaponSlot GetHeavySidearmWeapon() => WeaponSlots[ (int)WeaponSlotIndex.HeavySidearm ];

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public Resource GetCurrentMappingContext() => CurrentMappingContext;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public WeaponSlot[] GetWeaponSlots() => WeaponSlots;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public Godot.Collections.Dictionary<int, WeaponEntity> GetWeaponsStack() => WeaponsStack;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public Godot.Collections.Dictionary<int, AmmoStack> GetAmmoStacks() => AmmoStacks;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public Node GetInventory() => Inventory;
	public Arm GetWeaponHand( WeaponEntity weapon ) {
		if ( ArmLeft.Slot != WeaponSlot.INVALID && WeaponSlots[ ArmLeft.Slot ].GetWeapon() == weapon ) {
			return ArmLeft;
		} else if ( ArmRight.Slot != WeaponSlot.INVALID && WeaponSlots[ ArmRight.Slot ].GetWeapon() == weapon ) {
			return ArmRight;
		}
		return null;
	}
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public Godot.Vector2 GetInputVelocity() => InputVelocity;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public WeaponSlot GetSlot( int nSlot ) => WeaponSlots[ nSlot ];

	private void SyncShadow() {
		// TODO: quality adjustment for this?
		float rotation = GlobalPosition.AngleTo( WorldTimeManager.Instance.RedSunLight.GlobalPosition );

		LeftArmShadowAnimation.GlobalRotation = rotation + ArmLeft.Animations.GlobalRotation;
		RightArmShadowAnimation.GlobalRotation = rotation + ArmRight.Animations.GlobalRotation;

		bool flip = TorsoAnimation.FlipH;
		TorsoShadowAnimation.FlipH = flip;
		LegShadowAnimation.FlipH = flip;
		LeftArmShadowAnimation.FlipV = flip;
		RightArmShadowAnimation.FlipV = flip;

		Shadows.GlobalRotation = rotation;

		LeftArmShadowAnimation.Show();
		RightArmShadowAnimation.Show();

		AnimatedSprite2D backShadow = flip ? RightArmShadowAnimation : LeftArmShadowAnimation;
		AnimatedSprite2D frontShadow = flip ? LeftArmShadowAnimation : RightArmShadowAnimation;

		if ( HandsUsed == Hands.Both ) {
			if ( TorsoAnimation.FlipH ) {
				(frontShadow, backShadow) = (backShadow, frontShadow);
			} else {
				frontShadow = LastUsedArm == ArmLeft ? LeftArmShadowAnimation : RightArmShadowAnimation;
			}
		}
		backShadow.Visible = HandsUsed != Hands.Both;

		frontShadow.Show();

		Shadows.MoveChild( backShadow, 0 );
		Shadows.MoveChild( frontShadow, 3 );
	}
	public float GetArmAngle() {
		Godot.Vector2 mousePosition;
		if ( (int)SettingsData.GetWindowMode() >= 2 ) {
			mousePosition = DisplayServer.MouseGetPosition();
		} else {
			mousePosition = GetViewport().GetMousePosition();
		}

		if ( LastMousePosition != mousePosition ) {
			LastMousePosition = mousePosition;
			IdleReset();

			ArmAngle = GetLocalMousePosition().Angle();
			HeadAnimation.GlobalRotation = ArmAngle;
			if ( mousePosition.X >= ScreenSize.X / 2.0f ) {
				FlipSpriteRight();
			} else if ( mousePosition.X <= ScreenSize.X / 2.0f ) {
				FlipSpriteLeft();
			}
		}
		return ArmAngle;
	}

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public CanvasLayer GetHUD() => HUD;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public void SetArmAngle( float nAngle ) => ArmAngle = nAngle;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public Arm GetLastUsedArm() => LastUsedArm;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public void SetLastUsedArm( in Arm arm ) => LastUsedArm = arm;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public float GetSoundLevel() => SoundLevel;
	public void SetHealth( float nHealth ) {
		Health = nHealth;
		if ( Health > MaxHealth.MaxValue ) {
			Health = MaxHealth.MaxValue;
		}
		EmitSignalHealthChanged( Health );
	}
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public float GetRage() => Rage;
	public void SetRage( float nRage ) {
		Rage = nRage;
		if ( Rage > MaxRage.MaxValue ) {
			Rage = MaxRage.MaxValue;
		}
		EmitSignalRageChanged( Rage );
	}
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public PlayerFlags GetFlags() => Flags;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public void SetFlags( PlayerFlags flags ) => Flags = flags;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public Hands GetHandsUsed() => HandsUsed;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public void SetHandsUsed( Hands hands ) => HandsUsed = hands;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public WeaponSlot[] GetSlots() => WeaponSlots;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public void SetSlots( WeaponSlot[] slots ) => WeaponSlots = slots;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public void SetGroundMaterial( GroundMaterialType nType ) => GroundType = nType;

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public Arm GetLeftArm() => ArmLeft;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public Arm GetRightArm() => ArmRight;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public int GetCurrentWeapon() => CurrentWeapon;

	public static void ShakeCamera( float nAmount ) {
		ShakeStrength += nAmount;
		JoltDirection = Godot.Vector2.Zero;
	}
	public static void ShakeCameraDirectional( float nAmount, Godot.Vector2 direction ) {
		JoltDirection = direction;
		ShakeStrength = nAmount;
	}

	private void IdleReset() {
		IdleTimer.Start();
		IdleAnimation.Hide();
		IdleAnimation.Stop();

		AimLine.Show();

		HeadAnimation.Show();
		LegAnimation.Show();
		TorsoAnimation.Show();
		ArmRight.Animations.Show();
		ArmLeft.Animations.Show();
	}

	public override void PlaySound( AudioStreamPlayer2D channel, AudioStream stream ) {
		if ( channel == null ) {
			AudioChannel.Stream = stream;
			AudioChannel.PitchScale = (float)GD.RandRange( 0.8f, 1.4f );
			AudioChannel.Play();
		} else {
			channel.Stream = stream;
			channel.PitchScale = (float)GD.RandRange( 0.8f, 1.4f );
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

		SetHealth( Health );
		SetRage( Rage );

		if ( ( GameConfiguration.GameMode == GameMode.Online && SteamLobby.Instance.IsOwner() ) || GameConfiguration.GameMode == GameMode.SinglePlayer ) {
			ArchiveSystem.SaveGame( SettingsData.GetSaveSlot() );
		}
	}

	private void OnPlayerMultiplayerRespawn() {
		TorsoAnimation.AnimationFinished -= OnPlayerMultiplayerRespawn;

		MultiplayerReset();

		SyncObject.Write( (byte)SteamLobby.MessageType.ClientData );
		SyncObject.Write( (byte)PlayerUpdateType.Death );
		SyncObject.Sync( Steamworks.Constants.k_nSteamNetworkingSend_Reliable );

		BlockInput( false );

		SetProcess( true );

		Flags &= ~PlayerFlags.Dashing;
	}
	public void MultiplayerReset() {
		TorsoAnimation.Play( "default" );

		LegAnimation.Play( "idle" );
		LegAnimation.Show();

		ArmLeft.Animations.SpriteFrames = ArmLeft.DefaultAnimation;
		ArmLeft.Animations.Play( "idle" );
		ArmLeft.Animations.Show();

		ArmRight.Animations.SpriteFrames = ArmRight.DefaultAnimation;
		ArmRight.Animations.Play( "idle" );
		ArmRight.Animations.Show();

		ArmLeft.Slot = WeaponSlot.INVALID;
		ArmRight.Slot = WeaponSlot.INVALID;
		HandsUsed = Hands.Right;

		CurrentWeapon = WeaponSlot.INVALID;

		BloodAmount = 0.0f;
		BloodMaterial.SetShaderParameter( "blood_coef", BloodAmount );

		SetHealth( 100.0f );
		SetRage( 60.0f );

		WeaponsStack.Clear();
		AmmoStacks.Clear();
		ConsumableStacks.Clear();

		for ( int i = 0; i < MAX_WEAPON_SLOTS; i++ ) {
			WeaponSlots[ i ].SetWeapon( null );
			WeaponSlots[ i ].SetMode( WeaponEntity.Properties.None );
		}

		BlockInput( false );
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

		TorsoAnimationState = PlayerAnimationState.Dead;
		LegAnimationState = PlayerAnimationState.Dead;
		RightArmAnimationState = PlayerAnimationState.Dead;
		LeftArmAnimationState = PlayerAnimationState.Dead;

		TorsoAnimation.Play( "death" );
		if ( GameConfiguration.GameMode == GameMode.Multiplayer ) {
			TorsoAnimation.AnimationFinished += OnPlayerMultiplayerRespawn;
		} else {
			SetProcessUnhandledInput( true );
		}
		SetProcess( false );
	}

	public override void Damage( in Entity attacker, float nAmount ) {
		if ( ( Flags & PlayerFlags.Dashing ) != 0 ) {
			return; // iframes
		}
		if ( attacker != null ) {
			ShakeCameraDirectional( nAmount, ( attacker.GlobalPosition - GlobalPosition ).Normalized() );
		}

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
			CallDeferred( MethodName.OnDeath, attacker );
		} else {
			CallDeferred( MethodName.PlaySound, AudioChannel, ResourceCache.PlayerPainSfx[ RNJesus.IntRange( 0, ResourceCache.PlayerPainSfx.Length - 1 ) ] );
		}

		CallDeferred( MethodName.EmitSignal, SignalName.Damaged, attacker, this, nAmount );
	}

	public void OnParry( Bullet from, float damage ) {
		float distance = from.GlobalPosition.DistanceTo( from.GlobalPosition );

		// we punch the bullet or object so hard it creates a shrapnel cloud
		RayIntersectionInfo collider = GodotServerManager.CheckRayCast( GlobalPosition, ArmAngle, distance, GetRid() );
		if ( collider.Collider is Entity entity && entity != null ) {
			entity.Damage( this, damage );
			entity.AddStatusEffect( "status_burning" );
		}
	}
	public void OnParry( Vector2 from, Vector2 to, float damage ) {
		//		float distance = from.GlobalPosition.DistanceTo( from.TargetPosition );

		if ( ( Flags & PlayerFlags.LightParrying ) == 0 && ( Flags & PlayerFlags.HeavyParrying ) == 0 ) {
			return;
		}

		if ( Velocity == Godot.Vector2.Zero ) {
			PlaySound( MiscChannel, ResourceCache.GetSound( "res://sounds/player/parry_heavy.ogg" ) );
		} else {
			PlaySound( MiscChannel, ResourceCache.GetSound( "res://sounds/player/parry_light.ogg" ) );
		}

		EmitSignalParrySuccess();

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
				hitbox.OnHit( this, damage );
			}
		}

		Rage -= 35.0f;

		EmitSignalRageChanged( Rage );

		// create a freeze frame
		float timeScale = (float)Engine.TimeScale;
		Engine.TimeScale = 0.25f;
		ToSignal( GetTree().CreateTimer( 0.30f, false, false, true ), SceneTreeTimer.SignalName.Timeout ).OnCompleted( new Action( () => {
			Engine.TimeScale = timeScale;

			ParryDamageArea.SetDeferred( Area2D.PropertyName.Monitoring, false );

			HUD.CallDeferred( "EndParry" );
		} ) );
	}

	public void BlockInput( bool bBlocked ) {
		if ( bBlocked ) {
			Flags |= PlayerFlags.BlockedInput;
			Velocity = Godot.Vector2.Zero;
		} else {
			Flags &= ~PlayerFlags.BlockedInput;
		}
	}

	private void OnCheckpointRestBegin() {
		TorsoAnimationState = PlayerAnimationState.CheckpointIdle;
		LegAnimationState = PlayerAnimationState.CheckpointIdle;
		LeftArmAnimationState = PlayerAnimationState.CheckpointIdle;
		RightArmAnimationState = PlayerAnimationState.CheckpointIdle;

		HeadAnimation.Hide();
		TorsoAnimation.Hide();
		LegAnimation.Hide();
		ArmLeft.Animations.Hide();
		ArmRight.Animations.Hide();

		IdleAnimation.Show();
		IdleAnimation.Play( "checkpoint_idle" );

		AimLine.Hide();

		IdleAnimation.FlipH = GlobalPosition.DirectionTo( LastCheckpoint.GlobalPosition ).X < 0.0f;

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
		IdleAnimation.Disconnect( AnimatedSprite2D.SignalName.AnimationFinished, Callable.From( OnCheckpointExitEnd ) );
		BlockInput( false );
		SetProcessUnhandledInput( false );

		Flags &= ~PlayerFlags.Resting;
	}
	public void LeaveCampfire() {
		AimLine.Show();

		CheckpointDrinkTimer.Stop();
		CheckpointDrinkTimer.ProcessMode = ProcessModeEnum.Disabled;
		IdleAnimation.Play( "checkpoint_exit" );
		IdleAnimation.Connect( AnimatedSprite2D.SignalName.AnimationFinished, Callable.From( OnCheckpointExitEnd ) );
	}
	public void RestAtCampfire() {
		if ( ( Flags & PlayerFlags.Resting ) != 0 ) {
			return;
		}
		Debug.Assert( ( Flags & PlayerFlags.Resting ) == 0 );

		IdleTimer.Stop();

		AimLine.Hide();

		// clean all the blood off us
		BloodAmount = 0.0f;
		BloodMaterial.SetShaderParameter( "blood_coef", BloodAmount );

		GetNode<CanvasLayer>( "/root/TransitionScreen" ).Connect( "transition_finished", Callable.From( OnCheckpointRestBegin ) );
		GetNode<CanvasLayer>( "/root/TransitionScreen" ).Call( "transition" );

		Flags |= PlayerFlags.Resting;

		SetProcessUnhandledInput( true );
		BlockInput( true );
	}

	public void TalkBegin() {
	}

	public void EndInteraction() {
		EmitSignalHideInteraction();

		Flags &= ~PlayerFlags.Checkpoint;
	}
	public void BeginInteraction( InteractionItem item ) {
		EmitSignalShowInteraction( item );

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

		HeadAnimation.Hide();
		TorsoAnimation.Hide();
		ArmLeft.Animations.Hide();
		ArmRight.Animations.Hide();
		LegAnimation.Hide();
		IdleAnimation.Show();
		IdleAnimation.Play( "start" );

		AimLine.Hide();

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
			Player.ShakeCamera( 2.5f );
			if ( LegAnimation.Animation != "run_change" ) {
				FootSteps.AddStep( Velocity, GlobalPosition, GroundType, WalkChannel );
			}
			PlaySound( MiscChannel, ResourceCache.GetSound( "res://sounds/player/arm_foley.ogg" ) );
			SetSoundLevel( 36.0f );
		}
	}
	private void OnDashEnd() {
		EmitSignalDashEnd();
		DashLight.Hide();
		DashEffect.Emitting = false;
		Flags &= ~PlayerFlags.Dashing;
	}
	private void OnSlideTimeout() {
		SlideEffect.Emitting = false;
		Flags &= ~PlayerFlags.Sliding;
	}

	private bool IsInputBlocked( bool bIsInventory = false ) => ( Flags & PlayerFlags.BlockedInput ) != 0 || ( !bIsInventory && ( Flags & PlayerFlags.Inventory ) != 0 );

	private void OnDashBurnout() {
		Flags &= ~PlayerFlags.Dashing;
		AddChild( ResourceCache.GetScene( "res://scenes/effects/explosion.tscn" ).Instantiate<Explosion>() );
		Damage( this, 30.0f );
		AddStatusEffect( "status_burning" );
		ShakeCameraDirectional( 50.0f, DashDirection );

		SteamAchievements.ActivateAchievement( "ACH_AHHH_GAHHH_HAAAAAAA" );
	}
	private void OnDash() {
		if ( IsInputBlocked() || !DashKit.CanDash() ) {
			return;
		}

		IdleReset();
		Flags |= PlayerFlags.Dashing;
		DashEffect.Emitting = true;
		DashLight.Show();
		DashDirection = Velocity;

		DashKit.OnDash();

		EmitSignalDashStart();
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

			float soundLevel = 0.0f;
			if ( weapon.IsBladed() && ( Flags & PlayerFlags.UsingMelee ) == 0 ) {
				Flags |= PlayerFlags.UsingMelee;

				if ( InputVelocity.X < 0.0f ) {
					bool slash = TorsoAnimation.FlipH;

					BladeAttackType = slash ? BladeAttackType.Slash : BladeAttackType.Thrust;
					if ( slash ) {
						FrameDamage += weapon.Use( weapon.LastUsedMode, out soundLevel, BladeAttackType );
					} else {
						weapon.BladedThrustWindUp();
					}
				} else if ( InputVelocity.X > 0.0f ) {
					bool slash = !TorsoAnimation.FlipH;

					BladeAttackType = slash ? BladeAttackType.Slash : BladeAttackType.Thrust;
					if ( slash ) {
						FrameDamage += weapon.Use( weapon.LastUsedMode, out soundLevel, BladeAttackType );
					} else {
						weapon.BladedThrustWindUp();
					}
				} else {
					BladeAttackType = BladeAttackType.Slash;
					FrameDamage += weapon.Use( weapon.LastUsedMode, out soundLevel, BladeAttackType );
				}
			} else if ( weapon.IsFirearm() ) {
				FrameDamage += weapon.Use( weapon.LastUsedMode, out soundLevel, ( Flags & PlayerFlags.UsingWeapon ) != 0 );
			}

			EmitSignalUsedWeapon( weapon );
			if ( FrameDamage > 0.0f ) {
				IncreaseBlood( FrameDamage * 0.0001f );
				FreeFlow.IncreaseCombo();
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
			ArmAngle = ArmAngleAction.Get( "value_axis_2d" ).AsVector2().Angle();
		}
		AimLine.GlobalRotation = ArmAngle;

		if ( GameConfiguration.GameMode != GameMode.Multiplayer ) {
			SyncShadow();
		}
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
	private void OnBulletTime() {
		if ( IsInputBlocked() || Rage <= 0.0f ) {
			return;
		}

		IdleReset();
		EmitSignalBulletTimeStart();

		if ( ( Flags & PlayerFlags.BulletTime ) != 0 ) {
			ExitBulletTime();
		} else {
			PlaySound( MiscChannel, ResourceCache.SlowMoBeginSfx );

			Flags |= PlayerFlags.BulletTime;
			Engine.TimeScale = 0.40f;
		}
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
			(src.Slot, dst.Slot) = (dst.Slot, src.Slot);
		} else {
			// if we have nothing in the destination hand, then just clear the source hand
			(src.Slot, dst.Slot) = (WeaponSlot.INVALID, src.Slot);
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
			Console.PrintError( "Player.SwitchWeaponHand: invalid hand, setting to default of right" );
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
				break;
			}
		case Hands.Right: {
				int index = ArmRight.Slot;
				if ( index == WeaponSlot.INVALID ) {
					return;
				}
				slot = WeaponSlots[ index ];
				break;
			}
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
		if ( ( Flags & PlayerFlags.UsingMelee ) != 0 && BladeAttackType == BladeAttackType.Thrust ) {
			int slot = LastUsedArm.Slot;
			if ( slot == WeaponSlot.INVALID ) {
				return; // nothing equipped
			}
			if ( WeaponSlots[ slot ].IsUsed() ) {
				WeaponEntity weapon = WeaponSlots[ slot ].GetWeapon();
				weapon.SetAttackAngle( ArmAngle );
				if ( FrameDamage > 0.0f ) {
					IncreaseBlood( FrameDamage * 0.0001f );
					FreeFlow.IncreaseCombo();
				}
			}
		}

		Flags &= ~( PlayerFlags.UsingWeapon | PlayerFlags.UsingMelee );
	}

	private void OnMeleeFinished() {
		ArmLeft.Animations.AnimationFinished -= OnMeleeFinished;
		Flags &= ~( PlayerFlags.LightParrying | PlayerFlags.HeavyParrying );
		BlockInput( false );

		ParryDamageArea.SetDeferred( Area2D.PropertyName.Monitoring, false );
		ParryArea.SetDeferred( Area2D.PropertyName.Monitoring, false );
	}
	private void OnMelee() {
		if ( IsInputBlocked() ) {
			return;
		}

		ParryArea.SetDeferred( Area2D.PropertyName.Monitoring, true );
		ParryDamageArea.SetDeferred( Area2D.PropertyName.Monitoring, true );

		Flags |= ( PlayerFlags.LightParrying | PlayerFlags.HeavyParrying );

		if ( Velocity == Godot.Vector2.Zero ) {
			// if we're stationary, make the player commit to a heavier parry
			BlockInput( true );
			ArmLeft.Animations.CallDeferred( AnimatedSprite2D.MethodName.Play, "melee_heavy" );
		} else {
			ArmLeft.Animations.CallDeferred( AnimatedSprite2D.MethodName.Play, "melee_light" );
		}
		ArmLeft.Animations.SpriteFrames = DefaultLeftArmAnimations;
		ArmLeft.Animations.AnimationFinished += OnMeleeFinished;
		PlaySound( MiscChannel, ResourceCache.GetSound( "res://sounds/player/melee.ogg" ) );
	}

	public void SwitchInputMode( Resource InputContext ) {
		GetNode( "/root/GUIDE" ).Call( "enable_mapping_context", InputContext, true );

		CurrentMappingContext = InputContext;

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
			InteractAction = ResourceCache.InteractActionKeyboard;
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
		}

		EmitSignalInputMappingContextChanged();
	}
	public override void _ExitTree() {
		base._ExitTree();

		GetNode( "/root/GUIDE" ).Call( "disable_mapping_context", CurrentMappingContext );
	}

	private void OnRespawnTransitionFinished() {
		GetNode( "/root/TransitionScreen" ).Disconnect( "transition_finished", Callable.From( OnRespawnTransitionFinished ) );

		GlobalPosition = LastCheckpoint.GlobalPosition;
		BeginInteraction( LastCheckpoint );
	}

	public override void _UnhandledInput( InputEvent @event ) {
		base._UnhandledInput( @event );

		if ( ( Flags & PlayerFlags.Emoting ) != 0 ) {
			Flags &= ~PlayerFlags.Emoting;

			ArmLeft.Animations.Show();
			ArmRight.Animations.Show();
			TorsoAnimation.Show();
			LegAnimation.Show();

			CurrentEmote = null;

			// remove the emote sprite
			RemoveChild( GetChild( GetChildCount() - 1 ) );

			if ( Health > 0.0f ) {
				SetProcessUnhandledInput( false );
			}
		}

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
			GetNode( "/root/TransitionScreen" ).Connect( "transition_finished", Callable.From( OnRespawnTransitionFinished ) );
			GetNode( "/root/TransitionScreen" ).Call( "transition" );

			// dead
			SetHealth( MaxHealth.MaxValue );
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
		HeadAnimation.FlipV = true;
		LegAnimation.FlipH = true;
		TorsoAnimation.FlipH = true;
		ArmLeft.Flip = true;
		ArmRight.Flip = true;
	}
	private void FlipSpriteRight() {
		HeadAnimation.FlipV = false;
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

	private void OnMove() {
		if ( IsInputBlocked() ) {
			return;
		}

		InputVelocity = MoveAction.Get( "value_axis_2d" ).AsVector2();
	}
	private void OnUseGadget() {
		if ( IsInputBlocked() ) {
			return;
		}
		ArmAttachment?.Use();
	}

	public void ConnectGamepadBinds() {
		ResourceCache.MoveActionGamepad[ InputDevice ].Connect( "triggered", Callable.From( OnMove ) );
		ResourceCache.MoveActionGamepad[ InputDevice ].Connect( "completed", Callable.From( OnMove ) );
		//		ResourceCache.UseBothHandsActionsGamepad[ InputDevice ].Connect( "triggered", Callable.From( OnUseBothHands ) );
		ResourceCache.MeleeActionGamepad[ InputDevice ].Connect( "triggered", Callable.From( OnMelee ) );
		ResourceCache.SwitchWeaponModeActionGamepad[ InputDevice ].Connect( "triggered", Callable.From( SwitchWeaponMode ) );
		ResourceCache.BulletTimeActionGamepad[ InputDevice ].Connect( "triggered", Callable.From( OnBulletTime ) );
		ResourceCache.NextWeaponActionGamepad[ InputDevice ].Connect( "triggered", Callable.From( OnNextWeapon ) );
		ResourceCache.PrevWeaponActionGamepad[ InputDevice ].Connect( "triggered", Callable.From( OnPrevWeapon ) );
		ResourceCache.DashActionGamepad[ InputDevice ].Connect( "triggered", Callable.From( OnDash ) );
		ResourceCache.SlideActionGamepad[ InputDevice ].Connect( "triggered", Callable.From( OnSlide ) );
		ResourceCache.UseWeaponActionGamepad[ InputDevice ].Connect( "triggered", Callable.From( OnUseWeapon ) );
		ResourceCache.UseWeaponActionGamepad[ InputDevice ].Connect( "completed", Callable.From( OnStoppedUsingWeapon ) );
		ResourceCache.InteractActionGamepad[ InputDevice ].Connect( "triggered", Callable.From( EmitSignalInteraction ) );
	}
	private void ConnectKeyboardBinds() {
		ResourceCache.MoveActionKeyboard.Connect( "triggered", Callable.From( OnMove ) );
		ResourceCache.MoveActionKeyboard.Connect( "completed", Callable.From( OnMove ) );
		ResourceCache.UseBothHandsActionKeyboard.Connect( "triggered", Callable.From( OnUseBothHands ) );
		ResourceCache.MeleeActionKeyboard.Connect( "triggered", Callable.From( OnMelee ) );
		ResourceCache.SwitchWeaponModeActionKeyboard.Connect( "triggered", Callable.From( SwitchWeaponMode ) );
		ResourceCache.BulletTimeActionKeyboard.Connect( "triggered", Callable.From( OnBulletTime ) );
		ResourceCache.NextWeaponActionKeyboard.Connect( "triggered", Callable.From( OnNextWeapon ) );
		ResourceCache.PrevWeaponActionKeyboard.Connect( "triggered", Callable.From( OnPrevWeapon ) );
		ResourceCache.DashActionKeyboard.Connect( "triggered", Callable.From( OnDash ) );
		ResourceCache.SlideActionKeyboard.Connect( "triggered", Callable.From( OnSlide ) );
		ResourceCache.UseWeaponActionKeyboard.Connect( "triggered", Callable.From( OnUseWeapon ) );
		ResourceCache.UseWeaponActionKeyboard.Connect( "completed", Callable.From( OnStoppedUsingWeapon ) );
		ResourceCache.ArmAngleActionKeyboard.Connect( "triggered", Callable.From( OnArmAngleChanged ) );
		ResourceCache.InteractActionKeyboard.Connect( "triggered", Callable.From( EmitSignalInteraction ) );
		ResourceCache.UseGadgetActionKeyboard.Connect( "triggered", Callable.From( OnUseGadget ) );
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
		Engine.TimeScale = 1.0f;
		AudioServer.PlaybackSpeedScale = 1.0f;
		PlaySound( MiscChannel, ResourceCache.SlowMoEndSfx );

		EmitSignalBulletTimeEnd();
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

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	private void OnScreenSizeChanged() => ScreenSize = DisplayServer.WindowGetSize();
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	private void OnShadowAnimationSpriteSync( AnimatedSprite2D baseAnimation, AnimatedSprite2D shadowAnimation ) {
		shadowAnimation.Animation = baseAnimation.Animation;
		shadowAnimation.SpriteFrames = baseAnimation.SpriteFrames;
		shadowAnimation.Frame = baseAnimation.Frame;
		shadowAnimation.FrameProgress = baseAnimation.FrameProgress;
	}
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	private void OnShadowAnimationFrameSync( AnimatedSprite2D baseAnimation, AnimatedSprite2D shadowAnimation ) {
		shadowAnimation.Animation = baseAnimation.Animation;
		shadowAnimation.SpriteFrames = baseAnimation.SpriteFrames;
		shadowAnimation.Frame = baseAnimation.Frame;
		shadowAnimation.FrameProgress = baseAnimation.FrameProgress;
	}

	public override void _Ready() {
		base._Ready();

		AccessibilityManager.LoadBinds();

		DialogueManager.DialogueStarted += OnDialogueStarted;
		DialogueManager.DialogueEnded += OnDialogueEnded;
		DialogueManager.Mutated += OnMutated;

		ScreenSize = DisplayServer.WindowGetSize();
		if ( GameConfiguration.GameMode == GameMode.Multiplayer ) {
			MultiplayerData = new Multiplayer.PlayerData.MultiplayerMetadata( SteamManager.GetSteamID() );
			SyncObject = new NetworkSyncObject( 1024 );
			LastSyncState = new NetworkState();

			SteamLobby.Instance.AddPlayer( SteamUser.GetSteamID(),
				new SteamLobby.PlayerNetworkNode( this, SendPacket, ReceivePacket ) );
		} else {
			UnlockedBoons = new HashSet<Perk>();
			UnlockedRunes = new HashSet<Rune>();
			DiscoveredAreas = new HashSet<WorldArea>();
			JournalCache = new Godot.Collections.Dictionary<string, string>();

			WhisperChannel = new AudioStreamPlayer();
		}

		LevelData.Instance.PlayerRespawn += Respawn;

		MaxHealth = new PlayerStat<float>( 100.0f, 0.0f, float.MaxValue );
		MaxRage = new PlayerStat<float>( 100.0f, 0.0f, float.MaxValue );
		Speed = new PlayerStat<float>( MAX_SPEED, 0.0f, 100.0f );

		// don't allow keybind input when we're in the console
		Console.Control.VisibilityChanged += () => {
			BlockInput( Console.Control.Visible );
		};

		StartingPosition = GlobalPosition;

		AimLine = GetNode<Line2D>( "AimAssist/AimLine" );

		AimLine.Points[ 1 ].X = AimLine.Points[ 0 ].X * ( ScreenSize.X / 2.0f );

		Input.SetCustomMouseCursor( ResourceCache.GetTexture( "res://textures/hud/crosshairs/crosshairi.tga" ), Input.CursorShape.Arrow );

		Health = 100.0f;
		Rage = 60.0f;
		Sanity = 60.0f;

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

			DateTime Now = DateTime.Now;
			GetViewport().GetTexture().GetImage().SavePng(
				string.Format( "user://screenshots/screenshot{0}{1}{2}_{3}{4}{5}.png", Now.Year, Now.Month, Now.Day, Now.Hour, Now.Minute, Now.Second )
			);

			HUD.Show();
		} ) );

		/*
		HUD.EmoteTriggered += ( emote ) => {
			Flags &= ~( PlayerFlags.Checkpoint | PlayerFlags.IdleAnimation | PlayerFlags.Resting );
			Flags |= PlayerFlags.Emoting;
			CurrentEmote = emote;

			SetProcessUnhandledInput( true );

			Sprite2D EmoteSprite = new Sprite2D();
			EmoteSprite.Name = "Emote";
			EmoteSprite.Texture = emote;
			AddChild( EmoteSprite );

			IdleAnimation.Hide();
			ArmLeft.Animations.Hide();
			ArmRight.Animations.Hide();
			TorsoAnimation.Hide();
			LegAnimation.Hide();
		};
		*/

		SetProcessUnhandledInput( false );

		if ( !Renown.Constants.StartingQuestPath.IsEmpty && GetTree().CurrentScene.Name == "World" ) {
			QuestState.StartContract( ResourceLoader.Load( Renown.Constants.StartingQuestPath ), Renown.Constants.StartingQuestFlags, Renown.Constants.StartingQuestState );
		}

		ConnectGamepadBinds();
		ConnectKeyboardBinds();

		DashKit = new DashKit();
		DashKit.Connect( DashKit.SignalName.DashBurnedOut, Callable.From( OnDashBurnout ) );
		DashKit.Connect( DashKit.SignalName.DashBurnoutChanged, Callable.From<float>( EmitSignalDashBurnoutChanged ) );
		DashKit.Connect( DashKit.SignalName.DashEnd, Callable.From( OnDashEnd ) );
		AddChild( DashKit );

		//
		// initialize input context
		//
		SwitchToGamepad = ResourceLoader.Load( "res://resources/binds/actions/keyboard/switch_to_gamepad.tres" );
		SwitchToKeyboard = ResourceLoader.Load( "res://resources/binds/actions/gamepad/switch_to_keyboard.tres" );

		Viewpoint = GetNode<Camera2D>( "Camera2D" );

		AudioChannel = GetNode<AudioStreamPlayer2D>( "AudioChannel" );
		AudioChannel.VolumeDb = SettingsData.GetEffectsVolumeLinear();

		WalkChannel = new AudioStreamPlayer2D();
		WalkChannel.Name = "WalkChannel";
		WalkChannel.VolumeDb = SettingsData.GetEffectsVolumeLinear();
		AddChild( WalkChannel );

		MiscChannel = GetNode<AudioStreamPlayer2D>( "MiscChannel" );
		MiscChannel.Connect( AudioStreamPlayer2D.SignalName.Finished, Callable.From( OnSlowMoSfxFinished ) );
		MiscChannel.VolumeDb = SettingsData.GetEffectsVolumeLinear();

		CollisionShape2D SoundBounds = GetNode<CollisionShape2D>( "SoundArea/CollisionShape2D" );
		SoundArea = SoundBounds.Shape as CircleShape2D;

		ParryArea = GetNode<Area2D>( "AimAssist/AimLine/ParryArea" );
		ParryArea.SetMeta( "Owner", this );

		ParryBox = ParryArea.GetNode<CollisionShape2D>( "CollisionShape2D" );

		ParryDamageArea = GetNode<Area2D>( "AimAssist/AimLine/ParryDamageArea" );
		ParryDamageBox = GetNode<CollisionShape2D>( "AimAssist/AimLine/ParryDamageArea/CollisionShape2D" );

		Area2D Area = GetNode<Area2D>( "SoundArea" );
		Area.Connect( Area2D.SignalName.BodyShapeEntered, Callable.From<Rid, Node2D, int, int>( OnSoundAreaShape2DEntered ) );

		FootSteps = GetNode<FootSteps>( "FootSteps" );

		SwitchToKeyboard.Connect( "triggered", Callable.From( () => { SwitchInputMode( ResourceCache.KeyboardInputMappings ); } ) );
		SwitchToGamepad.Connect( "triggered", Callable.From( () => { SwitchInputMode( ResourceCache.GamepadInputMappings ); } ) );
		if ( GameConfiguration.GameMode != GameMode.LocalCoop2 ) {
			SwitchInputMode( ResourceCache.KeyboardInputMappings );
		}

		DefaultLeftArmAnimations = ArmLeft.Animations.SpriteFrames;

		HeadAnimation = GetNode<Sprite2D>( "Animations/Head" );

		IdleTimer = GetNode<Timer>( "IdleAnimationTimer" );
		IdleTimer.Connect( Timer.SignalName.Timeout, Callable.From( OnIdleAnimationTimerTimeout ) );

		IdleAnimation = GetNode<AnimatedSprite2D>( "Animations/Idle" );
		IdleAnimation.Connect( AnimatedSprite2D.SignalName.AnimationFinished, Callable.From( OnIdleAnimationAnimationFinished ) );

		LegAnimation = GetNode<AnimatedSprite2D>( "Animations/Legs" );
		LegAnimation.Connect( AnimatedSprite2D.SignalName.AnimationLooped, Callable.From( OnLegsAnimationLooped ) );

		TorsoAnimation = GetNode<AnimatedSprite2D>( "Animations/Torso" );

		Shadows = GetNode<Node2D>( "Animations/Shadows" );
		if ( GameConfiguration.GameMode == GameMode.Multiplayer ) {
			Shadows.Hide();
		} else {
			LegAnimation.Connect( AnimatedSprite2D.SignalName.FrameChanged, Callable.From( () => OnShadowAnimationFrameSync( LegAnimation, LegShadowAnimation ) ) );
			LegAnimation.Connect( AnimatedSprite2D.SignalName.SpriteFramesChanged, Callable.From( () => OnShadowAnimationSpriteSync( LegAnimation, LegShadowAnimation ) ) );

			TorsoAnimation.Connect( AnimatedSprite2D.SignalName.FrameChanged, Callable.From( () => OnShadowAnimationFrameSync( TorsoAnimation, TorsoShadowAnimation ) ) );
			TorsoAnimation.Connect( AnimatedSprite2D.SignalName.SpriteFramesChanged, Callable.From( () => OnShadowAnimationSpriteSync( TorsoAnimation, TorsoShadowAnimation ) ) );

			ArmLeft.Animations.Connect( AnimatedSprite2D.SignalName.FrameChanged, Callable.From( () => OnShadowAnimationFrameSync( ArmLeft.Animations, LeftArmShadowAnimation ) ) );
			ArmLeft.Animations.Connect( AnimatedSprite2D.SignalName.SpriteFramesChanged, Callable.From( () => OnShadowAnimationSpriteSync( ArmLeft.Animations, LeftArmShadowAnimation ) ) );

			ArmRight.Animations.Connect( AnimatedSprite2D.SignalName.FrameChanged, Callable.From( () => OnShadowAnimationFrameSync( ArmRight.Animations, RightArmShadowAnimation ) ) );
			ArmRight.Animations.Connect( AnimatedSprite2D.SignalName.SpriteFramesChanged, Callable.From( () => OnShadowAnimationSpriteSync( ArmRight.Animations, RightArmShadowAnimation ) ) );

			TorsoShadowAnimation = GetNode<AnimatedSprite2D>( "Animations/Shadows/TorsoShadow" );
			LegShadowAnimation = GetNode<AnimatedSprite2D>( "Animations/Shadows/LegsShadow" );
			LeftArmShadowAnimation = GetNode<AnimatedSprite2D>( "Animations/Shadows/ArmsLeftShadow" );
			RightArmShadowAnimation = GetNode<AnimatedSprite2D>( "Animations/Shadows/ArmsRightShadow" );
		}

		Flags |= PlayerFlags.Sober;

		Animations = GetNode( "Animations" );

		WalkEffect = GetNode<GpuParticles2D>( "Animations/DustPuff" );
		SlideEffect = GetNode<GpuParticles2D>( "Animations/SlidePuff" );
		DashEffect = GetNode<GpuParticles2D>( "Animations/DashEffect" );
		DashLight = GetNode<PointLight2D>( "Animations/DashEffect/PointLight2D" );

		SlideTime = GetNode<Timer>( "Timers/SlideTime" );
		SlideTime.Connect( Timer.SignalName.Timeout, Callable.From( OnSlideTimeout ) );

		CheckpointDrinkTimer = new Timer();
		CheckpointDrinkTimer.Name = "CheckpointDrinkTimer";
		CheckpointDrinkTimer.WaitTime = 10.5f;
		CheckpointDrinkTimer.ProcessMode = ProcessModeEnum.Disabled;
		CheckpointDrinkTimer.Connect( Timer.SignalName.Timeout, Callable.From( () => {
			IdleAnimation.CallDeferred( AnimatedSprite2D.MethodName.Play, "checkpoint_drink" );
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

		SteamAchievements.SteamAchievement achievement;
		if ( SteamAchievements.AchievementTable.TryGetValue( "ACH_BUILDING_THE_LEGEND", out achievement ) && !achievement.GetAchieved() ) {
			IncreaseRenown += ( Node self, int amount ) => {
				if ( amount >= 200 ) {
					SteamAchievements.ActivateAchievement( "ACH_BUIDLING_THE_LEGEND" );
				}
			};
		}
		if ( SteamAchievements.AchievementTable.TryGetValue( "ACH_RIGHT_BACK_AT_U", out achievement ) && !achievement.GetAchieved() ) {
			ParrySuccess += () => SteamAchievements.ActivateAchievement( "ACH_RIGHT_BACK_AT_U" );
		}
		if ( SteamAchievements.AchievementTable.TryGetValue( "ACH_MASTER_OF_THE_WASTES", out achievement ) && !achievement.GetAchieved() ) {
			LocationChanged += ( WorldArea location ) => {
				int count = 0;
				foreach ( var area in WorldArea.Cache.Cache ) {
					if ( DiscoveredAreas.Contains( area.Value ) ) {
						count++;
					}
				}
				if ( count == WorldArea.Cache.Cache.Count ) {
					SteamAchievements.ActivateAchievement( "ACH_MASTER_OF_THE_WASTES" );
				}
			};
		}

		//		RenderingServer.FramePostDraw += () => OnViewportFramePostDraw();
		//		RenderingServer.FramePreDraw += () => OnViewportFramePreDraw();

		Console.AddCommand( "suicide", Callable.From( CmdSuicide ), null, 0, "it's in the name" );
		Console.AddCommand( "teleport", Callable.From<string>( CmdTeleport ), new[] { "checkpoint" }, 1, "teleports the player to the specified location" );

		ProcessMode = ProcessModeEnum.Pausable;

		if ( SettingsData.GetShowBlood() ) {
			BloodDropTimer = new Timer();
			BloodDropTimer.Name = "BloodDropTimer";
			BloodDropTimer.WaitTime = 30.0f;
			BloodDropTimer.Connect( Timer.SignalName.Timeout, Callable.From( OnBloodDropTimerTimeout ) );
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
		} else {
			Totem = (Totem)ResourceCache.GetResource( "res://resources/totems/totem_of_remembrance.tres" );
		}

		Input.JoyConnectionChanged += ( device, connected ) => { if ( connected ) { SwitchInputMode( ResourceCache.GamepadInputMappings ); } };
	}
	private async void UpdateIdleBreath( float delta ) {
		IdleTime += delta;

		float breath = Mathf.Sin( IdleTime * 8.5f ) * 0.30f;
		TorsoAnimation.Position = new Vector2( 0.0f, breath );

		if ( IdleTime > NextShiftTime ) {
			NextShiftTime = IdleTime + GD.Randf() * 3.0f + 1.0f;

			float shiftAmount = GD.Randf() * 1.5f - 0.75f;
			float duration = 0.8f;

			CreateTween().TweenProperty( LegAnimation, "position", new Vector2( shiftAmount, LegAnimation.Position.Y ), duration ).SetEase( Tween.EaseType.Out );
			await ToSignal( GetTree().CreateTimer( duration ), "timeout" );
			CreateTween().TweenProperty( LegAnimation, "position", Vector2.Zero, duration * 0.7f ).SetEase( Tween.EaseType.InOut );
		}
	}
	public override void PhysicsUpdate( double delta ) {
		base._PhysicsProcess( delta );

		Player.ShakeCameraDirectional( 2.5f, new Vector2( RNJesus.FloatRange( -30.0f, 30.0f ), RNJesus.FloatRange( -30.0f, 30.0f ) ) );

		RayIntersectionInfo collision = GodotServerManager.CheckRayCast( GlobalPosition, ArmAngle, ScreenSize.X * 0.5f, GetRid() );
		if ( collision.Collider is Entity entity && entity != null && entity.GetFaction() != Faction ) {
			AimLine.DefaultColor = AimingAtTarget;
		} else if ( collision.Collider is Hitbox hitbox && hitbox != null && ( (Node2D)hitbox.GetMeta( "Owner" ) as Entity ).GetFaction() != Faction ) {
			AimLine.DefaultColor = AimingAtTarget;
		} else {
			AimLine.DefaultColor = AimingAtNull;
		}

		CheckStatus( (float)delta );

		if ( ( Flags & PlayerFlags.BlockedInput ) != 0 ) {
			return;
		}

		Godot.Vector2 velocity = Velocity;
		if ( InputVelocity == Godot.Vector2.Zero && velocity == Godot.Vector2.Zero ) {
			return;
		}

		float baseSpeed;
		if ( InputVelocity != Godot.Vector2.Zero ) {
			float speed = MAX_SPEED;
			// encumbured
			if ( TotalInventoryWeight >= MaximumInventoryWeight * 0.85f ) {
				speed *= 0.5f;
			}
			float accel = ACCEL;

			if ( ( Flags & PlayerFlags.Dashing ) != 0 ) {
				accel += 8800;
			}
			if ( ( Flags & PlayerFlags.Sliding ) != 0 ) {
				accel += 1200;
				LeftArmAnimationState = PlayerAnimationState.Sliding;
			}

			velocity = velocity.MoveToward( InputVelocity * speed, (float)delta * accel );
			TorsoAnimationState = PlayerAnimationState.Running;
			LegAnimationState = PlayerAnimationState.Running;

			WindUpProgress = Mathf.Clamp( WindUpProgress + WindUpDuration, 0.0f, 1.0f );
			baseSpeed = 1.0f;
			IdleTime = 0.0f;
		} else {
			velocity = velocity.MoveToward( Godot.Vector2.Zero, (float)delta * FRICTION );
			TorsoAnimationState = PlayerAnimationState.Idle;
			LegAnimationState = PlayerAnimationState.Idle;

			WindUpProgress = Mathf.Clamp( WindUpProgress - WindUpDuration * 2.0f, 0.0f, 1.0f );
			baseSpeed = 0.5f;

			if ( SettingsData.GetAnimationQuality() > AnimationQuality.Low ) {
				UpdateIdleBreath( (float)delta );
			}
		}

		float easedSpeedFactor = WindUpProgress < 0.1f ? 2.0f * WindUpProgress * WindUpProgress : 1.0f - Mathf.Pow( -2.0f * WindUpProgress + 2.0f, 2.0f ) / 2.0f;

		if ( baseSpeed == 1.0f ) {
			float armOffset = Mathf.Sin( Time.GetTicksMsec() / 120.0f ) * 2.0f * ( 1.0f - easedSpeedFactor );
			ArmLeft.Animations.SetDeferred( AnimatedSprite2D.PropertyName.Offset, new Vector2( 0.0f, armOffset ) );
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
			if ( SettingsData.GetAnimationQuality() > AnimationQuality.Low ) {
				if ( InputVelocity != Vector2.Zero ) {
					if ( ( TorsoAnimation.FlipH && InputVelocity.X > 0.0f ) || ( !TorsoAnimation.FlipH && InputVelocity.X < 0.0f ) ) {
						arm.Animations.PlayBackwards( "run" );
					} else {
						arm.Animations.Play( "run" );
					}
					animState = PlayerAnimationState.Running;
				} else {
					arm.Animations.Play( "idle" );
					animState = PlayerAnimationState.Idle;
				}
			} else {
				animState = InputVelocity != Vector2.Zero ? PlayerAnimationState.Running : PlayerAnimationState.Idle;
				arm.Animations.Play( animState == PlayerAnimationState.Idle ? "idle" : "run" );
			}
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
				if ( ( arm == ArmLeft && !arm.Animations.FlipV ) || ( arm == ArmRight && arm.Animations.FlipV ) ) {
					animationName += "_flip";
				}
			}
			arm.Animations.Play( animationName );
		}
	}
	private void CheckStatus( float delta ) {
		bool changed = false;
		if ( FrameDamage > 0.0f ) {
			// the more attacks we chain together without taking a hit, the more rage we get
			Rage += FrameDamage * FreeFlow.GetCurrentCombo() * delta;
			FrameDamage = 0.0f;
			Flags |= PlayerFlags.UsedMana;
			changed = true;
		}
		FrameDamage = 0.0f;
		if ( Health < 100.0f && Rage > 0.0f ) {
			Health += 3.0f * delta;
			Rage -= 10.0f * delta;
			changed = true;
			// mana conversion ratio to health is extremely inefficient

			Flags |= PlayerFlags.UsedMana;
			EmitSignalHealthChanged( Health );
		}
		if ( ( Flags & PlayerFlags.BulletTime ) != 0 ) {
			Rage -= 20.0f * delta;
			changed = true;
		}
		if ( Rage > 100.0f ) {
			Rage = 100.0f;
		} else if ( Rage < 0.0f ) {
			Rage = 0.0f;
			ExitBulletTime();
		}
		if ( changed ) {
			EmitSignalRageChanged( Rage );
		}
	}
	[MethodImpl( MethodImplOptions.AggressiveOptimization )]
	public override void Update( double delta ) {
		base._Process( delta );

		if ( InputVelocity != Godot.Vector2.Zero ) {
			if ( ( Flags & PlayerFlags.Sliding ) == 0 && ( Flags & PlayerFlags.OnHorse ) == 0 ) {
				if ( SettingsData.GetAnimationQuality() > AnimationQuality.Low ) {
					bool reverse;
					if ( ( TorsoAnimation.FlipH && InputVelocity.X > 0.0f ) || ( !TorsoAnimation.FlipH && InputVelocity.X < 0.0f ) ) {
						reverse = true;
					} else {
						reverse = false;
					}

					Vector2 velocity = Velocity;
					if ( !reverse ) {
						if ( ( InputVelocity.X < 0.0f && velocity.X > 0.0f ) || ( InputVelocity.X > 0.0f && velocity.X < 0.0f ) ) {
							LegAnimation.Play( "run_change" );
							PlaySound( WalkChannel, ResourceCache.SlideSfx[ 0 ] );
						} else {
							LegAnimation.Play( "run" );
						}
					} else {
						LegAnimation.PlayBackwards( "run" );
					}
				} else {
					LegAnimation.Play( "run" );
				}
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

			if ( JoltDirection != Godot.Vector2.Zero ) {
				Godot.Vector2 baseOffset = JoltDirection.Normalized() * ShakeStrength * DirectionalInfluence;

				/*
				Godot.Vector2 randomOffset = new Godot.Vector2(
					RNJesus.FloatRange( -ShakeStrength, ShakeStrength ) * ( 1.0f - DirectionalInfluence ),
					RNJesus.FloatRange( -ShakeStrength, ShakeStrength ) * ( 1.0f - DirectionalInfluence )
				);
				*/

				Viewpoint.Offset = baseOffset;
			} else {
				Viewpoint.Offset = new Godot.Vector2(
					RNJesus.FloatRange( -ShakeStrength, ShakeStrength ),
					RNJesus.FloatRange( -ShakeStrength, ShakeStrength )
				);
			}
		} else {
			Viewpoint.Offset = Godot.Vector2.Zero;
		}

		if ( SoundLevel > 0.1f ) {
			SoundLevel -= 1024.0f * (float)delta;
			if ( SoundLevel < 0.0f ) {
				SoundLevel = 0.1f;
			}
		}
		SoundArea.Radius = SoundLevel;

		if ( ( Flags & PlayerFlags.Resting ) != 0 || IdleAnimation.Visible ) {
			return;
		}

		//		GetArmAngle();

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
				(front, back) = (back, front);
			} else {
				front = LastUsedArm;
			}
		}
		back.Animations.Visible = HandsUsed != Hands.Both;

		front.Animations.Show();

		Animations.MoveChild( back.Animations, 1 );
		Animations.MoveChild( front.Animations, 4 );

		if ( ( Flags & PlayerFlags.LightParrying ) == 0 || ( Flags & PlayerFlags.HeavyParrying ) == 0 ) {
			CalcArmAnimation( ArmLeft, out LeftArmAnimationState );
			CalcArmAnimation( ArmRight, out RightArmAnimationState );
		}
	}

	private void OnWeaponModeChanged( WeaponEntity source, WeaponEntity.Properties useMode ) => EmitSignalWeaponStatusUpdated( source, useMode );

	public void PickupAmmo( AmmoEntity ammo, int nAmount = -1 ) {
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

			int hashCode = ammo.GetPath().GetHashCode();
			stack.SetType( ammo );
			stack.SetMeta( "hash", hashCode );
			AmmoStacks.Add( hashCode, stack );
		}
		stack.AddItems( nAmount == -1 ? (int)( (Godot.Collections.Dictionary)ammo.Data.Get( "properties" ) )[ "stack_add_amount" ] : nAmount );

		PlaySound( MiscChannel, ammo.PickupSfx );

		for ( int i = 0; i < MAX_WEAPON_SLOTS; i++ ) {
			WeaponSlot slot = WeaponSlots[ i ];
			if ( slot.IsUsed() && (int)slot.GetWeapon().Ammunition
				== (int)( (Godot.Collections.Dictionary)ammo.Data.Get( "properties" ) )[ "type" ] ) {
				slot.GetWeapon().SetReserve( stack );
				slot.GetWeapon().SetAmmo( ammo );
				if ( LastUsedArm.Slot == i ) {
					EmitSignalWeaponStatusUpdated( WeaponSlots[ i ].GetWeapon(), WeaponSlots[ i ].GetMode() );
				}
			}
		}
	}
	public override void PickupWeapon( WeaponEntity weapon ) {
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

		int hashCode = weapon.GetPath().GetHashCode();
		weapon.SetMeta( "hash", hashCode );
		WeaponsStack.Add( hashCode, weapon );
		TotalInventoryWeight += weapon.Weight;

		TorsoAnimation.FlipH = false;
		LegAnimation.FlipH = false;

		weapon.Connect( WeaponEntity.SignalName.ModeChanged, Callable.From<WeaponEntity, WeaponEntity.Properties>( OnWeaponModeChanged ) );

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

	public void DropWeapon( int hashCode ) {
		if ( !WeaponsStack.TryGetValue( hashCode, out WeaponEntity weapon ) ) {
			Console.PrintError( string.Format( "Player.DropWeapon: invalid hash id {0}", hashCode ) );
			return;
		}

		WeaponsStack.Remove( hashCode );
		weapon.Drop();

		for ( int i = 0; i < MAX_WEAPON_SLOTS; i++ ) {
			if ( WeaponSlots[ i ].GetWeapon() == weapon ) {
				if ( i == CurrentWeapon ) {
					CurrentWeapon = WeaponSlot.INVALID;
					EmitSignalWeaponStatusUpdated( null, WeaponEntity.Properties.None );

					if ( ArmLeft.Slot == i ) {
						ArmLeft.Slot = CurrentWeapon;
					}
					if ( ArmRight.Slot == i ) {
						ArmRight.Slot = CurrentWeapon;
					}
				}
				WeaponSlots[ i ].SetWeapon( null );
			}
		}
	}
	public void DropAmmo( int hashCode ) {
		if ( !AmmoStacks.TryGetValue( hashCode, out AmmoStack stack ) ) {
			Console.PrintError( string.Format( "Player.DropAmmo: invalid hash id {0}", hashCode ) );
			return;
		}

		AmmoStacks.Remove( hashCode );
	}
};