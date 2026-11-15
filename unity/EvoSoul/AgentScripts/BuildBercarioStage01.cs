using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class BuildBercarioStage01
{
    public static object Run()
    {
        const string EnvironmentPath = "Assets/Art/Models/Environment/BercarioEnvironment.fbx";
        const string PlayerPath = "Assets/Art/Models/virgin_drop.fbx";
        const string ScenePath = "Assets/Scenes/BercarioStage01.unity";

        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != ScenePath)
            throw new InvalidOperationException($"Expected active scene '{ScenePath}', got '{scene.path}'.");

        if (GameObject.Find("BercarioWorld") != null)
            throw new InvalidOperationException("BercarioStage01 already contains BercarioWorld; refusing to duplicate its setup.");

        GameObject environmentAsset = AssetDatabase.LoadAssetAtPath<GameObject>(EnvironmentPath);
        GameObject playerAsset = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPath);
        if (environmentAsset == null || playerAsset == null)
            throw new InvalidOperationException("Environment or player FBX asset could not be loaded.");

        var world = new GameObject("BercarioWorld");
        GameObject environment = (GameObject)PrefabUtility.InstantiatePrefab(environmentAsset, scene);
        environment.name = "EnvironmentVisual";
        environment.transform.SetParent(world.transform, false);

        var terrain = FindChild(environment.transform, "GEO-ENV-Terrain");
        if (terrain == null)
            throw new InvalidOperationException("Imported environment has no GEO-ENV-Terrain mesh.");
        terrain.gameObject.AddComponent<MeshCollider>();

        CreateBoundary(world.transform, "NorthBoundary", new Vector3(0f, 0.8f, 12.6f), new Vector3(32f, 1.6f, 0.5f));
        CreateBoundary(world.transform, "SouthBoundary", new Vector3(0f, 0.8f, -12.6f), new Vector3(32f, 1.6f, 0.5f));
        CreateBoundary(world.transform, "EastBoundary", new Vector3(16.6f, 0.8f, 0f), new Vector3(0.5f, 1.6f, 24f));
        CreateBoundary(world.transform, "WestBoundary", new Vector3(-16.6f, 0.8f, 0f), new Vector3(0.5f, 1.6f, 24f));

        var gameplay = new GameObject("GameplayMarkers");
        gameplay.transform.SetParent(world.transform, false);
        CreateMarker(gameplay.transform, "FruitSpawn_01", new Vector3(-5f, 0.25f, 2f));
        CreateMarker(gameplay.transform, "FruitSpawn_02", new Vector3(0f, 0.25f, 3f));
        CreateMarker(gameplay.transform, "FruitSpawn_03", new Vector3(5f, 0.25f, 1f));
        CreateMarker(gameplay.transform, "FruitSpawn_04", new Vector3(-3f, 0.25f, -3f));
        CreateMarker(gameplay.transform, "FruitSpawn_05", new Vector3(3f, 0.25f, -3f));
        CreateMarker(gameplay.transform, "NpcSpawn_01", new Vector3(-2f, 0.25f, 0f));
        CreateMarker(gameplay.transform, "NpcSpawn_02", new Vector3(2f, 0.25f, 0f));
        CreateMarker(gameplay.transform, "NpcSpawn_03", new Vector3(-6f, 0.25f, -1f));
        CreateMarker(gameplay.transform, "NpcSpawn_04", new Vector3(6f, 0.25f, -1f));

        GameObject player = (GameObject)PrefabUtility.InstantiatePrefab(playerAsset, scene);
        player.name = "VirginDropPlayer";
        player.transform.position = new Vector3(0f, 0.2f, -6f);
        var controller = player.AddComponent<VirginDropPlayerController>();

        AnimationClip[] clips = AssetDatabase.LoadAllAssetsAtPath(PlayerPath).OfType<AnimationClip>().ToArray();
        AnimationClip walk = clips.FirstOrDefault(clip => string.Equals(clip.name, "Walk", StringComparison.OrdinalIgnoreCase));
        AnimationClip attack = clips.FirstOrDefault(clip => string.Equals(clip.name, "Attack", StringComparison.OrdinalIgnoreCase));
        controller.Configure(player.transform, player.GetComponentInChildren<Animator>(), walk, attack);

        var cameraObject = new GameObject("ThirdPersonCamera");
        var camera = cameraObject.AddComponent<Camera>();
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = player.transform.position + new Vector3(0f, 3.2f, -6f);
        var follow = cameraObject.AddComponent<ThirdPersonFollowCamera>();
        follow.Configure(player.transform);

        var lightObject = new GameObject("GardenDirectionalLight");
        var directionalLight = lightObject.AddComponent<Light>();
        directionalLight.type = LightType.Directional;
        directionalLight.intensity = 1.35f;
        directionalLight.color = new Color(1f, 0.95f, 0.82f);
        lightObject.transform.rotation = Quaternion.Euler(45f, -30f, 0f);
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.42f, 0.55f, 0.68f);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        return new {
            scene = ScenePath,
            roots = scene.GetRootGameObjects().Select(root => root.name).ToArray(),
            environmentRenderers = environment.GetComponentsInChildren<Renderer>(true).Length,
            player = player.name,
            camera = cameraObject.name,
            markers = gameplay.transform.childCount
        };
    }

    private static Transform FindChild(Transform root, string name)
    {
        return root.GetComponentsInChildren<Transform>(true).FirstOrDefault(item => item.name == name);
    }

    private static void CreateBoundary(Transform parent, string name, Vector3 position, Vector3 size)
    {
        var boundary = new GameObject(name);
        boundary.transform.SetParent(parent, false);
        boundary.transform.position = position;
        boundary.AddComponent<BoxCollider>().size = size;
    }

    private static void CreateMarker(Transform parent, string name, Vector3 position)
    {
        var marker = new GameObject(name);
        marker.transform.SetParent(parent, false);
        marker.transform.position = position;
    }
}
