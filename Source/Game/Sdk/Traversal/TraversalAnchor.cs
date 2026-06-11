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

using System.Numerics;

namespace Nomad.Game.Sdk.Traversal
{
    public readonly struct TraversalAnchor
    {
        public readonly Vector3 Position;
        public readonly Vector3 Normal;
        public readonly Vector3 Up;

        public readonly TraversalAnchorFlags Flags;
        public readonly ushort SurfaceId;

        // Adjancency range into a flat edge array
        public readonly int FirstEdge;
        public readonly ushort EdgeCount;

        public Vector3 Right
        {
            get
            {
                var right = Vector3.Cross(Up, Normal);
                return right.LengthSquared() < 0.0001f ? Vector3.UnitX : Vector3.Normalize(right);
            }
        }

        public TraversalAnchor(
            Vector3 position,
            Vector3 normal,
            Vector3 up,
            TraversalAnchorFlags flags,
            ushort surfaceId,
            int firstEdge,
            ushort edgeCount
        )
        {
            Position = position;
            Normal = Vector3.Normalize(normal);
            Up = Vector3.Normalize(up);
            Flags = flags;
            SurfaceId = surfaceId;
            FirstEdge = firstEdge;
            EdgeCount = edgeCount;
        }
    }
}
