/*
===========================================================================
The Nomad MPLv2 Source Code
Copyright (C) 2025-2026 Noah Van Til

This Source Code Form is subject to the terms of the Mozilla Public
License, v2. If a copy of the MPL was not distributed with this
file, You can obtain one at https://mozilla.org/MPL/2.0/.

This software is provided "as is", without warranty of any kind,
express or implied including but not limited to the warranties
of merchantability, fitness for a particular purpose and noninfringement.
===========================================================================
*/

using Nomad.Core.Events;
using Nomad.Game.Sdk.Events.Player.JumpKit;
using Nomad.Game.Sdk.Multiplayer;
using Nomad.Game.Sdk.Player;

namespace Nomad.Game.Sdk.Player.JumpKit
{
    public interface IJumpKit
    {
        [Event(nameSpace: "Nomad.Game.Sdk.Events.Player.JumpKit", PayloadName = "PlayerJumpKitStatusChangedEventArgs")]
        [EventPayload("PlayerId", typeof(PlayerId), Order = 1)]
        [EventPayload("OldStatus", typeof(JumpKitStatus), Order = 2)]
        [EventPayload("NewStatus", typeof(JumpKitStatus), Order = 3)]
        IGameEvent<PlayerJumpKitStatusChangedEventArgs> StatusChanged { get; }

        JumpKitStatus Status { get; }

        float BurnoutAmount { get; }
        bool IsDashing { get; }
        bool IsBurnedOut { get; }
        bool CanDash { get; }

        IDashModule Module { get; }

        void SetModule(IDashModule module);
    }
}
