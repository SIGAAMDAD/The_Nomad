using Godot;
using Renown;

public partial class Horse : CharacterBody2D {
	private Entity Rider = null;

	[Signal]
	public delegate void MountedEventHandler( Entity entity );
};