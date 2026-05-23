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
using Nomad.Game.Domain.Events.Interactables;
using Nomad.Game.Domain.Interfaces.Interactables;

namespace Nomad.Game.Application.Gameplay.Interactables
{
	/*
	===================================================================================

	CheckpointEventRouter

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed class CheckpointEventRouter : ICheckpointEventRouter
	{
		private readonly ICheckpointService _service;

		private bool _isDisposed = false;

		public IGameEvent<CheckpointRestRequestedEventArgs> RestRequested => _restRequested;
		private readonly IGameEvent<CheckpointRestRequestedEventArgs> _restRequested = null;

		/*
		===============
		CheckpointEventRouter
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="service"></param>
		/// <param name="eventFactory"></param>
		public CheckpointEventRouter( ICheckpointService service, IGameEventRegistryService eventFactory )
		{
			_service = service ?? throw new ArgumentNullException( nameof( service ) );

			_restRequested = eventFactory.GetEvent<CheckpointRestRequestedEventArgs>(
				CheckpointRestRequestedEventArgs.Name,
				CheckpointRestRequestedEventArgs.NameSpace
			);
		}

		/*
		===============
		Dispose
		===============
		*/
		/// <summary>
		///
		/// </summary>
		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			_restRequested.Dispose();

			GC.SuppressFinalize( this );
			_isDisposed = true;
		}
	};
};
