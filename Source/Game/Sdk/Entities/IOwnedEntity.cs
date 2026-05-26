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

namespace Nomad.Game.Sdk.Entities
{
	/*
	===================================================================================

	IOwnedEntity

	===================================================================================
	*/
	/// <summary>
	/// Optional capability for entities owned by another entity.
	///
	/// Examples:
	/// - item owned by player
	/// - weapon owned by NPC
	/// - projectile owned by shooter
	/// - container contents owned by container
	/// </summary>

	public interface IOwnedEntity : IEntityBase
	{
		EntityId OwnerEntityId { get; }

		EntityOwnershipKind OwnershipKind { get; }

		uint OwnershipRevision { get; }

		bool HasOwner { get; }

		void SetOwner( EntityId ownerEntityId, EntityOwnershipKind ownershipKind );
		void ClearOwner();
	}
}
