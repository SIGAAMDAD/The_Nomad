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
using Nomad.Game.Sdk.Items;

namespace Nomad.Game.Application.Gameplay.Items
{
	internal sealed class ConsumableInstance : ItemInstance, IItemInstance
	{
		private readonly IConsumableBehavior _behavior;

		public ConsumableInstance( ItemInstanceId instanceId, ConsumableDefinition definition, IConsumableBehavior behavior, IGameEventRegistryService eventFactory )
			: base( instanceId, definition, eventFactory )
		{
			_behavior = behavior ?? throw new ArgumentNullException( nameof( behavior ) );
		}
	};
};
