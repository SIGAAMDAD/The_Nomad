/*
===========================================================================
The Nomad MPLv2 Source Code
Copyright (C) 2025-2026 Noah Van Til

This Source Code Form is subject to the terms of the Mozilla Public
License, v2. If a copy of the MPL was not distributed with this
file, You can obtain one at https://mozilla.org/MPL/2.0/.

This software is provided "as is", without warranty of any kind,
express or implied including but not limited to the warranties
of merchantability, fitness for a particular purpose and noninfringement.
===========================================================================
*/

using Godot;
using Nomad.Game.Prefabs;
using System;

namespace Nomad.Game.Gameplay.Player.JumpKit
{
	/*
	===================================================================================

	JumpKitTimerDriver

	===================================================================================
	*/
	/// <summary>
	/// Owns all jump-kit timer nodes. This keeps PlayerJumpKit event-driven and avoids
	/// ProcessFrame subscriptions or per-frame component work.
	/// </summary>

	internal sealed class JumpKitTimerDriver : IDisposable
	{
		private const float HEAT_COOLING_TICK_SECONDS = 0.10f;
		private const float MIN_TIMER_SECONDS = 0.001f;

		private readonly Timer _dashTimer;
		private readonly Timer _coolingDelayTimer;
		private readonly Timer _coolingPulseTimer;
		private readonly Timer _burnoutTimer;

		private bool _isDisposed;

		public event Action? DashTimedOut;
		public event Action? CoolingDelayTimedOut;
		public event Action<float>? CoolingPulse;
		public event Action? BurnoutTimedOut;

		public JumpKitTimerDriver( PlayerPrefab prefab )
		{
			_dashTimer = CreateTimer( "JumpKitDashTimer", oneShot: true );
			_coolingDelayTimer = CreateTimer( "JumpKitCoolingDelayTimer", oneShot: true );
			_coolingPulseTimer = CreateTimer( "JumpKitCoolingPulseTimer", oneShot: false );
			_burnoutTimer = CreateTimer( "JumpKitBurnoutTimer", oneShot: true );

			prefab.AddChild( _dashTimer );
			prefab.AddChild( _coolingDelayTimer );
			prefab.AddChild( _coolingPulseTimer );
			prefab.AddChild( _burnoutTimer );

			_dashTimer.Timeout += OnDashTimedOut;
			_coolingDelayTimer.Timeout += OnCoolingDelayTimedOut;
			_coolingPulseTimer.Timeout += OnCoolingPulse;
			_burnoutTimer.Timeout += OnBurnoutTimedOut;
		}

		public void StartDashTimer( float durationSeconds )
		{
			StartOneShot( _dashTimer, durationSeconds );
		}

		public void StopDashTimer()
		{
			_dashTimer.Stop();
		}

		public void StartCoolingDelay( float delaySeconds )
		{
			StopCooling();

			if ( delaySeconds <= 0.0f ) {
				OnCoolingDelayTimedOut();
				return;
			}

			StartOneShot( _coolingDelayTimer, delaySeconds );
		}

		public void StartCoolingPulse()
		{
			_coolingPulseTimer.Stop();
			_coolingPulseTimer.WaitTime = HEAT_COOLING_TICK_SECONDS;
			_coolingPulseTimer.Start();
		}

		public void StopCooling()
		{
			_coolingDelayTimer.Stop();
			_coolingPulseTimer.Stop();
		}

		public void StartBurnoutTimer( float durationSeconds )
		{
			StopCooling();
			StartOneShot( _burnoutTimer, durationSeconds );
		}

		public void StopBurnoutTimer()
		{
			_burnoutTimer.Stop();
		}

		private static Timer CreateTimer( string name, bool oneShot )
		{
			return new Timer {
				Name = name,
				OneShot = oneShot,
				Autostart = false,
				ProcessCallback = Timer.TimerProcessCallback.Physics
			};
		}

		private static void StartOneShot( Timer timer, float durationSeconds )
		{
			timer.Stop();
			timer.WaitTime = durationSeconds > MIN_TIMER_SECONDS ? durationSeconds : MIN_TIMER_SECONDS;
			timer.Start();
		}

		private void OnDashTimedOut()
		{
			DashTimedOut?.Invoke();
		}

		private void OnCoolingDelayTimedOut()
		{
			CoolingDelayTimedOut?.Invoke();
		}

		private void OnCoolingPulse()
		{
			CoolingPulse?.Invoke( HEAT_COOLING_TICK_SECONDS );
		}

		private void OnBurnoutTimedOut()
		{
			BurnoutTimedOut?.Invoke();
		}

		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			_dashTimer.Timeout -= OnDashTimedOut;
			_coolingDelayTimer.Timeout -= OnCoolingDelayTimedOut;
			_coolingPulseTimer.Timeout -= OnCoolingPulse;
			_burnoutTimer.Timeout -= OnBurnoutTimedOut;

			_dashTimer.QueueFree();
			_coolingDelayTimer.QueueFree();
			_coolingPulseTimer.QueueFree();
			_burnoutTimer.QueueFree();

			DashTimedOut = null;
			CoolingDelayTimedOut = null;
			CoolingPulse = null;
			BurnoutTimedOut = null;

			_isDisposed = true;
			GC.SuppressFinalize( this );
		}
	}
}
