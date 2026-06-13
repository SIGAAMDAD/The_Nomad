# Galakas MidCity Final Traversal Add-On

This revision combines the existing MidCity CSG module pack with a more diverse traversal-focused component set.

## What was added

### CrossBuilding
General roof-to-roof and building-to-building crossings:
- narrow beams
- broad timber bridges
- rough plank bundles
- damaged/broken plank crossings
- gallery connectors
- angled plank ramps for small height offsets

These are intended for AC-style urban route planning without baking route geometry directly into every building assembly.

### RoofCrossings
Small roof-specific traversal pieces:
- parapet gap steps
- eave grab lines
- flattened ridge walk helpers

Use these as overlays in the final MidCity assembly when you want controlled rooftop flow.

### ScaffoldTraversal
Basic scaffold platforms for temporary/lived-in traversal. These are useful around construction, repair zones, alley service walls, markets, and older commercial blocks.

### VerticalTransition
Subtle climb helpers:
- alternating wall pegs
- window-to-awning sill steps

These are intentionally simple so they do not overcomplicate baking or visual noise.

### RopeAndLineAnchors
CSG-only rope endpoint anchors. Pair these with the glTF rope/clothesline props or with later spline/mesh rope systems.

### GLTF_RopeLines
Lightweight geometry-only `.gltf` files:
- `GalakasMidCity_TRAV_ClothesLine_RopeSag.gltf`
- `GalakasMidCity_TRAV_RopeLine_RoofToRoof.gltf`
- `GalakasMidCity_TRAV_RopeAndClothesLine_Combined.gltf`

These are not CSG; they are mesh props for visual/traversal reference. They have no textures and use embedded glTF buffers, so there are no external `.bin` dependencies.

## Intended workflow

1. Block out buildings with the CSG BuildingTypes.
2. Convert finalized building assemblies to GLTF/GLB as needed.
3. Overlay these traversal components in the full MidCity assembly.
4. Bake/export the final traversal layout only after routes feel good.

## Placement convention

Most crossing components span along local `Z`. Width is usually local `X`, height is local `Y`.

Keep these as separate route overlays until late in the blockout phase, especially for MidCity, so roof gaps, alley widths, and route readability can be adjusted without rebaking every building.
