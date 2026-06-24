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
using Nomad.Core.Events;
using Nomad.Game.Sdk.Events.Player.JumpKit;
using Nomad.Game.Sdk.Multiplayer;
using Nomad.Game.Sdk.Player.JumpKit;
using Nomad.Game.Sdk.Player.State;
using NumericsVector2 = System.Numerics.Vector2;

namespace Nomad.Game.Application.Gameplay.Player.Movement
{
	internal sealed class PlayerDashMovementBridge : IDisposable
	{
		private const PlayerJumpKitFlags DASH_END_FLAGS = PlayerJumpKitFlags.DashEnded | PlayerJumpKitFlags.BurnoutEntered;

		private readonly PlayerId _playerId;
		private readonly PlayerMovementRuntime _runtime;
		private readonly PlayerCameraController _camera;
		private readonly IPlayerFlagService _flags;
		private readonly IDisposable _jumpKitStatusChanged;

		private NumericsVector2 _moveInput;
		private bool _isDisposed;

		public PlayerDashMovementBridge(
			PlayerId playerId,
			PlayerMovementRuntime runtime,
			PlayerCameraController camera,
			IPlayerFlagService flags,
			IGameEventRegistryService eventFactory
		)
		{
			_playerId = playerId;
			_runtime = runtime;
			_camera = camera;
			_flags = flags;

			_jumpKitStatusChanged = eventFactory
				.GetEvent<PlayerJumpKitStatusChangedEventArgs>(
					PlayerJumpKitStatusChangedEventArgs.Name,
					PlayerJumpKitStatusChangedEventArgs.NameSpace,
					EventFlags.NoLock
				)
				.Subscribe( OnJumpKitStatusChanged );
		}

		public void UpdateInput( NumericsVector2 moveInput )
		{
			_moveInput = moveInput;
		}

		public void Cancel()
		{
			_flags.RemoveFlags( PlayerFlags.Dashing );
		}

		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			_jumpKitStatusChanged.Dispose();
			_isDisposed = true;
			GC.SuppressFinalize( this );
		}

		private void OnJumpKitStatusChanged( in PlayerJumpKitStatusChangedEventArgs args )
		{
			if ( !args.PlayerId.Equals( _playerId ) ) {
				return;
			}

			PlayerJumpKitFlags jumpKitFlags = args.NewStatus.Flags;

			if ( (jumpKitFlags & PlayerJumpKitFlags.DashStarted) != 0 ) {
				Vector3 wishDirection = _camera.GetWishDirection( _moveInput );
				_runtime.DashDirection = _camera.ResolveActionDirection( wishDirection );
				_flags.AddFlags( PlayerFlags.Dashing );
				return;
			}

			if ( (jumpKitFlags & DASH_END_FLAGS) != 0 ) {
				_flags.RemoveFlags( PlayerFlags.Dashing );
			}
		}
	}
}
