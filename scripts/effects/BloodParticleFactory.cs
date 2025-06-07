using Godot;

public partial class BloodParticleFactory : Node {
	private Timer ReleaseTimer = null;
	private MultiMeshInstance2D MeshManager = null;
	private RandomNumberGenerator RandomFactory = new RandomNumberGenerator();

	private static BloodParticleFactory Instance = null;
	private static readonly int BloodInstanceMax = 256;

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
		MeshManager.Texture = ResourceCache.GetTexture( "res://textures/blood1.png" );
		( MeshManager.Multimesh.Mesh as QuadMesh ).Size = new Vector2( 8.0f, -8.0f );
		MeshManager.ZIndex = 10;
		AddChild( MeshManager );

		// cache a shitload
		MeshManager.Multimesh.InstanceCount = BloodInstanceMax;
		MeshManager.Multimesh.VisibleInstanceCount = 0;

		// clean up on respawn
		LevelData.Instance.PlayerRespawn += () => {
			MeshManager.Multimesh.VisibleInstanceCount = 0;
		};
	}

	private void CreateBloodSplatter( Vector2 from, Vector2 to ) {
		int bloodAmount = 16;

		if ( MeshManager.Multimesh.VisibleInstanceCount >= BloodInstanceMax ) {
			MeshManager.Multimesh.VisibleInstanceCount = 0;
		}

		ReleaseTimer.Start();

		for ( int i = 0; i < bloodAmount; i++ ) {
			Godot.Vector2 position = to;
			position.Y += RandomFactory.RandfRange( -120.25f, 120.25f );
			position.X += RandomFactory.RandfRange( -150.25f, 150.25f );
			MeshManager.Multimesh.VisibleInstanceCount++;
			MeshManager.Multimesh.SetInstanceTransform2D( MeshManager.Multimesh.VisibleInstanceCount, new Transform2D( 0.0f, position ) );
		}
	}
	public static void Create( Vector2 from, Vector2 to ) {
		Instance.CreateBloodSplatter( from, to );
	}
	public static void CreateDeferred( Vector2 from, Vector2 to ) {
		Instance.CallDeferred( "CreateBloodSplatter", from, to );
	}
};