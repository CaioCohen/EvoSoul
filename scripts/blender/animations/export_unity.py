"""Bake three independent shape-key actions into one FBX for Unity clip slicing.
Run by the main agent in the live Blender MCP after visual validation.
"""
from pathlib import Path
import sys
import bpy
sys.path.insert(0, str(Path(__file__).resolve().parent))
from common import OBJECT_NAME, KEY_NAMES, CLIPS, activate, neutral, fcurves

ROOT = Path(__file__).resolve().parents[3]
RANGES = {"Walk": (0, 21), "Attack": (30, 45), "Death": (60, 96)}
EXPORT_ACTION = "VirginDrop_ExportTimeline"

def build_export_timeline():
    obj = bpy.data.objects[OBJECT_NAME]
    keys = obj.data.shape_keys
    samples = {}
    for name, (offset, end) in RANGES.items():
        activate(name)
        for local in range(CLIPS[name] + 1):
            bpy.context.scene.frame_set(local)
            samples[offset + local] = [keys.key_blocks[k].value for k in KEY_NAMES]
    neutral(obj)
    old = bpy.data.actions.get(EXPORT_ACTION)
    if old:
        bpy.data.actions.remove(old)
    for frame in range(97):
        values = samples.get(frame, [0.0] * len(KEY_NAMES))
        for key_name, value in zip(KEY_NAMES, values):
            key = keys.key_blocks[key_name]
            key.value = value
            key.keyframe_insert(data_path="value", frame=frame)
    action = keys.animation_data.action
    action.name = EXPORT_ACTION
    action.use_fake_user = True
    action["evosoul_owned"] = True
    action["purpose"] = "Export-only sampled assembly; author with Walk, Attack, Death actions"
    for curve in fcurves(action):
        for point in curve.keyframe_points:
            point.interpolation = "LINEAR"
        curve.extrapolation = "CONSTANT"
    scene = bpy.context.scene
    scene.frame_start = 0
    scene.frame_end = 96
    scene.frame_set(0)
    bpy.context.view_layer.update()
    return action

def export():
    obj = bpy.data.objects[OBJECT_NAME]
    build_export_timeline()
    bpy.ops.object.select_all(action="DESELECT")
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj
    output = ROOT / "unity/EvoSoul/Assets/Art/Models/virgin_drop.fbx"
    bpy.ops.export_scene.fbx(
        filepath=str(output), use_selection=True, object_types={"MESH"},
        global_scale=1, apply_unit_scale=True, apply_scale_options="FBX_SCALE_ALL",
        bake_space_transform=True, use_mesh_modifiers=False,
        mesh_smooth_type="FACE", use_triangles=False,
        bake_anim=True, bake_anim_use_all_actions=False,
        bake_anim_use_nla_strips=False, bake_anim_step=1,
        bake_anim_simplify_factor=0, bake_anim_force_startend_keying=True,
        axis_forward="-Z", axis_up="Y", path_mode="AUTO",
    )
    neutral(obj)
    bpy.context.scene.frame_end = 96
    print({"fbx": str(output), "bytes": output.stat().st_size, "clip_frames": RANGES})
    return str(output)

if __name__ == "__main__":
    export()
