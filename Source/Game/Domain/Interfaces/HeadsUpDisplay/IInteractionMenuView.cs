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
using Nomad.Core.Util;
using Nomad.Game.Domain.Events.Interactables;

namespace Nomad.Game.Domain.Interfaces.HeadsUpDisplay
{
	public interface IInteractionMenuView : IHudComponentView
	{
		[Event( nameSpace: "Nomad.Game.Domain.Events.Interactables", PayloadName = "InteractionMenuOptionSelectedEventArgs" )]
		[EventPayload( "Option", typeof( int ), Order = 1 )]
		IGameEvent<InteractionMenuOptionSelectedEventArgs> OptionSelected { get; }

		void AddOption( InternString prompt, EventCallback<InteractionMenuOptionSelectedEventArgs> callback );
	};
};
