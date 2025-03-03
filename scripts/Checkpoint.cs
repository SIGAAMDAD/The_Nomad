using System.Resources;
using Godot;

public partial class Checkpoint : Area2D {
	private AudioStreamPlayer2D PassedCheckpoint;
	private AudioStreamPlayer2D Ambience;

	private PointLight2D Light;
	private CollisionShape2D Shape;
	private AnimatedSprite2D Bonfire;
	private Sprite2D Unlit;

	private bool Passed = false;

	public override void _ExitTree() {
		base._ExitTree();

		Shape.QueueFree();
		Bonfire.QueueFree();
		Unlit.QueueFree();
		Light.QueueFree();
		Ambience.QueueFree();
		PassedCheckpoint.QueueFree();
	}

	private void OnBodyShapeEntered( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		if ( body is not Player ) {
			return;
		}

		Ambience.Stop();

		Unlit.Show();

		Light.QueueFree();
		Shape.QueueFree();
		Ambience.QueueFree();
		Bonfire.QueueFree();
		PassedCheckpoint.Play();
		Passed = true;

		ArchiveSystem.SaveGame();
	}
    public override void _Ready() {
		PassedCheckpoint = GetNode<AudioStreamPlayer2D>( "PassCheckpoint" );
		Ambience = GetNode<AudioStreamPlayer2D>( "Ambience" );

		Light = GetNode<PointLight2D>( "PointLight2D" );
		Shape = GetNode<CollisionShape2D>( "CollisionShape2D" );
		Connect( "body_shape_entered", Callable.From<Rid, Node2D, int, int>( OnBodyShapeEntered ) );

		Bonfire = GetNode<AnimatedSprite2D>( "Bonfire" );
		Unlit = GetNode<Sprite2D>( "Unlit" );
	}
};
