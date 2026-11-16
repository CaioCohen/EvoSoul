using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

var scene = SceneManager.GetActiveScene();
var fruit = Resources.FindObjectsOfTypeAll<StageFruitTarget>().First(item => item.name == "FruitTarget_01" && item.gameObject.scene == scene);
var field = typeof(StageFruitTarget).GetField("fruitVisual", BindingFlags.Instance | BindingFlags.NonPublic);
var linkedVisual = field?.GetValue(fruit) as GameObject;
var renderers = fruit.GetComponentsInChildren<Renderer>(true)
    .Select(renderer => new { name = renderer.name, active = renderer.gameObject.activeInHierarchy, enabled = renderer.enabled, bounds = renderer.bounds.ToString() })
    .ToArray();
return new { fruit = fruit.name, active = fruit.gameObject.activeInHierarchy, position = fruit.transform.position.ToString("F2"), linkedVisual = linkedVisual != null ? new { linkedVisual.name, active = linkedVisual.activeInHierarchy, position = linkedVisual.transform.position.ToString("F2") } : null, renderers };
