using Godot;

namespace Nomad.Game.Traversal.Baking;

internal sealed class TraversalBakeContext
{
    public readonly TraversalBakeBuilder Builder;
    public readonly Node SourceRoot;
    public readonly float DefaultEdgeDuration;
    public readonly ushort DefaultSurfaceId;

    public TraversalBakeContext(
        TraversalBakeBuilder builder,
        Node sourceRoot,
        float defaultEdgeDuration,
        ushort defaultSurfaceId)
    {
        Builder = builder;
        SourceRoot = sourceRoot;
        DefaultEdgeDuration = defaultEdgeDuration;
        DefaultSurfaceId = defaultSurfaceId;
    }
}

internal interface ITraversalBakeSource
{
    void BakeTraversal(TraversalBakeContext context);
}
