using System.Collections.Generic;
using DialogueManagerRuntime;
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

		private CheckpointInteractor CheckpointInteractor;
		private MarginContainer JumpInteractor;
		private MarginContainer CurrentInteractor;
		private MarginContainer DoorInteractor;

		// eagles peak interaction
		private Button JumpYesButton;
		private Button JumpNoButton;
		private TextureRect JumpViewImage;
		private AudioStreamPlayer JumpMusic;
		private Callable OnYesPressed;
		private Callable OnNoPressed;

		private TextureRect LeftArmIndicator;
		private TextureRect RightArmIndicator;

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

		private RichTextLabel DialogueLabel;

		private Dictionary<string, StatusIcon> StatusIcons;

		private Control HellbreakerOverlay;

		public HealthBar GetHealthBar() => HealthBar;
		public RageBar GetRageBar() => RageBar;
		public TextureRect GetReflexOverlay() => ReflexOverlay;
		public TextureRect GetDashOverlay() => DashOverlay;
		
		public Player GetPlayerOwner() => _Owner;
		private void SaveStart() {
			SaveSpinner.SetProcess( true );
			SaveSpinner.Show();
		}
		private void SaveEnd() {
			SaveTimer.Start();
		}
		private void OnSaveTimerTimeout() {
			SaveSpinner.Hide();
			SaveSpinner.SetProcess( false );
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

		public static void StartThoughtBubble( string text ) {
			Resource dialogue = DialogueManager.CreateResourceFromText( string.Format( "~ thought_bubble\n{0}", text ) );
			DialogueManager.ShowDialogueBalloon( dialogue, "thought_bubble" );
		}

		private void OnDialogueEnded( Resource dialogueResource ) {
		}
		private void OnDialogueStarted( Resource dialogueResource ) {
		}

		private void OnOpenDoorButtonPressed() {
			if ( InteractionData is Door door && door != null ) {
			}
		}

		public Checkpoint GetCurrentCheckpoint() => CheckpointInteractor.GetCurrentCheckpoint();

		public override void _Ready() {
			base._Ready();

			StatusIcons = new Dictionary<string, StatusIcon>{
				{ "status_burning", GetNode<StatusIcon>( "MainHUD/HBoxContainer/BurningStatusIcon" ) },
				{ "status_freezing", GetNode<StatusIcon>( "MainHUD/HBoxContainer/FreezingStatusIcon" ) },
				{ "status_poisoned", GetNode<StatusIcon>( "MainHUD/HBoxContainer/PoisonedStatusIcon" ) },
			};

			DialogueManager.DialogueStarted += OnDialogueStarted;
			DialogueManager.DialogueEnded += OnDialogueEnded;

			if ( GameConfiguration.GameMode != GameMode.Multiplayer
				&& GetTree().CurrentScene.Name == "World" )
			{
				WorldTimeManager.Instance.TimeTick += OnWorldTimeTick;
			}

			ArchiveSystem.Instance.Connect( "SaveGameBegin", Callable.From( SaveStart ) );
			ArchiveSystem.Instance.Connect( "SaveGameEnd", Callable.From( SaveEnd ) );

			DialogueLabel = GetNode<RichTextLabel>( "DialogueLabel" );

			NoteBook = GetNode<Notebook>( "NotebookContainer" );
			NoteBook.ProcessMode = ProcessModeEnum.Disabled;

			HealthBar = GetNode<HealthBar>( "MainHUD/HealthBar" );
			RageBar = GetNode<RageBar>( "MainHUD/RageBar" );

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

			CheckpointInteractor = GetNode<CheckpointInteractor>( "CheckpointContainer" );

			ReflexOverlay = GetNode<TextureRect>( "MainHUD/Overlays/ReflexModeOverlay" );
			DashOverlay = GetNode<TextureRect>( "MainHUD/Overlays/DashOverlay" );

			HellbreakerOverlay = GetNode<Control>( "MainHUD/HellbreakerOverlay" );
			LevelData.Instance.HellbreakerBegin += HellbreakerOverlay.Show;
			LevelData.Instance.HellbreakerFinished += HellbreakerOverlay.Hide;

			WorldTimeYear = GetNode<Label>( "MainHUD/WorldTimeContainer/VBoxContainer/HBoxContainer/YearLabel" );
			WorldTimeMonth = GetNode<Label>( "MainHUD/WorldTimeContainer/VBoxContainer/HBoxContainer/MonthLabel" );
			WorldTimeDay = GetNode<Label>( "MainHUD/WorldTimeContainer/VBoxContainer/HBoxContainer2/DayLabel" );
			WorldTimeHour = GetNode<Label>( "MainHUD/WorldTimeContainer/VBoxContainer/HBoxContainer2/HourLabel" );
			WorldTimeMinute = GetNode<Label>( "MainHUD/WorldTimeContainer/VBoxContainer/HBoxContainer2/MinuteLabel" );

			SaveTimer = GetNode<Timer>( "MainHUD/SaveSpinner/SaveTimer" );
			SaveTimer.SetProcess( false );
			SaveTimer.SetProcessInternal( false );
			SaveTimer.Connect( "timeout", Callable.From( OnSaveTimerTimeout ) );

			SaveSpinner = GetNode<Control>( "MainHUD/SaveSpinner/SaveSpinner" );
			SaveSpinner.SetProcess( false );

			LeftArmIndicator = GetNode<TextureRect>( "MainHUD/WeaponStatus/MarginContainer/VBoxContainer/ArmUsage/LeftArmIndicator" );
			RightArmIndicator = GetNode<TextureRect>( "MainHUD/WeaponStatus/MarginContainer/VBoxContainer/ArmUsage/RightArmIndicator" );

			WeaponData = null;
			WeaponStatus = GetNode<TextureRect>( "MainHUD/WeaponStatus" );
			WeaponModeBladed = GetNode<TextureRect>( "MainHUD/WeaponStatus/MarginContainer/VBoxContainer/HBoxContainer/MarginContainer/StatusContainer/StatusBladed" );
			WeaponModeBlunt = GetNode<TextureRect>( "MainHUD/WeaponStatus/MarginContainer/VBoxContainer/HBoxContainer/MarginContainer/StatusContainer/StatusBlunt" );
			WeaponModeFirearm = GetNode<TextureRect>( "MainHUD/WeaponStatus/MarginContainer/VBoxContainer/HBoxContainer/MarginContainer/StatusContainer/StatusFirearm" );
			WeaponStatusFirearm = GetNode<VBoxContainer>( "MainHUD/WeaponStatus/MarginContainer/VBoxContainer/HBoxContainer/FireArmStatus" );
			WeaponStatusMelee = GetNode<VBoxContainer>( "MainHUD/WeaponStatus/MarginContainer/VBoxContainer/HBoxContainer/MeleeStatus" );
			WeaponStatusMeleeIcon = GetNode<TextureRect>( "MainHUD/WeaponStatus/MarginContainer/VBoxContainer/HBoxContainer/MeleeStatus/WeaponIcon" );
			WeaponStatusFirearmIcon = GetNode<TextureRect>( "MainHUD/WeaponStatus/MarginContainer/VBoxContainer/HBoxContainer/FireArmStatus/WeaponIcon" );
			WeaponStatusBulletCount = GetNode<Label>( "MainHUD/WeaponStatus/MarginContainer/VBoxContainer/HBoxContainer/FireArmStatus/AmmunitionContainer/BulletCountLabel" );
			WeaponStatusBulletReserve = GetNode<Label>( "MainHUD/WeaponStatus/MarginContainer/VBoxContainer/HBoxContainer/FireArmStatus/AmmunitionContainer/BulletReserveLabel" );

			JumpInteractor = GetNode<MarginContainer>( "JumpContainer" );
			JumpViewImage = GetNode<TextureRect>( "JumpContainer/ViewImage" );
			JumpYesButton = GetNode<Button>( "JumpContainer/JumpQueryContainer/VBoxContainer/YesButton" );
			JumpNoButton = GetNode<Button>( "JumpContainer/JumpQueryContainer/VBoxContainer/NoButton" );
			JumpMusic = GetNode<AudioStreamPlayer>( "JumpContainer/Theme" );
			JumpMusic.Set( "parameters/looping", true );

			DoorInteractor = GetNode<MarginContainer>( "DoorContainer" );
			Button OpenButton = GetNode<Button>( "DoorContainer/MarginContainer/OpenButton" );
			OpenButton.Connect( "pressed", Callable.From( OnOpenDoorButtonPressed ) );

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

		public void AddStatusEffect( string effectName ) {

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
				WeaponStatus.ProcessMode = ProcessModeEnum.Disabled;
				return;
			} else {
				WeaponStatus.Show();
				WeaponStatus.ProcessMode = ProcessModeEnum.Pausable;
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
				CheckpointInteractor.BeginInteraction( item );
				CurrentInteractor = CheckpointInteractor;
				break;
			case InteractionType.Door:
				CurrentInteractor = DoorInteractor;
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
			
			if ( CurrentInteractor == JumpInteractor ) {
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
				CheckpointInteractor.EndInteraction();
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
				NoteBook.ProcessMode = ProcessModeEnum.Disabled;
				GetNode<Control>( "MainHUD" ).MouseFilter = Control.MouseFilterEnum.Ignore;
				return;
			}
			GetNode<Control>( "MainHUD" ).MouseFilter = Control.MouseFilterEnum.Stop;

			NoteBook.ProcessMode = ProcessModeEnum.Pausable;
			NoteBook.Visible = true;
			NoteBook.OnShowBackpack();
		}
    };
};