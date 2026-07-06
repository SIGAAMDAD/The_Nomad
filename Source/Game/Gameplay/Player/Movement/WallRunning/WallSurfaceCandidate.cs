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

namespace Nomad.Game.Gameplay.Player.Movement.WallRunning
{
	public readonly struct WallSurfaceCandidate
	{
		public readonly Godot.Node3D Collider;
		public readonly Godot.Rid ColliderRid;

		public readonly Vector3 Point;
		public readonly Vector3 Normal;

		public readonly float Verticality;
		public readonly float Distance;

		public readonly bool IsRunnable;
		public readonly bool AllowsHorizontalRun;
		public readonly bool AllowsRunUp;
		public readonly bool AllowsRunDown;
		public readonly int Priority;

		public WallSurfaceCandidate(
			Godot.Node3D collider,
			Godot.Rid colliderRid,
			Vector3 point,
			Vector3 normal,
			float verticality,
			float distance,
			bool isRunnable,
			bool allowsHorizontalRun,
			bool allowsRunUp,
			bool allowsRunDown,
			int priority
		)
		{
			Collider = collider;
			ColliderRid = colliderRid;
			Point = point;
			Normal = normal;
			Verticality = verticality;
			Distance = distance;
			IsRunnable = isRunnable;
			AllowsHorizontalRun = allowsHorizontalRun;
			AllowsRunUp = allowsRunUp;
			AllowsRunDown = allowsRunDown;
			Priority = priority;
		}
	};
};
