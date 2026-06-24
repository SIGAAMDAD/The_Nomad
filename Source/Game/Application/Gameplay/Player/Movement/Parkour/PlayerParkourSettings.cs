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

namespace Nomad.Game.Application.Gameplay.Player.Movement.Parkour
{
	internal sealed class PlayerParkourSettings
	{
		public float AttachSearchRadius = 1.35f;
		public float MinFacingAnchorDot = 0.05f;
		public float MinFacingWallDot = 0.15f;
		public float EntryLineStartHeight = 1.05f;
		public float EntryLineEndHeight = 0.25f;
		public bool ValidateEntryLineOfSight = true;
		public uint ValidationMask = uint.MaxValue;

		public float AnchorSnapSpeed = 18.0f;
		public float TurnToWallSpeed = 20.0f;
		public float EdgeSnapDistance = 0.04f;
		public float MinimumEdgeDuration = 0.05f;
		public float DefaultEdgeDuration = 0.22f;
		public float EdgeInputDeadZone = 0.20f;
		public float EdgeSelectionMinScore = 0.25f;

		public float MantleInputThreshold = 0.60f;
		public float MantleUpOffset = 1.00f;
		public float MantleForwardOffset = 0.75f;
		public float VaultUpOffset = 0.20f;
		public float VaultForwardOffset = 1.25f;
		public float DropPushOffSpeed = 2.00f;
		public float DropDownSpeed = 2.50f;

		public int InitialQueryBufferCapacity = 96;
	}
}
