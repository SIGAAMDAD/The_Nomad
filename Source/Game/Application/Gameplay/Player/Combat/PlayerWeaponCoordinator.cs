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
using Nomad.Core.Events;
using Nomad.Game.Sdk.Multiplayer;
using Nomad.Game.Sdk;
using Nomad.Game.Sdk.Inventory;
using Nomad.Game.Application.Gameplay.Player;

namespace Nomad.Game.Application.Gameplay.Player.Combat
{
	internal sealed class PlayerWeaponCoordinator : IDisposable
	{
		private readonly PlayerWeaponController _controller;

		private bool _isDisposed = false;

		public PlayerWeaponCoordinator( PlayerId playerId, IInventoryCoordinator inventoryCoordinator, IGameEventRegistryService eventFactory )
		{
			playerId.ThrowIfInvalid( nameof( PlayerWeaponCoordinator ) );

			_controller = new PlayerWeaponController( eventFactory );
		}

		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			_controller.Dispose();

			GC.SuppressFinalize( this );
			_isDisposed = true;
		}
	};
};
