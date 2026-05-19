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
using Nomad.Core.Abstractions;
using Nomad.Core.Compatibility.Guards;
using Nomad.Core.Events;
using Nomad.Core.Util;
using Nomad.Game.Domain.Data.Entities;
using Nomad.Game.Domain.Events.Entity;
using Nomad.Game.Domain.Interfaces.Entity;

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

	internal abstract class EntityBase : IEntityBase
	{
		public IGameEvent<EntityDieEventArgs> EntityDie => entityDie;
		protected readonly IGameEvent<EntityDieEventArgs> entityDie = default;

		public IGameEvent<EntityTakeDamageEventArgs> EntityTakeDamage => takeDamage;
		protected readonly IGameEvent<EntityTakeDamageEventArgs> takeDamage = default;

		public IGameEvent<EntityApplyStatusEffectEventArgs> EntityApplyStatusEffect => applyStatusEffect;
		protected readonly IGameEvent<EntityApplyStatusEffectEventArgs> applyStatusEffect = default;

		public EntityId Id => id;
		protected readonly EntityId id;

		public EntityType Type { get; }

		private bool _isDisposed = false;

		/*
		===============
		EntityBase
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="id"></param>
		/// <param name="eventFactory"></param>
		public EntityBase( EntityId id, IGameEventRegistryService eventFactory )
		{
			ArgumentGuard.ThrowIfNull( eventFactory, nameof( eventFactory ) );

			this.id = id;

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

		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			entityDie?.Dispose();
			takeDamage?.Dispose();
			applyStatusEffect?.Dispose();

			Dispose( true );
			GC.SuppressFinalize( this );
			_isDisposed = true;
		}

		protected virtual void Dispose( bool disposing )
		{
		}

		public bool Equals( IEntityBase? other )
		{
			return id == other.Id;
		}
	};
};
