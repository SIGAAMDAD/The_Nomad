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

/*
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DialogueManagerRuntime;
using Godot;
using Renown;
using Renown.World;

namespace PlayerSystem {
	public partial class HeadsUpDisplay : CanvasLayer {
		[Export]
		private Player _Owner;

		private static readonly Color DefaultColor = new Color( 1.0f, 1.0f, 1.0f, 1.0f );
		private static readonly Color HiddenColor = new Color( 0.0f, 0.0f, 0.0f, 0.0f );
		private static readonly Color ArmUsedColor = new Color( 0.0f, 0.0f, 1.0f, 1.0f );
		private static readonly Color ArmUnusedColor = new Color( 0.20f, 0.20f, 0.20f, 1.0f );

		private Notebook NoteBook;

		private Tween FadeOutTween;
		private Tween FadeInTween;

		private HealthBar HealthBar;
		private RageBar RageBar;

		private Control EmoteMenu;

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
		private TextureRect ParryOverlay;

		private Timer SaveTimer;
		private Control SaveSpinner;

		private Timer WeaponStatusTimer;
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

		private Button OpenDoorButton;
		private Button UseDoorButton;

		private Control BossHealthBar;

		private Label WorldTimeYear;
		private Label WorldTimeMonth;
		private Label WorldTimeDay;
		private Label WorldTimeHour;
		private Label WorldTimeMinute;

		private Label LocationLabel;
		private Timer LocationStatusTimer;

		private Label ObjectiveLabel;
		private Timer ObjectiveStatusTimer;

		private static System.Action<int> DialogueCallback;

		private Dictionary<string, StatusIcon> StatusIcons;

		private Control HellbreakerOverlay;

		[Signal]
		public delegate void EmoteTriggeredEventHandler( Texture2D emote );

		public HealthBar GetHealthBar() => HealthBar;
		public RageBar GetRageBar() => RageBar;

		public TextureRect GetReflexOverlay() => ReflexOverlay;
		public TextureRect GetDashOverlay() => DashOverlay;
		public TextureRect GetParryOverlay() => ParryOverlay;

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
		public static void StartDialogue( Resource dialogueResource, string key, System.Action<int> callback ) {
			DialogueManager.ShowDialogueBalloon( dialogueResource, key );
			DialogueCallback = callback;
		}

		private void OnDialogueEnded( Resource dialogueResource ) {
		}
		private void OnDialogueStarted( Resource dialogueResource ) {
		}
		private void OnMutated( Godot.Collections.Dictionary mutation ) {
			DialogueCallback?.DynamicInvoke( DialogueGlobals.Get().PlayerChoice );
		}

		private void OnOpenDoorButtonPressed() {
			if ( InteractionData is Door door && door != null ) {
				bool open = door.UseDoor( _Owner, out string message );
				StartThoughtBubble( "Message: " + message );
				if ( open ) {
					OpenDoorButton.Hide();
					UseDoorButton.Show();
				} else {
					_Owner.PlaySound( null, ResourceCache.GetSound( "res://sounds/env/try_open_door.ogg" ) );
				}
			}
		}

		private void OnUseDoorTransitionFinished() {
			GetNode<CanvasLayer>( "/root/TransitionScreen" ).Disconnect( "transition_finished", Callable.From( OnUseDoorTransitionFinished ) );

			_Owner.GlobalPosition = ( InteractionData as Door ).GetDestination().GlobalPosition;
			_Owner.BlockInput( false );
		}
		private void OnUseDoorButtonPressed() {
			_Owner.BlockInput( true );
			GetNode<CanvasLayer>( "/root/TransitionScreen" ).Connect( "transition_finished", Callable.From( OnUseDoorTransitionFinished ) );
			GetNode<CanvasLayer>( "/root/TransitionScreen" ).Call( "transition" );
			_Owner.PlaySound( null, ResourceCache.GetSound( "res://sounds/env/open_door_" + new RandomNumberGenerator().RandiRange( 0, 2 ) + ".ogg" ) );

			DoorInteractor.Hide();
		}

		public Checkpoint GetCurrentCheckpoint() => CheckpointInteractor.GetCurrentCheckpoint();

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private void FadeUIElement( Control element, float duration ) {
			Tween tween = CreateTween();
			tween.TweenProperty( element, "modulate", HiddenColor, duration );
		}

		public override void _Ready() {
			base._Ready();

			StatusIcons = new Dictionary<string, StatusIcon>{
				{ "status_burning", GetNode<StatusIcon>( "MainHUD/StatusContainer/BurningStatusIcon" ) },
				{ "status_freezing", GetNode<StatusIcon>( "MainHUD/StatusContainer/FreezingStatusIcon" ) },
				{ "status_poisoned", GetNode<StatusIcon>( "MainHUD/StatusContainer/PoisonedStatusIcon" ) },
			};

			DialogueManager.DialogueStarted += OnDialogueStarted;
			DialogueManager.DialogueEnded += OnDialogueEnded;
			DialogueManager.Mutated += OnMutated;

			LevelData.Instance.PlayerRespawn += () => {
				foreach ( var icon in StatusIcons ) {
					icon.Value.SetProcess( false );
					icon.Value.Hide();
				}
			};

			if ( GameConfiguration.GameMode != GameMode.Multiplayer
				&& GetTree().CurrentScene.Name == "World" ) {
				WorldTimeManager.Instance.TimeTick += OnWorldTimeTick;
			}

			ArchiveSystem.Instance.Connect( "SaveGameBegin", Callable.From( SaveStart ) );
			ArchiveSystem.Instance.Connect( "SaveGameEnd", Callable.From( SaveEnd ) );

			NoteBook = GetNode<Notebook>( "NotebookContainer" );
			NoteBook.ProcessMode = ProcessModeEnum.Disabled;

			HealthBar = GetNode<HealthBar>( "MainHUD/HealthBar" );
			RageBar = GetNode<RageBar>( "MainHUD/RageBar" );

			AnnouncementContainer = GetNode<MarginContainer>( "MainHUD/AnnouncementLabel" );
			AnnouncementBackground = GetNode<TextureRect>( "MainHUD/AnnouncementLabel/TextureRect" );
			AnnouncementText = GetNode<Label>( "MainHUD/AnnouncementLabel/TextureRect/Label" );
			AnnouncementTimer = GetNode<Timer>( "MainHUD/AnnouncementLabel/Timer" );
			AnnouncementTimer.Connect( "timeout", Callable.From( () => {
				FadeOutTween = CreateTween();
				FadeOutTween.TweenProperty( AnnouncementBackground.Material, "shader_parameter/alpha", 0.0f, 2.5f );
				FadeOutTween.Connect( "finished", Callable.From( OnAnnouncementFadeOutTweenFinished ) );
			} ) );

			EmoteMenu = GetNode<Control>( "MainHUD/EmoteMenu" );
			EmoteMenu.Connect( "selection_canceled", Callable.From( () => {
				EmoteMenu.SetProcessInput( false );
				EmoteMenu.Hide();
				EmoteMenu.ProcessMode = ProcessModeEnum.Disabled;
			} ) );
			EmoteMenu.Connect( "slot_selected", Callable.From<Control, int>( ( slot, index ) => {
				EmoteMenu.SetProcessInput( false );
				EmoteMenu.Hide();
				EmoteMenu.ProcessMode = ProcessModeEnum.Disabled;

				if ( index != -2 ) {
					EmitSignalEmoteTriggered( ( slot as TextureRect ).Texture );
				}
			} ) );

			CheckpointInteractor = GetNode<CheckpointInteractor>( "CheckpointContainer" );

			ReflexOverlay = GetNode<TextureRect>( "MainHUD/Overlays/ReflexModeOverlay" );
			DashOverlay = GetNode<TextureRect>( "MainHUD/Overlays/DashOverlay" );
			ParryOverlay = GetNode<TextureRect>( "MainHUD/Overlays/ParryOverlay" );

			if ( GameConfiguration.GameMode == GameMode.ChallengeMode ) {
				HellbreakerOverlay = GetNode<Control>( "MainHUD/HellbreakerOverlay" );
				LevelData.Instance.HellbreakerBegin += HellbreakerOverlay.Show;
				LevelData.Instance.HellbreakerFinished += HellbreakerOverlay.Hide;
			}

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

			OpenDoorButton = GetNode<Button>( "DoorContainer/MarginContainer/OpenButton" );
			OpenDoorButton.Connect( "pressed", Callable.From( OnOpenDoorButtonPressed ) );

			UseDoorButton = GetNode<Button>( "DoorContainer/MarginContainer/UseButton" );
			UseDoorButton.Connect( "pressed", Callable.From( OnUseDoorButtonPressed ) );

			BossHealthBar = GetNode<Control>( "MainHUD/BossHealthBar" );

			WeaponStatusTimer = new Timer();
			WeaponStatusTimer.Name = "WeaponStatusTimer";
			WeaponStatusTimer.WaitTime = 10.5f;
			WeaponStatusTimer.OneShot = true;
			WeaponStatusTimer.Connect( "timeout", Callable.From( () => FadeUIElement( WeaponStatus, 1.0f ) ) );
			AddChild( WeaponStatusTimer );

			LocationLabel = GetNode<Label>( "MainHUD/LocationLabel" );
			LocationLabel.Theme = AccessibilityManager.DefaultTheme;

			LocationStatusTimer = new Timer();
			LocationStatusTimer.Name = "LocationStatusTimer";
			LocationStatusTimer.WaitTime = 5.90f;
			LocationStatusTimer.OneShot = true;
			LocationStatusTimer.Connect( "timeout", Callable.From( () => FadeUIElement( LocationLabel, 1.5f ) ) );
			AddChild( LocationStatusTimer );

			ObjectiveLabel = GetNode<Label>( "MainHUD/ObjectiveLabel" );
			ObjectiveLabel.Theme = AccessibilityManager.DefaultTheme;

			ObjectiveStatusTimer = new Timer();
			ObjectiveStatusTimer.Name = "ObjectiveStatusTimer";
			ObjectiveStatusTimer.WaitTime = 5.90f;
			ObjectiveStatusTimer.OneShot = true;
			ObjectiveStatusTimer.Connect( "timeout", Callable.From( () => FadeUIElement( ObjectiveLabel, 1.5f ) ) );
			AddChild( ObjectiveStatusTimer );

			_Owner.LocationChanged += ( WorldArea location ) => {
				LocationLabel.Text = location.GetAreaName();

				Tween Tweener = CreateTween();
				Tweener.TweenProperty( LocationLabel, "modulate", DefaultColor, 1.5f );
				Tweener.Connect( "finished", Callable.From( () => { LocationStatusTimer.Start(); } ) );
			};
			_Owner.Damaged += ( Entity source, Entity target, float nAmount ) => { HealthBar.SetHealth( target.GetHealth() ); };
			_Owner.SwitchedWeapon += SetWeapon;
			_Owner.UsedWeapon += OnWeaponUsed;
			_Owner.WeaponStatusUpdated += ( WeaponEntity source, WeaponEntity.Properties useMode ) => {
				if ( source != WeaponData ) {
					return;
				}

				WeaponStatus.Modulate = DefaultColor;
				WeaponStatusTimer.Start();

				WeaponModeBladed.Visible = ( source.LastUsedMode & WeaponEntity.Properties.IsBladed ) != 0;
				WeaponModeBlunt.Visible = ( source.LastUsedMode & WeaponEntity.Properties.IsBlunt ) != 0;

				if ( ( source.LastUsedMode & WeaponEntity.Properties.IsFirearm ) != 0 ) {
					WeaponModeFirearm.Show();
					WeaponStatusBulletCount.Text = source.BulletsLeft.ToString();
					if ( source.Reserve != null ) {
						WeaponStatusBulletReserve.Text = source.Reserve.Amount.ToString();
					}
				} else {
					WeaponModeFirearm.Hide();
				}
			};
			_Owner.HandsStatusUpdated += ( Player.Hands handsUsed ) => {
				WeaponStatus.Modulate = DefaultColor;
				WeaponStatusTimer.Start();

				switch ( handsUsed ) {
				case Player.Hands.Left:
					LeftArmIndicator.Modulate = ArmUsedColor;
					RightArmIndicator.Modulate = ArmUnusedColor;
					break;
				case Player.Hands.Right:
					LeftArmIndicator.Modulate = ArmUnusedColor;
					RightArmIndicator.Modulate = ArmUsedColor;
					break;
				case Player.Hands.Both:
					LeftArmIndicator.Modulate = ArmUsedColor;
					RightArmIndicator.Modulate = ArmUsedColor;
					break;
				};
			};

			HealthBar.Init( 100.0f );
			RageBar.Init( 60.0f );
		}
		public void AddStatusEffect( string effectName, StatusEffect effect ) {
			if ( StatusIcons.TryGetValue( effectName, out StatusIcon icon ) ) {
				icon.Show();
				icon.Start( effect );
				icon.Material.Set( "shader_parameter/progress", 1.0f );
			}
		}

		private void OnWeaponReloaded( WeaponEntity source ) {
			WeaponStatus.Modulate = DefaultColor;
			WeaponStatusTimer.Start();

			WeaponStatusBulletCount.Text = source.BulletsLeft.ToString();
			WeaponStatusBulletReserve.Text = source.Reserve != null ? source.Reserve.Amount.ToString() : "0";
		}
		private void OnWeaponUsed( WeaponEntity source ) {
			WeaponStatus.Modulate = DefaultColor;
			WeaponStatusTimer.Start();

			if ( ( source.LastUsedMode & WeaponEntity.Properties.IsFirearm ) != 0 ) {
				WeaponStatusBulletCount.Text = source.BulletsLeft.ToString();
			}
		}
		private void SetWeapon( WeaponEntity weapon ) {
			if ( WeaponData != weapon && WeaponData != null ) {
				WeaponData.Reloaded -= OnWeaponReloaded;
				WeaponData.Used -= OnWeaponUsed;
			}

			if ( weapon == null ) {
				WeaponStatus.Modulate = HiddenColor;
				WeaponStatus.ProcessMode = ProcessModeEnum.Disabled;
				return;
			} else {
				WeaponStatus.ProcessMode = ProcessModeEnum.Pausable;
			}

			WeaponStatus.Modulate = DefaultColor;
			WeaponStatusTimer.Start();

			if ( ( weapon.LastUsedMode & WeaponEntity.Properties.IsFirearm ) != 0 ) {
				WeaponStatusFirearm.Show();
				WeaponStatusMelee.Hide();

				WeaponStatusFirearmIcon.Texture = weapon.Icon;

				WeaponStatusBulletCount.Text = weapon.BulletsLeft.ToString();
				WeaponStatusBulletReserve.Text = weapon.Reserve != null ? weapon.Reserve.Amount.ToString() : "0";
			} else {
				WeaponStatusFirearm.Hide();
				WeaponStatusMelee.Show();
				WeaponStatusMeleeIcon.Texture = weapon.Icon;
			}

			WeaponData = weapon;
			WeaponData.Reloaded += OnWeaponReloaded;
			WeaponData.Used += OnWeaponUsed;
		}

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
			}
			;

			if ( CurrentInteractor != null ) {
				CurrentInteractor.Show();
			} else {
				return;
			}

			Input.SetCustomMouseCursor( ResourceCache.GetTexture( "res://cursor_n.png" ), Input.CursorShape.Arrow );

			InteractionData = item;

			if ( CurrentInteractor == JumpInteractor ) {
				EaglesPeak data = (EaglesPeak)item;

				OnYesPressed = Callable.From( data.OnYesButtonPressed );
				OnNoPressed = Callable.From( data.OnNoButtonPressed );

				JumpYesButton.Connect( "pressed", OnYesPressed );
				JumpNoButton.Connect( "pressed", OnNoPressed );

				JumpViewImage.Texture = data.GetViewImage();
				JumpMusic.Stream = data.GetMusic();

				JumpMusic.Play();
			} else if ( CurrentInteractor == DoorInteractor ) {
				switch ( ( item as Door ).GetState() ) {
				case DoorState.Locked:
					OpenDoorButton.Show();
					UseDoorButton.Hide();
					break;
				case DoorState.Unlocked:
					OpenDoorButton.Hide();
					UseDoorButton.Show();
					break;
				}
				;
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

			Input.SetCustomMouseCursor( ResourceCache.GetTexture( "res://textures/hud/crosshairs/crosshairi.tga" ), Input.CursorShape.Arrow );
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

		public override void _UnhandledInput( InputEvent @event ) {
			base._UnhandledInput( @event );

			if ( Input.IsActionJustPressed( "open_emote_menu" ) ) {
				EmoteMenu.Show();
				EmoteMenu.SetProcessInput( true );
				EmoteMenu.ProcessMode = ProcessModeEnum.Always;
			}
			if ( Input.IsActionJustReleased( "open_emote_menu" ) ) {
				EmoteMenu.SetProcessInput( false );
				EmoteMenu.Hide();
				EmoteMenu.ProcessMode = ProcessModeEnum.Disabled;
			}
		}
    };
};
*/