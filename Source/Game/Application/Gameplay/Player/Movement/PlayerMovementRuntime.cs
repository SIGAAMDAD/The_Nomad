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
using Nomad.Game.Sdk.Player.Movement;

namespace Nomad.Game.Application.Gameplay.Player.Movement
{
	/*
	===================================================================================

	PlayerMovementRuntime

	===================================================================================
	*/
	/// <summary>
	/// Shared mutable runtime state for cooperating movement controllers. This object is
	/// intentionally data-only except for coarse reset helpers.
	/// </summary>

	internal sealed class PlayerMovementRuntime
	{
		public Vector3 HorizontalVelocity = Vector3.Zero;
		public Vector3 LastPlanarWishDirection = new Vector3( 0.0f, 0.0f, -1.0f );
		public Vector3 DashDirection = new Vector3( 0.0f, 0.0f, -1.0f );
		public Vector3 SlideDirection = new Vector3( 0.0f, 0.0f, -1.0f );
		public Vector3 WallNormal = Vector3.Zero;
		public Vector3 WallRunDirection = Vector3.Zero;
		public WallMountedMoveMode WallMountedMode = WallMountedMoveMode.None;

		public void ClearWallRun()
		{
			WallNormal = Vector3.Zero;
			WallRunDirection = Vector3.Zero;
			WallMountedMode = WallMountedMoveMode.None;
		}
	};
};
