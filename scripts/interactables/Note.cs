using Godot;
using System;

public partial class Note : InteractionItem {
	[Export]
	private StringName TextId;

	private CanvasLayer UIElement;

	protected override void OnInteractionAreaBody2DEntered( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		if ( body is not Player ) {
			return;
		}

		UIElement.Show();
	}
	protected override void OnInteractionAreaBody2DExited( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		if ( body is not Player ) {
			return;
		}

		UIElement.Hide();
	}
	public override InteractionType GetInteractionType() {
		return InteractionType.Note;
	}

	public override void _Ready() {
		base._Ready();

		Connect( "body_shape_entered", Callable.From<Rid, Node2D, int, int>( OnInteractionAreaBody2DEntered ) );
		Connect( "body_shape_exited", Callable.From<Rid, Node2D, int, int>( OnInteractionAreaBody2DExited ) );

		UIElement = GetNode<CanvasLayer>( "CanvasLayer" );

		RichTextLabel Label = GetNode<RichTextLabel>( "CanvasLayer/Control/MarginContainer/RichTextLabel" );
		Label.ParseBbcode( TranslationServer.Translate( TextId ) );
	}
};