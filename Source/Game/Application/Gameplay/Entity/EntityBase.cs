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
using Nomad.Game.Domain.Events.Entity;

namespace Nomad.Game.Application.Gameplay.Entity
{
	internal abstract class EntityBase
	{
		[Event( nameSpace: "Nomad.Game.Domain.Events.Entity" )]
		public IGameEvent<EntityDieEventArgs> EntityDie => _entityDie;
		private readonly IGameEvent<EntityDieEventArgs> _entityDie = default;

		[Event( nameSpace: "Nomad.Game.Domain.Events.Entity" )]
		public IGameEvent<EntityTakeDamageEventArgs> EntityTakeDamage => _entityTakeDamage;
		private readonly IGameEvent<EntityTakeDamageEventArgs> _entityTakeDamage = default;

		public EntityBase( IGameEventRegistryService eventFactory )
		{
			_entityDie = eventFactory
				.GetEvent<EntityDieEventArgs>(
					EntityDieEventArgs.Name,
					EntityDieEventArgs.NameSpace
				);

			_entityTakeDamage = eventFactory
				.GetEvent<EntityTakeDamageEventArgs>(
					EntityTakeDamageEventArgs.Name,
					EntityTakeDamageEventArgs.NameSpace
				);
		}
	};
};
