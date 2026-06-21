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
using System.Numerics;
using Nomad.Game.Sdk.Traversal;

namespace Nomad.Game.Application.Gameplay.Traversal
{
	internal sealed class TraversalSpatialIndex
	{
		private readonly TraversalAnchor[] _anchors;
		private readonly Dictionary<TraversalCell, int[]> _cells;
		private readonly float _cellSize;
		private readonly float _inverseCellSize;

		public TraversalSpatialIndex( TraversalAnchor[] anchors, float cellSize )
		{
			_anchors = anchors;
			_cellSize = MathF.Max( cellSize, 0.25f );
			_inverseCellSize = 1.0f / _cellSize;

			var buildCells = new Dictionary<TraversalCell, List<int>>();

			for ( int i = 0; i < anchors.Length; i++ ) {
				TraversalCell cell = PositionToCell( anchors[i].Position );

				if ( !buildCells.TryGetValue( cell, out List<int> list ) ) {
					list = new List<int>( 8 );
					buildCells[cell] = list;
				}

				list.Add( i );
			}

			_cells = new Dictionary<TraversalCell, int[]>( buildCells.Count );

			foreach ( (TraversalCell cell, List<int> indices) in buildCells ) {
				_cells[cell] = indices.ToArray();
			}
		}

		public TraversalCell PositionToCell( Vector3 position )
		{
			return new TraversalCell(
				(int)MathF.Floor( position.X * _inverseCellSize ),
				(int)MathF.Floor( position.Y * _inverseCellSize ),
				(int)MathF.Floor( position.Z * _inverseCellSize )
			);
		}

		public void QuerySphereNonAlloc(
			Vector3 center,
			float radius,
			TraversalQueryBuffer buffer
		)
		{
			buffer.Clear();

			float radiusSq = radius * radius;

			TraversalCell min = PositionToCell( center - Vector3.One * radius );
			TraversalCell max = PositionToCell( center + Vector3.One * radius );

			for ( int z = min.Z; z <= max.Z; z++ ) {
				for ( int y = min.Y; y <= max.Y; y++ ) {
					for ( int x = min.X; x <= max.X; x++ ) {
						TraversalCell cell = new TraversalCell( x, y, z );

						if ( !_cells.TryGetValue( cell, out int[] indices ) ) {
							continue;
						}

						for ( int i = 0; i < indices.Length; i++ ) {
							int anchorIndex = indices[i];
							Vector3 p = _anchors[anchorIndex].Position;

							if ( (p - center).LengthSquared() <= radiusSq ) {
								buffer.Add( anchorIndex );
							}
						}
					}
				}
			}
		}
	}
}
