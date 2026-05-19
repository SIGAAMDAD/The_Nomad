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
using Nomad.Core.Util;
using Nomad.Game.Domain.Data.Entities;
using Nomad.Game.Domain.Events.Entity;

namespace Nomad.Game.Domain.Interfaces.Entity
{
	/// <summary>
	///
	/// </summary>
	public interface IEntityBase : IDisposable, IEquatable<IEntityBase>
	{
		/// <summary>
		///
		/// </summary>
		EntityId Id { get; }

		EntityType Type { get; }

		[Event( nameSpace: "Nomad.Game.Domain.Events.Entity" )]
		[EventPayload( "AttackerId", typeof( EntityId ), Order = 1 )]
		IGameEvent<EntityDieEventArgs> EntityDie { get; }

		[Event( nameSpace: "Nomad.Game.Domain.Events.Entity" )]
		[EventPayload( "AttackerId", typeof( EntityId ), Order = 1 )]
		[EventPayload( "Source", typeof( DamageSource ), Order = 2 )]
		[EventPayload( "Amount", typeof( float ), Order = 3 )]
		IGameEvent<EntityTakeDamageEventArgs> EntityTakeDamage { get; }

		[Event( nameSpace: "Nomad.Game.Domain.Events.Entity" )]
		[EventPayload( "EffectId", typeof( InternString ) )]
		IGameEvent<EntityApplyStatusEffectEventArgs> EntityApplyStatusEffect { get; }
	};
};
