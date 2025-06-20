using Godot;

namespace PlayerSystem {
	public partial class AfterImage : Node2D {
		private AnimatedSprite2D Legs;
		private AnimatedSprite2D Torso;
		private AnimatedSprite2D LeftArm;
		private AnimatedSprite2D RightArm;

		private NetworkSyncObject SyncObject;

		private void ServerSync() {
			SyncObject.Write( (byte)SteamLobby.MessageType.GameData );
			SyncObject.Write( GetPath().GetHashCode() );

			switch ( LeftArm.Animation ) {
			case "idle":
				SyncObject.Write( (byte)PlayerAnimationState.Idle );
				break;
			};
		}

		public AfterImage() {
			LeftArm = new AnimatedSprite2D();
			CallDeferred( "add_child", LeftArm );

			RightArm = new AnimatedSprite2D();
			CallDeferred( "add_child", RightArm );

			Torso = new AnimatedSprite2D();
			CallDeferred( "add_child", Torso );

			Legs = new AnimatedSprite2D();
			CallDeferred( "add_child", Legs );

			if ( GameConfiguration.GameMode == GameMode.Multiplayer ) {
				SyncObject = new NetworkSyncObject( 24 );
				SteamLobby.Instance.AddNetworkNode( GetPath(), new SteamLobby.NetworkNode( this, ServerSync, null ) );
			}
		}
		public void Update( Player player ) {
			if ( player == null ) {
				return;
			}

			GlobalPosition = player.GlobalPosition;
			LeftArm.SpriteFrames = player.GetLeftArmAnimation().SpriteFrames;
			RightArm.SpriteFrames = player.GetRightArmAnimation().SpriteFrames;
			Torso.SpriteFrames = player.GetTorsoAnimation().SpriteFrames;
			Legs.SpriteFrames = player.GetLegsAnimation().SpriteFrames;
		}
	};
};