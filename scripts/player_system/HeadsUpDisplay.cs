using Godot;
using Renown;
using Renown.World;

namespace PlayerSystem {
	public partial class HeadsUpDisplay : CanvasLayer {
		[Export]
		private Player _Owner;

		private Notebook NoteBook;

		private Tween FadeOutTween;
		private Tween FadeInTween;

		private HealthBar HealthBar;
		private RageBar RageBar;

		private MarginContainer AnnouncementContainer;
		private TextureRect AnnouncementBackground;
		private Label AnnouncementText;
		private Timer AnnouncementTimer;

		private Checkpoint CurrentCheckpoint;
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
		private Button ActiveCheckpointButton;
		private VBoxContainer InactiveContainter;
		private VBoxContainer CheckpointMainContainer;
		private VBoxContainer SavedGamesContainer;
		private HBoxContainer MemoryCloner;

		// eagles peak interaction
		private Button JumpYesButton;
		private Button JumpNoButton;
		private TextureRect JumpViewImage;
		private AudioStreamPlayer JumpMusic;
		private Callable OnYesPressed;
		private Callable OnNoPressed;

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

		private Label WorldTimeYear;
		private Label WorldTimeMonth;
		private Label WorldTimeDay;
		private Label WorldTimeHour;
		private Label WorldTimeMinute;

		public HealthBar GetHealthBar() => HealthBar;
		public RageBar GetRageBar() => RageBar;
		public TextureRect GetReflexOverlay() => ReflexOverlay;
		public TextureRect GetDashOverlay() => DashOverlay;
		
		public Player GetPlayerOwner() => _Owner;
		private void SaveStart() {
			SaveSpinner.Show();
		}
		private void SaveEnd() {
			SaveTimer.Start();
		}
		private void OnSaveTimerTimeout() {
			SaveSpinner.Hide();
		}
		private void OnAnnouncementFadeOutTweenFinished() {
			FadeOutTween.Disconnect( "finished", Callable.From( OnAnnouncementFadeOutTweenFinished ) );
			AnnouncementText.Hide();
		}

		private void OnWorldTimeTick( uint day, uint hour, uint minute ) {
			WorldTimeYear.Text = WorldTimeManager.Year.ToString();
			WorldTimeMonth.Text = WorldTimeManager.Month.ToString();
			WorldTimeDay.Text = day.ToString();
			WorldTimeHour.Text = hour.ToString();
			WorldTimeMinute.Text = minute.ToString();
		}

		public override void _Ready() {
			base._Ready();

			if ( GameConfiguration.GameMode != GameMode.Multiplayer
				&& GetTree().CurrentScene.Name == "World" )
			{
				WorldTimeManager.Instance.TimeTick += OnWorldTimeTick;
			}

			ArchiveSystem.Instance.Connect( "SaveGameBegin", Callable.From( SaveStart ) );
			ArchiveSystem.Instance.Connect( "SaveGameEnd", Callable.From( SaveEnd ) );

			NoteBook = GetNode<Notebook>( "NotebookContainer" );
			NoteBook.SetProcess( false );
			NoteBook.SetProcessInternal( false );

			HealthBar = GetNode<HealthBar>( "MainHUD/HealthBar" );
			HealthBar.SetProcess( false );
			HealthBar.SetProcessInternal( false );

			RageBar = GetNode<RageBar>( "MainHUD/RageBar" );
			RageBar.SetProcess( false );
			RageBar.SetProcessInternal( false );

			AnnouncementContainer = GetNode<MarginContainer>( "MainHUD/AnnouncementLabel" );
			AnnouncementContainer.SetProcess( false );
			AnnouncementContainer.SetProcessInternal( false );

			AnnouncementBackground = GetNode<TextureRect>( "MainHUD/AnnouncementLabel/TextureRect" );
			AnnouncementBackground.SetProcess( false );
			AnnouncementBackground.SetProcessInternal( false );

			AnnouncementText = GetNode<Label>( "MainHUD/AnnouncementLabel/TextureRect/Label" );
			AnnouncementText.SetProcess( false );
			AnnouncementText.SetProcessInternal( false );

			AnnouncementTimer = GetNode<Timer>( "MainHUD/AnnouncementLabel/Timer" );
			AnnouncementTimer.SetProcess( false );
			AnnouncementTimer.SetProcessInternal( false );
			AnnouncementTimer.Connect( "timeout", Callable.From( () => {
				FadeOutTween = CreateTween();
				FadeOutTween.TweenProperty( AnnouncementBackground.Material, "shader_parameter/alpha", 0.0f, 2.5f );
				FadeOutTween.Connect( "finished", Callable.From( OnAnnouncementFadeOutTweenFinished ) );
			} ) );

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

			WorldTimeYear = GetNode<Label>( "MainHUD/WorldTimeContainer/VBoxContainer/HBoxContainer/YearLabel" );
			WorldTimeYear.SetProcess( false );
			WorldTimeYear.SetProcessInternal( false );

			WorldTimeMonth = GetNode<Label>( "MainHUD/WorldTimeContainer/VBoxContainer/HBoxContainer/MonthLabel" );
			WorldTimeMonth.SetProcess( false );
			WorldTimeMonth.SetProcessInternal( false );

			WorldTimeDay = GetNode<Label>( "MainHUD/WorldTimeContainer/VBoxContainer/HBoxContainer2/DayLabel" );
			WorldTimeDay.SetProcess( false );
			WorldTimeDay.SetProcessInternal( false );

			WorldTimeHour = GetNode<Label>( "MainHUD/WorldTimeContainer/VBoxContainer/HBoxContainer2/HourLabel" );
			WorldTimeHour.SetProcess( false );
			WorldTimeHour.SetProcessInternal( false );

			WorldTimeMinute = GetNode<Label>( "MainHUD/WorldTimeContainer/VBoxContainer/HBoxContainer2/MinuteLabel" );
			WorldTimeMinute.SetProcess( false );
			WorldTimeMinute.SetProcessInternal( false );

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

			ActiveCheckpointButton = GetNode<Button>( "CheckpointContainer/VBoxContainer/MarginContainer/InactiveContainer/Button" );
			ActiveCheckpointButton.SetProcess( false );
			ActiveCheckpointButton.SetProcessInternal( false );
			ActiveCheckpointButton.Connect( "pressed", Callable.From( () => {
				CurrentCheckpoint.Activate();
				ShowAnnouncement( "ACQUIRED_MEMORY" );
			} ) );

			MemoryCloner = GetNode<HBoxContainer>( "CheckpointContainer/VBoxContainer/MarginContainer/SavedGamesContainer/Cloner" );
			InactiveContainter = GetNode<VBoxContainer>( "CheckpointContainer/VBoxContainer/MarginContainer/InactiveContainer" );
			CheckpointMainContainer = GetNode<VBoxContainer>( "CheckpointContainer/VBoxContainer/MarginContainer/MainContainer" );
			SavedGamesContainer = GetNode<VBoxContainer>( "CheckpointContainer/VBoxContainer/MarginContainer/SavedGamesContainer" );
			WarpLocationsContainer = GetNode<VScrollBar>( "CheckpointContainer/VBoxContainer/MarginContainer/VScrollBar" );

			JumpInteractor = GetNode<MarginContainer>( "JumpContainer" );
			JumpInteractor.SetProcess( false );
			JumpInteractor.SetProcessInternal( false );

			JumpViewImage = GetNode<TextureRect>( "JumpContainer/ViewImage" );
			JumpViewImage.SetProcess( false );
			JumpViewImage.SetProcessInternal( false );

			JumpYesButton = GetNode<Button>( "JumpContainer/JumpQueryContainer/VBoxContainer/YesButton" );
			JumpYesButton.SetProcess( false );
			JumpYesButton.SetProcessInternal( false );
			
			JumpNoButton = GetNode<Button>( "JumpContainer/JumpQueryContainer/VBoxContainer/NoButton" );
			JumpNoButton.SetProcess( false );
			JumpNoButton.SetProcessInternal( false );

			JumpMusic = GetNode<AudioStreamPlayer>( "JumpContainer/Theme" );
			JumpMusic.SetProcess( false );
			JumpMusic.SetProcessInternal( false );
			JumpMusic.Set( "parameters/looping", true );

			BossHealthBar = GetNode<Control>( "MainHUD/BossHealthBar" );

			_Owner.Damaged += ( Entity source, Entity target, float nAmount ) => { HealthBar.SetHealth( target.GetHealth() ); };
			_Owner.SwitchedWeapon += SetWeapon;
			_Owner.SwitchedWeaponMode += ( WeaponEntity source, WeaponEntity.Properties useMode ) => {
				WeaponModeBladed.Material.Set( "shader_parameter/status_active",
					( source.GetLastUsedMode() & WeaponEntity.Properties.IsBladed ) != 0 );
				WeaponModeBlunt.Material.Set( "shader_parameter/status_active",
					( source.GetLastUsedMode() & WeaponEntity.Properties.IsBlunt ) != 0 );
				
				if ( ( source.GetLastUsedMode() & WeaponEntity.Properties.IsFirearm ) != 0 ) {
					WeaponModeFirearm.Material.Set( "shader_parameter/status_active", true );
					WeaponStatusBulletCount.Text = source.GetBulletCount().ToString();
					if ( source.GetReserve() != null ) {
						WeaponStatusBulletReserve.Text = source.GetReserve().Amount.ToString();
					}
				} else {
					WeaponModeFirearm.Material.Set( "shader_parameter/status_active", false );
				}
			};

			HealthBar.Init( 100.0f );
			RageBar.Init( 60.0f );
		}
        public override void _ExitTree() {
            base._ExitTree();

			HealthBar.QueueFree();
			RageBar.QueueFree();

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

		private void OnWeaponReloaded( WeaponEntity source ) {
			WeaponStatusBulletCount.Text = source.GetBulletCount().ToString();
			WeaponStatusBulletReserve.Text = source.GetReserve() != null ? source.GetReserve().Amount.ToString() : "0";
		}
		private void OnWeaponUsed( WeaponEntity source ) {
			if ( ( source.GetLastUsedMode() & WeaponEntity.Properties.IsFirearm ) != 0 ) {
				WeaponStatusBulletCount.Text = source.GetBulletCount().ToString();
			}
		}
		private void SetWeapon( WeaponEntity weapon ) {
			if ( WeaponData != weapon && WeaponData != null ) {
				WeaponData.Reloaded -= OnWeaponReloaded;
				WeaponData.Used -= OnWeaponUsed;
			}

			if ( weapon == null ) {
				WeaponStatus.Hide();
				return;
			} else {
				WeaponStatus.Show();
			}

			if ( ( weapon.GetLastUsedMode() & WeaponEntity.Properties.IsFirearm ) != 0 ) {
				WeaponStatusFirearm.Show();
				WeaponStatusMelee.Hide();

				WeaponStatusFirearmIcon.Texture = weapon.GetIcon();

				WeaponStatusBulletCount.Text = weapon.GetBulletCount().ToString();
				WeaponStatusBulletReserve.Text = weapon.GetReserve() != null ? weapon.GetReserve().Amount.ToString() : "0";
			} else {
				WeaponStatusFirearm.Hide();
				WeaponStatusMelee.Show();
				WeaponStatusMeleeIcon.Texture = weapon.GetIcon();
			}
			
			WeaponData = weapon;
			WeaponData.Reloaded += OnWeaponReloaded;
			WeaponData.Used += OnWeaponUsed;
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

		private void OnJumpAudioTweenFadeOutFinished() {
			JumpMusic.Stop();
			JumpMusic.VolumeDb = 0.0f;
		}

		public void ShowInteraction( InteractionItem item ) {
			switch ( item.GetInteractionType() ) {
			case InteractionType.Checkpoint:
				CurrentCheckpoint = (Checkpoint)item;
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
				CheckpointNameLabel.Text = CurrentCheckpoint.GetTitle();
				if ( CurrentCheckpoint.GetActivated() ) {
					InactiveContainter.Hide();
					CheckpointMainContainer.Show();
				} else {
					InactiveContainter.Show();
					CheckpointMainContainer.Hide();
				}
			} else if ( CurrentInteractor == JumpInteractor ) {
				EaglesPeak data = (EaglesPeak)item;

				OnYesPressed = Callable.From( data.OnYesButtonPressed );
				OnNoPressed = Callable.From( data.OnNoButtonPressed );

				JumpYesButton.Connect( "pressed", OnYesPressed );
				JumpNoButton.Connect( "pressed", OnNoPressed );

				JumpViewImage.Texture = data.GetViewImage();
				JumpMusic.Stream = data.GetMusic();

				JumpMusic.Play();
			}
		}
		public void HideInteraction() {
			if ( CurrentInteractor == CheckpointInteractor ) {
				CheckpointMainContainer.Show();
				WarpLocationsContainer.Hide();
				SavedGamesContainer.Hide();
			} else if ( CurrentInteractor == JumpInteractor ) {
				Tween AudioTween = CreateTween();
				AudioTween.TweenProperty( JumpMusic, "volume_db", -20.0f, 1.5f );
				AudioTween.Connect( "finished", Callable.From( OnJumpAudioTweenFadeOutFinished ) );

				JumpYesButton.Disconnect( "pressed", OnYesPressed );
				JumpNoButton.Disconnect( "pressed", OnNoPressed );
			}
			CurrentInteractor?.Hide();
			CurrentInteractor = null;
		}

		public void ShowAnnouncement( string text ) {
			AnnouncementContainer.Show();

			AnnouncementTimer.Start();

			AnnouncementBackground.Material.Set( "shader_parameter/alpha", 0.0f );
			AnnouncementText.Text = text;
			AnnouncementText.Show();
			AnnouncementTimer.Start();

			FadeInTween = CreateTween();
			FadeInTween.TweenProperty( AnnouncementBackground.Material, "shader_parameter/alpha", 0.90f, 2.5f );
		}

		public bool IsNotebookOpen() => NoteBook.Visible;
		public void OnShowInventory() {
			if ( NoteBook.Visible ) {
				NoteBook.Visible = false;
				GetNode<Control>( "MainHUD" ).MouseFilter = Control.MouseFilterEnum.Ignore;
				return;
			}
			GetNode<Control>( "MainHUD" ).MouseFilter = Control.MouseFilterEnum.Stop;

			NoteBook.Visible = true;
			NoteBook.OnShowInventory();
		}
    };
};