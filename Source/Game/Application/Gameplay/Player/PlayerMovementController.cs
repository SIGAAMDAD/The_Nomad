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

using System.Numerics;
using System.Runtime.CompilerServices;
using Nomad.Core.Events;
using Nomad.EngineUtils;
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

		private readonly Godot.Timer _slideTimer;

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
			
			eventFactory
				.GetEvent<PlayerDashStartEventArgs>( EventNames.PLAYER_DASH_STARTED, EventNames.NAMESPACE )
				.Subscribe( OnDashStarted );
			
			eventFactory
				.GetEvent<EmptyEventArgs>( EventNames.PLAYER_DASH_ENDED, EventNames.NAMESPACE )
				.Subscribe( OnDashEnded );
			
			_slideTimer = new Godot.Timer() {
				WaitTime = Domain.Data.Player.Constants.SLIDE_DURATION,
				OneShot = true
			};
			_slideTimer.Timeout += OnSlideTimerTimeout;
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
			_prefab.AddChild( _slideTimer );

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
			return Flags.GetFlags( PlayerFlags.Dashing ) ? Stats.GetValue( DerivedStatType.EffectiveDashSpeed ) : 1.0f;
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
			return Flags.GetFlags( PlayerFlags.Sliding ) ? 1200.0f : 1.0f;
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
			float speed = _effectiveMovementSpeed + ApplyDashingSpeedBonus() + ApplySlidingSpeedBonus();
			float accel = Domain.Data.Player.Constants.MOVEMENT_ACCELERATION;
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

		/*
		===============
		OnStatChanged
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnStatChanged( in PlayerDerivedStatChangedEventArgs args ) {
			if ( args.StatId == DerivedStatType.EffectiveMovementSpeed ) {
				_effectiveMovementSpeed = args.NewValue;
			}
		}

		/*
		===============
		OnSlideTimerTimeout
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		private void OnSlideTimerTimeout() {
			Flags.RemoveFlags( PlayerFlags.Sliding );
		}

		/*
		===============
		OnDashStarted
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnDashStarted( in PlayerDashStartEventArgs args ) {
			Flags.AddFlags( PlayerFlags.Dashing );
		}

		/*
		===============
		OnDashEnded
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnDashEnded( in EmptyEventArgs args ) {
			Flags.RemoveFlags( PlayerFlags.Dashing );
		}
		
		/*
		===============
		OnMoveActionTriggered
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnMoveActionTriggered( in AxisActionEventArgs args ) {
			_moveInput = args.Value;
			_moveInput.Y = -_moveInput.Y;
			if ( args.Phase == InputActionPhase.Performed ) {
				_startMoving.Publish( new PlayerStartMovingEventArgs( _moveInput ) );
			} else if ( args.Phase == InputActionPhase.Canceled ) {
				_stopMoving.Publish( default );
			}
		}

		/*
		===============
		OnSlideActionTriggered
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnSlideActionTriggered( in ButtonActionEventArgs args ) {
			if ( args.Phase == InputActionPhase.Started ) {
				Flags.AddFlags( PlayerFlags.Sliding );
			}
		}
	};
};