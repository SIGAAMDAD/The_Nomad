#if TOOLS
using Godot;
using System.Collections.Generic;
using Nomad.Game.Traversal.Baking;
using Nomad.Game.Prefabs;

namespace Nomad.Game.Traversal.Editor;

[Tool]
internal partial class TraversalBakerPlugin : EditorPlugin
{
    private const string MenuItemName = "Bake Traversal Graph";
    private const string OutputPathSetting = "traversal_baker/output_path";
    private const string SpatialCellSizeSetting = "traversal_baker/spatial_cell_size";
    private const string DefaultEdgeDurationSetting = "traversal_baker/default_edge_duration";
    private const string DefaultSurfaceIdSetting = "traversal_baker/default_surface_id";

    private const string DefaultOutputPath = "res://Traversal/GeneratedTraversalDatabase.tres";

    public override void _EnterTree()
    {
        EnsureProjectSettings();

        // This deliberately uses the Tools menu only.
        // Do not add docks, bottom panels, or main screens here.
        AddToolMenuItem(MenuItemName, Callable.From(BakeCurrentEditedScene));
    }

    public override void _ExitTree()
    {
        RemoveToolMenuItem(MenuItemName);
    }

    private void BakeCurrentEditedScene()
    {
        Node root = GetEditedSceneRootSafe();

        if (root == null)
        {
            GD.PushWarning("Traversal bake failed: no edited scene root is available.");
            return;
        }

        string outputPath = GetProjectString(OutputPathSetting, DefaultOutputPath);
        float spatialCellSize = GetProjectFloat(SpatialCellSizeSetting, 2.0f);
        float defaultEdgeDuration = GetProjectFloat(DefaultEdgeDurationSetting, 0.22f);
        ushort defaultSurfaceId = (ushort)Mathf.Clamp(GetProjectInt(DefaultSurfaceIdSetting, 1), 0, ushort.MaxValue);

        List<ITraversalBakeSource> sources = new();
        GatherBakeSources(root, sources);

        if (sources.Count == 0)
        {
            GD.PushWarning(
                "Traversal bake found no authoring nodes. " +
                "Add TraversalPointAnchor3D, TraversalLedgeStrip3D, TraversalWallGrid3D, " +
                "TraversalCsgBoxLedge3D, or TraversalCsgBoxWallGrid3D to the current scene."
            );
            return;
        }

        TraversalBakeBuilder builder = new();
        TraversalBakeContext context = new(
            builder,
            root,
            defaultEdgeDuration,
            defaultSurfaceId
        );

        foreach (ITraversalBakeSource source in sources)
            source.BakeTraversal(context);

        TraversalDatabase database = builder.BuildDatabase(spatialCellSize);

        if (database.Positions.Length == 0)
        {
            GD.PushWarning("Traversal bake produced zero anchors. Check whether authoring nodes are disabled.");
            return;
        }

        Error dirError = EnsureOutputDirectory(outputPath);
        if (dirError != Error.Ok)
        {
            GD.PushError($"Traversal bake failed: could not create output directory for {outputPath}. Error: {dirError}");
            return;
        }

        Error saveError = ResourceSaver.Save(database, outputPath);

        if (saveError != Error.Ok)
        {
            GD.PushError($"Traversal bake failed: could not save {outputPath}. Error: {saveError}");
            return;
        }

        GD.Print(
            $"Traversal bake complete: {outputPath}. " +
            $"Sources: {sources.Count}, Anchors: {database.Positions.Length}, Edges: {database.EdgeTargets.Length}."
        );
    }

    private Node GetEditedSceneRootSafe()
    {
        EditorInterface editor = GetEditorInterface();

        if (editor != null)
        {
            Node root = editor.GetEditedSceneRoot();
            if (root != null)
                return root;
        }

        return GetTree()?.EditedSceneRoot;
    }

    private static void GatherBakeSources(Node node, List<ITraversalBakeSource> sources)
    {
        if (node is ITraversalBakeSource source)
            sources.Add(source);

        foreach (Node child in node.GetChildren())
            GatherBakeSources(child, sources);
    }

    private static Error EnsureOutputDirectory(string outputPath)
    {
        string directory = GetDirectory(outputPath);

        if (string.IsNullOrEmpty(directory) || directory == "res://")
            return Error.Ok;

        string absolute = ProjectSettings.GlobalizePath(directory);
        return DirAccess.MakeDirRecursiveAbsolute(absolute);
    }

    private static string GetDirectory(string path)
    {
        int slash = path.LastIndexOf('/');
        if (slash < 0)
            return "res://";
		
        return path.Substring(0, slash);
    }

    private static void EnsureProjectSettings()
    {
        EnsureSetting(OutputPathSetting, DefaultOutputPath);
        EnsureSetting(SpatialCellSizeSetting, 2.0f);
        EnsureSetting(DefaultEdgeDurationSetting, 0.22f);
        EnsureSetting(DefaultSurfaceIdSetting, 1);
    }

    private static void EnsureSetting(string name, Variant value)
    {
        if (!ProjectSettings.HasSetting(name))
		{
            ProjectSettings.SetSetting(name, value);
		}
    }

    private static string GetProjectString(string name, string fallback)
    {
        Variant value = ProjectSettings.GetSetting(name, fallback);
        return value.VariantType == Variant.Type.String ? value.AsString() : fallback;
    }

    private static float GetProjectFloat(string name, float fallback)
    {
        Variant value = ProjectSettings.GetSetting(name, fallback);
        if (value.VariantType == Variant.Type.Float)
		{
            return value.AsSingle();
		}
        if (value.VariantType == Variant.Type.Int)
		{
            return value.AsInt32();
		}
        return fallback;
    }

    private static int GetProjectInt(string name, int fallback)
    {
        Variant value = ProjectSettings.GetSetting(name, fallback);
        if (value.VariantType == Variant.Type.Int)
		{
            return value.AsInt32();
		}
        if (value.VariantType == Variant.Type.Float)
		{
            return Mathf.RoundToInt(value.AsSingle());
		}
        return fallback;
    }
}
#endif
