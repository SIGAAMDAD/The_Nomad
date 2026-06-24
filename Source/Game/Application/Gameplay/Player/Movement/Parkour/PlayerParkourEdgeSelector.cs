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

using Godot;
using Nomad.Game.Application.Gameplay.Traversal;
using Nomad.Game.Sdk.Traversal;

namespace Nomad.Game.Application.Gameplay.Player.Movement.Parkour
{
	internal sealed class PlayerParkourEdgeSelector
	{
		private readonly PlayerParkourSettings _settings;
		private readonly PlayerParkourEdgeMotion _edgeMotion;

		public PlayerParkourEdgeSelector( PlayerParkourSettings settings, PlayerParkourEdgeMotion edgeMotion )
		{
			_settings = settings;
			_edgeMotion = edgeMotion;
		}

		public bool TryStartPreferredSpecialEdge(
			TraversalGraph graph,
			TraversalAnchorHandle currentAnchor,
			TraversalMoveType moveType,
			out int edgeIndex
		)
		{
			edgeIndex = -1;

			if ( !IsValidAnchorIndex( graph, currentAnchor.AnchorIndex ) ) {
				return false;
			}

			ref readonly TraversalAnchor from = ref graph.Anchors[currentAnchor.AnchorIndex];
			int start = from.FirstEdge;
			int end = start + from.EdgeCount;

			for ( int i = start; i < end; i++ ) {
				ref readonly TraversalEdge edge = ref graph.Edges[i];

				if ( edge.MoveType == moveType ) {
					edgeIndex = i;
					return true;
				}
			}

			return false;
		}

		public bool TrySelectBestEdge(
			TraversalGraph graph,
			TraversalAnchorHandle currentAnchor,
			Vector2 input,
			bool wantsJump,
			bool wantsDrop,
			out int edgeIndex
		)
		{
			edgeIndex = -1;

			if ( !IsValidAnchorIndex( graph, currentAnchor.AnchorIndex ) ) {
				return false;
			}

			if ( input.LengthSquared() < _settings.EdgeInputDeadZone * _settings.EdgeInputDeadZone ) {
				return false;
			}

			ref readonly TraversalAnchor from = ref graph.Anchors[currentAnchor.AnchorIndex];
			Vector3 desired = PlayerParkourMath.AnchorRight( from ) * input.X + PlayerParkourMath.AnchorUp( from ) * input.Y;

			if ( desired.LengthSquared() < 0.0001f ) {
				return false;
			}

			desired = desired.Normalized();

			float bestScore = _settings.EdgeSelectionMinScore;
			int start = from.FirstEdge;
			int end = start + from.EdgeCount;

			for ( int i = start; i < end; i++ ) {
				ref readonly TraversalEdge edge = ref graph.Edges[i];

				if ( !IsValidAnchorIndex( graph, edge.To ) ) {
					continue;
				}

				if ( edge.MoveType == TraversalMoveType.Mantle && input.Y < _settings.MantleInputThreshold && !wantsJump ) {
					continue;
				}

				if ( edge.MoveType == TraversalMoveType.Drop && !wantsDrop && input.Y > -_settings.MantleInputThreshold ) {
					continue;
				}

				ref readonly TraversalAnchor to = ref graph.Anchors[edge.To];
				Vector3 moveDirection = _edgeMotion.GetDirection( from, to, edge.MoveType );
				float score = desired.Dot( moveDirection ) + _edgeMotion.GetMoveTypeBias( edge.MoveType, input ) - edge.Cost * 0.04f;

				if ( score > bestScore ) {
					bestScore = score;
					edgeIndex = i;
				}
			}

			return edgeIndex >= 0;
		}

		private static bool IsValidAnchorIndex( TraversalGraph graph, int index )
		{
			return graph != null && index >= 0 && index < graph.Anchors.Length;
		}
	}
}
