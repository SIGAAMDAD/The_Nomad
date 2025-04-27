using System.Collections.Generic;
using Godot;
using Renown;

namespace PlayerSystem {
	public partial class Notebook : MarginContainer {
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

		private readonly Godot.Vector2 BackpackItemMinimumSize = new Godot.Vector2( 64.0f, 64.0f );

		private HBoxContainer ContractClonerContainer;
		private VBoxContainer ContractList;

		public override void _ExitTree() {
			base._ExitTree();

			ItemName.QueueFree();
			ItemType.QueueFree();
			ItemCount.QueueFree();
			ItemStackMax.QueueFree();
			ItemIcon.QueueFree();
			ItemDescription.QueueFree();
			ItemEffect.QueueFree();
			StackList.QueueFree();
		}
		public override void _Ready() {
			base._Ready();

			_Owner = GetParent<HeadsUpDisplay>().GetPlayerOwner();

			Backpack = GetNode<TabBar>( "TabContainer/Backpack" );
			Backpack.SetProcess( false );
			Backpack.SetProcessInternal( false );

			StackList = Backpack.GetNode<VBoxContainer>( "MarginContainer/VBoxContainer/HBoxContainer/MarginContainer/VScrollBar/Cloner" );
			StackList.SetProcess( false );
			StackList.SetProcessInternal( false );

			ItemName = Backpack.GetNode<Label>( "MarginContainer/VBoxContainer/HBoxContainer/ItemInfo/HBoxContainer/VBoxContainer/NameLabel" );
			ItemName.SetProcess( false );
			ItemName.SetProcessInternal( false );

			ItemType = Backpack.GetNode<Label>( "MarginContainer/VBoxContainer/HBoxContainer/ItemInfo/HBoxContainer/VBoxContainer/MetaData/TypeContainer/Label" );
			ItemType.SetProcess( false );
			ItemType.SetProcessInternal( false );

			ItemCount = Backpack.GetNode<Label>( "MarginContainer/VBoxContainer/HBoxContainer/ItemInfo/HBoxContainer/VBoxContainer/MetaData/NoHeldContainer/HBoxContainer/CountLabel" );
			ItemCount.SetProcess( false );
			ItemCount.SetProcessInternal( false );

			ItemStackMax = Backpack.GetNode<Label>( "MarginContainer/VBoxContainer/HBoxContainer/ItemInfo/HBoxContainer/VBoxContainer/MetaData/NoHeldContainer/HBoxContainer/MaxLabel" );
			ItemStackMax.SetProcess( false );
			ItemStackMax.SetProcessInternal( false );

			ItemIcon = Backpack.GetNode<TextureRect>( "MarginContainer/VBoxContainer/HBoxContainer/ItemInfo/HBoxContainer/Icon" );
			ItemIcon.SetProcess( false );
			ItemIcon.SetProcessInternal( false );

			ItemDescription = Backpack.GetNode<RichTextLabel>( "MarginContainer/VBoxContainer/HBoxContainer/ItemInfo/DescriptionLabel" );
			ItemDescription.SetProcess( false );
			ItemDescription.SetProcessInternal( false );

			ItemEffect = Backpack.GetNode<Label>( "MarginContainer/VBoxContainer/HBoxContainer/ItemInfo/EffectContainer/Label2" );
			ItemEffect.SetProcess( false );
			ItemEffect.SetProcessInternal( false );

			TabBar Contracts = GetNode<TabBar>( "TabContainer/Contracts" );
			Contracts.SetProcess( false );
			Contracts.SetProcessInternal( false );

			ContractClonerContainer = Contracts.GetNode<HBoxContainer>( "MarginContainer/VBoxContainer//VScrollBar/ContractList/ClonerContainer" );
			ContractClonerContainer.SetProcess( false );
			ContractClonerContainer.SetProcessInternal( false );

			ContractList = Contracts.GetNode<VBoxContainer>( "MarginContainer/VBoxContainer/VScrollBar/ContractList" );
			ContractList.SetProcess( false );
			ContractList.SetProcessInternal( false );
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
				if ( (string)stack.Value.AmmoType.Get( "id" ) == id ) {
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

			Resource itemType = (Resource)( (Resource)_Owner.GetInventory().Get( "database" ) ).Call( "get_item", (string)item.GetMeta( "item_id" ) );
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
			ItemType.Text = (string)categories[0].Get( "name" );

			string category = (string)categories[0].Get( "name" );
			if ( category == "Misc" ) {
				ItemCount.Text = string.Format( "{0}/", GetItemCount( (string)itemType.Get( "id" ) ).ToString() );
			} else if ( category == "Ammo" ) {
				ItemCount.Text = string.Format( "{0}/", GetAmmoCount( (string)itemType.Get( "id" ) ).ToString() );
			} else if ( category == "Weapon" ) {
				ItemCount.Text = "1/";
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
			item.Texture = (Texture2D)( (Resource)( (Resource)_Owner.GetInventory().Get( "database" ) ).Call( "get_item", (string)stack.AmmoType.Get( "id" ) ) ).Get( "icon" );
			item.StretchMode = TextureRect.StretchModeEnum.KeepCentered;
			item.CustomMinimumSize = BackpackItemMinimumSize;
			item.SetMeta( "item_id", (string)stack.AmmoType.Get( "id" ) );

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
			item.Texture = (Texture2D)( (Resource)( (Resource)_Owner.GetInventory().Get( "database" ) ).Call( "get_item", (string)weapon.Data.Get( "id" ) ) ).Get( "icon" );
			item.StretchMode = TextureRect.StretchModeEnum.KeepCentered;
			item.CustomMinimumSize = BackpackItemMinimumSize;
			item.SetMeta( "item_id", (string)weapon.Data.Get( "id" ) );
			item.Show();

			return row;
		}
	};
};
