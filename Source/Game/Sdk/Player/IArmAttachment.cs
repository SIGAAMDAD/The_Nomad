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
using Nomad.Game.Sdk.Events.Player;

namespace Nomad.Game.Sdk.Player
{
    /// <summary>
    /// 
    /// </summary>
    public interface IArmAttachment
    {
        [Event(nameSpace: "Nomad.Game.Sdk.Events.Player", PayloadName = "ArmAttachmentEquippedEventArgs")]
        IGameEvent<ArmAttachmentEquippedEventArgs> Equipped { get; }

        [Event(nameSpace: "Nomad.Game.Sdk.Events.Player", PayloadName = "ArmAttachmentUsedEventArgs")]
        IGameEvent<ArmAttachmentUsedEventArgs> Used { get; }

        void Use();
    }
}
