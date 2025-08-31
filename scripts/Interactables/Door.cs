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
using PlayerSystem;

namespace Interactables {
	public enum DoorState : byte {
		Locked,
		Unlocked,

		Count
	};

	public partial class Door : InteractionItem {
		[Export]
		private CollisionShape2D InteractBody;
		[Export]
		private Resource Key;
		[Export]
		private DoorState State;
		[Export]
		private Sprite2D Closed;
		[Export]
		private Sprite2D Open;
		[Export]
		public Node Area { get; private set; }
		[Export]
		public Node2D Destination { get; private set; }

		public override InteractionType InteractionType => InteractionType.Door;
		
		/*
		===============
		UserHasKey
		===============
		*/
		private bool UserHasKey( Player user ) {
			Godot.Collections.Array<Resource> stacks = (Godot.Collections.Array<Resource>)user.Inventory.Get( "stacks" );
			for ( int i = 0; i < stacks.Count; i++ ) {
				if ( (string)stacks[ i ].Get( "item_id" ) == (string)Key.Get( "id" ) ) {
					return true;
				}
			}
			return false;
		}

		/*
		===============
		UseDoor
		===============
		*/
		public bool UseDoor( Player user, out string message ) {
			message = "";
			switch ( State ) {
				case DoorState.Locked:
					if ( !UserHasKey( user ) ) {
						message = "You don't have the required key";
						return false;
					}
					message = string.Format( "Used key {0}", Key.Get( "name" ) );
					State = DoorState.Unlocked;
					Closed?.Hide();
					Open?.Show();
					return true;
				case DoorState.Unlocked:
					return true;
				default:
					Console.PrintError( "Door.UseDoor: invalid door state!" );
					break;
			}
			return false;
		}

		/*
		===============
		OnInteractionAreaBody2DEntered
		===============
		*/
		protected override void OnInteractionAreaBody2DEntered( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
			if ( body is Player player && player != null ) {
				player.BeginInteraction( this );
			}
		}

		/*
		===============
		OnInteractionAreaBody2DExited
		===============
		*/
		protected override void OnInteractionAreaBody2DExited( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
			if ( body is Player player && player != null ) {
				player.EndInteraction();
			}
		}

		/*
		===============
		Save
		===============
		*/
		/// <summary>
		/// Saves the door's state to disk
		/// </summary>
		private void Save() {
			var writer = new SaveSystem.SaveSectionWriter( GetPath(), ArchiveSystem.SaveWriter );

			writer.SaveByte( nameof( State ), (byte)State );
		}

		/*
		===============
		Load
		===============
		*/
		/// <summary>
		/// Loads the door's state from disk
		/// </summary>
		private void Load() {
			using var reader = ArchiveSystem.GetSection( GetPath() );
			if ( reader == null ) {
				return;
			}
			State = (DoorState)reader.LoadByte( nameof( State ) );
			switch ( State ) {
				case DoorState.Locked:
					Open?.Hide();
					Closed?.Show();
					break;
				case DoorState.Unlocked:
					Open?.Show();
					Closed?.Hide();
					break;
			}
		}

		/*
		===============
		_Ready
		===============
		*/
		/// <summary>
		/// godot initialization override
		/// </summary>
		public override void _Ready() {
			base._Ready();

			Connect( SignalName.BodyShapeEntered, Callable.From<Rid, Node2D, int, int>( OnInteractionAreaBody2DEntered ) );
			Connect( SignalName.BodyShapeExited, Callable.From<Rid, Node2D, int, int>( OnInteractionAreaBody2DExited ) );

			if ( !IsInGroup( "Archive" ) ) {
				AddToGroup( "Archive" );
			}
			if ( ArchiveSystem.IsLoaded() ) {
				Load();
			}
		}
	};
};