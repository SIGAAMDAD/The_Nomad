using Godot;
using System.Collections.Generic;
using System.Reflection.Metadata;

public partial class DebrisFactory : Node {
	private Timer Timer = null;
	private List<Godot.Vector2> Speeds = null;
	private List<Transform2D> Transforms = null;
	private MultiMeshInstance2D MeshManager = null;
	private RandomNumberGenerator Random = null;

	private void OnTimerTimeout() {
		Timer.QueueFree();
		MeshManager.QueueFree();
		QueueFree();
	}

	public override void _Ready() {
		base._Ready();

		Timer = GetNode<Timer>( "Timer" );
		Timer.SetProcess( false );
		Timer.SetProcessInternal( false );
		Timer.Connect( "timeout", Callable.From( OnTimerTimeout ) );
		
		MeshManager = GetNode<MultiMeshInstance2D>( "MultiMeshInstance2D" );
		MeshManager.SetProcess( false );
		MeshManager.SetProcessInternal( false );

		// cache a shitload
		MeshManager.Multimesh.InstanceCount = 1024;
		MeshManager.Multimesh.VisibleInstanceCount = 0;

		Speeds = new List<Godot.Vector2>( MeshManager.Multimesh.InstanceCount );
		Transforms = new List<Transform2D>( MeshManager.Multimesh.InstanceCount );
		Random = new RandomNumberGenerator();
	}
	public override void _Process( double delta ) {
		if ( ( Engine.GetProcessFrames() % 30 ) != 0 ) {
			return;
		}

		base._Process( delta );

		for ( int i = 0; i < Speeds.Count; i++ ) {
			Transform2D transform = Transforms[i];
			Godot.Vector2 position = transform.Origin;
			position.X += Speeds[i].X;
			position.Y += Speeds[i].Y;
			transform.Origin = position;

			MeshManager.Multimesh.SetInstanceTransform2D( i, transform );
		}
	}

	public void Create( Godot.Vector2 position ) {
		const int numSmokeClouds = 64;

		int startIndex = MeshManager.Multimesh.VisibleInstanceCount;
		MeshManager.Multimesh.VisibleInstanceCount += numSmokeClouds;

		for ( int i = 0; i < numSmokeClouds; i++ ) {
			Transform2D transform = new Transform2D( 0.0f, position );
			Speeds.Add( new Godot.Vector2( Random.RandfRange( -0.25f, 0.25f ), Random.RandfRange( -0.1f, 0.1f ) ) );
			Transforms.Add( transform );
			MeshManager.Multimesh.SetInstanceTransform2D( startIndex + i, transform );
		}
	}
};