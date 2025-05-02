using System;
using Godot;
using PlayerSystem;
using Renown;
using Renown.Thinkers;
using Steamworks;

public enum PlayerAnimationState : byte {
	Idle,
	Sliding,
	Running,
	Dead,
	Hide,
	WeaponIdle,
	WeaponUse,
	WeaponReload,
	WeaponEmpty,

	TrueIdleStart,
	TrueIdleLoop,

	CheckpointDrinking,
	CheckpointExit,
	CheckpointIdle,
	
	Count
};

/// <summary>
/// sent to a Player on another machine, most usually a damage sync
/// </summary>
public enum PlayerUpdateType : byte {
	Damage,
	SetSpawn,

	Count
};

public enum PlayerDamageSource : byte {
	NPC,
	Player,
	Environment,

	Count
};

public partial class NetworkPlayer : Renown.Entity {
	private NetworkWriter SyncObject = new NetworkWriter( 24 );
	private NetworkReader SyncReader = new NetworkReader();

	private Random RandomFactory = null;
	
	public string MultiplayerUsername;
	public CSteamID MultiplayerId;
	public ulong MultiplayerKills;
	public ulong MultiplayerDeaths;
	
	private Resource Database;
	private Resource CurrentWeapon;
	private CSteamID OwnerId;
	private int NodeHash;

	private AudioStreamPlayer2D MoveChannel;

	private WeaponEntity.Properties WeaponUseMode = WeaponEntity.Properties.None;
	
	private AnimatedSprite2D IdleAnimation;
	private AnimatedSprite2D TorsoAnimation;
	private AnimatedSprite2D LeftArmAnimation;
	private AnimatedSprite2D RightArmAnimation;
	private AnimatedSprite2D LegAnimation;

	private SpriteFrames DefaultLeftArmSpriteFrames;
	private SpriteFrames DefaultRightArmSpriteFrames;

	private PlayerAnimationState LegAnimationState;
	private PlayerAnimationState LeftArmAnimationState;
	private PlayerAnimationState RightArmAnimationState;
	private PlayerAnimationState TorsoAnimationState;

	private void PlayAnimation( AnimatedSprite2D animator, string animation ) {
		if ( animator.Animation == animation && animator.IsPlaying() ) {
			return;
		}
		animator.CallDeferred( "play", animation );
	}
	
	// TODO: find some way of sending values back to the client
	
	public void Update( System.IO.BinaryReader packet ) {
		SyncReader.BeginRead( packet );

		if ( SyncReader.ReadSByte() != WeaponSlot.INVALID ) {
			WeaponUseMode = (WeaponEntity.Properties)SyncReader.ReadUInt32();
			if ( packet.ReadBoolean() ) {
				CurrentWeapon = (Resource)ResourceCache.ItemDatabase.Call( "get_item", SyncReader.ReadString() );
			}
		} else {
			WeaponUseMode = WeaponEntity.Properties.None;
		}
		Velocity = SyncReader.ReadVector2();

		LeftArmAnimation.SetDeferred( "global_rotation", SyncReader.ReadFloat() );
		SetArmAnimationState( LeftArmAnimation, (PlayerAnimationState)SyncReader.ReadByte(), DefaultLeftArmSpriteFrames );

		RightArmAnimation.SetDeferred( "global_rotation", SyncReader.ReadFloat() );
		SetArmAnimationState( RightArmAnimation, (PlayerAnimationState)SyncReader.ReadByte(), DefaultRightArmSpriteFrames );

		LegAnimationState = (PlayerAnimationState)SyncReader.ReadByte();
		TorsoAnimationState = (PlayerAnimationState)SyncReader.ReadByte();

		switch ( LegAnimationState ) {
		case PlayerAnimationState.Hide:
		case PlayerAnimationState.TrueIdleStart:
		case PlayerAnimationState.TrueIdleLoop:
		case PlayerAnimationState.Dead:
		case PlayerAnimationState.CheckpointDrinking:
		case PlayerAnimationState.CheckpointExit:
		case PlayerAnimationState.CheckpointIdle:
			LegAnimation.CallDeferred( "hide" );
			break;
		case PlayerAnimationState.Idle:
			LegAnimation.CallDeferred( "show" );
			LegAnimation.CallDeferred( "play", "idle" );
			break;
		case PlayerAnimationState.Running:
			LegAnimation.CallDeferred( "show" );
			LegAnimation.CallDeferred( "play", "run" );
			break;
		case PlayerAnimationState.Sliding:
			LegAnimation.CallDeferred( "show" );
			LegAnimation.CallDeferred( "play", "slide" );
			break;
		};
		
		switch ( TorsoAnimationState ) {
		case PlayerAnimationState.CheckpointDrinking:
			TorsoAnimation.CallDeferred( "hide" );
			IdleAnimation.CallDeferred( "show" );
			IdleAnimation.CallDeferred( "play", "checkpoint_drink" );
			break;
		case PlayerAnimationState.CheckpointExit:
			TorsoAnimation.CallDeferred( "hide" );
			IdleAnimation.CallDeferred( "show" );
			IdleAnimation.CallDeferred( "play", "checkpoint_exit" );
			break;
		case PlayerAnimationState.CheckpointIdle:
			TorsoAnimation.CallDeferred( "hide" );
			IdleAnimation.CallDeferred( "show" );
			IdleAnimation.CallDeferred( "play", "checkpoint_idle" );
			break;
		case PlayerAnimationState.Idle:
		case PlayerAnimationState.Sliding:
		case PlayerAnimationState.Running:
			TorsoAnimation.CallDeferred( "show" );
			TorsoAnimation.CallDeferred( "play", "default" );
			break;
		case PlayerAnimationState.TrueIdleStart:
			TorsoAnimation.CallDeferred( "hide" );
			IdleAnimation.CallDeferred( "show" );
			IdleAnimation.CallDeferred( "play", "start" );
			break;
		case PlayerAnimationState.TrueIdleLoop:
			TorsoAnimation.CallDeferred( "hide" );
			IdleAnimation.CallDeferred( "show" );
			IdleAnimation.CallDeferred( "play", "loop" );
			break;
		case PlayerAnimationState.Dead:
			TorsoAnimation.CallDeferred( "show" );
			TorsoAnimation.CallDeferred( "play", "dead" );
			break;
		};
	}
	public override void Damage( Entity source, float nAmount ) {
		SyncObject.Write( (byte)SteamLobby.MessageType.ClientData );
		SyncObject.Write( (byte)PlayerUpdateType.Damage );
		if ( source is Player player && player != null ) {
			SyncObject.Write( (byte)PlayerDamageSource.Player );
			SyncObject.Write( (ulong)player.MultiplayerId );
		} else if ( source is Thinker thinker && thinker != null ) {
			SyncObject.Write( (byte)PlayerDamageSource.NPC );
			SyncObject.Write( thinker.GetHashCode() );
		}
		SyncObject.Write( nAmount );
		SyncObject.Sync( OwnerId );
	}
	private void OnLegAnimationLooped() {
		if ( LegAnimationState == PlayerAnimationState.Running ) {
			MoveChannel.Stream = ResourceCache.MoveGravelSfx[ RandomFactory.Next( 0, ResourceCache.MoveGravelSfx.Length - 1 ) ];
			MoveChannel.Play();
		}
	}
	
	public override void _Ready() {
		base._Ready();

		TorsoAnimation = GetNode<AnimatedSprite2D>( "Torso" );

		LeftArmAnimation = GetNode<AnimatedSprite2D>( "LeftArm" );
		DefaultLeftArmSpriteFrames = LeftArmAnimation.SpriteFrames;

		RightArmAnimation = GetNode<AnimatedSprite2D>( "RightArm" );
		DefaultRightArmSpriteFrames = RightArmAnimation.SpriteFrames;

		LegAnimation = GetNode<AnimatedSprite2D>( "Legs" );
		LegAnimation.Connect( "animation_looped", Callable.From( OnLegAnimationLooped ) );

		MoveChannel = GetNode<AudioStreamPlayer2D>( "MoveChannel" );
		MoveChannel.VolumeDb = SettingsData.GetEffectsVolumeLinear();
		MoveChannel.SetProcess( false );
		MoveChannel.SetProcessInternal( false );

		IdleAnimation = GetNode<AnimatedSprite2D>( "Idle" );

		SteamLobby.Instance.AddPlayer( OwnerId, new SteamLobby.NetworkNode( this, null, Update ) );
	}

	private void SetArmAnimationState( AnimatedSprite2D arm, PlayerAnimationState state, SpriteFrames defaultFrames ) {
		switch ( state ) {
		case PlayerAnimationState.Hide:
		case PlayerAnimationState.TrueIdleStart:
		case PlayerAnimationState.TrueIdleLoop:
		case PlayerAnimationState.Dead:
		case PlayerAnimationState.CheckpointDrinking:
		case PlayerAnimationState.CheckpointExit:
		case PlayerAnimationState.CheckpointIdle:
			arm.SpriteFrames = defaultFrames;
			arm.CallDeferred( "hide" );
			break;
		case PlayerAnimationState.Sliding:
		case PlayerAnimationState.Idle:
			arm.SpriteFrames = defaultFrames;
			arm.CallDeferred( "play", "idle" );
			break;
		case PlayerAnimationState.Running:
			arm.SpriteFrames = defaultFrames;
			arm.CallDeferred( "play", "run" );
			break;
		case PlayerAnimationState.WeaponIdle: {
			string property = "";
			if ( ( WeaponUseMode & WeaponEntity.Properties.IsFirearm ) != 0 ) {
				property = "firearm_frames_left";
			} else if ( ( WeaponUseMode & WeaponEntity.Properties.IsBlunt ) != 0 ) {
				property = "blunt_frames_left";
			} else if ( ( WeaponUseMode & WeaponEntity.Properties.IsBladed ) != 0 ) {
				property = "bladed_frames_left";
			}
			
			arm.SpriteFrames =
				(SpriteFrames)( (Godot.Collections.Dictionary)CurrentWeapon.Get( "properties" ) )[ property ];
			arm.CallDeferred( "play", "idle" );
			break; }
		case PlayerAnimationState.WeaponUse: {
			arm.CallDeferred( "show" );
			
			string property = "";
			if ( ( WeaponUseMode & WeaponEntity.Properties.IsFirearm ) != 0 ) {
				property = "firearm_frames_left";
			} else if ( ( WeaponUseMode & WeaponEntity.Properties.IsBlunt ) != 0 ) {
				property = "blunt_frames_left";
			} else if ( ( WeaponUseMode & WeaponEntity.Properties.IsBladed ) != 0 ) {
				property = "bladed_frames_left";
			}
			
			arm.SpriteFrames =
				(SpriteFrames)( (Godot.Collections.Dictionary)CurrentWeapon.Get( "properties" ) )[ property ];
			arm.CallDeferred( "play", "use" );
			break; }
		case PlayerAnimationState.WeaponReload:
			arm.CallDeferred( "show" );
			arm.SpriteFrames =
				(SpriteFrames)( (Godot.Collections.Dictionary)CurrentWeapon.Get( "properties" ) )[ "firearm_frames_left" ];
			arm.CallDeferred( "play", "reload" );
			break;
		case PlayerAnimationState.WeaponEmpty:
			arm.CallDeferred( "show" );
			arm.SpriteFrames =
				(SpriteFrames)( (Godot.Collections.Dictionary)CurrentWeapon.Get( "properties" ) )[ "firearm_frames_left" ];
			arm.CallDeferred( "play", "empty" );
			break;
		};
	}
	public override void _PhysicsProcess( double delta ) {
		base._PhysicsProcess( delta );
		
		if ( Velocity != Godot.Vector2.Zero ) {
			MoveAndSlide();
		}
	}

	public void SetOwnerId( ulong steamId ) {
		OwnerId = (CSteamID)steamId;
	}
};