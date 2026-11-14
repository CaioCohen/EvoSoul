"""Non-destructive geometry and animation checks for Virgin Drop."""
import bpy
import hashlib
import json
import math
from pathlib import Path
import sys
sys.path.insert(0, str(Path(__file__).resolve().parent))
from common import OBJECT_NAME, CLIPS, activate, neutral

def fingerprint():
    obj = bpy.data.objects[OBJECT_NAME]
    mesh = obj.data
    data = {
        "vertices": [list(v.co) for v in mesh.vertices],
        "polygons": [list(p.vertices) for p in mesh.polygons],
        "material_indices": [p.material_index for p in mesh.polygons],
        "smooth": [p.use_smooth for p in mesh.polygons],
        "materials": [{"name": m.name, "diffuse": list(m.diffuse_color),
                       "base": list(next(n for n in m.node_tree.nodes if n.type == "BSDF_PRINCIPLED").inputs["Base Color"].default_value),
                       "roughness": next(n for n in m.node_tree.nodes if n.type == "BSDF_PRINCIPLED").inputs["Roughness"].default_value}
                      for m in mesh.materials],
        "matrix": [list(row) for row in obj.matrix_world],
    }
    return hashlib.sha256(json.dumps(data, sort_keys=True).encode()).hexdigest()

def coords(frame):
    scene = bpy.context.scene
    scene.frame_set(math.floor(frame), subframe=frame % 1)
    obj = bpy.data.objects[OBJECT_NAME]
    evaluated = obj.evaluated_get(bpy.context.evaluated_depsgraph_get())
    mesh = evaluated.to_mesh()
    result = [v.co.copy() for v in mesh.vertices]
    evaluated.to_mesh_clear()
    return result

def max_delta(a, b):
    return max((x-y).length for x,y in zip(a,b))

def validate():
    obj = bpy.data.objects[OBJECT_NAME]
    baseline = [v.co.copy() for v in obj.data.vertices]
    assert len(baseline) == 242
    assert sum(len(p.vertices)-2 for p in obj.data.polygons) == 428
    assert max_delta(baseline, [k.co.copy() for k in obj.data.shape_keys.key_blocks["Basis"].data]) == 0
    report = {"fingerprint": fingerprint(), "vertices": len(baseline), "triangles": 428, "clips": {}}
    for name, end in CLIPS.items():
        activate(name)
        first = coords(0)
        last = coords(end)
        minimum = min(v.z for step in range(end*4+1) for v in coords(step/4))
        entry = {"duration": end/30, "first_neutral_error": max_delta(first, baseline),
                 "last_neutral_error": max_delta(last, baseline), "minimum_z": minimum}
        assert minimum >= -1e-6
        assert entry["first_neutral_error"] < 1e-6
        if name in ("Walk", "Attack"):
            assert entry["last_neutral_error"] < 1e-6
        if name == "Walk":
            entry["loop_position_error"] = max_delta(first,last)
            # Endpoint speeds converge to zero for a smooth rest-to-rest seam.
            entry["first_0_001_frame_motion"] = max_delta(coords(.001),first)
            entry["last_0_001_frame_motion"] = max_delta(coords(end-.001),last)
        if name == "Death":
            entry["puddle_height"] = max(v.z for v in last)
            entry["puddle_width"] = max(v.x for v in last)-min(v.x for v in last)
            entry["last_three_frame_delta"] = max_delta(coords(end-3),last)
            assert entry["puddle_height"] <= .06
            assert entry["last_three_frame_delta"] < 1e-6
        report["clips"][name] = entry
    neutral(obj)
    assert max_delta(coords(0), baseline) < 1e-6
    print(json.dumps(report))
    return report

if __name__ == "__main__":
    validate()
