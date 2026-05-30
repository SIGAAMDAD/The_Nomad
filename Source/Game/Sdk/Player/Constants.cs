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

namespace Nomad.Game.Sdk.Player
{
    public static class Constants
    {
        public const float MOVEMENT_ACCELERATION = 800.0f;
        public const float MOVEMENT_FRICTION = 1000.0f;
        public const float MOVEMENT_MAX_SPEED = 440.0f;

        public const float SLIDE_DURATION = 1.0f;

        public const int MAX_HOT_SLOTS = 6;

        public static readonly Guid LOCAL_GUID = Guid.NewGuid();
    }
}
