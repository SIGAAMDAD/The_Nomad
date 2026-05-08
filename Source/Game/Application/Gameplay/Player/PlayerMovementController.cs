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
using Nomad.Core.Events;
using Nomad.EngineUtils;
using Nomad.Events.Globals;
using Nomad.Game.Domain.Data.Player;
using Nomad.Game.Domain.Events.Player;
using Nomad.Game.Domain.Interfaces.Player;
using Nomad.Game.Prefabs;
using Nomad.Input;
using Nomad.Input.ValueObjects;

namespace Nomad.Game.Application.Gameplay.Player
{
	/*
	===================================================================================
	
	PlayerMovementController
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	internal sealed class PlayerMovementController : NomadBehaviour
	{
		public Guid Id { get; set; }
		public IPlayerDerivedStatService Stats { get; set; }
		public IPlayerFlagService Flags { get; set; }

		private float _effectiveMovementSpeed = 0.0f;

		private bool _isMoving = false;
		private Vector2 _moveInput = Vector2.Zero;
		private Vector2 _velocity = Vector2.Zero;

		private PlayerPrefab _prefab;

		private readonly Godot.Timer _slideTimer;

		[Event( nameSpace: "Nomad.Game.Domain.Events.Player", PayloadName = "PlayerMovementChangedEventArgs" )]
		[EventPayload( "OldVelocity", typeof( Vector2 ), Order = 1 )]
		[EventPayload( "NewVelocity", typeof( Vector2 ), Order = 2 )]
		[EventPayload( "IsMoving", typeof( bool ), Order = 3 )]
		[EventPayload( "WalkingReverse", typeof( bool ), Order = 4 )]
		public IGameEvent<PlayerMovementChangedEventArgs> MovementChanged => _movementChanged;
		private IGameEvent<PlayerMovementChangedEventArgs> _movementChanged = default;

		/*
		===============
		PlayerMovementController
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="id"></param>
		public PlayerMovementController()
		{
			var eventFactory = GameEventRegistry.Instance;

			eventFactory
				.GetEvent<ButtonActionEventArgs>( $"Slide:{ButtonActionEventArgs.Name}", ButtonActionEventArgs.NameSpace )
				.Subscribe( OnSlideActionTriggered );

			eventFactory
				.GetEvent<AxisActionEventArgs>( $"Move:{AxisActionEventArgs.Name}", AxisActionEventArgs.NameSpace )
				.Subscribe( OnMoveActionTriggered );

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
		public override void OnInit()
		{
			base.OnInit();

			_prefab = Object.CastAs<PlayerPrefab>();
			_prefab.AddChild( _slideTimer );

			_effectiveMovementSpeed = Stats.GetValue( DerivedStatType.EffectiveMovementSpeed );

			var eventFactory = GameEventRegistry.Instance;

			eventFactory
				.GetEvent<PlayerDashStartEventArgs>( $"{Id}:{PlayerDashStartEventArgs.Name}", PlayerDashStartEventArgs.NameSpace )
				.Subscribe( OnDashStarted );

			eventFactory
				.GetEvent<PlayerDashEndedEventArgs>( $"{Id}:{PlayerDashEndedEventArgs.Name}", PlayerDashEndedEventArgs.NameSpace )
				.Subscribe( OnDashEnded );

			_movementChanged = eventFactory
				.GetEvent<PlayerMovementChangedEventArgs>( $"{Id}:{PlayerMovementChangedEventArgs.Name}", PlayerMovementChangedEventArgs.NameSpace, EventFlags.NoLock );
		}

		/*
		===============
		OnShutdown
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		public override void OnShutdown()
		{
			base.OnShutdown();

			var eventFactory = GameEventRegistry.Instance;

			eventFactory
				.GetEvent<PlayerDashStartEventArgs>( $"{Id}:{PlayerDashStartEventArgs.Name}", PlayerDashStartEventArgs.NameSpace )
				.Unsubscribe( OnDashStarted );

			eventFactory
				.GetEvent<PlayerDashEndedEventArgs>( $"{Id}:{PlayerDashEndedEventArgs.Name}", PlayerDashEndedEventArgs.NameSpace )
				.Unsubscribe( OnDashEnded );

			eventFactory
				.GetEvent<PlayerDerivedStatChangedEventArgs>( PlayerDerivedStatChangedEventArgs.Name, PlayerDerivedStatChangedEventArgs.NameSpace )
				.Unsubscribe( OnStatChanged );

			eventFactory
				.GetEvent<ButtonActionEventArgs>( $"Slide:{Input.Constants.Events.BUTTON_ACTION}", Input.Constants.Events.NAMESPACE )
				.Unsubscribe( OnSlideActionTriggered );

			eventFactory
				.GetEvent<AxisActionEventArgs>( $"Move:{Input.Constants.Events.AXIS_ACTION}", Input.Constants.Events.NAMESPACE )
				.Unsubscribe( OnMoveActionTriggered );

			_slideTimer?.Dispose();
			_movementChanged?.Dispose();
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
		public override void OnPhysicsUpdate( float delta )
		{
			base.OnPhysicsUpdate( delta );

			var previousVelocity = _velocity;

			if ( _isMoving ) {
				_velocity = HandleAcceleration( delta );
			} else {
				_velocity = HandleDeceleration( delta );
			}

			bool isMovingNow = _velocity.LengthSquared() > 0.001f;
			bool reverse =
				MathF.Sign( previousVelocity.X ) != 0 &&
				MathF.Sign( _moveInput.X ) != 0 &&
				MathF.Sign( previousVelocity.X ) != MathF.Sign( _moveInput.X );

			_movementChanged.Publish( new PlayerMovementChangedEventArgs( previousVelocity, _velocity, isMovingNow, reverse ) );

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
		private float ApplyDashingSpeedBonus()
		{
			return Flags.GetFlags( PlayerFlags.Dashing ) ? Stats.GetValue( DerivedStatType.EffectiveDashSpeed ) : 0.0f;
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
		private float ApplySlidingSpeedBonus()
		{
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
		private Vector2 HandleAcceleration( float delta )
		{
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
		public Vector2 HandleDeceleration( float delta )
		{
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
		private static Vector2 MoveToward( Vector2 from, Vector2 to, float delta )
		{
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
		private void OnStatChanged( in PlayerDerivedStatChangedEventArgs args )
		{
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
		private void OnSlideTimerTimeout()
		{
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
		private void OnDashStarted( in PlayerDashStartEventArgs args )
		{
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
		private void OnDashEnded( in PlayerDashEndedEventArgs args )
		{
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
		private void OnMoveActionTriggered( in AxisActionEventArgs args )
		{
			_isMoving = args.Phase == InputActionPhase.Started || args.Phase == InputActionPhase.Performed;

			_moveInput = args.Value;

			// invert the Y axis for the 2D plane
			_moveInput.Y = -_moveInput.Y;
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
		private void OnSlideActionTriggered( in ButtonActionEventArgs args )
		{
			if ( args.Phase == InputActionPhase.Started ) {
				Flags.AddFlags( PlayerFlags.Sliding );
				_slideTimer.Start();
			}
		}
	};
};
