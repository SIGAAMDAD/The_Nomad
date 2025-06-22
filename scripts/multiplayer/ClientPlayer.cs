using BenchmarkDotNet.Mathematics;
using Godot;
using System.Data;

// TODO: actually write this thing

namespace Multiplayer {
	public partial class ClientPlayer : Node2D {
		private static readonly byte InputByte_Dash = 0b00000001;
		private static readonly byte InputByte_Slide = 0b00000010;
		private static readonly byte InputByte_UseWeapon = 0b00000100;

		private Resource CurrentWeapon;
		private WeaponEntity.Properties WeaponUseMode;

		private Resource MoveAction;

		private AnimatedSprite2D TorsoAnimation;
		private AnimatedSprite2D LegAnimation;
		private AnimatedSprite2D LeftArmAnimation;
		private AnimatedSprite2D RightArmAnimation;

		private SpriteFrames DefaultLeftArmAnimation;
		private SpriteFrames DefaultRightArmAnimation;

		private NetworkSyncObject SyncObject = new NetworkSyncObject( 8 );

		private Godot.Vector2 Velocity = Godot.Vector2.Zero;
		private byte InputMask = 0;

		private void ServerSync() {
			SyncObject.Write( (byte)SteamLobby.MessageType.ClientData );
			SyncObject.Write( InputMask );
			SyncObject.Write( Velocity );
		}
		private void ClientSync( System.IO.BinaryReader packet ) {
			SyncObject.BeginRead( packet );

			GlobalPosition = SyncObject.ReadVector2();

			if ( SyncObject.ReadBoolean() ) {
				WeaponUseMode = (WeaponEntity.Properties)SyncObject.ReadUInt32();
			}
			if ( SyncObject.ReadBoolean() ) {
				CurrentWeapon = (Resource)ResourceCache.ItemDatabase.Call( "get_item", SyncObject.ReadString() );
			}
			if ( SyncObject.ReadBoolean() ) {
				float angle = SyncObject.ReadFloat();
				LeftArmAnimation.GlobalRotation = angle;
				RightArmAnimation.GlobalRotation = angle;
			}

			SetArmAnimationState( LeftArmAnimation, (PlayerAnimationState)SyncObject.ReadByte(), DefaultLeftArmAnimation );
			SetArmAnimationState( RightArmAnimation, (PlayerAnimationState)SyncObject.ReadByte(), DefaultRightArmAnimation );

			switch ( (PlayerAnimationState)SyncObject.ReadByte() ) {
			case PlayerAnimationState.CheckpointDrinking:
			case PlayerAnimationState.CheckpointExit:
			case PlayerAnimationState.CheckpointIdle:
			case PlayerAnimationState.Dead:
			case PlayerAnimationState.TrueIdleStart:
			case PlayerAnimationState.TrueIdleLoop:
				LegAnimation.Hide();
				break;
			case PlayerAnimationState.Sliding:
				LegAnimation.Show();
				LegAnimation.Play( "slide" );
				break;
			case PlayerAnimationState.Idle:
				LegAnimation.Show();
				LegAnimation.Play( "idle" );
				break;
			case PlayerAnimationState.Running:
				LegAnimation.Show();
				LegAnimation.Play( "move" );
				break;
			}
			;
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
					break;
				}
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
					break;
				}
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
					break;
				}
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
					break;
				}
			}
			;
		}

		public override void _Ready() {
			base._Ready();
		}
		public override void _PhysicsProcess( double delta ) {
			base._PhysicsProcess( delta );

			Velocity = (Godot.Vector2)MoveAction.Get( "value_axis_2d" );
		}
	};
};