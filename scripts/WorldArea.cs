using Godot;

public partial class WorldArea : Area2D {
	[Export]
	// the body shape where the player must be to have the location
	// loaded into RAM
	private CollisionPolygon2D LoadArea;
	[Export]
	protected string AreaName;

	private bool Loaded = false;

	public string GetAreaName() {
		return AreaName;
	}
	public bool IsLoaded() {
		return Loaded;
	}
};