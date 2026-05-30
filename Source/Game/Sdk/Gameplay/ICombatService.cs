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
using System.Numerics;
using Nomad.Core.Events;
using Nomad.Game.Sdk.Entities;
using Nomad.Game.Sdk.Items;
using Nomad.Game.Sdk.Events.Gameplay;

namespace Nomad.Game.Sdk.Gameplay
{
    public interface ICombatService : IDisposable
    {
        [Event(nameSpace: "Nomad.Game.Sdk.Events.Gameplay")]
        [EventPayload("WeaponId", typeof(ItemInstanceId), Order = 1)]
        [EventPayload("FromPosition", typeof(Vector2), Order = 2)]
        [EventPayload("AngleRadians", typeof(float), Order = 3)]
        IGameEvent<UseWeaponFirearmRequestEventArgs> UseWeaponFirearmRequest { get; }

        [Event(nameSpace: "Nomad.Game.Sdk.Events.Gameplay")]
        [EventPayload("HitEntityId", typeof(EntityId), Order = 1)]
        [EventPayload("DamageAmount", typeof(float), Order = 2)]
        IGameEvent<UseWeaponResultEventArgs> UseWeaponResult { get; }
    }
}
