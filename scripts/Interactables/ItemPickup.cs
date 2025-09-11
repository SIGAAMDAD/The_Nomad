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
using Items;

namespace Interactables {
	/*
	===================================================================================

	ItemPickup

	===================================================================================
	*/
	/// <summary>
	/// wrapper class for item pickup interactables, static, and will automatically
	/// free from the tree when used
	/// </summary>

	public partial class ItemPickup : InteractionItem {
		[Export]
		public Resource Data { get; private set; }
		[Export]
		public int Amount { get; private set; } = -1;

		public override InteractionType InteractionType => InteractionType.ItemPickup;

		protected Sprite2D? Icon;

		private RichTextLabel Text;
		private Callable Callback;

		/*
		===============
		OnPickupItem
		===============
		*/
		private void OnPickupItem( Player player ) {
			if ( player == null ) {
				Console.PrintError( "ItemPickup.OnPickupItem: player parameter is null" );
				return;
			}
			Godot.Collections.Array<Resource> Categories = (Godot.Collections.Array<Resource>)Data.Get( "categories" );

			bool done = false;
			for ( int i = 0; i < Categories.Count; i++ ) {
				string name = Categories[ i ].Get( "name" ).AsString();
				switch ( name ) {
					case "Firearm":
						WeaponFirearm weapon = new WeaponFirearm() {
							Name = $"Weapon{GetInstanceId()}",
							Data = Data
						};
						player.AddChild( weapon );
						weapon.TriggerPickup( player );
						done = true;
						break;
					case "Melee":
						break;
					case "Ammo":
						AmmoStack ammo = new AmmoStack( Data, Amount ) {
							Name = $"Ammo{GetInstanceId()}"
						};
						player.AddChild( ammo );
						player.Inventory.PickupAmmo( this );
						done = true;
						break;
				}
			}

			if ( done ) {
				Icon?.QueueFree();
				Icon = null;

				SetDeferred( PropertyName.Monitorable, false );
				GetChild<CollisionShape2D>( 0 ).SetDeferred( CollisionShape2D.PropertyName.Disabled, true );
			}

			Text.Hide();
			player.Disconnect( Player.SignalName.Interaction, Callback );
		}

		/*
		===============
		OnInteractionAreaBody2DEntered
		===============
		*/
		protected override void OnInteractionAreaBody2DEntered( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
			// TODO: auto-pickup toggle?
			if ( body is Player player && player != null ) {
				Callback = Callable.From( () => OnPickupItem( player ) );
				Text.Show();
				player.Connect( Player.SignalName.Interaction, Callback );
				player.EmitSignal( Player.SignalName.ShowInteraction, this );
			}
		}

		/*
		===============
		OnInteractionAreaBody2DExited
		===============
		*/
		protected override void OnInteractionAreaBody2DExited( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
			if ( body is Player player && player != null ) {
				Text.Hide();
				if ( player.IsConnected( Player.SignalName.Interaction, Callback ) ) {
					player.Disconnect( Player.SignalName.Interaction, Callback );
				}
			}
		}

		/*
		===============
		Load
		===============
		*/
		private void Load() {
			using ( var reader = ArchiveSystem.GetSection( GetPath() ) ) {
				if ( reader == null ) {
					return;
				}
				if ( !reader.LoadBoolean( "Used" ) ) {
					CreateSprite();
				} else {
					RemoveFromGroup( "Archive" );
					QueueFree();
				}
			}
		}

		/*
		===============
		Save
		===============
		*/
		public void Save() {
			using ( var writer = new SaveSystem.SaveSectionWriter( GetPath(), ArchiveSystem.SaveWriter ) ) {
				writer.SaveBool( "Used", Icon == null );
			}
		}

		/*
		===============
		CreateSprite
		===============
		*/
		protected void CreateSprite() {
			Icon = new Sprite2D();
			Icon.Name = "Icon";
			Icon.Texture = (Texture2D)Data.Get( "icon" );
			Icon.ZIndex = 8;
			AddChild( Icon );
		}

		/*
		===============

		_Ready

		godot initialization override

		===============
		*/
		public override void _Ready() {
			base._Ready();

			if ( Data == null ) {
				Console.PrintError( $"ItemPickup._Ready: invalid item definition (null)" );
				QueueFree();
				return;
			}

			Text = GetNode<RichTextLabel>( "RichTextLabel" );
			LevelData.Instance.ThisPlayer.InputMappingContextChanged += () =>
				Text.ParseBbcode( AccessibilityManager.GetBindString(
					LevelData.Instance.ThisPlayer.InputManager.GetBindActionResource( Player.InputController.ControlBind.Interact )
				) );

			Connect( SignalName.BodyShapeEntered, Callable.From<Rid, Node2D, int, int>( OnInteractionAreaBody2DEntered ) );
			Connect( SignalName.BodyShapeExited, Callable.From<Rid, Node2D, int, int>( OnInteractionAreaBody2DExited ) );

			if ( !IsInGroup( "Archive" ) ) {
				AddToGroup( "Archive" );
			}

			if ( ArchiveSystem.IsLoaded() ) {
				Load();
			} else {
				CreateSprite();
			}
		}
	};
};