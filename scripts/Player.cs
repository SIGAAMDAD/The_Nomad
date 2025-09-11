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
using System;
using PlayerSystem;
using System.Collections.Generic;
using Steamworks;
using System.Runtime.CompilerServices;
using Renown;
using Renown.World;
using System.Diagnostics;
using DialogueManagerRuntime;
using PlayerSystem.ArmAttachments;
using PlayerSystem.Upgrades;
using Interactables;
using System.Runtime.InteropServices;
using Steam;
using ResourceCache;
using PlayerSystem.Input;
using Menus;
using PlayerSystem.Inventory;
using PlayerSystem.Stats;

/*
===================================================================================

Player

===================================================================================
*/
/// <summary>
/// The code that handles player-to-game interactions
/// </summary>

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

	//
	// networking
	//
	[StructLayout( LayoutKind.Sequential )]
	private struct NetworkState {
		public Half LastNetworkAimAngle;
		public Half LastNetworkBloodAmount;
		public uint LastNetworkFlags;
		public uint LastNetworkUseMode;
		public string LastNetworkWeaponID;

		public NetworkState() {
			LastNetworkAimAngle = (Half)0.0f;
			LastNetworkBloodAmount = (Half)0.0f;
			LastNetworkFlags = 0;
			LastNetworkUseMode = (uint)WeaponEntity.Properties.None;
			LastNetworkWeaponID = "";
		}
	};

	public static readonly float PUNCH_RANGE = 40.0f;

	//
	// constants that can be modified by runes, perks, etc.
	//
	public static readonly float BASE_FAST_TRAVEL_RAGE_COST = 20.0f;
	public static readonly float BASE_FAST_TRAVEL_SANITY_COST = 0.0f;
	public static readonly float BASE_ENEMY_DETECTION_SPEED = 1.0f;

	private static readonly float DIRECTIONAL_INFLUENCE = 0.7f;

	public static Godot.Vector2I ScreenSize = Godot.Vector2I.Zero;

	[Export]
	public Node InventoryDatabase { get; private set; }
	[Export]
	public Arm ArmLeft { get; private set; }
	[Export]
	public Arm ArmRight { get; private set; }
	[Export]
	private CanvasLayer HUD;
	[Export]
	public ArmAttachment ArmAttachment { get; private set; }

	[ExportCategory( "Start" )]
	[Export]
	public PlayerFlags Flags { get; private set; } = 0;
	[Export]
	public Checkpoint StartingCheckpoint { get; private set; }

	[ExportCategory( "Camera Shake" )]
	[Export]
	private float RandomStrength = 36.0f;
	[Export]
	private float ShakeFade = 0.5f;

	/// <summary>
	/// The current TileMapFloor that the player occupies
	/// </summary>
	public TileMapFloor Floor { get; private set; }

	/// <summary>
	/// Handles torso animation
	/// </summary>
	public AnimatedSprite2D? TorsoAnimation { get; private set; }

	/// <summary>
	/// Handles leg animation
	/// </summary>
	public AnimatedSprite2D? LegAnimation { get; private set; }

	/// <summary>
	/// Handles all full body animations
	/// </summary>
	public AnimatedSprite2D? IdleAnimation { get; private set; }

	public GroundMaterialType GroundType { get; private set; }

	/// <summary>
	/// How much blood the player is covered in
	/// </summary>
	/// <seealso cref="BloodMaterial"/>
	public float BloodAmount { get; private set; }

	private static bool TutorialCompleted = false;

	private Texture2D? CurrentEmote;

	/// <summary>
	/// The camera marked as "Current" in Godot
	/// </summary>
	private Camera2D? Viewpoint;

	public AimAssist? AimAssist { get; private set; }

	/// <summary>
	/// Handles inventory data and management
	/// </summary>
	public InventoryManager? Inventory { get; private set; }

	private MovementHandler? Movement;

	public WeaponSlotManager? WeaponSlots { get; private set; }

	public StatManager? StatManager { get; private set; }

	/// <summary>
	/// Handles the jump kit and all functions related to the dash mechanic
	/// </summary>
	public DashKit DashKit { get; private set; }

	public void SetEquippedWeapon( WeaponSlotIndex weaponSlot ) => WeaponSlots.SetEquippedWeapon( weaponSlot );
	public WeaponSlot? GetHeavyPrimaryWeapon() => WeaponSlots.GetHeavyPrimaryWeapon();
	public WeaponSlot? GetPrimaryWeapon() => WeaponSlots.GetPrimaryWeapon();
	public WeaponSlot? GetHeavySidearmWeapon() => WeaponSlots.GetHeavySidearmWeapon();
	public WeaponSlot? GetSidearmWeapon() => WeaponSlots.GetSidearmWeapon();
	public void SetHeavyPrimaryWeapon( in WeaponEntity weapon ) => WeaponSlots.SetHeavyPrimaryWeapon( weapon );
	public void SetPrimaryWeapon( in WeaponEntity weapon ) => WeaponSlots.SetPrimaryWeapon( weapon );
	public void SetHeavySidearmWeapon( in WeaponEntity weapon ) => WeaponSlots.SetHeavySidearmWeapon( weapon );
	public void SetSidearmWeapon( in WeaponEntity weapon ) => WeaponSlots.SetSidearmWeapon( weapon );

	public void OnDash() => DashKit.OnDash();
	public void SetModule( IDashModule? module ) => DashKit.SetModule( module );

	public Resource? GetBindActionResource( InputController.ControlBind bind ) => InputManager.GetBindActionResource( bind );
	public void SetBindAction( InputController.ControlBind bind, GUIDEActionSignal signal, InputAction action ) => InputManager.SetBindAction( bind, signal, action );

	public void PickupAmmo( in ItemPickup ammo ) => Inventory.PickupAmmo( in ammo );
	public void PickupWeapon( in WeaponEntity weapon ) => Inventory.PickupWeapon( in weapon );
	public void DropAmmo( int hashCode ) => Inventory.DropAmmo( hashCode );
	public void DropWeapon( int weapon ) => Inventory.DropWeapon( weapon );

	public float GetHealth() => StatManager.GetHealth();
	public float GetRage() => StatManager.GetRage();
	public float GetSanity() => StatManager.GetSanity();
	public void SetHealth( float health ) => StatManager.SetHealth( health );
	public void SetRage( float rage ) => StatManager.SetRage( rage );
	public void SetSanity( float sanity ) => StatManager.SetSanity( sanity );
	public void SetStatMaxValue( string statName, float maxValue ) => StatManager.SetStatMaxValue( statName, maxValue );
	public void SetStatMinValue( string statName, float minValue ) => StatManager.SetStatMinValue( statName, minValue );
	public void SetStatValue( string statName, float newValue ) => StatManager.SetStatValue( statName, newValue );
	public float GetStatMaxValue( string statName ) => StatManager.GetStatMaxValue( statName );
	public float GetStatMinValue( string statName ) => StatManager.GetStatMinValue( statName );
	public float GetStatValue( string statName ) => StatManager.GetStatValue( statName );

	public Hands HandsUsed { get; private set; } = Hands.Right;
	public Arm LastUsedArm { get; private set; }

	public GpuParticles2D? WalkEffect { get; private set; }
	public GpuParticles2D? SlideEffect { get; private set; }
	public GpuParticles2D? DashEffect { get; private set; }
	public PointLight2D? DashLight { get; private set; }

	/// <summary>
	/// For sounds that are played by other classes related to player
	/// </summary>
	public AudioStreamPlayer2D MiscChannel { get; private set; }

	public int TileMapLevel { get; private set; }

	public HashSet<RenownValue> Relations { get; private set; }
	public HashSet<RenownValue> Debts { get; private set; }

	public InputController? InputManager { get; private set; }

	//
	// multiplayer data
	//
	public Multiplayer.PlayerData.MultiplayerMetadata MultiplayerData;

	private bool NetworkNeedsSync = false;

	/// <summary>
	/// For faster synchronization over the network of the left arm's animation state, directly influenced by <see cref="ArmLeft"/>
	/// </summary>
	private PlayerAnimationState LeftArmAnimationState;

	/// <summary>
	/// For faster synchronization over the network of the right arm's animation state, directly influenced by <see cref="ArmRight"/>
	/// </summary>
	private PlayerAnimationState RightArmAnimationState;

	/// <summary>
	/// For faster synchronization over the network of the torso's animation state, directly influenced by <see cref="TorsoAnimation"/>
	/// </summary>
	private PlayerAnimationState TorsoAnimationState;

	/// <summary>
	/// For faster synchronization over the network of the leg's animation state, directly influenced by <see cref="LegAnimation"/>
	/// </summary>
	private PlayerAnimationState LegAnimationState;

	/// <summary>
	/// Generic audio channel
	/// </summary>
	private AudioStreamPlayer2D AudioChannel;

	/// <summary>
	/// For jumpkit related sounds
	/// </summary>
	private AudioStreamPlayer2D DashChannel;

	/// <summary>
	/// For proximity chat
	/// </summary>
	private AudioStreamPlayer2D VoiceChannel;

	/// <summary>
	/// For footstep sound effects
	/// </summary>
	private AudioStreamPlayer2D WalkChannel;

	//
	// for the phantoms
	//
	private AudioStreamPlayer WhisperChannel;
	private Timer WhisperTimer;

	private FootSteps FootSteps;

	private Multiplayer.NetworkSyncObject SyncObject;

	private Godot.Collections.Dictionary<string, string> JournalCache;

	/// <summary>
	/// The currently discovered areas in the world
	/// </summary>
	private HashSet<WorldArea> DiscoveredAreas;

	private NetworkState LastSyncState;

	private Area2D ParryArea;
	private CollisionShape2D ParryBox;
	private Area2D ParryDamageArea;
	private CollisionShape2D ParryDamageBox;

	private Node? Animations;
	public SpriteFrames? DefaultLeftArmAnimations;
	private Sprite2D? HeadAnimation;

	private float WindUpDuration = 0.05f;
	private float WindUpProgress = 0.0f;
	private float IdleTime = 0.0f;
	private float NextShiftTime = 0.0f;

	private Node2D? Shadows;
	private AnimatedSprite2D? LeftArmShadowAnimation;
	private AnimatedSprite2D? RightArmShadowAnimation;
	private AnimatedSprite2D? TorsoShadowAnimation;
	private AnimatedSprite2D? LegShadowAnimation;

	private Timer? IdleTimer;
	private Timer? CheckpointDrinkTimer;
	private Timer? HealTimer;
	public Timer? SlideTime;

	/// <summary>
	/// The amount of time left before the blood starts fading off the player. Resets each time more blood is added.
	/// </summary>
	private Timer? BloodDropTimer;

	/// <summary>
	/// The shader for drawing blood splatter on the player model
	/// </summary>
	/// <remarks>
	/// The amount of red on the player's sprite is determined by <see cref="BloodAmount"/>
	/// </remarks>
	/// <seealso cref="BloodAmount"/>
	private ShaderMaterial? BloodMaterial;

	/// <summary>
	/// The amount of camera shake currently being applied to the <see cref="Viewpoint"/>
	/// </summary>
	/// <seealso cref="Viewpoint"/>
	private static float ShakeStrength = 0.0f;
	private static Godot.Vector2 JoltDirection = Godot.Vector2.Zero;

	public float ArmAngle { get; private set; } = 0.0f;
	private int InputDevice = 0;
	public float FrameDamage = 0.0f;
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

	private static Action<int> DialogueCallback;

	private Dictionary<string, object> AchievementData;

	private Resource CurrentQuest;

	private Node2D Waypoint;

	public BladeAttackType BladeAttackType;

	[Signal]
	public delegate void JoinFactionEventHandler( Faction faction, Entity entity );
	[Signal]
	public delegate void LeaveFactionEventHandler( Faction faction, Entity entity );
	[Signal]
	public delegate void FactionPromotionEventHandler( Faction faction, Entity entity );
	[Signal]
	public delegate void FactionDemotionEventHandler( Faction faction, Entity entity );
	[Signal]
	public delegate void GainMoneyEventHandler( Node entity, float amount );
	[Signal]
	public delegate void LoseMoneyEventHandler( Node entity, float amount );
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
	public delegate void DecreaseTraitScoreEventHandler( Node self, Trait trait, float score );
	[Signal]
	public delegate void IncreaseTraitScoreEventHandler( Node self, Trait trait, float score );
	[Signal]
	public delegate void MeetEntityEventHandler( Entity other, Node self );
	[Signal]
	public delegate void MeetFactionEventHandler( Faction faction, Node self );
	[Signal]
	public delegate void IncreaseRelationEventHandler( Node other, Node self, float amount );
	[Signal]
	public delegate void DecreaseRelationEventHandler( Node other, Node self, float amount );
	[Signal]
	public delegate void IncreaseRenownEventHandler( Node self, int amount );

	[Signal]
	public delegate void SwitchedWeaponEventHandler( WeaponEntity weapon );
	[Signal]
	public delegate void SwitchHandUsedEventHandler( Hands hands );
	[Signal]
	public delegate void UsedWeaponEventHandler( WeaponEntity source );
	[Signal]
	public delegate void WeaponPickedUpEventHandler( WeaponEntity source );
	[Signal]
	public delegate void AmmoPickedUpEventHandler( string ammoTypeID );
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
	public delegate void DashBurnoutChangedEventHandler( float dashBurnout );
	[Signal]
	public delegate void RageChangedEventHandler( float rage );
	[Signal]
	public delegate void HealthChangedEventHandler( float health );
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

	/*
	===============
	SetInputVelocity
	===============
	*/
	/// <summary>
	/// Sets the player's input velocity
	/// </summary>
	/// <param name="velocity">The new velocity</param>
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public void SetInputVelocity( Vector2 velocity ) {
		InputVelocity = velocity;
	}

	/*
	===============
	DecreaseFactionImportance
	===============
	*/
	/// <summary>
	/// Decreases the numerical value that the player has to a faction
	/// </summary>
	/// <param name="amount"></param>
	public void DecreaseFactionImportance( Faction faction, int amount ) {
		ArgumentNullException.ThrowIfNull( faction );
		ArgumentOutOfRangeException.ThrowIfLessThan( amount, 0 );
		if ( amount == 0 ) {
			Console.PrintWarning( "Player.DecreaseFactionImportance: amount is 0, redundant call" );
		}

		FactionImportance -= amount;
		EmitSignalFactionDemotion( Faction, this );
	}

	/*
	===============
	IncreaseFactionImportance
	===============
	*/
	public void IncreaseFactionImportance( Faction faction, int amount ) {
		ArgumentNullException.ThrowIfNull( faction );
		ArgumentOutOfRangeException.ThrowIfLessThan( amount, 0 );
		if ( amount == 0 ) {
			Console.PrintWarning( "Player.IncreaseFactionImportance: amount is 0, redundant call" );
		}
		FactionImportance += amount;
		EmitSignalFactionPromotion( Faction, this );
	}

	/*
	===============
	AddRenown
	===============
	*/
	public void AddRenown( int amount ) {
		// TODO: WorldArea based renown

		RenownScore += amount;
		EmitSignalIncreaseRenown( this, amount );
	}

	/*
	===============
	GetTraitScore
	===============
	*/
	public float GetTraitScore( Trait type ) {
		//TODO: make this location dependent
		return 0.0f;
	}

	/*
	===============
	ChangeTraitScore
	===============
	*/
	public void ChangeTraitScore( Trait type, float delta ) {
		if ( delta < 0.0f ) {
			EmitSignalDecreaseTraitScore( this, type, delta );
		} else {
			EmitSignalIncreaseTraitScore( this, type, delta );
		}
		Location?.ChangePlayerTraitScore( type, delta );
	}

	/*
	===============
	Meet
	===============
	*/
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

	/*
	===============
	RelationIncrease
	===============
	*/
	public virtual void RelationIncrease( Renown.Object other, float amount ) {
		if ( !Relations.TryGetValue( new RenownValue( other ), out RenownValue score ) ) {
			Meet( other );
		}
		score.Value += amount;
		EmitSignalIncreaseRelation( other as Node, this, amount );

		Console.PrintLine( string.Format( "Relation between {0} and {1} increased by {2}", this, other, amount ) );
	}

	/*
	===============
	RelationDecrease
	===============
	*/
	public virtual void RelationDecrease( Renown.Object other, float amount ) {
		if ( !Relations.TryGetValue( new RenownValue( other ), out RenownValue score ) ) {
			Meet( other );
		}
		score.Value -= amount;
		EmitSignalDecreaseRelation( other as Node, this, amount );

		Console.PrintLine( string.Format( "Relation between {0} and {1} decreased by {2}", this, other, amount ) );
	}

	/*
	===============
	OnFlameAreaBodyShape2DEntered
	===============
	*/
	private void OnFlameAreaBodyShape2DEntered( Rid bodyRid, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		if ( body is Entity entity && entity != null ) {
			entity.AddStatusEffect( "status_burning" );
		}
	}

	/*
	===============
	AddStatusEffect
	===============
	*/
	public override void AddStatusEffect( string effectName ) {
		base.AddStatusEffect( effectName );

		EmitSignalStatusEffectTriggered( effectName, StatusEffects[ effectName ] );
	}

	/*
	===============
	StartThoughtBubble
	===============
	*/
	public static void StartThoughtBubble( string text ) {
		Resource dialogue = DialogueManager.CreateResourceFromText( string.Format( "~ thought_bubble\n{0}", text ) );
		LevelData.Instance.ThisPlayer.Velocity = Godot.Vector2.Zero;
		DialogueManager.ShowDialogueBalloon( dialogue, "thought_bubble" );
	}

	/*
	===============
	StartDialogue
	===============
	*/
	public static void StartDialogue( Resource dialogueResource, string key, System.Action<int> callback ) {
		LevelData.Instance.ThisPlayer.Velocity = Godot.Vector2.Zero;
		DialogueManager.ShowDialogueBalloon( dialogueResource, key );
		DialogueCallback = callback;
	}

	/*
	===============
	ThoughtBubble
	===============
	*/
	public void ThoughtBubble( string text ) {
		StartThoughtBubble( text );
	}

	/*
	===============
	OnDialogueEnded
	===============
	*/
	private void OnDialogueEnded( Resource dialogueResource ) {
	}

	/*
	===============
	OnDialogueStarted
	===============
	*/
	private void OnDialogueStarted( Resource dialogueResource ) {
	}

	/*
	===============
	OnMutated
	===============
	*/
	private void OnMutated( Godot.Collections.Dictionary mutation ) {
		DialogueCallback?.DynamicInvoke( DialogueGlobals.Get().PlayerChoice );
	}

	/*
	===============
	SetTileMapFloorLevel
	===============
	*/
	public void SetTileMapFloorLevel( int level ) {
		TileMapLevel = level;
	}

	/*
	===============
	AddToJournal
	===============
	*/
	public void AddToJournal( Note note ) {
		JournalCache.TryAdd( TranslationServer.Translate( $"{note.TextId}_TITLE" ), TranslationServer.Translate( note.TextId ) );
	}

	/*
	===============
	SetSoundLevel
	===============
	*/
	public void SetSoundLevel( float soundLevel ) {
		if ( soundLevel > SoundLevel ) {
			SoundLevel = soundLevel;
		}
	}

	/*
	===============
	Save
	===============
	*/
	/// <summary>
	/// Writes the player's gamestate to disk
	/// </summary>
	public override void Save() {
		using var writer = new SaveSystem.SaveSectionWriter( "Player", ArchiveSystem.SaveWriter );

		writer.SaveInt( "Hellbreaks", Hellbreaks );
		writer.SaveUInt( "HandsUsed", (uint)HandsUsed );

		writer.SaveVector2( "Position", GlobalPosition );

		writer.SaveInt( "ArmLeftSlot", (int)ArmLeft.Slot );
		writer.SaveInt( "ArmRightSlot", (int)ArmRight.Slot );

		StatManager.Save( writer );
		Inventory.Save( writer );
		WeaponSlots.Save( writer );
	}

	/*
	===============
	Load
	===============
	*/
	/// <summary>
	/// Loads the player's gamestate from disk
	/// </summary>
	public override void Load() {
		using SaveSystem.SaveSectionReader reader = ArchiveSystem.GetSection( "Player" );

		Hellbreaks = reader.LoadInt( "Hellbreaks" );
		HandsUsed = (Hands)reader.LoadUInt( "HandsUsed" );

		GlobalPosition = reader.LoadVector2( "Position" );

		ArmLeft.SetSlot( (WeaponSlotIndex)reader.LoadInt( "ArmLeftSlot" ) );
		ArmRight.SetSlot( (WeaponSlotIndex)reader.LoadInt( "ArmRightSlot" ) );
		switch ( HandsUsed ) {
			case Hands.Left:
				LastUsedArm = ArmLeft;
				break;
			case Hands.Right:
			case Hands.Both:
				LastUsedArm = ArmRight;
				break;
		}

		StatManager.Load( reader );
		Inventory.Load( reader, this );
		WeaponSlots.Load( reader );
	}

	/*
	===============
	SetTileMapFloor
	===============
	*/
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public void SetTileMapFloor( TileMapFloor floor ) {
		Floor = floor;
	}

	/*
	===============
	ReceivePacket
	===============
	*/
	private void ReceivePacket( ulong senderId, System.IO.BinaryReader reader ) {
		SyncObject.BeginRead( reader );

		PlayerUpdateType type = (PlayerUpdateType)SyncObject.ReadByte();
		switch ( type ) {
			case PlayerUpdateType.Damage: {
					switch ( (PlayerDamageSource)SyncObject.ReadByte() ) {
						case PlayerDamageSource.Player:
							float damage = SyncObject.ReadFloat();
							Damage( Steam.SteamLobby.Instance.GetPlayer( (CSteamID)senderId ), damage );
							break;
						case PlayerDamageSource.NPC:
							break;
						case PlayerDamageSource.Environment:
							break;
					}
					break;
				}
			case PlayerUpdateType.SetSpawn:
				// only ever called in team modes
				GlobalPosition = SyncObject.ReadVector2();
				break;
			case PlayerUpdateType.Count:
			default:
				Console.PrintError( string.Format( "Player.ReceivePacket: invalid PlayerUpdateType {0}", (byte)type ) );
				break;
		}
	}

	/*
	===============
	SendPacket
	===============
	*/
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

		if ( LastSyncState.LastNetworkAimAngle != (Half)AimAssist.GlobalRotation ) {
			SyncObject.Write( true );
			LastSyncState.LastNetworkAimAngle = (Half)AimAssist.GlobalRotation;
			//			SyncObject.Write( LastSyncState.LastNetworkAimAngle );
		} else {
			SyncObject.Write( false );
		}

		if ( LastSyncState.LastNetworkBloodAmount != (Half)BloodAmount ) {
			SyncObject.Write( true );
			SyncObject.Write( BloodAmount );
			LastSyncState.LastNetworkBloodAmount = (Half)BloodAmount;
		} else {
			SyncObject.Write( false );
		}

		/*
		if ( Inventory.CurrentWeapon == WeaponSlot.INVALID ) {
			SyncObject.Write( false );
			SyncObject.Write( false );
		} else {
			WeaponEntity weapon = Inventory.WeaponSlots[ Inventory.CurrentWeapon ].Weapon;
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
		*/

		SyncObject.Write( (byte)LeftArmAnimationState );
		SyncObject.Write( (byte)RightArmAnimationState );
		SyncObject.Write( (byte)LegAnimationState );
		SyncObject.Write( (byte)TorsoAnimationState );

		SyncObject.Sync( Steamworks.Constants.k_nSteamNetworkingSend_Unreliable );
	}

	/*
	===============
	OnSoundAreaShape2DEntered
	===============
	*/
	private void OnSoundAreaShape2DEntered( Rid bodyRid, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		if ( body is Renown.Thinkers.MobBase mob && mob != null ) {
			if ( mob.TileFloor == Floor ) {
				mob.Alert( this );
			}
		}
	}

	/*
	===============
	OnSoundAreaShape2DExited
	===============
	*/
	private void OnSoundAreaShape2DExited( Rid bodyRid, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
	}

	/*
	===============
	IncreaseBlood
	===============
	*/
	public void IncreaseBlood( float amount ) {
		if ( !SettingsData.ShowBlood ) {
			return;
		}

		// cover us with more blood if we're sober
		if ( ( Flags & PlayerFlags.Sober ) == 0 ) {
			BloodAmount += amount * 0.25f;
			BloodMaterial.SetShaderParameter( "blood_coef", BloodAmount );
			BloodDropTimer.Start();
		} else {
			BloodAmount += amount;
			BloodMaterial.SetShaderParameter( "blood_coef", BloodAmount );
			BloodDropTimer.Start();
		}
	}

	/*
	===============
	OnBloodDropTimerTimeout
	===============
	*/
	private void OnBloodDropTimerTimeout() {
		if ( !SettingsData.ShowBlood ) {
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

	/*
	===============
	SetLocation
	===============
	*/
	public override void SetLocation( in WorldArea location ) {
		if ( location != Location ) {
			DiscoveredAreas.Add( location );
			EmitSignalLocationChanged( location );
		}

		base.SetLocation( location );
	}

	/*
	===============
	GetWeaponHand
	===============
	*/
	public Arm GetWeaponHand( WeaponEntity weapon ) {
		/*
		if ( ArmLeft.Slot != WeaponSlot.INVALID && Inventory.WeaponSlots[ ArmLeft.Slot ].Weapon == weapon ) {
			return ArmLeft;
		} else if ( ArmRight.Slot != WeaponSlot.INVALID && Inventory.WeaponSlots[ ArmRight.Slot ].Weapon == weapon ) {
			return ArmRight;
		}
		*/
		return null;
	}

	/*
	===============
	SyncShadow
	===============
	*/
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
		if ( (int)SettingsData.WindowMode >= 2 ) {
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
	public void SetArmAngle( float angle ) => ArmAngle = angle;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public void SetLastUsedArm( in Arm arm ) => LastUsedArm = arm;

	/*
	===============
	SetFlags
	===============
	*/
	/// <summary>
	/// 
	/// </summary>
	/// <param name="flags"></param>
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public void SetFlags( PlayerFlags flags ) {
		Flags = flags;
	}

	/*
	===============
	AddFlags
	===============
	*/
	/// <summary>
	/// 
	/// </summary>
	/// <param name="flags"></param>
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public void AddFlags( PlayerFlags flags ) {
		Flags |= flags;
	}

	/*
	===============
	RemoveFlags
	===============
	*/
	/// <summary>
	/// 
	/// </summary>
	/// <param name="flags"></param>
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public void RemoveFlags( PlayerFlags flags ) {
		Flags &= ~flags;
	}

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public void SetHandsUsed( Hands hands ) => HandsUsed = hands;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public void SetGroundMaterial( GroundMaterialType type ) => GroundType = type;

	public static void ShakeCamera( float amount ) {
		ShakeStrength += amount;
		JoltDirection = Godot.Vector2.Zero;
	}
	public static void ShakeCameraDirectional( float amount, Godot.Vector2 direction ) {
		JoltDirection = direction;
		ShakeStrength = amount;
	}

	/*
	===============
	IdleReset
	===============
	*/
	/// <summary>
	/// <para>Resets the idle animation timer, hides the idle animation, and reverts back to the default animations.</para>
	/// <para>Only called when exiting the idle animation or to ensure the player sprite isn't in the idle animation
	/// when someone else is happening.</para>
	/// </summary>
	public void IdleReset() {
		IdleTimer.Start();
		IdleAnimation.Hide();
		IdleAnimation.Stop();

		AimAssist.Show();

		HeadAnimation.Show();
		LegAnimation.Show();
		TorsoAnimation.Show();
		ArmRight.Animations.Show();
		ArmLeft.Animations.Show();
	}

	/*
	===============
	PlaySound
	===============
	*/
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

	/*
	===============
	SetupSplitScreen
	===============
	*/
	public void SetupSplitScreen( int inputIndex ) {
		if ( Input.GetConnectedJoypads().Count > 0 ) {
			SplitScreen = true;
		}
		InputDevice = inputIndex;
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

		TorsoAnimation.Play( "default" );
		LegAnimation.Play( "idle" );
		ArmLeft.Animations.Play( "idle" );
		ArmRight.Animations.Play( "idle" );

		TorsoAnimation.Show();
		LegAnimation.Show();
		ArmLeft.Animations.Show();
		ArmRight.Animations.Show();

		if ( ( GameConfiguration.GameMode == GameMode.Online && SteamLobby.Instance.IsHost ) || GameConfiguration.GameMode == GameMode.SinglePlayer ) {
			ArchiveSystem.SaveGame( SettingsData.LastSaveSlot );
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

		ArmLeft.SetSlot( WeaponSlotIndex.Invalid );
		ArmRight.SetSlot( WeaponSlotIndex.Invalid );
		HandsUsed = Hands.Right;

		WeaponSlots.SetEquippedWeapon( WeaponSlotIndex.Invalid );

		BloodAmount = 0.0f;
		BloodMaterial.SetShaderParameter( "blood_coef", BloodAmount );

		BlockInput( false );
	}
	private void OnDeath( Entity attacker ) {
		EmitSignalDie( attacker, this );

		//		PlaySound( AudioChannel, DieSfx[ RNJesus.IntRange( 0, ResourceCache.PlayerDieSfx.Length - 1 ) ] );

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

	public override void Damage( in Entity attacker, float amount ) {
		if ( ( Flags & PlayerFlags.Dashing ) != 0 ) {
			return; // iframes
		}
		if ( attacker != null ) {
			ShakeCameraDirectional( amount, ( attacker.GlobalPosition - GlobalPosition ).Normalized() );
		}

		FreeFlow.EndCombo();

		StatManager.SetHealth( StatManager.GetStatValue( "Health" ) - amount * 0.75f );
		StatManager.SetRage( StatManager.GetStatValue( "Rage" ) - amount * 0.01f );

		if ( attacker != null ) {
			BloodParticleFactory.Create( attacker.GlobalPosition, GlobalPosition );
		}

		if ( Health <= 0.0f ) {
			CallDeferred( MethodName.OnDeath, attacker );
		} else {
			CallDeferred( MethodName.PlaySound, AudioChannel, SoundCache.GetEffectRange( SoundEffect.PlayerPain0, 3 ) );
		}

		CallDeferred( MethodName.EmitSignal, SignalName.Damaged, attacker, this, amount );
	}

	/*
	===============
	ActivateParry
	===============
	*/
	public void ActivateParry() {
		ParryArea.SetDeferred( Area2D.PropertyName.Monitoring, true );
		ParryDamageArea.SetDeferred( Area2D.PropertyName.Monitoring, true );

		Flags |= PlayerFlags.LightParrying | PlayerFlags.HeavyParrying;

		if ( Velocity == Godot.Vector2.Zero ) {
			// if we're stationary, make the player commit to a heavier parry
			BlockInput( true );
			ArmLeft.Animations.CallDeferred( AnimatedSprite2D.MethodName.Play, "melee_heavy" );
		} else {
			ArmLeft.Animations.CallDeferred( AnimatedSprite2D.MethodName.Play, "melee_light" );
		}
	}

	/*
	===============
	DeactivateParry
	===============
	*/
	public void DeactivateParry() {
	}

	/*
	public void OnParry( Bullet from, float damage ) {
		float distance = from.GlobalPosition.DistanceTo( from.GlobalPosition );

		// we punch the bullet or object so hard it creates a shrapnel cloud
		RayIntersectionInfo collider = GodotServerManager.CheckRayCast( GlobalPosition, ArmAngle, distance, GetRid() );
		if ( collider.Collider is Entity entity && entity != null ) {
			entity.Damage( this, damage );
			entity.AddStatusEffect( "status_burning" );
		}
	}
	*/
	public void OnParry( Vector2 from, Vector2 to, float damage ) {
		//		float distance = from.GlobalPosition.DistanceTo( from.TargetPosition );

		if ( ( Flags & PlayerFlags.LightParrying ) == 0 && ( Flags & PlayerFlags.HeavyParrying ) == 0 ) {
			return;
		}

		if ( Velocity == Godot.Vector2.Zero ) {
			PlaySound( MiscChannel, SoundCache.StreamCache[ SoundEffect.ParryHeavy ] );
		} else {
			PlaySound( MiscChannel, SoundCache.StreamCache[ SoundEffect.ParryLight ] );
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

		StatManager.SetRage( StatManager.GetStatValue( "Rage" ) - 35.0f );

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

		AimAssist.Hide();

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
		AimAssist.Show();

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

		AimAssist.Hide();

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

		switch ( item.InteractionType ) {
			case InteractionType.Checkpoint:
				Flags |= PlayerFlags.Checkpoint;
				LastCheckpoint = item as Checkpoint;
				break;
		}
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

		AimAssist.Hide();

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
			PlaySound( MiscChannel, SoundCache.StreamCache[ SoundEffect.PlayerArmFoley ] );
			SetSoundLevel( 36.0f );
		}
	}

	/*
	===============
	OnRespawnTransitionFinished
	===============
	*/
	private void OnRespawnTransitionFinished() {
		GameEventBus.ConnectSignal( GetNode( "/root/TransitionScreen" ), "transition_finished", this, OnRespawnTransitionFinished );

		GlobalPosition = LastCheckpoint.GlobalPosition;
		BeginInteraction( LastCheckpoint );
	}

	/*
	===============
	_UnhandledInput
	===============
	*/
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
		}
		if ( Health <= 0.0f ) {
			GameEventBus.DisconnectAllForObject( GetNode( "/root/TransitionScreen" ) );
			GetNode( "/root/TransitionScreen" ).Call( "transition" );

			SetProcess( true );
			SetPhysicsProcess( true );
			SetProcessUnhandledInput( false );

			TorsoAnimation.Play( "default" );
			LegAnimation.Visible = true;
			ArmLeft.Animations.Visible = true;
			ArmRight.Animations.Visible = true;
		}
	}

	/*
	===============
	FlipSpriteLeft
	===============
	*/
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	private void FlipSpriteLeft() {
		HeadAnimation.FlipV = true;
		LegAnimation.FlipH = true;
		TorsoAnimation.FlipH = true;
		ArmLeft.Flip = true;
		ArmRight.Flip = true;
	}

	/*
	===============
	FlipSpriteRight
	===============
	*/
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	private void FlipSpriteRight() {
		HeadAnimation.FlipV = false;
		LegAnimation.FlipH = false;
		TorsoAnimation.FlipH = false;
		ArmLeft.Flip = false;
		ArmRight.Flip = false;
	}

	/*
	===============
	OnSlowMoSfxFinished
	===============
	*/
	private void OnSlowMoSfxFinished() {
		/*
		if ( MiscChannel.Stream == ResourceCache.SlowMoBeginSfx ) {
			// only start lagging audio playback after the slowmo begin finishes
			AudioServer.PlaybackSpeedScale = 0.50f;
		}
		*/
	}

	public void ExitBulletTime() {
		if ( ( Flags & PlayerFlags.BulletTime ) == 0 ) {
			return;
		}
		Flags &= ~PlayerFlags.BulletTime;
		Engine.TimeScale = 1.0f;
		AudioServer.PlaybackSpeedScale = 1.0f;
		PlaySound( MiscChannel, SoundCache.StreamCache[ SoundEffect.SlowmoEnd ] );

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

		InputManager = new InputController( this );
		Movement = new MovementHandler( this as IPlayerInput, this );
		Inventory = new InventoryManager( this );

		DialogueManager.DialogueStarted += OnDialogueStarted;
		DialogueManager.DialogueEnded += OnDialogueEnded;
		DialogueManager.Mutated += OnMutated;

		ScreenSize = DisplayServer.WindowGetSize();
		if ( GameConfiguration.GameMode == GameMode.Multiplayer ) {
			MultiplayerData = new Multiplayer.PlayerData.MultiplayerMetadata( SteamManager.GetSteamID() );
			SyncObject = new Multiplayer.NetworkSyncObject( 1024 );
			LastSyncState = new NetworkState();

			SteamLobby.Instance.AddPlayer( SteamUser.GetSteamID(),
				new SteamLobby.PlayerNetworkNode( this, SendPacket, ReceivePacket ) );
		} else {
			DiscoveredAreas = new HashSet<WorldArea>();
			JournalCache = new Godot.Collections.Dictionary<string, string>();
			WhisperChannel = new AudioStreamPlayer();
		}

		GameEventBus.Subscribe<LevelData.PlayerRespawnEventHandler>( this, Respawn );

		// don't allow keybind input when we're in the console
		Console.Control.VisibilityChanged += () => {
			BlockInput( Console.Control.Visible );
		};

		StartingPosition = GlobalPosition;

		Input.SetCustomMouseCursor( TextureCache.GetTexture( "res://textures/hud/crosshairs/crosshairi.tga" ), Input.CursorShape.Arrow );

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

		Viewpoint = GetNode<Camera2D>( "Camera2D" );

		AudioChannel = GetNode<AudioStreamPlayer2D>( "AudioChannel" );
		AudioChannel.VolumeDb = SettingsData.GetEffectsVolumeLinear();

		WalkChannel = new AudioStreamPlayer2D();
		WalkChannel.Name = "WalkChannel";
		WalkChannel.VolumeDb = SettingsData.GetEffectsVolumeLinear();
		AddChild( WalkChannel );

		MiscChannel = GetNode<AudioStreamPlayer2D>( "MiscChannel" );
		GameEventBus.ConnectSignal( MiscChannel, AudioStreamPlayer2D.SignalName.Finished, this, OnSlowMoSfxFinished );
		MiscChannel.VolumeDb = SettingsData.GetEffectsVolumeLinear();

		CollisionShape2D SoundBounds = GetNode<CollisionShape2D>( "SoundArea/CollisionShape2D" );
		SoundArea = SoundBounds.Shape as CircleShape2D;

		ParryArea = GetNode<Area2D>( "AimAssist/AimLine/ParryArea" );
		ParryArea.SetMeta( "Owner", this );

		ParryBox = ParryArea.GetNode<CollisionShape2D>( "CollisionShape2D" );

		ParryDamageArea = GetNode<Area2D>( "AimAssist/AimLine/ParryDamageArea" );
		ParryDamageBox = GetNode<CollisionShape2D>( "AimAssist/AimLine/ParryDamageArea/CollisionShape2D" );

		Area2D area = GetNode<Area2D>( "SoundArea" );
		GameEventBus.ConnectSignal( area, Area2D.SignalName.BodyShapeEntered, this, Callable.From<Rid, Node2D, int, int>( OnSoundAreaShape2DEntered ) );

		FootSteps = GetNode<FootSteps>( "FootSteps" );

		DefaultLeftArmAnimations = ArmLeft.Animations.SpriteFrames;

		HeadAnimation = GetNode<Sprite2D>( "Animations/Head" );

		IdleTimer = GetNode<Timer>( "IdleAnimationTimer" );
		IdleTimer.Connect( Timer.SignalName.Timeout, Callable.From( OnIdleAnimationTimerTimeout ) );

		IdleAnimation = GetNode<AnimatedSprite2D>( "Animations/Idle" );
		GameEventBus.ConnectSignal( IdleAnimation, AnimatedSprite2D.SignalName.AnimationFinished, this, OnIdleAnimationAnimationFinished );

		LegAnimation = GetNode<AnimatedSprite2D>( "Animations/Legs" );
		GameEventBus.ConnectSignal( LegAnimation, AnimatedSprite2D.SignalName.AnimationLooped, this, OnLegsAnimationLooped );

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

		//		SlideTime = GetNode<Timer>( "Timers/SlideTime" );
		//		GameEventBus.ConnectSignal( SlideTime, Timer.SignalName.Timeout, this, OnSlideTimeout );

		CheckpointDrinkTimer = new Timer();
		CheckpointDrinkTimer.Name = "CheckpointDrinkTimer";
		CheckpointDrinkTimer.WaitTime = 10.5f;
		CheckpointDrinkTimer.ProcessMode = ProcessModeEnum.Disabled;
		GameEventBus.ConnectSignal( CheckpointDrinkTimer, Timer.SignalName.Timeout, this,
			() => {
				IdleAnimation.CallDeferred( AnimatedSprite2D.MethodName.Play, "checkpoint_drink" );
				TorsoAnimationState = PlayerAnimationState.CheckpointDrinking;
				LegAnimationState = PlayerAnimationState.CheckpointDrinking;
				LeftArmAnimationState = PlayerAnimationState.CheckpointDrinking;
				RightArmAnimationState = PlayerAnimationState.CheckpointDrinking;
			}
		);
		AddChild( CheckpointDrinkTimer );

		LastUsedArm = ArmRight;

		GetTree().Root.SizeChanged += OnScreenSizeChanged;
		GameEventBus.ConnectSignal( GetTree().Root, Viewport.SignalName.SizeChanged, this, OnScreenSizeChanged );

		SteamAchievements.SteamAchievement achievement;
		if ( SteamAchievements.AchievementTable.TryGetValue( "ACH_BUILDING_THE_LEGEND", out achievement ) && !achievement.Achieved ) {
			IncreaseRenown += ( Node self, int amount ) => {
				if ( amount >= 200 ) {
					SteamAchievements.ActivateAchievement( "ACH_BUIDLING_THE_LEGEND" );
				}
			};
		}
		if ( SteamAchievements.AchievementTable.TryGetValue( "ACH_RIGHT_BACK_AT_U", out achievement ) && !achievement.Achieved ) {
			ParrySuccess += () => SteamAchievements.ActivateAchievement( "ACH_RIGHT_BACK_AT_U" );
		}
		if ( SteamAchievements.AchievementTable.TryGetValue( "ACH_MASTER_OF_THE_WASTES", out achievement ) && !achievement.Achieved ) {
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

		if ( SettingsData.ShowBlood ) {
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

		if ( ArchiveSystem.IsLoaded() ) {
			Load();
		}
	}
	public override void PhysicsUpdate( double delta ) {
		base._PhysicsProcess( delta );

		Player.ShakeCameraDirectional( 2.5f, new Vector2( RNJesus.FloatRange( -30.0f, 30.0f ), RNJesus.FloatRange( -30.0f, 30.0f ) ) );

		CheckStatus( (float)delta );

		if ( ( Flags & PlayerFlags.BlockedInput ) != 0 ) {
			return;
		}
	}

	[MethodImpl( MethodImplOptions.AggressiveOptimization )]
	private void CalcArmAnimation( Arm arm, out PlayerAnimationState animState ) {
		arm.Animations.GlobalRotation = ArmAngle;
		arm.Animations.FlipV = TorsoAnimation.FlipH;
		arm.Animations.SpriteFrames = arm.GetAnimationSet();

		if ( arm.Slot == WeaponSlotIndex.Invalid ) {
			if ( SettingsData.AnimationQuality > AnimationQuality.Low ) {
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
			WeaponEntity weapon = WeaponSlots[ arm.Slot ].Weapon;
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
			}

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
			//Rage += FrameDamage * FreeFlow.Instance.ComboCounter * delta;
			FrameDamage = 0.0f;
			Flags |= PlayerFlags.UsedMana;
			changed = true;
		}
		FrameDamage = 0.0f;
		if ( StatManager.GetStatValue( "Health" ) < 100.0f && StatManager.GetStatValue( "Rage" ) > 0.0f ) {
			Health += 3.0f * delta;
			//Rage -= 10.0f * delta;
			changed = true;
			// mana conversion ratio to health is extremely inefficient

			Flags |= PlayerFlags.UsedMana;
			EmitSignalHealthChanged( Health );
		}
		if ( ( Flags & PlayerFlags.BulletTime ) != 0 ) {
			//			Rage -= 20.0f * delta;
			changed = true;
		}
		/*
		if ( Rage > 100.0f ) {
			Rage = 100.0f;
		} else if ( Rage < 0.0f ) {
			Rage = 0.0f;
			ExitBulletTime();
		}
		if ( changed ) {
			EmitSignalRageChanged( Rage );
		}
		*/
	}
	[MethodImpl( MethodImplOptions.AggressiveOptimization )]
	public override void Update( double delta ) {
		base._Process( delta );

		if ( InputVelocity != Godot.Vector2.Zero ) {
			if ( ( Flags & PlayerFlags.Sliding ) == 0 && ( Flags & PlayerFlags.OnHorse ) == 0 ) {
				if ( SettingsData.AnimationQuality > AnimationQuality.Low ) {
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
							PlaySound( WalkChannel, SoundCache.StreamCache[ SoundEffect.Slide0 ] );
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
				Godot.Vector2 baseOffset = JoltDirection.Normalized() * ShakeStrength * DIRECTIONAL_INFLUENCE;

				/*
				Godot.Vector2 randomOffset = new Godot.Vector2(
					RNJesus.FloatRange( -ShakeStrength, ShakeStrength ) * ( 1.0f - DIRECTIONAL_INFLUENCE ),
					RNJesus.FloatRange( -ShakeStrength, ShakeStrength ) * ( 1.0f - DIRECTIONAL_INFLUENCE )
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
};