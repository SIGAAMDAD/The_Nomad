using Godot;
using Renown;

public partial class SeeThruSprite : Area2D {
	[Export]
	private Sprite2D Sprite;

	private ShaderMaterial InternalShader;
	private int EntityCount = 0;

	private void OnAreaBodyShape2DEntered( Rid bodyRid, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		if ( body is Entity entity && entity != null ) {
			InternalShader.SetShaderParameter( "alpha_blend", true );
			System.Threading.Interlocked.Increment( ref EntityCount );
		}
	}
	private void OnAreaBodyShape2DExited( Rid bodyRid, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		if ( body is Entity entity && entity != null ) {
			System.Threading.Interlocked.Decrement( ref EntityCount );
			if ( EntityCount == 0 ) {
				InternalShader.SetShaderParameter( "alpha_blend", false );
			}
		}
	}

	public override void _Ready() {
		base._Ready();

		InternalShader = ResourceLoader.Load<ShaderMaterial>( "res://resources/seethru_shader.tres" );

		Sprite.Material = InternalShader;

		Connect( "body_shape_entered", Callable.From<Rid, Node2D, int, int>( OnAreaBodyShape2DEntered ) );
		Connect( "body_shape_exited", Callable.From<Rid, Node2D, int, int>( OnAreaBodyShape2DExited ) );
	}
};
