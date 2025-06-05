using System.Collections.Generic;
using Godot;

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

	public Resource Data;

	private static readonly Dictionary<string, ExtraEffects> ExtraFlags = new Dictionary<string, ExtraEffects>{
		{ "Incendiary", ExtraEffects.Incendiary },
		{ "IonicCharge", ExtraEffects.IonicCharge },
		{ "Explosive", ExtraEffects.Explosive },
		{ "ArmorPiercing", ExtraEffects.ArmorPiercing },
		{ "HollowPoint", ExtraEffects.HollowPoint }
	};

	private AudioStream PickupSfx;
	private float Damage;
	private float Range;
	private float Velocity;
	private AmmoType AmmoType;
	private ExtraEffects Flags;
	private Curve DamageFalloff;

	private int PelletCount;
	private ShotgunBullshit ShotFlags;

	public AudioStream GetPickupSound() => PickupSfx;
	public AmmoType GetAmmoType() => AmmoType;
	public float GetDamage() => Damage;
	public float GetRange() => Range;
	public float GetVelocity() => Velocity;
	public ExtraEffects GetEffects() => Flags;
	public int GetPelletCount() => PelletCount;
	public ShotgunBullshit GetShotgunBullshit() => ShotFlags;
	public float GetDamageFalloff( float distance ) => DamageFalloff.SampleBaked( distance );

	public override void _Ready() {
		if ( Data == null ) {
			Console.PrintError( "Cannot initialize AmmoEntity without a valid ammo AmmoBase" );
			QueueFree();
			return;
		}

		Godot.Collections.Dictionary properties = (Godot.Collections.Dictionary)Data.Get( "properties" );

		if ( properties.ContainsKey( "effects" ) ) {
			Godot.Collections.Array<string> effects = (Godot.Collections.Array<string>)properties[ "effects" ];
			for ( int i = 0; i < effects.Count; i++ ) {
				Flags |= ExtraFlags[ effects[i] ];
			}
		}

		DamageFalloff = (Curve)properties[ "damage_falloff" ];
		PickupSfx = (AudioStream)properties[ "pickup_sfx" ];
		Damage = (float)properties[ "damage" ];
		Range = (float)properties[ "range" ];
		Velocity = (float)properties[ "velocity" ];
		AmmoType = (AmmoType)(int)properties[ "type" ];

		if ( AmmoType == AmmoType.Pellets ) {
			ShotFlags = (ShotgunBullshit)(int)properties[ "shotgun_bullshit" ];
			PelletCount = (int)properties[ "pellet_count" ];
		}
	}
};
