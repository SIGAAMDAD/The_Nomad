using GDExtension.Wrappers;
using Godot;
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
	private System.Collections.Generic.Dictionary<string, object> Packet =
		new System.Collections.Generic.Dictionary<string, object>();
	
	public string MultiplayerUsername;
	public CSteamID MultiplayerId;
	public ulong MultiplayerKills;
	public ulong MultiplayerDeaths;
	
	private InventoryDatabase Database;
	private Resource CurrentWeapon;
	private CSteamID OwnerId;
	
	private AnimatedSprite2D IdleAnimation;
	private AnimatedSprite2D TorsoAnimation;
	private AnimatedSprite2D LeftArmAnimation;
	private AnimatedSprite2D RightArmAnimation;
	private AnimatedSprite2D LegAnimation;
	
	// TODO: find some way of sending values back to the client
	
	public void Update( System.Collections.Generic.Dictionary<string, object> packet ) {
		GlobalPosition = (Godot.Vector2)packet[ "position" ];
		CurrentWeapon = Database.GetItem( (string)packet[ "weapon" ] );
		WeaponEntity.Properties mode = (WeaponEntity.Properties)packet[ "mode" ];
		
		LeftArmAnimation.GlobalRotation = (float)packet[ "lrot" ];
		
		switch ( (PlayerAnimationState)packet[ "lastate" ] ) {
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
		
		RightArmAnimation.GlobalRotation = (float)packet[ "rrot" ];
		switch ( (PlayerAnimationState)packet[ "rastate" ] ) {
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
		
		switch ( (PlayerAnimationState)packet[ "astate" ] ) {
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
	}
	
	public void Damage( CharacterBody2D attacker, float nAmount ) {
		Packet[ "type" ] = PacketType.DamagePlayer;
		Packet[ "amount" ] = nAmount;
		
		SteamLobby.Instance.SendP2PPacket( OwnerId, Packet, SteamLobby.MessageType.ClientData );
	}
	
	public override void _Ready() {
		base._Ready();

		CurrentWeapon = null;
		Database = ResourceLoader.Load<InventoryDatabase>( "res://resources/ItemDatabase.tres" );
		
		TorsoAnimation = GetNode<AnimatedSprite2D>( "Torso" );
		LeftArmAnimation = GetNode<AnimatedSprite2D>( "LeftArm" );
		RightArmAnimation = GetNode<AnimatedSprite2D>( "RightArm" );
		LegAnimation = GetNode<AnimatedSprite2D>( "Legs" );
	}

	public void SetOwnerId( CSteamID steamId ) {
		OwnerId = steamId;
	}
};