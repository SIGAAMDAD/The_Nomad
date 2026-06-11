using Godot;
using Nomad.Game.Sdk.Traversal;

namespace Nomad.Game.Traversal.Baking;

[Tool]
[GlobalClass]
internal abstract partial class TraversalAuthoringNode3D : Node3D, ITraversalBakeSource
{
    [Export] public bool EnabledForBake { get; set; } = true;
    [Export] public ushort SurfaceId { get; set; } = 0;
    [Export] public bool AddToBakeGroupAutomatically { get; set; } = true;

    public override void _EnterTree()
    {
        if (!global::Godot.Engine.IsEditorHint())
            return;

        if (!AddToBakeGroupAutomatically)
            return;

        if (!IsInGroup(TraversalBakeGroups.BakeSource))
            AddToGroup(TraversalBakeGroups.BakeSource);
    }

    public abstract void BakeTraversal(TraversalBakeContext context);

    protected Vector3 OutwardNormal => GlobalTransform.Basis.Z.Normalized();
    protected Vector3 TraversalUp => GlobalTransform.Basis.Y.Normalized();
    protected Vector3 TraversalRight => GlobalTransform.Basis.X.Normalized();

    protected ushort ResolveSurfaceId(TraversalBakeContext context)
    {
        return SurfaceId == 0 ? context.DefaultSurfaceId : SurfaceId;
    }
}
