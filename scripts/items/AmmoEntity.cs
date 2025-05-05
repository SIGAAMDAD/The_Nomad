using System.Collections.Generic;
using Godot;
using Steamworks;

public partial class AmmoEntity : Node2D {
	public enum ExtraEffects : uint {
		Incendiary		= 0x0001,
		IonicCharge		= 0x0002,
		Explosive		= 0x0004,
		ArmorPiercing	= 0x0008,
		HollowPoint		= 0x0010
	};
	public enum ShotgunBullshit {
		Flechette,
		Buckshot,
		Birdshot,
		Shrapnel,
		Slug,

		None
	};

	[Export]
	public Resource Data = null;

	private ExtraEffects Flags;
	private static readonly Dictionary<string, ExtraEffects> ExtraFlags = new Dictionary<string, ExtraEffects>{
		{ "Incendiary", ExtraEffects.Incendiary },
		{ "IonicCharge", ExtraEffects.IonicCharge },
		{ "Explosive", ExtraEffects.Explosive },
		{ "ArmorPiercing", ExtraEffects.ArmorPiercing },
		{ "HollowPoint", ExtraEffects.HollowPoint }
	};

	private Area2D PickupArea;
	private Sprite2D IconSprite;

	private AudioStream PickupSfx;
	private float Damage;
	private AmmoType AmmoType;

	public AudioStream GetPickupSound() => PickupSfx;
	public AmmoType GetAmmoType() => AmmoType;
	public float GetDamage() => Damage;
	public ExtraEffects GetEffects() => Flags;

	private void OnPickupArea2DBodyShapeEntered( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		if ( body is not Player player ) {
			return;
		}

		IconSprite.QueueFree();

		PickupArea.CallDeferred( "remove_child", PickupArea.GetChild( 0 ) );
		PickupArea.GetChild( 0 ).CallDeferred( "queue_free" );
		PickupArea.CallDeferred( "queue_free" );
		CallDeferred( "remove_child", PickupArea );

		CallDeferred( "reparent", body );
		player.PickupAmmo( this );
	}

	public override void _Ready() {
		if ( Data == null ) {
			Console.PrintError( "Cannot initialize AmmoEntity without a valid ammo AmmoBase" );
			QueueFree();
			return;
		}

		IconSprite = new Sprite2D();
		IconSprite.Name = "Icon";
		IconSprite.Texture = (Texture2D)Data.Get( "icon" );
		IconSprite.ProcessMode = ProcessModeEnum.Disabled;
		IconSprite.UseParentMaterial = true;
		AddChild( IconSprite );

		CircleShape2D circle = new CircleShape2D();
		circle.Radius = 7.0f;

		CollisionShape2D shape = new CollisionShape2D();
		shape.Shape = circle;

		PickupArea = new Area2D();
		PickupArea.CollisionLayer = 13;
		PickupArea.CollisionMask = 13;
		PickupArea.Name = "PickupArea";
		PickupArea.Connect( "body_shape_entered", Callable.From<Rid, Node2D, int, int>( OnPickupArea2DBodyShapeEntered ) );
		PickupArea.AddChild( shape );
		AddChild( PickupArea );

		Godot.Collections.Dictionary properties = (Godot.Collections.Dictionary)Data.Get( "properties" );

		if ( properties.ContainsKey( "properties" ) ) {
			Godot.Collections.Array<string> effects = (Godot.Collections.Array<string>)properties[ "effects" ];
			for ( int i = 0; i < effects.Count; i++ ) {
				Flags |= ExtraFlags[ effects[i] ];
			}
		}

		PickupSfx = (AudioStream)properties[ "pickup_sfx" ];
		Damage = (float)properties[ "damage" ];
		AmmoType = (AmmoType)(uint)properties[ "type" ];
	}
};
