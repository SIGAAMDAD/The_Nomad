using System.Runtime.CompilerServices;
using DialogueManagerRuntime;
using Godot;

public partial class HellbreakerExit : Node2D {
	private AudioStreamPlayer2D ExitSound;
	private AnimatedSprite2D UseAnimation;
	private AnimatedSprite2D DefaultAnimation;
	private Area2D Area;

	[Signal]
	public delegate void UsedEventHandler( HellbreakerExit exit );

	private void OnArea2DBodyShapeEntered( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		if ( body is not Player ) {
			return;
		}
		switch ( DefaultAnimation.Animation ) {
		case "idle":
			ExitSound.Play();
			DefaultAnimation.Hide();
			UseAnimation.Show();
			UseAnimation.Play( "use" );

			EmitSignalUsed( this );

			RemoveFromGroup( "HellbreakerExits" );
			break;
		case "dead":
			DialogueManager.ShowDialogueBalloon( ResourceCache.GetDialogue( "player" ), "hellbreaker_exit_used" );
			break;
		};
	}
	private void OnUseAnimationFinished() {
		UseAnimation.Hide();
		DefaultAnimation.Show();
		DefaultAnimation.Play( "dead" );
	}
	public bool IsUsed() {
		return DefaultAnimation.Animation == "dead";
	}
	public void Reset() {
		DefaultAnimation.Play( "idle" );
	}

	public override void _Ready() {
		base._Ready();

		AddToGroup( "HellbreakerExits" );

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
