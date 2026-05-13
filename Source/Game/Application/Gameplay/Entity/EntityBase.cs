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
using Nomad.Core.Util;
using Nomad.Game.Domain.Data.Entities;
using Nomad.Game.Domain.Events.Entity;

namespace Nomad.Game.Application.Gameplay.Entity
{
	/*
	===================================================================================

	EntityBase

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal abstract class EntityBase
	{
		[Event( nameSpace: "Nomad.Game.Domain.Events.Entity" )]
		[EventPayload( "AttackerId", typeof( Guid ), Order = 1 )]
		public IGameEvent<EntityDieEventArgs> EntityDie => entityDie;
		protected readonly IGameEvent<EntityDieEventArgs> entityDie = default;

		[Event( nameSpace: "Nomad.Game.Domain.Events.Entity" )]
		[EventPayload( "AttackerId", typeof( Guid ), Order = 1 )]
		[EventPayload( "Source", typeof( DamageSource ), Order = 2 )]
		[EventPayload( "Amount", typeof( float ), Order = 3 )]
		public IGameEvent<EntityTakeDamageEventArgs> EntityTakeDamage => takeDamage;
		protected readonly IGameEvent<EntityTakeDamageEventArgs> takeDamage = default;

		[Event( nameSpace: "Nomad.Game.Domain.Events.Entity" )]
		[EventPayload( "EffectId", typeof( InternString ) )]
		public IGameEvent<EntityApplyStatusEffectEventArgs> EntityApplyStatusEffect => applyStatusEffect;
		protected readonly IGameEvent<EntityApplyStatusEffectEventArgs> applyStatusEffect = default;

		public Guid Guid => guid;
		protected readonly Guid guid;

		public EntityBase( Guid guid, IGameEventRegistryService eventFactory )
		{
			ArgumentGuard.ThrowIfNull( eventFactory );

			this.guid = guid;

			entityDie = eventFactory
				.GetEvent<EntityDieEventArgs>(
					EntityDieEventArgs.Name,
					EntityDieEventArgs.NameSpace
				);

			takeDamage = eventFactory
				.GetEvent<EntityTakeDamageEventArgs>(
					EntityTakeDamageEventArgs.Name,
					EntityTakeDamageEventArgs.NameSpace
				);

			applyStatusEffect = eventFactory
				.GetEvent<EntityApplyStatusEffectEventArgs>(
					EntityApplyStatusEffectEventArgs.Name,
					EntityApplyStatusEffectEventArgs.NameSpace
				);
		}
	};
};
