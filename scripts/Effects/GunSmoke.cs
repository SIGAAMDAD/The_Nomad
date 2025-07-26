using Godot;

public partial class GunSmoke : GpuParticles2D {
	public override void _Ready() {
		base._Ready();

		Emitting = true;

		Connect( SignalName.Finished, Callable.From( QueueFree ) );
	}
};