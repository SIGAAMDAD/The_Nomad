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
using System.Buffers;
using System.Numerics;
using Nomad.Core.Collections;
using Nomad.Game.Sdk.Traversal;

namespace Nomad.Game.Gameplay.Traversal
{
	internal sealed class TraversalSpatialIndex
	{
		private readonly TraversalAnchor[] _anchors;
		private readonly TraversalCell _minCell;
		private readonly TraversalCell _maxCell;

		private readonly int _cellCountX;
		private readonly int _cellCountY;
		private readonly int _cellCountZ;
		private readonly int _cellsPerLayer;

		private readonly TraversalCellRange[] _cellRanges;
		private readonly int[] _cellAnchorIndices;

		private readonly int _xWordCount;
		private readonly ulong[] _rowOccupancyWords;

		private readonly float _cellSize;
		private readonly float _inverseCellSize;

		public TraversalSpatialIndex( TraversalAnchor[] anchors, float cellSize )
		{
			_anchors = anchors;
			_cellSize = MathF.Max( cellSize, 0.25f );
			_inverseCellSize = 1.0f / _cellSize;

			if ( anchors.Length == 0 ) {
				_minCell = new TraversalCell( 0, 0, 0 );
				_maxCell = new TraversalCell( 0, 0, 0 );

				_cellCountX = 1;
				_cellCountY = 1;
				_cellCountZ = 1;

				_cellRanges = new TraversalCellRange[1];
				_cellAnchorIndices = Array.Empty<int>();

				return;
			}

			CalculateCellBounds( anchors, out _minCell, out _maxCell );

			_cellCountX = (_maxCell.X - _minCell.X) + 1;
			_cellCountY = (_maxCell.Y - _minCell.Y) + 1;
			_cellCountZ = (_maxCell.Z - _minCell.Z) + 1;

			_cellsPerLayer = checked(_cellCountX * _cellCountY);

			int cellCount = checked(_cellsPerLayer * _cellCountZ);
			int rowCount = checked(_cellCountY * _cellCountZ);

			_xWordCount = (_cellCountX + 63) >> 6;

			_cellRanges = new TraversalCellRange[cellCount];
			_cellAnchorIndices = new int[anchors.Length];
			_rowOccupancyWords = new ulong[checked(rowCount * _xWordCount)];

			int[] counts = ArrayPool<int>.Shared.Rent( cellCount );

			for ( int i = 0; i < anchors.Length; i++ ) {
				TraversalCell cell = PositionToCell( anchors[i].Position );

				if ( !TryEncodeCell( cell.X, cell.Y, cell.Z, out int cellId ) ) {
					continue;
				}

				counts[cellId]++;
			}

			_cellRanges = new TraversalCellRange[cellCount];
			_cellAnchorIndices = new int[anchors.Length];

			int start = 0;

			for ( int i = 0; i < counts.Length; i++ ) {
				int count = counts[i];
				_cellRanges[i] = new TraversalCellRange( start, count );
				start += count;

				counts[i] = 0;
			}

			for ( int i = 0; i < anchors.Length; i++ ) {
				TraversalCell cell = PositionToCell( anchors[i].Position );

				if ( !TryEncodeCell( cell.X, cell.Y, cell.Z, out int cellId ) ) {
					continue;
				}

				TraversalCellRange range = _cellRanges[cellId];

				int writeIndex = range.Start + counts[cellId];
				_cellAnchorIndices[writeIndex] = i;

				counts[cellId]++;
			}
		}

		private void BuildCells( TraversalAnchor[] anchors, int cellCount )
		{
			int[] counts = new int[cellCount];

			for ( int i = 0; i < anchors.Length; i++ ) {
				TraversalCell cell = PositionToCell( anchors[i].Position );

				int cellId = EncodeCellUnchecked( cell.X, cell.Y, cell.Z );
				counts[cellId]++;

				int localX = cell.X - _minCell.X;
				int localY = cell.Y - _minCell.Y;
				int localZ = cell.Z - _minCell.Z;

				SetOccupied( localX, localY, localZ );
			}

			int start = 0;

			for ( int i = 0; i < counts.Length; i++ ) {
				int count = counts[i];

				_cellRanges[i] = new TraversalCellRange( start, count );
				start += count;

				counts[i] = 0;
			}

			for ( int i = 0; i < anchors.Length; i++ ) {
				TraversalCell cell = PositionToCell( anchors[i].Position );

				int cellId = EncodeCellUnchecked( cell.X, cell.Y, cell.Z );
				TraversalCellRange range = _cellRanges[cellId];

				int writeIndex = range.Start + counts[cellId];
				_cellAnchorIndices[writeIndex] = i;

				counts[cellId]++;
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
			PooledList<int> buffer
		)
		{
			buffer.Clear();

			if ( _anchors.Length == 0 ) {
				return;
			}

			float radiusSq = radius * radius;

			TraversalCell min = PositionToCell( center - Vector3.One * radius );
			TraversalCell max = PositionToCell( center + Vector3.One * radius );

			int minX = Math.Max( min.X, _minCell.X );
			int minY = Math.Max( min.Y, _minCell.Y );
			int minZ = Math.Max( min.Z, _minCell.Z );

			int maxX = Math.Min( max.X, _maxCell.X );
			int maxY = Math.Min( max.Y, _maxCell.Y );
			int maxZ = Math.Min( max.Z, _maxCell.Z );

			if ( minX > maxX || minY > maxY || minZ > maxZ ) {
				return;
			}

			int localMinX = minX - _minCell.X;
			int localMinY = minY - _minCell.Y;
			int localMinZ = minZ - _minCell.Z;

			int localMaxX = maxX - _maxCell.X;
			int localMaxY = maxY - _maxCell.Y;

			int minWord = localMinX >> 6;
			int maxWord = localMaxX >> 6;

			for ( int z = minZ; z <= maxZ; z++ ) {
				int localZ = z - _minCell.Z;
				int zBase = _cellsPerLayer * localZ;
				int rowBase = localZ * _cellCountY;

				for ( int y = minY; y <= maxY; y++ ) {
					int localY = y - _minCell.Y;
					int yBase = zBase + (_cellCountX * localY);

					int rowId = rowBase + localY;
					int rowWordStart = rowId * _xWordCount;

					for ( int word = minWord; word <= maxWord; word++ ) {
						int wordStartX = word << 6;

						int startBit = Math.Max( localMinX - wordStartX, 0 );
						int endBit = Math.Min( localMaxX - wordStartX, 63 );

						ulong mask = _rowOccupancyWords[rowWordStart + word] & CreateBitRangeMask( startBit, endBit );

						while ( mask != 0UL ) {
							int bit = BitOperations.TrailingZeroCount( mask );
							mask &= mask - 1UL;

							int localX = wordStartX + bit;
							int cellId = yBase + localX;

							TraversalCellRange range = _cellRanges[cellId];

							for ( int i = 0; i < range.Count; i++ ) {
								int anchorIndex = _cellAnchorIndices[range.Start + i];
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

		private void CalculateCellBounds( TraversalAnchor[] anchors, out TraversalCell minCell, out TraversalCell maxCell )
		{
			TraversalCell first = PositionToCell( anchors[0].Position );

			int minX = first.X;
			int minY = first.Y;
			int minZ = first.Z;

			int maxX = first.X;
			int maxY = first.Y;
			int maxZ = first.Z;

			for ( int i = 1; i < anchors.Length; i++ ) {
				TraversalCell cell = PositionToCell( anchors[i].Position );

				minX = Math.Min( minX, cell.X );
				minY = Math.Min( minY, cell.Y );
				minZ = Math.Min( minZ, cell.Z );

				maxX = Math.Max( maxX, cell.X );
				maxY = Math.Max( maxY, cell.Y );
				maxZ = Math.Max( maxZ, cell.Z );
			}

			minCell = new TraversalCell( minX, minY, minZ );
			maxCell = new TraversalCell( maxX, maxY, maxZ );
		}

		private bool TryEncodeCell( int x, int y, int z, out int cellId )
		{
			int localX = x - _minCell.X;
			int localY = y - _minCell.Y;
			int localZ = z - _minCell.Z;

			if (
				(uint)localX >= (uint)_cellCountX ||
				(uint)localY >= (uint)_cellCountY ||
				(uint)localZ >= (uint)_cellCountZ
			) {
				cellId = -1;
				return false;
			}

			cellId = localX + (_cellCountX * (localY + (_cellCountY * localZ)));
			return true;
		}

		private int EncodeCellUnchecked( int x, int y, int z )
		{
			int localX = x - _minCell.X;
			int localY = y - _minCell.Y;
			int localZ = z - _minCell.Z;

			return localX + (_cellCountX + localY) + (_cellsPerLayer * localZ);
		}

		private void SetOccupied( int localX, int localY, int localZ )
		{
			int rowId = (localZ * _cellCountY) + localY;
			int word = localX >> 6;
			int bit = localX & 63;

			_rowOccupancyWords[(rowId * _xWordCount) + word] |= 1UL << bit;
		}

		private static ulong CreateBitRangeMask( int startBit, int endBit )
		{
			int count = (endBit - startBit) + 1;

			if ( count >= 64 ) {
				return ulong.MaxValue;
			}

			return ((1UL << count) - 1UL) << startBit;
		}
	};
};
