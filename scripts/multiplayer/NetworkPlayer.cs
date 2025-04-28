using System;
using System.Collections.Generic;
using Godot;
using PlayerSystem;
using Renown;
using Renown.Thinkers;
using Steamworks;

public enum PlayerAnimationState : uint {
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
	
	private AnimatedSprite2D IdleAnimation;
	private AnimatedSprite2D TorsoAnimation;
	private AnimatedSprite2D LeftArmAnimation;
	private AnimatedSprite2D RightArmAnimation;
	private AnimatedSprite2D LegAnimation;

	private SpriteFrames DefaultLeftArmSpriteFrames;
	private SpriteFrames DefaultRightArmSpriteFrames;

	private PlayerAnimationState LegAnimationState;
	
	// TODO: find some way of sending values back to the client
	
	public void Update( System.IO.BinaryReader packet ) {
		WeaponEntity.Properties mode = WeaponEntity.Properties.None;
		if ( packet.ReadSByte() != WeaponSlot.INVALID ) {
			mode = (WeaponEntity.Properties)packet.ReadUInt32();
			if ( packet.ReadBoolean() ) {
				CurrentWeapon = (Resource)ResourceCache.ItemDatabase.Call( "get_item", packet.ReadString() );
			}
		}
		Godot.Vector2 position = Godot.Vector2.Zero;
		position.X = (float)packet.ReadDouble();
		position.Y = (float)packet.ReadDouble();
		GlobalPosition = position;

		float ArmAngle = (float)packet.ReadDouble();

		LeftArmAnimation.GlobalRotation = ArmAngle;
		switch ( (PlayerAnimationState)packet.ReadByte() ) {
		case PlayerAnimationState.Hide:
		case PlayerAnimationState.TrueIdleStart:
		case PlayerAnimationState.TrueIdleLoop:
		case PlayerAnimationState.Dead:
			LeftArmAnimation.Hide();
			break;
		case PlayerAnimationState.Sliding:
		case PlayerAnimationState.Idle:
			LeftArmAnimation.Show();
			LeftArmAnimation.Play( "idle" );
			break;
		case PlayerAnimationState.Running:
			LeftArmAnimation.Show();
			LeftArmAnimation.Play( "run" );
			break;
		case PlayerAnimationState.WeaponIdle:
			LeftArmAnimation.Show();
			LeftArmAnimation.Play( "idle" );
			break;
		case PlayerAnimationState.WeaponUse: {
			LeftArmAnimation.Show();
			
			string property = "";
			if ( ( mode & WeaponEntity.Properties.IsFirearm ) != 0 ) {
				property = "firearm_frames_left";
			} else if ( ( mode & WeaponEntity.Properties.IsBlunt ) != 0 ) {
				property = "blunt_frames_left";
			} else if ( ( mode & WeaponEntity.Properties.IsBladed ) != 0 ) {
				property = "bladed_frames_left";
			}
			
			LeftArmAnimation.SpriteFrames =
				(SpriteFrames)( (Godot.Collections.Dictionary)CurrentWeapon.Get( "properties" ) )[ property ];
			LeftArmAnimation.Play( "use" );
			break; }
		case PlayerAnimationState.WeaponReload:
			LeftArmAnimation.Show();
			LeftArmAnimation.SpriteFrames =
				(SpriteFrames)( (Godot.Collections.Dictionary)CurrentWeapon.Get( "properties" ) )[ "firearm_frames_left" ];
			LeftArmAnimation.Play( "reload" );
			break;
		case PlayerAnimationState.WeaponEmpty:
			LeftArmAnimation.Show();
			LeftArmAnimation.SpriteFrames =
				(SpriteFrames)( (Godot.Collections.Dictionary)CurrentWeapon.Get( "properties" ) )[ "firearm_frames_left" ];
			LeftArmAnimation.Play( "empty" );
			break;
		};
		
		RightArmAnimation.GlobalRotation = ArmAngle;
		switch ( (PlayerAnimationState)packet.ReadByte() ) {
		case PlayerAnimationState.Hide:
		case PlayerAnimationState.TrueIdleStart:
		case PlayerAnimationState.TrueIdleLoop:
		case PlayerAnimationState.Dead:
//			RightArmAnimation.Hide();
			break;
		case PlayerAnimationState.Sliding:
		case PlayerAnimationState.Idle:
			RightArmAnimation.Show();
			RightArmAnimation.Play( "idle" );
			break;
		case PlayerAnimationState.Running:
			RightArmAnimation.Show();
			RightArmAnimation.Play( "run" );
			break;
		case PlayerAnimationState.WeaponIdle:
			RightArmAnimation.Show();
			RightArmAnimation.Play( "idle" );
			break;
		case PlayerAnimationState.WeaponUse: {
			RightArmAnimation.Show();
			
			string property = "";
			if ( ( mode & WeaponEntity.Properties.IsFirearm ) != 0 ) {
				property = "firearm_frames_right";
			} else if ( ( mode & WeaponEntity.Properties.IsBlunt ) != 0 ) {
				property = "blunt_frames_right";
			} else if ( ( mode & WeaponEntity.Properties.IsBladed ) != 0 ) {
				property = "bladed_frames_right";
			}
			
			RightArmAnimation.SpriteFrames =
				(SpriteFrames)( (Godot.Collections.Dictionary)CurrentWeapon.Get( "properties" ) )[ property ];
			RightArmAnimation.Play( "use" );
			break; }
		case PlayerAnimationState.WeaponReload:
			RightArmAnimation.Show();
			RightArmAnimation.SpriteFrames =
				(SpriteFrames)( (Godot.Collections.Dictionary)CurrentWeapon.Get( "properties" ) )[ "firearm_frames_right" ];
			RightArmAnimation.Play( "reload" );
			break;
		case PlayerAnimationState.WeaponEmpty:
			RightArmAnimation.Show();
			RightArmAnimation.SpriteFrames =
				(SpriteFrames)( (Godot.Collections.Dictionary)CurrentWeapon.Get( "properties" ) )[ "firearm_frames_right" ];
			RightArmAnimation.Play( "empty" );
			break;
		};

		LegAnimationState = (PlayerAnimationState)packet.ReadByte();
		switch ( LegAnimationState ) {
		case PlayerAnimationState.Hide:
		case PlayerAnimationState.TrueIdleStart:
		case PlayerAnimationState.TrueIdleLoop:
		case PlayerAnimationState.Dead:
			LegAnimation.CallDeferred( "hide" );
			break;
		case PlayerAnimationState.Idle:
			LegAnimation.Show();
			LegAnimation.Play( "idle" );
			break;
		case PlayerAnimationState.Running:
			LegAnimation.Show();
			LegAnimation.Play( "run" );
			break;
		case PlayerAnimationState.Sliding:
			LegAnimation.Show();
			LegAnimation.Play( "slide" );
			break;
		};
		
		switch ( (PlayerAnimationState)packet.ReadByte() ) {
		case PlayerAnimationState.Idle:
		case PlayerAnimationState.Sliding:
		case PlayerAnimationState.Running:
			TorsoAnimation.Show();
			TorsoAnimation.Play( "default" );
			break;
		case PlayerAnimationState.TrueIdleStart:
			TorsoAnimation.Hide();
			IdleAnimation.Show();
			IdleAnimation.Play( "start" );
			break;
		case PlayerAnimationState.TrueIdleLoop:
			TorsoAnimation.Hide();
			IdleAnimation.Show();
			IdleAnimation.Play( "loop" );
			break;
		case PlayerAnimationState.Dead:
			TorsoAnimation.Show();
			TorsoAnimation.Play( "dead" );
			break;
		};

		byte handsUsed = packet.ReadByte();
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
		SyncObject.Sync();
	}
	private void OnLegAnimationLooped() {
		if ( LegAnimationState == PlayerAnimationState.Running ) {
			MoveChannel.Stream = ResourceCache.MoveGravelSfx[ RandomFactory.Next( 0, ResourceCache.MoveGravelSfx.Length - 1 ) ];
			MoveChannel.Play();
		}
	}
	
	public override void _Ready() {
		base._Ready();

		GD.Print( "Initializing network_player..." );

		CurrentWeapon = null;
		
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

		ProcessMode = ProcessModeEnum.Disabled;

		SteamLobby.Instance.AddPlayer( OwnerId, new SteamLobby.NetworkNode( this, null, Update ) );
	}

	public void SetOwnerId( ulong steamId ) {
		OwnerId = (CSteamID)steamId;
	}
};