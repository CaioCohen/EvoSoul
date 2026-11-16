using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

var scene = SceneManager.GetActiveScene();
GameObject FindRoot(string name) => scene.GetRootGameObjects().FirstOrDefault(item => item.name == name);
GameObject FindAny(string name) => Resources.FindObjectsOfTypeAll<Transform>()
    .FirstOrDefault(item => item.gameObject.scene == scene && item.name == name)?.gameObject;

GameObject EnsureChild(Transform parent, string name)
{
    var existing = parent.Find(name);
    if (existing != null) return existing.gameObject;
    var created = new GameObject(name);
    Undo.RegisterCreatedObjectUndo(created, "Create stage hierarchy group");
    created.transform.SetParent(parent, false);
    return created;
}

void Reparent(Transform child, Transform parent)
{
    if (child != null && child.parent != parent)
        Undo.SetTransformParent(child, parent, "Organize Bercario stage hierarchy");
}

var world = FindRoot("BercarioWorld") ?? new GameObject("BercarioWorld");
var environment = EnsureChild(world.transform, "Environment");
var oceanGroup = EnsureChild(world.transform, "Ocean");
EnsureChild(world.transform, "GameplayMarkers");
var environmentVisual = FindAny("EnvironmentVisual");
if (environmentVisual != null) Reparent(environmentVisual.transform, environment.transform);
var gardenLight = FindRoot("GardenDirectionalLight");
if (gardenLight != null) Reparent(gardenLight.transform, environment.transform);
var ocean = FindRoot("BercarioOcean");
if (ocean != null) Reparent(ocean.transform, oceanGroup.transform);

var fruitRoot = FindRoot("StageFruitTargets") ?? FindRoot("StageFruits");
if (fruitRoot == null) fruitRoot = new GameObject("StageFruits");
fruitRoot.name = "StageFruits";
var fruitVisualField = typeof(StageFruitTarget).GetField("fruitVisual", BindingFlags.Instance | BindingFlags.NonPublic);
var additionalVisualsField = typeof(StageFruitTarget).GetField("additionalVisuals", BindingFlags.Instance | BindingFlags.NonPublic);
var fruits = Resources.FindObjectsOfTypeAll<StageFruitTarget>().Where(item => item.gameObject.scene == scene).OrderBy(item => item.name).ToArray();
foreach (var fruit in fruits)
{
    var suffix = fruit.name.Substring("FruitTarget_".Length);
    var fruitGroup = EnsureChild(fruitRoot.transform, "Fruit_" + suffix);
    fruitGroup.transform.SetPositionAndRotation(fruit.transform.position, fruit.transform.rotation);
    var visualGroup = EnsureChild(fruitGroup.transform, "Visual");
    var colliderGroup = EnsureChild(fruitGroup.transform, "Collider");
    Reparent(fruit.transform, colliderGroup.transform);
    var primary = fruitVisualField?.GetValue(fruit) as GameObject;
    // Meshes 01–05 are immutable children of the imported environment model.
    // Clone just those visual nodes, then disable the original model nodes so
    // each fruit still has exactly one visible fruit and one visible leaf.
    if (primary != null && primary.transform.parent != visualGroup.transform)
    {
        var source = primary;
        primary = UnityEngine.Object.Instantiate(source);
        primary.name = source.name;
        primary.transform.SetParent(visualGroup.transform, true);
        source.SetActive(false);
    }
    else if (primary != null) primary.transform.SetParent(visualGroup.transform, true);
    // Early expanded fruits shared Fruit_01's leaf reference. Select the leaf
    // that belongs to each visual, then keep the target's hide list correct.
    var leafName = int.Parse(suffix) <= 5
        ? $"GEO-Fruit_{suffix}_Leaf"
        : $"Expanded_GEO-Fruit_01_Leaf_{suffix}";
    var existingExtras = additionalVisualsField?.GetValue(fruit) as GameObject[];
    var leaf = existingExtras?.FirstOrDefault(item => item != null) ?? FindAny(leafName);
    if (leaf != null && leaf.transform.parent != visualGroup.transform)
    {
        var source = leaf;
        leaf = UnityEngine.Object.Instantiate(source);
        leaf.name = source.name;
        leaf.transform.SetParent(visualGroup.transform, true);
        source.SetActive(false);
    }
    else if (leaf != null) leaf.transform.SetParent(visualGroup.transform, true);
    fruit.ConfigureVisuals(primary, leaf != null ? new[] { leaf } : Array.Empty<GameObject>());
}

var gameplay = FindRoot("StageGameplay") ?? new GameObject("StageGameplay");
foreach (var rootName in new[] { "VirginDropPlayer", "ThirdPersonCamera", "EventSystem", "StageDebugCanvas" })
{
    var root = FindRoot(rootName);
    if (root != null) Reparent(root.transform, gameplay.transform);
}

EditorSceneManager.MarkSceneDirty(scene);
EditorSceneManager.SaveScene(scene);
return new { roots = scene.GetRootGameObjects().Select(item => item.name).OrderBy(item => item).ToArray(), fruits = fruits.Length, npcs = Resources.FindObjectsOfTypeAll<StageDropletNpcTarget>().Count(item => item.gameObject.scene == scene), fruitRoot = fruitRoot.name, oceanParent = ocean != null ? ocean.transform.parent.name : "missing" };
