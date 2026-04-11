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

using Nomad.Core.Events;
using Nomad.Game.Domain.Data.Player;
using Nomad.Input.Events;

namespace Nomad.Game.Application.Gameplay.Player.Inventory {
	internal sealed class WeaponSlotService {
		private readonly WeaponSlot[] _slots = new WeaponSlot[(int)WeaponSlotIndex.Count];

		public WeaponSlotService( IGameEventRegistryService eventFactory ) {
			eventFactory
				.GetEvent<ButtonActionEventArgs>( $"SwitchToPrimary:{Input.Constants.Events.BUTTON_ACTION}", Input.Constants.Events.NAMESPACE )
				.Subscribe( OnSwitchToPrimaryWeaponTriggered );
			
			eventFactory
				.GetEvent<ButtonActionEventArgs>( $"SwitchToSecondary:{Input.Constants.Events.BUTTON_ACTION}", Input.Constants.Events.NAMESPACE )
				.Subscribe( OnSwitchToSecondaryWeaponTriggered );
		}

		private void OnSwitchToSecondaryWeaponTriggered( in ButtonActionEventArgs args ) {
		}

		private void OnSwitchToPrimaryWeaponTriggered( in ButtonActionEventArgs args ) {
		}
	};
};