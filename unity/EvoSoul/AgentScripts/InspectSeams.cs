using System.Linq;
using UnityEngine;

var fruits = Object.FindObjectsByType<StageFruitTarget>(FindObjectsInactive.Include, FindObjectsSortMode.None)
    .Select(target => $"FRUIT {target.name} active={target.gameObject.activeInHierarchy} pos={target.transform.position} layer={target.gameObject.layer} colliders={target.GetComponentsInChildren<Collider>(true).Length}");
var npcs = Object.FindObjectsByType<StageDropletNpcTarget>(FindObjectsInactive.Include, FindObjectsSortMode.None)
    .Select(target => $"NPC {target.name} active={target.gameObject.activeInHierarchy} pos={target.transform.position} layer={target.gameObject.layer} colliders={target.GetComponentsInChildren<Collider>(true).Length}");
return string.Join("\n", fruits.Concat(npcs));
