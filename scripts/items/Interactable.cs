using Godot;
using System;

public partial class Interactable : Sprite2D {
	public enum State {
		Weapon,
		Using,
		Broken,

		Invalid = -1,
	};

	private Area2D InteractionArea;
	private State Status;

	private void OnInteractionAreaBody2DEntered( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		if ( body is not Player ) {
			return;
		}

		
	}

	public override void _Ready() {
		base._Ready();

		InteractionArea = GetNode<Area2D>( "InteractionArea2D" );
	}
};
