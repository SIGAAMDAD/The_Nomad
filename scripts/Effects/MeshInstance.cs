using Godot;

public class MeshInstance {
	public Transform2D Transform;
	public MeshInstance Prev = null;
	public MeshInstance Next = null;
	public ulong EndTime = 0;
	public int Id = 0;
	public float SpeedX = 0.0f;
	public float SpeedY = 0.0f;

	public MeshInstance() {
	}
};