using Godot;

public partial class WorldArea : Area2D {
	[Export]
	// the body shape where the player must be to have the location
	// loaded into RAM
	private CollisionPolygon2D LoadArea;

	private bool Loaded = false;

	public bool IsLoaded() {
		return Loaded;
	}
};