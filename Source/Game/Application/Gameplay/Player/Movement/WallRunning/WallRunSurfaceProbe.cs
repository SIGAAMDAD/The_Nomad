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

namespace Nomad.Game.Application.Gameplay.Player.Movement.WallRunning
{
	/*
	===================================================================================

	WallRunSurfaceProbe

	===================================================================================
	*/
	/// <summary>
	/// Owns all physics queries used by wall-running. Query objects and exclude arrays are
	/// allocated once and reused to keep the physics tick predictable.
	/// </summary>

	internal sealed class WallRunSurfaceProbe : IDisposable
	{
		private const float ForwardBias = 0.55f;
		private const float SideBias = 0.35f;
		private const float DiagonalBias = 0.18f;

		private readonly PlayerPrefab _prefab;
		private readonly WallRunSettings _settings;
		private readonly WallRunSurfaceClassifier _classifier;
		private readonly PhysicsRayQueryParameters3D _rayQuery = new();
		private readonly Godot.Collections.Array<Rid> _rayExclude = new();

		private bool _isDisposed;

		public WallRunSurfaceProbe(
			PlayerPrefab prefab,
			WallRunSettings settings,
			WallRunSurfaceClassifier classifier
		)
		{
			_prefab = prefab ?? throw new ArgumentNullException( nameof( prefab ) );
			_settings = settings ?? throw new ArgumentNullException( nameof( settings ) );
			_classifier = classifier ?? throw new ArgumentNullException( nameof( classifier ) );

			_rayExclude.Add( _prefab.GetRid() );
			_rayQuery.CollideWithAreas = false;
			_rayQuery.CollideWithBodies = true;
			_rayQuery.Exclude = _rayExclude;
			_rayQuery.CollisionMask = _settings.SurfaceMask;
		}

		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			_rayQuery.Dispose();
			GC.SuppressFinalize( this );
			_isDisposed = true;
		}

		public bool TryFindBestSurface( in WallRunProbeRequest request, out WallSurfaceCandidate best )
		{
			best = default;
			bool found = false;
			float bestScore = float.NegativeInfinity;

			Vector3 source = request.PlanarWishDirection.LengthSquared() > WallRunMath.Epsilon
				? request.PlanarWishDirection
				: request.HorizontalVelocity;
			Vector3 forward = WallRunMath.SafePlanarNormalized( source, request.FallbackForward );
			Vector3 right = WallRunMath.PlanarRight( forward );

			TryScoreRay( forward, _settings.ForwardProbeDistance, ForwardBias, request.HorizontalVelocity, request.FallbackForward, ref found, ref bestScore, ref best );
			TryScoreRay( right, _settings.SideProbeDistance, SideBias, request.HorizontalVelocity, request.FallbackForward, ref found, ref bestScore, ref best );
			TryScoreRay( -right, _settings.SideProbeDistance, SideBias, request.HorizontalVelocity, request.FallbackForward, ref found, ref bestScore, ref best );
			TryScoreRay( WallRunMath.SafePlanarNormalized( forward + right, forward ), _settings.SideProbeDistance, DiagonalBias, request.HorizontalVelocity, request.FallbackForward, ref found, ref bestScore, ref best );
			TryScoreRay( WallRunMath.SafePlanarNormalized( forward - right, forward ), _settings.SideProbeDistance, DiagonalBias, request.HorizontalVelocity, request.FallbackForward, ref found, ref bestScore, ref best );

			return found;
		}

		public bool TryRefreshSurface(
			in WallSurfaceCandidate current,
			in WallRunProbeRequest request,
			out WallSurfaceCandidate candidate
		)
		{
			candidate = default;

			Vector3 towardWall = -current.Normal;
			if ( TryCastSurface( towardWall, _settings.SurfaceRetainDistance, out candidate, 1 ) ) {
				return true;
			}

			return TryFindBestSurface( request, out candidate );
		}

		private void TryScoreRay(
			Vector3 direction,
			float distance,
			float directionalBias,
			Vector3 horizontalVelocity,
			Vector3 fallbackForward,
			ref bool found,
			ref float bestScore,
			ref WallSurfaceCandidate best
		)
		{
			if ( !TryCastSurface( direction, distance, out WallSurfaceCandidate candidate, 0 ) ) {
				return;
			}

			Vector3 velocityDirection = horizontalVelocity.LengthSquared() > WallRunMath.Epsilon
				? WallRunMath.SafePlanarNormalized( horizontalVelocity, fallbackForward )
				: WallRunMath.SafePlanarNormalized( fallbackForward, Vector3.Forward );
			float tangentScore = WallRunMath.ProjectOnPlane( velocityDirection, candidate.Normal ).Length();
			float score =
				candidate.Priority * 2.0f +
				candidate.Verticality * 1.6f +
				tangentScore * 0.9f +
				directionalBias -
				candidate.Distance * 0.35f;

			if ( score <= bestScore ) {
				return;
			}

			bestScore = score;
			best = candidate;
			found = true;
		}

		private bool TryCastSurface( Vector3 direction, float distance, out WallSurfaceCandidate candidate, int priorityBias )
		{
			candidate = default;

			if ( direction.LengthSquared() <= WallRunMath.Epsilon ) {
				return false;
			}

			direction = WallRunMath.SafeNormalized( direction, Vector3.Forward );
			Vector3 playerOrigin = _prefab.GlobalPosition;
			Vector3 chestOrigin = playerOrigin + Vector3.Up * _settings.ChestProbeHeight;

			if ( TryCastSurfaceFrom( chestOrigin, direction, distance, out candidate, priorityBias ) ) {
				return true;
			}

			Vector3 hipOrigin = playerOrigin + Vector3.Up * _settings.HipProbeHeight;
			return TryCastSurfaceFrom( hipOrigin, direction, distance, out candidate, priorityBias );
		}

		private bool TryCastSurfaceFrom(
			Vector3 origin,
			Vector3 direction,
			float distance,
			out WallSurfaceCandidate candidate,
			int priorityBias
		)
		{
			candidate = default;

			_rayQuery.From = origin;
			_rayQuery.To = origin + direction * distance;
			_rayQuery.CollisionMask = _settings.SurfaceMask;

			Godot.Collections.Dictionary hit = _prefab.GetWorld3D().DirectSpaceState.IntersectRay( _rayQuery );
			return _classifier.TryClassify( hit, origin, priorityBias, out candidate );
		}
	}
}
