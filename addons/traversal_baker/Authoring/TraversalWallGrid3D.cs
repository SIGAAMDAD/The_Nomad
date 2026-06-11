using Godot;
using Nomad.Game.Sdk.Traversal;

namespace Nomad.Game.Traversal.Baking;

[Tool]
[GlobalClass]
internal partial class TraversalWallGrid3D : TraversalAuthoringNode3D
{
    [Export] public float Width { get; set; } = 4.0f;
    [Export] public float Height { get; set; } = 4.0f;

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

        float width = Mathf.Max(Width, 0.1f);
        float height = Mathf.Max(Height, 0.1f);
        float hSpacing = Mathf.Max(HorizontalSpacing, 0.1f);
        float vSpacing = Mathf.Max(VerticalSpacing, 0.1f);

        int columns = Mathf.Max(1, Mathf.RoundToInt(width / hSpacing) + 1);
        int rows = Mathf.Max(1, Mathf.RoundToInt(height / vSpacing) + 1);
        var grid = new int[columns, rows];

        for (int y = 0; y < rows; y++)
        {
            float ty = rows == 1 ? 0.0f : y / (float)(rows - 1);
            float localY = Mathf.Lerp(0.0f, height, ty);

            for (int x = 0; x < columns; x++)
            {
                float tx = columns == 1 ? 0.5f : x / (float)(columns - 1);
                float localX = Mathf.Lerp(-width * 0.5f, width * 0.5f, tx);

                TraversalAnchorFlags flags = TraversalAnchorFlags.WallHold;

                if (EntryAllowed)
                    flags |= TraversalAnchorFlags.EntryAllowed;

                if (y == rows - 1)
                {
                    flags |= TraversalAnchorFlags.Ledge | TraversalAnchorFlags.ExitAllowed;

                    if (GenerateMantleEdgesOnTopRow)
                        flags |= TraversalAnchorFlags.MantleTop;
                }

                Vector3 position = GlobalPosition + TraversalRight * localX + TraversalUp * localY;

                int anchor = context.Builder.AddAnchor(
                    position,
                    OutwardNormal,
                    TraversalUp,
                    flags,
                    ResolveSurfaceId(context)
                );

                grid[x, y] = anchor;

                if (y == rows - 1 && GenerateMantleEdgesOnTopRow)
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
