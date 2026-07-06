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

using Nomad.Core.Events;
using Nomad.Game.Sdk.Events.Player.JumpKit;
using Nomad.Game.Sdk.Multiplayer;
using Nomad.Game.Sdk.Player.JumpKit;
using System;

namespace Nomad.Game.Gameplay.Player.JumpKit
{
	/*
	===================================================================================

	PlayerJumpKitEventPublisher

	===================================================================================
	*/
	/// <summary>
	/// Caches the single jump-kit status channel and owns payload construction.
	/// The channel is registry-owned; this type never disposes the underlying event.
	/// </summary>

	internal sealed class PlayerJumpKitEventPublisher : IDisposable
	{
		public IGameEvent<PlayerJumpKitStatusChangedEventArgs> StatusChanged => _statusChanged;

		private readonly IGameEvent<PlayerJumpKitStatusChangedEventArgs> _statusChanged;
		private bool _isDisposed;

		public PlayerJumpKitEventPublisher( IGameEventRegistryService eventFactory )
		{
			_statusChanged = eventFactory.GetEvent<PlayerJumpKitStatusChangedEventArgs>(
				PlayerJumpKitStatusChangedEventArgs.Name,
				PlayerJumpKitStatusChangedEventArgs.NameSpace,
				EventFlags.NoLock
			);
		}

		public void PublishStatusChanged(
			PlayerId playerId,
			in JumpKitStatus oldStatus,
			in JumpKitStatus newStatus
		)
		{
			_statusChanged.Publish( new PlayerJumpKitStatusChangedEventArgs( playerId, oldStatus, newStatus ) );
		}

		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			// Registry-owned channel. Disposing here would clear shared subscribers.
			_isDisposed = true;
			GC.SuppressFinalize( this );
		}
	}
}
