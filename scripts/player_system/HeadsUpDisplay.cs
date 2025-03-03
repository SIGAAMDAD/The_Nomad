using System.Collections;
using System.ComponentModel;
using GDExtension.Wrappers;
using Godot;

namespace PlayerSystem {
	public partial class HeadsUpDisplay : CanvasLayer {
		[Export]
		public Player _Owner;

		private ProgressBar HealthBar;
		private ProgressBar RageBar;
		private MarginContainer Inventory;
		private VBoxContainer StackList;

		private Label ItemName;
		private Label ItemType;
		private Label ItemCount;
		private Label ItemStackMax;
		private TextureRect ItemIcon;
		private RichTextLabel ItemDescription;
		private Label ItemEffect;

		private TextureRect ReflexOverlay;
		private TextureRect DashOverlay;

		private Timer SaveTimer;
		private Control SaveSpinner;

		private WeaponEntity WeaponData;
		private TextureRect WeaponStatus;
		private TextureRect WeaponModeBladed;
		private TextureRect WeaponModeBlunt;
		private TextureRect WeaponModeFirearm;
		private VBoxContainer WeaponStatusFirearm;
		private VBoxContainer WeaponStatusMelee;
		private TextureRect WeaponStatusMeleeIcon;
		private TextureRect WeaponStatusFirearmIcon;
		private Label WeaponStatusBulletCount;
		private Label WeaponStatusBulletReserve;

		private Control BossHealthBar;

		public TextureRect GetReflexOverlay() {
			return ReflexOverlay;
		}
		public TextureRect GetDashOverlay() {
			return DashOverlay;
		}
		
		private void SaveStart() {
			SaveSpinner.Show();
		}
		private void SaveEnd() {
			SaveTimer.Start();
		}
		private void OnSaveTimerTimeout() {
			SaveSpinner.Hide();
		}

		public override void _Ready() {
			base._Ready();

			ArchiveSystem.Instance.Connect( "SaveGameBegin", Callable.From( SaveStart ) );
			ArchiveSystem.Instance.Connect( "SaveGameEnd", Callable.From( SaveEnd ) );

			HealthBar = GetNode<ProgressBar>( "HealthBar" );
			RageBar = GetNode<ProgressBar>( "RageBar" );
			Inventory = GetNode<MarginContainer>( "Inventory/MarginContainer" );
			StackList = GetNode<VBoxContainer>( "Inventory/MarginContainer/VBoxContainer/HBoxContainer/MarginContainer/Cloner" );

			ItemName = GetNode<Label>( "Inventory/MarginContainer/VBoxContainer/HBoxContainer/ItemInfo/HBoxContainer/VBoxContainer/NameLabel" );
			ItemType = GetNode<Label>( "Inventory/MarginContainer/VBoxContainer/HBoxContainer/ItemInfo/HBoxContainer/VBoxContainer/MetaData/TypeContainer/Label" );
			ItemCount = GetNode<Label>( "Inventory/MarginContainer/VBoxContainer/HBoxContainer/ItemInfo/HBoxContainer/VBoxContainer/MetaData/NoHeldContainer/HBoxContainer/CountLabel" );
			ItemStackMax = GetNode<Label>( "Inventory/MarginContainer/VBoxContainer/HBoxContainer/ItemInfo/HBoxContainer/VBoxContainer/MetaData/NoHeldContainer/HBoxContainer/MaxLabel" );
			ItemIcon = GetNode<TextureRect>( "Inventory/MarginContainer/VBoxContainer/HBoxContainer/ItemInfo/HBoxContainer/Icon" );
			ItemDescription = GetNode<RichTextLabel>( "Inventory/MarginContainer/VBoxContainer/HBoxContainer/ItemInfo/DescriptionLabel" );
			ItemEffect = GetNode<Label>( "Inventory/MarginContainer/VBoxContainer/HBoxContainer/ItemInfo/EffectContainer/Label2" );

			ReflexOverlay = GetNode<TextureRect>( "Overlays/ReflexModeOverlay" );
			DashOverlay = GetNode<TextureRect>( "Overlays/DashOverlay" );

			SaveTimer = GetNode<Timer>( "SaveSpinner/SaveTimer" );
			SaveTimer.Connect( "timeout", Callable.From( OnSaveTimerTimeout ) );

			SaveSpinner = GetNode<Control>( "SaveSpinner/SaveSpinner" );

			WeaponData = null;
			WeaponStatus = GetNode<TextureRect>( "WeaponStatus" );
			WeaponModeBladed = GetNode<TextureRect>( "WeaponStatus/MarginContainer/HBoxContainer/MarginContainer/StatusContainer/StatusBladed" );
			WeaponModeBlunt = GetNode<TextureRect>( "WeaponStatus/MarginContainer/HBoxContainer/MarginContainer/StatusContainer/StatusBlunt" );
			WeaponModeFirearm = GetNode<TextureRect>( "WeaponStatus/MarginContainer/HBoxContainer/MarginContainer/StatusContainer/StatusFirearm" );
			WeaponStatusFirearm = GetNode<VBoxContainer>( "WeaponStatus/MarginContainer/HBoxContainer/FireArmStatus" );
			WeaponStatusMelee = GetNode<VBoxContainer>( "WeaponStatus/MarginContainer/HBoxContainer/MeleeStatus" );
			WeaponStatusMeleeIcon = GetNode<TextureRect>( "WeaponStatus/MarginContainer/HBoxContainer/MeleeStatus/WeaponIcon" );
			WeaponStatusFirearmIcon = GetNode<TextureRect>( "WeaponStatus/MarginContainer/HBoxContainer/FireArmStatus/WeaponIcon" );
			WeaponStatusBulletCount = GetNode<Label>( "WeaponStatus/MarginContainer/HBoxContainer/FireArmStatus/AmmunitionContainer/BulletCountLabel" );
			WeaponStatusBulletReserve = GetNode<Label>( "WeaponStatus/MarginContainer/HBoxContainer/FireArmStatus/AmmunitionContainer/BulletReserveLabel" );

			BossHealthBar = GetNode<Control>( "BossHealthBar" );
		}
		public override void _Process( double delta ) {
			base._Process( delta );

			if ( Engine.TimeScale == 0.0f ) {
				Visible = false;
				return;
			} else {
				Visible = true;
			}

			if ( WeaponData != null ) {
				WeaponModeBladed.Material.Set( "shader_parameter/status_active",
					( WeaponData.GetLastUsedMode() & WeaponEntity.Properties.IsBladed ) != 0 );
				WeaponModeBlunt.Material.Set( "shader_parameter/status_active",
					( WeaponData.GetLastUsedMode() & WeaponEntity.Properties.IsBlunt ) != 0 );
				
				if ( ( WeaponData.GetLastUsedMode() & WeaponEntity.Properties.IsFirearm ) != 0 ) {
					WeaponModeFirearm.Material.Set( "shader_parameter/status_active", true );
					WeaponStatusBulletCount.Text = WeaponData.GetBulletCount().ToString();
					if ( WeaponData.GetReserve() != null ) {
						WeaponStatusBulletReserve.Text = WeaponData.GetReserve().Amount.ToString();
					}
				} else {
					WeaponModeFirearm.Material.Set( "shader_parameter/status_active", false );
				}
			}
		}

        public override void _ExitTree() {
            base._ExitTree();

			HealthBar.QueueFree();
			RageBar.QueueFree();
			Inventory.QueueFree();
			StackList.QueueFree();

			ItemName.QueueFree();
			ItemType.QueueFree();
			ItemCount.QueueFree();
			ItemStackMax.QueueFree();
			ItemIcon.QueueFree();
			ItemDescription.QueueFree();
			ItemEffect.QueueFree();

			ReflexOverlay.QueueFree();
			DashOverlay.QueueFree();

			SaveTimer.QueueFree();
			SaveSpinner.QueueFree();

			WeaponStatus.QueueFree();
			WeaponModeBladed.QueueFree();
			WeaponModeBlunt.QueueFree();
			WeaponModeFirearm.QueueFree();
			WeaponStatusFirearm.QueueFree();
			WeaponStatusMelee.QueueFree();
			WeaponStatusMeleeIcon.QueueFree();
			WeaponStatusFirearmIcon.QueueFree();
			WeaponStatusBulletCount.QueueFree();
			WeaponStatusBulletReserve.QueueFree();

			BossHealthBar.QueueFree();

			QueueFree();
        }

		public void SetWeapon( WeaponEntity weapon ) {
			if ( weapon == null ) {
				WeaponStatus.Hide();
				return;
			} else {
				WeaponStatus.Show();
			}

			if ( ( weapon.GetLastUsedMode() & WeaponEntity.Properties.IsFirearm ) != 0 ) {
				WeaponStatusFirearm.Show();
				WeaponStatusMelee.Show();

				WeaponStatusFirearmIcon.Texture = weapon.GetIcon();
				WeaponStatusBulletCount.Text = weapon.GetBulletCount().ToString();
				if ( weapon.GetReserve() != null ) {
					WeaponStatusBulletReserve.Text = ( (int)weapon.GetReserve().Get( "amount" ) ).ToString();
				} else {
					WeaponStatusBulletReserve.Text = "0";
				}
				WeaponData = weapon;
			}
			else {
				WeaponStatusFirearm.Hide();
				WeaponStatusMelee.Show();
				WeaponStatusMeleeIcon.Texture = weapon.GetIcon();
			}
		}

		private int GetItemCount( string id ) {
			Godot.Collections.Array<ItemStack> stackList = _Owner.GetInventory().Stacks;
			for ( int i = 0; i < stackList.Count; i++ ) {
				if ( stackList[i].ItemId == id ) {
					return stackList[i].Amount;
				}
			}
			return 0;
		}

		private void OnInventoryItemSelected( InputEvent guiEvent, TextureRect item ) {
			if ( !item.HasMeta( "item_id" ) || guiEvent is not InputEventMouseButton ) {
				return;
			} else if ( ( (InputEventMouseButton)guiEvent ).ButtonIndex != MouseButton.Left ) {
				return;
			}

			ItemDefinition itemType = _Owner.GetInventory().GetItemFromId( (string)item.GetMeta( "item_id" ) );
			if ( itemType.Properties.ContainsKey( "description" ) ) {
				ItemDescription.Show();
				ItemDescription.Text = (string)itemType.Properties[ "description" ];
			} else {
				ItemDescription.Hide();
			}

			if ( itemType.Properties.ContainsKey( "effect" ) ) {
				ItemEffect.Show();
				ItemEffect.Text = (string)itemType.Properties[ "effects" ];
			} else {
				ItemEffect.Hide();
			}

			ItemName.Text = itemType.Name;
			ItemIcon.Texture = itemType.Icon;
			ItemType.Text = itemType.Categories[0].Name;

			string category = itemType.Categories[0].Name;
			if ( category == "misc" || category == "ammo" ) {
				ItemCount.Text = GetItemCount( itemType.Id ).ToString();
			} else if ( category == "weapon" ) {
				ItemCount.Text = "1";
			}
			ItemStackMax.Text = itemType.MaxStack.ToString();
		}

		private HBoxContainer AddItemToInventory( HBoxContainer row, Resource stack ) {
			if ( row.GetChildCount() == 4 ) {
				row = new HBoxContainer();
				StackList.AddChild( row );
			}

			TextureRect item = new TextureRect();
			row.AddChild( item );

			item.Connect( "gui_input", Callable.From<InputEvent, TextureRect>( OnInventoryItemSelected ) );
			item.Texture = _Owner.GetInventory().Database.GetItem( (string)stack.Get( "item_id" ) ).Icon;
			item.StretchMode = TextureRect.StretchModeEnum.KeepCentered;
			item.CustomMinimumSize = new Godot.Vector2( 64.0f, 64.0f );
			item.SetMeta( "item_id", (string)stack.Get( "item_id" ) );

			return row;
		}
		private HBoxContainer AddAmmoStackToInventory( HBoxContainer row, AmmoStack stack ) {
			if ( row.GetChildCount() == 4 ) {
				row = new HBoxContainer();
				StackList.AddChild( row );
			}

			TextureRect item = new TextureRect();
			row.AddChild( item );

			item.Connect( "gui_input", Callable.From<InputEvent, TextureRect>( OnInventoryItemSelected ) );
			item.Texture = _Owner.GetInventory().Database.GetItem( (string)stack.AmmoType.Get( "item_id" ) ).Icon;
			item.StretchMode = TextureRect.StretchModeEnum.KeepCentered;
			item.CustomMinimumSize = new Godot.Vector2( 64.0f, 64.0f );
			item.SetMeta( "item_id", (string)stack.AmmoType.Get( "item_id" ) );

			return row;
		}
		private HBoxContainer AddWeaponToInventory( HBoxContainer row, WeaponEntity weapon ) {
			if ( row.GetChildCount() == 4 ) {
				row = new HBoxContainer();
				StackList.AddChild( row );
			}

			TextureRect item = new TextureRect();
			row.AddChild( item );

			item.Connect( "gui_input", Callable.From<InputEvent, TextureRect>( OnInventoryItemSelected ) );
			item.Texture = _Owner.GetInventory().Database.GetItem( (string)weapon.Data.Get( "id" ) ).Icon;
			item.StretchMode = TextureRect.StretchModeEnum.KeepCentered;
			item.CustomMinimumSize = new Godot.Vector2( 64.0f, 64.0f );
			item.SetMeta( "item_id", (string)weapon.Data.Get( "id" ) );

			return row;
		}

		public void OnShowInventory() {
			if ( Inventory.Visible ) {
				Inventory.Visible = false;
				return;
			}

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

			foreach ( var stack in _Owner.GetLightAmmoStacks() ) {
				row = AddAmmoStackToInventory( row, stack );
			}
			foreach ( var stack in _Owner.GetHeavyAmmoStacks() ) {
				row = AddAmmoStackToInventory( row, stack );
			}
			foreach ( var stack in _Owner.GetPelletAmmoStacks() ) {
				row = AddAmmoStackToInventory( row, stack );
			}
			foreach ( var stack in _Owner.GetWeaponStack() ) {
				row = AddWeaponToInventory( row, stack );
			}
			foreach ( var stack in _Owner.GetInventory().Stacks ) {
				row = AddItemToInventory( row, stack );
			}

			Inventory.Visible = true;
		}
    };
};