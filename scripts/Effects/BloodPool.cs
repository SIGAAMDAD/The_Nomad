using Godot;

public partial class BloodPool : MeshInstance2D {
	private class BloodDrop {
		public bool Active {
			get;
			private set;
		}
		private int Index;
		public static ShaderMaterial Material;

		public BloodDrop( int index ) {
			Index = index;
		}

		public void Start( Godot.Vector2 position ) {
			Active = true;
			Material.SetShaderParameter( string.Format( "positions[{0}]", Index ), position );
		}
		public void Animate( float value ) {
			Material.SetShaderParameter( string.Format( "scales[{0}]", Index ), value );
		}
		public void End() {
			Active = false;
		}
	};

	[Export]
	private int PoolSize = 64;
	[Export]
	private float GrowingTime = 0.2f;
	[Export]
	private float DryingTime = 4.0f;
	[Export]
	private float DelayUntilDryingStarts = 0.2f;

	private ShaderMaterial ShaderMaterial;
	private BloodDrop[] BloodyPool;

	private static BloodPool Instance;

	public override void _Ready() {
		ShaderMaterial = ( Mesh as PlaneMesh ).Material as ShaderMaterial;
		BloodyPool = new BloodDrop[ PoolSize ];

		ShaderMaterial.SetShaderParameter( "positions", new Godot.Vector2[ PoolSize ] );
		ShaderMaterial.SetShaderParameter( "scales", new float[ PoolSize ] );

		BloodDrop.Material = ShaderMaterial;
		for ( int i = 0; i < PoolSize; i++ ) {
			BloodyPool[ i ] = new BloodDrop( i );
		}

		Instance = this;
	}

	public static void Drop( Godot.Vector2 position ) {
		for ( int i = 0; i < Instance.BloodyPool.Length; i++ ) {
			if ( !Instance.BloodyPool[ i ].Active ) {
				Instance.BloodyPool[ i ].Start( position );
				Tween tween = Instance.GetTree().CreateTween();
				tween.TweenMethod( Callable.From( ( float value ) => Instance.BloodyPool[ i ].Animate( value ) ), 0.0f, Instance.GrowingTime, Instance.GrowingTime );
				tween.TweenMethod( Callable.From( ( float value ) => Instance.BloodyPool[ i ].Animate( value ) ), Instance.GrowingTime, 0.0f, Instance.DryingTime ).SetDelay( Instance.DelayUntilDryingStarts );
				tween.Connect( "finished", Callable.From( Instance.BloodyPool[ i ].End ) );
				break;
			}
		}
	}
};