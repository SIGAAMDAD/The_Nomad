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
using Nomad.Game.Sdk.Player.State;

namespace Nomad.Game.Application.Gameplay.Player.Movement
{
	internal sealed class PlayerSlideController : IDisposable
	{
		private readonly PlayerMovementRuntime _runtime;
		private readonly PlayerCameraController _camera;
		private readonly IPlayerFlagService _flags;
		private readonly Timer _timer;

		private bool _isDisposed;

		public PlayerSlideController(
			PlayerPrefab prefab,
			PlayerMovementRuntime runtime,
			PlayerCameraController camera,
			IPlayerFlagService flags
		)
		{
			_runtime = runtime;
			_camera = camera;
			_flags = flags;
			_timer = new Timer {
				WaitTime = Sdk.Player.Constants.SLIDE_DURATION,
				OneShot = true
			};
			_timer.Timeout += OnTimerTimeout;
			prefab.AddChild( _timer );
		}

		public void TryStart( Vector3 wishDirection )
		{
			if ( _flags.GetFlags( PlayerFlags.Sliding ) ) {
				return;
			}

			_runtime.SlideDirection = _camera.ResolveActionDirection( wishDirection );
			_flags.AddFlags( PlayerFlags.Sliding );
			_timer.Start();
		}

		public void Cancel()
		{
			if ( _timer.TimeLeft > 0.0 ) {
				_timer.Stop();
			}

			_flags.RemoveFlags( PlayerFlags.Sliding );
		}

		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			_timer.Timeout -= OnTimerTimeout;
			_timer.QueueFree();
			_isDisposed = true;
			GC.SuppressFinalize( this );
		}

		private void OnTimerTimeout()
		{
			_flags.RemoveFlags( PlayerFlags.Sliding );
		}
	}
}
