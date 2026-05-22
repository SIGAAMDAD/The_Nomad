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
using System.Numerics;
using Nomad.Core.Events;
using Nomad.EngineUtils;
using Nomad.Game.Domain.Data.Interactables;
using Nomad.Game.Domain.Interfaces.Interactables;
using Nomad.Game.Prefabs;

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

	internal sealed class CheckpointInstance : InteractableAggregate, ICheckpoint
	{
		public CheckpointStatus Status => _status;
		private CheckpointStatus _status = CheckpointStatus.Inactive;

		private readonly IDisposable _saveBegin;
		private readonly IDisposable _loadBegin;

		private readonly InteractableRoot _root;
		private readonly CheckpointDefinition _definition;
		private readonly CheckpointInstanceId _instanceId;

		public bool IsTemporary => false;

		public CheckpointInstance( CheckpointDefinition definition, CheckpointInstanceId instanceId, InteractableRoot prefab, IGameEventRegistryService eventFactory )
			: base( prefab )
		{
			_instanceId = instanceId;
			_definition = definition ?? throw new ArgumentNullException( nameof( definition ) );
			_root = prefab ?? throw new ArgumentNullException( nameof( prefab ) );
		}

		private void SetStatus( CheckpointStatus status )
		{
			CheckpointStatus previousStatus = status;
			_status = status;
		}
	};
};
