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
using Nomad.Game.Sdk.Player.Input;
using Nomad.Game.Sdk.Player.Movement;
using NumericsVector2 = System.Numerics.Vector2;

namespace Nomad.Game.Gameplay.Player.Movement
{
	/*
	===================================================================================

	PlayerMovementFrame

	===================================================================================
	*/
	/// <summary>
	/// Mutable per-physics-frame movement transaction. Controllers mutate this value in
	/// sequence, then PlayerMovementBodyMotor performs the single CharacterBody3D commit.
	/// Frequently used derived values are cached here to avoid repeated hot-path queries.
	/// </summary>

	internal struct PlayerMovementFrame
	{
		public PlayerInputFrame Input;
		public uint Tick;
		public float DeltaTime;

		public bool CanDriveMovement;
		public bool IsGrounded;
		public bool HasMoveInput;

		public NumericsVector2 MoveInput;
		public float MoveInputLengthSquared;

		public Vector3 PreviousBodyVelocity;
		public float PreviousBodySpeedSquared;
		public Vector3 BodyVelocity;
		public Vector3 RequestedBodyVelocity;
		public float BodySpeedSquared;

		public Vector3 PreviousHorizontalVelocity;
		public float PreviousHorizontalSpeedSquared;
		public Vector3 HorizontalVelocity;
		public Vector3 RequestedHorizontalVelocity;
		public float HorizontalSpeedSquared;

		public Vector3 WishDirection;
		public float WishDirectionLengthSquared;
		public Vector3 FacingDirection;
		public float FacingDirectionLengthSquared;
		public Vector3 LastPlanarWishDirection;

		public Vector3 WallNormal;
		public Vector3 WallRunDirection;
		public WallMountedMoveMode WallMountedMode;

		public PlayerMovementMode PreviousMode;
		public PlayerMovementMode Mode;
		public PlayerMovementActionFlags ActionFlags;
		public PlayerMovementCommitKind CommitKind;
		public PlayerMovementFrameDirtyFlags DirtyFlags;

		public bool MovementHandled;
		public bool ShouldPublishLocomotion;
		public bool ShouldResetDirectionalSnapshot;
		public bool ForceFacingUpdate;

		public bool IsDashing => (ActionFlags & PlayerMovementActionFlags.Dashing) != 0;
		public bool IsSliding => (ActionFlags & PlayerMovementActionFlags.Sliding) != 0;

		public void Begin(
			in PlayerInputFrame input,
			float deltaTime,
			Vector3 bodyVelocity,
			bool isGrounded,
			bool canDriveMovement,
			Vector3 previousLastPlanarWishDirection,
			PlayerMovementMode previousMode
		)
		{
			Input = input;
			Tick = input.Tick;
			DeltaTime = deltaTime;
			CanDriveMovement = canDriveMovement;
			IsGrounded = isGrounded;

			PreviousBodyVelocity = BodyVelocity;
			PreviousBodySpeedSquared = BodySpeedSquared;
			PreviousHorizontalVelocity = HorizontalVelocity;
			PreviousHorizontalSpeedSquared = HorizontalSpeedSquared;
			PreviousMode = previousMode;

			BodyVelocity = bodyVelocity;
			RequestedBodyVelocity = bodyVelocity;
			BodySpeedSquared = bodyVelocity.LengthSquared();

			HorizontalVelocity = PlayerMovementMath.ToPlanar( bodyVelocity );
			RequestedHorizontalVelocity = HorizontalVelocity;
			HorizontalSpeedSquared = HorizontalVelocity.LengthSquared();

			MoveInput = ClampInput( input.Move, out MoveInputLengthSquared );
			HasMoveInput = MoveInputLengthSquared > PlayerMovementSettings.MOVING_THRESHOLD;

			WishDirection = Vector3.Zero;
			WishDirectionLengthSquared = 0.0f;
			FacingDirection = Vector3.Zero;
			FacingDirectionLengthSquared = 0.0f;
			LastPlanarWishDirection = previousLastPlanarWishDirection;

			WallNormal = Vector3.Zero;
			WallRunDirection = Vector3.Zero;
			WallMountedMode = WallMountedMoveMode.None;

			Mode = isGrounded ? PlayerMovementMode.Ground : PlayerMovementMode.Falling;
			ActionFlags = PlayerMovementActionFlags.None;
			CommitKind = PlayerMovementCommitKind.None;
			DirtyFlags = PlayerMovementFrameDirtyFlags.Input;

			MovementHandled = false;
			ShouldPublishLocomotion = false;
			ShouldResetDirectionalSnapshot = false;
			ForceFacingUpdate = false;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public bool IsDirty( PlayerMovementFrameDirtyFlags flags )
		{
			return (DirtyFlags & flags) != 0;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void MarkDirty( PlayerMovementFrameDirtyFlags flags )
		{
			DirtyFlags |= flags;
		}

		public void SetActionFlags( PlayerMovementActionFlags actionFlags )
		{
			if ( ActionFlags == actionFlags ) {
				return;
			}

			ActionFlags = actionFlags;
			MarkDirty( PlayerMovementFrameDirtyFlags.Flags );
		}

		public void AddActionFlags( PlayerMovementActionFlags actionFlags )
		{
			SetActionFlags( ActionFlags | actionFlags );
		}

		public void RemoveActionFlags( PlayerMovementActionFlags actionFlags )
		{
			SetActionFlags( ActionFlags & ~actionFlags );
		}

		public void SetMode( PlayerMovementMode mode )
		{
			if ( Mode == mode ) {
				return;
			}

			Mode = mode;
			MarkDirty( PlayerMovementFrameDirtyFlags.MovementMode );
		}

		public void SetWishDirection( Vector3 wishDirection )
		{
			wishDirection.Y = 0.0f;
			WishDirection = SafePlanarNormalized( wishDirection, Vector3.Zero, out WishDirectionLengthSquared );
			if ( WishDirectionLengthSquared > PlayerMovementSettings.EPSILON ) {
				LastPlanarWishDirection = WishDirection;
			}

			MarkDirty( PlayerMovementFrameDirtyFlags.WishDirection );
		}

		public void SetFacingDirection( Vector3 facingDirection, bool force = false )
		{
			FacingDirection = SafePlanarNormalized( facingDirection, Vector3.Zero, out FacingDirectionLengthSquared );
			ForceFacingUpdate |= force;
			MarkDirty( PlayerMovementFrameDirtyFlags.FacingDirection );
		}

		public void SetHorizontalVelocity( Vector3 horizontalVelocity )
		{
			horizontalVelocity.Y = 0.0f;
			HorizontalVelocity = horizontalVelocity;
			HorizontalSpeedSquared = horizontalVelocity.LengthSquared();
			MarkDirty( PlayerMovementFrameDirtyFlags.HorizontalVelocity | PlayerMovementFrameDirtyFlags.Locomotion );
		}

		public void SetBodyVelocity( Vector3 bodyVelocity )
		{
			BodyVelocity = bodyVelocity;
			BodySpeedSquared = bodyVelocity.LengthSquared();
			SetHorizontalVelocity( PlayerMovementMath.ToPlanar( bodyVelocity ) );
			MarkDirty( PlayerMovementFrameDirtyFlags.BodyVelocity );
		}

		public void RequestGroundVelocity( Vector3 horizontalVelocity, PlayerMovementMode mode )
		{
			horizontalVelocity.Y = 0.0f;
			RequestedHorizontalVelocity = horizontalVelocity;
			SetHorizontalVelocity( horizontalVelocity );
			SetMode( mode );
			CommitKind = PlayerMovementCommitKind.GroundVelocity;
			MovementHandled = true;
			ShouldPublishLocomotion = true;
			MarkDirty( PlayerMovementFrameDirtyFlags.RequestedCommit );
		}

		public void RequestBodyVelocity( Vector3 bodyVelocity, PlayerMovementMode mode )
		{
			RequestedBodyVelocity = bodyVelocity;
			SetBodyVelocity( bodyVelocity );
			SetMode( mode );
			CommitKind = PlayerMovementCommitKind.DirectBodyVelocity;
			MovementHandled = true;
			ShouldPublishLocomotion = true;
			MarkDirty( PlayerMovementFrameDirtyFlags.RequestedCommit );
		}

		public void RequestLockedVelocity()
		{
			RequestedHorizontalVelocity = Vector3.Zero;
			SetHorizontalVelocity( Vector3.Zero );
			SetMode( PlayerMovementMode.Locked );
			CommitKind = PlayerMovementCommitKind.LockedBodyVelocity;
			MovementHandled = true;
			ShouldPublishLocomotion = true;
			ShouldResetDirectionalSnapshot = true;
			MarkDirty( PlayerMovementFrameDirtyFlags.RequestedCommit );
		}

		public void MarkParkourHandled( Vector3 bodyVelocity )
		{
			SetBodyVelocity( bodyVelocity );
			SetMode( PlayerMovementMode.Parkour );
			CommitKind = PlayerMovementCommitKind.None;
			MovementHandled = true;
			ShouldPublishLocomotion = false;
			ShouldResetDirectionalSnapshot = true;
		}

		public void SetWallRunState( Vector3 normal, Vector3 runDirection, WallMountedMoveMode mode )
		{
			WallNormal = normal;
			WallRunDirection = runDirection;
			WallMountedMode = mode;
			MarkDirty( PlayerMovementFrameDirtyFlags.WallRun );
		}

		public void SynchronizeCommittedBodyVelocity( Vector3 bodyVelocity )
		{
			SetBodyVelocity( bodyVelocity );
			CommitKind = PlayerMovementCommitKind.None;
			MarkDirty( PlayerMovementFrameDirtyFlags.BodyCommitted );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private static NumericsVector2 ClampInput( NumericsVector2 input, out float lengthSquared )
		{
			lengthSquared = input.LengthSquared();
			if ( lengthSquared <= 1.0f ) {
				return input;
			}

			NumericsVector2 normalized = NumericsVector2.Normalize( input );
			lengthSquared = 1.0f;
			return normalized;
		}

		private static Vector3 SafePlanarNormalized( Vector3 value, Vector3 fallback, out float normalizedLengthSquared )
		{
			value.Y = 0.0f;
			float lengthSquared = value.LengthSquared();
			if ( lengthSquared > PlayerMovementSettings.EPSILON ) {
				normalizedLengthSquared = 1.0f;
				return value / MathF.Sqrt( lengthSquared );
			}

			fallback.Y = 0.0f;
			lengthSquared = fallback.LengthSquared();
			if ( lengthSquared > PlayerMovementSettings.EPSILON ) {
				normalizedLengthSquared = 1.0f;
				return fallback / MathF.Sqrt( lengthSquared );
			}

			normalizedLengthSquared = 0.0f;
			return Vector3.Zero;
		}
	}
}
