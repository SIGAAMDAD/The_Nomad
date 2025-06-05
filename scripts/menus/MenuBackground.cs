using Godot;

public partial class MenuBackground : Control {
	// TODO: change main menu based on game's save state
	private void OnScreenSizeChanged() {
		Godot.Vector3 extents;
		Godot.Vector2I windowSize = DisplayServer.WindowGetSize();

		extents.X = windowSize.X;
		extents.Y = 0.0f;
		extents.Z = 0.0f;

		GpuParticles2D EmberEmitter = GetNode<GpuParticles2D>( "EmberParticlesEmitter" );
		EmberEmitter.GlobalPosition = new Godot.Vector2( 0.0f, windowSize.Y );
		( EmberEmitter.ProcessMaterial as ParticleProcessMaterial ).EmissionBoxExtents = extents;

		GpuParticles2D SandEmitter = GetNode<GpuParticles2D>( "SandParticlesEmitter" );
		SandEmitter.GlobalPosition = new Godot.Vector2( 0.0f, windowSize.Y );
		( SandEmitter.ProcessMaterial as ParticleProcessMaterial ).EmissionBoxExtents = extents;
	}

	public override void _Ready() {
		base._Ready();

		GetTree().Root.SizeChanged += OnScreenSizeChanged;
	}
};
