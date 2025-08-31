/*
===========================================================================
The Nomad AGPL Source Code
Copyright (C) 2025 Noah Van Til

The Nomad Source Code is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

The Nomad Source Code is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with The Nomad Source Code.  If not, see <http://www.gnu.org/licenses/>.

If you have questions concerning this license or the applicable additional
terms, you may contact me via email at nyvantil@gmail.com.
===========================================================================
*/

using Godot;
using Multiplayer;
using Steam;

namespace PlayerSystem {
	/*
	===================================================================================
	
	AfterImage
	
	===================================================================================
	*/
	/// <summary>
	/// A low-alpha representation of the player's last known position for the AI
	/// </summary>
	
	public partial class AfterImage : Node2D {
		private AnimatedSprite2D Legs;
		private AnimatedSprite2D Torso;
		private AnimatedSprite2D LeftArm;
		private AnimatedSprite2D RightArm;

		private NetworkSyncObject SyncObject;

		/*
		===============
		AfterImage
		===============
		*/
		public AfterImage() {
			LeftArm = new AnimatedSprite2D();
			CallDeferred( MethodName.AddChild, LeftArm );

			RightArm = new AnimatedSprite2D();
			CallDeferred( MethodName.AddChild, RightArm );

			Torso = new AnimatedSprite2D();
			CallDeferred( MethodName.AddChild, Torso );

			Legs = new AnimatedSprite2D();
			CallDeferred( MethodName.AddChild, Legs );

			if ( GameConfiguration.GameMode == GameMode.Multiplayer ) {
				SyncObject = new NetworkSyncObject( 24 );
				SteamLobby.Instance.AddNetworkNode( GetPath(), new SteamLobby.NetworkNode( this, ServerSync, null ) );
			}
		}

		/*
		===============
		ServerSync
		===============
		*/
		private void ServerSync() {
			SyncObject.Write( (byte)SteamLobby.MessageType.GameData );
			SyncObject.Write( GetPath().GetHashCode() );

			switch ( LeftArm.Animation ) {
				case "idle":
					SyncObject.Write( (byte)PlayerAnimationState.Idle );
					break;
			}
		}

		/*
		===============
		Update
		===============
		*/
		/// <summary>
		/// Updates the AfterImage's position and animation
		/// </summary>
		/// <param name="player"></param>
		public void Update( Player player ) {
			if ( player == null ) {
				return;
			}

			GlobalPosition = player.GlobalPosition;
			LeftArm.SpriteFrames = player.ArmLeft.Animations.SpriteFrames;
			RightArm.SpriteFrames = player.ArmRight.Animations.SpriteFrames;
			Torso.SpriteFrames = player.TorsoAnimation.SpriteFrames;
			Legs.SpriteFrames = player.LegAnimation.SpriteFrames;
		}
	};
};