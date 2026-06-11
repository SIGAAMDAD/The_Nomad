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

namespace Nomad.Game.Sdk.Traversal
{
    public readonly struct TraversalEdge
    {
        public readonly int To;
        public readonly TraversalMoveType MoveType;
        public readonly TraversalEdgeFlags Flags;

        public readonly float Cost;
        public readonly float Duration;

        public readonly int AnimationId;

        public TraversalEdge(
            int to,
            TraversalMoveType moveType,
            TraversalEdgeFlags flags,
            float cost,
            float duration,
            int animationId
        )
        {
            To = to;
            MoveType = moveType;
            Flags = flags;
            Cost = cost;
            Duration = duration <= 0.0f ? 0.25f : duration;
            AnimationId = animationId;
        }
    }
}
