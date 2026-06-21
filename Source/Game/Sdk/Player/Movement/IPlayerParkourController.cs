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
using System;
using Nomad.Game.Sdk.Events.Player.Movement;

namespace Nomad.Game.Sdk.Player.Movement
{
    public interface IPlayerParkourController : IDisposable
    {
        PlayerParkourState State { get; }
        bool IsActive { get; }
        int CurrentAnchorIndex { get; }
        Vector2 LastMoveInput { get; }

        void RequestAttach();
        void RequestJump();
        void RequestDrop();
        void ForceDetach(bool preserveVelocity = true);
        void SetTraversalInput(Vector2 moveInput);

        void HandlePlayerCameraStatusChanged(in PlayerCameraStatusChangedEventArgs args);
        void RebuildRuntimeGraph();
    }
}
