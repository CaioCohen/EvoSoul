using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public static class BuildBercarioOcean
{
    private const string ScenePath = "Assets/Scenes/BercarioStage01.unity";
    private const string AssetFolder = "Assets/Art/Environment/Ocean";
    private const string GraphPath = AssetFolder + "/BercarioOceanWater.shadergraph";
    private const string MaterialPath = AssetFolder + "/BercarioOceanWater.mat";
    private const string TexturePath = AssetFolder + "/BercarioOceanPattern.png";

    public static object Main()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        EnsureFolder("Assets/Art");
        EnsureFolder("Assets/Art/Environment");
        EnsureFolder(AssetFolder);

        Texture2D waterPattern = EnsureWaterPattern();
        Shader shader = EnsureShaderGraph();
        Material waterMaterial = EnsureMaterial(shader, waterPattern);

        GameObject terrain = GameObject.Find("GEO-ENV-Terrain");
        if (terrain == null)
            throw new InvalidOperationException("GEO-ENV-Terrain was not found in BercarioStage01.");

        Renderer terrainRenderer = terrain.GetComponent<Renderer>();
        if (terrainRenderer == null)
            throw new InvalidOperationException("The terrain has no Renderer.");

        float waterLevel = terrainRenderer.bounds.min.y - 0.14f;
        GameObject oceanRoot = GameObject.Find("BercarioOcean") ?? new GameObject("BercarioOcean");
        GameObject ocean = GameObject.Find("OceanSurface") ?? GameObject.CreatePrimitive(PrimitiveType.Plane);
        ocean.name = "OceanSurface";
        ocean.transform.SetParent(oceanRoot.transform, false);
        ocean.transform.position = new Vector3(0f, waterLevel, 0f);
        ocean.transform.rotation = Quaternion.identity;
        ocean.transform.localScale = new Vector3(140f, 1f, 140f); // 1400 x 1400m plane reaches past the horizon.

        Collider oceanCollider = ocean.GetComponent<Collider>();
        if (oceanCollider != null)
            UnityEngine.Object.DestroyImmediate(oceanCollider);

        Renderer oceanRenderer = ocean.GetComponent<Renderer>();
        oceanRenderer.sharedMaterial = waterMaterial;
        oceanRenderer.shadowCastingMode = ShadowCastingMode.Off;
        oceanRenderer.receiveShadows = false;

        if (ocean.GetComponent<OceanSurfaceMotion>() == null)
            ocean.AddComponent<OceanSurfaceMotion>();

        SetupHorizon();
        VerifyBoundaries();
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        return new
        {
            scene = ScenePath,
            shaderGraph = GraphPath,
            material = MaterialPath,
            pattern = TexturePath,
            waterLevel,
            oceanSize = "1400x1400m",
            boundariesPresent = true
        };
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
            return;
        string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
        string leaf = Path.GetFileName(path);
        if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(leaf))
            throw new InvalidOperationException("Invalid asset folder path: " + path);
        AssetDatabase.CreateFolder(parent, leaf);
    }

    private static Texture2D EnsureWaterPattern()
    {
        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);
        if (texture != null)
            return texture;

        const int size = 128;
        Texture2D generated = new Texture2D(size, size, TextureFormat.RGBA32, false, false)
        {
            name = "BercarioOceanPattern",
            wrapMode = TextureWrapMode.Repeat,
            filterMode = FilterMode.Bilinear
        };

        Color deep = new Color(0.025f, 0.24f, 0.39f, 1f);
        Color shallow = new Color(0.05f, 0.45f, 0.62f, 1f);
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float u = (float)x / size;
            float v = (float)y / size;
            float waves = Mathf.Sin((u + v) * Mathf.PI * 12f) * 0.10f +
                          Mathf.Sin((u * 2.7f - v * 1.8f) * Mathf.PI * 9f) * 0.08f;
            float noise = Mathf.PerlinNoise(u * 6f, v * 6f) * 0.18f;
            float blend = Mathf.Clamp01(0.36f + waves + noise);
            generated.SetPixel(x, y, Color.Lerp(deep, shallow, blend));
        }
        generated.Apply(false, false);
        File.WriteAllBytes(Path.Combine(Application.dataPath, "Art/Environment/Ocean/BercarioOceanPattern.png"), generated.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(generated);
        AssetDatabase.ImportAsset(TexturePath, ImportAssetOptions.ForceSynchronousImport);
        texture = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);
        TextureImporter importer = AssetImporter.GetAtPath(TexturePath) as TextureImporter;
        if (importer != null)
        {
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Compressed;
            importer.maxTextureSize = 128;
            importer.SaveAndReimport();
        }
        return texture;
    }

    private static Shader EnsureShaderGraph()
    {
        Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(GraphPath);
        if (shader != null)
            return shader;

        const string template = "Packages/com.unity.shadergraph/GraphTemplates/Cross Pipeline/Unlit Simple.shadergraph";
        if (!AssetDatabase.CopyAsset(template, GraphPath))
            throw new InvalidOperationException("Could not create Shader Graph from the URP Unlit Simple template.");
        AssetDatabase.ImportAsset(GraphPath, ImportAssetOptions.ForceSynchronousImport);
        shader = AssetDatabase.LoadAssetAtPath<Shader>(GraphPath);
        if (shader == null)
            throw new InvalidOperationException("Shader Graph import did not produce a Shader.");
        return shader;
    }

    private static Material EnsureMaterial(Shader shader, Texture2D texture)
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        if (material == null)
        {
            material = new Material(shader) { name = "BercarioOceanWater" };
            AssetDatabase.CreateAsset(material, MaterialPath);
        }
        material.shader = shader;
        if (material.HasProperty("_BaseMap"))
        {
            material.SetTexture("_BaseMap", texture);
            material.SetTextureScale("_BaseMap", new Vector2(48f, 48f));
        }
        if (material.HasProperty("_BaseColor"))
            material.SetColor("_BaseColor", new Color(0.52f, 0.82f, 0.95f, 1f));
        EditorUtility.SetDirty(material);
        return material;
    }

    private static void SetupHorizon()
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = new Color(0.15f, 0.45f, 0.62f, 1f);
        RenderSettings.fogStartDistance = 170f;
        RenderSettings.fogEndDistance = 520f;
        RenderSettings.ambientSkyColor = new Color(0.21f, 0.49f, 0.70f, 1f);
        Camera[] cameras = UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
        foreach (Camera camera in cameras)
        {
            camera.farClipPlane = Mathf.Max(camera.farClipPlane, 700f);
            camera.clearFlags = CameraClearFlags.Skybox;
        }
    }

    private static void VerifyBoundaries()
    {
        string[] names = { "NorthBoundary", "SouthBoundary", "EastBoundary", "WestBoundary" };
        foreach (string name in names)
        {
            GameObject boundary = GameObject.Find(name);
            if (boundary == null || boundary.GetComponent<BoxCollider>() == null)
                throw new InvalidOperationException("Required invisible boundary is missing: " + name);
        }
    }
}
