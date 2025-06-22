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

using System;
using System.Collections.Generic;
using Godot;
using Renown;

namespace PlayerSystem {
	public partial class Notebook : MarginContainer {
		private enum TabSelect {
			Backpack,
			Contracts,
			RecentEvents,
			Equipment,

			Count
		};

		private static readonly Godot.Vector2 BACKPACK_ITEM_MINIMUM_SIZE = new Godot.Vector2( 64.0f, 64.0f );
		private static readonly int MAX_ITEMS_PER_ROW = 4;

		private Player _Owner;

		//
		// Weapon MetaData
		//
		private Label Weapon_BladedDamage;
		private Label Weapon_BladedRange;
		private Label Weapon_BluntDamage;
		private Label Weapon_BluntRange;
		private TextureRect Weapon_AmmoData;

		//
		// Item Metadata
		//
		private Label ItemName;
		private Label ItemType;
		private Label ItemCount;
		private Label ItemStackMax;
		private TextureRect ItemIcon;
		private RichTextLabel ItemDescription;
		private RichTextLabel ItemEffect;

		private NodePath SelectedItem;

		private Label EncumbranceAmountLabel;
		private Label OverweightLabel;

		private VBoxContainer StackList;
		private TabBar Backpack;

		private HBoxContainer ContractClonerContainer;
		private VBoxContainer ContractList;

		private TabBar Equipment;

		//
		// Active Loadout
		//
		private VBoxContainer WeaponList;
		private TextureRect PrimaryWeapon;
		private TextureRect HeavyPrimaryWeapon;
		private TextureRect SidearmWeapon;
		private TextureRect HeavySidearmWeapon;

		private MarginContainer SelectionContainer;

		private void InitEquipment() {
			if ( _Owner.GetPrimaryWeapon().GetWeapon() != null ) {
				PrimaryWeapon.Texture = _Owner.GetPrimaryWeapon().GetWeapon().Icon;
			}
			if ( _Owner.GetHeavyPrimaryWeapon().GetWeapon() != null ) {
				HeavyPrimaryWeapon.Texture = _Owner.GetHeavyPrimaryWeapon().GetWeapon().Icon;
			}
			if ( _Owner.GetSidearmWeapon().GetWeapon() != null ) {
				SidearmWeapon.Texture = _Owner.GetSidearmWeapon().GetWeapon().Icon;
			}
			if ( _Owner.GetHeavySidearmWeapon().GetWeapon() != null ) {
				HeavySidearmWeapon.Texture = _Owner.GetHeavySidearmWeapon().GetWeapon().Icon;
			}
		}

		private void DropItemStack() {
			TextureRect item = GetNode<TextureRect>( SelectedItem );

			Resource itemType = (Resource)item.GetMeta( "item" );

			ItemPickup pickup = new ItemPickup();
			pickup.Name = "ItemPickup_" + itemType.Get( "id" ).AsString();
			pickup.GlobalPosition = _Owner.GlobalPosition;
			pickup.Amount = (int)item.GetMeta( "amount" );
			pickup.Data = (Resource)ResourceCache.ItemDatabase.Call( "get_item", itemType.Get( "id" ).AsString() );

			HBoxContainer row = item.GetOwner<HBoxContainer>();
			row.RemoveChild( item );
			item.CallDeferred( "queue_free" );

			if ( row.GetChildCount() == 0 ) {
				StackList.CallDeferred( "remove_child", row );
				row.CallDeferred( "queue_free" );
			}

			_Owner.GetLocation().CallDeferred( "add_child", pickup );
		}

		private List<WeaponEntity> GetWeaponsInCategory( string category ) {
			List<WeaponEntity> weapons = new List<WeaponEntity>();
			object LockObject = new object();

			System.Threading.Tasks.Parallel.ForEach( _Owner.GetWeaponStack(), ( weapon ) => {
				Godot.Collections.Array<Resource> categories = (Godot.Collections.Array<Resource>)weapon.Value.Data.Get( "categories" );
				for ( int i = 0; i < categories.Count; i++ ) {
					if ( (string)categories[ i ].Get( "id" ) == category ) {
						lock ( LockObject ) {
							weapons.Add( weapon.Value );
						}
					}
				}
			} );
			return weapons;
		}
		private void OnWeaponSelected( InputEvent guiEvent, TextureRect weaponSlot ) {
			if ( !IsItemSelectInputValid( guiEvent ) ) {
				return;
			}

			SelectionContainer.Show();

			foreach ( var child in WeaponList.GetChildren() ) {
				WeaponList.CallDeferred( "remove_child", child );
				child.CallDeferred( "queue_free" );
			}

			List<WeaponEntity> weapons = GetWeaponsInCategory( (string)weaponSlot.GetMeta( "category" ) );
			for ( int i = 0; i < weapons.Count; i++ ) {
				TextureRect item = new TextureRect();
				item.Texture = weapons[ i ].Icon;
				item.StretchMode = TextureRect.StretchModeEnum.KeepCentered;
				item.CustomMinimumSize = BACKPACK_ITEM_MINIMUM_SIZE;
				item.SetMeta( "weapon", weapons[ i ] );
				item.Connect( "gui_input", Callable.From<InputEvent>( ( guiInput ) => {
					if ( !IsItemSelectInputValid( guiEvent ) ) {
						return;
					}

					switch ( (string)weaponSlot.GetMeta( "category" ) ) {
					case "WEAPON_CATEGORY_PRIMARY":
						_Owner.SetPrimaryWeapon( (WeaponEntity)item.GetMeta( "weapon" ) );
						break;
					case "WEAPON_CATEGORY_SIDEARM":
						_Owner.SetSidearmWeapon( (WeaponEntity)item.GetMeta( "weapon" ) );
						break;
					};
					InitEquipment();
					SelectionContainer.GetNode<Button>( "VBoxContainer/GoBackButton" ).EmitSignal( "pressed" );
				} ) );

				WeaponList.CallDeferred( "add_child", item );
			}
		}

		public override void _Ready() {
			base._Ready();

			_Owner = GetParent<HeadsUpDisplay>().GetPlayerOwner();

			TabContainer TabContainer = GetNode<TabContainer>( "TabContainer" );
			TabContainer.Connect( "tab_clicked", Callable.From(
				( int tab ) => {
					switch ( (TabSelect)tab ) {
					case TabSelect.Backpack:
						break;
					case TabSelect.Contracts:
						break;
					case TabSelect.RecentEvents:
						break;
					case TabSelect.Equipment:
						InitEquipment();
						break;
					};
				}
			) );

			Equipment = GetNode<TabBar>( "TabContainer/Equipment" );

			SelectionContainer = Equipment.GetNode<MarginContainer>( "MarginContainer/SelectionScreen" );
			SelectionContainer.VisibilityChanged += () => {
				Equipment.GetNode<VBoxContainer>( "MarginContainer/LoadoutScreen" ).Visible = !SelectionContainer.Visible;
			};

			Button GoBackButton = SelectionContainer.GetNode<Button>( "VBoxContainer/GoBackButton" );
			GoBackButton.Connect( "pressed", Callable.From( SelectionContainer.Hide ) );

			PrimaryWeapon = Equipment.GetNode<TextureRect>( "MarginContainer/LoadoutScreen/PrimaryWeaponsContainer/PrimaryIcon" );
			PrimaryWeapon.Connect( "gui_input", Callable.From<InputEvent>( ( guiEvent ) => OnWeaponSelected( guiEvent, PrimaryWeapon ) ) );
			PrimaryWeapon.SetMeta( "category", "WEAPON_CATEGORY_PRIMARY" );

			HeavyPrimaryWeapon = Equipment.GetNode<TextureRect>( "MarginContainer/LoadoutScreen/PrimaryWeaponsContainer/HeavyPrimaryIcon" );

			SidearmWeapon = Equipment.GetNode<TextureRect>( "MarginContainer/LoadoutScreen/SidearmWeaponsContainer/SidearmIcon" );
			SidearmWeapon.Connect( "gui_input", Callable.From<InputEvent>( ( guiEvent ) => OnWeaponSelected( guiEvent, SidearmWeapon ) ) );
			SidearmWeapon.SetMeta( "category", "WEAPON_CATEGORY_SIDEARM" );

			HeavySidearmWeapon = Equipment.GetNode<TextureRect>( "MarginContainer/LoadoutScreen/SidearmWeaponsContainer/HeavySidearmIcon" );

			Backpack = GetNode<TabBar>( "TabContainer/Backpack" );

			StackList = Backpack.GetNode<VBoxContainer>( "MarginContainer/VBoxContainer/HBoxContainer/MarginContainer/VScrollBar/Cloner" );
			ItemName = Backpack.GetNode<Label>( "MarginContainer/VBoxContainer/HBoxContainer/ItemInfo/HBoxContainer/VBoxContainer/NameLabel" );
			ItemType = Backpack.GetNode<Label>( "MarginContainer/VBoxContainer/HBoxContainer/ItemInfo/HBoxContainer/VBoxContainer/MetaData/TypeContainer/Label" );
			ItemCount = Backpack.GetNode<Label>( "MarginContainer/VBoxContainer/HBoxContainer/ItemInfo/HBoxContainer/VBoxContainer/MetaData/NoHeldContainer/HBoxContainer/CountLabel" );
			ItemStackMax = Backpack.GetNode<Label>( "MarginContainer/VBoxContainer/HBoxContainer/ItemInfo/HBoxContainer/VBoxContainer/MetaData/NoHeldContainer/HBoxContainer/MaxLabel" );
			ItemIcon = Backpack.GetNode<TextureRect>( "MarginContainer/VBoxContainer/HBoxContainer/ItemInfo/HBoxContainer/Icon" );

			ItemDescription = Backpack.GetNode<RichTextLabel>( "MarginContainer/VBoxContainer/HBoxContainer/ItemInfo/DescriptionLabel" );
			ItemDescription.Theme = SettingsData.GetDyslexiaMode() ? AccessibilityManager.DyslexiaTheme : null;

			ItemEffect = Backpack.GetNode<RichTextLabel>( "MarginContainer/VBoxContainer/HBoxContainer/ItemInfo/EffectContainer/Label2" );
			ItemEffect.Theme = SettingsData.GetDyslexiaMode() ? AccessibilityManager.DyslexiaTheme : null;

			Weapon_BladedDamage = Backpack.GetNode<Label>( "MarginContainer/VBoxContainer/HBoxContainer/ItemInfo/WeaponStatusContainer/BladedInfo/DamageLabel" );
			Weapon_BladedRange = Backpack.GetNode<Label>( "MarginContainer/VBoxContainer/HBoxContainer/ItemInfo/WeaponStatusContainer/BladedInfo/RangeLabel" );
			Weapon_BluntDamage = Backpack.GetNode<Label>( "MarginContainer/VBoxContainer/HBoxContainer/ItemInfo/WeaponStatusContainer/BluntInfo/DamageLabel" );
			Weapon_BluntRange = Backpack.GetNode<Label>( "MarginContainer/VBoxContainer/HBoxContainer/ItemInfo/WeaponStatusContainer/BluntInfo/RangeLabel" );
			Weapon_AmmoData = Backpack.GetNode<TextureRect>( "MarginContainer/VBoxContainer/HBoxContainer/ItemInfo/WeaponStatusContainer/AmmoLoaded/AmmoIcon" );

			EncumbranceAmountLabel = Backpack.GetNode<Label>( "MarginContainer/VBoxContainer/BackpackDataContainer/EncumbranceContainer/AmountLabel" );
			OverweightLabel = Backpack.GetNode<Label>( "MarginContainer/VBoxContainer/BackpackDataContainer/EncumbranceContainer/OverweightLabel" );

			Button DropItemStackButton = Backpack.GetNode<Button>( "MarginContainer/VBoxContainer/HBoxContainer/ItemInfo/DropStackButton" );
			DropItemStackButton.Connect( "pressed", Callable.From( DropItemStack ) );

			TabBar Contracts = GetNode<TabBar>( "TabContainer/Contracts" );
			ContractClonerContainer = Contracts.GetNode<HBoxContainer>( "MarginContainer/VBoxContainer//VScrollBar/ContractList/ClonerContainer" );
			ContractList = Contracts.GetNode<VBoxContainer>( "MarginContainer/VBoxContainer/VScrollBar/ContractList" );
		}

		public void AddContract( Contract contract ) {
			HBoxContainer container = ContractClonerContainer.Duplicate() as HBoxContainer;
			( container.GetChild( 0 ) as Label ).Text = contract.Contractor.GetObjectName();
			( container.GetChild( 1 ) as Label ).Text = contract.Guild.GetFactionName();
			container.Show();
			ContractList.AddChild( container );
		}

		private int GetAmmoCount( string id ) {
			Dictionary<int, AmmoStack> stacks = _Owner.GetAmmoStacks();
			foreach ( var stack in stacks ) {
				if ( (string)stack.Value.AmmoType.Data.Get( "id" ) == id ) {
					return stack.Value.Amount;
				}
			}
			return 0;
		}
		private int GetItemCount( string id ) {
			Godot.Collections.Array<Resource> stackList = (Godot.Collections.Array<Resource>)_Owner.GetInventory().Get( "stacks" );
			for ( int i = 0; i < stackList.Count; i++ ) {
				if ( (string)stackList[i].Get( "item_id" ) == id ) {
					return (int)stackList[i].Get( "amount" );
				}
			}
			return 0;
		}

		public void OnShowBackpack() {
			Backpack.Visible = true;

			foreach ( var child in StackList.GetChildren() ) {
				foreach ( var image in child.GetChildren() ) {
					child.RemoveChild( image );
					image.QueueFree();
				}
				StackList.RemoveChild( child );
				child.QueueFree();
			}

			StackList.Visible = true;
			
			HBoxContainer row = new HBoxContainer();
			StackList.AddChild( row );

			float weight = 0.0f;
			foreach ( var stack in _Owner.GetAmmoStacks() ) {
				row = AddAmmoStackToBackpack( row, stack.Value );
				weight += (float)stack.Value.AmmoType.Data.Get( "weight" ) * stack.Value.Amount;
			}
			foreach ( var stack in _Owner.GetWeaponStack() ) {
				row = AddWeaponToBackpack( row, stack.Value );
				weight += (float)stack.Value.Weight;
			}
			foreach ( var stack in (Godot.Collections.Array<Resource>)_Owner.GetInventory().Get( "stacks" ) ) {
				// FIXME: this is hideous
				Resource itemResource = (Resource)ResourceCache.ItemDatabase.Call( "item_id", (string)stack.Get( "item_id" ) );
				row = AddItemToBackpack( row, itemResource );
				weight += (float)itemResource.Get( "weight" ) * (int)stack.Get( "amount" );
			}

			EncumbranceAmountLabel.Text = string.Format( "{0}/{1}", weight, _Owner.MaximumInventoryWeight );
			if ( ( _Owner.GetFlags() & Player.PlayerFlags.Encumbured ) != 0 ) {
				OverweightLabel.Show();
			} else {
				OverweightLabel.Hide();
			}

			Backpack.Visible = true;
		}

		private static bool IsItemSelectInputValid( InputEvent guiEvent ) {
			if ( guiEvent is InputEventMouseButton mouse && mouse != null ) {
				return mouse.ButtonIndex == MouseButton.Left;
			} else if ( guiEvent is InputEventJoypadButton joyButton && joyButton != null ) {
				return joyButton.ButtonIndex == JoyButton.A;
			}
			return false;
		}
		private void OnBackpackItemSelected( InputEvent guiEvent, TextureRect item ) {
			if ( !item.HasMeta( "item" ) || !IsItemSelectInputValid( guiEvent ) ) {
				return;
			}

			Resource itemType = (Resource)item.GetMeta( "item" );

			if ( item.HasMeta( "description" ) ) {
				ItemDescription.Show();
				ItemDescription.ParseBbcode( TranslationServer.Translate( item.GetMeta( "description" ).AsString() ) );
			} else {
				ItemDescription.Hide();
			}

			if ( item.HasMeta( "Effects" ) ) {
				ItemEffect.Show();
				ItemEffect.ParseBbcode( TranslationServer.Translate( item.GetMeta( "effects" ).AsString() ) );
			} else {
				ItemEffect.Hide();
			}

			string category = item.GetMeta( "category" ).AsString();
			if ( category == "Weapon" ) {
				Godot.Collections.Dictionary properties = (Godot.Collections.Dictionary)itemType.Get( "properties" );

				if ( properties.TryGetValue( "bladed_damage", out Variant BladedDamage ) ) {
					Weapon_BladedDamage.Text = BladedDamage.AsSingle().ToString();
				}
				if ( properties.TryGetValue( "bladed_range", out Variant BladedRange ) ) {
					Weapon_BladedRange.Text = BladedRange.AsSingle().ToString();
				}
				if ( properties.TryGetValue( "blunt_damage", out Variant BluntDamage ) ) {
					Weapon_BluntDamage.Text = BluntDamage.AsSingle().ToString();
				}
				if ( properties.TryGetValue( "blunt_range", out Variant BluntRange ) ) {
					Weapon_BluntRange.Text = BluntRange.AsSingle().ToString();
				}
			} else if ( category == "Ammo" ) {
			}

			ItemName.Text = itemType.Get( "name" ).AsString();
			ItemIcon.Texture = item.Texture;
			ItemType.Text = category;
			ItemCount.Text = Convert.ToString( (int)item.GetMeta( "amount" ) );
			ItemStackMax.Text = Convert.ToString( itemType.Get( "max_stack" ).AsInt32() );

			SelectedItem = (NodePath)item.GetMeta( "node" );
		}

		private void AddItem( HBoxContainer row, Resource itemType, int stackAmount ) {
			//
			// make sure its a valid item first
			//

			string itemId = (string)itemType.Get( "id" );
			if ( itemId == null ) {
				Console.PrintError( string.Format( "Notebook.AddItem: invalid itemType, does not contain item_id!" ) );
				return;
			}

			string name = (string)itemType.Get( "name" );
			if ( name == null ) {
				Console.PrintError( string.Format( "Notebook.AddItem: invalid itemType, does not contain v!" ) );
				return;
			}

			Texture2D icon = (Texture2D)itemType.Get( "icon" );
			if ( icon == null ) {
				Console.PrintError( string.Format( "Notebook.AddItem: invalid itemType, does not contain icon!" ) );
				return;
			}

			int maxStack = 0;
			try {
				maxStack = (int)itemType.Get( "max_stack" );
			} catch ( Exception ) {
				// assume we don't have it
				Console.PrintError( string.Format( "Notebook.AddItem: invalid itemType, does not contain max_stack!" ) );
				return;
			}

			float weight = 0.0f;
			try {
				weight = (float)itemType.Get( "weight" );
			} catch ( Exception ) {
				// assume we don't have it
				Console.PrintError( string.Format( "Notebook.AddItem: invalid itemType, does not contain weight!" ) );
				return;
			}

			Godot.Collections.Dictionary properties = (Godot.Collections.Dictionary)itemType.Get( "properties" );
			if ( properties == null ) {
				Console.PrintError( string.Format( "Notebook.AddItem: invalid itemType, does not contain properties!" ) );
				return;
			}

			bool hasDescription = properties.TryGetValue( "description", out Variant description );
			bool hasEffects = properties.TryGetValue( "effects", out Variant effects );

			Godot.Collections.Array<Resource> categories = (Godot.Collections.Array<Resource>)itemType.Get( "categories" );
			if ( categories == null ) {
				Console.PrintError( string.Format( "Notebook.AddItem: invalid itemType, does not contain properties!" ) );
				return;
			}

			int category = 0;
			bool found = false;
			for ( int i = 0; i < categories.Count && !found; i++ ) {
				string id = (string)categories[ i ].Get( "id" );
				switch ( id ) {
				case "ITEM_CATEGORY_MISC":
				case "ITEM_CATEGORY_CONSUMABLE":
				case "ITEM_CATEGORY_AMMO":
				case "ITEM_CATEGORY_WEAPON":
					category = i;
					found = true;
					break;
				default:
					break;
				};
			}
			if ( !found ) {
				Console.PrintError( string.Format( "Notebook.OnBackpackItemSelected: invalid item category \"{0}\"", (string)categories[ category ].Get( "id" ) ) );
				return;
			}

			if ( row.GetChildCount() == MAX_ITEMS_PER_ROW ) {
				row = new HBoxContainer();
				StackList.AddChild( row );
			}
			TextureRect item = new TextureRect();
			row.AddChild( item );

			item.Texture = icon;
			item.StretchMode = TextureRect.StretchModeEnum.KeepCentered;
			item.CustomMinimumSize = BACKPACK_ITEM_MINIMUM_SIZE;
			item.Connect( "gui_input", Callable.From<InputEvent>( ( inputEvent ) => OnBackpackItemSelected( inputEvent, item ) ) );
			item.SetMeta( "item", itemType );
			if ( hasDescription ) {
				item.SetMeta( "description", description.AsString() );
			}
			if ( hasEffects ) {
				item.SetMeta( "effects", effects.AsString() );
			}
			item.SetMeta( "amount", stackAmount );
			item.SetMeta( "category", (string)categories[ category ].Get( "id" ) );
			item.SetMeta( "node", item.GetPath() );

			row.Show();
		}
		private HBoxContainer AddItemToBackpack( HBoxContainer row, Resource stack ) {
			AddItem( row, (Resource)ResourceCache.ItemDatabase.Call( "get_item", (string)stack.Get( "item_id" ) ),
				(int)stack.Get( "amount" ) );

			return row;
		}
		private HBoxContainer AddAmmoStackToBackpack( HBoxContainer row, AmmoStack stack ) {
			AddItem( row, stack.AmmoType.Data, stack.Amount );
			return row;
		}
		private HBoxContainer AddWeaponToBackpack( HBoxContainer row, WeaponEntity weapon ) {
			// weapons will never stack
			AddItem( row, weapon.Data, 1 );
			return row;
		}
	};
};
