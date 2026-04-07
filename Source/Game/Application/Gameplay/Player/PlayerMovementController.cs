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
using System.Numerics;
using System.Runtime.CompilerServices;
using Nomad.Audio.Interfaces;
using Nomad.Core.Events;
using Nomad.Core.ServiceRegistry.Globals;
using Nomad.EngineUtils;
using Nomad.Events.Extensions;
using Nomad.Events.Globals;
using Nomad.Game.Domain.Data.Player;
using Nomad.Game.Domain.Events.Player;
using Nomad.Game.Domain.Interfaces.Player;
using Nomad.Game.Prefabs;
using Nomad.Input.Events;
using Nomad.Input.ValueObjects;

namespace Nomad.Game.Application.Gameplay.Player {
	/*
	===================================================================================
	
	PlayerMovementController
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	internal sealed class PlayerMovementController : NomadBehaviour {
		public IPlayerDerivedStatService Stats { get; set; }
		public IPlayerFlagService Flags { get; set; }
		private float _effectiveMovementSpeed = 0.0f;

		private Vector2 _moveInput = Vector2.Zero;
		private Vector2 _velocity = Vector2.Zero;

		private PlayerPrefab _prefab;

		private readonly IGameEvent<EmptyEventArgs> _slideTimer;

		public IGameEvent<PlayerStartMovingEventArgs> StartMoving => _startMoving;
		private readonly IGameEvent<PlayerStartMovingEventArgs> _startMoving;

		public IGameEvent<EmptyEventArgs> StopMoving => _stopMoving;
		private readonly IGameEvent<EmptyEventArgs> _stopMoving;

		/*
		===============
		PlayerMovementController
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		public PlayerMovementController() {
			var eventFactory = GameEventRegistry.Instance;

			_startMoving = eventFactory
				.GetEvent<PlayerStartMovingEventArgs>( EventNames.PLAYER_START_MOVING, EventNames.NAMESPACE );
			
			_stopMoving = eventFactory
				.GetEvent<EmptyEventArgs>( EventNames.PLAYER_STOP_MOVING, EventNames.NAMESPACE );
			
			eventFactory
				.GetEvent<ButtonActionEventArgs>( $"Slide:{Input.Constants.Events.BUTTON_ACTION}", Input.Constants.Events.NAMESPACE )
				.Subscribe( OnSlideActionTriggered );

			eventFactory
				.GetEvent<AxisActionEventArgs>( $"Move:{Input.Constants.Events.AXIS_ACTION}", Input.Constants.Events.NAMESPACE )
				.Subscribe( OnMoveActionTriggered );
			
			_slideTimer = eventFactory
				.GetEvent<EmptyEventArgs>( $"{Guid.NewGuid()}:{nameof(_slideTimer)}", nameof( PlayerMovementController ) )
				.PublishAfter( (int)Domain.Data.Player.Constants.SLIDE_DURATION );
			_slideTimer.Subscribe( OnSlideTimerTimeout );
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

			_effectiveMovementSpeed = Stats.GetValue( DerivedStatType.EffectiveMovementSpeed );
		}

		/*
		===============
		OnShutdown
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		public override void OnShutdown() {
			base.OnShutdown();

			var eventFactory = GameEventRegistry.Instance;

			eventFactory
				.GetEvent<PlayerDerivedStatChangedEventArgs>( EventNames.PLAYER_DERIVED_STAT_CHANGED, EventNames.NAMESPACE )
				.Unsubscribe( OnStatChanged );
			
			eventFactory
				.GetEvent<ButtonActionEventArgs>( $"Slide:{Input.Constants.Events.BUTTON_ACTION}", Input.Constants.Events.NAMESPACE )
				.Unsubscribe( OnSlideActionTriggered );

			eventFactory
				.GetEvent<AxisActionEventArgs>( $"Move:{Input.Constants.Events.AXIS_ACTION}", Input.Constants.Events.NAMESPACE )
				.Unsubscribe( OnMoveActionTriggered );

			_slideTimer?.Dispose();
			_startMoving?.Dispose();
			_stopMoving?.Dispose();
		}

		/*
		===============
		OnPhysicsUpdate
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="delta"></param>
		public override void OnPhysicsUpdate( float delta ) {
			base.OnPhysicsUpdate( delta );

			var moveInput = _moveInput;
			if ( moveInput != Vector2.Zero ) {
				_velocity = HandleAcceleration( delta );
			} else {
				_velocity = HandleDeceleration( delta );
			}

			_prefab.Velocity = _velocity.ToGodot();
			_prefab.MoveAndSlide();
		}

		/*
		===============
		ApplyDashingSpeedBonus
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private float ApplyDashingSpeedBonus() {
			return Flags.GetFlags( PlayerFlags.Dashing ) ? 8800.0f : 0.0f;
		}

		/*
		===============
		ApplySlidingSpeedBonus
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private float ApplySlidingSpeedBonus() {
			return Flags.GetFlags( PlayerFlags.Sliding ) ? 1200.0f : 0.0f;
		}

		/*
		===============
		HandleAcceleration
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="delta"></param>
		/// <returns></returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private Vector2 HandleAcceleration( float delta ) {
			float speed = _effectiveMovementSpeed;
			float accel = Domain.Data.Player.Constants.MOVEMENT_ACCELERATION + ApplyDashingSpeedBonus() + ApplySlidingSpeedBonus();
			return MoveToward( _velocity, _moveInput * speed, delta * accel );
		}

		/*
		===============
		HandleDeceleration
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="delta"></param>
		/// <returns></returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public Vector2 HandleDeceleration( float delta ) {
			return MoveToward( _velocity, Vector2.Zero, delta * Domain.Data.Player.Constants.MOVEMENT_FRICTION );
		}

		/*
		===============
		MoveToward
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="from"></param>
		/// <param name="to"></param>
		/// <param name="delta"></param>
		/// <returns></returns>
		private static Vector2 MoveToward( Vector2 from, Vector2 to, float delta ) {
			Vector2 vector = to - from;
			float length = vector.Length();
			if ( length <= delta || length < 1E-06f ) {
				return to;
			}
			return from + vector / length * delta;
		}

		private void OnStatChanged( in PlayerDerivedStatChangedEventArgs args ) {
			if ( args.StatId == DerivedStatType.EffectiveMovementSpeed ) {
				_effectiveMovementSpeed = args.NewValue;
			}
		}

		private void OnSlideTimerTimeout( in EmptyEventArgs args ) {
			Flags.RemoveFlags( PlayerFlags.Sliding );
		}
		
		private void OnMoveActionTriggered( in AxisActionEventArgs args ) {
			_moveInput = args.Value;
			_moveInput.Y = -_moveInput.Y;
			if ( args.Phase == InputActionPhase.Performed ) {
				_startMoving.Publish( new PlayerStartMovingEventArgs( _moveInput ) );
			} else if ( args.Phase == InputActionPhase.Canceled ) {
				_stopMoving.Publish( default );
			}
		}

		private void OnSlideActionTriggered( in ButtonActionEventArgs args ) {
			if ( args.Phase == InputActionPhase.Performed ) {
				Flags.AddFlags( PlayerFlags.Sliding );
			}
		}
	};
};