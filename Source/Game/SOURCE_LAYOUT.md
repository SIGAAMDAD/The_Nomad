# The Nomad source layout

This layout is intentionally single-assembly friendly. The code under `Game/` is expected to compile into `TheNomad.dll` from the root `TheNomad.csproj`; the old per-layer project files were removed from this package.

## Top-level folders

- `Sdk/` - public game contracts, IDs, definitions, and data shapes.
- `Gameplay/` - runtime gameplay systems: player, combat, inventory, NPCs, traversal, story, renown, world simulation, compiled runtime database types.
- `Multiplayer/` - multiplayer session, lobby, match, network simulation, and multiplayer-specific caches.
- `Content/` - catalogs, compilers, mod/module loading, content/runtime helpers, and asset/resource caches.
- `Presentation/` - UI/menu/HUD presentation code and Godot UI scenes/widgets.
- `Prefabs/` - player/world/rendering prefab scripts and scenes that are not gameplay systems.
- `Streaming/` - region streaming core, planning, runtime state, streamed region nodes/resources.
- `Godot/` - Godot host/bootstrap/composition and Godot-specific rendering/world startup glue.
- `Properties/` - assembly metadata.

## Boundary note

This is a source-layout cleanup, not a multi-assembly architecture pass. Gameplay code remains gameplay code. Files were not moved into a Godot adapter layer merely because they reference Godot types.
