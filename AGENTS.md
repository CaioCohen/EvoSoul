
## Blender and Unity Asset Workflow

For Blender animation creation, editing, action switching, or playback diagnosis, use .agents/skills/blender-animation-binding/SKILL.md. Follow it to bind Actions and Action Slots to the correct animated datablock, inspect NLA conflicts, and validate evaluated motion before saving.

Also use .agents/skills/blender-animation-switching/SKILL.md for Blender animation creation and editing, including independent Action switching and validation.

For all 3D asset creation tasks:

1. Use the `text-to-blender` skill to plan and execute Blender modeling.
2. Follow the `lowpoly-style` skill to maintain a consistent low-poly visual style.
3. Use the `blender-export` skill for exporting assets to Unity.
4. Use the `unity-cli` skill for Unity integration and validation.

For any stage surface intended to read as ocean, lava, fog, void, or another
unbounded backdrop, use `.agents/skills/infinite-environment-surface/SKILL.md`.

### Rules

- Prefer the live Blender MCP for modeling operations.
- Verify the MCP connection before creating or modifying models.
- Save source assets as `.blend` files inside `blender/`.
- Export game-ready assets to the Unity project's Assets directory.
- Preserve proper scale, pivot, geometry and materials.
- Optimize all assets for Android.
- Inspect models visually before exporting.
- Verify that exported models import correctly in Unity.
- Never claim an MCP interaction succeeded unless the tool actually ran.
- Never silently substitute a standalone Blender script for a requested MCP operation.
- Report modeling, export and import failures.
- Every tree placed in a playable stage needs a simple non-trigger trunk collider.
  Match the established Stage 01 collision behavior: it must block the player at
  the trunk without turning branches/canopy into a large invisible obstacle.

