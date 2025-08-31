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
using Renown;
using Steam;

namespace Multiplayer.Objectives {
	public partial class Flag : InteractionItem {
		private enum FlagCommand : byte {
			Dropped,
			Pickup,
		};

		[Export]
		private Modes.Team Team;

		private Timer ReturnTimer;
		private Sprite2D Icon;
		private Sprite2D FlashyThing;
		private Node Parent;

		private bool AtHome = true;

		private NetworkSyncObject SyncObject = new NetworkSyncObject( 6 );

		public override InteractionType InteractionType => InteractionType.MultiplayerFlag;

		[Signal]
		public delegate void StolenEventHandler( Entity source );
		[Signal]
		public delegate void ReturnedEventHandler( Entity source );
		[Signal]
		public delegate void CapturedEventHandler( Entity source );
		[Signal]
		public delegate void DroppedEventHandler();

		private void OnReturnFlag() {
			Reparent( Parent );
			AtHome = true;

			FlashyThing.Material.Set( "shader_parameter/highlight_strength", 2.05f );
		}
		private void OnCarrierDie( Entity source, Entity target ) {
			target.Die -= OnCarrierDie;

			Reparent( Parent );

			EmitSignalDropped();
		}

		private void OnFlagPickup( Rid bodyRid, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
			if ( body is Entity entity && entity != null ) {
				if ( (Modes.Team)entity.GetMeta( "Team" ) != Team && entity.Health > 0.0f ) {
					// ensure a corpse isn't picking it up

					AtHome = false;

					ReturnTimer.Stop();
					FlashyThing.Material.Set( "shader_parameter/highlight_strength", 0.0f );
					entity.Die += OnCarrierDie;
					if ( entity is Player player && player != null ) {
//						player.Die += 
					} else if ( entity is NetworkPlayer node && node != null ) {
						SendPickupPacket();
					}
					Reparent( entity );
				} else {
					// returning
					ReturnTimer.Start();
				}
			}
		}

		private void SendPickupPacket() {
			SyncObject.Write( (byte)SteamLobby.MessageType.GameData );
			SyncObject.Write( GetPath().GetHashCode() );
			SyncObject.Write( (byte)FlagCommand.Pickup );
			SyncObject.Sync();
		}
		private void SendDropPacket() {
			SyncObject.Write( (byte)SteamLobby.MessageType.GameData );
			SyncObject.Write( GetPath().GetHashCode() );
			SyncObject.Write( (byte)FlagCommand.Dropped );
			SyncObject.Sync();
		}

		private void ReceivePacket( System.IO.BinaryReader packet ) {
			SyncObject.BeginRead( packet );

			switch ( (FlagCommand)packet.ReadByte() ) {
			case FlagCommand.Dropped:
				Reparent( Parent );
				EmitSignalDropped();
				break;
			case FlagCommand.Pickup: {
//				System.Action<Entity> useWeaponCallback;
				Entity.DieEventHandler dieCallback;

//				useWeaponCallback = Callable.From<Entity>( ( source ) => SendDropPacket() );
				dieCallback = ( source, target ) => {
//					LevelData.Instance.ThisPlayer.Die -= dieCallback;
					SendDropPacket();
				};

				LevelData.Instance.ThisPlayer.UsedWeapon += ( source ) => { };
				LevelData.Instance.ThisPlayer.Die += ( source, target ) => { };
				break; }
			};
		}

		public override void _Ready() {
			base._Ready();

			SteamLobby.Instance.AddNetworkNode( GetPath(), new SteamLobby.NetworkNode( this, null, ReceivePacket ) );

			// only the host runs the objective logic
			if ( !SteamLobby.Instance.IsHost ) {
				return;
			}

			Parent = GetParent();

			Icon = GetNode<Sprite2D>( "Icon" );

			FlashyThing = GetNode<Sprite2D>( "FlashyThing" );

			ReturnTimer = GetNode<Timer>( "ReturnTimer" );
			ReturnTimer.Connect( Timer.SignalName.Timeout, Callable.From( OnReturnFlag ) );

			Connect( SignalName.BodyShapeEntered, Callable.From<Rid, Node2D, int, int>( OnFlagPickup ) );
		}
	};
};