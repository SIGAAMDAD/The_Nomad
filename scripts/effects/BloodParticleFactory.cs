using Godot;

public partial class BloodParticleFactory : Node {
	private Timer ReleaseTimer = null;
	private Transform2D[] Transforms = null;
	private MultiMeshInstance2D MeshManager = null;

	private static BloodParticleFactory Instance = null;

	private void OnReleaseTimerTimeout() {
		int instanceCount = MeshManager.Multimesh.VisibleInstanceCount - 24;
		if ( instanceCount < 0 ) {
			instanceCount = 0;
		}
		MeshManager.Multimesh.VisibleInstanceCount = instanceCount;
		if ( Instance.MeshManager.Multimesh.VisibleInstanceCount > 0 ) {
			ReleaseTimer.Start();
		}
	}

	public override void _Ready() {
		base._Ready();

		Instance = this;

		ReleaseTimer = new Timer();
		ReleaseTimer.WaitTime = 5.0f;
		ReleaseTimer.OneShot = true;
		ReleaseTimer.SetProcess( false );
		ReleaseTimer.SetProcessInternal( false );
		ReleaseTimer.Connect( "timeout", Callable.From( OnReleaseTimerTimeout ) );
		AddChild( ReleaseTimer );
		
		MeshManager = new MultiMeshInstance2D();
		MeshManager.Multimesh = new MultiMesh();
		MeshManager.Multimesh.Mesh = new QuadMesh();
		( MeshManager.Multimesh.Mesh as QuadMesh ).Size = new Vector2( 4.0f, -4.0f );
		MeshManager.Texture = ResourceCache.GetTexture( "res://textures/blood1.png" );
		MeshManager.ZIndex = 3;
		MeshManager.SetProcess( false );
		MeshManager.SetProcessInternal( false );
		AddChild( MeshManager );

		// cache a shitload
		MeshManager.Multimesh.InstanceCount = 2048;
		MeshManager.Multimesh.VisibleInstanceCount = 0;

		Transforms = new Transform2D[ MeshManager.Multimesh.InstanceCount ];
	}
	public static void Create( Vector2 from, Vector2 to ) {
		int bloodAmount = (int)Mathf.Lerp( 2.0f, 60.0f, 1.0f / from.DistanceSquaredTo( to ) );

		int instanceCount = Instance.MeshManager.Multimesh.VisibleInstanceCount;
		int startIndex = instanceCount;

		instanceCount += bloodAmount;
		if ( instanceCount >= Instance.MeshManager.Multimesh.InstanceCount ) {
			instanceCount -= Instance.MeshManager.Multimesh.InstanceCount / 2;
		}

		if ( Instance.ReleaseTimer.IsStopped() ) {
			Instance.ReleaseTimer.Start();
		}

		for ( int i = 0; i < bloodAmount; i++ ) {
			Instance.Transforms[ startIndex + i ] = new Transform2D( 0.0f, to );
			Instance.MeshManager.Multimesh.SetInstanceTransform2D( startIndex + i, Instance.Transforms[i] );
		}

		Instance.MeshManager.Multimesh.VisibleInstanceCount = instanceCount;
	}
};