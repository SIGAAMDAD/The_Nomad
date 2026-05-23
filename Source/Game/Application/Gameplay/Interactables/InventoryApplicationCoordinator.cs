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
using Nomad.Game.Application.Gameplay.Interactables;
using Nomad.Game.Domain.Interfaces.Interactables;

namespace Nomad.Game.Application.Gameplay.Inventory
{
	internal sealed class InteractableApplicationCoordinator : IDisposable
	{
		private readonly ICheckpointEventRouter _checkpointEventRouter;
		private readonly ICheckpointService _checkpointService;

		private bool _isDisposed;

		public InteractableApplicationCoordinator( IGameEventRegistryService eventFactory )
		{
			_checkpointService = new CheckpointService( eventFactory );
			_checkpointEventRouter = new CheckpointEventRouter( _checkpointService, eventFactory );
		}

		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			GC.SuppressFinalize( this );
			_isDisposed = true;
		}
	};
};
