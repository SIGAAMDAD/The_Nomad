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

using Nomad.Game.Domain.Data.Entities;
using Nomad.Game.Domain.Data.Interactables;

namespace Nomad.Game.Domain.Interfaces.Entities
{
	/*
	===================================================================================

	ICheckpointEntity

	===================================================================================
	*/
	/// <summary>
	/// Optional capability for checkpoint/rest entities.
	/// </summary>

	public interface ICheckpointEntity : IInteractableEntity
	{
		CheckpointInstanceId CheckpointId { get; }

		CheckpointStatus Status { get; }

		bool CanRest { get; }

		bool IsTemporary { get; }

		uint CheckpointRevision { get; }

		bool ActivateCheckpoint( EntityId actorId );
		bool Rest( EntityId actorId );
	};
};
