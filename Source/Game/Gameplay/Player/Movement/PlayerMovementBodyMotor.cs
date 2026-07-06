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
using Nomad.Core.Numerics;
using Nomad.EngineUtils;
using Nomad.Game.Prefabs;

namespace Nomad.Game.Gameplay.Player.Movement
{
	internal sealed class PlayerMovementBodyMotor
	{
		private readonly PlayerPrefab _prefab;
		private readonly PlayerMovementRuntime _runtime;

		public PlayerMovementBodyMotor( PlayerPrefab prefab, PlayerMovementRuntime runtime )
		{
			_prefab = prefab ?? throw new ArgumentNullException( nameof( prefab ) );
			_runtime = runtime ?? throw new ArgumentNullException( nameof( runtime ) );
		}

		public void ConfigureBody()
		{
			_prefab.UpDirection = Godot.Vector3.Up;
			_prefab.FloorSnapLength = PlayerMovementSettings.FLOOR_SNAP_LENGTH;
			_prefab.FloorMaxAngle = AngleMath.ToRadians( PlayerMovementSettings.FLOOR_MAX_ANGLE_DEGREES );
			_prefab.SlideOnCeiling = true;
		}

		public bool CommitFrame( ref PlayerMovementFrame frame )
		{
			if ( frame.CommitKind == PlayerMovementCommitKind.None ) {
				_runtime.SynchronizeCommittedBodyVelocity( _prefab.Velocity.ToSystem() );
				return false;
			}

			Vector3 velocity;
			if ( frame.CommitKind == PlayerMovementCommitKind.GroundVelocity ) {
				velocity = BuildGroundVelocity( ref frame );
			} else if ( frame.CommitKind == PlayerMovementCommitKind.DirectBodyVelocity ) {
				velocity = frame.RequestedBodyVelocity;
			} else if ( frame.CommitKind == PlayerMovementCommitKind.LockedBodyVelocity ) {
				velocity = BuildLockedVelocity( ref frame );
			} else {
				velocity = _prefab.Velocity.ToSystem();
			}

			_prefab.Velocity = velocity.ToGodot();
			_prefab.MoveAndSlide();

			Vector3 committedVelocity = _prefab.Velocity.ToSystem();
			if ( frame.Mode == PlayerMovementMode.WallRun && frame.WallNormal.LengthSquared() > PlayerMovementSettings.EPSILON ) {
				committedVelocity = RemovePositiveNormalComponent( committedVelocity, frame.WallNormal );
				_prefab.Velocity = committedVelocity.ToGodot();
			}

			_runtime.SynchronizeCommittedBodyVelocity( committedVelocity );
			return true;
		}

		private Vector3 BuildGroundVelocity( ref PlayerMovementFrame frame )
		{
			Vector3 velocity = frame.BodyVelocity;
			velocity.X = frame.RequestedHorizontalVelocity.X;
			velocity.Z = frame.RequestedHorizontalVelocity.Z;

			if ( frame.IsGrounded ) {
				if ( velocity.Y < 0.0f ) {
					velocity.Y = PlayerMovementSettings.GROUND_STICK_VELOCITY;
				}
			} else {
				velocity.Y -= PlayerMovementSettings.GRAVITY * frame.DeltaTime;
			}

			return velocity;
		}

		private Vector3 BuildLockedVelocity( ref PlayerMovementFrame frame )
		{
			Vector3 velocity = frame.BodyVelocity;
			velocity.X = 0.0f;
			velocity.Z = 0.0f;
			if ( frame.IsGrounded ) {
				velocity.Y = PlayerMovementSettings.GROUND_STICK_VELOCITY;
			}

			return velocity;
		}

		private static Vector3 RemovePositiveNormalComponent( Vector3 velocity, Vector3 normal )
		{
			float dot = Vector3.Dot( velocity, normal );
			return dot > 0.0f ? velocity - normal * dot : velocity;
		}
	};
};
