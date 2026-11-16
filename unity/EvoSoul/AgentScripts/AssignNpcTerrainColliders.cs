using System;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

var scene = SceneManager.GetActiveScene();
var npcs = Resources.FindObjectsOfTypeAll<StageDropletNpcTarget>()
    .Where(item => item.gameObject.scene == scene && item.gameObject.activeSelf)
    .OrderBy(item => item.name)
    .ToArray();
MeshCollider TerrainAt(Vector3 point)
{
    var hit = Physics.RaycastAll(new Ray(new Vector3(point.x, 100f, point.z), Vector3.down), 200f, ~0, QueryTriggerInteraction.Collide)
        .OrderBy(value => value.distance)
        .FirstOrDefault(value => value.collider is MeshCollider && value.collider.gameObject.name.IndexOf("Terrain", StringComparison.OrdinalIgnoreCase) >= 0);
    if (hit.collider is not MeshCollider terrain) throw new InvalidOperationException($"No terrain beneath {point}.");
    return terrain;
}
foreach (var npc in npcs)
    npc.ConfigureGroundCollider(TerrainAt(npc.transform.position));
EditorSceneManager.MarkSceneDirty(scene);
EditorSceneManager.SaveScene(scene);
return new { count = npcs.Length, assigned = npcs.Select(npc => new { npc.name, position = npc.transform.position.ToString("F2") }).ToArray() };
