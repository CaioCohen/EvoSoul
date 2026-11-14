---
name: blender-animation-binding
description: Bind, switch, and validate Blender actions on Objects, Armatures, and Shape Key datablocks, especially Blender 5.x layered Actions and Action Slots.
---

# Blender animation binding

Use this skill whenever creating, editing, switching, or diagnosing Blender animations. Use the live Blender MCP when the project workflow requests it. Preserve existing meshes, keys, actions, and keyframes unless the user explicitly requests a change.

## Bind to the animated datablock

An Action containing curves is not necessarily playing. Inspect the intended ID's own animation_data: animation belongs to its actual target datablock.

- Object transform or custom-property curves: obj.animation_data.
- Pose-bone curves: armature_obj.animation_data.
- Relative Shape Key values: obj.data.shape_keys.animation_data (the Key datablock, not the mesh Object or Mesh datablock).

Blender 5.x layered Actions can have Action Slots. Inspect action.slots and each slot's target_id_type; select a slot compatible with the owner (KEY for Shape Keys). Assign both Action and slot on the correct owner:

    owner.animation_data_create()
    ad = owner.animation_data
    ad.action = action
    ad.action_slot = next(s for s in action.slots if s.target_id_type == expected_id_type)

Some Actions have no slots (legacy Actions); only assign action_slot when a compatible slot exists. Set the Action before its slot. Never “fix” an unassigned action by creating new curves or replacing keys before inspecting its slots and bindings.

## Switch and preview independently

When switching, inspect both the Object/Armature and animated data block for stray active Actions. Clear a misplaced Object Action if the intended curves animate Shape Keys. Inspect all relevant NLA tracks and strips (mute state, action, slot, blend type, influence and range): NLA can continue contributing or override a newly selected Action. For isolated previews, mute conflicting NLA tracks or disable NLA evaluation temporarily; preserve tracks and restore their state after testing. Do not delete NLA data as a shortcut.

Before previewing another Action, set animated keys or properties to their neutral values when they are not fully keyed by every Action. Then assign that Action and its compatible slot, set an appropriate frame range, move to the first frame and update the view layer. Keying neutral values across all relevant channels is preferable when authoring Actions intended to switch independently. A Shape Key Editor context (space.mode = 'SHAPEKEY', space.ui_mode = 'SHAPEKEY') helps ensure the Action Editor selector is operating on the Shape Key datablock. Selecting an Action on the mesh Object does not switch a Shape Key Action.

## Validate evaluated animation

Do not infer playback from the Action name, keyframe count, or visible key markers. For each Action independently:

1. Confirm the Action is assigned on its target ID and the active slot targets that ID type. Inspect NLA influence.
2. Evaluate a start and keyed middle/peak frame with scene.frame_set(frame) and update the dependency graph/view layer.
3. Read animated values from the intended datablock and confirm at least one changes. For Actions, inspect Blender 5.x channel bags via action.layers[].strips[].channelbags[].fcurves (do not rely on legacy action.fcurves).
4. For shape animation, compare obj.evaluated_get(depsgraph).to_mesh() vertex coordinates across frames and clear the temporary evaluated mesh. For transforms, compare evaluated matrices or transform values; for armatures, compare evaluated pose-bone transforms.
5. Switch through the other Actions, checking their values and evaluated motion. Verify no previous Action or NLA strip leaves residual deformation. Visually inspect representative frames using Blender MCP look when available.
6. Restore the requested default Action, compatible slot, neutral starting frame, and playback range, then save the .blend.

For Action-specific reference ranges, sample authored keyframes and verify an observable value or evaluated-pose change. A curve may legitimately be constant; validate the intended channels rather than requiring every curve to change.

## Shape Key diagnostic pattern (Blender 5.3)

    obj = bpy.data.objects['GEO-character']
    keys = obj.data.shape_keys
    ad = keys.animation_data
    # Check obj.animation_data.action too: it does not drive key values.
    for action_name in ('Walk', 'Attack', 'Death'):
        action = bpy.data.actions[action_name]
        slot = next(s for s in action.slots if s.target_id_type == 'KEY')
        ad.action = action
        ad.action_slot = slot
        scene.frame_set(start_frame)
        bpy.context.view_layer.update()
        before = [k.value for k in keys.key_blocks]
        scene.frame_set(peak_frame)
        bpy.context.view_layer.update()
        after = [k.value for k in keys.key_blocks]
        assert before != after
        # Also compare evaluated mesh coordinates; values alone do not prove visible deformation.

Do not export or modify gameplay/scene systems unless requested. Report the target datablock, slot, NLA state, and observable validation result when diagnosing binding problems.
