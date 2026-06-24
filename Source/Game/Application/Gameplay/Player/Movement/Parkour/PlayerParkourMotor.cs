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
using Nomad.Game.Prefabs;
using Nomad.Game.Sdk.Traversal;

namespace Nomad.Game.Application.Gameplay.Player.Movement.Parkour
{
	internal sealed class PlayerParkourMotor
	{
		private readonly PlayerPrefab _prefab;
		private readonly PlayerParkourSettings _settings;

		public PlayerParkourMotor( PlayerPrefab prefab, PlayerParkourSettings settings )
		{
			_prefab = prefab ?? throw new ArgumentNullException( nameof( prefab ) );
			_settings = settings ?? throw new ArgumentNullException( nameof( settings ) );
		}

		public void SnapBodyToAnchor( in TraversalAnchor anchor, float dt )
		{
			float alpha = Mathf.Clamp( _settings.AnchorSnapSpeed * dt, 0.0f, 1.0f );
			_prefab.GlobalPosition = _prefab.GlobalPosition.Lerp( PlayerParkourMath.AnchorPosition( anchor ), alpha );
		}

		public void AttachImmediate( in TraversalAnchor anchor )
		{
			_prefab.GlobalPosition = PlayerParkourMath.AnchorPosition( anchor );
			_prefab.Velocity = Vector3.Zero;
			FaceWallImmediate( PlayerParkourMath.AnchorNormal( anchor ) );
		}

		public void FaceWall( Vector3 wallNormal, float dt )
		{
			Vector3 forward = -wallNormal;
			forward.Y = 0.0f;

			if ( forward.LengthSquared() < 0.0001f ) {
				return;
			}

			forward = forward.Normalized();
			Vector3 currentForward = -_prefab.GlobalTransform.Basis.Z;
			currentForward.Y = 0.0f;

			if ( currentForward.LengthSquared() < 0.0001f ) {
				FaceWallImmediate( wallNormal );
				return;
			}

			currentForward = currentForward.Normalized();
			float alpha = Mathf.Clamp( _settings.TurnToWallSpeed * dt, 0.0f, 1.0f );
			Vector3 blended = currentForward.Slerp( forward, alpha ).Normalized();

			_prefab.LookAt( _prefab.GlobalPosition + blended, Vector3.Up );
		}

		public void FaceWallImmediate( Vector3 wallNormal )
		{
			Vector3 forward = -wallNormal;
			forward.Y = 0.0f;

			if ( forward.LengthSquared() < 0.0001f ) {
				return;
			}

			_prefab.LookAt( _prefab.GlobalPosition + forward.Normalized(), Vector3.Up );
		}

		public void TraverseEdge( PlayerParkourRuntime runtime, float dt )
		{
			float t = Mathf.Clamp( runtime.EdgeElapsed / Mathf.Max( runtime.EdgeDuration, _settings.MinimumEdgeDuration ), 0.0f, 1.0f );
			float eased = PlayerParkourMath.SmoothStep( t );

			Vector3 targetPosition = runtime.EdgeStart.Lerp( runtime.EdgeEnd, eased );
			Vector3 targetNormal = PlayerParkourMath.SafeNormalized(
				runtime.EdgeStartNormal.Lerp( runtime.EdgeEndNormal, eased ),
				runtime.EdgeStartNormal
			);

			Vector3 delta = targetPosition - _prefab.GlobalPosition;
			_prefab.Velocity = delta / Mathf.Max( dt, 0.0001f );
			_prefab.MoveAndSlide();
			FaceWall( targetNormal, dt );
		}

		public bool IsNearEdgeEnd( PlayerParkourRuntime runtime )
		{
			float edgeSnapDistanceSq = _settings.EdgeSnapDistance * _settings.EdgeSnapDistance;
			return _prefab.GlobalPosition.DistanceSquaredTo( runtime.EdgeEnd ) <= edgeSnapDistanceSq;
		}

		public void DropFromAnchor( in TraversalAnchor anchor )
		{
			_prefab.Velocity = PlayerParkourMath.AnchorNormal( anchor ) * _settings.DropPushOffSpeed + Vector3.Down * _settings.DropDownSpeed;
		}
	}
}
