using Godot;
using Nomad.Core;
using Nomad.Core.CVars;
using Nomad.Core.Engine.Windowing;
using Nomad.Core.Events;
using Nomad.CVars.Global;

namespace Nomad.Game.Prefabs {
	// TODO: make an engine agnostic version of GpuParticles2D that works in godot, unity, and UE5
	public partial class EmbersEmitter : GpuParticles2D {
		private ISubscriptionHandle _onWindowSizeChanged;

		public override void _Ready() {
			base._Ready();

			var windowSize = CVarSystem.GetCVar<WindowResolution>( Constants.CVars.EngineUtils.Display.WINDOW_RESOLUTION );
			_onWindowSizeChanged = windowSize.ValueChanged.Subscribe( OnWindowSizeChanged );
			CalculateBounds( windowSize.Value );
		}

		private void OnWindowSizeChanged( in CVarValueChangedEventArgs<WindowResolution> args ) {
			CalculateBounds( (WindowSize)args.NewValue );
		}

		private void CalculateBounds( WindowSize size ) {
			if ( ProcessMaterial is ParticleProcessMaterial material ) {
				material.EmissionBoxExtents = new Vector3( size.Width, 1.0f, 1.0f );
				GlobalPosition = new Vector2( size.Width / 2.0f, size.Height );
			}
		}
	};
};
