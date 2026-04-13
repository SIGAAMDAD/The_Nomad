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

namespace Nomad.Game.Domain.Interfaces.Player {
	/// <summary>
	/// 
	/// </summary>
	public interface IDashModule {
		/// <summary>
		/// The name of this dash module/upgrade.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// The upgrade's description.
		/// </summary>
		string Description { get; }

		/// <summary>
		/// The maximum amount of burnout that is allowed before overcharge.
		/// </summary>
		float BurnoutMax { get; }

		/// <summary>
		/// How much burnout accumulates per usage of the jumpkit.
		/// </summary>
		float DashBurnoutIncrease { get; }

		/// <summary>
		/// The amount of time in milliseconds that each dash gets.
		/// </summary>
		float DashDuration { get; }

		/// <summary>
		/// The amount of time before the dashkit cools down after being used.
		/// </summary>
		float BurnoutCooldown { get; }

		/// <summary>
		/// The cooldown length after an overcharge before the dashkit can be used again.
		/// </summary>
		float BurnoutResetDuration { get; }

		/// <summary>
		/// 
		/// </summary>
		float DashVelocity { get; }

		/// <summary>
		/// 
		/// </summary>
		int BaseIFrames { get; }

		/// <summary>
		/// The module's audio pitch curve.
		/// </summary>
		float EnginePitchPerChain { get; }
	};
};