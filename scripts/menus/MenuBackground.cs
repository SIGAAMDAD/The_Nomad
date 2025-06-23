using Godot;

public partial class MenuBackground : Control {
	// TODO: change main menu based on game's save state
	private void OnScreenSizeChanged() {
		Godot.Vector2 windowSize = GetViewportRect().Size;
		Godot.Vector3 extents = new Godot.Vector3( windowSize.X / 2.0f, 0.0f, 0.0f );
		Godot.Vector2 position = new Godot.Vector2( extents.X, windowSize.Y );

		GpuParticles2D EmberEmitter = GetNode<GpuParticles2D>( "EmberParticlesEmitter" );
		EmberEmitter.GlobalPosition = position;
		( EmberEmitter.ProcessMaterial as ParticleProcessMaterial ).EmissionBoxExtents = extents;

		GpuParticles2D SandEmitter = GetNode<GpuParticles2D>( "SandParticlesEmitter" );
		SandEmitter.GlobalPosition = position;
		( SandEmitter.ProcessMaterial as ParticleProcessMaterial ).EmissionBoxExtents = extents;
	}

	public override void _Ready() {
		base._Ready();

		Console.PrintLine( "Initializing menu background..." );

		GetViewport().SizeChanged += OnScreenSizeChanged;

		Godot.Vector2 windowSize = GetViewportRect().Size;
		Godot.Vector3 extents = new Godot.Vector3( windowSize.X / 2.0f, 0.0f, 0.0f );
		Godot.Vector2 position = new Godot.Vector2( extents.X, windowSize.Y );

		GpuParticles2D EmberEmitter = GetNode<GpuParticles2D>( "EmberParticlesEmitter" );
		EmberEmitter.GlobalPosition = position;
		( EmberEmitter.ProcessMaterial as ParticleProcessMaterial ).EmissionBoxExtents = extents;

		GpuParticles2D SandEmitter = GetNode<GpuParticles2D>( "SandParticlesEmitter" );
		SandEmitter.GlobalPosition = position;
		( SandEmitter.ProcessMaterial as ParticleProcessMaterial ).EmissionBoxExtents = extents;
	}
};
