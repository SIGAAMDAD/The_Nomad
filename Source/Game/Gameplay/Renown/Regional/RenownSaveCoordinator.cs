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
using Nomad.Game.Sdk.World;
using Nomad.Save.Services;

namespace Nomad.Game.Gameplay.Renown.Regional
{
	internal sealed class RenownSaveCoordinator
	{
		private readonly object _lock = new();
		private readonly RenownTrackerService _tracker;

		public RenownSaveCoordinator( RenownTrackerService tracker, IGameEventRegistryService eventFactory )
		{
			_tracker = tracker ?? throw new ArgumentNullException( nameof( tracker ) );

			eventFactory
				.GetEvent<SaveBeginEventArgs>(
					SaveBeginEventArgs.Name,
					SaveBeginEventArgs.NameSpace
				)
				.Subscribe( OnSaveBegin );
		}

		private void OnSaveBegin( in SaveBeginEventArgs args )
		{
			lock ( _lock ) {
				var writer = args.Writer.AddSection( "RenownData" );
			}
		}
	};
};
