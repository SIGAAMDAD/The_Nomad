using System.ComponentModel;
using Godot;

public partial class DebrisFactory : Node {
	private Timer ReleaseTimer = null;
	private Vector2[] Speeds = null;
	private Transform2D[] Transforms = null;
	private MultiMeshInstance2D MeshManager = null;
	private RandomNumberGenerator Random = null;

	private static DebrisFactory Instance = null;

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
		( MeshManager.Multimesh.Mesh as QuadMesh ).Size = new Vector2( 32.0f, -32.0f );
		MeshManager.Texture = ResourceCache.GetTexture( "res://textures/env/dustcloud.png" );
		MeshManager.ZIndex = 3;
		MeshManager.SetProcess( false );
		MeshManager.SetProcessInternal( false );
		AddChild( MeshManager );

		// cache a shitload
		MeshManager.Multimesh.InstanceCount = 1024;
		MeshManager.Multimesh.VisibleInstanceCount = 0;

		Speeds = new Vector2[ MeshManager.Multimesh.InstanceCount ];
		Transforms = new Transform2D[ MeshManager.Multimesh.InstanceCount ];
		Random = new RandomNumberGenerator();
	}
	public override void _Process( double delta ) {
		if ( ( Engine.GetProcessFrames() % 30 ) != 0 ) {
			return;
		}

		base._Process( delta );

		for ( int i = 0; i < Instance.MeshManager.Multimesh.VisibleInstanceCount; i++ ) {
			Vector2 position = Transforms[i].Origin;
			position.X += Speeds[i].X;
			position.Y += Speeds[i].Y;
			Transforms[i].Origin = position;

			MeshManager.Multimesh.SetInstanceTransform2D( i, Transforms[i] );
		}
	}

	public static void Create( Vector2 position ) {
		const int numSmokeClouds = 24;

		int instanceCount = Instance.MeshManager.Multimesh.VisibleInstanceCount;
		int startIndex = instanceCount;

		instanceCount += numSmokeClouds;
		if ( instanceCount >= Instance.MeshManager.Multimesh.InstanceCount ) {
			instanceCount -= Instance.MeshManager.Multimesh.InstanceCount / 2;
		}

		if ( Instance.ReleaseTimer.IsStopped() ) {
			Instance.ReleaseTimer.Start();
		}

		for ( int i = 0; i < numSmokeClouds; i++ ) {
			Instance.Speeds[ startIndex + i ] = new Vector2( Instance.Random.RandfRange( -0.25f, 0.25f ), Instance.Random.RandfRange( -0.1f, 0.1f ) );
			Instance.Transforms[ startIndex + i ] = new Transform2D( 0.0f, position );
			Instance.MeshManager.Multimesh.SetInstanceTransform2D( startIndex + i, Instance.Transforms[i] );
		}

		Instance.MeshManager.Multimesh.VisibleInstanceCount = instanceCount;
	}
};