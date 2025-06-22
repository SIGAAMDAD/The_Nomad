/*
===========================================================================
Copyright (C) 2023-2025 Noah Van Til

This file is part of The Nomad source code.

The Nomad source code is free software; you can redistribute it
and/or modify it under the terms of the GNU General Public License as
published by the Free Software Foundation; either version 2 of the License,
or (at your option) any later version.

The Nomad source code is distributed in the hope that it will be
useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Foobar; if not, write to the Free Software
Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
===========================================================================
*/

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