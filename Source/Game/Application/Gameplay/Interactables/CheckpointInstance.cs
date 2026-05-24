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
using Nomad.Game.Application.Gameplay.Entity;
using Nomad.Game.Domain.Data.Entities;
using Nomad.Game.Domain.Data.Interactables;
using Nomad.Game.Domain.Interfaces.Entities;
using Nomad.Game.Prefabs;
using Nomad.Game.Domain.Data.Items;
using Nomad.Game.Domain.Data.Multiplayer;

namespace Nomad.Game.Application.Gameplay.Interactables
{
	/*
	===================================================================================

	CheckpointInstance

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed class CheckpointInstance : InteractableAggregate, ICheckpointEntity
	{
		public CheckpointStatus Status => _status;
		private CheckpointStatus _status = CheckpointStatus.Inactive;

		public CheckpointInstanceId CheckpointId => _instanceId;

		public PlayerId CurrentPlayerId => _currentPlayerId;
		private PlayerId _currentPlayerId = PlayerId.Invalid;

		public bool IsTemporary => _definition.IsTemporary;
		public bool CanRest => _status == CheckpointStatus.Activated || ( IsTemporary && _status == CheckpointStatus.Inactive );

		public uint CheckpointRevision => _revision;
		private uint _revision = 0;

		private readonly CheckpointDefinition _definition;
		private readonly CheckpointInstanceId _instanceId;

		public CheckpointInstance( CheckpointDefinition definition, CheckpointInstanceId instanceId, CheckpointPrefab prefab, IGameEventRegistryService eventFactory )
			: this( definition, instanceId, prefab, EntityFlags.Interactable | EntityFlags.Persistent )
		{
		}

		public CheckpointInstance( CheckpointDefinition definition, CheckpointInstanceId instanceId )
			: this( definition, instanceId, null, EntityFlags.Interactable | EntityFlags.Transient )
		{
		}

		private CheckpointInstance( CheckpointDefinition definition, CheckpointInstanceId instanceId, CheckpointPrefab? prefab, EntityFlags flags )
			: base(
				prefab,
				new EntityId( instanceId.Value ),
				RequireDefinition( definition ).Id.Value,
				RequireDefinition( definition ).DisplayName,
				flags
			)
		{
			_instanceId = instanceId;
			_definition = definition;
		}

		private static CheckpointDefinition RequireDefinition( CheckpointDefinition definition )
		{
			return definition ?? throw new ArgumentNullException( nameof( definition ) );
		}

		private void SetStatus( CheckpointStatus status )
		{
			if ( _status == status ) {
				return;
			}

			_status = status;
			_revision++;
		}

		public bool TryActivateCheckpoint( PlayerId playerId )
		{
			playerId.ThrowIfInvalid( nameof( TryActivateCheckpoint ) );

			if ( IsTemporary ) {
				return false;
			}

			if ( _status != CheckpointStatus.Inactive ) {
				return true;
			}

			SetStatus( CheckpointStatus.Activated );

			return true;
		}

		public bool TryRest( PlayerId playerId )
		{
			playerId.ThrowIfInvalid( nameof( TryRest ) );

			if ( _currentPlayerId.IsValid && _currentPlayerId != playerId ) {
				return false;
			}

			if ( !CanRest ) {
				return false;
			}

			if ( _status == CheckpointStatus.Inactive ) {
				SetStatus( CheckpointStatus.Activated );
			}

			_currentPlayerId = playerId;
			SetStatus( CheckpointStatus.Current );

			return true;
		}

		public bool TryLeave( PlayerId playerId )
		{
			playerId.ThrowIfInvalid( nameof( TryLeave ) );

			if ( _status != CheckpointStatus.Current ) {
				return false;
			}

			if ( _currentPlayerId.IsValid && _currentPlayerId != playerId ) {
				return false;
			}

			_currentPlayerId = PlayerId.Invalid;
			SetStatus( CheckpointStatus.Activated );

			return true;
		}
	};
};
