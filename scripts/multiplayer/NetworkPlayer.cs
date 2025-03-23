using System.Xml.Linq;
using GDExtension.Wrappers;
using Godot;
using PlayerSystem;
using Steamworks;

public enum PlayerAnimationState : uint {
	TrueIdleStart,
	TrueIdleLoop,
	Idle,
	Sliding,
	Running,
	Dead,
	Hide,
	WeaponIdle,
	WeaponUse,
	WeaponReload,
	WeaponEmpty,
	
	Count
};

public partial class NetworkPlayer : CharacterBody2D {
	private byte[] Packet = new byte[ 24 ];
	private System.IO.MemoryStream PacketStream = null;
	private System.IO.BinaryWriter PacketWriter = null;
	
	public string MultiplayerUsername;
	public CSteamID MultiplayerId;
	public ulong MultiplayerKills;
	public ulong MultiplayerDeaths;
	
	private InventoryDatabase Database;
	private Resource CurrentWeapon;
	private CSteamID OwnerId;
	private int NodeHash;
	
	private AnimatedSprite2D IdleAnimation;
	private AnimatedSprite2D TorsoAnimation;
	private AnimatedSprite2D LeftArmAnimation;
	private AnimatedSprite2D RightArmAnimation;
	private AnimatedSprite2D LegAnimation;
	
	// TODO: find some way of sending values back to the client
	
	public void Update( System.IO.BinaryReader packet ) {
		WeaponEntity.Properties mode = WeaponEntity.Properties.None;
		if ( packet.ReadInt32() != WeaponSlot.INVALID ) {
			mode = (WeaponEntity.Properties)packet.ReadUInt32();
			if ( packet.ReadBoolean() ) {
				string weaponId = packet.ReadString();
				CurrentWeapon = Database.GetItem( weaponId );
			}
		}
		Godot.Vector2 position = Godot.Vector2.Zero;
		position.X = (float)packet.ReadDouble();
		position.Y = (float)packet.ReadDouble();
		GlobalPosition = position;

		LeftArmAnimation.GlobalRotation = (float)packet.ReadDouble();
		switch ( (PlayerAnimationState)packet.ReadByte() ) {
		case PlayerAnimationState.Idle:
			LeftArmAnimation.Show();
			LeftArmAnimation.Play( "idle" );
			break;
		case PlayerAnimationState.Running:
			LeftArmAnimation.Show();
			LeftArmAnimation.Play( "move" );
			break;
		case PlayerAnimationState.Sliding:
			LeftArmAnimation.Show();
			LeftArmAnimation.Play( "slide" );
			break;
		case PlayerAnimationState.Hide:
			LeftArmAnimation.Hide();
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
		
		RightArmAnimation.GlobalRotation = (float)packet.ReadDouble();
		switch ( (PlayerAnimationState)packet.ReadByte() ) {
		case PlayerAnimationState.Idle:
			RightArmAnimation.Show();
			RightArmAnimation.Play( "idle" );
			break;
		case PlayerAnimationState.Running:
			RightArmAnimation.Show();
			RightArmAnimation.Play( "move" );
			break;
		case PlayerAnimationState.Sliding:
			RightArmAnimation.Show();
			RightArmAnimation.Play( "slide" );
			break;
		case PlayerAnimationState.Hide:
			RightArmAnimation.Hide();
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

		switch ( (PlayerAnimationState)packet.ReadByte() ) {
		case PlayerAnimationState.Hide:
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
			TorsoAnimation.Play( "idle" );
			break;
		case PlayerAnimationState.Running:
			TorsoAnimation.Play( "move" );
			break;
		case PlayerAnimationState.Sliding:
			TorsoAnimation.Play( "slide" );
			break;
		case PlayerAnimationState.TrueIdleStart:
			TorsoAnimation.Play( "true_idle_start" );
			break;
		case PlayerAnimationState.TrueIdleLoop:
			TorsoAnimation.Play( "true_idle_loop" );
			break;
		case PlayerAnimationState.Dead:
			TorsoAnimation.Play( "dead" );
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
	
	public override void _Ready() {
		base._Ready();

		GD.Print( "Initializing network_player..." );

		CurrentWeapon = null;
		Database = ResourceLoader.Load<InventoryDatabase>( "res://resources/ItemDatabase.tres" );
		
		TorsoAnimation = GetNode<AnimatedSprite2D>( "Torso" );
		LeftArmAnimation = GetNode<AnimatedSprite2D>( "LeftArm" );
		RightArmAnimation = GetNode<AnimatedSprite2D>( "RightArm" );
		LegAnimation = GetNode<AnimatedSprite2D>( "Legs" );

		PacketStream = new System.IO.MemoryStream( Packet );
		PacketWriter = new System.IO.BinaryWriter( PacketStream );

		SteamLobby.Instance.AddNetworkNode( SteamFriends.GetFriendPersonaName( OwnerId ).GetHashCode(),
			new SteamLobby.NetworkNode( this, null, Update ) );
	}

	public void SetOwnerId( ulong steamId ) {
		OwnerId = (CSteamID)steamId;
	}
};