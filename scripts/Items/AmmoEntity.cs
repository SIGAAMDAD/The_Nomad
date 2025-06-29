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

	public void Save() {
		using var writer = new SaveSystem.SaveSectionWriter( GetPath() );

		GD.Print( "Saving ammo data to " + GetPath() );

		writer.SaveString( "Id", (string)Data.Get( "id" ) );
		writer.SaveFloat( "Damage", Damage );
		writer.SaveFloat( "Range", Range );
		writer.SaveFloat( "Velocity", Velocity );
		writer.SaveUInt( "Type", (uint)AmmoType );
		writer.SaveUInt( "Flags", (uint)Flags );
		writer.SaveInt( "PelletCount", PelletCount );
		writer.SaveUInt( "ShotgunBullshit", (uint)ShotFlags );
	}
	public void Load( NodePath path ) {
		using var reader = ArchiveSystem.GetSection( path );

		GD.Print( "Loading ammo data from " + path );

		CallDeferred( "SetData", reader.LoadString( "Id" ) );
		Damage = reader.LoadFloat( "Damage" );
		Range = reader.LoadFloat( "Range" );
		Velocity = reader.LoadFloat( "Velocity" );
		AmmoType = (AmmoType)reader.LoadUInt( "Type" );
		Flags = (ExtraEffects)reader.LoadUInt( "Flags" );
		PelletCount = reader.LoadInt( "PelletCount" );
		ShotFlags = (ShotgunBullshit)reader.LoadUInt( "ShotgunBullshit" );
	}
	private void SetData( string id ) {
		Data = (Resource)ResourceCache.ItemDatabase.Call( "get_item", id );
		if ( Data == null ) {
			Console.PrintError( "Cannot initialize AmmoEntity without a valid ItemDefinition (id = " + id + ")" );
			QueueFree();
			return;
		}
		DamageFalloff = (Curve)( (Godot.Collections.Dictionary)Data.Get( "properties" ) )[ "damage_falloff" ];
	}

	// for generic calls
	public void Load() {
	}

	public override void _Ready() {
		if ( !ArchiveSystem.Instance.IsLoaded() && Data == null ) {
			Console.PrintError( "Cannot initialize AmmoEntity without a valid ItemDefinition" );
			QueueFree();
			return;
		}

		AddToGroup( "Archive" );

		if ( ArchiveSystem.Instance.IsLoaded() && Data == null ) {
			return;
		}

		Godot.Collections.Dictionary properties = (Godot.Collections.Dictionary)Data.Get( "properties" );

		if ( properties.ContainsKey( "effects" ) ) {
			Godot.Collections.Array<string> effects = (Godot.Collections.Array<string>)properties[ "effects" ];
			for ( int i = 0; i < effects.Count; i++ ) {
				Flags |= ExtraFlags[ effects[ i ] ];
			}
		}

		DamageFalloff = (Curve)properties[ "damage_falloff" ];
		DamageFalloff.Bake();
		
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
