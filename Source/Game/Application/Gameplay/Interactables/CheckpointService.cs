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

using System.Collections.Generic;
using Nomad.Core.Events;
using Nomad.Game.Domain.Data.Interactables;
using Nomad.Game.Domain.Interfaces.Interactables;

namespace Nomad.Game.Application.Gameplay.Interactables
{
	/*
	===================================================================================

	CheckpointService

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed class CheckpointService : ICheckpointService
	{
		/// <summary>
		/// Represents all the meliora in the game.
		/// </summary>
		private readonly Dictionary<CheckpointInstanceId, CheckpointInstance> _permanent = new();

		/// <summary>
		/// Represents the temporary firelink checkpoint.
		/// </summary>
		private readonly CheckpointInstance _temporary;

		public CheckpointService( IGameEventRegistryService eventFactory )
		{
		}

		public void Dispose()
		{
		}
	};
};
