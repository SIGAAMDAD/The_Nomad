using Godot;
using Renown.Thinkers;
using Renown.World;
using System;

public partial class EndlessSpawner : Node2D {
	[Export]
	private Resource[] Weapons;
	[Export]
	private Resource[] Ammo;
	[Export]
	private Resource[] AmmoAt1Minute;
	[Export]
	private Resource[] AmmoAt3Minutes;
	[Export]
	private Resource[] AmmoAt10Minutes;

	private Timer RespawnTimer;
	private Random Random = new Random();
	private int RangeMin = 1;
	private int RangeMax = 3;
	private int GatlingChance = 0;
	private int RangeMinGatling = 0;
	private int RangeMaxGatling = 0;
	private Resource[] AmmoList;

	private void Activate() {
		if ( !ResourceCache.Initialized ) {
			return;
		}

		Resource ammo = AmmoList[ Random.Next( 0, AmmoList.Length - 1 ) ];
		bool isGatling = false;
		if ( GatlingChance > 0 ) {
			isGatling = Random.Next( 0, 100 ) > GatlingChance;
		}
		int count = isGatling ? Random.Next( RangeMinGatling, RangeMaxGatling ) : Random.Next( RangeMin, RangeMax );

		for ( int i = 0; i < count; i++ ) {
			if ( isGatling ) {
				GatlingGunner mob = ResourceLoader.Load<PackedScene>( "res://scenes/mobs/mercenary/mercenary_gatling_gunner.tscn" ).Instantiate<GatlingGunner>();
				mob.SetFaction( GetTree().CurrentScene.GetNode<Faction>( "EvilFaction" ) );
				mob.Scale = new Vector2( 0.75f, 0.75f );
				GetTree().CurrentScene.GetNode( "NavigationRegion2D" ).AddChild( mob );
			} else {
				Resource weapon = Weapons[ Random.Next( 0, Weapons.Length - 1 ) ];
				Mercenary mob = ResourceLoader.Load<PackedScene>( "res://scenes/mobs/mercenary/mercenary.tscn" ).Instantiate<Mercenary>();
				mob.DefaultWeapon = weapon;
				mob.DefaultAmmo = ammo;
				mob.SetFaction( GetTree().CurrentScene.GetNode<Faction>( "EvilFaction" ) );
				mob.Scale = new Vector2( 0.75f, 0.75f );
				GetTree().CurrentScene.GetNode( "NavigationRegion2D" ).AddChild( mob );
			}
		}
	}

	private void OnOneMinuteMarkerHit() {
		AmmoList = AmmoAt1Minute;
		RangeMin = 2;
		RangeMax = 4;
		RespawnTimer.WaitTime = 20.0f;
	}
	private void OnThreeMinuteMarkerHit() {
		AmmoList = AmmoAt3Minutes;
		RangeMin = 3;
		RangeMax = 5;
		GatlingChance = 50;
		RangeMinGatling = 1;
		RangeMaxGatling = 1;
		RespawnTimer.WaitTime = 45.0f;
	}
	private void OnTenMinuteMarkerHit() {
		AmmoList = AmmoAt10Minutes;
		RangeMin = 4;
		RangeMax = 6;
		GatlingChance = 25;
		RangeMinGatling = 1;
		RangeMaxGatling = 2;
		RespawnTimer.WaitTime = 90.0f;
	}

	public override void _Ready() {
		base._Ready();

		AmmoList = Ammo;

		LevelData.Instance.Connect( "OneMinuteMarker", Callable.From( OnOneMinuteMarkerHit ) );
		LevelData.Instance.Connect( "ThreeMinuteMarker", Callable.From( OnThreeMinuteMarkerHit ) );
		LevelData.Instance.Connect( "TenMinuteMarker", Callable.From( OnTenMinuteMarkerHit ) );

		RespawnTimer = new Timer();
		RespawnTimer.Name = "RespawnTimer";
		RespawnTimer.WaitTime = 10.0f;
		RespawnTimer.Autostart = true;
		RespawnTimer.Connect( "timeout", Callable.From( Activate ) );
		AddChild( RespawnTimer );
	}
};