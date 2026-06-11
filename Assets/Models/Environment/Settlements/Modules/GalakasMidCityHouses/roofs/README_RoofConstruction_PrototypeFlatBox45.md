# Galakas MidCity Roof Construction - v6 Axis Fixed

The v6 roofs correct the bad axis layout from v4/v5.

## Important fix

A roof slab must begin as a flat horizontal box:

```text
size = Vector3(width_or_slope_run, thickness_y, depth_or_slope_run)
```

The broad visible roof surface is the local X/Z face. The box is thin on local Y.

The broken versions effectively used this layout:

```text
size = Vector3(width, slope_run_y, thickness_z)
```

That makes the box behave like a standing wall/fin once rotated.

## Correct rotations

For slopes that rise across Z:

```text
Front slope: rotation = Vector3(-45deg, 0, 0)
Back slope:  rotation = Vector3(+45deg, 0, 0)
```

For slopes that rise across X:

```text
Left slope:  rotation = Vector3(0, 0, +45deg)
Right slope: rotation = Vector3(0, 0, -45deg)
```

All roof modules are material-free. CSG node names indicate intended materials, for example:

```text
Mat_RoofTile_BoxPlane_FrontSlope_XNeg45
Mat_WoodBeam_RidgeCap_FlatBox
```

Assemblies still instance these roof scenes through `ext_resource`, so changing these roof files fixes the composed building scenes too.
