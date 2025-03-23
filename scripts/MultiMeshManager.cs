using Godot;
using System.Collections.Generic;

public partial class MultiMeshManager : MultiMeshInstance2D {
	private Stack<int> RetiredIds = null;
	private Godot.Vector2I ScreenSize = new Godot.Vector2I( 640, 480 );

	public override void _Ready() {
		base._Ready();

		RetiredIds = new Stack<int>( 4000 );

		// cache a shitload
		Multimesh.InstanceCount = 4000;
		Multimesh.VisibleInstanceCount = 0;
	}

	public int RegisterInstance( Node2D node ) {
		int id;

		if ( RetiredIds.TryPop( out id  ) ) {
			Multimesh.SetInstanceColor( id, Color.Color8( 1, 1, 1, 1 ) );
			return id;
		}

		id = Multimesh.VisibleInstanceCount;
		Multimesh.VisibleInstanceCount++;
		Multimesh.SetInstanceColor( id, Color.Color8( 1, 1, 1, 1 ) );
		return id;
	}
	public void RemoveInstance( Node2D node ) {
		int instanceId = (int)node.GetInstanceId();

		RetiredIds.Push( instanceId );
		Multimesh.SetInstanceColor( instanceId, Color.Color8( 0, 0, 0, 0 ) );
	}
};