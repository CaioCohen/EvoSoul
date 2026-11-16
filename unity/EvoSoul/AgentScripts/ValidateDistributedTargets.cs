using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

var scene = SceneManager.GetActiveScene();
var fruits = Resources.FindObjectsOfTypeAll<StageFruitTarget>().Where(item => item.gameObject.scene == scene && item.gameObject.activeInHierarchy).OrderBy(item => item.name).ToArray();
var npcs = Resources.FindObjectsOfTypeAll<StageDropletNpcTarget>().Where(item => item.gameObject.scene == scene && item.gameObject.activeInHierarchy).OrderBy(item => item.name).ToArray();
if (fruits.Length != 10 || npcs.Length != 10)
    throw new InvalidOperationException($"Expected 10 active fruits/NPCs, found {fruits.Length}/{npcs.Length}.");

var fruitVisualField = typeof(StageFruitTarget).GetField("fruitVisual", BindingFlags.Instance | BindingFlags.NonPublic);
var npcVisualField = typeof(StageDropletNpcTarget).GetField("visualRoot", BindingFlags.Instance | BindingFlags.NonPublic);
float GroundAt(Vector3 point)
{
    var hit = Physics.RaycastAll(new Ray(new Vector3(point.x, 100f, point.z), Vector3.down), 200f, ~0, QueryTriggerInteraction.Collide)
        .OrderBy(value => value.distance)
        .FirstOrDefault(value => value.collider is MeshCollider && value.collider.gameObject.name.IndexOf("Terrain", StringComparison.OrdinalIgnoreCase) >= 0);
    if (hit.collider == null) throw new InvalidOperationException($"No terrain at {point}.");
    return hit.point.y;
}
float Lowest(Transform root) => root.GetComponentsInChildren<Renderer>(true).Where(item => item.enabled && item.gameObject.activeInHierarchy).Min(item => item.bounds.min.y);

var fruitDeltas = fruits.Select(fruit =>
{
    var visual = (GameObject)fruitVisualField.GetValue(fruit);
    return Math.Abs(Lowest(visual.transform) - GroundAt(fruit.transform.position));
}).ToArray();
var npcDeltas = npcs.Select(npc =>
{
    var visual = (Transform)npcVisualField.GetValue(npc);
    return Math.Abs(Lowest(visual) - GroundAt(npc.transform.position));
}).ToArray();

int Region(Vector3 p)
{
    int x = p.x < -16 ? 0 : p.x > 16 ? 2 : 1;
    int z = p.z < -12 ? 0 : p.z > 12 ? 2 : 1;
    return z * 3 + x;
}
var regions = fruits.Cast<Component>().Concat(npcs).GroupBy(item => Region(item.transform.position)).ToDictionary(group => group.Key, group => group.Count());
if (regions.Count != 9 || regions.Values.Any(value => value == 0))
    throw new InvalidOperationException("Targets do not cover all 9 island regions.");

var selector = Resources.FindObjectsOfTypeAll<StageInteractionSelector>().First(item => item.gameObject.scene == scene && item.gameObject.activeInHierarchy);
var player = selector.transform;
var remoteFruit = fruits.First(item => item.transform.position.x < -35f && item.transform.position.z < -12f);
player.position = remoteFruit.transform.position - new Vector3(0f, 0f, 1.2f);
player.forward = (remoteFruit.transform.position - player.position).normalized;
selector.RefreshSelection();
var fruitSelected = object.ReferenceEquals(selector.CurrentTarget, remoteFruit) && selector.CurrentAction == StageInteractionAction.Eat;
var remoteNpc = npcs.First(item => item.transform.position.x > 35f && item.transform.position.z < -12f);
player.position = remoteNpc.transform.position - new Vector3(0f, 0f, 1.2f);
player.forward = (remoteNpc.transform.position - player.position).normalized;
selector.RefreshSelection();
var npcSelected = object.ReferenceEquals(selector.CurrentTarget, remoteNpc) && selector.CurrentAction == StageInteractionAction.Attack;
if (!fruitSelected || !npcSelected) throw new InvalidOperationException($"Remote selection failed: fruit={fruitSelected}, npc={npcSelected}.");

return new { fruits = fruits.Length, npcs = npcs.Length, regions = regions.OrderBy(pair => pair.Key).ToDictionary(pair => pair.Key, pair => pair.Value), fruitDeltas, npcDeltas, maxGroundDelta = fruitDeltas.Concat(npcDeltas).Max(), fruitSelected, npcSelected };
