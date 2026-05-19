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
using Nomad.Game.Domain.Data.Multiplayer;
using Nomad.Game.Domain.Data.Player;
using Nomad.Game.Domain.Data.Player.State;
using Nomad.Game.Domain.Events.Player;
using Nomad.Game.Domain.Interfaces.Player;
using Nomad.Game.Prefabs;

namespace Nomad.Game.Application.Gameplay.Player
{
	/*
	===================================================================================

	PlayerMovementController

	===================================================================================
	*/
	/// <summary>
	/// Applies movement from an IPlayerInputSource.
	///
	/// This controller no longer subscribes directly to input actions. Local and
	/// remote input are supplied through LocalPlayerInputSource and
	/// RemotePlayerInputSource.
	/// </summary>

	internal sealed class PlayerMovementController : NomadBehaviour
	{
		public PlayerId Id { get; set; }
		public IPlayerDerivedStatService Stats { get; set; }
		public IPlayerFlagService Flags { get; set; }
		public IPlayerInputSource InputSource { get; set; }
		public IPlayerStateReader StateReader { get; set; }
		public IPlayerStateWriter StateWriter { get; set; }

		private float _effectiveMovementSpeed = 0.0f;

		private bool _isMoving = false;
		private Vector2 _moveInput = Vector2.Zero;
		private Vector2 _velocity = Vector2.Zero;
		private uint _inputTick;

		private PlayerPrefab _prefab;

		private readonly Godot.Timer _slideTimer;

		[Event( nameSpace: "Nomad.Game.Domain.Events.Player", PayloadName = "PlayerMovementChangedEventArgs" )]
		[EventPayload( "OldVelocity", typeof( Vector2 ), Order = 1 )]
		[EventPayload( "NewVelocity", typeof( Vector2 ), Order = 2 )]
		[EventPayload( "IsMoving", typeof( bool ), Order = 3 )]
		[EventPayload( "WalkingReverse", typeof( bool ), Order = 4 )]
		public IGameEvent<PlayerMovementChangedEventArgs> MovementChanged => _movementChanged;
		private IGameEvent<PlayerMovementChangedEventArgs> _movementChanged = null;

		/*
		===============
		PlayerMovementController
		===============
		*/
		/// <summary>
		///
		/// </summary>
		public PlayerMovementController()
		{
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
				.GetEvent<PlayerDashStartEventArgs>(
					$"{Id}:{PlayerDashStartEventArgs.Name}",
					PlayerDashStartEventArgs.NameSpace
				)
				.Subscribe( OnDashStarted );

			eventFactory
				.GetEvent<PlayerDashEndedEventArgs>(
					$"{Id}:{PlayerDashEndedEventArgs.Name}",
					PlayerDashEndedEventArgs.NameSpace
				)
				.Subscribe( OnDashEnded );

			eventFactory
				.GetEvent<PlayerDerivedStatChangedEventArgs>(
					$"{Id}:{PlayerDerivedStatChangedEventArgs.Name}",
					PlayerDerivedStatChangedEventArgs.NameSpace
				)
				.Subscribe( OnStatChanged );

			_movementChanged = eventFactory
				.GetEvent<PlayerMovementChangedEventArgs>(
					$"{Id}:{PlayerMovementChangedEventArgs.Name}",
					PlayerMovementChangedEventArgs.NameSpace,
					EventFlags.NoLock
				);
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
				.GetEvent<PlayerDashStartEventArgs>(
					PlayerDashStartEventArgs.Name,
					PlayerDashStartEventArgs.NameSpace
				)
				.Unsubscribe( OnDashStarted );

			eventFactory
				.GetEvent<PlayerDashEndedEventArgs>(
					PlayerDashEndedEventArgs.Name,
					PlayerDashEndedEventArgs.NameSpace
				)
				.Unsubscribe( OnDashEnded );

			eventFactory
				.GetEvent<PlayerDerivedStatChangedEventArgs>(
					PlayerDerivedStatChangedEventArgs.Name,
					PlayerDerivedStatChangedEventArgs.NameSpace
				)
				.Unsubscribe( OnStatChanged );

			_slideTimer.Timeout -= OnSlideTimerTimeout;
			_slideTimer.Dispose();

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
			Vector2 previousVelocity = _velocity;

			if ( !StateReader.CanMove || !StateReader.CanTakeInput ) {
				// make sure we enforce a no movement state if its present.

				_moveInput = Vector2.Zero;
				_isMoving = false;
				_velocity = Vector2.Zero;

				if ( previousVelocity.LengthSquared() > 0.001f ) {
					_movementChanged.Publish(
						new PlayerMovementChangedEventArgs(
							previousVelocity,
							_velocity,
							false,
							false
						)
					);
				}

				_prefab.Velocity = _velocity.ToGodot();
				_prefab.MoveAndSlide();

				return;
			}

			PlayerInputFrame input = InputSource != null
				? InputSource.ReadFrame( _inputTick++ )
				: PlayerInputFrame.Empty;

			_moveInput = input.Move;
			_isMoving = input.IsMoving;

			if ( input.SlidePressed ) {
				StartSlide();
			}

			if ( _isMoving ) {
				_velocity = HandleAcceleration( delta );
			} else {
				_velocity = HandleDeceleration( delta );
			}

			bool isMovingNow = _velocity.LengthSquared() > 0.001f;

			if ( isMovingNow ) {
				StateWriter.SetMoving( PlayerStateChangeReason.Movement );
			} else {
				StateWriter.SetIdle( PlayerStateChangeReason.Movement );
			}

			bool reverse =
				MathF.Sign( previousVelocity.X ) != 0 &&
				MathF.Sign( _moveInput.X ) != 0 &&
				MathF.Sign( previousVelocity.X ) != MathF.Sign( _moveInput.X );

			_movementChanged.Publish(
				new PlayerMovementChangedEventArgs(
					previousVelocity,
					_velocity,
					isMovingNow,
					reverse
				)
			);

			_prefab.Velocity = _velocity.ToGodot();
			_prefab.MoveAndSlide();
		}

		/*
		===============
		StartSlide
		===============
		*/
		private void StartSlide()
		{
			if ( Flags.GetFlags( PlayerFlags.Sliding ) ) {
				return;
			}

			Flags.AddFlags( PlayerFlags.Sliding );
			_slideTimer.Start();
		}

		/*
		===============
		ApplyDashingSpeedBonus
		===============
		*/
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
		private void OnSlideTimerTimeout()
		{
			Flags.RemoveFlags( PlayerFlags.Sliding );
		}

		/*
		===============
		OnDashStarted
		===============
		*/
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
	};
};
