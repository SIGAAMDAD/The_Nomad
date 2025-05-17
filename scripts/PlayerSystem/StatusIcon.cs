using Godot;

namespace PlayerSystem {
	public partial class StatusIcon : TextureRect {
		private StatusEffect StatusEffect;

		public override void _Ready() {
			base._Ready();

			LevelData.Instance.ThisPlayer.Die += ( source, target ) => { SetProcess( false ); Hide(); };

			Material = ResourceLoader.Load<Material>( "res://resources/materials/status_icon.tres" );
			Material.Set( "shader_parameter/progress", 1.0f );
			SetProcess( false );
		}
		public override void _Process( double delta ) {
			Material.Set( "shader_parameter/progress", Mathf.Lerp( 1.0f, 0.0f, StatusEffect.GetDuration() / StatusEffect.GetTimeLeft() ) );
		}

		public void Start( StatusEffect effect ) {
			Show();
			SetProcess( true );
			StatusEffect = effect;
			Texture = StatusEffect.GetIcon();

			StatusEffect.Timeout += Stop;
		}
		public void Stop() {
			Hide();
			SetProcess( false );

			StatusEffect.Timeout -= Stop;
		}
	};
};