"""Blender helper: bind a named Action to its actual owner, preserving NLA data."""
import bpy


def switch_action(owner, action, *, neutral_shape_values=None):
    """Switch an Object or Key owner; callers supply intended neutral key values."""
    ad = owner.animation_data_create()
    if any(not track.mute and any(not strip.mute for strip in track.strips)
           for track in ad.nla_tracks) and ad.use_nla:
        raise RuntimeError("Active NLA strips may affect playback; inspect them first")
    expected = owner.id_type
    slots = [slot for slot in action.slots if slot.target_id_type == expected]
    if len(slots) != 1:
        raise ValueError(f"Expected one {expected} slot, found {len(slots)}")
    if owner.id_type == 'OBJECT' and owner.type == 'ARMATURE':
        for bone in owner.pose.bones:
            bone.location = (0, 0, 0)
            bone.rotation_euler = (0, 0, 0)
            bone.rotation_quaternion = (1, 0, 0, 0)
            bone.rotation_axis_angle = (0, 0, 1, 0)
            bone.scale = (1, 1, 1)
    if owner.id_type == 'KEY' and neutral_shape_values is not None:
        for name, value in neutral_shape_values.items():
            owner.key_blocks[name].value = value
    ad.action = action
    ad.action_slot = slots[0]
    bpy.context.view_layer.update()
    return slots[0].identifier
