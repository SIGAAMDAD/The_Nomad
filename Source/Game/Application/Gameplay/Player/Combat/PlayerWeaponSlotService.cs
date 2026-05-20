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
using Nomad.Core.Compatibility.Guards;
using Nomad.Core.Events;
using Nomad.Game.Domain.Data.Items;
using Nomad.Game.Domain.Data.Multiplayer;
using Nomad.Game.Domain.Data.Player;
using Nomad.Game.Domain.Events.Player;
using Nomad.Game.Domain.Interfaces.Player;

namespace Nomad.Game.Application.Gameplay.Player.Combat
{
	internal sealed class PlayerWeaponSlotService : IWeaponSlotService
	{
		private readonly WeaponDefinition[]? _slots = new WeaponDefinition[(int)WeaponSlotIndex.Count];

		public WeaponSlotIndex Current => _activeSlot;
		private WeaponSlotIndex _activeSlot = WeaponSlotIndex.LightPrimary;

		public IGameEvent<WeaponSlotChangedEventArgs> WeaponSlotChanged => _weaponSlotChanged;
		private readonly IGameEvent<WeaponSlotChangedEventArgs> _weaponSlotChanged = null;

		private bool _isDisposed = false;

		public PlayerWeaponSlotService( PlayerId playerId, IGameEventRegistryService eventFactory )
		{
			ArgumentGuard.ThrowIfNull( eventFactory, nameof( eventFactory ) );
			playerId.ThrowIfInvalid( nameof( PlayerWeaponSlotService ) );

			_weaponSlotChanged = eventFactory.GetEvent<WeaponSlotChangedEventArgs>(
				WeaponSlotChangedEventArgs.Name,
				WeaponSlotChangedEventArgs.NameSpace
			);
		}

		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			_weaponSlotChanged.Dispose();

			GC.SuppressFinalize( this );
			_isDisposed = true;
		}

		public bool IsOccupied( WeaponSlotIndex slot )
		{
			return _slots[(int)slot] != null;
		}

		public bool TryGetSlot( WeaponSlotIndex slot, out WeaponDefinition weapon )
		{
			int index = (int)slot;
			weapon = _slots[index];
			return weapon != null;
		}

		public bool TrySetSlot( WeaponSlotIndex slot, WeaponDefinition weapon )
		{
			throw new System.NotImplementedException();
		}
	};
};
