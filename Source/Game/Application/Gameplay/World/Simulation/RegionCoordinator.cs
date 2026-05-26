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
using System.Collections.Immutable;
using Nomad.Core.Events;
using Nomad.Core.Util;
using Nomad.Game.Sdk.World;
using Nomad.Game.Sdk.Events.World;
using Nomad.Save.Services;

namespace Nomad.Game.Application.Gameplay.World
{
	/*
	===================================================================================
	
	RegionCoordinator
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	internal sealed class RegionCoordinator
	{
		[Event( nameSpace: "Nomad.Game.Sdk.Events.World", PayloadName = "RegionPlayerStatusChangedEventArgs" )]
		[EventPayload( "Id", typeof( Guid ), Order = 1 )]
		[EventPayload( "NewStatus", typeof( RegionPlayerStatus ), Order = 2 )]
		public IGameEvent<RegionPlayerStatusChangedEventArgs> PlayerStatusChanged => _playerStatusChanged;
		private readonly IGameEvent<RegionPlayerStatusChangedEventArgs> _playerStatusChanged = default;

		private readonly ImmutableDictionary<InternString, RegionDefinition> _regions;

		public RegionCoordinator( IGameEventRegistryService eventFactory )
		{
			eventFactory
				.GetEvent<SaveBeginEventArgs>( SaveBeginEventArgs.Name, SaveBeginEventArgs.NameSpace )
				.Subscribe( OnSaveBegin );
		}

		private void OnSaveBegin( in SaveBeginEventArgs args )
		{
			lock ( this ) {
				var writer = args.Writer.AddSection( "RegionData" );
			}
		}
	};
};
