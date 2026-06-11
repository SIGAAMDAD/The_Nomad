# Nomad Terrain3D Vegetation Pack

This is a lightweight placeholder/prototype vegetation pack for Godot 4 / Terrain3D.

## Contents

- `meshes/*.obj` — combined single-object vegetation meshes, with baked transforms and bottom-centered origins.
- `meshes/nomad_vegetation.mtl` — simple material colors for OBJ import.
- `scenes/*.tscn` — Godot MeshInstance3D wrapper scenes for quick preview/manual placement.
- `terrain3d_vegetation_palette.json` — suggested usage notes.

## Terrain3D usage

For Terrain3D's foliage instancer, prefer the OBJ mesh assets directly instead of complex scene hierarchies.

The included `.tscn` files are intentionally simple root `MeshInstance3D` scenes that reference the OBJ mesh. They are useful for preview, manual placement, and quick testing. If your Terrain3D workflow asks for a Mesh resource, select/import the OBJ mesh from the `meshes` folder.

Folder path expected by the `.tscn` files:

`res://nomad_terrain3d_vegetation/`

If you move the folder somewhere else, update the `res://` paths inside the `.tscn` files.

## Art direction

The pack is tuned for Bellatum Terrae / Galakas:
- dry Safe Sands vegetation
- MidCity maintained greenery
- ornamental market trees
- desert scrub, reeds, vines, and small flowers

These are intentionally low-poly and scatter-safe. Replace them later with authored hero foliage if needed.

## Asset list

- grass_dry_tuft_a
- grass_green_tuft_a
- dune_grass_sparse
- reeds_clump_tall
- flower_yellow_patch
- flower_red_patch
- dry_scrub_bush_small
- thorn_bush_medium
- midcity_low_hedge
- ground_vines_creeper
- midcity_wall_vines_hanging
- small_desert_acacia
- midcity_olive_tree
- dead_tree_snag
- young_date_palm
- small_ribbed_cactus

## Import notes

These assets use simple material slots from the OBJ/MTL. In Godot, you can replace the imported materials with your actual project vegetation materials.

Suggested material setup:
- vegetation roughness: 0.75-0.95
- bark roughness: 0.85-1.0
- metallic: 0.0
- alpha scissor/blend only if you later replace cards with alpha-cutout textures
- disable collision for grass and flowers
- use manual collision only for trees if the player can touch them

## Scale notes

Units are meters.
The origin is at the ground contact point, centered around X/Z zero, so random yaw/scale in Terrain3D should behave predictably.