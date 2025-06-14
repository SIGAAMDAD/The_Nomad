using System;
using System.Reflection;
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

	public Multiplayer.PlayerData.MultiplayerMetadata MultiplayerData;

	private GroundMaterialType GroundType;
	
	private Resource Database;
	private Resource CurrentWeapon;
	private CSteamID OwnerId;
	private int NodeHash;

	private GpuParticles2D WalkEffect;
	private GpuParticles2D DashEffect;
	private GpuParticles2D SlideEffect;
	private PointLight2D DashLight;

	private AudioStreamPlayer2D MoveChannel;
	private AudioStreamPlayer2D DashChannel;

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

	private FootSteps FootSteps;

	private void PlayAnimation( AnimatedSprite2D animator, string animation ) {
		if ( animator.Animation == animation && animator.IsPlaying() ) {
			return;
		}
		animator.CallDeferred( "play", animation );
	}
	
	// TODO: find some way of sending values back to the client
	
	public void Update( System.IO.BinaryReader packet ) {
		if ( packet.ReadSByte() != WeaponSlot.INVALID ) {
			WeaponUseMode = (WeaponEntity.Properties)packet.ReadUInt32();
			if ( packet.ReadBoolean() ) {
				string weaponId = packet.ReadString();
				Console.PrintLine( "NetworkPlayer is using weapon " + weaponId );
				CurrentWeapon = (Resource)ResourceCache.ItemDatabase.Call( "get_item", weaponId );
			}
		} else {
			WeaponUseMode = WeaponEntity.Properties.None;
		}

		Godot.Vector2 position = Godot.Vector2.Zero;
		position.X = (float)packet.ReadDouble();
		position.Y = (float)packet.ReadDouble();
		GlobalPosition = position;

		LeftArmAnimation.SetDeferred( "global_rotation", (float)packet.ReadDouble() );
		SetArmAnimationState( LeftArmAnimation, (PlayerAnimationState)packet.ReadByte(), DefaultLeftArmSpriteFrames );

		RightArmAnimation.SetDeferred( "global_rotation", (float)packet.ReadDouble() );
		SetArmAnimationState( RightArmAnimation, (PlayerAnimationState)packet.ReadByte(), DefaultRightArmSpriteFrames );

		LegAnimationState = (PlayerAnimationState)packet.ReadByte();
		TorsoAnimationState = (PlayerAnimationState)packet.ReadByte();

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

			WalkEffect.SetDeferred( "emitting", false );
			SlideEffect.SetDeferred( "emitting", false );
			break;
		case PlayerAnimationState.Running:
			LegAnimation.CallDeferred( "show" );
			LegAnimation.CallDeferred( "play", "run" );

			WalkEffect.SetDeferred( "emitting", true );
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

		Player.Hands handsUsed = (Player.Hands)packet.ReadByte();

		bool flip = packet.ReadBoolean();

		TorsoAnimation.SetDeferred( "flip_h", flip );
		LegAnimation.SetDeferred( "flip_h", flip );
		LeftArmAnimation.SetDeferred( "flip_v", flip );
		RightArmAnimation.SetDeferred( "flip_v", flip );

		Player.PlayerFlags flags = (Player.PlayerFlags)packet.ReadUInt32();

		bool isDashing = ( flags & Player.PlayerFlags.Dashing ) != 0;
		DashEffect.SetDeferred( "emitting", isDashing );
		DashLight.SetDeferred( "visible", isDashing );

		SlideEffect.SetDeferred( "emitting", ( flags & Player.PlayerFlags.Sliding ) != 0 );
	}
	public override void Damage( in Entity source, float nAmount ) {
		SyncObject.Write( (byte)SteamLobby.MessageType.ClientData );
		SyncObject.Write( (byte)PlayerUpdateType.Damage );
		if ( source is Player player && player != null ) {
			SyncObject.Write( (byte)PlayerDamageSource.Player );
			SyncObject.Write( (ulong)player.MultiplayerData.Id );
		} else if ( source is Thinker thinker && thinker != null ) {
			SyncObject.Write( (byte)PlayerDamageSource.NPC );
			SyncObject.Write( thinker.GetHashCode() );
		}
		SyncObject.Write( nAmount );
		SyncObject.Sync( OwnerId );
	}
	private void OnLegAnimationLooped() {
		if ( LegAnimationState == PlayerAnimationState.Running ) {
			FootSteps.AddStep( Velocity, GlobalPosition, GroundType );
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

		WalkEffect = GetNode<GpuParticles2D>( "DustPuff" );
		SlideEffect = GetNode<GpuParticles2D>( "SlidePuff" );
		DashEffect = GetNode<GpuParticles2D>( "DashEffect" );
		DashLight = GetNode<PointLight2D>( "DashEffect/PointLight2D" );

		DashChannel = GetNode<AudioStreamPlayer2D>( "DashChannel" );
		DashChannel.VolumeDb = SettingsData.GetEffectsVolumeLinear();

		MoveChannel = GetNode<AudioStreamPlayer2D>( "MoveChannel" );
		MoveChannel.VolumeDb = SettingsData.GetEffectsVolumeLinear();

		IdleAnimation = GetNode<AnimatedSprite2D>( "Idle" );

		FootSteps = GetNode<FootSteps>( "FootSteps" );

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
			arm.SetDeferred( "sprite_frames", defaultFrames );
			arm.CallDeferred( "hide" );
			break;
		case PlayerAnimationState.Sliding:
		case PlayerAnimationState.Idle:
			arm.SetDeferred( "sprite_frames", defaultFrames );
			arm.CallDeferred( "play", "idle" );
			break;
		case PlayerAnimationState.Running:
			arm.SetDeferred( "sprite_frames", defaultFrames );
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
			
			arm.SetDeferred( "sprite_frames",
				(SpriteFrames)( (Godot.Collections.Dictionary)CurrentWeapon.Get( "properties" ) )[ property ] );
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
			
			arm.SetDeferred( "sprite_frames",
				(SpriteFrames)( (Godot.Collections.Dictionary)CurrentWeapon.Get( "properties" ) )[ property ] );
			arm.CallDeferred( "play", "use" );
			break; }
		case PlayerAnimationState.WeaponReload:
			arm.CallDeferred( "show" );
			arm.SetDeferred( "sprite_frames",
				(SpriteFrames)( (Godot.Collections.Dictionary)CurrentWeapon.Get( "properties" ) )[ "firearm_frames_left" ] );
			arm.CallDeferred( "play", "reload" );
			break;
		case PlayerAnimationState.WeaponEmpty:
			arm.CallDeferred( "show" );
			arm.SetDeferred( "sprite_frames",
				(SpriteFrames)( (Godot.Collections.Dictionary)CurrentWeapon.Get( "properties" ) )[ "firearm_frames_left" ] );
			arm.CallDeferred( "play", "empty" );
			break;
		};
	}
	public void SetOwnerId( ulong steamId ) {
		OwnerId = (CSteamID)steamId;
	}
};