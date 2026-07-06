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
using Nomad.EngineUtils;
using Nomad.Game.Gameplay.Traversal;
using Nomad.Game.Sdk.Traversal;

namespace Nomad.Game.Gameplay.Player.Movement.Parkour
{
	internal sealed class PlayerParkourEdgeValidator
	{
		private readonly PlayerParkourRayValidator _rayValidator;
		private readonly PlayerParkourEdgeMotion _edgeMotion;

		public PlayerParkourEdgeValidator( PlayerParkourRayValidator rayValidator, PlayerParkourEdgeMotion edgeMotion )
		{
			_rayValidator = rayValidator ?? throw new ArgumentNullException( nameof( rayValidator ) );
			_edgeMotion = edgeMotion ?? throw new ArgumentNullException( nameof( edgeMotion ) );
		}

		public bool CanStartEdge( TraversalGraph graph, int currentAnchorIndex, int edgeIndex )
		{
			if ( graph == null || edgeIndex < 0 || edgeIndex >= graph.Edges.Length ) {
				return false;
			}

			if ( currentAnchorIndex < 0 || currentAnchorIndex >= graph.Anchors.Length ) {
				return false;
			}

			ref readonly TraversalEdge edge = ref graph.Edges[edgeIndex];

			if ( edge.To < 0 || edge.To >= graph.Anchors.Length ) {
				return false;
			}

			ref readonly TraversalAnchor from = ref graph.Anchors[currentAnchorIndex];
			ref readonly TraversalAnchor to = ref graph.Anchors[edge.To];

			if ( (edge.Flags & TraversalEdgeFlags.RequiresLineOfSight) != 0 ) {
				Vector3 lineStart = PlayerParkourMath.AnchorPosition( from ) + PlayerParkourMath.AnchorUp( from ) * 0.5f;
				Vector3 lineEnd = PlayerParkourMath.AnchorPosition( to ) + PlayerParkourMath.AnchorUp( to ) * 0.5f;

				if ( !_rayValidator.ValidateLine( lineStart.ToGodot(), lineEnd.ToGodot() ) ) {
					return false;
				}
			}

			if ( (edge.Flags & TraversalEdgeFlags.RequiresClearance) != 0 ) {
				Vector3 target = _edgeMotion.GetTargetPosition( from, to, edge );
				Vector3 lineStart = PlayerParkourMath.AnchorPosition( from ) + PlayerParkourMath.AnchorUp( from ) * 0.9f;
				Vector3 lineEnd = target + PlayerParkourMath.AnchorUp( to ) * 0.9f;

				if ( !_rayValidator.ValidateLine( lineStart.ToGodot(), lineEnd.ToGodot() ) ) {
					return false;
				}
			}

			return true;
		}
	}
}
