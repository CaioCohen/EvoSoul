using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

const int size = 128;
var texture = new Texture2D(size, size, TextureFormat.RGBA32, false, false)
{
    name = "BercarioOceanPattern",
    wrapMode = TextureWrapMode.Repeat,
    filterMode = FilterMode.Bilinear,
};

for (var y = 0; y < size; y++)
{
    for (var x = 0; x < size; x++)
    {
        float waveA = Mathf.Sin((x * 0.20f) + (y * 0.08f));
        float waveB = Mathf.Sin((x * -0.07f) + (y * 0.18f));
        float variation = (waveA + waveB) * 0.035f;
        texture.SetPixel(x, y, new Color(0.34f + variation, 0.80f + variation, 1.00f, 1f));
    }
}
texture.Apply(false, false);

var path = "Assets/Art/Environment/Ocean/BercarioOceanPattern.png";
File.WriteAllBytes(path, texture.EncodeToPNG());
Object.DestroyImmediate(texture);
AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

var importer = AssetImporter.GetAtPath(path) as TextureImporter;
if (importer != null)
{
    importer.wrapMode = TextureWrapMode.Repeat;
    importer.filterMode = FilterMode.Bilinear;
    importer.mipmapEnabled = false;
    importer.sRGBTexture = true;
    importer.SaveAndReimport();
}

var material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Environment/Ocean/BercarioOceanWater.mat");
if (material == null)
    throw new System.InvalidOperationException("Ocean material was not found.");
material.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(path));
material.SetColor("_BaseColor", new Color(0.30f, 0.78f, 1.00f, 1f));
EditorUtility.SetDirty(material);
AssetDatabase.SaveAssets();
EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
EditorSceneManager.SaveScene(SceneManager.GetActiveScene());

return new { path, baseColor = material.GetColor("_BaseColor"), texture = material.GetTexture("_BaseMap")?.name };
