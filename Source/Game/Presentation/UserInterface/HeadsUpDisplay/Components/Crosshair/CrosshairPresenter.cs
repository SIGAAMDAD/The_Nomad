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
using Nomad.Game.Sdk.Events.Player.Movement;
using Nomad.Game.Sdk.Multiplayer;
using Nomad.Game.Sdk.Player.Input;
using Nomad.Game.Sdk.Player.Movement;
using NumericsVector2 = System.Numerics.Vector2;
using NumericsVector3 = System.Numerics.Vector3;

namespace Nomad.Game.Presentation.UserInterface.HeadsUpDisplay.Components.Crosshair
{
	/*
	===================================================================================

	CrosshairPresenter

	===================================================================================
	*/
	/// <summary>
	/// RE4R/RE9-style reticle response: settle while the camera and locomotion are calm,
	/// bloom on movement/turning, snap open briefly on hard locomotion cues, and warn
	/// when the approximated weapon line is obstructed before the camera aim point.
	/// </summary>

	internal sealed class CrosshairPresenter : HudComponentPresenter<CrosshairView>
	{
		private const float BASE_SPREAD_PIXELS = 8.5f;
		private const float MOVEMENT_SPREAD_PIXELS = 11.0f;
		private const float STRAFE_SPREAD_PIXELS = 4.5f;
		private const float BACKPEDAL_SPREAD_PIXELS = 3.5f;
		private const float CAMERA_SPREAD_PIXELS = 16.0f;
		private const float IMPULSE_SPREAD_PIXELS = 1.0f;
		private const float OBSTRUCTED_SPREAD_PIXELS = 5.0f;
		private const float MAX_OFFSET_PIXELS = 9.0f;

		private const float SPREAD_RESPONSE = 18.0f;
		private const float OFFSET_RESPONSE = 14.0f;
		private const float SETTLE_RESPONSE = 10.0f;
		private const float OBSTRUCTION_RESPONSE = 24.0f;
		private const float LOCOMOTION_HOLD_SECONDS = 0.22f;
		private const float CAMERA_DECAY = 8.0f;
		private const float CUE_DECAY = 9.5f;
		private const float KICK_DECAY = 12.0f;
		private const float CAMERA_DOT_MIN = -0.9995f;
		private const float CAMERA_DOT_MAX = 0.9995f;
		private const float MIN_RAY_DISTANCE = 0.05f;

		private readonly PlayerId _playerId;
		private readonly IDisposable _locomotionCue;
		private readonly IDisposable _directionalLocomotion;
		private readonly IDisposable _cameraStatusChanged;

		private NumericsVector3 _lastCameraForward;
		private NumericsVector3 _lastCameraRight;
		private bool _hasCamera;

		private Vector3 _cameraOrigin = Vector3.Zero;
		private Vector3 _cameraForward = new Vector3( 0.0f, 0.0f, -1.0f );
		private Vector3 _cameraRight = new Vector3( 1.0f, 0.0f, 0.0f );
		private bool _hasCameraPose;

		private float _movementAmount;
		private float _strafeAmount;
		private float _backpedalAmount;
		private float _locomotionHold;

		private float _cameraImpulse;
		private float _cueImpulse;
		private Vector2 _cameraKick = Vector2.Zero;
		private Vector2 _cueKick = Vector2.Zero;

		private float _spreadPixels = BASE_SPREAD_PIXELS;
		private float _settle = 1.0f;
		private float _obstructionAmount;
		private Vector2 _offsetPixels = Vector2.Zero;

		public CrosshairPresenter( PlayerId playerId, CrosshairView view, IGameEventRegistryService eventFactory )
			: base( view )
		{
			playerId.ThrowIfInvalid( nameof( CrosshairPresenter ) );

			_playerId = playerId;
			eventFactory = eventFactory ?? throw new ArgumentNullException( nameof( eventFactory ) );

			_locomotionCue = eventFactory
				.GetEvent<PlayerLocomotionCueEventArgs>(
					PlayerLocomotionCueEventArgs.Name,
					PlayerLocomotionCueEventArgs.NameSpace
				)
				.Subscribe( OnLocomotionCue );

			_directionalLocomotion = eventFactory
				.GetEvent<PlayerDirectionalLocomotionEventArgs>(
					PlayerDirectionalLocomotionEventArgs.Name,
					PlayerDirectionalLocomotionEventArgs.NameSpace
				)
				.Subscribe( OnDirectionalLocomotion );

			_cameraStatusChanged = eventFactory
				.GetEvent<PlayerCameraStatusChangedEventArgs>(
					PlayerCameraStatusChangedEventArgs.Name,
					PlayerCameraStatusChangedEventArgs.NameSpace
				)
				.Subscribe( OnCameraStatusChanged );
		}

		protected override void Dispose( bool disposing )
		{
			if ( !disposing ) {
				return;
			}
			base.Dispose( disposing );

			_locomotionCue.Dispose();
			_directionalLocomotion.Dispose();
			_cameraStatusChanged.Dispose();
		}

		public override void Render( float delta )
		{
			base.Render( delta );

			AdvanceTransientState( delta );
			UpdateLineOfFireObstruction( delta );

			float targetSpread = BASE_SPREAD_PIXELS
				+ (_movementAmount * MOVEMENT_SPREAD_PIXELS)
				+ (MathF.Abs( _strafeAmount ) * STRAFE_SPREAD_PIXELS)
				+ (_backpedalAmount * BACKPEDAL_SPREAD_PIXELS)
				+ (_cameraImpulse * CAMERA_SPREAD_PIXELS)
				+ (_cueImpulse * IMPULSE_SPREAD_PIXELS)
				+ (_obstructionAmount * OBSTRUCTED_SPREAD_PIXELS);

			_spreadPixels = Damp( _spreadPixels, targetSpread, SPREAD_RESPONSE, delta );

			float targetSettle = 1.0f - Mathf.Clamp(
				(_movementAmount * 0.45f) + (_cameraImpulse * 0.55f) + (_cueImpulse * 0.035f) + (_obstructionAmount * 0.35f),
				0.0f,
				1.0f
			);
			_settle = Damp( _settle, targetSettle, SETTLE_RESPONSE, delta );

			Vector2 locomotionOffset = new Vector2(
				_strafeAmount * 3.25f,
				_backpedalAmount * 2.0f
			);
			Vector2 targetOffset = ClampLength( locomotionOffset + _cameraKick + _cueKick, MAX_OFFSET_PIXELS );
			_offsetPixels = Damp( _offsetPixels, targetOffset, OFFSET_RESPONSE, delta );

			view.SetReticle( _spreadPixels, _settle, 1.0f, _offsetPixels );
			view.SetLineOfFireObstruction( _obstructionAmount );
		}

		public void AddWeaponBloom( float pixels )
		{
			_cueImpulse = MathF.Max( _cueImpulse, MathF.Max( 0.0f, pixels ) );
		}

		private void OnLocomotionCue( in PlayerLocomotionCueEventArgs args )
		{
			if ( args.PlayerId != _playerId ) {
				return;
			}

			float impulse = args.Cue switch {
				PlayerLocomotionCue.HardStart => 4.0f,
				PlayerLocomotionCue.HardStop => 5.5f,
				PlayerLocomotionCue.Reverse => 8.0f,
				PlayerLocomotionCue.SharpTurn => 6.5f,
				_ => 0.0f
			};

			if ( impulse <= 0.0f ) {
				return;
			}

			_cueImpulse = MathF.Max( _cueImpulse, impulse );
			_cueKick += ToGodot( args.MoveInput ) * (impulse * 0.22f);
			_cueKick = ClampLength( _cueKick, MAX_OFFSET_PIXELS );
		}

		private void OnDirectionalLocomotion( in PlayerDirectionalLocomotionEventArgs args )
		{
			if ( args.PlayerId != _playerId ) {
				return;
			}

			_locomotionHold = LOCOMOTION_HOLD_SECONDS;

			if ( !args.IsMoving || args.Direction == PlayerLocomotionDirection.Idle ) {
				_movementAmount = 0.0f;
				_strafeAmount = 0.0f;
				_backpedalAmount = 0.0f;
				return;
			}

			_movementAmount = Mathf.Clamp( MathF.Abs( args.ForwardAmount ) + (MathF.Abs( args.RightAmount ) * 0.85f), 0.0f, 1.0f );
			_strafeAmount = Mathf.Clamp( args.RightAmount, -1.0f, 1.0f );
			_backpedalAmount = Mathf.Clamp( -args.ForwardAmount, 0.0f, 1.0f );

			if ( args.Direction == PlayerLocomotionDirection.Running180 ) {
				_cueImpulse = MathF.Max( _cueImpulse, 9.0f );
			}
		}

		private void OnCameraStatusChanged( in PlayerCameraStatusChangedEventArgs args )
		{
			if ( args.PlayerId != _playerId ) {
				return;
			}

			NumericsVector3 forward = SafeNormalize( args.Forward, new NumericsVector3( 0.0f, 0.0f, -1.0f ) );
			NumericsVector3 right = SafeNormalize( args.Right, new NumericsVector3( 1.0f, 0.0f, 0.0f ) );

			_cameraOrigin = ToGodot( args.Origin );
			_cameraForward = ToGodot( forward );
			_cameraRight = ToGodot( right );
			_hasCameraPose = true;

			if ( !_hasCamera ) {
				_lastCameraForward = forward;
				_lastCameraRight = right;
				_hasCamera = true;
				return;
			}

			float dot = Math.Clamp( NumericsVector3.Dot( _lastCameraForward, forward ), CAMERA_DOT_MIN, CAMERA_DOT_MAX );
			float angularDelta = MathF.Acos( dot );

			float lateralDelta = NumericsVector3.Dot( _lastCameraForward, right );
			float verticalDelta = forward.Y - _lastCameraForward.Y;

			_cameraImpulse = MathF.Max( _cameraImpulse, Mathf.Clamp( angularDelta * 5.5f, 0.0f, 1.0f ) );
			_cameraKick += new Vector2( -lateralDelta, verticalDelta ) * 9.0f;
			_cameraKick = ClampLength( _cameraKick, MAX_OFFSET_PIXELS );

			_lastCameraForward = forward;
			_lastCameraRight = right;
		}

		private void AdvanceTransientState( float delta )
		{
			_locomotionHold -= delta;
			if ( _locomotionHold <= 0.0f ) {
				_movementAmount = MoveToward( _movementAmount, 0.0f, delta * 5.0f );
				_strafeAmount = MoveToward( _strafeAmount, 0.0f, delta * 5.0f );
				_backpedalAmount = MoveToward( _backpedalAmount, 0.0f, delta * 5.0f );
			}

			_cameraImpulse = MoveToward( _cameraImpulse, 0.0f, delta * CAMERA_DECAY );
			_cueImpulse = MoveToward( _cueImpulse, 0.0f, delta * CUE_DECAY );
			_cameraKick = Damp( _cameraKick, Vector2.Zero, KICK_DECAY, delta );
			_cueKick = Damp( _cueKick, Vector2.Zero, KICK_DECAY, delta );
		}

		private void UpdateLineOfFireObstruction( float delta )
		{
			float target = IsLineOfFireObstructed() ? 1.0f : 0.0f;
			_obstructionAmount = Damp( _obstructionAmount, target, OBSTRUCTION_RESPONSE, delta );
		}

		private bool IsLineOfFireObstructed()
		{
			if ( !_hasCameraPose || !view.LineOfFireObstructionEnabled ) {
				return false;
			}

			Viewport viewport = view.GetViewport();
			if ( viewport == null ) {
				return false;
			}

			World3D? world = viewport.World3D;
			PhysicsDirectSpaceState3D? space = world?.DirectSpaceState;
			if ( space == null ) {
				return false;
			}

			Vector3 forward = SafeNormalize( _cameraForward, new Vector3( 0.0f, 0.0f, -1.0f ) );
			Vector3 right = SafeNormalize( _cameraRight, new Vector3( 1.0f, 0.0f, 0.0f ) );
			Vector3 up = SafeNormalize( right.Cross( forward ), new Vector3( 0.0f, 1.0f, 0.0f ) );

			Vector3 cameraEnd = _cameraOrigin + (forward * view.LineOfFireRange);
			Vector3 aimPoint = ResolveAimPoint( space, _cameraOrigin, cameraEnd );
			Vector3 muzzleOrigin = _cameraOrigin
				+ (right * view.LineOfFireMuzzleRightOffset)
				+ (up * view.LineOfFireMuzzleUpOffset)
				+ (forward * view.LineOfFireMuzzleForwardOffset);

			float aimDistance = muzzleOrigin.DistanceTo( aimPoint );
			if ( aimDistance <= MIN_RAY_DISTANCE ) {
				return false;
			}

			var query = PhysicsRayQueryParameters3D.Create( muzzleOrigin, aimPoint );
			query.CollisionMask = view.LineOfFireCollisionMask;
			query.CollideWithAreas = view.LineOfFireCollideWithAreas;
			query.CollideWithBodies = view.LineOfFireCollideWithBodies;

			Godot.Collections.Dictionary hit = space.IntersectRay( query );
			if ( hit.Count == 0 || !hit.ContainsKey( "position" ) ) {
				return false;
			}

			Vector3 hitPosition = hit["position"].AsVector3();
			float hitDistance = muzzleOrigin.DistanceTo( hitPosition );
			float endpointTolerance = MathF.Max( 0.0f, view.LineOfFireEndpointTolerance );

			return hitDistance < aimDistance - endpointTolerance;
		}

		private Vector3 ResolveAimPoint( PhysicsDirectSpaceState3D space, Vector3 origin, Vector3 end )
		{
			var query = PhysicsRayQueryParameters3D.Create( origin, end );
			query.CollisionMask = view.LineOfFireCollisionMask;
			query.CollideWithAreas = view.LineOfFireCollideWithAreas;
			query.CollideWithBodies = view.LineOfFireCollideWithBodies;

			Godot.Collections.Dictionary hit = space.IntersectRay( query );
			if ( hit.Count == 0 || !hit.ContainsKey( "position" ) ) {
				return end;
			}

			return hit["position"].AsVector3();
		}

		private static NumericsVector3 SafeNormalize( NumericsVector3 value, NumericsVector3 fallback )
		{
			return value.LengthSquared() > 0.0001f
				? NumericsVector3.Normalize( value )
				: fallback;
		}

		private static Vector3 SafeNormalize( Vector3 value, Vector3 fallback )
		{
			return value.LengthSquared() > 0.0001f
				? value.Normalized()
				: fallback;
		}

		private static Vector2 ToGodot( NumericsVector2 value )
		{
			return new Vector2( value.X, value.Y );
		}

		private static Vector3 ToGodot( NumericsVector3 value )
		{
			return new Vector3( value.X, value.Y, value.Z );
		}

		private static float Damp( float current, float target, float response, float delta )
		{
			return Mathf.Lerp( current, target, 1.0f - MathF.Exp( -response * delta ) );
		}

		private static Vector2 Damp( Vector2 current, Vector2 target, float response, float delta )
		{
			return current.Lerp( target, 1.0f - MathF.Exp( -response * delta ) );
		}

		private static float MoveToward( float current, float target, float delta )
		{
			if ( current < target ) {
				return MathF.Min( current + delta, target );
			}
			return MathF.Max( current - delta, target );
		}

		private static Vector2 ClampLength( Vector2 value, float maxLength )
		{
			return value.LengthSquared() > maxLength * maxLength
				? value.Normalized() * maxLength
				: value;
		}
	};
};
