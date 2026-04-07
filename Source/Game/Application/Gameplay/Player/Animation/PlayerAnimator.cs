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
using Nomad.Audio.Interfaces;
using Nomad.Core.Events;
using Nomad.Core.ServiceRegistry.Globals;
using Nomad.EngineUtils;
using Nomad.Events.Globals;
using Nomad.Game.Domain.Data.Player;
using Nomad.Game.Domain.Events.Player;
using Nomad.Game.Prefabs;
using Nomad.Logger.Globals;

namespace Nomad.Game.Application.Gameplay.Player.Animation {
	/*
	===================================================================================
	
	PlayerAnimator
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	internal sealed class PlayerAnimator : NomadBehaviour {
		private AnimatedSprite2D _torsoAnimator;
		private AnimatedSprite2D _legAnimator;

		private PlayerPrefab _prefab;

		private readonly IAudioEmitter _emitter;
		private readonly IListenerService _listenerService;

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

			_emitter = ServiceLocator.GetService<IEmitterFactory>().CreateEmitter( "SoundCategory:Foley" );
			_listenerService = ServiceLocator.GetService<IListenerService>();

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

			_prefab = Object.CastAs<PlayerPrefab>();
			_torsoAnimator = _prefab.GetNode<AnimatedSprite2D>( "TorsoAnimator" );
			_legAnimator = _prefab.GetNode<AnimatedSprite2D>( "LegAnimator" );
			_legAnimator.AnimationLooped += OnLegAnimationLooped;
		}

		private void OnLegAnimationLooped() {
			if ( _prefab.Velocity != Vector2.Zero ) {
				_emitter.PlaySound( "event:/SoundEffects/Environment/Player/WalkSand", _prefab.GlobalPosition.ToSystem() );
			}
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

			var velocity = _prefab.Velocity;
			if ( velocity.X > 0.0f ) {
				Flip( false );
			} else if ( velocity.X < 0.0f ) {
				Flip( true );
			}

			_emitter.Position = _prefab.GlobalPosition.ToSystem();
			_listenerService.SetListenerPosition( 0, _prefab.GlobalPosition.ToSystem() );
		}

		private void Flip( bool flip ) {
			_torsoAnimator.FlipH = flip;
			_legAnimator.FlipH = flip;
		}

		private void OnPlayerStartMoving( in PlayerStartMovingEventArgs args ) {
			_legAnimator.Play( "move" );

			// are we backpedaling?
			if ( ( _torsoAnimator.FlipH && args.Velocity.X > 0.0f ) || ( !_torsoAnimator.FlipH && args.Velocity.X < 0.0f ) ) {
				// sudden stop?
				if ( ( args.Velocity.X < 0.0f && _prefab.Velocity.X > 0.0f ) || ( args.Velocity.X > 0.0f && _prefab.Velocity.X < 0.0f ) ) {
					_legAnimator.Play( "sudden_stop" );
				} else {
					_legAnimator.Play( "move" );
				}
			} else {
				_legAnimator.PlayBackwards( "move" );
			}
		}

		private void OnPlayerStopMoving( in EmptyEventArgs args ) {
			_legAnimator.Play( "idle" );
		}
	};
};