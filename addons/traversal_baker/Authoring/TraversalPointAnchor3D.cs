using Godot;
using Nomad.Game.Sdk.Traversal;

namespace Nomad.Game.Traversal.Baking;

[Tool]
[GlobalClass]
internal partial class TraversalPointAnchor3D : TraversalAuthoringNode3D
{
    [Export] public TraversalAnchorFlags Flags { get; set; } =
        TraversalAnchorFlags.WallHold |
        TraversalAnchorFlags.EntryAllowed;

    [Export] public bool GenerateMantleEdge { get; set; } = false;
    [Export] public float MantleDuration { get; set; } = 0.35f;

    public override void BakeTraversal(TraversalBakeContext context)
    {
        if (!EnabledForBake)
            return;

        int anchor = context.Builder.AddAnchor(
            GlobalPosition,
            OutwardNormal,
            TraversalUp,
            Flags,
            ResolveSurfaceId(context)
        );

        if (GenerateMantleEdge)
            context.Builder.AddMantleEdge(anchor, MantleDuration);
    }
}
