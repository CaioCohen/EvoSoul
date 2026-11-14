# Virgin Drop animation contract
Main agent owns live Blender/Unity, common.py, integration, backup, export and validation.
Each worker owns ONLY its assigned walk.py / attack.py / death.py. Do not call Blender or Unity MCP. Do not read other worker scripts. No inherited chat context.
Read AGENTS.md, docs/EvoSoul_Game_Design_Document.md, installed text-to-blender and lowpoly-style (export/unity skills if needed). Model-specific facts below are authoritative.

## Model and invariants
Blender 5.3.0; scene Virgin Drop; object GEO-VirginDrop; mesh VirginDrop-Smiling-Mesh.
242 vertices; 428 triangles; first 194 vertices watertight body, indices 194..241 surface eyes/smile geometry.
Body z=0..1; x/y extent +/-0.4. Front is Blender -Y (Unity +Z after FBX conversion).
Bottom-center origin (0,0,0); identity transforms. Two existing materials, grey body and charcoal facial marks.
No initial rig, modifiers, keys or actions. Do not replace mesh, move original vertices, change polygon/material data, add topology, rig, or object animation. Preserve Basis exactly. Shape-key deformation only.
Apply spatial deformation to face along with body, avoid intersections/detached features. Death may tuck face mark vertices inside final puddle, preserving their topology/material.
Ground z>=0. Walk/Attack in-place, no root translation. Death holds final puddle, not neutral.

## API and files
Standalone Python module with a build() function, plus if __name__ == "__main__": build().
Import common from directory of __file__ (insert that directory into sys.path if needed).
common.build_clip(name, end_frame, poses, tracks) -> action.
poses: dictionary of owned shape-key name -> callable(co, vertex_index), returning 3 coordinates. co is original Basis coordinate copy; NEVER mutate mesh.
tracks: dictionary of owned key name -> [(frame,value),...], all within 0..end_frame and values 0..1.
common creates Basis and all six named keys, fills ONLY passed poses, creates Blender 5.3 layered action via keyframe_insert, controls ALL six keys with zero tracks for unused keys, uses BEZIER/AUTO_CLAMPED interpolation, fake_user, sets 30 fps. Returns neutral after building.
Use only names:
Walk: Walk_Stretch, Walk_Gather; action Walk; frames 0..21 (0.7s).
Attack: Attack_Anticipate, Attack_Lunge; action Attack; frames 0..15 (0.5s).
Death: Death_Soften, Death_Puddle; action Death; frames 0..36 (1.2s).
No legacy action.fcurves. Helper walks action.layers[].strips[].channelbags[].fcurves.
Do not modify common.py. Pure math deformation code and keyframe schedules in your file; informative docstring and short build report.

## Motion
Walk: snail-like grounded extension forward (-Y), then rear gathering / recovery. Not bouncy or rigid scale-only. Identical neutral first/last, smooth seam (zero endpoint velocity), no root drift.
Attack: brief backward lean (+Y upper body), rapid forward head-butt deformation (-Y), recoil/recover neutral at frame15.
Death: progressive loss of support, intermediate slump, broad flattened puddle ~0.04m or less thick. Nonuniform deformation, preserve orientation/no inverted body geometry, final several frames static. No root scaling.
Prefer complementary stage weights when transitioning between intermediate and final poses so deformation does not double.

## Export contract (verified live)
Keep three separate Blender KEY actions on shape-key datablock, fake user.
Main bakes export-only action into ranges Walk0..21, Attack30..45, Death60..96 at30fps; original actions preserved.
Export single virgin_drop.fbx using global take, bake_anim_use_all_actions=False, bake_anim_use_nla_strips=False,
use_mesh_modifiers=False, use_triangles=False, bake_space_transform=True, bake_anim_step=1, simplify=0, -Z forward/Y up.
CRITICAL: Blender5.3 FBX use_triangles=True produces static zero blend-shape curves; experimentally verified. Let Unity triangulate instead.
Unity ModelImporter splits above ranges into three independently playable Generic clips with SkinnedMeshRenderer blendShape.* bindings.
Live probe verified two independent clips sample weights0->100->0 and identity root orientation. No armature needed.
No gameplay systems, Animator Controller, or Unity scene changes.
