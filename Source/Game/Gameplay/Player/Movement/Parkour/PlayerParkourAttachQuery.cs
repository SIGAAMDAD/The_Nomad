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
using Nomad.Core.Collections;
using Nomad.EngineUtils;
using Nomad.Game.Gameplay.Traversal;
using Nomad.Game.Sdk.Traversal;

namespace Nomad.Game.Gameplay.Player.Movement.Parkour
{
	internal sealed class PlayerParkourAttachQuery
	{
		private readonly ITraversalGraphProvider _graphProvider;
		private readonly PlayerParkourSettings _settings;
		private readonly PooledList<int> _queryBuffer;
		private readonly PlayerParkourRayValidator _rayValidator;

		public PlayerParkourAttachQuery(
			ITraversalGraphProvider graphProvider,
			PlayerParkourSettings settings,
			PooledList<int> queryBuffer,
			PlayerParkourRayValidator rayValidator
		)
		{
			_graphProvider = graphProvider ?? throw new ArgumentNullException( nameof( graphProvider ) );
			_settings = settings ?? throw new ArgumentNullException( nameof( settings ) );
			_queryBuffer = queryBuffer ?? throw new ArgumentNullException( nameof( queryBuffer ) );
			_rayValidator = rayValidator ?? throw new ArgumentNullException( nameof( rayValidator ) );
		}

		public bool TryFindBestAnchor(
			Vector3 bodyPosition,
			Vector3 facing,
			out TraversalAnchorHandle handle
		)
		{
			handle = TraversalAnchorHandle.Invalid;

			Vector3 normalizedFacing = PlayerParkourMath.SafeNormalized( facing, -Vector3.UnitZ );
			float bestScore = float.NegativeInfinity;

			int graphCount = _graphProvider.GraphCount;
			for ( int graphIndex = 0; graphIndex < graphCount; graphIndex++ ) {
				TraversalGraphSource source = _graphProvider.GetGraphSource( graphIndex );
				TraversalGraph graph = source.Graph;

				graph.SpatialIndex.QuerySphereNonAlloc(
					bodyPosition,
					_settings.AttachSearchRadius,
					_queryBuffer
				);

				for ( int i = 0; i < _queryBuffer.Count; i++ ) {
					int anchorIndex = _queryBuffer[i];
					ref readonly TraversalAnchor anchor = ref graph.Anchors[anchorIndex];

					if ( (anchor.Flags & TraversalAnchorFlags.EntryAllowed) == 0 ) {
						continue;
					}

					Vector3 anchorPosition = PlayerParkourMath.AnchorPosition( anchor );
					Vector3 toAnchor = anchorPosition - bodyPosition;
					float distSq = toAnchor.LengthSquared();

					if ( distSq < 0.0001f ) {
						continue;
					}

					Vector3 toAnchorDir = toAnchor / MathF.Sqrt( distSq );
					float facingAnchorDot = Vector3.Dot( normalizedFacing, toAnchorDir );
					if ( facingAnchorDot < _settings.MinFacingAnchorDot ) {
						continue;
					}

					Vector3 anchorNormal = PlayerParkourMath.AnchorNormal( anchor );
					float facingWallDot = Vector3.Dot( normalizedFacing, -anchorNormal );
					if ( facingWallDot < _settings.MinFacingWallDot ) {
						continue;
					}

					if ( _settings.ValidateEntryLineOfSight && !ValidateEntryLineOfSight( bodyPosition, anchor ) ) {
						continue;
					}

					float score = facingAnchorDot * 1.15f + facingWallDot * 1.85f - distSq * 0.05f;

					if ( score > bestScore ) {
						bestScore = score;
						handle = new TraversalAnchorHandle( source.SourceId, anchorIndex );
					}
				}
			}

			return handle.IsValid;
		}

		private bool ValidateEntryLineOfSight( Vector3 bodyPosition, in TraversalAnchor anchor )
		{
			Vector3 from = bodyPosition + Vector3.UnitY * _settings.EntryLineStartHeight;
			Vector3 to = PlayerParkourMath.AnchorPosition( anchor ) + PlayerParkourMath.AnchorUp( anchor ) * _settings.EntryLineEndHeight;
			return _rayValidator.ValidateLine( from.ToGodot(), to.ToGodot() );
		}
	};
};
