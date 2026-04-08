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

using Godot;
using Nomad.Audio.Interfaces;
using Nomad.Core.Events;
using Nomad.Core.ServiceRegistry.Globals;
using Nomad.EngineUtils;
using Nomad.Events.Globals;
using Nomad.Game.Domain.Data.Player;
using Nomad.Game.Domain.Events.Player;
using Nomad.Game.Prefabs;

namespace Nomad.Game.Application.Gameplay.Player.Animation {
	/*
	===================================================================================
	
	PlayerAnimator
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	internal abstract class PlayerAnimator : NomadBehaviour {
		protected PlayerPrefab prefab;

		public abstract IGameEvent<PlayerAnimationStateChangedEventArgs> AnimationStateChanged { get; }

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

			eventFactory
				.GetEvent<PlayerStartMovingEventArgs>( EventNames.PLAYER_START_MOVING, EventNames.NAMESPACE )
				.Subscribe( OnPlayerStartMoving );
			
			eventFactory
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

			prefab = Object.CastAs<PlayerPrefab>();
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

			var velocity = prefab.Velocity;
			if ( velocity.X > 0.0f ) {
				Flip( false );
			} else if ( velocity.X < 0.0f ) {
				Flip( true );
			}
		}

		protected abstract void Flip( bool flip );
		protected abstract void OnPlayerStartMoving( in PlayerStartMovingEventArgs args );
		protected abstract void OnPlayerStopMoving( in EmptyEventArgs args );
	};
};