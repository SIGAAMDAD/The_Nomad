using Godot;
using System;
using System.Collections.Generic;

public partial class DebrisCloud : Node2D {
	private Timer Timer = null;
	private Godot.Vector2[] Speeds = null;
	private Transform2D[] Transforms = null;
	private MultiMeshInstance2D MeshManager;

	private void OnTimerTimeout() {
		Timer.QueueFree();
		QueueFree();
	}

	public override void _Ready() {
		Timer = GetNode<Timer>( "Timer" );
		Timer.SetProcess( false );
		Timer.SetProcessInternal( false );
		Timer.Connect( "timeout", Callable.From( OnTimerTimeout ) );
		
		MeshManager = GetNode<MultiMeshInstance2D>( "MultiMeshInstance2D" );
		MeshManager.SetProcess( false );
		MeshManager.SetProcessInternal( false );
		MeshManager.Multimesh.InstanceCount = 64;
		MeshManager.Multimesh.VisibleInstanceCount = 0;
	}
	public override void _Process( double delta ) {
		if ( ( Engine.GetProcessFrames() % 30 ) != 0 ) {
			return;
		}

		base._Process( delta );

		for ( int i = 0; i < Transforms.Length; i++ ) {
			Godot.Vector2 position = Transforms[i].Origin;
			position.X += Speeds[i].X;
			position.Y += Speeds[i].Y;
			Transforms[i].Origin = position;
			MeshManager.Multimesh.SetInstanceTransform2D( i, Transforms[i] );
		}
	}

    public void Create( Godot.Vector2 position ) {
		const int numSmokeClouds = 64;

		MeshManager.ZIndex = 3;

		MeshManager.Multimesh.VisibleInstanceCount = numSmokeClouds;
		Speeds = new Godot.Vector2[ numSmokeClouds ];
		Transforms = new Transform2D[ numSmokeClouds ];

		RandomNumberGenerator random = new RandomNumberGenerator();

		Timer.Start();

		for ( int i = 0; i < numSmokeClouds; i++ ) {
			/*
			DebrisSmoke cloud = new DebrisSmoke();
			cloud.Texture = Texture;
			cloud.GlobalPosition = position;
			AddChild( cloud );
			*/
			Transforms[i] = new Transform2D( 0.0f, position );
			Speeds[i] = new Godot.Vector2( random.RandfRange( -0.25f, 0.25f ), random.RandfRange( -0.1f, 0.1f ) );
			MeshManager.Multimesh.SetInstanceTransform2D( i, Transforms[i] );
		}
	}
};
