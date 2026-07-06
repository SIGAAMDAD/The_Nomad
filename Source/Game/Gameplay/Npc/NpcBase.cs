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

using Nomad.Core.Util;
using Nomad.Game.Gameplay.Entity;
using Nomad.Game.Sdk.Entities;
using Nomad.Game.Sdk.Npc;

namespace Nomad.Game.Gameplay.Npc
{
	internal abstract class NpcBase : EntityBase, INpcEntity
	{
		public abstract NpcType BotType { get; }

		protected NpcBase( EntityId entityId, InternString definitionId, InternString displayName, EntityType type, EntityFlags flags = EntityFlags.None )
			: base( entityId, definitionId, displayName, type, flags )
		{
		}
	};
};
