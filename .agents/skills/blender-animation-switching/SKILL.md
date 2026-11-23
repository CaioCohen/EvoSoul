---
name: blender-animation-switching
description: Switch and validate independent Blender 5.3 animation Actions, including slots, Shape Keys, and NLA conflicts. Use alongside blender-animation-binding when creating or editing Blender animations.
---

# Blender animation switching

Read the project’s `blender-animation-binding` skill first; it owns Action Slot and evaluated-motion fundamentals. Inspect the actual animated owners and Action Editor context before altering bindings. Selecting an Action on a root or mesh does not switch the armature or Shape Key datablock driving the model.

Use `scripts/switch_action.py` inside the live Blender Python environment for an explicitly identified owner and Action. It binds the compatible slot and resets unkeyed pose channels. It refuses ambiguous slots and active NLA tracks rather than deleting or muting user data. For coordinated Object and Shape Key Actions, switch each owner explicitly, resetting unkeyed Shape Key values to the intended neutral state; inspect any other animation owners for residual Actions. Never clear unrelated animation or assume all shape keys have a neutral value of zero.

Before saving, select the actual animated owner and ensure the Action Editor follows it (unpin it when needed). For every Action, switch from another Action, sample its start and meaningful peak frame, and compare evaluated mesh vertices or pose matrices. Repeat the same Action after a different predecessor and compare the results to detect residual values. Inspect relevant NLA tracks and Shape Key owners; temporarily isolate conflicts without deleting tracks. Visually inspect representative frames with MCP `look`. Restore the desired default Action, slot, frame, and editor selection, then save.
