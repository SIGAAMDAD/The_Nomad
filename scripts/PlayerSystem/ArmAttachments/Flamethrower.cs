using Godot;
using Renown;

namespace PlayerSystem.ArmAttachments {
	public partial class Flamethrower : ArmAttachment {
		private AudioStreamPlayer2D AudioChannel;
		private AudioStream FlamethrowerStart;
		private AudioStream FlamethrowerLoop;
		private GpuParticles2D Visuals;
		private Timer UseTimer;

		private bool Usable = true;

		public override void Use() {
			if ( !Usable ) {
				return;
			}
			AudioChannel.Stream = FlamethrowerStart;
			AudioChannel.Set( "parameters/looping", false );
			AudioChannel.Play();

			Visuals.Emitting = true;
			Visuals.Show();

			Usable = false;
			UseTimer.Start();
		}

		private void OnUseTimerTimeout() {
			Visuals.Emitting = false;
			AudioChannel.Stop();
			CooldownTimer.Start();
		}
		private void OnFlameBodyEntered( Node2D body ) {
			if ( body is StaticBody2D ) {

			} else if ( body is Entity entity && entity != null ) {
				entity.AddStatusEffect( "status_burning" );
			}
		}
		private void OnFlamethrowerAudioFinished() {
			if ( AudioChannel.Stream == FlamethrowerLoop ) {
				return;
			}
			AudioChannel.Stream = FlamethrowerLoop;
			AudioChannel.Set( "parameters/looping", false );
			AudioChannel.Play();
		}

		public override void _Ready() {
			base._Ready();

			FlamethrowerStart = ResourceCache.GetSound( "res://sounds/player/flamethrower_start.ogg" );
			FlamethrowerLoop = ResourceCache.GetSound( "res://sounds/player/flamethrower_loop.ogg" );

			AudioChannel = new AudioStreamPlayer2D();
			AudioChannel.VolumeDb = SettingsData.GetEffectsVolumeLinear();
			AudioChannel.Connect( AudioStreamPlayer2D.SignalName.Finished, Callable.From( OnFlamethrowerAudioFinished ) );
			AddChild( AudioChannel );

			Area2D area = GetNode<Area2D>( "Area2D" );
			area.Connect( Area2D.SignalName.BodyEntered, Callable.From<Node2D>( OnFlameBodyEntered ) );

			Visuals = GetNode<GpuParticles2D>( "Area2D/GPUParticles2D" );

			UseTimer = GetNode<Timer>( "UseTimer" );
			UseTimer.Connect( Timer.SignalName.Timeout, Callable.From( OnUseTimerTimeout ) );

			CooldownTimer = GetNode<Timer>( "CooldownTimer" );
			CooldownTimer.Connect( Timer.SignalName.Timeout, Callable.From( () => Usable = true ) );
		}
	};
};