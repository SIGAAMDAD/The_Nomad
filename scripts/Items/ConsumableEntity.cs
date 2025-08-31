using Godot;

public partial class ConsumableEntity : CharacterBody2D {
	[Export]
	public Resource Data;

	public void Use() {
	}
	public override void _Ready() {
		base._Ready();

		if ( !ArchiveSystem.IsLoaded() && Data == null ) {
			Console.PrintError( string.Format( "ConsumableEntity._Ready: invalid ItemDefinition (null)" ) );
			QueueFree();
			return;
		}
	}
};