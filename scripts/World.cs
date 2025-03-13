using Godot;

public partial class World : Node2D {
	[Export]
	private Godot.Collections.Array<WorldArea> Areas;

	private WorldArea CurrentArea = null;
};
