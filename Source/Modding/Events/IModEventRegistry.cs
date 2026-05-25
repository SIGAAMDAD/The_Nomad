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

namespace Nomad.Modding.Events
{
	public interface IModEventRegistry
	{
		ISubscriptionHandle On<TArgs>(
            ModEventId<TArgs> eventId,
            EventCallback<TArgs> callback
        )
            where TArgs : struct;

        ISubscriptionHandle OnGame<TArgs>(
            GameEventId<TArgs> eventId,
            EventCallback<TArgs> callback
        )
            where TArgs : struct;

        void Publish<TArgs>(
            ModEventId<TArgs> eventId,
            in TArgs args
        )
            where TArgs : struct;
	}
}
