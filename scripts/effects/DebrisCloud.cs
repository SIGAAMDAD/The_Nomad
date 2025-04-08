using Godot;
using System;
using System.Collections.Generic;

public partial class DebrisCloud : Node2D {
	private Timer Timer = null;
	private Texture2D Texture = null;
	private List<Godot.Vector2> Speeds = null;
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

		Texture = ResourceCache.GetTexture( "res://textures/env/dustcloud.png" );
	}
	public override void _Process( double delta ) {
		if ( ( Engine.GetProcessFrames() % 30 ) != 0 ) {
			return;
		}

		base._Process( delta );

		for ( int i = 0; i < MeshManager.Multimesh.VisibleInstanceCount; i++ ) {
			Godot.Vector2 position = MeshManager.Multimesh.GetInstanceTransform2D( i ).Origin;
			position.X += Speeds[i].X;
			position.Y += Speeds[i].Y;
			Transform2D transform = new Transform2D( 0.0f, position );
			MeshManager.Multimesh.SetInstanceTransform2D( i, transform );
		}
	}

    public void Create( Godot.Vector2 position ) {
		const int numSmokeClouds = 64;

		ZIndex = 3;

		MeshManager.Multimesh.VisibleInstanceCount = numSmokeClouds;
		Speeds = new List<Godot.Vector2>( numSmokeClouds );

		RandomNumberGenerator random = new RandomNumberGenerator();

		for ( int i = 0; i < numSmokeClouds; i++ ) {
			/*
			DebrisSmoke cloud = new DebrisSmoke();
			cloud.Texture = Texture;
			cloud.GlobalPosition = position;
			AddChild( cloud );
			*/
			Transform2D transform = new Transform2D( 0.0f, position );
			Speeds.Add( new Godot.Vector2( random.RandfRange( -0.25f, 0.25f ), random.RandfRange( -0.1f, 0.1f ) ) );
			MeshManager.Multimesh.SetInstanceTransform2D( i, transform );
		}
	}
};
