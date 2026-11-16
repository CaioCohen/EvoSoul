---
name: unity-architecture-review
description: Review and safely organize EvoSoul Unity scene hierarchies, especially Bercario stage gameplay targets, without breaking serialized references or runtime selection.
---

# Unity Architecture Review

Use this skill when reviewing or reorganizing Unity scenes in this project.

## Bercario stage layout

Keep `BercarioStage01` readable at the root level:

```text
BercarioWorld
  Environment
  Ocean
  GameplayMarkers
StageFruits
  Fruit_XX
    Visual
    Collider
NurseryDroplets
  DropletNpc_XX
StageGameplay
```

Each `Fruit_XX` owns its visual meshes (including the leaf) and the object that
carries `StageFruitTarget` plus its collider. Each NPC remains a single
`DropletNpc_XX` target with its visual and puddle children.

## Safe review workflow

1. Use the live Unity Editor/Pipeline to inspect the active scene before changing it; do not hand-edit `.unity` YAML.
2. Preserve world transforms when changing parents. Do not duplicate target components or recreate visual objects if the existing serialized object can be moved.
3. Preserve and inspect serialized references: `StageFruitTarget` visual lists, `StageDropletNpcTarget` visual/ground references, player selector references, camera links, and UnityEvents.
4. Keep target discovery component-based (`StageFruitTarget` and `StageDropletNpcTarget`), not dependent on hierarchy paths or display names.
5. Do not modify scenes while Unity is in Play Mode. Save only in Edit Mode, then verify the saved scene.

## Acceptance checks

- Exactly ten active `StageFruitTarget` and ten `StageDropletNpcTarget` objects remain in `BercarioStage01`.
- Every fruit target's primary and additional visuals are children of its matching `Fruit_XX/Visual` branch.
- Reparenting does not alter world positions, collider dimensions, or script/component assignments.
- Enter Play Mode and confirm there are no missing-reference or compile errors; verify a fruit can still be selected and consumed and an NPC can still be selected.

