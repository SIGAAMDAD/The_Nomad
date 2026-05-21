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
using Nomad.Game.Domain.Data.Combat;
using Nomad.Game.Domain.Data.Items;
using Nomad.Game.Domain.Events.Combat;
using Nomad.Game.Domain.Interfaces.Entity;
using Nomad.Game.Domain.Interfaces.Player;
using Nomad.Game.Domain.Interfaces.Player.Inventory;

namespace Nomad.Game.Domain.Interfaces.Combat
{
	public interface IFirearmInstance : IEntityBase
	{
		FirearmMagazine AmmoSnapshot { get; }

		FirearmResolvedStats Stats { get; }
		FirearmDefinition Definition { get; }

		[Event( nameSpace: "Nomad.Game.Domain.Events.Combat" )]
		IGameEvent<FirearmJammedEventArgs> FirearmJammed { get; }

		[Event( nameSpace: "Nomad.Game.Domain.Events.Combat" )]
		IGameEvent<FirearmReloadedEventArgs> FirearmReloaded { get; }

		bool TryReload( IStorageUnit inventory );

		bool TryStartUse();
		bool TryEndUse();

		bool TryAddMod( FirearmModSlot slot, FirearmModDefinition mod );
		bool TryRemoveMod( FirearmModSlot slot );
	};
};
