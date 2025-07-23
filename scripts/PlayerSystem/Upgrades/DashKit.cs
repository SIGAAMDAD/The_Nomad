using System;
using System.Collections.Generic;
using Godot;

namespace PlayerSystem.Upgrades {
	public sealed partial class DashKit : Node2D, IUpgradable {
		public int Level { get; private set; } = 0;
		public int MaxLevel { get; private set; } = 8;

		private Timer DashTime;
		private Timer DashBurnoutCooldownTimer;
		private Timer DashCooldownTime;

		private AudioStreamPlayer2D AudioChannel;

		private static readonly float DashTimerBase = 0.3f;

		private float DashBurnout = 0.0f;
		private float DashTimer = DashTimerBase;

		[Signal]
		public delegate void DashEndEventHandler();
		[Signal]
		public delegate void DashStartEventHandler();
		[Signal]
		public delegate void DashBurnedOutEventHandler();
		[Signal]
		public delegate void DashBurnoutChangedEventHandler( float nAmount );

		public IReadOnlyDictionary<string, int> GetUpgradeCost() => Level switch {
			0 => new Dictionary<string, int> { [ "" ] = 4 },
			_ => new Dictionary<string, int> { }
		};
		public void ApplyUpgrade() {
			Level = Math.Min( Level + 1, MaxLevel );
		}

		private void OnDashBurnoutCooldownTimerTimeout() {
			DashBurnout = 0.0f;
			DashTimer = 0.3f;

			AudioChannel.Stream = ResourceCache.GetSound( "res://sounds/player/dash_chargeup.ogg" );
			AudioChannel.CallDeferred( AudioStreamPlayer2D.MethodName.Play );
		}
		private void OnDashTimeTimeout() {
			EmitSignalDashEnd();
		}

		public void OnDash() {
			if ( DashBurnout >= 1.0f ) {
				AudioChannel.Stream = ResourceCache.DashExplosion;
				AudioChannel.CallDeferred( AudioStreamPlayer2D.MethodName.Play );

				DashBurnoutCooldownTimer.Start();

				EmitSignalDashBurnedOut();
				EmitSignalDashBurnoutChanged( 0.0f );

				return;
			}

			DashTime.WaitTime = DashTimer;
			DashTime.Start();

			AudioChannel.PitchScale = 1.0f + DashBurnout;
			AudioChannel.Stream = ResourceCache.DashSfx[ RNJesus.IntRange( 0, ResourceCache.DashSfx.Length - 1 ) ];
			AudioChannel.CallDeferred( AudioStreamPlayer2D.MethodName.Play );
			EmitSignalDashStart();

			DashBurnout += 0.30f;
			if ( DashTimer >= 0.10f ) {
				DashTimer -= 0.05f;
			}
			DashCooldownTime.WaitTime = 1.50f;
			DashCooldownTime.Start();

			EmitSignalDashBurnoutChanged( DashBurnout );
		}

		public bool CanDash() => DashBurnoutCooldownTimer.TimeLeft == 0.0f;

		public override void _Ready() {
			base._Ready();

			DashTime = new Timer();
			DashTime.Name = "DashTime";
			DashTime.WaitTime = DashTimer;
			DashTime.OneShot = true;
			DashTime.Connect( Timer.SignalName.Timeout, Callable.From( OnDashTimeTimeout ) );
			AddChild( DashTime );

			DashBurnoutCooldownTimer = new Timer();
			DashBurnoutCooldownTimer.Name = "DashBurnoutCooldownTimer";
			DashBurnoutCooldownTimer.WaitTime = 2.5f;
			DashBurnoutCooldownTimer.OneShot = true;
			DashBurnoutCooldownTimer.Connect( Timer.SignalName.Timeout, Callable.From( OnDashBurnoutCooldownTimerTimeout ) );
			AddChild( DashBurnoutCooldownTimer );

			AudioChannel = new AudioStreamPlayer2D();
			AudioChannel.Name = "AudioChannel";
			AudioChannel.VolumeDb = SettingsData.GetEffectsVolumeLinear();
			AddChild( AudioChannel );

			DashCooldownTime = new Timer();
			DashCooldownTime.Name = "DashCooldownTime";
			DashCooldownTime.WaitTime = 1.2f;
			DashCooldownTime.OneShot = true;
			AddChild( DashCooldownTime );
		}

		public override void _Process( double delta ) {
			base._Process( delta );

			// cool down the jet engine if applicable
			if ( DashBurnout > 0.0f && DashCooldownTime.TimeLeft == 0.0f ) {
				DashBurnout -= 0.10f * (float)delta;
				DashTimer += 0.05f * (float)delta;
				if ( DashBurnout < 0.0f ) {
					DashBurnout = 0.0f;
				}
				EmitSignalDashBurnoutChanged( DashBurnout );
			}
		}
	};
};