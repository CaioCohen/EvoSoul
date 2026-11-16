using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

var scene = SceneManager.GetActiveScene();
var npc = Resources.FindObjectsOfTypeAll<StageDropletNpcTarget>().First(item => item.name == "DropletNpc_01" && item.gameObject.scene == scene);
var field = typeof(StageDropletNpcTarget).GetField("visualRoot", BindingFlags.Instance | BindingFlags.NonPublic);
var visual = field?.GetValue(npc) as Transform;
var renderers = (visual != null ? visual.GetComponentsInChildren<Renderer>(true) : npc.GetComponentsInChildren<Renderer>(true))
    .Select(renderer => new { name = renderer.name, active = renderer.gameObject.activeInHierarchy, enabled = renderer.enabled, bounds = renderer.bounds.ToString() })
    .ToArray();
return new { npc = npc.name, npcPosition = npc.transform.position.ToString("F2"), visual = visual != null ? new { name = visual.name, position = visual.position.ToString("F2"), parent = visual.parent != null ? visual.parent.name : null } : null, renderers };
