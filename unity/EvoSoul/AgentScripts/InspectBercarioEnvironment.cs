using UnityEditor;
using UnityEngine;

var root = AssetDatabase.LoadAssetAtPath<GameObject>(
    "Assets/Art/Models/Environment/BercarioEnvironment.fbx");

if (root == null)
    throw new System.InvalidOperationException("Environment FBX root was not imported.");

var filters = root.GetComponentsInChildren<MeshFilter>(true);
var renderers = root.GetComponentsInChildren<Renderer>(true);
var materialCount = 0;
foreach (var renderer in renderers)
    materialCount += renderer.sharedMaterials.Length;

return new
{
    root = root.name,
    meshFilters = filters.Length,
    renderers = renderers.Length,
    materialSlots = materialCount,
    rootScale = root.transform.localScale
};
