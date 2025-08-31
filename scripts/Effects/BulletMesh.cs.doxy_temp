using Godot;
using System.Collections.Generic;

public partial class BulletMesh : Node2D {
	private Dictionary<Resource, MultiMeshInstance2D> Meshes = new Dictionary<Resource, MultiMeshInstance2D>( 256 );
	private MultiMeshInstance2D Cloner;

	private static BulletMesh Instance = null;

	public override void _Ready() {
		base._Ready();

		Cloner = new MultiMeshInstance2D();
		Cloner.Multimesh = new MultiMesh();
		Cloner.Multimesh.Mesh = new QuadMesh();
		( Cloner.Multimesh.Mesh as QuadMesh ).Size = new Vector2( 3.0f, -1.0f );
		AddChild( Cloner );

		Instance = this;
	}

	public static void SetBulletTransform( Resource ammo, int instanceId, Vector2 position ) {
		Instance.Meshes[ ammo ].Multimesh.SetInstanceTransform2D( instanceId, new Transform2D( 0.0f, position ) );
	}

	private MultiMeshInstance2D AddMesh( Bullet bullet ) {
		MultiMeshInstance2D meshInstance = (MultiMeshInstance2D)Cloner.Duplicate();
		meshInstance.Texture = ResourceLoader.Load<Texture2D>( "res://resources/rifle_shell.png" );
		meshInstance.Multimesh.InstanceCount = 4096;
		meshInstance.Multimesh.VisibleInstanceCount = 0;
		meshInstance.Show();
		AddChild( meshInstance );
		Meshes.Add( bullet.AmmoType.Data, meshInstance );

		return meshInstance;
	}
	private int AddBulletInternal( Bullet bullet ) {
		if ( !Meshes.TryGetValue( bullet.AmmoType.Data, out MultiMeshInstance2D instance ) ) {
			instance = Instance.AddMesh( bullet );
		}
		if ( instance.Multimesh.VisibleInstanceCount >= 4096 ) {
			instance.Multimesh.VisibleInstanceCount = 0;
		}
		instance.Multimesh.SetInstanceTransform2D( instance.Multimesh.VisibleInstanceCount, new Transform2D( Mathf.Atan2( bullet.Direction.Y, bullet.Direction.X ), bullet.GlobalPosition ) );
		int instanceId = instance.Multimesh.VisibleInstanceCount;
		instance.Multimesh.VisibleInstanceCount++;

		return instanceId;
	}
	public static void AddBulletDeferred( Bullet bullet ) {
		Instance.CallDeferred( MethodName.AddBulletInternal, bullet );
	}
	public static int AddBullet( Bullet bullet ) {
		return Instance.AddBulletInternal( bullet );
	}
};