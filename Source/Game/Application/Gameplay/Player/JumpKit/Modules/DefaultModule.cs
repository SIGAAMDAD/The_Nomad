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

using Nomad.Game.Domain.Interfaces.Player;

namespace Nomad.Game.Application.Gameplay.Player.JumpKit.Modules
{
	/*
	===================================================================================
	
	DefaultModule
	
	===================================================================================
	*/
	/// <summary>
	/// The default dash module that is equipped at the start, does nothing special.
	/// </summary>

	internal sealed class DefaultModule : IDashModule
	{
		public string Name => "Default";
		public string Description => "The default module, no special effects.";

		public float BurnoutMax => 1.0f;
		public float DashBurnoutIncrease => 0.30f;
		public float DashDuration => 0.65f;
		public float BurnoutCooldown => 0.30f;

		public float BurnoutResetDuration {
			get {
				throw new System.NotImplementedException();
			}
		}

		public float DashVelocity {
			get {
				throw new System.NotImplementedException();
			}
		}

		public int BaseIFrames {
			get {
				throw new System.NotImplementedException();
			}
		}

		public float EnginePitchPerChain {
			get {
				throw new System.NotImplementedException();
			}
		}
	};
};
