using Godot;
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
	private NetworkSyncObject SyncObject = new NetworkSyncObject( 128 );

	public Multiplayer.PlayerData.MultiplayerMetadata MultiplayerData;

	private GroundMaterialType GroundType;

	private Godot.Vector2 NetworkPosition;
	private float CurrentSpeed = 0.0f;

	private ShaderMaterial BloodShader;

	private Resource CurrentWeapon;
	private CSteamID OwnerId;
	private int NodeHash;

	private GpuParticles2D WalkEffect;
	private GpuParticles2D DashEffect;
	private GpuParticles2D SlideEffect;
	private PointLight2D DashLight;

	private AudioStreamPlayer2D AudioChannel;
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

	private bool LastFlipState;

	private FootSteps FootSteps;

	private void PlayAnimation( AnimatedSprite2D animator, string animation ) {
		if ( animator.Animation == animation && animator.IsPlaying() ) {
			return;
		}
		animator.CallDeferred( "play", animation );
	}

	// TODO: find some way of sending values back to the client

	public override void PlaySound( AudioStreamPlayer2D channel, AudioStream stream ) {
		base.PlaySound( channel == null ? AudioChannel : channel, stream );
	}

	public void Update( System.IO.BinaryReader packet ) {
		SyncObject.BeginRead( packet );

		bool flip = SyncObject.ReadBoolean();
		if ( flip != LastFlipState ) {
			TorsoAnimation.SetDeferred( "flip_h", flip );
			LegAnimation.SetDeferred( "flip_h", flip );
			LeftArmAnimation.SetDeferred( "flip_v", flip );
			RightArmAnimation.SetDeferred( "flip_v", flip );
			LastFlipState = flip;
		}

		NetworkPosition = SyncObject.ReadVector2();
		CurrentSpeed = Player.MAX_SPEED;

		if ( SyncObject.ReadBoolean() ) {
			Player.PlayerFlags flags = (Player.PlayerFlags)SyncObject.ReadUInt32();

			bool isDashing = ( flags & Player.PlayerFlags.Dashing ) != 0;
			if ( isDashing && !DashChannel.Playing ) {
				CurrentSpeed += 1000.0f;
				PlaySound( DashChannel, ResourceCache.DashSfx[ RNJesus.IntRange( 0, ResourceCache.DashSfx.Length - 1 ) ] );
			}
			DashEffect.SetDeferred( "emitting", isDashing );
			DashLight.SetDeferred( "visible", isDashing );

			bool isSliding = ( flags & Player.PlayerFlags.Sliding ) != 0;
			SlideEffect.SetDeferred( "emitting", isSliding );
			if ( isSliding ) {
				CurrentSpeed += 400;
			}
		}

		if ( SyncObject.ReadBoolean() ) {
			float angle = SyncObject.ReadFloat();
			LeftArmAnimation.SetDeferred( "global_rotation", angle );
			RightArmAnimation.SetDeferred( "global_rotation", angle );
		}
		if ( SyncObject.ReadBoolean() ) {
			float bloodAmount = SyncObject.ReadFloat();
		}
		if ( SyncObject.ReadBoolean() ) {
			string weaponId = SyncObject.ReadString();
			CurrentWeapon = (Resource)ResourceCache.ItemDatabase.Call( "get_item", weaponId );
		}
		if ( SyncObject.ReadBoolean() ) {
			WeaponUseMode = (WeaponEntity.Properties)SyncObject.ReadUInt32();
		}

		LeftArmAnimationState = (PlayerAnimationState)SyncObject.ReadByte();
		SetArmAnimationState( LeftArmAnimation, LeftArmAnimationState, DefaultLeftArmSpriteFrames );

		RightArmAnimationState = (PlayerAnimationState)SyncObject.ReadByte();
		SetArmAnimationState( RightArmAnimation, RightArmAnimationState, DefaultRightArmSpriteFrames );

		LegAnimationState = (PlayerAnimationState)SyncObject.ReadByte();
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

		TorsoAnimationState = (PlayerAnimationState)SyncObject.ReadByte();
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

			LeftArmAnimation.CallDeferred( "hide" );
			RightArmAnimation.CallDeferred( "hide" );
			break;
		case PlayerAnimationState.Idle:
		case PlayerAnimationState.Sliding:
		case PlayerAnimationState.Running:
			TorsoAnimation.CallDeferred( "show" );
			TorsoAnimation.CallDeferred( "play", "default" );
			IdleAnimation.CallDeferred( "hide" );
			break;
		case PlayerAnimationState.TrueIdleStart:
			TorsoAnimation.CallDeferred( "hide" );
			IdleAnimation.CallDeferred( "show" );
			IdleAnimation.CallDeferred( "play", "start" );

			LeftArmAnimation.CallDeferred( "hide" );
			RightArmAnimation.CallDeferred( "hide" );
			break;
		case PlayerAnimationState.TrueIdleLoop:
			TorsoAnimation.CallDeferred( "hide" );
			IdleAnimation.CallDeferred( "show" );
			IdleAnimation.CallDeferred( "play", "loop" );

			LeftArmAnimation.CallDeferred( "hide" );
			RightArmAnimation.CallDeferred( "hide" );
			break;
		case PlayerAnimationState.Dead:
			TorsoAnimation.CallDeferred( "show" );
			TorsoAnimation.CallDeferred( "play", "dead" );

			LeftArmAnimation.CallDeferred( "hide" );
			RightArmAnimation.CallDeferred( "hide" );
			break;
		};
	}
	public override void Damage( in Entity source, float nAmount ) {
		SyncObject.Write( (byte)SteamLobby.MessageType.ServerSync );
		SyncObject.Write( (byte)PlayerUpdateType.Damage );
		if ( source is Player player && player != null ) {
			SyncObject.Write( (byte)PlayerDamageSource.Player );
			SyncObject.Write( player.MultiplayerData.Id.ToString() );
			GD.Print( "Sending Damage Packet from " + player.MultiplayerData.Id );
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

			WalkEffect.Emitting = true;
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

		AudioChannel = GetNode<AudioStreamPlayer2D>( "AudioChannel" );
		AudioChannel.VolumeDb = SettingsData.GetEffectsVolumeLinear();

		IdleAnimation = GetNode<AnimatedSprite2D>( "Idle" );

		FootSteps = GetNode<FootSteps>( "FootSteps" );

		SteamLobby.Instance.AddPlayer( OwnerId, new SteamLobby.NetworkNode( this, null, Update ) );
	}
	public override void _Process( double delta ) {
		base._Process( delta );

		float effectiveFactor = 1.0f - Mathf.Pow( 1.0f - 0.06f, (float)delta * 60.0f );
		GlobalPosition = GlobalPosition.Lerp( NetworkPosition, effectiveFactor );
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
			string append = arm == LeftArmAnimation ? "left" : arm == RightArmAnimation ? "right" : "";
			if ( ( WeaponUseMode & WeaponEntity.Properties.IsFirearm ) != 0 ) {
				property = "firearm_frames_" + append;
			} else if ( ( WeaponUseMode & WeaponEntity.Properties.IsBlunt ) != 0 ) {
				property = "blunt_frames_" + append;
			} else if ( ( WeaponUseMode & WeaponEntity.Properties.IsBladed ) != 0 ) {
				property = "bladed_frames_" + append;
			}
			string path = "resources/animations/player/" + (string)( (Godot.Collections.Dictionary)CurrentWeapon.Get( "properties" ) )[ property ];
			arm.SetDeferred( "sprite_frames", ResourceCache.GetSpriteFrames( path ) );
			arm.CallDeferred( "play", "idle" );
			arm.CallDeferred( "show" );
			break; }
		case PlayerAnimationState.WeaponUse: {
			string property = "";
			string append = arm == LeftArmAnimation ? "left" : arm == RightArmAnimation ? "right" : "";
			if ( ( WeaponUseMode & WeaponEntity.Properties.IsFirearm ) != 0 ) {
				property = "firearm_frames_" + append;
			} else if ( ( WeaponUseMode & WeaponEntity.Properties.IsBlunt ) != 0 ) {
				property = "blunt_frames_" + append;
			} else if ( ( WeaponUseMode & WeaponEntity.Properties.IsBladed ) != 0 ) {
				property = "bladed_frames_" + append;
			}
			string path = "resources/animations/player/" + (string)( (Godot.Collections.Dictionary)CurrentWeapon.Get( "properties" ) )[ property ];
			arm.SetDeferred( "sprite_frames", ResourceCache.GetSpriteFrames( path ) );
			arm.CallDeferred( "play", "use" );
			arm.CallDeferred( "show" );
			break; }
		case PlayerAnimationState.WeaponReload: {
			string path = "";
			if ( arm == LeftArmAnimation ) {
				path = "resources/animations/player/" + (string)( (Godot.Collections.Dictionary)CurrentWeapon.Get( "properties" ) )[ "firearm_frames_left" ];
			} else if ( arm == RightArmAnimation ) {
				path = "resources/animations/player/" + (string)( (Godot.Collections.Dictionary)CurrentWeapon.Get( "properties" ) )[ "firearm_frames_right" ];
			}
			arm.SpriteFrames = ResourceCache.GetSpriteFrames( path );
			arm.Play( "reload" );
			arm.Show();
			break; }
		case PlayerAnimationState.WeaponEmpty: {
			string path = "";
			if ( arm == LeftArmAnimation ) {
				path = "resources/animations/player/" + (string)( (Godot.Collections.Dictionary)CurrentWeapon.Get( "properties" ) )[ "firearm_frames_left" ];
			} else if ( arm == RightArmAnimation ) {
				path = "resources/animations/player/" + (string)( (Godot.Collections.Dictionary)CurrentWeapon.Get( "properties" ) )[ "firearm_frames_right" ];
			}
			arm.SetDeferred( "sprite_frames", ResourceCache.GetSpriteFrames( path ) );
			arm.CallDeferred( "play", "empty" );
			arm.CallDeferred( "show" );
			break; }
		};
	}
	public void SetOwnerId( CSteamID steamId ) {
		OwnerId = steamId;
	}
};