using System;
using GDExtension.Wrappers;
using Godot;
using PlayerSystem;
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

public partial class NetworkPlayer : CharacterBody2D {
	private byte[] Packet = new byte[ 24 ];
	private System.IO.MemoryStream PacketStream = null;
	private System.IO.BinaryWriter PacketWriter = null;

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

	private PlayerAnimationState LegAnimationState;
	
	// TODO: find some way of sending values back to the client
	
	public void Update( System.IO.BinaryReader packet ) {
		WeaponEntity.Properties mode = WeaponEntity.Properties.None;
		if ( packet.ReadSByte() != WeaponSlot.INVALID ) {
			mode = (WeaponEntity.Properties)packet.ReadUInt32();
			if ( packet.ReadBoolean() ) {
				string weaponId = packet.ReadString();
				CurrentWeapon = (Resource)Database.Call( "get_item_from_id",  weaponId );
			}
		}
		Godot.Vector2 position = Godot.Vector2.Zero;
		position.X = (float)packet.ReadDouble();
		position.Y = (float)packet.ReadDouble();
		GlobalPosition = position;

		LeftArmAnimation.GlobalRotation = (float)packet.ReadDouble();
		switch ( (PlayerAnimationState)packet.ReadByte() ) {
		case PlayerAnimationState.Hide:
		case PlayerAnimationState.TrueIdleStart:
		case PlayerAnimationState.TrueIdleLoop:
		case PlayerAnimationState.Dead:
		case PlayerAnimationState.Sliding:
			LeftArmAnimation.CallDeferred( "stop" );
			LeftArmAnimation.Hide();
			break;
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
			LeftArmAnimation.CallDeferred( "play", "use" );
			break; }
		case PlayerAnimationState.WeaponReload:
			LeftArmAnimation.Show();
			LeftArmAnimation.SpriteFrames =
				(SpriteFrames)( (Godot.Collections.Dictionary)CurrentWeapon.Get( "properties" ) )[ "firearm_frames_left" ];
			LeftArmAnimation.CallDeferred( "play", "reload" );
			break;
		case PlayerAnimationState.WeaponEmpty:
			LeftArmAnimation.Show();
			LeftArmAnimation.SpriteFrames =
				(SpriteFrames)( (Godot.Collections.Dictionary)CurrentWeapon.Get( "properties" ) )[ "firearm_frames_left" ];
			LeftArmAnimation.CallDeferred( "play", "empty" );
			break;
		};
		
		RightArmAnimation.GlobalRotation = (float)packet.ReadDouble();
		switch ( (PlayerAnimationState)packet.ReadByte() ) {
		case PlayerAnimationState.Hide:
		case PlayerAnimationState.TrueIdleStart:
		case PlayerAnimationState.TrueIdleLoop:
		case PlayerAnimationState.Dead:
		case PlayerAnimationState.Sliding:
			RightArmAnimation.Show();
			break;
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
			RightArmAnimation.CallDeferred( "play", "use" );
			break; }
		case PlayerAnimationState.WeaponReload:
			RightArmAnimation.Show();
			RightArmAnimation.SpriteFrames =
				(SpriteFrames)( (Godot.Collections.Dictionary)CurrentWeapon.Get( "properties" ) )[ "firearm_frames_right" ];
			RightArmAnimation.CallDeferred( "play", "reload" );
			break;
		case PlayerAnimationState.WeaponEmpty:
			RightArmAnimation.Show();
			RightArmAnimation.SpriteFrames =
				(SpriteFrames)( (Godot.Collections.Dictionary)CurrentWeapon.Get( "properties" ) )[ "firearm_frames_right" ];
			RightArmAnimation.CallDeferred( "play", "empty" );
			break;
		};

		LegAnimationState = (PlayerAnimationState)packet.ReadByte();
		switch ( LegAnimationState ) {
		case PlayerAnimationState.Hide:
		case PlayerAnimationState.TrueIdleStart:
		case PlayerAnimationState.TrueIdleLoop:
		case PlayerAnimationState.Dead:
			LegAnimation.Hide();
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
			TorsoAnimation.CallDeferred( "play", "default" );
			break;
		case PlayerAnimationState.TrueIdleStart:
			TorsoAnimation.Hide();
			IdleAnimation.Show();
			IdleAnimation.CallDeferred( "play", "start" );
			break;
		case PlayerAnimationState.TrueIdleLoop:
			TorsoAnimation.Hide();
			IdleAnimation.Show();
			IdleAnimation.CallDeferred( "play", "loop" );
			break;
		case PlayerAnimationState.Dead:
			TorsoAnimation.Show();
			TorsoAnimation.CallDeferred( "play", "dead" );
			break;
		};

		byte handsUsed = packet.ReadByte();
	}
	public void Damage( CharacterBody2D attacker, float nAmount ) {
		PacketStream.Seek( 0, System.IO.SeekOrigin.Begin );
		PacketWriter.Write( (byte)SteamLobby.MessageType.ClientData );
		PacketWriter.Write( (int)attacker.Get( "NodeHash" ) );
		PacketWriter.Write( nAmount );
		
		SteamLobby.Instance.SendP2PPacket( OwnerId, Packet );
	}
	private void OnLegAnimationFinished() {
		if ( LegAnimationState == PlayerAnimationState.Running ) {
			MoveChannel.Stream = ResourceCache.MoveGravelSfx[ RandomFactory.Next( 0, ResourceCache.MoveGravelSfx.Length - 1 ) ];
			MoveChannel.Play();
		}
	}
	
	public override void _Ready() {
		base._Ready();

		GD.Print( "Initializing network_player..." );

		CurrentWeapon = null;
		Database = ResourceLoader.Load<Resource>( "res://resources/ItemDatabase.tres" );
		
		TorsoAnimation = GetNode<AnimatedSprite2D>( "Torso" );
		LeftArmAnimation = GetNode<AnimatedSprite2D>( "LeftArm" );
		RightArmAnimation = GetNode<AnimatedSprite2D>( "RightArm" );
		LegAnimation = GetNode<AnimatedSprite2D>( "Legs" );
		LegAnimation.Connect( "animation_finished", Callable.From( OnLegAnimationFinished ) );

		MoveChannel = GetNode<AudioStreamPlayer2D>( "MoveChannel" );
		MoveChannel.SetProcess( false );
		MoveChannel.SetProcessInternal( false );

		IdleAnimation = GetNode<AnimatedSprite2D>( "Idle" );

		PacketStream = new System.IO.MemoryStream( Packet );
		PacketWriter = new System.IO.BinaryWriter( PacketStream );

		SteamLobby.Instance.AddPlayer( OwnerId, new SteamLobby.NetworkNode( this, null, Update ) );
	}

	public void SetOwnerId( ulong steamId ) {
		OwnerId = (CSteamID)steamId;
	}
};