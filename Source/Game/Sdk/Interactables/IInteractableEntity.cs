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
using Nomad.Core.Util;
using Nomad.Game.Sdk.Multiplayer;
using Nomad.Game.Sdk.Events.Interactables;
using Nomad.Game.Sdk.Entities;

namespace Nomad.Game.Sdk.Interactables
{
    /// <summary>
    /// <para>Optional capability for entities that can be interacted with.</para>
    ///
    /// <para>This covers pickup, talk, inspect, loot, open, close, rest, read, consume,
    /// equip, use, activate, and deactivate style interactions.</para>
    /// </summary>
    public interface IInteractableEntity : IEntityBase
    {
        bool IsInteractable { get; }

        InternString InteractionPrompt { get; }

        IReadOnlyList<EntityInteractionKind> InteractionKinds { get; }

        uint InteractionRevision { get; }

        PlayerInteractionStatus PlayerStatus { get; }

        [Event(nameSpace: "Nomad.Game.Sdk.Events.Interactables", PayloadName = "PlayerInteractionStatusChangedEventArgs")]
        [EventPayload("InteractorId", typeof(PlayerId), Order = 1)]
        [EventPayload("OldStatus", typeof(PlayerInteractionStatus), Order = 2)]
        [EventPayload("NewStatus", typeof(PlayerInteractionStatus), Order = 3)]
        IGameEvent<PlayerInteractionStatusChangedEventArgs> StatusChanged { get; }

        bool CanInteract(in EntityInteractionContext context);
        EntityInteractionResult Interact(in EntityInteractionContext context);
    }
}
