# Galakas MidCity Example Scenes

## GalakasMidCity_EXAMPLE_MarketStreet_RooftopLoop_CSG.tscn

A sample MidCity blockout slice assembled from the existing Galakas CSG building-type modules and traversal components.

### Layout intent
- A central market street with a north commercial arcade row and a south rooftop-route row.
- North side emphasizes commerce: long arcade receiver, triple shop row, mercer corner, alehouse, and guildhall massing.
- South side emphasizes traversal and backstreet access: wide roofline run, bridge receiver, side-door cloth hall, side-entry shop row, and ropewalk/service building.
- East/west caps suggest denser city continuation rather than an isolated test row.
- CSG street slabs, curbs, and alley plates are included for quick lighting and navigation tests.

### Traversal intent
- Mid-height gallery crossings connect the upper commercial layer.
- Roof-height timber bridges and rope lines suggest AC-style movement across building gaps.
- Wall pegs, scaffold platform, and a wide stair create basic vertical access without forcing traversal into every building module.

### Notes
- This scene uses instances of existing component scenes rather than reconstructing building geometry.
- The GLTF rope/clothesline props are visual reference props; use CSG or custom collision if you want them to become actual traversal/collision elements.
- All paths are `res://Assets/Models/Environment/Settlements/Modules/Galakas/MidCity/Houses/...`.
