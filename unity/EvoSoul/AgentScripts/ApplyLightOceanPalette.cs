using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

var scene = SceneManager.GetActiveScene();
var material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Environment/Ocean/BercarioOceanWater.mat");
if (material == null)
    throw new System.InvalidOperationException("Ocean material was not found.");

// Light, saturated ocean-blue; the repeating pattern supplies the subtle wave variation.
var waterColor = new Color(0.12f, 0.66f, 0.94f, 1f);
material.SetColor("_BaseColor", waterColor);
EditorUtility.SetDirty(material);

// Keep the horizon continuous with the water while retaining enough contrast for the island.
RenderSettings.fog = true;
RenderSettings.fogMode = FogMode.Linear;
RenderSettings.fogColor = new Color(0.45f, 0.73f, 0.90f, 1f);
RenderSettings.fogStartDistance = 170f;
RenderSettings.fogEndDistance = 520f;
RenderSettings.ambientSkyColor = new Color(0.42f, 0.70f, 0.90f, 1f);
RenderSettings.ambientEquatorColor = new Color(0.56f, 0.78f, 0.92f, 1f);
RenderSettings.ambientGroundColor = new Color(0.28f, 0.43f, 0.40f, 1f);

var ocean = GameObject.Find("BercarioOcean/OceanSurface");
if (ocean == null)
    ocean = GameObject.Find("OceanSurface");
if (ocean == null)
    throw new System.InvalidOperationException("OceanSurface was not found in the active scene.");

var renderer = ocean.GetComponent<Renderer>();
if (renderer == null)
    throw new System.InvalidOperationException("OceanSurface has no Renderer.");
renderer.sharedMaterial = material;
EditorUtility.SetDirty(ocean);
AssetDatabase.SaveAssets();
EditorSceneManager.MarkSceneDirty(scene);
EditorSceneManager.SaveScene(scene);

return new
{
    material = AssetDatabase.GetAssetPath(material),
    shaderGraph = AssetDatabase.GetAssetPath(AssetDatabase.LoadAssetAtPath<Shader>("Assets/Art/Environment/Ocean/BercarioOceanWater.shadergraph")),
    baseColor = material.GetColor("_BaseColor"),
    fog = RenderSettings.fogColor,
    fogStart = RenderSettings.fogStartDistance,
    fogEnd = RenderSettings.fogEndDistance,
    scene = scene.path,
};
