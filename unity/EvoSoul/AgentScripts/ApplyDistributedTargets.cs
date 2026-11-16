using System;
using System.Linq;
using System.Reflection;
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
    throw new InvalidOperationException($"Expected 10 fruit and 10 NPC targets; found {fruits.Length} fruit and {npcs.Length} NPCs.");

var fruitPositions = new[]
{
    new Vector2(-42f, -20f), new Vector2(-34f, 18f), new Vector2(-18f, -14f), new Vector2(-9f, 15f), new Vector2(2f, -7f),
    new Vector2(13f, 20f), new Vector2(22f, -18f), new Vector2(31f, 10f), new Vector2(40f, -9f), new Vector2(42f, 20f)
};
var npcPositions = new[]
{
    new Vector2(-42f, 7f), new Vector2(-34f, -8f), new Vector2(-24f, 6f), new Vector2(-7f, -6f), new Vector2(-1f, -21f),
    new Vector2(4f, 20f), new Vector2(18f, 2f), new Vector2(28f, 18f), new Vector2(38f, -19f), new Vector2(43f, 5f)
};

float GroundAt(Vector2 target)
{
    var hits = Physics.RaycastAll(new Ray(new Vector3(target.x, 100f, target.y), Vector3.down), 200f, ~0, QueryTriggerInteraction.Collide)
        .OrderBy(hit => hit.distance);
    var terrain = hits.FirstOrDefault(hit => hit.collider is MeshCollider && hit.collider.gameObject.name.IndexOf("Terrain", StringComparison.OrdinalIgnoreCase) >= 0);
    if (terrain.collider == null)
        throw new InvalidOperationException($"No terrain hit at {target}.");
    return terrain.point.y;
}

float LowestRendererY(Transform root)
{
    var renderers = root.GetComponentsInChildren<Renderer>(true).Where(renderer => renderer.enabled && renderer.gameObject.activeInHierarchy).ToArray();
    if (renderers.Length == 0)
        throw new InvalidOperationException($"No active renderers under {root.name}.");
    return renderers.Min(renderer => renderer.bounds.min.y);
}

var fruitVisualField = typeof(StageFruitTarget).GetField("fruitVisual", BindingFlags.Instance | BindingFlags.NonPublic);
var npcWanderCenterField = typeof(StageDropletNpcTarget).GetField("wanderCenter", BindingFlags.Instance | BindingFlags.NonPublic);
if (fruitVisualField == null || npcWanderCenterField == null)
    throw new InvalidOperationException("Expected serialized target fields are unavailable.");

for (int index = 0; index < fruits.Length; index++)
{
    var fruit = fruits[index];
    var desired = fruitPositions[index];
    var visual = fruitVisualField.GetValue(fruit) as GameObject;
    if (visual == null)
        throw new InvalidOperationException($"{fruit.name} has no linked fruitVisual.");
    float ground = GroundAt(desired);
    visual.transform.position = new Vector3(desired.x, 50f, desired.y);
    visual.transform.position += Vector3.up * (ground + 0.01f - LowestRendererY(visual.transform));
    var visualBounds = visual.GetComponentsInChildren<Renderer>(true).Where(renderer => renderer.enabled && renderer.gameObject.activeInHierarchy)
        .Aggregate(new Bounds(), (bounds, renderer) => bounds.size == Vector3.zero ? renderer.bounds : Encapsulate(bounds, renderer.bounds));
    fruit.transform.position = new Vector3(desired.x, visualBounds.center.y, desired.y);
}

for (int index = 0; index < npcs.Length; index++)
{
    var npc = npcs[index];
    var desired = npcPositions[index];
    float ground = GroundAt(desired);
    npc.transform.position = new Vector3(desired.x, 50f, desired.y);
    npc.transform.position += Vector3.up * (ground + 0.01f - LowestRendererY(npc.transform));
    npcWanderCenterField.SetValue(npc, null);
}

EditorSceneManager.MarkSceneDirty(scene);
EditorSceneManager.SaveScene(scene);

Bounds Encapsulate(Bounds first, Bounds next) { first.Encapsulate(next); return first; }

return new
{
    fruitCount = fruits.Length,
    npcCount = npcs.Length,
    fruits = fruits.Select(item => new { item.name, position = item.transform.position.ToString("F2") }).ToArray(),
    npcs = npcs.Select(item => new { item.name, position = item.transform.position.ToString("F2") }).ToArray()
};
