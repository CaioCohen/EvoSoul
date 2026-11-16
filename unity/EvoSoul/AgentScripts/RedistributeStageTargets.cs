using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

var scene = SceneManager.GetActiveScene();
var fruits = Resources.FindObjectsOfTypeAll<StageFruitTarget>()
    .Where(item => item.gameObject.scene == scene && item.gameObject.activeSelf)
    .OrderBy(item => item.name)
    .ToArray();
var npcs = Resources.FindObjectsOfTypeAll<StageDropletNpcTarget>()
    .Where(item => item.gameObject.scene == scene && item.gameObject.activeSelf)
    .OrderBy(item => item.name)
    .ToArray();

if (fruits.Length != 10 || npcs.Length != 10)
    throw new InvalidOperationException($"Expected exactly 10 fruits and 10 NPCs, found {fruits.Length} and {npcs.Length}.");

var fruitPositions = new[]
{
    new Vector2(-40f, -18f), new Vector2(-29f, 16f), new Vector2(-12f, -17f), new Vector2(-5f, 14f), new Vector2(7f, -6f),
    new Vector2(15f, 19f), new Vector2(24f, -18f), new Vector2(31f, 8f), new Vector2(42f, -8f), new Vector2(40f, 18f)
};
var npcPositions = new[]
{
    new Vector2(-42f, 7f), new Vector2(-34f, -7f), new Vector2(-20f, 3f), new Vector2(-8f, -5f), new Vector2(0f, -20f),
    new Vector2(4f, 20f), new Vector2(18f, 2f), new Vector2(28f, 17f), new Vector2(38f, -18f), new Vector2(43f, 7f)
};

float SnapToTerrain(Transform root, Vector2 target)
{
    root.position = new Vector3(target.x, 50f, target.y);
    var hits = Physics.RaycastAll(new Ray(new Vector3(target.x, 100f, target.y), Vector3.down), 200f, ~0, QueryTriggerInteraction.Collide)
        .OrderBy(hit => hit.distance);
    var terrainHit = hits.FirstOrDefault(hit => hit.collider is MeshCollider && hit.collider.gameObject.name.IndexOf("Terrain", StringComparison.OrdinalIgnoreCase) >= 0);
    if (terrainHit.collider == null)
        throw new InvalidOperationException($"No terrain beneath {root.name} at {target}.");

    var renderers = root.GetComponentsInChildren<Renderer>(true).Where(renderer => renderer.gameObject.activeInHierarchy).ToArray();
    if (renderers.Length == 0)
        throw new InvalidOperationException($"No active renderer beneath {root.name}.");
    float lowest = renderers.Min(renderer => renderer.bounds.min.y);
    root.position += Vector3.up * (terrainHit.point.y - lowest + 0.01f);
    return terrainHit.point.y;
}

var fruitGroundHeights = fruits.Select((fruit, index) => SnapToTerrain(fruit.transform, fruitPositions[index])).ToArray();
var npcGroundHeights = npcs.Select((npc, index) => SnapToTerrain(npc.transform, npcPositions[index])).ToArray();

EditorSceneManager.MarkSceneDirty(scene);
EditorSceneManager.SaveScene(scene);

return new
{
    fruitCount = fruits.Length,
    npcCount = npcs.Length,
    fruitPositions = fruits.Select(fruit => new { fruit.name, position = fruit.transform.position.ToString("F2") }).ToArray(),
    npcPositions = npcs.Select(npc => new { npc.name, position = npc.transform.position.ToString("F2") }).ToArray(),
    fruitGroundHeights,
    npcGroundHeights
};
