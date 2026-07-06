/*
===========================================================================
The Nomad MPLv2 Source Code
Copyright (C) 2025-2026 Noah Van Til

This Source Code Form is subject to the terms of the Mozilla Public
License, v2. If a copy of the MPL was not distributed with this
file, You can obtain one at https://mozilla.org/MPL/2.0/.

This software is provided "as is", without warranty of any kind,
express or implied, including but not limited to the warranties
of merchantability, fitness for a particular purpose and noninfringement.
===========================================================================
*/

using System;
using Godot;
using Nomad.Game.Prefabs;
using Nomad.Game.Sdk.Player.Animation;

namespace Nomad.Game.Gameplay.Player.Animation
{
	internal sealed class PlayerIdleAnimationController : IDisposable
	{
		private const double START_SECONDS = 10.0;
		private const double MIDDLE_SECONDS = 20.0;
		private const double FULL_SECONDS = 30.0;

		private readonly PlayerAnimationPlaybackRouter _playback;
		private readonly Timer _timer;

		private int _stage;
		private bool _isIdle;
		private bool _isDisposed;

		public PlayerIdleAnimationController( PlayerPrefab prefab, PlayerAnimationPlaybackRouter playback )
		{
			_playback = playback ?? throw new ArgumentNullException( nameof( playback ) );
			_timer = new Timer() {
				OneShot = true,
				WaitTime = START_SECONDS
			};

			_timer.Timeout += OnTimeout;
			prefab.AddChild( _timer );
		}

		public void Begin()
		{
			_isIdle = true;
			_stage = 0;
			_playback.TravelState( PlayerAnimationState.Idle );
			ArmTimer( START_SECONDS );
		}

		public void End()
		{
			_isIdle = false;
			_stage = 0;
			_timer.Stop();
		}

		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			_timer.Timeout -= OnTimeout;
			_timer.Stop();
			_timer.Dispose();

			GC.SuppressFinalize( this );
			_isDisposed = true;
		}

		private void OnTimeout()
		{
			if ( !_isIdle ) {
				return;
			}

			switch ( _stage ) {
				case 0:
					_stage = 1;
					_playback.TravelState( PlayerAnimationState.IdleStart );
					ArmTimer( MIDDLE_SECONDS - START_SECONDS );
					break;

				case 1:
					_stage = 2;
					_playback.TravelState( PlayerAnimationState.IdleMiddle );
					ArmTimer( FULL_SECONDS - MIDDLE_SECONDS );
					break;

				case 2:
					_stage = 3;
					_playback.TravelState( PlayerAnimationState.IdleFull );
					break;
			}
		}

		private void ArmTimer( double waitTime )
		{
			_timer.WaitTime = waitTime;
			_timer.Start();
		}
	}
}
