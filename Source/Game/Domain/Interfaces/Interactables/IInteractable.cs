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

using Nomad.Core.Events;
using Nomad.Game.Domain.Data.Interactables;
using Nomad.Game.Domain.Events.Interactables;
using Nomad.Game.Domain.Data.Multiplayer;
using System;

namespace Nomad.Game.Domain.Interfaces.Interactables
{
	/// <summary>
	///
	/// </summary>
	public interface IInteractable
	{
		/// <summary>
		///
		/// </summary>
		PlayerInteractionStatus PlayerStatus { get; }

		/// <summary>
		///
		/// </summary>
		[Event( nameSpace: "Nomad.Game.Domain.Events.Interactables", PayloadName = "PlayerInteractionStatusChangedEventArgs" )]
		[EventPayload( "InteractorId", typeof( PlayerId ), Order = 1 )]
		[EventPayload( "OldStatus", typeof( PlayerInteractionStatus ), Order = 2 )]
		[EventPayload( "NewStatus", typeof( PlayerInteractionStatus ), Order = 3 )]
		IGameEvent<PlayerInteractionStatusChangedEventArgs> StatusChanged { get; }
	};
};
