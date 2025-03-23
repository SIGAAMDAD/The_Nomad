using Godot;

public partial class AISpriteBatcher : Node {
	[Export]
	private MobBase[] SpriteList;

	private MultiMeshManager MultiMesh;

	public override void _Ready() {
		base._Ready();

		MultiMesh = (MultiMeshManager)GetNode<MultiMeshInstance2D>( "MultiMeshInstance2D" );
		for ( int i = 0; i < SpriteList.Length; i++ ) {
			MultiMesh.Multimesh.SetInstanceTransform2D( (int)SpriteList[i].GetInstanceId(), SpriteList[i].Transform );
		}
	}
	public override void _Process( double delta ) {
		base._Process( delta );

		for ( int i = 0; i < SpriteList.Length; i++ ) {
			MultiMesh.Multimesh.SetInstanceTransform2D( (int)SpriteList[i].GetInstanceId(), SpriteList[i].Transform );
		}
	}
};