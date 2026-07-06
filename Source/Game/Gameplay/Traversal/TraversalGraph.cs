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
using System.Collections.Generic;
using Nomad.Core.Compatibility.Guards;
using Nomad.EngineUtils;
using Nomad.Game.Prefabs;
using Nomad.Game.Sdk.Traversal;

namespace Nomad.Game.Gameplay.Traversal
{
    internal sealed record TraversalGraph
    {
        public readonly TraversalAnchor[] Anchors = Array.Empty<TraversalAnchor>();
        public readonly TraversalEdge[] Edges = Array.Empty<TraversalEdge>();
		public readonly TraversalSpatialIndex SpatialIndex;

		public int AnchorCount => Anchors.Length;
		public int EdgeCount => Edges.Length;

		public TraversalGraph( TraversalAnchor[] anchors, TraversalEdge[] edges, float spatialCellSize )
		{
			Anchors = anchors;
			Edges = edges;
			SpatialIndex = new TraversalSpatialIndex( anchors, spatialCellSize );
		}

		public ReadOnlySpan<TraversalEdge> GetEdges( int anchorIndex )
		{
			ref readonly TraversalAnchor anchor = ref Anchors[anchorIndex];
			return Edges.AsSpan( anchor.FirstEdge, anchor.EdgeCount );
		}

		public static TraversalGraph FromDatabase( TraversalDatabase database )
		{
			ArgumentGuard.ThrowIfNull( database, nameof( database ) );

			if ( !database.IsValid( out string error ) ) {
				throw new InvalidOperationException( error );
			}

			int anchorCount = database.AnchorCount;
			int edgeCount = database.EdgeCount;

			// TODO: might want to pool these and stream a traversal database relative to the in-game location.
			var anchors = new TraversalAnchor[anchorCount];

			for ( int i = 0; i < anchorCount; i++ ) {
				anchors[i] = new TraversalAnchor(
					database.Positions[i].ToSystem(),
					database.Normals[i].ToSystem(),
					database.Ups[i].ToSystem(),
					(TraversalAnchorFlags)database.AnchorFlags[i],
					(ushort)Math.Clamp( database.SurfaceIds[i], 0, ushort.MaxValue ),
					database.FirstEdges[i],
					(ushort)Math.Clamp( database.EdgeCounts[i], 0, ushort.MaxValue )
				);
			}

			var edges = new TraversalEdge[edgeCount];

			for ( int i = 0; i < edgeCount; i++ ) {
				edges[i] = new TraversalEdge(
					database.EdgeTargets[i],
					(TraversalMoveType)database.EdgeMoveTypes[i],
					(TraversalEdgeFlags)database.EdgeFlags[i],
					database.EdgeCosts[i],
					database.EdgeDurations[i],
					database.EdgeAnimationIds[i]
				);
			}

			return new TraversalGraph(
				anchors,
				edges,
				MathF.Max( 0.25f, database.SpatialCellSize )
			);
		}
    };
};
