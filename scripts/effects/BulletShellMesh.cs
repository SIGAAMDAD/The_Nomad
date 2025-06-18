using Godot;
using Renown;
using System.Collections.Generic;

public partial class BulletShellMesh : Node {
	private Dictionary<Resource, MultiMeshInstance2D> Meshes = new Dictionary<Resource, MultiMeshInstance2D>( 64 );

	private static BulletShellMesh Instance = null;
	private static readonly int BulletShellInstanceMax = 256;

	private NetworkWriter SyncObject = new NetworkWriter( 1024 );

	public override void _Ready() {
		base._Ready();

		Instance = this;
	}
	private MultiMeshInstance2D AddMesh( Resource ammo ) {
		MultiMeshInstance2D meshInstance = new MultiMeshInstance2D();
		meshInstance.Multimesh = new MultiMesh();
		meshInstance.Multimesh.Mesh = new QuadMesh();
		( meshInstance.Multimesh.Mesh as QuadMesh ).Size = new Vector2( 10.0f, -4.0f );
		meshInstance.Texture = (Texture2D)( (Godot.Collections.Dictionary)ammo.Get( "properties" ) )[ "casing_icon" ];
		meshInstance.Multimesh.InstanceCount = BulletShellInstanceMax;
		meshInstance.Multimesh.VisibleInstanceCount = 0;
		meshInstance.ZIndex = 10;
		meshInstance.Show();
		AddChild( meshInstance );
		Meshes.Add( ammo, meshInstance );

		return meshInstance;
	}
	private static void OnTimerTimeout( Timer timer, Entity from, Resource ammo ) {
		AudioStream stream = (AmmoType)(uint)( (Godot.Collections.Dictionary)ammo.Get( "properties" ) )[ "type" ] switch {
			AmmoType.Light => ResourceCache.BulletShell[ RNJesus.IntRange( 0, ResourceCache.BulletShell.Length - 1 ) ],
			AmmoType.Heavy => ResourceCache.BulletShell[ RNJesus.IntRange( 0, ResourceCache.BulletShell.Length - 1 ) ],
			AmmoType.Pellets => ResourceCache.ShotgunShell[ RNJesus.IntRange( 0, ResourceCache.ShotgunShell.Length - 1 ) ],
			_ => null
		};

		AudioStreamPlayer2D player = new AudioStreamPlayer2D();
		player.Stream = stream;
		player.VolumeDb = SettingsData.GetEffectsVolumeLinear();
		player.Connect( "finished", Callable.From( () => { from.RemoveChild( player ); player.QueueFree(); } ) );
		from.AddChild( player );
		player.Play();
		Instance.RemoveChild( timer );
		timer.QueueFree();
	}
	private void AddShellInternal( Entity from, Resource ammo ) {
		if ( !Meshes.TryGetValue( ammo, out MultiMeshInstance2D instance ) ) {
			instance = Instance.AddMesh( ammo );
		}
		if ( instance.Multimesh.VisibleInstanceCount >= BulletShellInstanceMax ) {
			instance.Multimesh.VisibleInstanceCount = 0;
		}
		instance.Multimesh.VisibleInstanceCount++;
		instance.Multimesh.SetInstanceTransform2D( instance.Multimesh.VisibleInstanceCount, new Transform2D( 0.0f, from.GlobalPosition ) );

		Timer timer = new Timer();
		timer.WaitTime = 0.15f;
		timer.OneShot = true;
		timer.Connect( "timeout", Callable.From( () => OnTimerTimeout( timer, from, ammo ) ) );
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