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
using System.Runtime.CompilerServices;
using Godot;
using Nomad.EngineUtils;
using Nomad.Game.Sdk.Traversal;

namespace Nomad.Game.Application.Gameplay.Traversal
{
	internal sealed class TraversalSolver
	{
		private readonly TraversalGraph _graph;
		private readonly TraversalQueryBuffer _queryBuffer;

		public TraversalSolver( TraversalGraph graph, TraversalQueryBuffer queryBuffer )
		{
			_graph = graph ?? throw new ArgumentNullException( nameof( graph ) );
			_queryBuffer = queryBuffer ?? throw new ArgumentNullException( nameof( queryBuffer ) );
		}

		public bool TryFindEntryAnchor(
			Vector3 characterPosition,
			Vector3 facingDirection,
			float searchRadius,
			TraversalAnchorFlags requiredFlags,
			out int anchorIndex
		)
		{
			anchorIndex = -1;

			Vector3 facing = SafeNormalized( facingDirection, Vector3.Forward );

			_graph.SpatialIndex.QuerySphereNonAlloc(
				characterPosition.ToSystem(),
				searchRadius,
				_queryBuffer
			);

			float bestScore = 0.2f;

			for ( int i = 0; i < _queryBuffer.Count; i++ ) {
				int candidateIndex = _queryBuffer.Items[i];
				ref readonly TraversalAnchor anchor = ref _graph.Anchors[candidateIndex];

				if ( (anchor.Flags & requiredFlags) != requiredFlags ) {
					continue;
				}

				Vector3 toAnchor = anchor.Position.ToGodot() - characterPosition;
				float distSq = toAnchor.LengthSquared();

				if ( distSq < 0.0001f ) {
					continue;
				}

				Vector3 toAnchorDir = toAnchor / MathF.Sqrt( distSq );

				// Character should be looking roughly toward the anchor
				float facingScore = facing.Dot( toAnchorDir );
				if ( facingScore < 0.1f ) {
					continue;
				}

				// Anchor normal points outward from the wall.
				// Facing into the wall means facing is close to -normal.
				float wallFacingScore = facing.Dot( -anchor.Normal.ToGodot() );
				if ( wallFacingScore < 0.15f ) {
					continue;
				}

				float distancePenalty = distSq * 0.05f;

				float score =
					facingScore * 1.25f +
					wallFacingScore * 1.75f -
					distancePenalty;

				if ( score > bestScore ) {
					bestScore = score;
					anchorIndex = candidateIndex;
				}
			}

			return anchorIndex >= 0;
		}

		public bool TrySelectEdge(
			int currentAnchorIndex,
			Vector2 traversalInput,
			out int edgeIndex
		)
		{
			const float TRAVERSAL_EPSILON = 0.15f * 0.15f;

			edgeIndex = -1;

			if ( currentAnchorIndex < 0 || currentAnchorIndex >= _graph.AnchorCount ) {
				return false;
			}

			if ( traversalInput.LengthSquared() < TRAVERSAL_EPSILON ) {
				return false;
			}

			ref readonly TraversalAnchor from = ref _graph.Anchors[currentAnchorIndex];

			Vector3 desired =
				from.Right.ToGodot() * traversalInput.X +
				from.Up.ToGodot() * traversalInput.Y;

			desired = SafeNormalized( desired, Vector3.Zero );

			if ( desired == Vector3.Zero ) {
				return false;
			}

			float bestScore = 0.35f;

			int start = from.FirstEdge;
			int end = start + from.EdgeCount;

			for ( int i = start; i < end; i++ ) {
				ref readonly TraversalEdge edge = ref _graph.Edges[i];

				if ( edge.To < 0 || edge.To >= _graph.AnchorCount ) {
					continue;
				}

				ref readonly TraversalAnchor to = ref _graph.Anchors[edge.To];

				Vector3 moveVector = ( to.Position - from.Position ).ToGodot();

				// Mantle/drop/vault edges may intentionally point to the same anchor.
				// Give them directional meaning from their type.
				Vector3 moveDir = moveVector.LengthSquared() > 0.0001f
					? moveVector.Normalized()
					: DirectionFromMoveType( to, edge.MoveType );

				float directionScore = desired.Dot( moveDir );

				float moveBias = MoveTypeBias( edge.MoveType, traversalInput );
				float costPenalty = edge.Cost * 0.05f;

				float score = directionScore + moveBias - costPenalty;

				if ( score > bestScore ) {
					bestScore = score;
					edgeIndex = i;
				}
			}

			return edgeIndex >= 0;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public ref readonly TraversalAnchor GetAnchor( int index )
		{
			return ref _graph.Anchors[index];
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public ref readonly TraversalEdge GetEdge( int index )
		{
			return ref _graph.Edges[index];
		}

		public static Vector3 DirectionFromMoveType(
			in TraversalAnchor anchor,
			TraversalMoveType moveType
		)
		{
			return moveType switch {
				TraversalMoveType.ClimbUp => anchor.Up.ToGodot(),
				TraversalMoveType.ClimbDown => -anchor.Up.ToGodot(),
				TraversalMoveType.ShimmyLeft => -anchor.Right.ToGodot(),
				TraversalMoveType.ShimmyRight => anchor.Right.ToGodot(),
				TraversalMoveType.Mantle => (anchor.Up - anchor.Normal).ToGodot(),
				TraversalMoveType.Vault => -anchor.Normal.ToGodot(),
				TraversalMoveType.Drop => -anchor.Up.ToGodot(),
				TraversalMoveType.JumpAcross => -anchor.Normal.ToGodot(),
				_ => Vector3.Zero
			};
		}

		public static float MoveTypeBias(
			TraversalMoveType moveType,
			Vector2 input
		)
		{
			return moveType switch {
				TraversalMoveType.ClimbUp when input.Y > 0.5f => 0.25f,
				TraversalMoveType.ClimbDown when input.Y < -0.5f => 0.25f,
				TraversalMoveType.ShimmyLeft when input.X < -0.5f => 0.20f,
				TraversalMoveType.ShimmyRight when input.X > 0.5f => 0.20f,
				TraversalMoveType.Mantle when input.Y > 0.65f => 0.35f,
				TraversalMoveType.Drop when input.Y < -0.65f => 0.20f,
				_ => 0.0f
			};
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private static Vector3 SafeNormalized( Vector3 value, Vector3 fallback )
		{
			float lenSq = value.LengthSquared();
			return lenSq < 0.0001f ? fallback : value / MathF.Sqrt( lenSq );
		}
	}
}
