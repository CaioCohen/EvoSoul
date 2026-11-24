---
name: infinite-environment-surface
description: Create or adapt a lightweight Unity surface that reads as an unbounded ocean, lava field, fog layer, or void instead of an incomplete visible platform.
---

# Infinite Environment Surface

Use this for large background surfaces whose visible edge would break the
illusion of a continuous world. It is not for ordinary bounded gameplay floors.

## Reference and construction

1. Inspect the closest existing stage surface first. Reuse its proven shader,
   tiling, motion component, render order, and camera-relative behavior when
   appropriate; make a material variant rather than changing the reference.
2. Use one simple plane or other minimal mesh sized far beyond the maximum
   gameplay-camera view. Calculate its coverage from camera range and scene
   bounds; do not rely on a small platform hidden by its colour.
3. Give the new surface a seamless, repeatable texture with visible large-scale
   variation at gameplay distance. For lava, distinguish crust, hot flow, and
   brighter veins; for water, avoid a flat single-colour fill.
4. Prefer a shared mobile-friendly shader/material and UV scrolling over dense
   geometry, particles, or high-resolution textures. Give decorative surfaces
   no collider unless gameplay explicitly needs one.

## Validation

- Inspect from the normal gameplay camera near the island edge and from the
  player spawn. No mesh boundary or texture seam may be visible.
- Test both direct scene start and the gameplay scene-transition path, since
  material instances and surface-motion components can initialize differently.
- Confirm the texture tiles and motion are active, the surface does not block
  movement unintentionally, and the scene remains lightweight.

