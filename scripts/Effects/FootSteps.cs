using System.Collections.Generic;
using Godot;
using Renown;

public partial class FootSteps : Node {
	private static readonly int MaxSteps = 24;

	private MultiMeshInstance2D MeshManager;
	private Queue<Transform2D> Steps = new Queue<Transform2D>( MaxSteps );

	private bool Wet = false;

	public override void _Ready() {
		base._Ready();

		MeshManager = new MultiMeshInstance2D();
		MeshManager.Multimesh = new MultiMesh();
		MeshManager.Texture = ResourceCache.GetTexture( "res://textures/env/footstep.png" );
		MeshManager.Multimesh.Mesh = new QuadMesh();
		MeshManager.Multimesh.VisibleInstanceCount = 0;
		MeshManager.Multimesh.InstanceCount = MaxSteps;
		MeshManager.Modulate = new Color( 1.0f, 1.0f, 1.0f, 0.75f );
		( MeshManager.Multimesh.Mesh as QuadMesh ).Size = new Vector2( 16.0f, -8.0f );
		AddChild( MeshManager );
	}
	private void CheckCapacity() {
		if ( Steps.Count < MaxSteps ) {
			return;
		}

		for ( int i = 0; i < 3; i++ ) {
			Steps.Dequeue();

			int instance = 0;
			foreach ( var step in Steps ) {
				MeshManager.Multimesh.SetInstanceTransform2D( instance, step );
				instance++;
			}
		}

		MeshManager.Multimesh.VisibleInstanceCount -= 3;
	}
	public void AddStep( Vector2 velocity, Vector2 position, GroundMaterialType GroundType, AudioStreamPlayer2D customChannel = null ) {
		Transform2D transform = new Transform2D( 0.0f, new Vector2( position.X, position.Y + 24.0f ) );
		CheckCapacity();
		MeshManager.Multimesh.VisibleInstanceCount++;
		MeshManager.Multimesh.SetInstanceTransform2D( Steps.Count, transform );
		Steps.Enqueue( transform );
		AudioStream stream = GroundType switch {
			GroundMaterialType.Stone => ResourceCache.MoveStoneSfx[ RNJesus.IntRange( 0, ResourceCache.MoveStoneSfx.Length - 1 ) ],
			GroundMaterialType.Sand => ResourceCache.PlayerMoveSandSfx[ RNJesus.IntRange( 0, ResourceCache.PlayerMoveSandSfx.Length - 1 ) ],
			GroundMaterialType.Water => ResourceCache.MoveWaterSfx[ RNJesus.IntRange( 0, ResourceCache.MoveWaterSfx.Length - 1 ) ],
			GroundMaterialType.Wood => ResourceCache.MoveWoodSfx[ RNJesus.IntRange( 0, ResourceCache.MoveWoodSfx.Length - 1 ) ],
			_ => ResourceCache.MoveGravelSfx[ RNJesus.IntRange( 0, ResourceCache.MoveGravelSfx.Length - 1 ) ],
		};
		GetParent<Entity>().PlaySound( customChannel, stream );
	}
};