# Traversal Baker Addon

Standalone Godot 4 C# editor addon for the traversal runtime discussed in chat.

## What it does

- Adds **Project > Tools > Bake Traversal Graph**.
- Scans the currently edited scene for traversal authoring nodes.
- Bakes those nodes into the existing `Game.Traversal.TraversalDatabase` resource format.
- Does **not** create docks, bottom panels, or main screens, so it should not hide or replace the FileSystem dock.

## Required existing runtime types

This addon assumes your project already contains the runtime traversal system with:

- `Game.Traversal.TraversalDatabase`
- `Game.Traversal.TraversalAnchorFlags`
- `Game.Traversal.TraversalEdgeFlags`
- `Game.Traversal.TraversalMoveType`

It intentionally does not duplicate those runtime types to avoid namespace/class conflicts.

## Output path

Default output:

```text
res://Traversal/GeneratedTraversalDatabase.tres
```

You can change it in Project Settings after enabling the addon:

```text
traversal_baker/output_path
```

## Authoring nodes included

- `TraversalPointAnchor3D`
- `TraversalLedgeStrip3D`
- `TraversalWallGrid3D`
- `TraversalCsgBoxLedge3D`
- `TraversalCsgBoxWallGrid3D`

## CSG usage

For a CSG wall:

```text
CsgBox3D
└── TraversalCsgBoxWallGrid3D
```

For a CSG ledge:

```text
CsgBox3D
└── TraversalCsgBoxLedge3D
```

Then use **Project > Tools > Bake Traversal Graph**.
