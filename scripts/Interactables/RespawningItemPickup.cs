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

namespace Interactables {

	/*
	===================================================================================
	
	RespawningItemPickup
	
	===================================================================================
	*/

	public partial class RespawningItemPickup : ItemPickup {
		[Export]
		private float CooldownTime = 15.0f;

		private CollisionShape2D InteractArea;
		private Timer RespawnTimer;

		private Multiplayer.NetworkSyncObject SyncObject;

		protected override void OnInteractionAreaBody2DEntered( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
			// TODO: auto-pickup toggle?
			if ( body is Player player && player != null ) {
				Godot.Collections.Array<Resource> Categories = (Godot.Collections.Array<Resource>)Data.Get( "categories" );

				bool done = false;
				for ( int i = 0; i < Categories.Count; i++ ) {
					string name = (string)Categories[ i ].Get( "name" );
					switch ( name ) {
						case "Weapon":
							WeaponEntity weapon = new WeaponEntity();
							weapon.Name = "Weapon" + weapon;
							weapon.Data = Data;
							weapon.SetOwner( player );
							weapon._Ready();
							weapon.TriggerPickup( player );
							done = true;
							break;
						case "Ammo":
							break;
						/*
							Ammo ammo = new Ammo();
							ammo.Name = "Ammo" + ammo;
							ammo.Data = Data;
							ammo._Ready();
							player.PickupAmmo( ammo );
							done = true;
							break;
							*/
					}
				}

				if ( done ) {
					Icon.Hide();
					InteractArea.SetDeferred( "disabled", true );
					RespawnTimer.Start();
				}

				ServerSync();
			}
		}

		private void OnRespawnTimerTimeout() {
			Icon.Show();
			InteractArea.SetDeferred( "disabled", false );
		}

		private void ServerSync() {
			SyncObject.Write( (byte)Steam.SteamLobby.MessageType.GameData );
			SyncObject.Write( GetPath().GetHashCode() );
			SyncObject.Write( Icon.Visible );
			SyncObject.ServerSync();
		}
		private void ClientSync( System.IO.BinaryReader packet ) {
			SyncObject.BeginRead( packet );

			if ( SyncObject.ReadBoolean() ) {
				Icon.Show();
				InteractArea.SetDeferred( "disabled", false );
			} else {
				Icon.Hide();
				InteractArea.SetDeferred( "disabled", true );
				RespawnTimer.Start();
			}
		}

		public override void _Ready() {
			InteractArea = GetNode<CollisionShape2D>( "InteractBody" );

			RespawnTimer = new Timer();
			RespawnTimer.Name = "RespawnTimer";
			RespawnTimer.WaitTime = CooldownTime;
			RespawnTimer.OneShot = true;
			RespawnTimer.Connect( "timeout", Callable.From( OnRespawnTimerTimeout ) );
			AddChild( RespawnTimer );

			CreateSprite();
			MoveChild( InteractArea, GetChildCount() - 1 );

			Connect( "body_shape_entered", Callable.From<Rid, Node2D, int, int>( OnInteractionAreaBody2DEntered ) );
			Connect( "body_shape_exited", Callable.From<Rid, Node2D, int, int>( OnInteractionAreaBody2DExited ) );

			if ( GameConfiguration.GameMode == GameMode.Multiplayer ) {
				Steam.SteamLobby.Instance.AddNetworkNode( GetPath(), new Steam.SteamLobby.NetworkNode( this, ServerSync, ClientSync ) );
//				SyncObject = new NetworkSyncObject( 24 );
			}
		}
	};
};