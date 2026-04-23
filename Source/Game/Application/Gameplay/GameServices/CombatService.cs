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

using Nomad.Game.Domain.Data.Gameplay;
using Nomad.Game.Domain.Events.Gameplay;
using Nomad.Game.Domain.Interfaces.Gameplay;
using Nomad.Game.Infrastructure.Gameplay.Items;
using Nomad.Game.Domain.Interfaces.Items;
using Nomad.Game.Domain.Data.Items;

namespace Nomad.Game.Application.Gameplay.GameServices {
	internal sealed class CombatService : ICombatService {
		private readonly IItemCatalog _firearmCatalog;

		public CombatService( IItemCatalog database ) {
			_firearmCatalog = database;
		}

		public DamageResult UseWeapon( in UseWeaponRequestEventArgs args ) {
			var weapon = _firearmCatalog.Get<FirearmDefinition>( args.WeaponId );
			if ( weapon == null ) {
				return new DamageResult(
					null,
					0.0f,
					false
				);
			}

			return new DamageResult();
		}
	};
};