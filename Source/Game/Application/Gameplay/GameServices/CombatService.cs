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

using Nomad.Game.Sdk.Gameplay;
using Nomad.Game.Sdk.Events.Gameplay;
using Nomad.Game.Sdk.Items;
using Nomad.Core.Events;

namespace Nomad.Game.Application.Gameplay.GameServices
{
	internal sealed class CombatService : ICombatService
	{
		private readonly IItemCatalog _firearmCatalog;

		public IGameEvent<UseWeaponResultEventArgs> UseWeaponResult => _useWeaponResult;

		public IGameEvent<UseWeaponFirearmRequestEventArgs> UseWeaponFirearmRequest {
			get {
				throw new System.NotImplementedException();
			}
		}

		private readonly IGameEvent<UseWeaponResultEventArgs> _useWeaponResult = null;

		public CombatService( IItemCatalog database )
		{
			_firearmCatalog = database;
		}

		public void Dispose()
		{
		}
	};
};
