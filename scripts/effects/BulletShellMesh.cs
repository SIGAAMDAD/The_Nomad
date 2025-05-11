using Godot;
using Renown;
using System.Collections.Generic;

public partial class BulletShellMesh : Node2D {
	private Dictionary<Resource, MultiMeshInstance2D> Meshes = new Dictionary<Resource, MultiMeshInstance2D>( 64 );
	private MultiMeshInstance2D Cloner;
	private RandomNumberGenerator Random = new RandomNumberGenerator();

	private static BulletShellMesh Instance = null;

	public override void _Ready() {
		base._Ready();

		Cloner = new MultiMeshInstance2D();
		Cloner.Multimesh = new MultiMesh();
		Cloner.Multimesh.Mesh = new QuadMesh();
		( Cloner.Multimesh.Mesh as QuadMesh ).Size = new Vector2( 5.0f, -2.0f );
		AddChild( Cloner );

		Instance = this;
	}
	private MultiMeshInstance2D AddMesh( Resource ammo ) {
		MultiMeshInstance2D meshInstance = (MultiMeshInstance2D)Cloner.Duplicate();
		meshInstance.Texture = (Texture2D)( (Godot.Collections.Dictionary)ammo.Get( "properties" ) )[ "casing_icon" ];
		meshInstance.Multimesh.InstanceCount = 4096;
		meshInstance.Multimesh.VisibleInstanceCount = 0;
		meshInstance.Show();
		AddChild( meshInstance );
		Meshes.Add( ammo, meshInstance );

		return meshInstance;
	}
	private static void OnTimerTimeout( Timer timer, Renown.Entity from, Resource ammo ) {
		AudioStream stream = null;
		switch ( (AmmoType)(uint)( (Godot.Collections.Dictionary)ammo.Get( "properties" ) )[ "type" ] ) {
		case AmmoType.Heavy:
		case AmmoType.Light:
			stream = ResourceCache.BulletShell[ Instance.Random.RandiRange( 0, ResourceCache.BulletShell.Length - 1 ) ];
			break;
		case AmmoType.Pellets:
			stream = ResourceCache.ShotgunShell[ Instance.Random.RandiRange( 0, ResourceCache.ShotgunShell.Length - 1 ) ];
			break;
		}
		from.PlaySound( null, stream );
		Instance.RemoveChild( timer );
		timer.QueueFree();
	}
	private void AddShellInternal( Entity from, Resource ammo ) {
		if ( !Meshes.TryGetValue( ammo, out MultiMeshInstance2D instance ) ) {
			instance = Instance.AddMesh( ammo );
		}
		if ( instance.Multimesh.VisibleInstanceCount >= 4096 ) {
			instance.Multimesh.VisibleInstanceCount = 0;
		}
		instance.Multimesh.SetInstanceTransform2D( instance.Multimesh.VisibleInstanceCount, new Transform2D( 0.0f, from.GlobalPosition ) );
		instance.Multimesh.VisibleInstanceCount++;

		Timer timer = new Timer();
		timer.WaitTime = 0.15f;
		timer.OneShot = true;
		timer.Connect( "timeout", Callable.From( () => { OnTimerTimeout( timer, from, ammo ); } ) );
		Instance.AddChild( timer );
		timer.Start();
	}
	public static void AddShellDeferred( Entity from, Resource ammo ) {
		Instance.CallDeferred( "AddShellInternal", from, ammo );
	}
	public static void AddShell( Entity from, Resource ammo ) {
		Instance.AddShellInternal( from, ammo );
	}
};