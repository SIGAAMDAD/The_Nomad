using Godot;

public partial class MultiMeshManager : MultiMeshInstance2D {
	[Export]
	private Node2D[] Nodes = null;

	public override void _Ready() {
		base._Ready();

		Multimesh.InstanceCount = Nodes.Length;
		Multimesh.VisibleInstanceCount = Nodes.Length;
		for ( int i = 0; i < Nodes.Length; i++ ) {
			Transform2D transform = new Transform2D( 0.0f, Nodes[i].GlobalPosition );
			Multimesh.SetInstanceTransform2D( i, transform );
		}
	}
};