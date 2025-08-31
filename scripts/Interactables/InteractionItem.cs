using Godot;

public enum InteractionType {
	Checkpoint,
	TreasureChest,
	Tutorial,
	Note,
	Door,
	Dialogue,
	CutsceneTrigger,
	ItemPickup,
	EndOfChallenge,
	HealthPack,
	MultiplayerFlag,

	EaglesPeak,

	Count
};

public abstract partial class InteractionItem : Area2D {
	public abstract InteractionType InteractionType { get; }

	/*
	===============
	OnInteractionAreaBody2DEntered
	===============
	*/
	protected virtual void OnInteractionAreaBody2DEntered( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
	}

	/*
	===============
	OnInteractionAreaBody2DExited
	===============
	*/
	protected virtual void OnInteractionAreaBody2DExited( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
	}

	/*
	===============
	_Ready
	===============
	*/
	/// <summary>
	/// godot initialization override
	/// </summary>
	public override void _Ready() {
		base._Ready();
	}
};