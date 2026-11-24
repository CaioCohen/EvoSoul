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

    // Kept separate from Main so an already-authored Stage 02 scene can be
    // safely rebalanced without rebuilding or touching Stage 01.
    public static string PolishCurrentStage02()
    {
        var scene = SceneManager.GetActiveScene();
        if (scene.path != ScenePath)
            throw new Exception("Open ProfaneGardenStage02 before polishing it.");

        var world = GameObject.Find("ProfaneGardenWorld");
        if (world == null) throw new Exception("ProfaneGardenWorld is missing.");

        // The previous runtime combiner generated zero-bounds meshes for the
        // multi-material FBX props after a scene transition. Keeping the
        // lightweight source renderers is more reliable than culling the
        // garden; the kit remains comfortably small for this single stage.
        var visualBatcher = world.GetComponent<ProfaneGardenVisualBatcher>();
        if (visualBatcher != null) visualBatcher.enabled = false;
        RestoreSourceRenderers(world.transform.Find("GardenDecoration"));
        RestoreSourceRenderers(world.transform.Find("PurifiablePlants"));

        // Spread ambient scenery across the full island instead of pulling it
        // into a central ring. A fixed seed keeps the authored layout stable
        // between direct loads and portal transitions. The spawn/interaction
        // pocket remains open so the player never appears inside decoration.
        var decor = world.transform.Find("GardenDecoration");
        if (decor != null)
        {
            var random = new System.Random(73129);
            var playerSpawn = new Vector2(0f, -2.5f);
            foreach (Transform item in decor)
            {
                if (item.name == "LavaSea") continue;
                bool isTree = item.name.Contains("tree");
                Vector2 candidate = Vector2.zero;
                // Trees form a sparse outer silhouette; smaller props fill
                // the middle and outer rings without crowding the centre.
                for (int attempt = 0; attempt < 32; attempt++)
                {
                    float angle = (float)(random.NextDouble() * Mathf.PI * 2d);
                    float minRadius = isTree ? 19f : 10f;
                    float maxRadius = isTree ? 29f : 28f;
                    float radius = Mathf.Lerp(minRadius, maxRadius, Mathf.Sqrt((float)random.NextDouble()));
                    candidate = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                    if ((candidate - playerSpawn).sqrMagnitude >= 64f) break;
                }
                item.position = new Vector3(candidate.x, item.position.y, candidate.y);
                // Deliberately preserve the authored transform orientation.  Imported
                // FBX props have a conversion rotation (notably trees), so assigning a
                // fresh world rotation here would discard it and lay the mesh down.
            }
        }

        // Keep all ten NPCs available, but place their initial wandering
        // radius close enough that a player arriving from the portal reads a
        // living garden immediately.
        var spawner = UnityEngine.Object.FindAnyObjectByType<ProfaneNpcSpawner>();
        if (spawner != null)
        {
            var so = new SerializedObject(spawner);
            so.FindProperty("radius").floatValue = 14f;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        CreateOrRefreshLavaSea(world.transform, decor);
        ConfigureTreeTrunkColliders(decor);
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene, ScenePath)) throw new Exception("Could not save Stage 02.");
        return "Polished Stage 02: deterministically distributed ambient decoration, preserved clear spawn pocket and NPC arrival radius; lava sea created.";
    }

    static void CreateOrRefreshLavaSea(Transform world, Transform decor)
    {
        var old = world.Find("LavaSea");
        if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
        var sea = GameObject.CreatePrimitive(PrimitiveType.Plane);
        sea.name = "LavaSea";
        sea.transform.SetParent(world, false);
        sea.transform.position = new Vector3(0f, -4.15f, 0f);
        // Match Stage 01's ocean coverage.  A primitive plane is 10 metres
        // across, so this one low-poly quad covers 2 km square: the gameplay
        // camera can never reach its visual edge around the 73m island.
        sea.transform.localScale = new Vector3(200f, 1f, 200f);
        var collider = sea.GetComponent<Collider>();
        if (collider != null) UnityEngine.Object.DestroyImmediate(collider);
        var path = Directory + "PG_Lava.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        var oceanShader = AssetDatabase.LoadAssetAtPath<Shader>(
            "Assets/Art/Environment/Ocean/BercarioOceanWater.shadergraph");
        var lavaTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(Directory + "PG_LavaPattern.png");
        if (mat == null || oceanShader == null || lavaTexture == null)
            throw new Exception("Missing Profane lava material, texture, or Stage 01 ocean shader.");

        // The lava intentionally uses the exact repeated, scrolling Shader
        // Graph arrangement as Stage 01 water; only the texture/palette and
        // flow direction differ. This prevents a flat colour plane and keeps
        // the visual cost to a single textured quad.
        mat.shader = oceanShader;
        mat.SetTexture("_BaseMap", lavaTexture);
        mat.SetTextureScale("_BaseMap", new Vector2(72f, 72f));
        mat.SetColor("_BaseColor", Color.white);
        EditorUtility.SetDirty(mat);

        var movement = sea.AddComponent<OceanSurfaceMotion>();
        var motion = new SerializedObject(movement);
        motion.FindProperty("scrollSpeed").vector2Value = new Vector2(-.018f, .011f);
        motion.FindProperty("textureScale").vector2Value = new Vector2(72f, 72f);
        motion.ApplyModifiedPropertiesWithoutUndo();
        sea.GetComponent<MeshRenderer>().sharedMaterial = mat;
    }

    static void RestoreSourceRenderers(Transform root)
    {
        if (root == null) return;
        var combined = root.GetComponentsInChildren<Transform>(true)
            .Where(t => t.name == "CombinedVisual").ToArray();
        foreach (var item in combined) UnityEngine.Object.DestroyImmediate(item.gameObject);
        foreach (var renderer in root.GetComponentsInChildren<MeshRenderer>(true))
            renderer.enabled = true;
    }

    // Trees are solid at their trunks only, matching the Stage 01 convention.
    // Canopies and branches deliberately remain non-blocking so their broad
    // low-poly silhouettes do not create invisible walls for the player.
    static void ConfigureTreeTrunkColliders(Transform decor)
    {
        if (decor == null) return;
        foreach (var tree in decor.Cast<Transform>().Where(item => item.name.StartsWith("pg_tree")))
        {
            foreach (var renderer in tree.GetComponentsInChildren<Renderer>(true)
                .Where(item => item.name.IndexOf("TreeTrunk", StringComparison.OrdinalIgnoreCase) >= 0))
            {
                var collider = renderer.GetComponent<CapsuleCollider>();
                if (collider == null) collider = renderer.gameObject.AddComponent<CapsuleCollider>();
                var bounds = renderer.bounds;
                var scale = renderer.transform.lossyScale;
                float safeX = Mathf.Max(.0001f, Mathf.Abs(scale.x));
                float safeY = Mathf.Max(.0001f, Mathf.Abs(scale.y));
                float safeZ = Mathf.Max(.0001f, Mathf.Abs(scale.z));
                collider.center = renderer.transform.InverseTransformPoint(bounds.center);
                collider.direction = 1;
                collider.height = Mathf.Max(.2f, bounds.size.y / safeY);
                collider.radius = Mathf.Max(.05f, Mathf.Min(bounds.size.x / safeX, bounds.size.z / safeZ) * .32f);
                collider.isTrigger = false;
            }
        }
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
        ConfigureTreeTrunkColliders(scenery);
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
