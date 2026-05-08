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

using Nomad.Core.Events;
using Nomad.Game.Domain.Data.Player;
using Nomad.Scene.GameObjects;
using Nomad.Game.Domain.Events.Player;
using Nomad.Events.Globals;
using Godot;

namespace Nomad.Game.Application.Gameplay.Player.Animation
{
	/*
	===================================================================================

	PlayerHandAnimator

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed class PlayerHandAnimator : PlayerAnimator
	{
		public SpriteFrames Frames { get; set; }

		public override IGameEvent<PlayerAnimationStateChangedEventArgs> AnimationStateChanged => _animationStateChanged;
		private readonly IGameEvent<PlayerAnimationStateChangedEventArgs> _animationStateChanged = default;

		/*
		===============
		PlayerHandAnimator
		===============
		*/
		/// <summary>
		///
		/// </summary>
		public PlayerHandAnimator()
		{
			var eventFactory = GameEventRegistry.Instance;

			_animationStateChanged = eventFactory
				.GetEvent<PlayerAnimationStateChangedEventArgs>(
					$"Hand:{Id}:{PlayerAnimationStateChangedEventArgs.NameSpace}",
					PlayerAnimationStateChangedEventArgs.NameSpace
				);
		}

		/*
		===============
		OnInit
		===============
		*/
		/// <summary>
		///
		/// </summary>
		public override void OnInit()
		{
			base.OnInit();

			animator = prefab.FindChild<EngineAnimatedSprite2D>( "LegAnimator" );
			animator.SpriteFrames = Frames;
		}

		/*
		===============
		OnPlayerMovementChanged
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		protected override void OnPlayerMovementChanged( in PlayerMovementChangedEventArgs args )
		{
			if ( !args.IsMoving ) {
				animator.Play( "idle" );
				return;
			}
			animator.Play( "move" );
		}
	};
};
