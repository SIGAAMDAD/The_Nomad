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
using Nomad.Game.Domain.Data.Gameplay;
using Nomad.Game.Domain.Events.Gameplay;

namespace Nomad.Game.Domain.Interfaces.Gameplay
{
	public interface IWorldBootstrapper
	{
		[Event( nameSpace: "Nomad.Game.Domain.Events.Gameplay", PayloadName = "WorldBootstrapFailureEventArgs" )]
		[EventPayload( "RequestId", typeof( Guid ), Order = 1 )]
		[EventPayload( "Mode", typeof( WorldBootstrapMode ), Order = 2 )]
		[EventPayload( "WorldId", typeof( string ), Order = 3 )]
		[EventPayload( "Reason", typeof( WorldBootstrapFailureReason ), Order = 4 )]
		[EventPayload( "Detail", typeof( string ), Order = 5 )]
		IGameEvent<WorldBootstrapFailureEventArgs> BootstrapFailure { get; }

		[Event( nameSpace: "Nomad.Game.Domain.Events.Gameplay", PayloadName = "WorldBootstrapSucceededEventArgs" )]
		[EventPayload( "RequestId", typeof( Guid ), Order = 1 )]
		[EventPayload( "Mode", typeof( WorldBootstrapMode ), Order = 2 )]
		[EventPayload( "WorldId", typeof( string ), Order = 3 )]
		[EventPayload( "WorldInstanceId", typeof( Guid ), Order = 4 )]
		[EventPayload( "LobbyId", typeof( Guid? ), Order = 5 )]
		IGameEvent<WorldBootstrapSucceededEventArgs> BootstrapSucceeded { get; }

		[Event( nameSpace: "Nomad.Game.Domain.Events.Gameplay", PayloadName = "WorldBootstrapRequestEventArgs" )]
		[EventPayload( "RequestId", typeof( Guid ), Order = 1 )]
		[EventPayload( "Mode", typeof( WorldBootstrapMode ), Order = 2 )]
		[EventPayload( "WorldId", typeof( string ), Order = 3 )]
		[EventPayload( "Difficulty", typeof( DifficultyPreset ), Order = 4 )]
		[EventPayload( "Lobbyid", typeof( Guid? ), Order = 5 )]
		IGameEvent<WorldBootstrapRequestEventArgs> BootstrapRequest { get; }
	};
};
