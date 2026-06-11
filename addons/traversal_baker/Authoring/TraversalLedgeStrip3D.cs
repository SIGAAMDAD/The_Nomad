using Godot;
using System.Collections.Generic;
using Nomad.Game.Sdk.Traversal;

namespace Nomad.Game.Traversal.Baking;

[Tool]
[GlobalClass]
internal partial class TraversalLedgeStrip3D : TraversalAuthoringNode3D
{
    [Export] public float Length { get; set; } = 4.0f;
    [Export] public float SampleSpacing { get; set; } = 0.65f;

    [Export] public bool EntryAllowed { get; set; } = true;
    [Export] public bool ExitAllowed { get; set; } = true;
    [Export] public bool GenerateShimmyEdges { get; set; } = true;
    [Export] public bool GenerateMantleEdges { get; set; } = true;

    [Export] public float ShimmyDuration { get; set; } = 0.22f;
    [Export] public float MantleDuration { get; set; } = 0.35f;

    public override void BakeTraversal(TraversalBakeContext context)
    {
        if (!EnabledForBake)
            return;

        float length = Mathf.Max(Length, 0.01f);
        float spacing = Mathf.Max(SampleSpacing, 0.1f);
        int sampleCount = Mathf.Max(2, Mathf.RoundToInt(length / spacing) + 1);

        TraversalAnchorFlags flags = TraversalAnchorFlags.WallHold | TraversalAnchorFlags.Ledge;

        if (EntryAllowed)
            flags |= TraversalAnchorFlags.EntryAllowed;
        if (ExitAllowed)
            flags |= TraversalAnchorFlags.ExitAllowed;
        if (GenerateMantleEdges)
            flags |= TraversalAnchorFlags.MantleTop;

        var anchors = new List<int>(sampleCount);

        for (int i = 0; i < sampleCount; i++)
        {
            float t = sampleCount == 1 ? 0.5f : i / (float)(sampleCount - 1);
            float x = Mathf.Lerp(-length * 0.5f, length * 0.5f, t);
            Vector3 position = GlobalPosition + TraversalRight * x;

            int anchor = context.Builder.AddAnchor(
                position,
                OutwardNormal,
                TraversalUp,
                flags,
                ResolveSurfaceId(context)
            );

            anchors.Add(anchor);

            if (GenerateMantleEdges)
                context.Builder.AddMantleEdge(anchor, MantleDuration);
        }

        if (!GenerateShimmyEdges)
            return;

        for (int i = 0; i < anchors.Count - 1; i++)
        {
            context.Builder.AddBidirectionalEdge(
                anchors[i],
                anchors[i + 1],
                TraversalMoveType.ShimmyRight,
                TraversalMoveType.ShimmyLeft,
                TraversalEdgeFlags.EndsAttached,
                cost: 1.0f,
                duration: ShimmyDuration
            );
        }
    }
}
