using Godot;
using Nomad.Game.Sdk.Traversal;

namespace Nomad.Game.Traversal.Baking;

[Tool]
[GlobalClass]
internal partial class TraversalCsgBoxWallGrid3D : TraversalAuthoringNode3D
{
    [Export] public TraversalCsgBoxFace Face { get; set; } = TraversalCsgBoxFace.PositiveZ;

    [Export] public float HorizontalInset { get; set; } = 0.1f;
    [Export] public float BottomInset { get; set; } = 0.1f;
    [Export] public float TopInset { get; set; } = 0.1f;
    [Export] public float NormalOffset { get; set; } = 0.05f;

    [Export] public float HorizontalSpacing { get; set; } = 0.65f;
    [Export] public float VerticalSpacing { get; set; } = 0.65f;

    [Export] public bool EntryAllowed { get; set; } = true;
    [Export] public bool GenerateMantleEdgesOnTopRow { get; set; } = true;

    [Export] public float MoveDuration { get; set; } = 0.22f;
    [Export] public float MantleDuration { get; set; } = 0.35f;

    public override void BakeTraversal(TraversalBakeContext context)
    {
        if (!EnabledForBake)
            return;

        CsgBox3D box = GetParent() as CsgBox3D;

        if (box == null)
        {
            GD.PushWarning($"{Name}: TraversalCsgBoxWallGrid3D must be a child of CsgBox3D.");
            return;
        }

        Vector3 size = box.Size;
        Vector3 localNormal = TraversalCsgBoxFaceUtility.GetLocalNormal(Face);
        Vector3 localRight = TraversalCsgBoxFaceUtility.GetLocalRight(Face);

        float faceWidth = TraversalCsgBoxFaceUtility.GetFaceWidth(size, Face);
        float usableWidth = Mathf.Max(0.1f, faceWidth - HorizontalInset * 2.0f);
        float usableHeight = Mathf.Max(0.1f, size.Y - BottomInset - TopInset);

        int columns = Mathf.Max(
            1,
            Mathf.RoundToInt(usableWidth / Mathf.Max(HorizontalSpacing, 0.1f)) + 1
        );

        int rows = Mathf.Max(
            1,
            Mathf.RoundToInt(usableHeight / Mathf.Max(VerticalSpacing, 0.1f)) + 1
        );

        Transform3D boxTransform = box.GlobalTransform;
        Vector3 worldNormal = (boxTransform.Basis * localNormal).Normalized();
        Vector3 worldUp = (boxTransform.Basis * Vector3.Up).Normalized();
        Vector3 faceCenter = TraversalCsgBoxFaceUtility.GetFaceCenterLocal(size, Face);

        float bottomY = -size.Y * 0.5f + BottomInset;
        float topY = size.Y * 0.5f - TopInset;

        var grid = new int[columns, rows];

        for (int yIndex = 0; yIndex < rows; yIndex++)
        {
            float ty = rows == 1 ? 0.0f : yIndex / (float)(rows - 1);
            float localY = Mathf.Lerp(bottomY, topY, ty);

            for (int xIndex = 0; xIndex < columns; xIndex++)
            {
                float tx = columns == 1 ? 0.5f : xIndex / (float)(columns - 1);
                float localX = Mathf.Lerp(-usableWidth * 0.5f, usableWidth * 0.5f, tx);

                Vector3 localPoint =
                    faceCenter +
                    localRight * localX +
                    Vector3.Up * localY +
                    localNormal * NormalOffset;

                Vector3 worldPoint = boxTransform * localPoint;

                TraversalAnchorFlags flags = TraversalAnchorFlags.WallHold;

                if (EntryAllowed)
                    flags |= TraversalAnchorFlags.EntryAllowed;

                bool topRow = yIndex == rows - 1;

                if (topRow)
                {
                    flags |= TraversalAnchorFlags.Ledge | TraversalAnchorFlags.ExitAllowed;

                    if (GenerateMantleEdgesOnTopRow)
                        flags |= TraversalAnchorFlags.MantleTop;
                }

                int anchor = context.Builder.AddAnchor(
                    worldPoint,
                    worldNormal,
                    worldUp,
                    flags,
                    ResolveSurfaceId(context)
                );

                grid[xIndex, yIndex] = anchor;

                if (topRow && GenerateMantleEdgesOnTopRow)
                    context.Builder.AddMantleEdge(anchor, MantleDuration);
            }
        }

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                int current = grid[x, y];

                if (x + 1 < columns)
                {
                    context.Builder.AddBidirectionalEdge(
                        current,
                        grid[x + 1, y],
                        TraversalMoveType.ShimmyRight,
                        TraversalMoveType.ShimmyLeft,
                        TraversalEdgeFlags.EndsAttached,
                        cost: 1.0f,
                        duration: MoveDuration
                    );
                }

                if (y + 1 < rows)
                {
                    context.Builder.AddBidirectionalEdge(
                        current,
                        grid[x, y + 1],
                        TraversalMoveType.ClimbUp,
                        TraversalMoveType.ClimbDown,
                        TraversalEdgeFlags.EndsAttached,
                        cost: 1.0f,
                        duration: MoveDuration
                    );
                }
            }
        }
    }
}
