using Godot;
using System;

public partial class MultiMeshBase : Node2D {
	protected Rid CanvasItem;
	protected Rid MultiMesh;
	protected NetworkSyncObject SyncObject;

	private QuadMesh Mesh;
	private Texture2D Texture;

	public MultiMeshBase( Vector2 size, int nMaxInstances, Texture2D texture ) {
		Mesh = new QuadMesh();
		Mesh.Size = size;

		MultiMesh = RenderingServer.MultimeshCreate();
		switch ( SettingsData.GetParticleQuality() ) {
		case ParticleQuality.Low:
			RenderingServer.MultimeshSetPhysicsInterpolationQuality( MultiMesh, RenderingServer.MultimeshPhysicsInterpolationQuality.Fast );
			RenderingServer.MultimeshAllocateData( MultiMesh, nMaxInstances / 2, RenderingServer.MultimeshTransformFormat.Transform2D, false, false, false );
			break;
		case ParticleQuality.High:
			RenderingServer.MultimeshSetPhysicsInterpolationQuality( MultiMesh, RenderingServer.MultimeshPhysicsInterpolationQuality.High );
			RenderingServer.MultimeshAllocateData( MultiMesh, nMaxInstances, RenderingServer.MultimeshTransformFormat.Transform2D, false, false, false );
			break;
		};
		RenderingServer.MultimeshSetMesh( MultiMesh, Mesh.GetRid() );
		RenderingServer.MultimeshSetVisibleInstances( MultiMesh, 0 );

		CanvasItem = RenderingServer.CanvasItemCreate();
		RenderingServer.CanvasItemSetZIndex( CanvasItem, 8 );
		RenderingServer.CanvasItemSetParent( CanvasItem, LevelData.Instance.GetCanvasItem() );
		RenderingServer.CanvasItemSetVisible( CanvasItem, true );
		RenderingServer.CanvasItemAddMultimesh( CanvasItem, MultiMesh, texture.GetRid() );
		Texture = texture;
	}

	public override void _ExitTree() {
		Mesh?.Free();

		if ( MultiMesh.IsValid ) {
			RenderingServer.FreeRid( MultiMesh );
		}
		if ( CanvasItem.IsValid ) {
			RenderingServer.FreeRid( CanvasItem );
		}
	}
	public override void _Process( double delta ) {
		base._Process( delta );
	}

	protected int GetInstanceCount() => RenderingServer.MultimeshGetVisibleInstances( MultiMesh );
	protected void SetInstanceCount( int nInstanceCount ) => RenderingServer.MultimeshSetVisibleInstances( MultiMesh, nInstanceCount );
	protected Transform2D AddInstance( Vector2 position ) {
		int instanceCount = RenderingServer.MultimeshGetVisibleInstances( MultiMesh );
		if ( instanceCount >= RenderingServer.MultimeshGetInstanceCount( MultiMesh ) ) {
			instanceCount = 0;
		}

		Transform2D transform = new Transform2D( 0.0f, position );

		instanceCount++;
		RenderingServer.MultimeshSetVisibleInstances( MultiMesh, instanceCount );
		RenderingServer.MultimeshInstanceSetTransform2D( MultiMesh, instanceCount, transform );

		return transform;
	}
};