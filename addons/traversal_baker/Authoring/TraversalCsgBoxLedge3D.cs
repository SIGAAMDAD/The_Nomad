using Godot;
using System.Collections.Generic;
using Nomad.Game.Sdk.Traversal;

namespace Nomad.Game.Traversal.Baking {
	[Tool]
	[GlobalClass]
	internal partial class TraversalCsgBoxLedge3D : TraversalAuthoringNode3D {
		[Export] public TraversalCsgBoxFace Face { get; set; } = TraversalCsgBoxFace.PositiveZ;

		[Export] public float HeightFromBottom { get; set; } = 2.0f;
		[Export] public float HorizontalInset { get; set; } = 0.0f;
		[Export] public float NormalOffset { get; set; } = 0.05f;

		[Export] public float SampleSpacing { get; set; } = 0.65f;

		[Export] public bool EntryAllowed { get; set; } = true;
		[Export] public bool GenerateShimmyEdges { get; set; } = true;
		[Export] public bool GenerateMantleEdges { get; set; } = true;

		[Export] public float ShimmyDuration { get; set; } = 0.22f;
		[Export] public float MantleDuration { get; set; } = 0.35f;

		public override void BakeTraversal( TraversalBakeContext context ) {
			if ( !EnabledForBake ) {
				return;
			}

			CsgBox3D box = GetParent() as CsgBox3D;

			if ( box == null ) {
				GD.PushWarning( $"{Name}: TraversalCsgBoxLedge3D must be a child of CsgBox3D." );
				return;
			}

			Vector3 size = box.Size;
			Vector3 localNormal = TraversalCsgBoxFaceUtility.GetLocalNormal( Face );
			Vector3 localRight = TraversalCsgBoxFaceUtility.GetLocalRight( Face );

			float faceWidth = TraversalCsgBoxFaceUtility.GetFaceWidth( size, Face );
			float usableWidth = Mathf.Max( 0.1f, faceWidth - HorizontalInset * 2.0f );

			float y = Mathf.Clamp(
				-size.Y * 0.5f + HeightFromBottom,
				-size.Y * 0.5f,
				size.Y * 0.5f
			);

			Vector3 faceCenter = TraversalCsgBoxFaceUtility.GetFaceCenterLocal( size, Face );
			faceCenter.Y = y;

			int sampleCount = Mathf.Max(
				2,
				Mathf.RoundToInt( usableWidth / Mathf.Max( SampleSpacing, 0.1f ) ) + 1
			);

			Transform3D boxTransform = box.GlobalTransform;
			Vector3 worldNormal = ( boxTransform.Basis * localNormal ).Normalized();
			Vector3 worldUp = ( boxTransform.Basis * Vector3.Up ).Normalized();

			TraversalAnchorFlags flags =
				TraversalAnchorFlags.WallHold |
				TraversalAnchorFlags.Ledge |
				TraversalAnchorFlags.ExitAllowed;

			if ( EntryAllowed ) {
				flags |= TraversalAnchorFlags.EntryAllowed;
			}

			if ( GenerateMantleEdges ) {
				flags |= TraversalAnchorFlags.MantleTop;
			}

			var anchors = new int[ sampleCount ];

			for ( int i = 0; i < sampleCount; i++ ) {
				float t = i / (float)( sampleCount - 1 );
				float offset = Mathf.Lerp( -usableWidth * 0.5f, usableWidth * 0.5f, t );

				Vector3 localPoint = faceCenter + localRight * offset + localNormal * NormalOffset;
				Vector3 worldPoint = boxTransform * localPoint;

				int anchor = context.Builder.AddAnchor(
					worldPoint,
					worldNormal,
					worldUp,
					flags,
					ResolveSurfaceId( context )
				);

				anchors[ i ] = anchor;

				if ( GenerateMantleEdges ) {
					context.Builder.AddMantleEdge( anchor, MantleDuration );
				}
			}

			if ( !GenerateShimmyEdges ) {
				return;
			}

			for ( int i = 0; i < anchors.Length - 1; i++ ) {
				context.Builder.AddBidirectionalEdge(
					anchors[ i ],
					anchors[ i + 1 ],
					TraversalMoveType.ShimmyRight,
					TraversalMoveType.ShimmyLeft,
					TraversalEdgeFlags.EndsAttached,
					cost: 1.0f,
					duration: ShimmyDuration
				);
			}
		}
	};
};
