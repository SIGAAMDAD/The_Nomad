/*
===========================================================================
The Nomad AGPL Source Code
Copyright (C) 2025 Noah Van Til

The Nomad Source Code is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

The Nomad Source Code is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with The Nomad Source Code.  If not, see <http://www.gnu.org/licenses/>.

If you have questions concerning this license or the applicable additional
terms, you may contact me via email at nyvantil@gmail.com.
===========================================================================
*/

using Godot;
using Renown;
using ResourceCache;
using Menus;

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

			FlamethrowerStart = AudioCache.GetStream( "res://sounds/player/flamethrower_start.ogg" );
			FlamethrowerLoop = AudioCache.GetStream( "res://sounds/player/flamethrower_loop.ogg" );

			AudioChannel = new AudioStreamPlayer2D() {
				Name = nameof( AudioChannel ),
				VolumeDb = SettingsData.GetEffectsVolumeLinear()
			};
			GameEventBus.ConnectSignal( AudioChannel, AudioStreamPlayer2D.SignalName.Finished, this, OnFlamethrowerAudioFinished );
			AddChild( AudioChannel );

			GameEventBus.ConnectSignal( GetNode<Area2D>( "Area2D" ), Area2D.SignalName.BodyEntered, this, Callable.From<Node2D>( OnFlameBodyEntered ) );

			Visuals = GetNode<GpuParticles2D>( "Area2D/GPUParticles2D" );

			UseTimer = GetNode<Timer>( "UseTimer" );
			GameEventBus.ConnectSignal( UseTimer, Timer.SignalName.Timeout, this, OnUseTimerTimeout );

			CooldownTimer = GetNode<Timer>( "CooldownTwimer" );
			GameEventBus.ConnectSignal( CooldownTimer, Timer.SignalName.Timeout, this, () => Usable = true );
		}
	};
};