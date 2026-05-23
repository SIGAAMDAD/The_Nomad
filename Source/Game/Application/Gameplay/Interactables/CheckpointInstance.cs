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
using System.Collections.Generic;
using System.Numerics;
using Nomad.Core.Events;
using Nomad.Core.Util;
using Nomad.EngineUtils;
using Nomad.Game.Application.Gameplay.Entity;
using Nomad.Game.Domain.Data.Entities;
using Nomad.Game.Domain.Data.Interactables;
using Nomad.Game.Domain.Interfaces.Entities;
using Nomad.Game.Domain.Events.Interactables;
using Nomad.Game.Prefabs;
using Nomad.Game.Domain.Data.Items;

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

		public bool IsTemporary => false;
		public bool CanRest => _status == CheckpointStatus.Activated;

		public uint CheckpointRevision => _revision;
		private uint _revision = 0;

		private readonly IDisposable _saveBegin;
		private readonly IDisposable _loadBegin;

		private readonly CheckpointDefinition _definition;
		private readonly CheckpointInstanceId _instanceId;

		private ItemInstanceId _backpackStorageId = ItemInstanceId.Invalid;

		public CheckpointInstance( CheckpointDefinition definition, CheckpointInstanceId instanceId, CheckpointPrefab prefab, IGameEventRegistryService eventFactory )
			: base( prefab )
		{
			_instanceId = instanceId;
			_definition = definition ?? throw new ArgumentNullException( nameof( definition ) );
		}

		private void SetStatus( CheckpointStatus status )
		{
			CheckpointStatus previousStatus = status;
			_status = status;
			_revision++;
		}

		public bool ActivateCheckpoint( EntityId actorId )
		{
			SetStatus( CheckpointStatus.Activated );

			return true;
		}

		public bool Rest( EntityId actorId )
		{
			if ( !CanRest ) {
				return false;
			}

			return true;
		}
	};
};
