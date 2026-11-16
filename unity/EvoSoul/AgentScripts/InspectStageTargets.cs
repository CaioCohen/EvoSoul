using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

var scene = SceneManager.GetActiveScene();
var fruits = Resources.FindObjectsOfTypeAll<StageFruitTarget>()
    .Where(item => item.gameObject.scene == scene)
    .Select(item => new { item.name, position = item.transform.position.ToString("F2"), active = item.gameObject.activeInHierarchy })
    .ToArray();
var npcs = Resources.FindObjectsOfTypeAll<StageDropletNpcTarget>()
    .Where(item => item.gameObject.scene == scene)
    .Select(item => new { item.name, position = item.transform.position.ToString("F2"), active = item.gameObject.activeInHierarchy })
    .ToArray();
var grounds = Resources.FindObjectsOfTypeAll<MeshCollider>()
    .Where(item => item.gameObject.scene == scene && item.name.IndexOf("Terrain", StringComparison.OrdinalIgnoreCase) >= 0)
    .Select(item => new { item.name, position = item.transform.position.ToString("F2"), bounds = item.bounds.ToString() })
    .ToArray();
return new { scene = scene.path, fruitCount = fruits.Length, npcCount = npcs.Length, fruits, npcs, grounds };
