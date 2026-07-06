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
using Nomad.Game.Sdk.Entities;
using Nomad.Game.Sdk;
using Nomad.Game.Sdk.Items;
using Nomad.Game.Gameplay.Items;

namespace Nomad.Game.Gameplay.Combat
{
	internal abstract class WeaponInstance<TWeaponDefinition> : ItemInstance
		where TWeaponDefinition : WeaponDefinition
	{
		public WeaponDefinition WeaponDefinition => definition;
		protected readonly TWeaponDefinition definition;

		protected float dirtiness = 0.0f;

		public WeaponInstance( EntityId id, ItemInstanceId instanceId, IGameEventRegistryService eventFactory, TWeaponDefinition definition )
			: base( instanceId, definition, eventFactory )
		{
			this.definition = definition ?? throw new ArgumentNullException( nameof( definition ) );
		}
	};
};
