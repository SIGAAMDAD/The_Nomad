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

using System;
using Godot;
using Nomad.Game.Domain.Data.Player;
using Nomad.Game.Domain.Interfaces.Player;
using Nomad.Game.Prefabs;

namespace Nomad.Game.Application.Gameplay.Player.Animation
{
	/*
	===================================================================================

	PlayerAnimationCoordinator

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed class PlayerAnimationCoordinator
	{
		public PlayerAnimationState Current => _current;
		private PlayerAnimationState _current = PlayerAnimationState.Idle;

		private readonly PlayerTorsoAnimator _torsoAnimator;
		private readonly PlayerHeadAnimator _headAnimator;
		private readonly PlayerLegAnimator _legAnimator;
		private readonly PlayerFootsteps _footsteps;

		private readonly PlayerLeftHandAnimator _leftHandAnimator;
		private readonly PlayerRightHandAnimator _rightHandAnimator;

		/*
		===============
		PlayerAnimationCoordinator
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="id"></param>
		/// <param name="prefab"></param>
		/// <param name="initialState"></param>
		/// <param name="movementController"></param>
		public PlayerAnimationCoordinator( Guid id, PlayerPrefab prefab, PlayerAnimationState initialState, IPlayerStateReader stateReader, PlayerMovementController movementController )
		{
			_headAnimator = prefab.AddComponent<PlayerHeadAnimator>();
			_legAnimator = prefab.AddComponent<PlayerLegAnimator>( comp => {
				comp.Id = id;
			} );
			_torsoAnimator = prefab.AddComponent<PlayerTorsoAnimator>( comp => {
				comp.Id = id;
			} );
			_footsteps = prefab.AddComponent<PlayerFootsteps>();

			_leftHandAnimator = prefab.AddComponent<PlayerLeftHandAnimator>( comp => {
				comp.Frames = ResourceLoader.Load<SpriteFrames>( "res://Assets/Animations/Player/LeftArmAnimations.tres" );
			} );
			_rightHandAnimator = prefab.AddComponent<PlayerRightHandAnimator>( comp => {
				comp.Frames = ResourceLoader.Load<SpriteFrames>( "res://Assets/Animations/Player/RightArmAnimations.tres" );
			} );
		}
	};
};
