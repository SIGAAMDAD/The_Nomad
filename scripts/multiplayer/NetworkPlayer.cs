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

	private ShaderMaterial TorsoBloodShader;
	private ShaderMaterial LeftArmBloodShader;
	private ShaderMaterial RightArmBloodShader;
	private ShaderMaterial LegBloodShader;

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
		animator.CallDeferred( AnimatedSprite2D.MethodName.Play, animation );
	}

	// TODO: find some way of sending values back to the client

	public override void PlaySound( AudioStreamPlayer2D channel, AudioStream stream ) {
		base.PlaySound( channel == null ? AudioChannel : channel, stream );
	}

	public void Update( ulong senderId, System.IO.BinaryReader packet ) {
		SyncObject.BeginRead( packet );

		bool flip = SyncObject.ReadBoolean();
		if ( flip != LastFlipState ) {
			TorsoAnimation.SetDeferred( AnimatedSprite2D.PropertyName.FlipH, flip );
			LegAnimation.SetDeferred( AnimatedSprite2D.PropertyName.FlipH, flip );
			LeftArmAnimation.SetDeferred( AnimatedSprite2D.PropertyName.FlipV, flip );
			RightArmAnimation.SetDeferred( AnimatedSprite2D.PropertyName.FlipV, flip );
			LastFlipState = flip;
		}

		NetworkPosition = new Godot.Vector2( SyncObject.ReadFloat(), SyncObject.ReadFloat() );
		CurrentSpeed = Player.MAX_SPEED;

		if ( SyncObject.ReadBoolean() ) {
			Player.PlayerFlags flags = (Player.PlayerFlags)SyncObject.ReadUInt32();

			bool isDashing = ( flags & Player.PlayerFlags.Dashing ) != 0;
			if ( isDashing && !DashChannel.Playing ) {
				CurrentSpeed += 1000.0f;
				PlaySound( DashChannel, ResourceCache.DashSfx[ RNJesus.IntRange( 0, ResourceCache.DashSfx.Length - 1 ) ] );
			}
			DashEffect.SetDeferred( GpuParticles2D.PropertyName.Emitting, isDashing );
			DashLight.SetDeferred( Light2D.PropertyName.Visible, isDashing );

			bool isSliding = ( flags & Player.PlayerFlags.Sliding ) != 0;
			SlideEffect.SetDeferred( GpuParticles2D.PropertyName.Emitting, isSliding );
			if ( isSliding ) {
				CurrentSpeed += 400;
			}
		}

		if ( SyncObject.ReadBoolean() ) {
			float angle = SyncObject.ReadFloat();
			LeftArmAnimation.SetDeferred( AnimatedSprite2D.PropertyName.GlobalRotation, angle );
			RightArmAnimation.SetDeferred( AnimatedSprite2D.PropertyName.GlobalRotation, angle );
		}
		if ( SyncObject.ReadBoolean() ) {
			float bloodAmount = SyncObject.ReadFloat();

			TorsoBloodShader.SetShaderParameter( "blood_coef", bloodAmount );
			LeftArmBloodShader.SetShaderParameter( "blood_coef", bloodAmount );
			RightArmBloodShader.SetShaderParameter( "blood_coef", bloodAmount );
			LegBloodShader.SetShaderParameter( "blood_coef", bloodAmount );
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
			LegAnimation.CallDeferred( AnimatedSprite2D.MethodName.Hide );
			break;
		case PlayerAnimationState.Idle:
			LegAnimation.CallDeferred( AnimatedSprite2D.MethodName.Show );
			LegAnimation.CallDeferred( AnimatedSprite2D.MethodName.Play, "idle" );
			WalkEffect.SetDeferred( GpuParticles2D.PropertyName.Emitting, false );
			SlideEffect.SetDeferred( GpuParticles2D.PropertyName.Emitting, false );
			break;
		case PlayerAnimationState.Running:
			LegAnimation.CallDeferred( AnimatedSprite2D.MethodName.Show );
			LegAnimation.CallDeferred( AnimatedSprite2D.MethodName.Play, "run" );
			WalkEffect.SetDeferred( GpuParticles2D.PropertyName.Emitting, true );
			break;
		case PlayerAnimationState.Sliding:
			LegAnimation.CallDeferred( AnimatedSprite2D.MethodName.Show );
			LegAnimation.CallDeferred( AnimatedSprite2D.MethodName.Play, "slide" );
			break;
		};

		TorsoAnimationState = (PlayerAnimationState)SyncObject.ReadByte();
		switch ( TorsoAnimationState ) {
		case PlayerAnimationState.CheckpointDrinking:
			TorsoAnimation.CallDeferred( AnimatedSprite2D.MethodName.Hide );
			IdleAnimation.CallDeferred( AnimatedSprite2D.MethodName.Show );
			IdleAnimation.CallDeferred( AnimatedSprite2D.MethodName.Play, "checkpoint_drink" );
			break;
		case PlayerAnimationState.CheckpointExit:
			TorsoAnimation.CallDeferred( AnimatedSprite2D.MethodName.Hide );
			IdleAnimation.CallDeferred( AnimatedSprite2D.MethodName.Show );
			IdleAnimation.CallDeferred( AnimatedSprite2D.MethodName.Play, "checkpoint_exit" );
			break;
		case PlayerAnimationState.CheckpointIdle:
			TorsoAnimation.CallDeferred( AnimatedSprite2D.MethodName.Hide );
			IdleAnimation.CallDeferred( AnimatedSprite2D.MethodName.Show );
			IdleAnimation.CallDeferred( AnimatedSprite2D.MethodName.Play, "checkpoint_idle" );

			LeftArmAnimation.CallDeferred( AnimatedSprite2D.MethodName.Hide );
			RightArmAnimation.CallDeferred( AnimatedSprite2D.MethodName.Hide );
			break;
		case PlayerAnimationState.Idle:
		case PlayerAnimationState.Sliding:
		case PlayerAnimationState.Running:
			TorsoAnimation.CallDeferred( AnimatedSprite2D.MethodName.Show );
			TorsoAnimation.CallDeferred( AnimatedSprite2D.MethodName.Play, "default" );
			IdleAnimation.CallDeferred( AnimatedSprite2D.MethodName.Hide );
			break;
		case PlayerAnimationState.TrueIdleStart:
			TorsoAnimation.CallDeferred( AnimatedSprite2D.MethodName.Hide );
			IdleAnimation.CallDeferred( AnimatedSprite2D.MethodName.Show );
			IdleAnimation.CallDeferred( AnimatedSprite2D.MethodName.Play, "start" );

			LeftArmAnimation.CallDeferred( AnimatedSprite2D.MethodName.Hide );
			RightArmAnimation.CallDeferred( AnimatedSprite2D.MethodName.Hide );
			break;
		case PlayerAnimationState.TrueIdleLoop:
			TorsoAnimation.CallDeferred( AnimatedSprite2D.MethodName.Hide );
			IdleAnimation.CallDeferred( AnimatedSprite2D.MethodName.Show );
			IdleAnimation.CallDeferred( AnimatedSprite2D.MethodName.Play, "loop" );

			LeftArmAnimation.CallDeferred( AnimatedSprite2D.MethodName.Hide );
			RightArmAnimation.CallDeferred( AnimatedSprite2D.MethodName.Hide );
			break;
		case PlayerAnimationState.Dead:
			TorsoAnimation.CallDeferred( AnimatedSprite2D.MethodName.Show );
			TorsoAnimation.CallDeferred( AnimatedSprite2D.MethodName.Play, "dead" );

			LeftArmAnimation.CallDeferred( AnimatedSprite2D.MethodName.Hide );
			RightArmAnimation.CallDeferred( AnimatedSprite2D.MethodName.Hide );
			break;
		};
	}
	private void SendDamagePacket( Entity source, float nAmount ) {
		SyncObject.Write( (byte)SteamLobby.MessageType.ServerSync );
		SyncObject.Write( (byte)SteamLobby.Instance.GetMemberIndex( OwnerId ) );
		SyncObject.Write( (byte)PlayerUpdateType.Damage );
		SyncObject.Write( (byte)PlayerDamageSource.Player );
		SyncObject.Write( nAmount );
		SyncObject.Sync( Steamworks.Constants.k_nSteamNetworkingSend_Reliable );
	}
	public override void Damage( in Entity source, float nAmount ) {
		SendDamagePacket( source, nAmount );
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
		TorsoBloodShader = TorsoAnimation.Material as ShaderMaterial;

		LeftArmAnimation = GetNode<AnimatedSprite2D>( "LeftArm" );
		DefaultLeftArmSpriteFrames = LeftArmAnimation.SpriteFrames;
		LeftArmBloodShader = LeftArmAnimation.Material as ShaderMaterial;

		RightArmAnimation = GetNode<AnimatedSprite2D>( "RightArm" );
		DefaultRightArmSpriteFrames = RightArmAnimation.SpriteFrames;
		RightArmBloodShader = RightArmAnimation.Material as ShaderMaterial;

		LegAnimation = GetNode<AnimatedSprite2D>( "Legs" );
		LegAnimation.Connect( "animation_looped", Callable.From( OnLegAnimationLooped ) );
		LegBloodShader = LegAnimation.Material as ShaderMaterial;

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

		SteamLobby.Instance.AddPlayer( OwnerId, new SteamLobby.PlayerNetworkNode( this, null, Update ) );
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
			arm.SetDeferred( AnimatedSprite2D.PropertyName.SpriteFrames, defaultFrames );
			arm.CallDeferred( AnimatedSprite2D.MethodName.Hide );
			break;
		case PlayerAnimationState.Sliding:
		case PlayerAnimationState.Idle:
			arm.SetDeferred( AnimatedSprite2D.PropertyName.SpriteFrames, defaultFrames );
			arm.CallDeferred( AnimatedSprite2D.MethodName.Play, "idle" );
			break;
		case PlayerAnimationState.Running:
			arm.SetDeferred( AnimatedSprite2D.PropertyName.SpriteFrames, defaultFrames );
			arm.CallDeferred( AnimatedSprite2D.MethodName.Play, "run" );
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
			arm.SetDeferred( AnimatedSprite2D.PropertyName.SpriteFrames, ResourceCache.GetSpriteFrames( path ) );
			arm.CallDeferred( AnimatedSprite2D.MethodName.Play, "idle" );
			arm.CallDeferred( AnimatedSprite2D.MethodName.Show );
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
			arm.SetDeferred( AnimatedSprite2D.PropertyName.SpriteFrames, ResourceCache.GetSpriteFrames( path ) );
			arm.CallDeferred( AnimatedSprite2D.MethodName.Play, "use" );
			arm.CallDeferred( AnimatedSprite2D.MethodName.Show );
			break; }
		case PlayerAnimationState.WeaponReload: {
			string path = "";
			if ( arm == LeftArmAnimation ) {
				path = "resources/animations/player/" + (string)( (Godot.Collections.Dictionary)CurrentWeapon.Get( "properties" ) )[ "firearm_frames_left" ];
			} else if ( arm == RightArmAnimation ) {
				path = "resources/animations/player/" + (string)( (Godot.Collections.Dictionary)CurrentWeapon.Get( "properties" ) )[ "firearm_frames_right" ];
			}
			arm.SetDeferred( AnimatedSprite2D.PropertyName.SpriteFrames, ResourceCache.GetSpriteFrames( path ) );
			arm.CallDeferred( AnimatedSprite2D.MethodName.Play, "reload" );
			arm.CallDeferred( AnimatedSprite2D.MethodName.Show );
			break; }
		case PlayerAnimationState.WeaponEmpty: {
			string path = "";
			if ( arm == LeftArmAnimation ) {
				path = "resources/animations/player/" + (string)( (Godot.Collections.Dictionary)CurrentWeapon.Get( "properties" ) )[ "firearm_frames_left" ];
			} else if ( arm == RightArmAnimation ) {
				path = "resources/animations/player/" + (string)( (Godot.Collections.Dictionary)CurrentWeapon.Get( "properties" ) )[ "firearm_frames_right" ];
			}
			arm.SetDeferred( AnimatedSprite2D.PropertyName.SpriteFrames, ResourceCache.GetSpriteFrames( path ) );
			arm.CallDeferred( AnimatedSprite2D.MethodName.Play, "empty" );
			arm.CallDeferred( AnimatedSprite2D.MethodName.Show );
			break; }
		};
	}
	public void SetOwnerId( CSteamID steamId ) {
		OwnerId = steamId;
	}
};