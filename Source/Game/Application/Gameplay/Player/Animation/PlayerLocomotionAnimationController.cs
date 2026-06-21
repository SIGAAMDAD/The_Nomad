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

using Nomad.Game.Sdk.Player.Input;

namespace Nomad.Game.Application.Gameplay.Player.Animation
{
	internal sealed class PlayerLocomotionAnimationController
	{
		private readonly PlayerAnimationPlaybackRouter _playback;

		private PlayerLocomotionDirection _activeDirection = PlayerLocomotionDirection.Count;
		private PlayerAnimationClip _activeOneShotClip = PlayerAnimationClip.Count;

		public PlayerLocomotionAnimationController( PlayerAnimationPlaybackRouter playback )
		{
			_playback = playback;
		}

		public void Reset()
		{
			_activeDirection = PlayerLocomotionDirection.Count;
			_activeOneShotClip = PlayerAnimationClip.Count;
		}

		public void PlayDirectional( PlayerLocomotionDirection direction )
		{
			if ( IsOneShotActive() || _activeDirection == direction ) {
				return;
			}

			_playback.TravelLocomotion( direction );
			_activeDirection = direction;
		}

		public void PlayHardStop()
		{
			PlayOneShot( PlayerAnimationClip.SuddenStop );
		}

		public void PlayReverse()
		{
			PlayOneShot( PlayerAnimationClip.Running180 );
		}

		private void PlayOneShot( PlayerAnimationClip clip )
		{
			_playback.TravelLocomotionClip( clip );
			_activeOneShotClip = clip;
			_activeDirection = PlayerLocomotionDirection.Count;
		}

		private bool IsOneShotActive()
		{
			if ( _activeOneShotClip == PlayerAnimationClip.Count ) {
				return false;
			}

			if ( _playback.IsCurrentClip( _activeOneShotClip ) ) {
				return true;
			}

			_activeOneShotClip = PlayerAnimationClip.Count;
			return false;
		}
	}
}
