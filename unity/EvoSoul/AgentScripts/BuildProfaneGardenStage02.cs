using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class BuildProfaneGardenStage02
{
    const string Directory = "Assets/Art/Environment/ProfaneGarden/";
    const string ScenePath = "Assets/Scenes/ProfaneGardenStage02.unity";

    static GameObject Asset(string name)
    {
        var asset = AssetDatabase.LoadAssetAtPath<GameObject>(Directory + name + ".fbx");
        if (asset == null) throw new Exception("Missing garden asset: " + name);
        return asset;
    }
    static GameObject Place(GameObject prefab, Transform parent, string name, Vector3 position, float yaw = 0f)
    {
        var item = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        item.name = name;
        item.transform.SetParent(parent, false);
        item.transform.SetPositionAndRotation(position, Quaternion.Euler(0, yaw, 0) * prefab.transform.rotation);
        return item;
    }
    static AnimationClip Clip(string name)
    {
        return AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Characters/profane_drop_animated.fbx")
            .OfType<AnimationClip>().First(c => !c.name.StartsWith("__preview__") && c.name.EndsWith(name));
    }

    public static string Main()
    {
        var original = SceneManager.GetActiveScene();
        if (original.path != "Assets/Scenes/BercarioStage01.unity" || original.isDirty)
            throw new Exception("Expected clean Stage01 scene; no scene was changed.");
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null)
            throw new Exception("Stage02 already exists; refusing to overwrite.");
        var roots = original.GetRootGameObjects();
        var originalGameplay = roots.First(o => o.name == "StageGameplay");
        var originalNpc = roots.First(o => o.name == "NurseryDroplets").transform.GetChild(0).gameObject;
        var terrainAsset = Asset("pg_terrain");
        var corrupted = Asset("pg_plant_corrupt");
        var purified = Asset("pg_plant_purified");
        var profane = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Characters/profane_drop_animated.fbx");
        if (profane == null) throw new Exception("Missing animated Profane Drop.");
        var walk = Clip("Profane_Walk"); var attack = Clip("Profane_Attack"); var death = Clip("Profane_Death");

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        SceneManager.SetActiveScene(scene);
        var world = new GameObject("ProfaneGardenWorld");
        var terrain = Place(terrainAsset, world.transform, "GardenTerrain", Vector3.zero);
        terrain.transform.localScale *= 3.5f;
        var ground = terrain.GetComponentsInChildren<MeshFilter>().First().gameObject.AddComponent<MeshCollider>();
        ground.sharedMesh = ground.GetComponent<MeshFilter>().sharedMesh;
        var boundaries = new GameObject("IslandBoundaries").transform;
        boundaries.SetParent(world.transform, false);
        for (int i = 0; i < 24; i++)
        {
            float angle = i * 15f, radians = angle * Mathf.Deg2Rad;
            var boundary = new GameObject("Boundary_" + i);
            boundary.transform.SetParent(boundaries, false);
            boundary.transform.SetPositionAndRotation(
                new Vector3(Mathf.Sin(radians) * 9.15f * 3.5f, 1.5f, Mathf.Cos(radians) * 9.15f * 3.5f),
                Quaternion.Euler(0, angle, 0));
            boundary.AddComponent<BoxCollider>().size = new Vector3(2.5f * 3.5f, 3f, .3f);
        }
        var scenery = new GameObject("GardenDecoration").transform; scenery.SetParent(world.transform, false);
        var props = new[] {"pg_tree", "pg_bush", "pg_rock", "pg_thorns", "pg_mushrooms", "pg_growth", "pg_soilpatch"};
        for (int i = 0; i < 35; i++)
        {
            float angle = i * 137.50776f * Mathf.Deg2Rad;
            float radius = 7.4f + 1.7f * ((i % 5) / 4f);
            var point = new Vector3(Mathf.Cos(angle) * radius * 3.5f, 0, Mathf.Sin(angle) * radius * 3.5f);
            var prop = Place(Asset(props[i % props.Length]), scenery, props[i % props.Length] + "_" + i, point, i * 47f);
            if (props[i % props.Length] == "pg_tree") prop.transform.localScale *= 2f;
        }
        for (int i = 0; i < 8; i++)
        {
            float angle = (i * 45f + 18f) * Mathf.Deg2Rad;
            float radius = 17f + (i % 2) * 6f;
            var point = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
            var tree = Place(Asset("pg_tree"), scenery, "pg_tree_inner_" + i, point, i * 37f);
            tree.transform.localScale *= 2f;
        }
        var lights = new GameObject("GardenLight"); lights.transform.SetParent(world.transform, false);
        var light = lights.AddComponent<Light>(); light.type = LightType.Directional;
        light.intensity = 1.1f; light.color = new Color(1f,.82f,.88f); light.shadows = LightShadows.Soft;
        lights.transform.rotation = Quaternion.Euler(48,-35,0);

        var gameplay = UnityEngine.Object.Instantiate(originalGameplay);
        gameplay.name = "StageGameplay";
        SceneManager.MoveGameObjectToScene(gameplay, scene);
        var advance = gameplay.GetComponent<StageEvolutionAdvanceHook>();
        if (advance != null) UnityEngine.Object.DestroyImmediate(advance);
        var ledger = gameplay.GetComponent<StageEnergyLedger>(); ledger.ConfigureStage(2, StageEvolutionLineage.Demonic);
        var player = gameplay.GetComponentInChildren<VirginDropPlayerController>(true);
        player.transform.position = new Vector3(0,.02f,-2.5f);
        foreach (var renderer in player.GetComponents<Renderer>()) renderer.enabled = false;
        var playerVisual = Place(profane, player.transform, "ProfaneDropVisual", player.transform.position);
        playerVisual.transform.localPosition = Vector3.zero;
        playerVisual.transform.localRotation = profane.transform.localRotation;
        player.Configure(playerVisual.transform, playerVisual.GetComponentInChildren<Animator>(), walk, attack);
        var camera = gameplay.GetComponentInChildren<ThirdPersonFollowCamera>(true);
        camera.Configure(player.transform);
        var interaction = player.GetComponent<StageNpcInteractionController>();
        var sequence = gameplay.AddComponent<StageEvolutionSequence>();
        // Second-stage forms are not authored yet. An empty mapping intentionally
        // leaves evolution unconfirmed rather than replacing the player incorrectly.
        sequence.Configure(ledger, player, playerVisual.transform, camera.GetComponent<Camera>(), camera,
            new Behaviour[] { interaction, player.GetComponent<StageFruitInteractionController>(),
                player.GetComponent<StageInteractionSelector>() }, Array.Empty<StageEvolutionSequence.Form>(), null);

        var plantRoot = new GameObject("PurifiablePlants"); plantRoot.transform.SetParent(world.transform, false);
        for (int i = 0; i < 10; i++)
        {
            float angle = i * 137.50776f * Mathf.Deg2Rad;
            float radius = 8f + (i % 3) * 8f;
            var pos = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
            var holder = new GameObject("PurifiablePlant_" + (i+1).ToString("00"));
            holder.transform.SetParent(plantRoot.transform, false); holder.transform.position = pos;
            var bad = Place(corrupted, holder.transform, "Corrupted", pos, i * 31f);
            var good = Place(purified, holder.transform, "Purified", pos, i * 31f);
            var hit = holder.AddComponent<SphereCollider>(); hit.center = new Vector3(0,.5f,0); hit.radius = .6f;
            var target = holder.AddComponent<StagePurifiablePlant>(); target.Configure(bad, good);
        }

        var npcTemplate = UnityEngine.Object.Instantiate(originalNpc);
        npcTemplate.name = "ProfaneNpcTemplate"; SceneManager.MoveGameObjectToScene(npcTemplate, scene);
        npcTemplate.SetActive(false);
        var obsoleteHighlight = npcTemplate.GetComponent<StageTargetHighlight>();
        if (obsoleteHighlight != null) UnityEngine.Object.DestroyImmediate(obsoleteHighlight);
        var oldVisual = npcTemplate.transform.Find("VisualBody");
        if (oldVisual != null) UnityEngine.Object.DestroyImmediate(oldVisual.gameObject);
        var oldPuddle = npcTemplate.transform.Find("PuddleVisual");
        if (oldPuddle != null) UnityEngine.Object.DestroyImmediate(oldPuddle.gameObject);
        var npcVisual = Place(profane, npcTemplate.transform, "ProfaneVisual", npcTemplate.transform.position);
        npcVisual.transform.localPosition = Vector3.zero; npcVisual.transform.localRotation = profane.transform.localRotation;
        var npc = npcTemplate.GetComponent<StageDropletNpcTarget>();
        npc.ConfigurePresentation(npcVisual.transform, null, walk, death); npc.ConfigureGroundCollider(ground);
        var npcSo = new SerializedObject(npc); npcSo.FindProperty("fadeRenderers").arraySize = 0;
        npcSo.FindProperty("wanderSpeed").floatValue = .7f; npcSo.FindProperty("wanderRadius").floatValue = 1.5f;
        npcSo.ApplyModifiedPropertiesWithoutUndo();
        var spawnerObject = new GameObject("ProfaneNpcSpawner");
        var spawner = spawnerObject.AddComponent<ProfaneNpcSpawner>();
        spawner.Configure(npcTemplate, 10, 24f);

        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene, ScenePath)) throw new Exception("Failed to save scene");
        var buildScenes = EditorBuildSettings.scenes.ToList();
        if (!buildScenes.Any(x => x.path == ScenePath)) { buildScenes.Add(new EditorBuildSettingsScene(ScenePath, true)); EditorBuildSettings.scenes = buildScenes.ToArray(); }
        if (!EditorSceneManager.CloseScene(original, true)) throw new Exception("Could not close clean Stage01 scene");
        return "Saved " + ScenePath + "; plants=10, NPC population=10 with delayed replacements; Stage01 scene was not saved or modified.";
    }
}
