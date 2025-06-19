using Godot;

public partial class Spawner : Node2D {
	[Export]
	private Timer SpawnTime;

	public void Reset() => SpawnTime.Stop();
	public bool IsUsed() => SpawnTime.TimeLeft > 0.0f;
	public void Use() => SpawnTime.Start();
}
