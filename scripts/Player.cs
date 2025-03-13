using Godot;
using System;
using PlayerSystem;
using System.Linq;
using System.Collections.Generic;
using Renown;

public partial class Player : CharacterBody2D {
	public enum GameMode {
		Singleplayer,
		Coop2,
		Coop3,
		Coop4,
		Multiplayer
	};

	public enum Hands : byte {
		Left,
		Right,
		Both
	};

	public enum PlayerFlags : uint {
		Sliding			= 0x0001,
		Crouching		= 0x0002,
		BulletTime		= 0x0004,
		Dashing			= 0x0008,
		DemonRage		= 0x0010,
		UsedMana		= 0x0020,
		DemonSight		= 0x0040,
		OnHorse			= 0x0080,
		IdleAnimation	= 0x1000,
		Checkpoint		= 0x2000,
	};

	public enum AnimationState : byte {
		Idle,
		Move,
		Melee
	};

	public class WarpPoint {
		private Checkpoint Location;
		private Biome Biome;
		private Texture2D Icon;

		public WarpPoint( Checkpoint checkpoint ) {
			Location = checkpoint;
		}

		public Texture2D GetIcon() {
			return Icon;
		}
		public Checkpoint GetLocation() {
			return Location;
		}
		public Biome GetBiome() {
			return Biome;
		}
	};

	private List<WeaponEntity.Properties> WeaponModeList = new List<WeaponEntity.Properties>{
		WeaponEntity.Properties.IsOneHanded | WeaponEntity.Properties.IsBladed,
		WeaponEntity.Properties.IsOneHanded | WeaponEntity.Properties.IsBlunt,
		WeaponEntity.Properties.IsOneHanded | WeaponEntity.Properties.IsFirearm,

		WeaponEntity.Properties.IsTwoHanded | WeaponEntity.Properties.IsBladed,
		WeaponEntity.Properties.IsTwoHanded | WeaponEntity.Properties.IsBlunt,
		WeaponEntity.Properties.IsTwoHanded | WeaponEntity.Properties.IsFirearm
	};

	private const int MAX_WEAPON_SLOTS = 4;
	public static System.IO.BinaryReader SaveReader = null;
	public static System.IO.BinaryWriter SaveWriter = null;

	private const float ACCEL = 900.0f;
	private const float FRICTION = 1400.0f;
	private const float MAX_SPEED = 240.0f;
	private const float JUMP_VELOCITY = -400.0f;

	private Random RandomFactory = new Random( System.DateTime.Now.Year + System.DateTime.Now.Month + System.DateTime.Now.Day );

	private static Godot.Vector2I ScreenSize = Godot.Vector2I.Zero;

	private Resource CurrentMappingContext;
	private Resource KeyboardInputMappings;
	private Resource GamepadInputMappings;

	private Resource SwitchToKeyboard;
	private Resource SwitchToGamepad;

	private Resource MoveAction;
	private Resource DashAction;
	private Resource SlideAction;
	private Resource MeleeAction;
	private Resource UseWeaponAction;
	private Resource SwitchWeaponModeAction;
	private Resource NextWeaponAction;
	private Resource PrevWeaponAction;
	private Resource OpenInventoryAction;
	private Resource BulletTimeAction;
	public readonly Resource InteractionAction;

	private Resource MoveActionGamepad;
	private Resource DashActionGamepad;
	private Resource SlideActionGamepad;
	private Resource MeleeActionGamepad;
	private Resource UseWeaponActionGamepad;
	private Resource SwitchWeaponModeActionGamepad;
	private Resource NextWeaponActionGamepad;
	private Resource PrevWeaponActionGamepad;
	private Resource OpenInventoryActionGamepad;
	private Resource BulletTimeActionGamepad;
	private Resource ArmAngleAction;
	public readonly Resource InteractionActionGamepad;

	private Resource MoveActionKeyboard;
	private Resource DashActionKeyboard;
	private Resource SlideActionKeyboard;
	private Resource MeleeActionKeyboard;
	private Resource UseWeaponActionKeyboard;
	private Resource SwitchWeaponModeActionKeyboard;
	private Resource NextWeaponActionKeyboard;
	private Resource PrevWeaponActionKeyboard;
	private Resource OpenInventoryActionKeyboard;
	private Resource BulletTimeActionKeyboard;
	public readonly Resource InteractionActionKeyboard;

	private GpuParticles2D WalkEffect;
	private GpuParticles2D SlideEffect;
	private GpuParticles2D DashEffect;
	private AnimatedSprite2D JumpkitSparks;

	private PackedScene BloodSplatter;

	private Node2D Animations;
	private AnimatedSprite2D TorsoAnimation;
	private AnimatedSprite2D LegAnimation;
	private AnimatedSprite2D IdleAnimation;

	private Timer IdleTimer;

	private Timer DashTime;
	private Timer SlideTime;
	private Timer DashCooldownTime;

	[Export]
	private Node Inventory;
	[Export]
	private Arm ArmLeft;
	[Export]
	private Arm ArmRight;
	[Export]
	private HeadsUpDisplay HUD;

	private static bool TutorialCompleted = false;

	// renown data
	private uint RenownAmount = 0;
	private uint WarCrimeCount = 0;
	private List<Renown.Trait> Traits = new List<Renown.Trait>();

	private List<WeaponSlot> WeaponSlots;

	private float Health = 100.0f;
	private float Rage = 60.0f;
	private PlayerFlags Flags = 0;
	private int CurrentWeapon = 0;

	// multiplayer data
	public ulong MultiplayerId = 0; // literally just a steamID
	public string MultiplayerUsername;
	public uint MultiplayerKills = 0;
	public uint MultiplayerDeaths = 0;
	public uint MultiplayerFlagCaptures = 0;
	public uint MultiplayerFlagReturns = 0;
	public float MultiplayerHilltime = 0.0f;

	private AudioStreamPlayer2D MoveChannel;
	private AudioStreamPlayer2D DashChannel;
	private AudioStreamPlayer2D SlideChannel;
	private AudioStreamPlayer2D PainChannel;
	private AudioStreamPlayer2D SlowMoChannel;
	private AudioStreamPlayer2D ActionChannel;

	private AudioStream ChangeWeaponSfx;
	private List<AudioStream> PainSfx;
	private List<AudioStream> DieSfx;
	private List<AudioStream> DeathSfx;
	private List<AudioStream> MoveGravelSfx;
	private List<AudioStream> DashSfx;
	private List<AudioStream> SlideSfx;
	private AudioStream DashExplosion;
	private AudioStream SlowMoBeginSfx;
	private AudioStream SlowMoEndSfx;

	private List<WeaponEntity> WeaponsStack = new List<WeaponEntity>();
	private List<ConsumableStack> ConsumableStacks = new List<ConsumableStack>();
	private List<AmmoStack> AmmoStacks = new List<AmmoStack>();

	private List<Checkpoint> WarpList = new List<Checkpoint>();

	private Hands HandsUsed = Hands.Right;
	private Arm LastUsedArm;
	private float ArmAngle = 0.0f;
	private float DashBurnout = 0.0f;
	private int InputDevice = 0;
	private float FrameDamage = 0.0f;
	private int Hellbreaks = 0;
	private bool SplitScreen = false;
	private float SoundLevel = 0.0f;
	private Godot.Vector2 DashDirection = Godot.Vector2.Zero;
	private Godot.Vector2 InputVelocity = Godot.Vector2.Zero;
	private Godot.Vector2 LastMousePosition = Godot.Vector2.Zero;

	[Signal]
	public delegate void DieEventHandler( CharacterBody2D attacker, CharacterBody2D target );
	[Signal]
	public delegate void DamagedEventHandler( CharacterBody2D attacker, CharacterBody2D target, float nAmount );

	public static bool IsTutorialActive() {
		return !TutorialCompleted;
	}

	public void Save() {
		if ( (uint)GetNode( "/root/GameConfiguration" ).Get( "_game_mode" ) == (uint)GameMode.Multiplayer && MultiplayerId != SteamManager.GetSteamID() ) {
			return; // don't save other people's data, we ain't the government
		}

		DirAccess.MakeDirRecursiveAbsolute( ArchiveSystem.Instance.GetSaveDirectory() );

		string path = ProjectSettings.GlobalizePath( ArchiveSystem.Instance.GetSaveDirectory() + "PlayerData.ngd" );
		System.IO.FileStream stream;
		try {
			stream = new System.IO.FileStream( path, System.IO.FileMode.Create );
		} catch ( System.IO.FileNotFoundException exception ) {
			GD.PushError( exception.Source + ":" + exception.Message + "\n" + exception.StackTrace );
			return;
		}
		System.IO.BinaryWriter writer = new System.IO.BinaryWriter( stream );

		GD.Print( "saving player data..." );

		// multiplayer data
		writer.Write( MultiplayerKills );
		writer.Write( MultiplayerDeaths );
		writer.Write( MultiplayerFlagCaptures );
		writer.Write( MultiplayerFlagReturns );
		writer.Write( (double)MultiplayerHilltime );

		writer.Write( (double)GlobalPosition.X );
		writer.Write( (double)GlobalPosition.Y );

		writer.Write( (double)Health );
		writer.Write( (double)Rage );
		writer.Write( (uint)Flags );
		writer.Write( Hellbreaks );
		writer.Write( CurrentWeapon );
		writer.Write( (uint)HandsUsed );
		writer.Write( WarCrimeCount );
		writer.Write( RenownAmount );
		writer.Write( TutorialCompleted );

		writer.Write( ArmLeft.GetSlot() );
		writer.Write( ArmRight.GetSlot() );

		/*
		writer.Write( Traits.Count );
		for ( int i = 0; i < Traits.Count; i++ ) {
			writer.Write( (uint)Traits[i].GetTraitType() );
		}
		*/

		writer.Write( AmmoStacks.Count );
		for ( int i = 0; i < AmmoStacks.Count; i++ ) {
			writer.Write( AmmoStacks[i].Amount );
			writer.Write( (string)AmmoStacks[i].AmmoType.Get( "id" ) );
		}

		writer.Write( WeaponsStack.Count );
		for ( int i = 0; i < WeaponsStack.Count; i++ ) {
			writer.Write( (string)WeaponsStack[i].Data.Get( "id" ) );
			if ( ( WeaponsStack[i].GetProperties() & WeaponEntity.Properties.IsFirearm ) != 0 ) {
				writer.Write( WeaponsStack[i].GetBulletCount() );
			}
		}

		writer.Write( MAX_WEAPON_SLOTS );
		for ( int i = 0; i < WeaponSlots.Count; i++ ) {
			writer.Write( WeaponSlots[i].IsUsed() );
			if ( WeaponSlots[i].IsUsed() ) {
				int weaponIndex;
				for ( weaponIndex = 0; weaponIndex < WeaponsStack.Count; weaponIndex++ ) {
					if ( WeaponsStack[ weaponIndex ] == WeaponSlots[ i ].GetWeapon() ) {
						break;
					}
				}
				writer.Write( weaponIndex );
				writer.Write( (uint)WeaponSlots[i].GetMode() );
			}
		}
		
		writer.Write( ConsumableStacks.Count );
		for ( int i = 0; i < ConsumableStacks.Count; i++ ) {
			writer.Write( ConsumableStacks[i].Amount );
			writer.Write( (string)ConsumableStacks[i].ItemType.Get( "id" ) );
		}

//		SaveWriter = writer;

		Godot.Collections.Array<Node> nodes = GetTree().GetNodesInGroup( "PlayerArchive" );
		for ( int i = 0; i < nodes.Count; i++ ) {
//			nodes[i].Call( "Save" );
		}
	}
	public void Load() {
		if ( (uint)GetNode( "/root/GameConfiguration" ).Get( "_game_mode" ) == (uint)GameMode.Multiplayer && MultiplayerId != SteamManager.GetSteamID() ) {
			return; // don't load other people's data, we ain't the government
		}

		string path = ProjectSettings.GlobalizePath( ArchiveSystem.Instance.GetSaveDirectory() + "PlayerData.ngd" );
		System.IO.FileStream stream;
		try {
			stream = new System.IO.FileStream( path, System.IO.FileMode.Open );
		} catch ( System.IO.FileNotFoundException exception ) {
			GD.PushError( exception.Source + ":" + exception.Message + "\n" + exception.StackTrace );
			return;
		}
		System.IO.BinaryReader reader = new System.IO.BinaryReader( stream );

		GD.Print( "Loading player data..." );
		GetNode( "/root/Console" ).Call( "print_line", "Loading player data..." );

		MultiplayerKills = reader.ReadUInt32();
		MultiplayerDeaths = reader.ReadUInt32();
		MultiplayerFlagCaptures = reader.ReadUInt32();
		MultiplayerFlagReturns = reader.ReadUInt32();
		MultiplayerHilltime = (float)reader.ReadDouble();

		Godot.Vector2 position = Godot.Vector2.Zero;
		position.X = (float)reader.ReadDouble();
		position.Y = (float)reader.ReadDouble();
		GlobalPosition = position;

		Health = (float)reader.ReadDouble();
		HUD.GetHealthBar().SetHealth( Health );

		Rage = (float)reader.ReadDouble();
		HUD.GetRageBar().Value = Rage;

		Flags = (PlayerFlags)reader.ReadUInt32();
		Hellbreaks = reader.ReadInt32();
		CurrentWeapon = reader.ReadInt32();
		HandsUsed = (Hands)reader.ReadUInt32();
		WarCrimeCount = reader.ReadUInt32();
		RenownAmount = reader.ReadUInt32();
		TutorialCompleted = reader.ReadBoolean();
		
		/*
		Traits.Clear();
		int numTraits = reader.ReadInt32();
		for ( int i = 0; i < numTraits; i++ ) {
			uint traitType = reader.ReadUInt32();
		}
		*/
		
		int leftSlot = reader.ReadInt32();
		if ( leftSlot == WeaponSlot.INVALID ) {
			ArmLeft.SetWeapon( WeaponSlot.INVALID );
		} else {
			ArmLeft.SetWeapon( leftSlot );
		}
		
		int rightSlot = reader.ReadInt32();
		if ( rightSlot == WeaponSlot.INVALID ) {
			ArmRight.SetWeapon( WeaponSlot.INVALID );
		} else {
			ArmRight.SetWeapon( rightSlot );
		}

		AmmoStacks.Clear();
		int numAmmoStacks = reader.ReadInt32();
		for ( int i = 0; i < numAmmoStacks; i++ ) {
			AmmoStack stack = new AmmoStack();
			stack.Amount = reader.ReadInt32();
			stack.AmmoType = (Resource)Inventory.Call( "get_item_from_id", reader.ReadString() );
			AmmoStacks.Add( stack );
		}

		WeaponsStack.Clear();
		int numWeapons = reader.ReadInt32();
		for ( int i = 0; i < numWeapons; i++ ) {
			WeaponEntity weapon = new WeaponEntity();
			weapon.Data = (Resource)Inventory.Call( "get_item_from_id", reader.ReadString() );
			weapon.SetOwner( this );
			WeaponsStack.Add( weapon );
			AddChild( weapon );

			if ( ( weapon.GetProperties() & WeaponEntity.Properties.IsFirearm ) != 0 ) {
				int nBulletCount = reader.ReadInt32();
				if ( nBulletCount == 0 ) {
					weapon.SetWeaponState( WeaponEntity.WeaponState.Reload );
				} else {
					weapon.SetWeaponState( WeaponEntity.WeaponState.Idle );
				}
				weapon.SetBulletCount( nBulletCount );
			} else {
				weapon.SetWeaponState( WeaponEntity.WeaponState.Idle );
			}

			AmmoStack stack = null;
			for ( int a = 0; a < numAmmoStacks; a++ ) {
				if ( (int)( (Godot.Collections.Dictionary)AmmoStacks[i].AmmoType.Get( "properties" ) )[ "type" ] ==
					(int)weapon.GetAmmoType() )
				{
					stack = AmmoStacks[i];
					break;
				}
			}
			if ( stack != null ) {
				weapon.SetReserve( stack );
				weapon.SetAmmo( stack.AmmoType );
			}
		}

		int maxSlots = reader.ReadInt32();
		for ( int i = 0; i < maxSlots; i++ ) {
			if ( reader.ReadBoolean() ) {
				WeaponEntity weapon = WeaponsStack[ reader.ReadInt32() ];
				weapon.SetEquippedState( true );
				WeaponSlots[i].SetWeapon( weapon );
				WeaponSlots[i].SetMode( (WeaponEntity.Properties)reader.ReadUInt32() );
				GD.Print( "Loaded weapon slot " + i.ToString() );
			}
		}

		ConsumableStacks.Clear();
		int numConsumableStacks = reader.ReadInt32();
		for ( int i = 0; i < numConsumableStacks; i++ ) {
			ConsumableStack stack = new ConsumableStack();
			stack.Amount = reader.ReadInt32();
			stack.ItemType = (Resource)Inventory.Call( "get_item_from_id", reader.ReadString() );
			ConsumableStacks.Add( stack );
		}

		HUD.SetWeapon( WeaponSlots[ CurrentWeapon ].GetWeapon() );
	}

	public Resource GetCurrentMappingContext() {
		return CurrentMappingContext;
	}
	public List<WeaponSlot> GetWeaponSlots() {
		return WeaponSlots;
	}
	public List<WeaponEntity> GetWeaponStack() {
		return WeaponsStack;
	}
	public List<AmmoStack> GetAmmoStacks() {
		return AmmoStacks;
	}
	public Node GetInventory() {
		return Inventory;
	}
	public Arm GetWeaponHand( WeaponEntity weapon ) {
		if ( ArmLeft.GetSlot() != WeaponSlot.INVALID && WeaponSlots[ ArmLeft.GetSlot() ].GetWeapon() == weapon ) {
			return ArmLeft;
		} else if ( ArmRight.GetSlot() != WeaponSlot.INVALID && WeaponSlots[ ArmRight.GetSlot() ].GetWeapon() == weapon ) {
			return ArmRight;
		}
		return null;
	}
	public Godot.Vector2 GetInputVelocity() {
		return InputVelocity;
	}
	public WeaponSlot GetSlot( int nSlot ) {
		return WeaponSlots[ nSlot ];
	}
	public float GetArmAngle() {
		return ArmAngle;
	}
	public void SetArmAngle( float nAngle ) {
		ArmAngle = nAngle;
	}
	public Arm GetLastUsedArm() {
		return LastUsedArm;
	}
	public void SetLastUsedArm( Arm arm ) {
		LastUsedArm = arm;
	}
	public float GetSoundLevel() {
		return SoundLevel;
	}
	public float GetHealth() {
		return Health;
	}
	public void SetHealth( float nHealth ) {
		Health = nHealth;
		HUD.GetHealthBar().SetHealth( Health );
	}
	public float GetRage() {
		return Rage;
	}
	public void SetRage( float nRage ) {
		Rage = nRage;
		HUD.GetRageBar().Value = Rage;
	}
	public PlayerFlags GetFlags() {
		return Flags;
	}
	public void SetFlags( PlayerFlags flags ) {
		Flags = flags;
	}
	public Hands GetHandsUsed() {
		return HandsUsed;
	}
	public void SetHandsUsed( Hands hands ) {
		HandsUsed = hands;
	}
	public List<WeaponSlot> GetSlots() {
		return WeaponSlots;
	}
	public void SetSlots( List<WeaponSlot> slots ) {
		WeaponSlots = slots;
	}
	public List<Checkpoint> GetWarpPoints() {
		return WarpList;
	}

	public Arm GetLeftArm() {
		return ArmLeft;
	}
	public Arm GetRightArm() {
		return ArmRight;
	}

	private void IdleReset() {
		IdleTimer.CallDeferred( "start" );
		IdleAnimation.CallDeferred( "hide" );
		IdleAnimation.CallDeferred( "stop" );

		LegAnimation.CallDeferred( "show" );
		TorsoAnimation.CallDeferred( "show" );
		ArmRight.Animations.CallDeferred( "show" );
		ArmLeft.Animations.CallDeferred( "show" );
	}

	public void SetupSplitScreen( int nInputIndex ) {
		GetNode( "/root/Console" ).Call( "print_line", "Setting up split-screen input for " + nInputIndex );

		if ( Input.GetConnectedJoypads().Count > 0 ) {
			SplitScreen = true;
		}
		InputDevice = nInputIndex;
	}

	public override void _ExitTree() {
		base._ExitTree();

		for ( int i = 0; i < AmmoStacks.Count; i++ ) {
			AmmoStacks[i].QueueFree();
		}
		AmmoStacks.Clear();

		for ( int i = 0; i < ConsumableStacks.Count; i++ ) {
			ConsumableStacks[i].QueueFree();
		}
		ConsumableStacks.Clear();

		for ( int i = 0; i < WeaponsStack.Count; i++ ) {
			RemoveChild( WeaponsStack[i] );
			WeaponsStack[i].QueueFree();
		}
		WeaponsStack.Clear();

		MoveChannel.QueueFree();
		DashChannel.QueueFree();
		SlideChannel.QueueFree();
		SlowMoChannel.QueueFree();
		ActionChannel.QueueFree();
		PainChannel.QueueFree();

		PainSfx.Clear();
		DieSfx.Clear();
		DeathSfx.Clear();
		MoveGravelSfx.Clear();
		DashSfx.Clear();
		SlideSfx.Clear();

		DashTime.QueueFree();
		SlideTime.QueueFree();
		DashCooldownTime.QueueFree();

		WalkEffect.QueueFree();
		DashEffect.QueueFree();
		SlideEffect.QueueFree();
		JumpkitSparks.QueueFree();

		IdleTimer.QueueFree();
		IdleAnimation.QueueFree();

		TorsoAnimation.QueueFree();
		LegAnimation.QueueFree();
		ArmLeft.Animations.QueueFree();
		ArmRight.Animations.QueueFree();
		ArmLeft.QueueFree();
		ArmRight.QueueFree();

		IdleTimer.QueueFree();
		IdleAnimation.QueueFree();
		Inventory.QueueFree();
		HUD.QueueFree();

		WeaponSlots.Clear();

		QueueFree();
	}

	private void OnDeath( CharacterBody2D attacker ) {
		EmitSignal( "Die", attacker, this );
		LegAnimation.Hide();
		ArmLeft.Animations.Hide();
		ArmRight.Animations.Hide();

		TorsoAnimation.Play( "death" );

		PainChannel.Stream = DieSfx[ RandomFactory.Next( 0, DieSfx.Count - 1 ) ];
		PainChannel.Play();

		SetProcessUnhandledInput( true );
	}

	public void Damage( CharacterBody2D attacker, float nAmount ) {
		if ( ( Flags & PlayerFlags.Dashing ) != 0 ) {
			return; // iframes
		}

		EmitSignal( "Damaged", attacker, this, nAmount );

		Health -= nAmount;
		Rage += nAmount;

		Node2D blood = BloodSplatter.Instantiate<Node2D>();
		blood.GlobalPosition = GlobalPosition;
		AddChild( blood );

		if ( Health <= 0.0f ) {
			OnDeath( attacker );
		} else {
			PainChannel.Stream = PainSfx[ RandomFactory.Next( 0, PainSfx.Count - 1 ) ];
			PainChannel.Play();
		}
	}

	public void EndInteraction() {
		HUD.HideInteraction();
		Flags &= ~PlayerFlags.Checkpoint;
	}
	public void BeginInteraction( InteractionItem item ) {
		HUD.ShowInteraction( item );

		switch ( item.GetInteractionType() ) {
		case InteractionType.Checkpoint:
			Flags |= PlayerFlags.Checkpoint;
			break;
		};
	}

	private void OnIdleAnimationTimerTimeout() {
		if ( IdleAnimation.IsPlaying() ) {
			return;
		}
		TorsoAnimation.Hide();
		ArmLeft.Animations.Hide();
		ArmRight.Animations.Hide();
		LegAnimation.Hide();
		IdleAnimation.Show();
		IdleAnimation.Play( "start" );
		SteamAchievements.ActivateAchievement( "ACH_SMOKE_BREAK" );
	}
	private void OnIdleAnimationAnimationFinished()	{
		IdleAnimation.Play( "loop" );
	}
	private void OnLegsAnimationLooped() {
		if ( Velocity != Godot.Vector2.Zero ) {
			MoveChannel.Stream = MoveGravelSfx[ RandomFactory.Next( 0, MoveGravelSfx.Count - 1 ) ];
			MoveChannel.Play();
		}
	}
	private void OnDashTimeTimeout() {
		HUD.GetDashOverlay().Hide();
		( (PointLight2D)DashEffect.GetChild( 0 ) ).Hide();
		DashEffect.Emitting = false;
		Flags &= ~PlayerFlags.Dashing;
		if ( LegAnimation.FlipH ) {
			JumpkitSparks.FlipH = false;
			JumpkitSparks.Offset = new Godot.Vector2( 255.0f, 0.0f );
		} else {
			JumpkitSparks.FlipH = false;
			JumpkitSparks.Offset = Godot.Vector2.Zero;
		}
		DashCooldownTime.Start();
	}
	private void OnSlideTimeout() {
		SlideEffect.Emitting = false;
		Flags &= ~PlayerFlags.Sliding;
	}

	private void OnDash() {
		// TODO: upgradable dash burnout?
		if ( DashBurnout >= 1.0f ) {
			DashChannel.Stream = DashExplosion;
			DashChannel.Play();

			DashBurnout = 0.0f;
			return;
		}

		IdleReset();
		Flags |= PlayerFlags.Dashing;
		DashTime.Start();
		DashChannel.Stream = DashSfx[ RandomFactory.Next( 0, DashSfx.Count - 1 ) ];
		DashChannel.PitchScale = 1.0f + DashBurnout;
		DashChannel.Play();
		( (PointLight2D)DashEffect.GetChild( 0 ) ).Show();
		DashEffect.Emitting = true;
		DashDirection = Velocity;
		HUD.GetDashOverlay().Show();

		DashBurnout += 0.25f;
		DashCooldownTime.Start();
	}
	private void OnSlide() {
		if ( ( Flags & PlayerFlags.Sliding ) != 0 ) {
			return;
		}
		IdleReset();
		Flags |= PlayerFlags.Sliding;
		SlideTime.Start();
		SlideEffect.Emitting = true;
		LegAnimation.Play( "slide" );
	}
	private void OnUseWeapon() {
		IdleReset();

		int slot = LastUsedArm.GetSlot();
		if ( slot == WeaponSlot.INVALID ) {
			return; // nothing equipped
		}
		if ( WeaponSlots[ slot ].IsUsed() ) {
			FrameDamage += WeaponSlots[ slot ].GetWeapon().Use( WeaponSlots[ slot ].GetWeapon().GetLastUsedMode() );
		}
	}
	private void OnArmAngleChanged() {
		ArmAngle = (float)ArmAngleAction.Get( "value_axis_1d" );
	}
	private void OnPrevWeapon() {
		int index = CurrentWeapon <= 0 ? MAX_WEAPON_SLOTS - 1 : CurrentWeapon - 1;
		while ( index != -1 ) {
			if ( WeaponSlots[ index ].IsUsed() ) {
				break;
			}
			index--;
		}

		if ( index == MAX_WEAPON_SLOTS ) {
			index = -1;
		}

		// adjust arm state
		if ( index != WeaponSlot.INVALID ) {
			WeaponEntity weapon;
			Arm otherArm;

			if ( LastUsedArm == ArmLeft ) {
				otherArm = ArmRight;
				HandsUsed = Hands.Left;
			} else if ( LastUsedArm == ArmRight ) {
				otherArm = ArmLeft;
				HandsUsed = Hands.Right;
			} else {
				GD.PushError( "OnNextWeapon: invalid LastUsedArm" );
				LastUsedArm = ArmRight;
				return;
			}

			weapon = WeaponSlots[ index ].GetWeapon();
			if ( ( weapon.GetLastUsedMode() & WeaponEntity.Properties.IsTwoHanded ) != 0 ) {
				otherArm.SetWeapon( WeaponSlot.INVALID );
				HandsUsed = Hands.Both;
			}

			HUD.SetWeapon( weapon );
		} else {
			HUD.SetWeapon( null );
		}

		ActionChannel.Stream = ChangeWeaponSfx;
		ActionChannel.Play();

		CurrentWeapon = index;
		LastUsedArm.SetWeapon( CurrentWeapon );
	}
	private void OnNextWeapon() {
		int index = CurrentWeapon == MAX_WEAPON_SLOTS - 1 ? 0 : CurrentWeapon + 1;
		while ( index < MAX_WEAPON_SLOTS ) {
			if ( WeaponSlots[ index ].IsUsed() ) {
				break;
			}
			index++;
		}

		if ( index == MAX_WEAPON_SLOTS ) {
			index = -1;
		}

		// adjust arm state
		if ( index != WeaponSlot.INVALID ) {
			WeaponEntity weapon;
			Arm otherArm;

			if ( LastUsedArm == ArmLeft ) {
				otherArm = ArmRight;
				HandsUsed = Hands.Left;
			} else if ( LastUsedArm == ArmRight ) {
				otherArm = ArmLeft;
				HandsUsed = Hands.Right;
			} else {
				GD.PushError( "OnNextWeapon: invalid LastUsedArm" );
				LastUsedArm = ArmRight;
				return;
			}

			weapon = WeaponSlots[ index ].GetWeapon();
			if ( ( weapon.GetLastUsedMode() & WeaponEntity.Properties.IsTwoHanded ) != 0 ) {
				otherArm.SetWeapon( WeaponSlot.INVALID );
				HandsUsed = Hands.Both;
			}

			HUD.SetWeapon( weapon );
		} else {
			HUD.SetWeapon( null );
		}

		ActionChannel.Stream = ChangeWeaponSfx;
		ActionChannel.Play();

		CurrentWeapon = index;
		LastUsedArm.SetWeapon( CurrentWeapon );
	}
	private void OnBulletTime() {
		IdleReset();
		if ( ( Flags & PlayerFlags.BulletTime ) != 0 ) {
			ExitBulletTime();
		} else {
			SlowMoChannel.Stream = SlowMoBeginSfx;
			SlowMoChannel.Play();
			Flags |= PlayerFlags.BulletTime;
			Engine.TimeScale = 0.40f;
		}
	}
	private void OnToggleInventory() {
		HUD.OnShowInventory();
	}
	private void SwitchWeaponWielding() {
		Arm src;
		Arm dst;

		switch ( HandsUsed ) {
		case Hands.Left:
			src = ArmLeft;
			dst = ArmRight;
			break;
		case Hands.Right:
			src = ArmRight;
			dst = ArmLeft;
			break;
		case Hands.Both:
		default:
			src = LastUsedArm;
			dst = src;
			break;
		};

		if ( src.GetSlot() == WeaponSlot.INVALID ) {
			// nothing in the source hand, deny
			return;
		}

		LastUsedArm = dst;

		WeaponEntity srcWeapon = WeaponSlots[ src.GetSlot() ].GetWeapon();
		if ( ( srcWeapon.GetLastUsedMode() & WeaponEntity.Properties.IsTwoHanded ) != 0 && ( srcWeapon.GetLastUsedMode() & WeaponEntity.Properties.IsOneHanded ) == 0 ) {
			// cannot change hands, no one-handing allowed
			return;
		}
		
		// check if the destination hand has something in it, if true, then swap
		if ( dst.GetSlot() != WeaponSlot.INVALID ) {
			int tmp = dst.GetSlot();

			dst.SetWeapon( src.GetSlot() );
			src.SetWeapon( tmp );
		} else {
			// if we have nothing in the destination hand, then just clear the source hand
			int tmp = src.GetSlot();

			src.SetWeapon( WeaponSlot.INVALID );
			dst.SetWeapon( tmp );
		}
	}
	private void SwitchWeaponHand() {
		// TODO: implement use both hands
		
		switch ( HandsUsed ) {
		case Hands.Left:
			HandsUsed = Hands.Right; // set to right
			LastUsedArm = ArmRight;
			break;
		case Hands.Right:
			HandsUsed = Hands.Left; // set to right
			LastUsedArm = ArmLeft;
			break;
		// can't switch if we're using both hands for one weapon
		case Hands.Both:
		default:
			GD.PushWarning( "SwitchWeaponHand: invalid hand, setting to default of right" );
			HandsUsed = Hands.Right;
			break;
		};
		if ( LastUsedArm.GetSlot() != WeaponSlot.INVALID ) {
			EquipSlot( LastUsedArm.GetSlot() );
		}
	}
	private void SwitchWeaponMode() {
		// weapon mode order (default)
		// blade -> blunt -> firearm

		WeaponSlot slot;

		switch ( HandsUsed ) {
		case Hands.Left: {
			int index = ArmLeft.GetSlot();
			if ( index == WeaponSlot.INVALID ) {
				return;
			}
			slot = WeaponSlots[ index ];
			break; }
		case Hands.Right: {
			int index = ArmRight.GetSlot();
			if ( index == WeaponSlot.INVALID ) {
				return;
			}
			slot = WeaponSlots[ index ];
			break; }
		case Hands.Both:
		default:
			slot = WeaponSlots[ CurrentWeapon ];
			break;
		};

		WeaponEntity.Properties mode = slot.GetMode();
		WeaponEntity weapon = slot.GetWeapon();

		slot.SetMode( WeaponEntity.Properties.None ); // clear the modes

		// find the next most suitable mode
		WeaponEntity.Properties props = weapon.GetProperties();
		bool IsOneHanded = ( props & WeaponEntity.Properties.IsOneHanded ) != 0;
		bool IsTwoHanded = ( props & WeaponEntity.Properties.IsTwoHanded ) != 0;
		const WeaponEntity.Properties hands = ~(  WeaponEntity.Properties.IsOneHanded | WeaponEntity.Properties.IsTwoHanded );

		for ( int i = 0; i < WeaponModeList.Count; i++ ) {
			WeaponEntity.Properties current = WeaponModeList[i];
			if ( mode == current ) {
				// same mode, don't switch
				continue;
			}
			if ( ( props & ( current & hands ) ) != 0 ) {
				// apply some filters
				if ( ( current & WeaponEntity.Properties.IsOneHanded ) != 0 && !IsOneHanded ) {
					// one handing not possible, deny
					continue;
				}
				if ( ( current & WeaponEntity.Properties.IsTwoHanded ) != 0 && IsTwoHanded ) {
					// two handing not possible, deny
					continue;
				}
				
				// we've got a match!
				slot.SetMode( current );
				slot.GetWeapon().SetUseMode( current );
			}
		}
	}

	public void EquipSlot( int nSlot ) {
		CurrentWeapon = nSlot;

		WeaponEntity weapon = WeaponSlots[ nSlot ].GetWeapon();
		if ( weapon != null ) {
			// apply rules of various weapon properties
			if ( ( weapon.GetLastUsedMode() & WeaponEntity.Properties.IsTwoHanded ) != 0 )	{
				ArmLeft.SetWeapon( CurrentWeapon );
				ArmRight.SetWeapon( CurrentWeapon );

				// this will automatically override any other modes
				WeaponSlots[ ArmLeft.GetSlot() ].SetMode( weapon.GetDefaultMode() );
			}

			WeaponSlots[ LastUsedArm.GetSlot() ].SetMode( weapon.GetProperties() );
		}
		else {
			ArmLeft.SetWeapon( -1 );
			ArmRight.SetWeapon( -1 );
		}

		// update hand data
		LastUsedArm.SetWeapon( CurrentWeapon );

		HUD.SetWeapon( weapon );
	}

	private void SwitchInputMode( Resource InputContext ) {
		GetNode( "/root/GUIDE" ).Call( "enable_mapping_context", InputContext );

		GD.Print( "Remapping input..." );
		
		if ( SwitchWeaponModeAction != null ) {
			SwitchWeaponModeAction.Disconnect( "triggered", Callable.From( SwitchWeaponMode ) );
		}
		if ( BulletTimeAction != null ) {
			BulletTimeAction.Disconnect( "triggered", Callable.From( OnBulletTime ) );
		}
		if ( NextWeaponAction != null ) {
			NextWeaponAction.Disconnect( "triggered", Callable.From( OnNextWeapon ) );
		}
		if ( PrevWeaponAction != null ) {
			PrevWeaponAction.Disconnect( "triggered", Callable.From( OnPrevWeapon ) );
		}
		if ( DashAction != null ) {
			DashAction.Disconnect( "triggered", Callable.From( OnDash ) );
		}
		if ( SlideAction != null ) {
			SlideAction.Disconnect( "triggered", Callable.From( OnSlide ) );
		}
		if ( UseWeaponAction != null ) {
			UseWeaponAction.Disconnect( "triggered", Callable.From( OnUseWeapon ) );
		}
		if ( OpenInventoryAction != null ) {
			OpenInventoryAction.Disconnect( "triggered", Callable.From( OnToggleInventory ) );
		}

		if ( InputContext == KeyboardInputMappings ) {
			MoveAction = MoveActionKeyboard;
			DashAction = DashActionKeyboard;
			SlideAction = SlideActionKeyboard;
			BulletTimeAction = BulletTimeActionKeyboard;
			PrevWeaponAction = PrevWeaponActionKeyboard;
			NextWeaponAction = NextWeaponActionKeyboard;
			SwitchWeaponModeAction = SwitchWeaponModeActionKeyboard;
			OpenInventoryAction = OpenInventoryActionKeyboard;
			UseWeaponAction = UseWeaponActionKeyboard;

			CurrentMappingContext = KeyboardInputMappings;
			if ( ArmAngleAction.IsConnected( "triggered", Callable.From( OnArmAngleChanged ) ) ) {
				ArmAngleAction.Disconnect( "triggered", Callable.From( OnArmAngleChanged ) );
			}
		} else {
			MoveAction = MoveActionGamepad;
			DashAction = DashActionGamepad;
			SlideAction = SlideActionGamepad;
			BulletTimeAction = BulletTimeActionGamepad;
			PrevWeaponAction = PrevWeaponActionGamepad;
			NextWeaponAction = NextWeaponActionGamepad;
			SwitchWeaponModeAction = SwitchWeaponModeActionGamepad;
			OpenInventoryAction = OpenInventoryActionGamepad;
			UseWeaponAction = UseWeaponActionGamepad;

			CurrentMappingContext = GamepadInputMappings;
			ArmAngleAction.Connect( "triggered", Callable.From( OnArmAngleChanged ) );
		}

		SwitchWeaponModeAction.Connect( "triggered", Callable.From( SwitchWeaponMode ) );
		BulletTimeAction.Connect( "triggered", Callable.From( OnBulletTime ) );
		NextWeaponAction.Connect( "triggered", Callable.From( OnNextWeapon ) );
		PrevWeaponAction.Connect( "triggered", Callable.From( OnPrevWeapon ) );
		DashAction.Connect( "triggered", Callable.From( OnDash ) );
		SlideAction.Connect( "triggered", Callable.From( OnSlide ) );
		UseWeaponAction.Connect( "triggered", Callable.From( OnUseWeapon ) );
		OpenInventoryAction.Connect( "triggered", Callable.From( OnToggleInventory ) );
	}

	public override void _UnhandledInput( InputEvent @event ) {
		base._UnhandledInput( @event );
		
		if ( Health <= 0.0f ) {
			// dead
			SetHealth( 100.0f );
			SetRage( 0.0f );

			SetProcess( true );
			SetPhysicsProcess( true );
			SetProcessUnhandledInput( false );

			TorsoAnimation.Play( "idle" );
			LegAnimation.Show();
			ArmLeft.Animations.Show();
			ArmRight.Animations.Show();

			EmitSignal( "Respawn" );
		}
	}

	private void FlipSpriteLeft() {
		LegAnimation.FlipH = true;
		TorsoAnimation.FlipH = true;
		ArmLeft.Flip = true;
		ArmRight.Flip = true;
	}
	private void FlipSpriteRight() {
		LegAnimation.FlipH = false;
		TorsoAnimation.FlipH = false;
		ArmLeft.Flip = false;
		ArmRight.Flip = false;
	}

	private void OnSlowMoSfxFinished() {
		if ( SlowMoChannel.Stream == SlowMoBeginSfx ) {
			// only start lagging audio playback after the slowmo begin finishes
			AudioServer.PlaybackSpeedScale = 0.50f;
		}
	}

	private void LoadGamepadBinds() {
		MoveActionGamepad = ResourceLoader.Load( "res://resources/binds/actions/gamepad/move_player0.tres" );
		DashActionGamepad = ResourceLoader.Load( "res://resources/binds/actions/gamepad/dash_player0.tres" );
		SlideActionGamepad = ResourceLoader.Load( "res://resources/binds/actions/gamepad/slide_player0.tres" );
		UseWeaponActionGamepad = ResourceLoader.Load( "res://resources/binds/actions/gamepad/use_weapon_player0.tres" );
		NextWeaponActionGamepad = ResourceLoader.Load( "res://resources/binds/actions/gamepad/next_weapon_player0.tres" );
		PrevWeaponActionGamepad = ResourceLoader.Load( "res://resources/binds/actions/gamepad/prev_weapon_player0.tres" );
		SwitchWeaponModeActionGamepad = ResourceLoader.Load( "res://resources/binds/actions/gamepad/switch_weapon_mode_player0.tres" );
		BulletTimeActionGamepad = ResourceLoader.Load( "res://resources/binds/actions/gamepad/bullet_time_player0.tres" );
		OpenInventoryActionGamepad = ResourceLoader.Load( "res://resources/binds/actions/gamepad/open_inventory_player0.tres" );
		ArmAngleAction = ResourceLoader.Load( "res://resources/binds/actions/gamepad/arm_angle.tres" );
	}
	private void LoadKeyboardBinds() {
		MoveActionKeyboard = ResourceLoader.Load( "res://resources/binds/actions/keyboard/move.tres" );
		DashActionKeyboard = ResourceLoader.Load( "res://resources/binds/actions/keyboard/dash.tres" );
		SlideActionKeyboard = ResourceLoader.Load( "res://resources/binds/actions/keyboard/slide.tres" );
		UseWeaponActionKeyboard = ResourceLoader.Load( "res://resources/binds/actions/keyboard/use_weapon.tres" );
		NextWeaponActionKeyboard = ResourceLoader.Load( "res://resources/binds/actions/keyboard/next_weapon.tres" );
		PrevWeaponActionKeyboard = ResourceLoader.Load( "res://resources/binds/actions/keyboard/prev_weapon.tres" );
		SwitchWeaponModeActionKeyboard = ResourceLoader.Load( "res://resources/binds/actions/keyboard/switch_weapon_mode.tres" );
		BulletTimeActionKeyboard = ResourceLoader.Load( "res://resources/binds/actions/keyboard/bullet_time.tres" );
		OpenInventoryActionKeyboard = ResourceLoader.Load( "res://resources/binds/actions/keyboard/open_inventory.tres" );
	}
	private void LoadSfx() {
		ActionChannel = GetNode<AudioStreamPlayer2D>( "ActionChannel" );
		PainChannel = GetNode<AudioStreamPlayer2D>( "PainChannel" );
		SlowMoChannel = GetNode<AudioStreamPlayer2D>( "SlowMoChannel" );
		SlowMoChannel.Connect( "finished", Callable.From( OnSlowMoSfxFinished ) );

		DashChannel = GetNode<AudioStreamPlayer2D>( "DashChannel" );
		SlideChannel = GetNode<AudioStreamPlayer2D>( "SlideChannel" );
		MoveChannel = GetNode<AudioStreamPlayer2D>( "MoveChannel" );

		ChangeWeaponSfx = ResourceLoader.Load<AudioStream>( "res://sounds/player/change_weapon.ogg" );

		PainSfx = new List<AudioStream>{
			ResourceLoader.Load<AudioStream>( "res://sounds/player/pain0.ogg" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/player/pain1.ogg" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/player/pain2.ogg" )
		};
		DieSfx = new List<AudioStream>{
			ResourceLoader.Load<AudioStream>( "res://sounds/player/death1.ogg" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/player/death2.ogg" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/player/death3.ogg" )
		};
		DeathSfx = new List<AudioStream>{
			ResourceLoader.Load<AudioStream>( "res://sounds/player/dying_0.ogg" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/player/dying_1.ogg" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/player/dying_2.wav" )
		};
		MoveGravelSfx = new List<AudioStream>{
			ResourceLoader.Load<AudioStream>( "res://sounds/player/moveGravel0.ogg" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/player/moveGravel1.ogg" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/player/moveGravel2.ogg" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/player/moveGravel3.ogg" )
		};
		DashSfx = new List<AudioStream>{
			ResourceLoader.Load<AudioStream>( "res://sounds/player/jumpjet_burn_v2_m_01.wav" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/player/jumpjet_burn_v2_m_02.wav" )
		};
		SlideSfx = new List<AudioStream>{
			ResourceLoader.Load<AudioStream>( "res://sounds/player/slide0.ogg" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/player/slide1.ogg" )
		};
		SlowMoBeginSfx = ResourceLoader.Load<AudioStream>( "res://sounds/player/slowmo_begin.ogg" );
		SlowMoEndSfx = ResourceLoader.Load<AudioStream>( "res://sounds/player/slowmo_end.ogg" );
		DashExplosion = ResourceLoader.Load<AudioStream>( "res://sounds/player/dash_explosion.mp3" );
	}

	private void ExitBulletTime() {
		Flags &= ~PlayerFlags.BulletTime;
		HUD.GetReflexOverlay().Hide();
		Engine.TimeScale = 1.0f;
		AudioServer.PlaybackSpeedScale = 1.0f;

		SlowMoChannel.Stream = SlowMoEndSfx;
		SlowMoChannel.Play();
	}

	private void CmdSuicide() {
		Health = 0.0f;
		OnDeath( this );
	}

	private float LitValue = 0.0f;
	private Texture2D LastSpriteTexture = null;
	private Rect2I LastCropRect = new Rect2I();

	private void OnViewportFramePreDraw() {
		Texture2D spriteTexture = TorsoAnimation.SpriteFrames.GetFrameTexture( TorsoAnimation.Animation, TorsoAnimation.Frame );
		Image image = spriteTexture.GetImage();

		image.GetPixel( 0, 0 );

		LastSpriteTexture = TorsoAnimation.SpriteFrames.GetFrameTexture( TorsoAnimation.Animation, TorsoAnimation.Frame );
		Godot.Vector2 viewportScale = TorsoAnimation.GetViewportTransform().Scale;
		Godot.Vector2I screenPos = (Godot.Vector2I)( TorsoAnimation.GetScreenTransform().Origin * viewportScale );
		LastCropRect = new Godot.Rect2I( screenPos - ( ( (Godot.Vector2I)LastSpriteTexture.GetSize() / new Godot.Vector2I( 2, 2 ) ) * (Godot.Vector2I)viewportScale ), (Godot.Vector2I)LastSpriteTexture.GetSize() * (Godot.Vector2I)viewportScale );
	}
	private void OnViewportFramePostDraw() {
		if ( LastSpriteTexture == null ) {
			return;
		}
		Image viewImage = GetViewport().GetTexture().GetImage();
		Image spriteImage = LastSpriteTexture.GetImage();

		viewImage = viewImage.GetRegion( LastCropRect );
		viewImage.Resize( (int)LastSpriteTexture.GetSize().X, (int)LastSpriteTexture.GetSize().Y );
		viewImage.Convert( spriteImage.GetFormat() );

		Image finalImage = Image.CreateEmpty( (int)LastSpriteTexture.GetSize().X, (int)LastSpriteTexture.GetSize().Y, false, Image.Format.Rgba8 );
		finalImage.BlitRectMask( viewImage, spriteImage, new Rect2I( Vector2I.Zero, (Vector2I)LastSpriteTexture.GetSize() ), Vector2I.Zero );
		finalImage.FixAlphaEdges();
		finalImage.Resize( 1, 1, Image.Interpolation.Lanczos );
		spriteImage.Resize( 1, 1, Image.Interpolation.Lanczos );

		Color finalColor = finalImage.GetPixel( 0, 0 );
		Color baseColor = spriteImage.GetPixel( 0, 0 );
		if ( baseColor.A != 0.0f ) {
			finalColor = finalColor / finalColor.A;
			baseColor = baseColor / baseColor.A;
			LitValue = finalColor.Luminance - baseColor.Luminance;
		}
	}

	private void OnScreenSizeChanged() {
		ScreenSize = DisplayServer.WindowGetSize();
	}
	public override void _Ready() {
		base._Ready();

		Engine.MaxFps = 0;

		Health = 100.0f;
		Rage = 60.0f;

		LoadKeyboardBinds();
		LoadGamepadBinds();
		LoadSfx();

		SetProcessUnhandledInput( false );

		KeyboardInputMappings = ResourceLoader.Load( "res://resources/binds/binds_keyboard.tres" );
		GamepadInputMappings = ResourceLoader.Load( "res://resources/binds/binds_gamepad.tres" );

		//
		// initialize input context
		//
		SwitchToGamepad = ResourceLoader.Load( "res://resources/binds/actions/keyboard/switch_to_gamepad.tres" );
		SwitchToKeyboard = ResourceLoader.Load( "res://resources/binds/actions/gamepad/switch_to_keyboard.tres" );

//		SwitchToGamepad.Connect( "triggered", Callable.From<Resource>( SwitchInputMode ) );
//		SwitchToKeyboard.Connect( "triggered", Callable.From<Resource>( SwitchInputMode ) );
//		SwitchToKeyboard.Connect( "triggered", Callable.From( () => { SwitchInputMode( KeyboardInputMappings ); } ) );
//		SwitchToGamepad.Connect( "triggered", Callable.From( () => { SwitchInputMode( GamepadInputMappings ); } ) );

//		if ( Input.GetConnectedJoypads().Count > 0 ) {
//			SwitchInputMode( GamepadInputMappings );
//		} else {
			SwitchInputMode( KeyboardInputMappings );
//		}

		DashTime = GetNode<Timer>( "Timers/DashTime" );
		DashTime.Connect( "timeout", Callable.From( OnDashTimeTimeout ) );

		IdleTimer = GetNode<Timer>( "IdleAnimationTimer" );
		IdleTimer.Connect( "timeout", Callable.From( OnIdleAnimationTimerTimeout ) );

		IdleAnimation = GetNode<AnimatedSprite2D>( "Animations/Idle" );
		IdleAnimation.Connect( "animation_finished", Callable.From( OnIdleAnimationAnimationFinished ) );

		LegAnimation = GetNode<AnimatedSprite2D>( "Animations/Legs" );
		LegAnimation.Connect( "animation_looped", Callable.From( OnLegsAnimationLooped ) );

		TorsoAnimation = GetNode<AnimatedSprite2D>( "Animations/Torso" );
		Animations = GetNode<Node2D>( "Animations" );

		WalkEffect = GetNode<GpuParticles2D>( "Animations/DustPuff" );
		SlideEffect = GetNode<GpuParticles2D>( "Animations/SlidePuff" );
		DashEffect = GetNode<GpuParticles2D>( "Animations/DashEffect" );
		JumpkitSparks = GetNode<AnimatedSprite2D>( "Animations/JumpkitSparks" );

		SlideTime = GetNode<Timer>( "Timers/SlideTime" );
		SlideTime.Connect( "timeout", Callable.From( OnSlideTimeout ) );
		DashCooldownTime = GetNode<Timer>( "Timers/DashCooldownTime" );

		WeaponSlots = new List<WeaponSlot>();
		for ( int i = 0; i < MAX_WEAPON_SLOTS; i++ ) {
			WeaponSlots.Add( new WeaponSlot() );
			WeaponSlots[i].SetIndex( i );
		}

		LastUsedArm = ArmRight;

		ScreenSize = DisplayServer.WindowGetSize();

		GetTree().Root.SizeChanged += OnScreenSizeChanged;

//		RenderingServer.FramePostDraw += () => OnViewportFramePostDraw();
//		RenderingServer.FramePreDraw += () => OnViewportFramePreDraw();

		GetNode( "/root/Console" ).Call( "add_command", "suicide", Callable.From( CmdSuicide ) );

		if ( ArchiveSystem.Instance.IsLoaded() ) {
			Load();
		}
	}

	public override void _PhysicsProcess( double delta ) {
		base._PhysicsProcess( delta );

		if ( Velocity != Godot.Vector2.Zero ) {
			IdleReset();
		}

		float speed = MAX_SPEED;
		if ( ( Flags & PlayerFlags.Dashing ) != 0 ) {
			speed += 1800;
		}
		if ( ( Flags & PlayerFlags.Sliding ) != 0 ) {
			speed += 400;
		}

		InputVelocity = (Godot.Vector2)MoveAction.Get( "value_axis_2d" );
		if ( InputVelocity != Godot.Vector2.Zero ) {
			Velocity = Velocity.MoveToward( InputVelocity * speed, (float)delta * ACCEL );
		} else {
			Velocity = Velocity.MoveToward( Godot.Vector2.Zero, (float)delta * FRICTION );
		}

		if ( InputVelocity != Godot.Vector2.Zero ) {
			if ( ( Flags & PlayerFlags.Sliding ) == 0 && ( Flags & PlayerFlags.OnHorse ) == 0 ) {
				LegAnimation.CallDeferred( "play", "run" );
				WalkEffect.Emitting = true;
			}
		} else {
			LegAnimation.CallDeferred( "play", "idle" );
			WalkEffect.Emitting = false;
			SlideEffect.Emitting = false;
		}

		CallDeferred( "move_and_slide" );
    }

	private void CalcArmAnimation( Arm arm ) {
		arm.Animations.GlobalRotation = ArmAngle;
		arm.Animations.SpriteFrames = arm.GetAnimationSet();
		arm.Animations.FlipV = TorsoAnimation.FlipH;

		if ( arm.GetSlot() == WeaponSlot.INVALID ) {
			arm.Animations.CallDeferred( "play", InputVelocity != Godot.Vector2.Zero ? "run" : "idle" );
		} else {
			WeaponEntity weapon = WeaponSlots[ arm.GetSlot() ].GetWeapon();
			string animationName = "";
			switch ( weapon.GetCurrentState() ) {
			case WeaponEntity.WeaponState.Idle:
				animationName = "idle";
				break;
			case WeaponEntity.WeaponState.Reload:
				animationName = "reload";
				break;
			case WeaponEntity.WeaponState.Use:
				animationName = "use";
				break;
			case WeaponEntity.WeaponState.Empty:
				animationName = "empty";
				break;
			};
			if ( ( weapon.GetLastUsedMode() & WeaponEntity.Properties.IsOneHanded ) != 0 ) {
				if ( ( arm == ArmLeft && !arm.Animations.FlipH )
					|| ( arm == ArmRight && arm.Animations.FlipH ) ) {
					animationName += "_flip";
				}
			}
			arm.Animations.CallDeferred( "play", animationName );
		}
	}
	private void CheckStatus( float delta ) {
		if ( Rage < 100.0f ) {
			if ( ( Flags & PlayerFlags.UsedMana ) != 0 ) {
			}
			HUD.GetRageBar().Show();
		}
		if ( FrameDamage > 0.0f ) {
			Rage += FrameDamage * delta;
			FrameDamage = 0.0f;
			Flags |= PlayerFlags.UsedMana;
			HUD.GetRageBar().Show();
		}
		FrameDamage = 0.0f;
		if ( Health < 100.0f ) {
			Health += 0.075f * delta;
			Rage -= 0.5f * delta;
			// mana conversion ratio to health is extremely inefficient

			Flags |= PlayerFlags.UsedMana;
			HUD.GetHealthBar().Show();
		}
		if ( Rage > 100.0f ) {
			Rage = 100.0f;
		} else if ( Rage < 0.0f ) {
			Rage = 0.0f;
		}

		HUD.GetHealthBar().SetHealth( Health );
		HUD.GetRageBar().Value = Rage;
	}
    public override void _Process( double delta ) {
		base._Process( delta );

		// cool down the jet engine if applicable
		if ( DashBurnout > 0.0f && DashCooldownTime.TimeLeft == 0.0f ) {
			DashBurnout -= 0.10f;
			if ( DashBurnout < 0.0f ) {
				DashBurnout = 0.0f;
			}
		}

		if ( CurrentMappingContext == GamepadInputMappings ) {
			if ( ArmAngle > 0.0f ) {
				FlipSpriteRight();
			} else if ( ArmAngle < 0.0f ) {
				FlipSpriteLeft();
			}
		} else {
			Godot.Vector2 mousePosition;

			// TODO: implement settings manager in C#?
			if ( (int)GetNode( "/root/SettingsData" ).Get( "_window_mode" ) >= 2 ) {
				mousePosition = DisplayServer.MouseGetPosition();
			} else {
				mousePosition = GetViewport().GetMousePosition();
			}
			
			float y = mousePosition.Y;
			float x = mousePosition.X;
			int width = ScreenSize.X;
			int height = ScreenSize.Y;

			if ( LastMousePosition != mousePosition ) {
				LastMousePosition = mousePosition;
				IdleReset();
			}

			ArmAngle = (float)Math.Atan2( y - ( height / 2.0f ), x - ( width / 2.0f ) );
			if ( x > width * 0.5f ) {
				FlipSpriteRight();
			} else if ( x < width * 0.5f ) {
				FlipSpriteLeft();
			}
		}

		Arm back;
		Arm front;
		if ( TorsoAnimation.FlipH ) {
			back = ArmRight;
			front = ArmLeft;
		} else {
			back = ArmLeft;
			front = ArmRight;
		}

		CheckStatus( (float)delta );
		if ( ( Flags & PlayerFlags.BulletTime ) != 0 ) {
			Rage -= 0.05f * (float)delta;
		}

		ArmLeft.Animations.CallDeferred( "show" );
		ArmRight.Animations.CallDeferred( "show" );

		ArmLeft.Animations.Rotation = ArmAngle;
		ArmRight.Animations.Rotation = ArmAngle;

		if ( HandsUsed == Hands.Both ) {
			front = LastUsedArm;
			back.Animations.CallDeferred( "hide" );
		} else {
			back.Animations.CallDeferred( "show" );
		}

		Animations.CallDeferred( "move_child", back.Animations, 0 );
		Animations.CallDeferred( "move_child", front.Animations, 3 );

		front.CallDeferred( "show" );

		CalcArmAnimation( ArmLeft );
		CalcArmAnimation( ArmRight );
	}

	public void PickupAmmo( AmmoEntity ammo ) {
		AmmoStack stack = null;
		bool found = false;
		
		for ( int i = 0; i < AmmoStacks.Count(); i++ ) {
			if ( ammo.Data == AmmoStacks[i].AmmoType ) {
				found = true;
				stack = AmmoStacks[i];
				break;
			}
		}
		if ( !found ) {
			GD.Print( "Adding ammo stack" );
			stack = new AmmoStack();
			stack.SetType( ammo );
			AmmoStacks.Add( stack );
		}
		stack.AddItems( (int)( (Godot.Collections.Dictionary)ammo.Data.Get( "properties" ) )[ "stack_add_amount" ] );

		for ( int i = 0; i < MAX_WEAPON_SLOTS; i++ ) {
			WeaponSlot slot = WeaponSlots[i];
			if ( slot.IsUsed() && (int)( (Godot.Collections.Dictionary)slot.GetWeapon().Data.Get( "properties" ) )[ "ammo_type" ]
				== (int)( (Godot.Collections.Dictionary)ammo.Data.Get( "properties" ) )[ "type" ] )
			{
				GetNode( "/root/Console" ).Call( "print_line", "Assigned weapon slot ammunition...", true );
				slot.GetWeapon().SetReserve( stack );
				slot.GetWeapon().SetAmmo( ammo.Data );
			}
		}

		LastUsedArm = ArmRight;
	}
	public void PickupWeapon( WeaponEntity weapon ) {
		GetNode( "/root/Console" ).Call( "print_line", "Picked up weapon...", true );

		for ( int i = 0; i < MAX_WEAPON_SLOTS; i++ ) {
			if ( !WeaponSlots[i].IsUsed() ) {
				GetNode( "/root/Console" ).Call( "print_line", "Assigned weapon slot " + i.ToString() + "...", true );
				WeaponSlots[i].SetWeapon( weapon );
				CurrentWeapon = i;
				break;
			}
		}
		
		WeaponsStack.Add( weapon );

		TorsoAnimation.FlipH = false;
		LegAnimation.FlipH = false;

		AmmoStack stack = null;
		for ( int i = 0; i < AmmoStacks.Count; i++ ) {
			if ( (int)( (Godot.Collections.Dictionary)AmmoStacks[i].AmmoType.Get( "properties" ) )[ "type" ] ==
				(int)weapon.GetAmmoType() )
			{
				stack = AmmoStacks[i];
				break;
			}
		}
		if ( stack != null ) {
			weapon.SetReserve( stack );
			weapon.SetAmmo( stack.AmmoType );
		}
		if ( (bool)GetNode( "/root/SettingsData" ).Get( "_equip_weapon_on_pickup" ) ) {
			HUD.SetWeapon( weapon );

			// apply rules of various weapon properties
			if ( ( weapon.GetDefaultMode() & WeaponEntity.Properties.IsTwoHanded ) != 0 ) {
				ArmLeft.SetWeapon( CurrentWeapon );
				ArmRight.SetWeapon( CurrentWeapon );

				HandsUsed = Hands.Both;
				LastUsedArm = ArmRight;

				// this will automatically overwrite any other modes
				WeaponSlots[ ArmLeft.GetSlot() ].SetMode( weapon.GetDefaultMode() );
				WeaponSlots[ ArmRight.GetSlot() ].SetMode( weapon.GetDefaultMode() );
			}
			else if ( ( weapon.GetDefaultMode() & WeaponEntity.Properties.IsOneHanded ) != 0 ) {
				if ( LastUsedArm == null ) {
					LastUsedArm = ArmRight;
				}
				LastUsedArm.SetWeapon( CurrentWeapon );

				if ( LastUsedArm == ArmRight ) {
					HandsUsed = Hands.Right;
				} else if ( LastUsedArm == ArmLeft ) {
					HandsUsed = Hands.Left;
				}

				WeaponSlots[ LastUsedArm.GetSlot() ].SetMode( weapon.GetDefaultMode() );
			}

			// update the hand data
			LastUsedArm.SetWeapon( CurrentWeapon );
			WeaponSlots[ LastUsedArm.GetSlot() ].SetMode( weapon.GetProperties() );
			weapon.SetUseMode( weapon.GetDefaultMode() );
		}

		if ( (int)GetNode( "/root/GameConfiguration" ).Get( "_game_mode" ) == (int)GameMode.Multiplayer ) {
			( (MultiplayerData)GetTree().CurrentScene ).SendPlayerUpdate( this, MultiplayerData.UpdateType.WeaponSlots );
		}
	}
};