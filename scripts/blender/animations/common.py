"""Shared Blender 5.3 shape-key clip authoring. Run only inside live Blender."""
import bpy

OBJECT_NAME = "GEO-VirginDrop"
KEY_NAMES = ("Walk_Stretch", "Walk_Gather", "Attack_Anticipate",
             "Attack_Lunge", "Death_Soften", "Death_Puddle")
CLIPS = {"Walk": 21, "Attack": 15, "Death": 36}
OWNED = {"Walk": KEY_NAMES[:2], "Attack": KEY_NAMES[2:4], "Death": KEY_NAMES[4:]}

def fcurves(action):
    for layer in action.layers:
        for strip in layer.strips:
            for bag in strip.channelbags:
                yield from bag.fcurves

def neutral(obj=None):
    obj = obj or bpy.data.objects[OBJECT_NAME]
    keys = obj.data.shape_keys
    if keys:
        if keys.animation_data:
            keys.animation_data.action = None
        for key in keys.key_blocks:
            key.value = 0.0
    bpy.context.scene.frame_set(0)
    bpy.context.view_layer.update()

def build_clip(name, end_frame, poses, tracks):
    if name not in CLIPS or end_frame != CLIPS[name]:
        raise ValueError("Clip duration differs from shared contract")
    if set(poses) != set(OWNED[name]) or set(tracks) != set(poses):
        raise ValueError("Each worker must supply exactly its two owned shapes/tracks")
    obj = bpy.data.objects[OBJECT_NAME]
    if len(obj.data.vertices) != 242:
        raise ValueError("Unexpected character topology")
    if obj.data.shape_keys is None:
        obj.shape_key_add(name="Basis", from_mix=False)
    for key_name in KEY_NAMES:
        if key_name not in obj.data.shape_keys.key_blocks:
            obj.shape_key_add(name=key_name, from_mix=False)
    keys = obj.data.shape_keys
    neutral(obj)
    basis = keys.key_blocks["Basis"]
    for key_name, deform in poses.items():
        key = keys.key_blocks[key_name]
        for i, (source, target) in enumerate(zip(basis.data, key.data)):
            co = tuple(deform(source.co.copy(), i))
            if len(co) != 3 or any(not __import__("math").isfinite(c) for c in co):
                raise ValueError("Invalid shape coordinate")
            if co[2] < -1e-7:
                raise ValueError("Shape penetrates ground")
            target.co = co
    existing = bpy.data.actions.get(name)
    if existing:
        if not existing.get("evosoul_owned"):
            raise ValueError("Refusing to overwrite unrelated action")
        bpy.data.actions.remove(existing)
    keys.animation_data_create()
    keys.animation_data.action = None
    # keyframe_insert creates the correct KEY slot/layer/channelbag in Blender 5.3.
    for key_name in KEY_NAMES:
        schedule = tracks.get(key_name, [(0, 0.0), (end_frame, 0.0)])
        if schedule[0][0] != 0 or schedule[-1][0] != end_frame:
            raise ValueError("All tracks must cover the complete clip")
        if any(not 0 <= f <= end_frame or not 0 <= v <= 1 for f, v in schedule):
            raise ValueError("Invalid track sample")
        key = keys.key_blocks[key_name]
        for frame, value in schedule:
            key.value = value
            key.keyframe_insert(data_path="value", frame=frame, group=name)
    action = keys.animation_data.action
    action.name = name
    action.use_fake_user = True
    action["evosoul_owned"] = True
    action["fps"] = 30
    action["duration_seconds"] = end_frame / 30
    action["loop"] = name == "Walk"
    for curve in fcurves(action):
        for point in curve.keyframe_points:
            point.interpolation = "BEZIER"
            point.handle_left_type = "AUTO_CLAMPED"
            point.handle_right_type = "AUTO_CLAMPED"
        curve.extrapolation = "CONSTANT"
    scene = bpy.context.scene
    scene.render.fps = 30
    scene.render.fps_base = 1.0
    scene.frame_start = 0
    scene.frame_end = end_frame
    neutral(obj)
    print({"clip": name, "frames": [0, end_frame], "duration": end_frame / 30,
           "curves": len(list(fcurves(action))), "shape_keys": list(poses)})
    return action

def activate(name):
    obj = bpy.data.objects[OBJECT_NAME]
    neutral(obj)
    action = bpy.data.actions[name]
    keys = obj.data.shape_keys
    keys.animation_data.action = action
    keys.animation_data.action_slot = action.slots[0]
    bpy.context.scene.frame_start = 0
    bpy.context.scene.frame_end = CLIPS[name]
    bpy.context.scene.frame_set(0)
    bpy.context.view_layer.update()
    return action
