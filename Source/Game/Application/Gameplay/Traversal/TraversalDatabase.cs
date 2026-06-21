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

namespace Nomad.Game.Prefabs
{
	internal sealed partial class TraversalDatabase : Resource
	{
		[Export] public Vector3[] Positions { get; set; } = Array.Empty<Vector3>();
		[Export] public Vector3[] Normals { get; set; } = Array.Empty<Vector3>();
		[Export] public Vector3[] Ups { get; set; } = Array.Empty<Vector3>();

		[Export] public int[] AnchorFlags { get; set; } = Array.Empty<int>();
		[Export] public int[] SurfaceIds { get; set; } = Array.Empty<int>();
		[Export] public int[] FirstEdges { get; set; } = Array.Empty<int>();
		[Export] public int[] EdgeCounts { get; set; } = Array.Empty<int>();

		[Export] public int[] EdgeTargets { get; set; } = Array.Empty<int>();
		[Export] public byte[] EdgeMoveTypes { get; set; } = Array.Empty<byte>();
		[Export] public int[] EdgeFlags { get; set; } = Array.Empty<int>();
		[Export] public float[] EdgeCosts { get; set; } = Array.Empty<float>();
		[Export] public float[] EdgeDurations { get; set; } = Array.Empty<float>();
		[Export] public int[] EdgeAnimationIds { get; set; } = Array.Empty<int>();

		[Export] public float SpatialCellSize { get; set; } = 2.0f;

		public int AnchorCount => Positions.Length;
		public int EdgeCount => EdgeTargets.Length;

		public bool IsValid( out string error )
		{
			int anchorCount = AnchorCount;

			if ( anchorCount == 0 ) {
				error = "TraversalDatabase has no anchors.";
				return false;
			}

			if (
				Normals.Length != anchorCount ||
				Ups.Length != anchorCount ||
				AnchorFlags.Length != anchorCount ||
				SurfaceIds.Length != anchorCount ||
				FirstEdges.Length != anchorCount ||
				EdgeCounts.Length != anchorCount
			) {
				error = "TraversalDatabase anchor arrays have mismatched lengths.";
				return false;
			}

			int edgeCount = EdgeCount;

			if (
				EdgeMoveTypes.Length != edgeCount ||
				EdgeFlags.Length != edgeCount ||
				EdgeCosts.Length != edgeCount ||
				EdgeDurations.Length != edgeCount ||
				EdgeAnimationIds.Length != edgeCount
			) {
				error = "TraversalDatabase edge arrays have mismatched lengths.";
				return false;
			}

			for ( int i = 0; i < edgeCount; i++ ) {
				int target = EdgeTargets[i];
				if ( target < 0 || target >= anchorCount ) {
					error = $"TraversalDatabase edge {i} has invalid target {target}";
					return false;
				}
			}

			error = string.Empty;
			return true;
		}
	}
}
