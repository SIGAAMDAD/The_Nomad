using Godot;

public partial class LightData : PointLight2D {
	[Export]
	private Area2D Area = null;

	private void OnLightAreaBodyShape2DEntered( Rid bodyRid, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		if ( body is Renown.Entity entity && entity != null ) {
			entity.AddLightSource( this );
		}
	}
	private void OnLightAreaBodyShape2DExited( Rid bodyRid, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		if ( body is Renown.Entity entity && entity != null ) {
			entity.RemoveLightSource( this );
		}
	}

	public override void _Ready() {
		base._Ready();

		if ( Area == null ) {
			Area = new Area2D();

			RectangleShape2D rect = new RectangleShape2D();
			rect.Size = new Godot.Vector2( Texture.GetWidth() * TextureScale, Texture.GetHeight() * TextureScale );
			
			CollisionShape2D shape = new CollisionShape2D();
			shape.Shape = rect;

			Area.AddChild( shape );
		}
		Area.Connect( "body_shape_entered", Callable.From<Rid, Node2D, int, int>( OnLightAreaBodyShape2DEntered ) );
		Area.Connect( "body_shape_exited", Callable.From<Rid, Node2D, int, int>( OnLightAreaBodyShape2DExited ) );
	}
};
