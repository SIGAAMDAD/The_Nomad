using Godot;

public partial class BloodParticleFactory : Node2D {
	private Timer ReleaseTimer = null;
	private Transform2D[] Transforms = null;
	private MultiMeshInstance2D MeshManager = null;
	private RandomNumberGenerator RandomFactory = new RandomNumberGenerator();

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
		ReleaseTimer.WaitTime = 32.0f;
		ReleaseTimer.OneShot = true;
		ReleaseTimer.Connect( "timeout", Callable.From( OnReleaseTimerTimeout ) );
		AddChild( ReleaseTimer );
		
		MeshManager = new MultiMeshInstance2D();
		MeshManager.Multimesh = new MultiMesh();
		MeshManager.Multimesh.Mesh = new QuadMesh();
		( MeshManager.Multimesh.Mesh as QuadMesh ).Size = new Vector2( 4.0f, -4.0f );
		MeshManager.Texture = ResourceCache.GetTexture( "res://textures/blood1.png" );
		MeshManager.ZIndex = 6;
		AddChild( MeshManager );

		// cache a shitload
		MeshManager.Multimesh.InstanceCount = 4096;
		MeshManager.Multimesh.VisibleInstanceCount = 0;

		Transforms = new Transform2D[ MeshManager.Multimesh.InstanceCount ];
	}
	
	private void CreateBloodSplatter( Vector2 from, Vector2 to ) {
		int bloodAmount = 32;

		int instanceCount = MeshManager.Multimesh.VisibleInstanceCount;
		int startIndex = instanceCount;

		instanceCount += bloodAmount;
		if ( instanceCount > MeshManager.Multimesh.InstanceCount ) {
			instanceCount = bloodAmount;
			startIndex = 0;
		}

		ReleaseTimer.Start();

		for ( int i = 0; i < bloodAmount; i++ ) {
			Godot.Vector2 position = to;
			position.X += RandomFactory.RandfRange( -20.25f, 20.25f );
			position.X += RandomFactory.RandfRange( -50.25f, 50.25f );
			Transforms[ startIndex + i ] = new Transform2D( 0.0f, position );
			MeshManager.Multimesh.SetInstanceTransform2D( startIndex + i, Transforms[i] );
		}

		MeshManager.Multimesh.VisibleInstanceCount = instanceCount;
	}
	public static void Create( Vector2 from, Vector2 to ) {
		Instance.CreateBloodSplatter( from, to );
	}
	public static void CreateDeferred( Vector2 from, Vector2 to ) {
		Instance.CallDeferred( "CreateBloodSplatter", from, to );
	}
};