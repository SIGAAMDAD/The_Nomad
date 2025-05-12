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

		private readonly Godot.Vector2 BackpackItemMinimumSize = new Godot.Vector2( 64.0f, 64.0f );

		private Player _Owner;

		private Label ItemName;
		private Label ItemType;
		private Label ItemCount;
		private Label ItemStackMax;
		private TextureRect ItemIcon;
		private RichTextLabel ItemDescription;
		private Label ItemEffect;

		private VBoxContainer StackList;
		private TabBar Backpack;

		private HBoxContainer ContractClonerContainer;
		private VBoxContainer ContractList;

		private TabBar Equipment;

		private TextureRect PrimaryWeapon;
		private TextureRect HeavyPrimaryWeapon;
		private TextureRect SidearmWeapon;
		private TextureRect HeavySidearmWeapon;

		private void InitEquipment() {
			if ( _Owner.GetPrimaryWeapon().GetWeapon() != null ) {
				PrimaryWeapon.Texture = _Owner.GetPrimaryWeapon().GetWeapon().GetIcon();
			}
			if ( _Owner.GetHeavyPrimaryWeapon().GetWeapon() != null ) {
				HeavyPrimaryWeapon.Texture = _Owner.GetHeavyPrimaryWeapon().GetWeapon().GetIcon();
			}
			if ( _Owner.GetSidearmWeapon().GetWeapon() != null ) {
				SidearmWeapon.Texture = _Owner.GetSidearmWeapon().GetWeapon().GetIcon();
			}
			if ( _Owner.GetHeavySidearmWeapon().GetWeapon() != null ) {
				HeavySidearmWeapon.Texture = _Owner.GetHeavySidearmWeapon().GetWeapon().GetIcon();
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

			PrimaryWeapon = Equipment.GetNode<TextureRect>( "MarginContainer/VBoxContainer/PrimaryWeaponsContainer/PrimaryIcon" );
			HeavyPrimaryWeapon = Equipment.GetNode<TextureRect>( "MarginContainer/VBoxContainer/PrimaryWeaponsContainer/HeavyPrimaryIcon" );

			SidearmWeapon = Equipment.GetNode<TextureRect>( "MarginContainer/VBoxContainer/SidearmWeaponsContainer/SidearmIcon" );
			HeavySidearmWeapon = Equipment.GetNode<TextureRect>( "MarginContainer/VBoxContainer/SidearmWeaponsContainer/HeavySidearmIcon" );

			Backpack = GetNode<TabBar>( "TabContainer/Backpack" );

			StackList = Backpack.GetNode<VBoxContainer>( "MarginContainer/VBoxContainer/HBoxContainer/MarginContainer/VScrollBar/Cloner" );
			ItemName = Backpack.GetNode<Label>( "MarginContainer/VBoxContainer/HBoxContainer/ItemInfo/HBoxContainer/VBoxContainer/NameLabel" );
			ItemType = Backpack.GetNode<Label>( "MarginContainer/VBoxContainer/HBoxContainer/ItemInfo/HBoxContainer/VBoxContainer/MetaData/TypeContainer/Label" );
			ItemCount = Backpack.GetNode<Label>( "MarginContainer/VBoxContainer/HBoxContainer/ItemInfo/HBoxContainer/VBoxContainer/MetaData/NoHeldContainer/HBoxContainer/CountLabel" );
			ItemStackMax = Backpack.GetNode<Label>( "MarginContainer/VBoxContainer/HBoxContainer/ItemInfo/HBoxContainer/VBoxContainer/MetaData/NoHeldContainer/HBoxContainer/MaxLabel" );
			ItemIcon = Backpack.GetNode<TextureRect>( "MarginContainer/VBoxContainer/HBoxContainer/ItemInfo/HBoxContainer/Icon" );
			ItemDescription = Backpack.GetNode<RichTextLabel>( "MarginContainer/VBoxContainer/HBoxContainer/ItemInfo/DescriptionLabel" );
			ItemEffect = Backpack.GetNode<Label>( "MarginContainer/VBoxContainer/HBoxContainer/ItemInfo/EffectContainer/Label2" );

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

			foreach ( var stack in _Owner.GetAmmoStacks() ) {
				row = AddAmmoStackToBackpack( row, stack.Value );
			}
			foreach ( var stack in _Owner.GetWeaponStack() ) {
				row = AddWeaponToBackpack( row, stack.Value );
			}
//			foreach ( var stack in _Owner.GetInventory().Stacks ) {
//				row = AddItemToBackpack( row, stack );
//			}

			Backpack.Visible = true;
		}
		private void OnBackpackItemSelected( InputEvent guiEvent, TextureRect item ) {
			if ( !item.HasMeta( "item_id" ) || guiEvent is not InputEventMouseButton ) {
				return;
			} else if ( ( (InputEventMouseButton)guiEvent ).ButtonIndex != MouseButton.Left ) {
				return;
			}

			string itemId = (string)item.GetMeta( "item_id" );
			Resource itemType = (Resource)( (Resource)_Owner.GetInventory().Get( "database" ) ).Call( "get_item", itemId );
			if ( itemType == null ) {
				Console.PrintError( string.Format( "Notebook.OnBackpackItemSelected: invalid item_id \"{0}\"", itemId ) );
				return;
			}

			Godot.Collections.Dictionary properties = (Godot.Collections.Dictionary)itemType.Get( "properties" );
			if ( properties.ContainsKey( "description" ) ) {
				ItemDescription.Show();
				ItemDescription.ParseBbcode( TranslationServer.Translate( (string)properties[ "description" ] ) );
			} else {
				ItemDescription.Hide();
			}
			ItemDescription.Theme = SettingsData.GetDyslexiaMode() ? AccessibilityManager.DyslexiaTheme : null;

			if ( properties.ContainsKey( "effect" ) ) {
				ItemEffect.Show();
				ItemEffect.Text = (string)properties[ "effects" ];
			} else {
				ItemEffect.Hide();
			}

			Godot.Collections.Array<Resource> categories = (Godot.Collections.Array<Resource>)itemType.Get( "categories" );

			ItemName.Text = (string)itemType.Get( "name" );
			ItemIcon.Texture = (Texture2D)itemType.Get( "icon" );
			ItemType.Text = (string)categories[1].Get( "name" );

			string category = (string)categories[1].Get( "id" );
			if ( category == "ITEM_CATEGORY_MISC" ) {
				ItemCount.Text = string.Format( "{0}/", GetItemCount( (string)itemType.Get( "id" ) ).ToString() );
			} else if ( category == "ITEM_CATEGORY_AMMO" ) {
				ItemCount.Text = string.Format( "{0}/", GetAmmoCount( (string)itemType.Get( "id" ) ).ToString() );
			} else if ( category == "ITEM_CATEGORY_WEAPON" ) {
				ItemCount.Text = "1/";
			} else {
				Console.PrintWarning( string.Format( "Notebook.OnBackpackItemSelected: invalid backpack item category \"{0}\"", category ) );
			}
			ItemStackMax.Text = ( (int)itemType.Get( "max_stack" ) ).ToString();
		}

		private HBoxContainer AddItemToBackpack( HBoxContainer row, Resource stack ) {
			if ( row.GetChildCount() == 4 ) {
				row = new HBoxContainer();
				StackList.AddChild( row );
			}

			TextureRect item = new TextureRect();
			row.AddChild( item );

			item.Connect( "gui_input", Callable.From<InputEvent>( ( inputEvent ) => { OnBackpackItemSelected( inputEvent, item ); } ) );
			item.Texture = (Texture2D)( (Resource)( (Resource)_Owner.GetInventory().Get( "database" ) ).Call( "get_item", (string)stack.Get( "item_id" ) ) ).Get( "icon" );
			item.StretchMode = TextureRect.StretchModeEnum.KeepCentered;
			item.CustomMinimumSize = BackpackItemMinimumSize;
			item.SetMeta( "item_id", (string)stack.Get( "item_id" ) );

			return row;
		}
		private HBoxContainer AddAmmoStackToBackpack( HBoxContainer row, AmmoStack stack ) {
			if ( row.GetChildCount() == 4 ) {
				row = new HBoxContainer();
				StackList.AddChild( row );
			}

			TextureRect item = new TextureRect();
			row.AddChild( item );

			item.Connect( "gui_input", Callable.From<InputEvent>( ( inputEvent ) => { OnBackpackItemSelected( inputEvent, item ); } ) );
			item.Texture = (Texture2D)stack.AmmoType.Data.Get( "icon" );
			item.StretchMode = TextureRect.StretchModeEnum.KeepCentered;
			item.CustomMinimumSize = BackpackItemMinimumSize;
			item.SetMeta( "item_id", (string)stack.AmmoType.Data.Get( "id" ) );

			return row;
		}
		private HBoxContainer AddWeaponToBackpack( HBoxContainer row, WeaponEntity weapon ) {
			if ( row.GetChildCount() == 4 ) {
				row = new HBoxContainer();
				StackList.AddChild( row );
			}

			TextureRect item = new TextureRect();
			row.AddChild( item );
			row.Show();

			item.Connect( "gui_input", Callable.From<InputEvent>( ( inputEvent ) => { OnBackpackItemSelected( inputEvent, item ); } ) );
			item.Texture = (Texture2D)weapon.Data.Get( "icon" );
			item.StretchMode = TextureRect.StretchModeEnum.KeepCentered;
			item.CustomMinimumSize = BackpackItemMinimumSize;
			item.SetMeta( "item_id", (string)weapon.Data.Get( "id" ) );
			item.Show();

			return row;
		}
	};
};
