using Godot;
using System.Collections.Generic;
using Nomad.Game.Sdk.Traversal;
using Nomad.Game.Prefabs;

namespace Nomad.Game.Traversal.Baking {
	internal sealed class TraversalBakeBuilder {
		private struct AnchorRecord {
			public Vector3 Position;
			public Vector3 Normal;
			public Vector3 Up;
			public TraversalAnchorFlags Flags;
			public ushort SurfaceId;
		}

		private struct EdgeRecord {
			public int To;
			public TraversalMoveType MoveType;
			public TraversalEdgeFlags Flags;
			public float Cost;
			public float Duration;
			public int AnimationId;
		}

		private readonly List<AnchorRecord> _anchors = new();
		private readonly List<List<EdgeRecord>> _edgesByAnchor = new();

		public int AnchorCount => _anchors.Count;

		public int EdgeCount {
			get {
				int count = 0;
				for ( int i = 0; i < _edgesByAnchor.Count; i++ ) {
					count += _edgesByAnchor[ i ].Count;
				}
				return count;
			}
		}

		public int AddAnchor(
			Vector3 position,
			Vector3 normal,
			Vector3 up,
			TraversalAnchorFlags flags,
			ushort surfaceId = 0
		) {
			Vector3 cleanNormal = SafeNormalized( normal, Vector3.Back );
			Vector3 cleanUp = SafeNormalized( up, Vector3.Up );

			cleanUp -= cleanNormal * cleanUp.Dot( cleanNormal );

			if ( cleanUp.LengthSquared() < 0.0001f ) {
				cleanUp = Vector3.Up;
			}

			cleanUp = cleanUp.Normalized();

			int index = _anchors.Count;

			_anchors.Add( new AnchorRecord {
				Position = position,
				Normal = cleanNormal,
				Up = cleanUp,
				Flags = flags,
				SurfaceId = surfaceId,
			} );

			_edgesByAnchor.Add( new List<EdgeRecord>( 8 ) );

			return index;
		}

		public void AddEdge(
			int from,
			int to,
			TraversalMoveType moveType,
			TraversalEdgeFlags flags = TraversalEdgeFlags.EndsAttached,
			float cost = 1.0f,
			float duration = 0.22f,
			int animationId = -1
		) {
			if ( from < 0 || from >= _anchors.Count ) {
				return;
			}

			if ( to < 0 || to >= _anchors.Count ) {
				return;
			}

			if ( animationId < 0 ) {
				animationId = (int)moveType;
			}

			_edgesByAnchor[ from ].Add( new EdgeRecord {
				To = to,
				MoveType = moveType,
				Flags = flags,
				Cost = cost,
				Duration = Mathf.Max( duration, 0.01f ),
				AnimationId = animationId,
			} );
		}

		public void AddBidirectionalEdge(
			int a,
			int b,
			TraversalMoveType aToB,
			TraversalMoveType bToA,
			TraversalEdgeFlags flags = TraversalEdgeFlags.EndsAttached,
			float cost = 1.0f,
			float duration = 0.22f
		) {
			AddEdge( a, b, aToB, flags, cost, duration );
			AddEdge( b, a, bToA, flags, cost, duration );
		}

		public void AddMantleEdge( int from, float duration = 0.35f ) {
			AddEdge(
				from,
				from,
				TraversalMoveType.Mantle,
				TraversalEdgeFlags.RequiresClearance | TraversalEdgeFlags.EndsGrounded,
				cost: 0.5f,
				duration: duration,
				animationId: (int)TraversalMoveType.Mantle
			);
		}

		public TraversalDatabase BuildDatabase( float spatialCellSize ) {
			int anchorCount = _anchors.Count;

			var positions = new Vector3[ anchorCount ];
			var normals = new Vector3[ anchorCount ];
			var ups = new Vector3[ anchorCount ];
			var anchorFlags = new int[ anchorCount ];
			var surfaceIds = new int[ anchorCount ];
			var firstEdges = new int[ anchorCount ];
			var edgeCounts = new int[ anchorCount ];

			var edgeTargets = new List<int>();
			var edgeMoveTypes = new List<byte>();
			var edgeFlags = new List<int>();
			var edgeCosts = new List<float>();
			var edgeDurations = new List<float>();
			var edgeAnimationIds = new List<int>();

			for ( int i = 0; i < anchorCount; i++ ) {
				AnchorRecord anchor = _anchors[ i ];

				positions[ i ] = anchor.Position;
				normals[ i ] = anchor.Normal;
				ups[ i ] = anchor.Up;
				anchorFlags[ i ] = (int)anchor.Flags;
				surfaceIds[ i ] = anchor.SurfaceId;

				firstEdges[ i ] = edgeTargets.Count;
				edgeCounts[ i ] = _edgesByAnchor[ i ].Count;

				foreach ( EdgeRecord edge in _edgesByAnchor[ i ] ) {
					edgeTargets.Add( edge.To );
					edgeMoveTypes.Add( (byte)edge.MoveType );
					edgeFlags.Add( (int)edge.Flags );
					edgeCosts.Add( edge.Cost );
					edgeDurations.Add( edge.Duration );
					edgeAnimationIds.Add( edge.AnimationId );
				}
			}

			return new TraversalDatabase {
				Positions = positions,
				Normals = normals,
				Ups = ups,

				AnchorFlags = anchorFlags,
				SurfaceIds = surfaceIds,
				FirstEdges = firstEdges,
				EdgeCounts = edgeCounts,

				EdgeTargets = edgeTargets.ToArray(),
				EdgeMoveTypes = edgeMoveTypes.ToArray(),
				EdgeFlags = edgeFlags.ToArray(),
				EdgeCosts = edgeCosts.ToArray(),
				EdgeDurations = edgeDurations.ToArray(),
				EdgeAnimationIds = edgeAnimationIds.ToArray(),

				SpatialCellSize = Mathf.Max( spatialCellSize, 0.25f ),
			};
		}

		private static Vector3 SafeNormalized( Vector3 value, Vector3 fallback ) {
			float lenSq = value.LengthSquared();
			if ( lenSq < 0.0001f )
				return fallback;
			return value / Mathf.Sqrt( lenSq );
		}
	};
};
