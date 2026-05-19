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

namespace Nomad.Game.Domain.Data.Player.State
{
	public readonly struct PlayerStateTransitionResult
	{
		public bool Success { get; }
		public PlayerStateId OldState { get; }
		public PlayerStateId NewState { get; }
		public string? FailureReason { get; }

		public PlayerStateTransitionResult(
			bool success,
			PlayerStateId oldState,
			PlayerStateId newState,
			string? failureReason = null
		)
		{
			Success = success;
			OldState = oldState;
			NewState = newState;
			FailureReason = failureReason;
		}

		public static PlayerStateTransitionResult Succeeded(
			PlayerStateId oldState,
			PlayerStateId newState
		)
		{
			return new PlayerStateTransitionResult(
				true,
				oldState,
				newState
			);
		}

		public static PlayerStateTransitionResult Failed(
			PlayerStateId oldState,
			PlayerStateId newState,
			string reason
		)
		{
			return new PlayerStateTransitionResult(
				false,
				oldState,
				newState,
				reason
			);
		}
	};
};
