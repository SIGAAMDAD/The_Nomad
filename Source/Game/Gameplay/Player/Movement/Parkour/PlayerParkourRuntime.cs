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

using Godot;
using Nomad.Game.Gameplay.Traversal;
using Nomad.Game.Sdk.Player.Movement;

namespace Nomad.Game.Gameplay.Player.Movement.Parkour
{
	internal sealed class PlayerParkourRuntime
	{
		public PlayerParkourState State = PlayerParkourState.Inactive;
		public TraversalAnchorHandle CurrentAnchor = TraversalAnchorHandle.Invalid;
		public int ActiveEdgeIndex = -1;

		public Vector3 EdgeStart;
		public Vector3 EdgeEnd;
		public Vector3 EdgeStartNormal;
		public Vector3 EdgeEndNormal;
		public float EdgeElapsed;
		public float EdgeDuration;

		public void ClearAnchor()
		{
			CurrentAnchor = TraversalAnchorHandle.Invalid;
			ActiveEdgeIndex = -1;
			EdgeElapsed = 0.0f;
		}
	};
};
