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

namespace Nomad.Game.Sdk.Player.JumpKit
{
	public readonly struct DashUpdateResult
	{
		public float BurnoutAmount { get; }
		public float DashDuration { get; }
		public float RemainingDashTime { get; }
		public bool DashEnded { get; }
		public bool BurnoutChangedThisFrame { get; }
		public bool BurnedOutThisFrame { get; }
		public bool RechargedThisFrame { get; }
		public bool IsDashing { get; }
		public bool IsBurnedOut { get; }

		public DashUpdateResult(
			float burnoutAmount,
			float dashDuration,
			float remainingDashTime,
			bool dashEnded,
			bool burnoutChangedThisFrame,
			bool burnedOutThisFrame,
			bool rechargedThisFrame,
			bool isDashing,
			bool isBurnedOut
		)
		{
			BurnoutAmount = burnoutAmount;
			DashDuration = dashDuration;
			RemainingDashTime = remainingDashTime;
			DashEnded = dashEnded;
			BurnoutChangedThisFrame = burnoutChangedThisFrame;
			BurnedOutThisFrame = burnedOutThisFrame;
			RechargedThisFrame = rechargedThisFrame;
			IsDashing = isDashing;
			IsBurnedOut = isBurnedOut;
		}
	}
}
