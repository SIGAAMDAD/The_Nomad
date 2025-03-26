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

		private MarginContainer CheckpointInteractor;
		private MarginContainer JumpInteractor;
		private MarginContainer CurrentInteractor;

		// checkpoint interaction
		private Button SaveGameButton;
		private Button LoadGameButton;
		private Button OpenStorageButton;
		private Button RestHereButton;
		private Button WarpButton;
		private Label CheckpointNameLabel;
		private HBoxContainer WarpCloner;
		private VScrollBar WarpLocationsContainer;
		private VBoxContainer CheckpointMainContainer;
		private VBoxContainer SavedGamesContainer;
		private HBoxContainer MemoryCloner;

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

		private InteractionItem InteractionData;

		private Control BossHealthBar;

		public HealthBar GetHealthBar() => (HealthBar)HealthBar;
		public RageBar GetRageBar() => (RageBar)RageBar;
		public TextureRect GetReflexOverlay() => ReflexOverlay;
		public TextureRect GetDashOverlay() => DashOverlay;
		
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

			HealthBar = GetNode<ProgressBar>( "MainHUD/HealthBar" );
			HealthBar.SetProcess( false );
			HealthBar.SetProcessInternal( false );

			RageBar = GetNode<ProgressBar>( "MainHUD/RageBar" );
			RageBar.SetProcess( false );
			RageBar.SetProcessInternal( false );

			Inventory = GetNode<MarginContainer>( "StatusControl/MarginContainer/TabContainer/Inventory/MarginContainer" );
			Inventory.SetProcess( false );
			Inventory.SetProcessInternal( false );

			StackList = GetNode<VBoxContainer>( "StatusControl/MarginContainer/TabContainer/Inventory/MarginContainer/VBoxContainer/HBoxContainer/MarginContainer/Cloner" );
			StackList.SetProcess( false );
			StackList.SetProcessInternal( false );

			ItemName = GetNode<Label>( "StatusControl/MarginContainer/TabContainer/Inventory/MarginContainer/VBoxContainer/HBoxContainer/ItemInfo/HBoxContainer/VBoxContainer/NameLabel" );
			ItemName.SetProcess( false );
			ItemName.SetProcessInternal( false );

			ItemType = GetNode<Label>( "StatusControl/MarginContainer/TabContainer/Inventory/MarginContainer/VBoxContainer/HBoxContainer/ItemInfo/HBoxContainer/VBoxContainer/MetaData/TypeContainer/Label" );
			ItemType.SetProcess( false );
			ItemType.SetProcessInternal( false );

			ItemCount = GetNode<Label>( "StatusControl/MarginContainer/TabContainer/Inventory/MarginContainer/VBoxContainer/HBoxContainer/ItemInfo/HBoxContainer/VBoxContainer/MetaData/NoHeldContainer/HBoxContainer/CountLabel" );
			ItemCount.SetProcess( false );
			ItemCount.SetProcessInternal( false );

			ItemStackMax = GetNode<Label>( "StatusControl/MarginContainer/TabContainer/Inventory/MarginContainer/VBoxContainer/HBoxContainer/ItemInfo/HBoxContainer/VBoxContainer/MetaData/NoHeldContainer/HBoxContainer/MaxLabel" );
			ItemStackMax.SetProcess( false );
			ItemStackMax.SetProcessInternal( false );

			ItemIcon = GetNode<TextureRect>( "StatusControl/MarginContainer/TabContainer/Inventory/MarginContainer/VBoxContainer/HBoxContainer/ItemInfo/HBoxContainer/Icon" );
			ItemIcon.SetProcess( false );
			ItemIcon.SetProcessInternal( false );

			ItemDescription = GetNode<RichTextLabel>( "StatusControl/MarginContainer/TabContainer/Inventory/MarginContainer/VBoxContainer/HBoxContainer/ItemInfo/DescriptionLabel" );
			ItemDescription.SetProcess( false );
			ItemDescription.SetProcessInternal( false );

			ItemEffect = GetNode<Label>( "StatusControl/MarginContainer/TabContainer/Inventory/MarginContainer/VBoxContainer/HBoxContainer/ItemInfo/EffectContainer/Label2" );
			ItemEffect.SetProcess( false );
			ItemEffect.SetProcessInternal( false );

			CheckpointInteractor = GetNode<MarginContainer>( "CheckpointContainer" );
			CheckpointInteractor.SetProcess( false );
			CheckpointInteractor.SetProcessInternal( false );

			ReflexOverlay = GetNode<TextureRect>( "MainHUD/Overlays/ReflexModeOverlay" );
			ReflexOverlay.SetProcess( false );
			ReflexOverlay.SetProcessInternal( false );

			DashOverlay = GetNode<TextureRect>( "MainHUD/Overlays/DashOverlay" );
			DashOverlay.SetProcess( false );
			DashOverlay.SetProcessInternal( false );
			DashOverlay.SetPhysicsProcess( false );

			SaveTimer = GetNode<Timer>( "MainHUD/SaveSpinner/SaveTimer" );
			SaveTimer.SetProcess( false );
			SaveTimer.SetProcessInternal( false );
			SaveTimer.Connect( "timeout", Callable.From( OnSaveTimerTimeout ) );

			SaveSpinner = GetNode<Control>( "MainHUD/SaveSpinner/SaveSpinner" );
			SaveSpinner.SetProcess( false );

			WeaponData = null;
			WeaponStatus = GetNode<TextureRect>( "MainHUD/WeaponStatus" );
			WeaponStatus.SetProcess( false );
			WeaponStatus.SetProcessInternal( false );

			WeaponModeBladed = GetNode<TextureRect>( "MainHUD/WeaponStatus/MarginContainer/HBoxContainer/MarginContainer/StatusContainer/StatusBladed" );
			WeaponModeBladed.SetProcess( false );
			WeaponModeBladed.SetProcessInternal( false );

			WeaponModeBlunt = GetNode<TextureRect>( "MainHUD/WeaponStatus/MarginContainer/HBoxContainer/MarginContainer/StatusContainer/StatusBlunt" );
			WeaponModeBlunt.SetProcess( false );
			WeaponModeBlunt.SetProcessInternal( false );

			WeaponModeFirearm = GetNode<TextureRect>( "MainHUD/WeaponStatus/MarginContainer/HBoxContainer/MarginContainer/StatusContainer/StatusFirearm" );
			WeaponModeFirearm.SetProcess( false );
			WeaponModeFirearm.SetProcessInternal( false );

			WeaponStatusFirearm = GetNode<VBoxContainer>( "MainHUD/WeaponStatus/MarginContainer/HBoxContainer/FireArmStatus" );
			WeaponStatusFirearm.SetProcess( false );
			WeaponStatusFirearm.SetProcessInternal( false );

			WeaponStatusMelee = GetNode<VBoxContainer>( "MainHUD/WeaponStatus/MarginContainer/HBoxContainer/MeleeStatus" );
			WeaponStatusMelee.SetProcess( false );
			WeaponStatusMelee.SetProcessInternal( false );

			WeaponStatusMeleeIcon = GetNode<TextureRect>( "MainHUD/WeaponStatus/MarginContainer/HBoxContainer/MeleeStatus/WeaponIcon" );
			WeaponStatusMeleeIcon.SetProcess( false );
			WeaponStatusMeleeIcon.SetProcessInternal( false );

			WeaponStatusFirearmIcon = GetNode<TextureRect>( "MainHUD/WeaponStatus/MarginContainer/HBoxContainer/FireArmStatus/WeaponIcon" );
			WeaponStatusFirearmIcon.SetProcess( false );
			WeaponStatusFirearmIcon.SetProcessInternal( false );

			WeaponStatusBulletCount = GetNode<Label>( "MainHUD/WeaponStatus/MarginContainer/HBoxContainer/FireArmStatus/AmmunitionContainer/BulletCountLabel" );
			WeaponStatusBulletCount.SetProcess( false );
			WeaponStatusBulletCount.SetProcessInternal( false );

			WeaponStatusBulletReserve = GetNode<Label>( "MainHUD/WeaponStatus/MarginContainer/HBoxContainer/FireArmStatus/AmmunitionContainer/BulletReserveLabel" );
			WeaponStatusBulletReserve.SetProcess( false );
			WeaponStatusBulletReserve.SetProcessInternal( false );

			SaveGameButton = GetNode<Button>( "CheckpointContainer/VBoxContainer/MarginContainer/MainContainer/SaveProgressButton" );
//			SaveGameButton.Connect( "pressed", Callable.From( OnSaveGameButtonPressed ) );

			LoadGameButton = GetNode<Button>( "CheckpointContainer/VBoxContainer/MarginContainer/MainContainer/LoadProgressButton" );
//			LoadGameButton.Connect( "pressed", Callable.From( OnLoadGameButtonPressed ) );

			RestHereButton = GetNode<Button>( "CheckpointContainer/VBoxContainer/MarginContainer/MainContainer/RestHereButton" );
			RestHereButton.Connect( "pressed", Callable.From( OnRestHereButtonPressed ) );

			WarpButton = GetNode<Button>( "CheckpointContainer/VBoxContainer/MarginContainer/MainContainer/WarpButton" );
			WarpButton.Connect( "pressed", Callable.From( OnWarpButtonPressed ) );

			WarpCloner = GetNode<HBoxContainer>( "CheckpointContainer/VBoxContainer/MarginContainer/VScrollBar/WarpLocationsContainer/Cloner" );
			CheckpointNameLabel = GetNode<Label>( "CheckpointContainer/VBoxContainer/CheckpointNameLabel" );

			MemoryCloner = GetNode<HBoxContainer>( "CheckpointContainer/VBoxContainer/MarginContainer/SavedGamesContainer/Cloner" );
			CheckpointMainContainer = GetNode<VBoxContainer>( "CheckpointContainer/VBoxContainer/MarginContainer/MainContainer" );
			SavedGamesContainer = GetNode<VBoxContainer>( "CheckpointContainer/VBoxContainer/MarginContainer/SavedGamesContainer" );
			WarpLocationsContainer = GetNode<VScrollBar>( "CheckpointContainer/VBoxContainer/MarginContainer/VScrollBar" );

			JumpInteractor = GetNode<MarginContainer>( "JumpContainer" );

			BossHealthBar = GetNode<Control>( "MainHUD/BossHealthBar" );

			( (HealthBar)HealthBar ).Init( 100.0f );
			( (RageBar)RageBar ).Init( 60.0f );
		}
		public override void _Process( double delta ) {
			if ( ( Engine.GetProcessFrames() % 60 ) != 0 ) {
				return;
			}
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
				WeaponStatus.CallDeferred( "hide" );
				return;
			} else {
				WeaponStatus.CallDeferred( "show" );
			}

			if ( ( weapon.GetLastUsedMode() & WeaponEntity.Properties.IsFirearm ) != 0 ) {
				WeaponStatusFirearm.CallDeferred( "show" );
				WeaponStatusMelee.CallDeferred( "hide" );

				WeaponStatusFirearmIcon.SetDeferred( "texture", weapon.GetIcon() );
				WeaponStatusBulletCount.SetDeferred( "text", weapon.GetBulletCount().ToString() );
				if ( weapon.GetReserve() != null ) {
					WeaponStatusBulletReserve.SetDeferred( "text", ( (int)weapon.GetReserve().Get( "amount" ) ).ToString() );
				} else {
					WeaponStatusBulletReserve.SetDeferred( "text", "0" );
				}
				WeaponData = weapon;
			}
			else {
				WeaponStatusFirearm.CallDeferred( "hide" );
				WeaponStatusMelee.CallDeferred( "show" );
				WeaponStatusMeleeIcon.SetDeferred( "texture", weapon.GetIcon() );
			}
		}

		private int GetItemCount( string id ) {
			Godot.Collections.Array<ItemStack> stackList = (Godot.Collections.Array<ItemStack>)_Owner.GetInventory().Get( "stacks" );
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

			ItemDefinition itemType = (ItemDefinition)(Resource)_Owner.GetInventory().Call( "get_item_from_id", (string)item.GetMeta( "item_id" ) );
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
//			item.Texture = _Owner.GetInventory().Database.GetItem( (string)stack.Get( "item_id" ) ).Icon;
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
//			item.Texture = _Owner.GetInventory().Database.GetItem( (string)stack.AmmoType.Get( "item_id" ) ).Icon;
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
//			item.Texture = _Owner.GetInventory().Database.GetItem( (string)weapon.Data.Get( "id" ) ).Icon;
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

			foreach ( var stack in _Owner.GetAmmoStacks() ) {
				row = AddAmmoStackToInventory( row, stack.Value );
			}
			foreach ( var stack in _Owner.GetWeaponStack() ) {
				row = AddWeaponToInventory( row, stack.Value );
			}
//			foreach ( var stack in _Owner.GetInventory().Stacks ) {
//				row = AddItemToInventory( row, stack );
//			}

			Inventory.Visible = true;
		}

		private void OnWarpButtonPressed() {
			CheckpointMainContainer.Hide();

			for ( int i = 0; i < WarpLocationsContainer.GetChildCount(); i++ ) {
				WarpLocationsContainer.GetChild( i ).QueueFree();
				WarpLocationsContainer.RemoveChild( WarpLocationsContainer.GetChild( i ) );
			}
			/*
			List<Player.WarpPoint> warpList = _Owner.GetWarpPoints();
			for ( int i = 0; i < warpList.Count; i++ ) {
				HBoxContainer warpPoint = new HBoxContainer();
				( (TextureRect)warpPoint.GetChild( 0 ) ).Texture = warpList[i].GetIcon();
				( (Button)warpPoint.GetChild( 1 ) ).Text = warpList[i].GetLocation().GetTitle();
				( (Label)warpPoint.GetChild( 2 ) ).Text = "N/A";
				WarpLocationsContainer.AddChild( warpPoint );
			}
			*/

			WarpLocationsContainer.Show();
		}
		private void OnRestHereButtonPressed() {
			_Owner.SetHealth( 100.0f );
			_Owner.SetRage( 100.0f );

			ArchiveSystem.SaveGame( null, 0 );
		}

		/*
		private void OnSaveGameButtonPressed() {
//			Hide();
//			RenderingServer.ForceDraw();
			ArchiveSystem.SaveGame( GetViewport().GetTexture().GetImage(), 0 );
//			Show();
		}
		private void OnLoadGameButtonPressed() {
			CheckpointMainContainer.Hide();

			SaveSystem.Slot slot = ArchiveSystem.GetSlot();
			System.Collections.Generic.List<SaveSystem.Slot.MemoryMetadata> memoryList = slot.GetMemoryList();
			for ( int i = 0; i < SavedGamesContainer.GetChildCount(); i++ ) {
				SavedGamesContainer.GetChild( i ).QueueFree();
				SavedGamesContainer.RemoveChild( SavedGamesContainer.GetChild( i ) );
			}

			for ( int i = 0; i < memoryList.Count; i++ ) {
				HBoxContainer memory = new HBoxContainer();
				Texture2D texture = ResourceLoader.Load<Texture2D>( slot.GetPath() + "/Memories/Screenshot_" + slot.GetCurrentMemory() + ".png" );

				( (TextureRect)memory.GetChild( 0 ) ).Texture = texture;
				( (Label)memory.GetChild( 0 ) ).Text = memoryList[i].Month.ToString() + " " + memoryList[i].Day.ToString() + ", " + memoryList[i].Year.ToString();

				SavedGamesContainer.AddChild( memory );
			}

			SavedGamesContainer.Show();
		}
		*/

		public void ShowInteraction( InteractionItem item ) {
			switch ( item.GetInteractionType() ) {
			case InteractionType.Checkpoint:
				CurrentInteractor = CheckpointInteractor;
				break;
			case InteractionType.EaglesPeak:
				CurrentInteractor = JumpInteractor;
				break;
			};
			
			if ( CurrentInteractor != null ) {
				CurrentInteractor.Show();
			} else {
				return;
			}
			
			if ( CurrentInteractor == CheckpointInteractor ) {
				CheckpointNameLabel.Text = ( (Checkpoint)item ).GetTitle();
			}
		}
		public void HideInteraction() {
			if ( CurrentInteractor == CheckpointInteractor ) {
				CheckpointMainContainer.Show();
				WarpLocationsContainer.Hide();
				SavedGamesContainer.Hide();
			}
			if ( CurrentInteractor != null ) {
				CurrentInteractor.Hide();
			}
			CurrentInteractor = null;
		}
    };
};