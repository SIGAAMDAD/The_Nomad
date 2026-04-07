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
using Nomad.Core.Events;
using Nomad.EngineUtils;
using Nomad.Events.Globals;
using Nomad.Game.Domain.Data.Player;
using Nomad.Game.Domain.Events.Player;
using Nomad.Game.Prefabs;

namespace Nomad.Game.Application.Gameplay.Player.Animation {
	internal sealed class PlayerAnimator : NomadBehaviour {
		private AnimatedSprite2D _torsoAnimator;
		private AnimatedSprite2D _legAnimator;

		private readonly ISubscriptionHandle _startMoving;
		private readonly ISubscriptionHandle _stopMoving;
		
		/*
		===============
		PlayerAnimator
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		public PlayerAnimator() {
			var eventFactory = GameEventRegistry.Instance;

			_startMoving = eventFactory
				.GetEvent<PlayerStartMovingEventArgs>( EventNames.PLAYER_START_MOVING, EventNames.NAMESPACE )
				.Subscribe( OnPlayerStartMoving );
			
			_stopMoving = eventFactory
				.GetEvent<EmptyEventArgs>( EventNames.PLAYER_STOP_MOVING, EventNames.NAMESPACE )
				.Subscribe( OnPlayerStopMoving );
		}

		/*
		===============
		OnInit
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		public override void OnInit() {
			base.OnInit();

			var prefab = Object.CastAs<PlayerPrefab>();
			_torsoAnimator = prefab.GetNode<AnimatedSprite2D>( "TorsoAnimator" );
			_legAnimator = prefab.GetNode<AnimatedSprite2D>( "LegAnimator" );
		}

		/*
		===============
		OnUpdate
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="delta"></param>
		public override void OnUpdate( float delta ) {
			base.OnUpdate( delta );
		}

		private void OnPlayerStartMoving( in PlayerStartMovingEventArgs args ) {
			_legAnimator.Play( "move" );
			if ( !( _torsoAnimator.FlipH && args.Velocity.X > 0.0f ) || ( !_torsoAnimator.FlipH && args.Velocity.X < 0.0f ) ) {
			} else {
			}
		}

		private void OnPlayerStopMoving( in EmptyEventArgs args ) {
			_legAnimator.Play( "idle" );
		}
	};
};