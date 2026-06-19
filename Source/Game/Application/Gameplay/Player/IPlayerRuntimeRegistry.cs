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

using Nomad.Game.Sdk.Multiplayer;
using Nomad.Game.Sdk.Player.Inventory;
using Nomad.Game.Sdk.Player.State;

namespace Nomad.Game.Application.Gameplay.Player
{
	internal interface IPlayerRuntimeRegistry
	{
		bool TryGetInventory( PlayerId playerId, out IPlayerInventoryCoordinator? inventory );
		bool TryGetState( PlayerId playerId, out IPlayerStateReader? reader, out IPlayerStateWriter? writer );
		bool TryGetWeaponSlots( PlayerId playerId, out IWeaponSlotService? slots );
	};
};
