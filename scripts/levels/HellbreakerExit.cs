using Godot;

public partial class HellbreakerExit : Node2D {
	private AudioStreamPlayer2D ExitSound;
	private AnimatedSprite2D UseAnimation;
	private AnimatedSprite2D DefaultAnimation;
	private Area2D Area;

	private void OnArea2DBodyShapeEntered( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		if ( body is not Player ) {
			return;
		}
		ExitSound.Play();
		DefaultAnimation.Hide();
		UseAnimation.Show();
		UseAnimation.Play( "use" );
	}
	private void OnUseAnimationFinished() {
		UseAnimation.Hide();
		DefaultAnimation.Show();
		DefaultAnimation.Play( "dead" );
		Area.QueueFree();
	}

	public override void _Ready() {
		ExitSound = GetNode<AudioStreamPlayer2D>( "ExitSound" );
		ExitSound.GlobalPosition = GlobalPosition;

		UseAnimation = GetNode<AnimatedSprite2D>( "UseAnimation" );
		UseAnimation.Connect( "animation_finished", Callable.From( OnUseAnimationFinished ) );
		UseAnimation.Hide();

		DefaultAnimation = GetNode<AnimatedSprite2D>( "Idle" );
		DefaultAnimation.Show();

		Area = GetNode<Area2D>( "Area2D" );
		Area.Connect( "body_shape_entered", Callable.From<Rid, Node2D, int, int>( OnArea2DBodyShapeEntered ) );
	}
};
