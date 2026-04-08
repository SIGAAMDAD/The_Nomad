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

using Nomad.Game.Domain.Data.Player;
using Nomad.Game.Prefabs;

namespace Nomad.Game.Application.Gameplay.Player.Animation {
	internal sealed class PlayerAnimationRepository {
		public PlayerAnimationState Current => _current;
		private PlayerAnimationState _current = PlayerAnimationState.Idle;

		private readonly PlayerTorsoAnimator _torsoAnimator;
		private readonly PlayerHeadAnimator _headAnimator;
		private readonly PlayerLegAnimator _legAnimator;
		private readonly PlayerFootsteps _footsteps;

		public PlayerAnimationRepository( PlayerPrefab prefab, PlayerMovementController movementController ) { 
			_torsoAnimator = prefab.AddComponent<PlayerTorsoAnimator>();
			_headAnimator = prefab.AddComponent<PlayerHeadAnimator>();
			_legAnimator = prefab.AddComponent<PlayerLegAnimator>();
			_footsteps = prefab.AddComponent<PlayerFootsteps>();
		}
	};
};