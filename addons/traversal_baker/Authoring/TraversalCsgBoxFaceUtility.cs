using Godot;

namespace Nomad.Game.Traversal.Baking {
	internal enum TraversalCsgBoxFace : byte {
		PositiveZ,
		NegativeZ,
		PositiveX,
		NegativeX
	};

	internal static class TraversalCsgBoxFaceUtility {
		public static Vector3 GetLocalNormal( TraversalCsgBoxFace face ) {
			return face switch {
				TraversalCsgBoxFace.PositiveZ => Vector3.Back,
				TraversalCsgBoxFace.NegativeZ => Vector3.Forward,
				TraversalCsgBoxFace.PositiveX => Vector3.Right,
				TraversalCsgBoxFace.NegativeX => Vector3.Left,
				_ => Vector3.Back,
			};
		}

		public static Vector3 GetLocalRight( TraversalCsgBoxFace face ) {
			Vector3 normal = GetLocalNormal( face );
			Vector3 right = Vector3.Up.Cross( normal );

			if ( right.LengthSquared() < 0.0001f ) {
				return Vector3.Right;
			}

			return right.Normalized();
		}

		public static float GetFaceWidth( Vector3 boxSize, TraversalCsgBoxFace face ) {
			return face switch {
				TraversalCsgBoxFace.PositiveZ => boxSize.X,
				TraversalCsgBoxFace.NegativeZ => boxSize.X,
				TraversalCsgBoxFace.PositiveX => boxSize.Z,
				TraversalCsgBoxFace.NegativeX => boxSize.Z,
				_ => boxSize.X,
			};
		}

		public static Vector3 GetFaceCenterLocal( Vector3 boxSize, TraversalCsgBoxFace face ) {
			Vector3 normal = GetLocalNormal( face );

			return new Vector3(
				normal.X * boxSize.X * 0.5f,
				0.0f,
				normal.Z * boxSize.Z * 0.5f
			);
		}
	};
};
