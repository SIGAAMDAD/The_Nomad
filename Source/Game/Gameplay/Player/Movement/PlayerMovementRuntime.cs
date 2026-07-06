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
using Nomad.Game.Sdk.Player.Input;
using Nomad.Game.Sdk.Player.Movement;

namespace Nomad.Game.Gameplay.Player.Movement
{
	/*
	===================================================================================

	PlayerMovementRuntime

	===================================================================================
	*/
	/// <summary>
	/// Shared movement runtime cache. Previous/Current frame snapshots provide stable
	/// frame-start data and one in-flight transaction for all movement controllers.
	/// </summary>

	internal sealed class PlayerMovementRuntime
	{
		private Vector3 _lastPlanarWishDirection = new Vector3( 0.0f, 0.0f, -1.0f );

		public PlayerMovementFrame Previous;
		public PlayerMovementFrame Current;
		public PlayerMovementActionRuntime Actions;

		public Vector3 HorizontalVelocity {
			get => Current.HorizontalVelocity;
			set => Current.SetHorizontalVelocity( value );
		}

		public Vector3 LastPlanarWishDirection {
			get => _lastPlanarWishDirection;
			set {
				Vector3 normalized = PlayerMovementMath.NormalizePlanarOrDefault( value, _lastPlanarWishDirection );
				if ( normalized.LengthSquared() > PlayerMovementSettings.EPSILON ) {
					_lastPlanarWishDirection = normalized;
					Current.LastPlanarWishDirection = normalized;
				}
			}
		}

		public Vector3 DashDirection {
			get => Actions.DashDirection;
			set => Actions.SetDashDirection( value );
		}

		public Vector3 SlideDirection {
			get => Actions.SlideDirection;
			set => Actions.SetSlideDirection( value );
		}

		public Vector3 WallNormal {
			get => Current.WallNormal;
			set {
				Current.WallNormal = value;
				Current.MarkDirty( PlayerMovementFrameDirtyFlags.WallRun );
			}
		}

		public Vector3 WallRunDirection {
			get => Current.WallRunDirection;
			set {
				Current.WallRunDirection = value;
				Current.MarkDirty( PlayerMovementFrameDirtyFlags.WallRun );
			}
		}

		public WallMountedMoveMode WallMountedMode {
			get => Current.WallMountedMode;
			set {
				Current.WallMountedMode = value;
				Current.MarkDirty( PlayerMovementFrameDirtyFlags.WallRun );
			}
		}

		public void BeginFrame(
			in PlayerInputFrame input,
			float deltaTime,
			Vector3 bodyVelocity,
			bool isGrounded,
			bool canDriveMovement
		)
		{
			Previous = Current;
			Current.Begin(
				input,
				deltaTime,
				bodyVelocity,
				isGrounded,
				canDriveMovement,
				_lastPlanarWishDirection,
				Previous.Mode
			);
		}

		public void EndFrame()
		{
			if ( Current.LastPlanarWishDirection.LengthSquared() > PlayerMovementSettings.EPSILON ) {
				_lastPlanarWishDirection = Current.LastPlanarWishDirection;
			}
		}

		public void SynchronizeCommittedBodyVelocity( Vector3 bodyVelocity )
		{
			Current.SynchronizeCommittedBodyVelocity( bodyVelocity );
		}

		public void ClearWallRun()
		{
			Current.SetWallRunState( Vector3.Zero, Vector3.Zero, WallMountedMoveMode.None );
		}
	};

	internal struct PlayerMovementActionRuntime
	{
		public Vector3 DashDirection;
		public Vector3 SlideDirection;
		public float SlideTimeRemaining;

		public void SetDashDirection( Vector3 direction )
		{
			DashDirection = PlayerMovementMath.NormalizePlanarOrDefault( direction, new Vector3( 0.0f, 0.0f, -1.0f ) );
		}

		public void SetSlideDirection( Vector3 direction )
		{
			SlideDirection = PlayerMovementMath.NormalizePlanarOrDefault( direction, new Vector3( 0.0f, 0.0f, -1.0f ) );
		}
	};
};
