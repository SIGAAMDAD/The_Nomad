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

namespace Nomad.Game.Application.Gameplay.Player.Movement.WallRunning
{
	/*
	===================================================================================

	WallRunSettings

	===================================================================================
	*/
	/// <summary>
	/// Tunable constants for dynamic wall-mounted movement. Kept separate from the
	/// controller so profiling/tuning changes do not touch the state machine.
	/// </summary>

	internal sealed class WallRunSettings
	{
		public uint SurfaceMask = uint.MaxValue;
		public float ChestProbeHeight = 1.05f;
		public float HipProbeHeight = 0.45f;
		public float SideProbeDistance = 0.90f;
		public float ForwardProbeDistance = 0.80f;
		public float SurfaceRetainDistance = 0.75f;
		public float MinimumSurfaceVerticality = 0.76f;
		public float MinimumEntrySpeed = 2.25f;
		public float MinimumInputStrength = 0.16f;
		public float MinimumIntoWallForVerticalRun = 0.42f;
		public float UpIntentThreshold = 0.28f;
		public float DownIntentThreshold = -0.42f;
		public float MaximumRunDuration = 2.20f;
		public float ReattachCooldown = 0.14f;
		public float HorizontalSpeedMultiplier = 1.14f;
		public float MinimumHorizontalSpeed = 4.25f;
		public float UpRunSpeed = 4.80f;
		public float DownRunSpeed = 5.40f;
		public float TangentialAcceleration = 34.0f;
		public float VerticalAcceleration = 28.0f;
		public float WallStickSpeed = 1.65f;
		public float HorizontalGravityScale = 0.16f;
		public float VerticalGravityScale = 0.08f;
		public float DownGravityScale = 0.20f;
		public float Gravity = 1800.0f / PlayerMovementController.GAMEPLAY_UNITS_PER_WORLD_UNIT;
		public float WallJumpAwaySpeed = 5.25f;
		public float WallJumpUpSpeed = 5.75f;
		public float DropExitDownSpeed = 2.50f;
		public float TraversalTransferVelocityScale = 0.35f;
		public float TraversalTransferUpBias = 0.45f;
		public float MinimumParkourTransferDuration = 0.12f;
	}
}
