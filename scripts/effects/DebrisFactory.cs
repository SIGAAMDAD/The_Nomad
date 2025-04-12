using System;
using Godot;

public partial class DebrisFactory : Node {
	private Timer ReleaseTimer = null;
	private Vector2[] Speeds = null;
	private Transform2D[] Transforms = null;
	private Color[] Colors = null;
	private MultiMeshInstance2D MeshManager = null;
	private RandomNumberGenerator Random = null;
	private static DebrisFactory Instance = null;

	private void OnReleaseTimerTimeout() {
		int instanceCount = MeshManager.Multimesh.VisibleInstanceCount - 1;
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
		ReleaseTimer.WaitTime = 3.5f;
		ReleaseTimer.OneShot = true;
		ReleaseTimer.SetProcess( false );
		ReleaseTimer.SetProcessInternal( false );
		ReleaseTimer.Connect( "timeout", Callable.From( OnReleaseTimerTimeout ) );
		AddChild( ReleaseTimer );
		
		MeshManager = new MultiMeshInstance2D();
		MeshManager.Multimesh = new MultiMesh();
		MeshManager.Multimesh.Mesh = new QuadMesh();
		MeshManager.Multimesh.UseColors = true;
		( MeshManager.Multimesh.Mesh as QuadMesh ).Size = new Vector2( 32.0f, -32.0f );
		MeshManager.Texture = ResourceCache.GetTexture( "res://textures/env/dustcloud.png" );
		MeshManager.ZIndex = 3;
		MeshManager.SetProcess( false );
		MeshManager.SetProcessInternal( false );
		AddChild( MeshManager );

		// cache a shitload
		MeshManager.Multimesh.InstanceCount = 8192;
		MeshManager.Multimesh.VisibleInstanceCount = 0;

		Speeds = new Vector2[ MeshManager.Multimesh.InstanceCount ];
		Transforms = new Transform2D[ MeshManager.Multimesh.InstanceCount ];
		Colors = new Color[ MeshManager.Multimesh.InstanceCount ];
		Random = new RandomNumberGenerator();

		for ( int i = 0; i < Colors.Length; i++ ) {
			Colors[i] = new Color( 1.0f, 0.25f, 0.0f, 1.0f );
		}
	}
	public override void _Process( double delta ) {
		if ( MeshManager.Multimesh.VisibleInstanceCount == 0 || ( Engine.GetProcessFrames() % 120 ) != 0 ) {
			return;
		}

		base._Process( delta );

		for ( int i = 0; i < Instance.MeshManager.Multimesh.VisibleInstanceCount; i++ ) {
			if ( Speeds[i].X > 0.0f ) {
				Speeds[i].X -= 0.0025f;
				if ( Speeds[i].X < 0.0f ) {
					Speeds[i].X = 0.0f;
				}
			} else if ( Speeds[i].X < 0.0f ) {
				Speeds[i].X += 0.0025f;
				if ( Speeds[i].X > 0.0f ) {
					Speeds[i].X = 0.0f;
				}
			}
			if ( Speeds[i].Y > 0.0f ) {
				Speeds[i].Y -= 0.0025f;
				if ( Speeds[i].Y < 0.0f ) {
					Speeds[i].Y = 0.0f;
				}
			} else if ( Speeds[i].Y < 0.0f ) {
				Speeds[i].Y += 0.0025f;
				if ( Speeds[i].Y > 0.0f ) {
					Speeds[i].Y = 0.0f;
				}
			}
			Transforms[i].Origin += Speeds[i];
			Colors[i].A -= 0.25f * (float)delta;

			MeshManager.Multimesh.SetInstanceColor( i, Colors[i] );
			MeshManager.Multimesh.SetInstanceTransform2D( i, Transforms[i] );
		}
	}
	public static void Create( Vector2 position ) {
		const int numSmokeClouds = 8;

		int instanceCount = Instance.MeshManager.Multimesh.VisibleInstanceCount;
		int startIndex = instanceCount;

		instanceCount += numSmokeClouds;
		if ( instanceCount > Instance.MeshManager.Multimesh.InstanceCount ) {
			instanceCount = numSmokeClouds;
			startIndex = 0;
		}

		if ( Instance.ReleaseTimer.IsStopped() ) {
			Instance.ReleaseTimer.Start();
		}

		for ( int i = 0; i < numSmokeClouds; i++ ) {
			Instance.Speeds[ startIndex + i ] = new Vector2( Instance.Random.RandfRange( -2.5f, 2.5f ), Instance.Random.RandfRange( -0.75f, 0.75f ) );
			Instance.Transforms[ startIndex + i ] = new Transform2D( 0.0f, position );
			Instance.Colors[ startIndex + i ].A = 1.0f;
			Instance.MeshManager.Multimesh.SetInstanceTransform2D( startIndex + i, Instance.Transforms[i] );
		}

		Instance.MeshManager.Multimesh.VisibleInstanceCount = instanceCount;
	}
};