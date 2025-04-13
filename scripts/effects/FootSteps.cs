using System.Collections.Generic;
using Godot;

public partial class FootSteps : Node {
	private static readonly int MaxSteps = 16;

	private MultiMeshInstance2D MeshManager;
	private Queue<Transform2D> Steps = new Queue<Transform2D>( MaxSteps );

	public override void _Ready() {
		base._Ready();

		MeshManager = new MultiMeshInstance2D();
		MeshManager.Multimesh = new MultiMesh();
		MeshManager.Texture = ResourceCache.GetTexture( "res://resources/ui_background_gradient.tres" );
		MeshManager.Multimesh.Mesh = new QuadMesh();
		MeshManager.Multimesh.VisibleInstanceCount = 0;
		MeshManager.Multimesh.InstanceCount = MaxSteps;
		( MeshManager.Multimesh.Mesh as QuadMesh ).Size = new Vector2( 6, -3.0f );
		AddChild( MeshManager );
	}
	private void CheckCapacity() {
		if ( Steps.Count < MaxSteps ) {
			return;
		}
		Steps.Dequeue();

		int instance = 0;
		foreach ( var step in Steps ) {
			MeshManager.Multimesh.SetInstanceTransform2D( instance, step );
			instance++;
		}

		MeshManager.Multimesh.VisibleInstanceCount--;
	}
	public void AddStep( Vector2 position ) {
		Transform2D transform = new Transform2D( 0.0f, position );
		Steps.Enqueue( transform );
		CheckCapacity();
		MeshManager.Multimesh.VisibleInstanceCount++;
		MeshManager.Multimesh.SetInstanceTransform2D( Steps.Count, transform );
	}
};