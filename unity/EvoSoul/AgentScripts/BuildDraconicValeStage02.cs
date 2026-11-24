using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Builds the independent Stage 02 Draconic Vale without editing Stage 01.</summary>
public static class BuildDraconicValeStage02
{
    private const string Directory = "Assets/Art/Environment/DraconicVale/";
    private const string ScenePath = "Assets/Scenes/DraconicValeStage02.unity";

    private static GameObject Asset(string name)
    {
        var asset = AssetDatabase.LoadAssetAtPath<GameObject>(Directory + name + ".fbx");
        if (asset == null) throw new Exception("Missing Draconic Vale asset: " + name);
        return asset;
    }

    private static GameObject Place(GameObject prefab, Transform parent, string name, Vector3 position, float yaw = 0f)
    {
        var item = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        item.name = name;
        item.transform.SetParent(parent, false);
        item.transform.SetPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f) * prefab.transform.rotation);
        return item;
    }

    private static AnimationClip Clip(string name) => AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Characters/drakeling_slime_animated.fbx")
        .OfType<AnimationClip>().First(c => !c.name.StartsWith("__preview__") && c.name.EndsWith(name));

    public static string Main()
    {
        var original = SceneManager.GetActiveScene();
        if (original.path != "Assets/Scenes/BercarioStage01.unity" || original.isDirty)
            throw new Exception("Open a clean BercarioStage01 scene before building Draconic Vale.");
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null)
            throw new Exception("Draconic Vale already exists; refusing to overwrite it.");

        var stageGameplay = original.GetRootGameObjects().First(o => o.name == "StageGameplay");
        var nurseryNpc = original.GetRootGameObjects().First(o => o.name == "NurseryDroplets").transform.GetChild(0).gameObject;
        var drakeling = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Characters/drakeling_slime_animated.fbx");
        if (drakeling == null) throw new Exception("Missing animated Drakeling Slime.");
        var walk = Clip("Walk");
        var attack = Clip("Attack");
        var death = Clip("Death");

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        SceneManager.SetActiveScene(scene);
        var world = new GameObject("DraconicValeWorld");
        var ground = CreateTerrain(world.transform);

        CreateOcean(world.transform);
        CreateBoundaries(world.transform);
        var decor = new GameObject("ValeDecoration").transform; decor.SetParent(world.transform, false);
        CreateDecoration(decor);
        ConfigureTreeTrunkColliders(decor);
        CreateLighting(world.transform);

        var gameplay = UnityEngine.Object.Instantiate(stageGameplay);
        gameplay.name = "StageGameplay";
        SceneManager.MoveGameObjectToScene(gameplay, scene);
        var advance = gameplay.GetComponent<StageEvolutionAdvanceHook>();
        if (advance != null) UnityEngine.Object.DestroyImmediate(advance);
        var ledger = gameplay.GetComponent<StageEnergyLedger>();
        ledger.ConfigureStage(2, StageEvolutionLineage.Draconic);
        var player = gameplay.GetComponentInChildren<VirginDropPlayerController>(true);
        player.transform.position = new Vector3(0f, .02f, -3f);
        foreach (var renderer in player.GetComponentsInChildren<Renderer>(true)) renderer.enabled = false;
        var visual = Place(drakeling, player.transform, "DrakelingSlimeVisual", player.transform.position);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = drakeling.transform.localRotation;
        player.Configure(visual.transform, visual.GetComponentInChildren<Animator>(), walk, attack);
        var camera = gameplay.GetComponentInChildren<ThirdPersonFollowCamera>(true);
        camera.Configure(player.transform);
        var interaction = player.GetComponent<StageNpcInteractionController>();
        var sequence = gameplay.AddComponent<StageEvolutionSequence>();
        sequence.Configure(ledger, player, visual.transform, camera.GetComponent<Camera>(), camera,
            new Behaviour[] { interaction, player.GetComponent<StageFruitInteractionController>(), player.GetComponent<StageInteractionSelector>() },
            Array.Empty<StageEvolutionSequence.Form>(), null);

        CreateRestorableBushes(world.transform, ground);
        CreateDrakelingNpcs(nurseryNpc, drakeling, walk, death, ground);
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene, ScenePath)) throw new Exception("Could not save Draconic Vale.");
        var scenes = EditorBuildSettings.scenes.ToList();
        if (!scenes.Any(s => s.path == ScenePath)) { scenes.Add(new EditorBuildSettingsScene(ScenePath, true)); EditorBuildSettings.scenes = scenes.ToArray(); }
        EditorSceneManager.CloseScene(original, true);
        return "Saved Draconic Vale Stage 02 with 10 orange-to-green bushes and a 10 Drakeling NPC cap.";
    }

    private static void CreateOcean(Transform world)
    {
        var sea = GameObject.CreatePrimitive(PrimitiveType.Plane);
        sea.name = "ValeOcean";
        sea.transform.SetParent(world, false);
        sea.transform.position = new Vector3(0f, -2.15f, 0f);
        sea.transform.localScale = new Vector3(200f, 1f, 200f);
        UnityEngine.Object.DestroyImmediate(sea.GetComponent<Collider>());
        sea.GetComponent<MeshRenderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Environment/Ocean/BercarioOceanWater.mat");
        sea.AddComponent<OceanSurfaceMotion>();
    }

    // Deliberately use a Unity primitive for the playable island. It avoids
    // FBX axis conversion affecting collision while all visible garden props
    // remain sourced from the Blender asset kit.
    private static MeshCollider CreateTerrain(Transform world)
    {
        var island = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        island.name = "ValeTerrain";
        island.transform.SetParent(world, false);
        island.transform.position = new Vector3(0f, -.7f, 0f);
        // Stage 01-scale playable land: broad enough for exploration, with the
        // ocean remaining purely visual beyond the physical perimeter.
        island.transform.localScale = new Vector3(96f, .7f, 72f);
        var materialPath = Directory + "DraconicGrass.mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (material == null)
        {
            material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            material.color = new Color(.20f, .48f, .19f, 1f);
            AssetDatabase.CreateAsset(material, materialPath);
        }
        island.GetComponent<MeshRenderer>().sharedMaterial = material;
        var primitiveCollider = island.GetComponent<Collider>();
        UnityEngine.Object.DestroyImmediate(primitiveCollider);
        var meshCollider = island.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = island.GetComponent<MeshFilter>().sharedMesh;
        return meshCollider;
    }

    private static void CreateBoundaries(Transform world)
    {
        var holder = new GameObject("IslandBoundaries").transform; holder.SetParent(world, false);
        for (int i = 0; i < 24; i++)
        {
            float angle = i * 15f * Mathf.Deg2Rad;
            var boundary = new GameObject("Boundary_" + i); boundary.transform.SetParent(holder, false);
            boundary.transform.SetPositionAndRotation(new Vector3(Mathf.Cos(angle) * 47.2f, 1f, Mathf.Sin(angle) * 35.2f), Quaternion.Euler(0f, -i * 15f, 0f));
            boundary.AddComponent<BoxCollider>().size = new Vector3(5.5f, 3f, .6f);
        }
    }

    private static void CreateDecoration(Transform decor)
    {
        var assets = new[] { "tree", "tree", "rock", "fern", "flower", "log" };
        var random = new System.Random(96173);
        for (int i = 0; i < 42; i++)
        {
            string asset = assets[i % assets.Length];
            float angle = (float)(random.NextDouble() * Mathf.PI * 2d);
            float radius = asset == "tree" ? Mathf.Lerp(28f, 43f, (float)random.NextDouble()) : Mathf.Lerp(10f, 42f, (float)random.NextDouble());
            var p = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
            if (p.sqrMagnitude < 64f) { i--; continue; }
            float yaw = (float)(random.NextDouble() * 360d);
            switch (asset)
            {
                case "tree": CreateCompleteTree(decor, "drk_tree_" + i, p, yaw); break;
                case "rock": CreateSimpleProp(decor, "drk_rock_" + i, p, yaw, PrimitiveType.Sphere, new Vector3(1.3f,.7f,1f), RockMaterial()); break;
                case "fern": CreateSimpleProp(decor, "drk_fern_" + i, p, yaw, PrimitiveType.Capsule, new Vector3(.35f,.7f,.35f), LeafMaterial()); break;
                case "flower": CreateSimpleProp(decor, "drk_flower_" + i, p, yaw, PrimitiveType.Sphere, new Vector3(.45f,.35f,.45f), FlowerMaterial()); break;
                default: CreateSimpleProp(decor, "drk_log_" + i, p, yaw, PrimitiveType.Cylinder, new Vector3(.38f,1.2f,.38f), BarkMaterial()); break;
            }
        }
    }

    private static Material Material(string name, Color color)
    {
        var path = Directory + name + ".mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material != null) return material;
        material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard")) { color = color };
        AssetDatabase.CreateAsset(material, path);
        return material;
    }
    private static Material BarkMaterial() => Material("DraconicBark", new Color(.25f,.15f,.06f));
    private static Material LeafMaterial() => Material("DraconicLeaf", new Color(.20f,.58f,.16f));
    private static Material RockMaterial() => Material("DraconicRock", new Color(.30f,.37f,.34f));
    private static Material FlowerMaterial() => Material("DraconicFlower", new Color(1f,.72f,.12f));

    private static void CreateCompleteTree(Transform parent, string name, Vector3 position, float yaw)
    {
        var root = new GameObject(name); root.transform.SetParent(parent, false);
        root.transform.SetPositionAndRotation(position, Quaternion.Euler(0f,yaw,0f));
        var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk.name = "TreeTrunk"; trunk.transform.SetParent(root.transform, false);
        trunk.transform.localPosition = Vector3.up * 2f; trunk.transform.localScale = new Vector3(.55f,2f,.55f);
        trunk.GetComponent<MeshRenderer>().sharedMaterial = BarkMaterial();
        UnityEngine.Object.DestroyImmediate(trunk.GetComponent<Collider>());
        var crown = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        crown.name = "TreeCanopy"; crown.transform.SetParent(root.transform, false);
        crown.transform.localPosition = Vector3.up * 4.5f; crown.transform.localScale = new Vector3(2.35f,1.65f,2.35f);
        crown.GetComponent<MeshRenderer>().sharedMaterial = LeafMaterial();
        UnityEngine.Object.DestroyImmediate(crown.GetComponent<Collider>());
        var crown2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        crown2.name = "TreeCanopyTop"; crown2.transform.SetParent(root.transform, false);
        crown2.transform.localPosition = Vector3.up * 5.8f; crown2.transform.localScale = new Vector3(1.5f,1.1f,1.5f);
        crown2.GetComponent<MeshRenderer>().sharedMaterial = LeafMaterial();
        UnityEngine.Object.DestroyImmediate(crown2.GetComponent<Collider>());
    }

    private static void CreateSimpleProp(Transform parent, string name, Vector3 position, float yaw, PrimitiveType type, Vector3 scale, Material material)
    {
        var prop = GameObject.CreatePrimitive(type);
        prop.name = name; prop.transform.SetParent(parent, false);
        Quaternion rotation = Quaternion.Euler(0f,yaw,0f);
        Vector3 heightOffset = Vector3.up * (scale.y * .5f);
        // A decorative log is horizontal; other primitive props stand upright.
        if (type == PrimitiveType.Cylinder)
        {
            rotation *= Quaternion.Euler(0f,0f,90f);
            heightOffset = Vector3.up * scale.x;
        }
        prop.transform.SetPositionAndRotation(position + heightOffset, rotation);
        prop.transform.localScale = scale; prop.GetComponent<MeshRenderer>().sharedMaterial = material;
        UnityEngine.Object.DestroyImmediate(prop.GetComponent<Collider>());
    }

    private static void ConfigureTreeTrunkColliders(Transform decor)
    {
        foreach (var tree in decor.Cast<Transform>().Where(t => t.name.StartsWith("drk_tree")))
        foreach (var renderer in tree.GetComponentsInChildren<Renderer>(true).Where(r => r.name.IndexOf("trunk", StringComparison.OrdinalIgnoreCase) >= 0))
        {
            var c = renderer.gameObject.AddComponent<CapsuleCollider>();
            var b = renderer.bounds; var s = renderer.transform.lossyScale;
            c.center = renderer.transform.InverseTransformPoint(b.center); c.direction = 1; c.isTrigger = false;
            c.height = Mathf.Max(.2f, b.size.y / Mathf.Max(.0001f, Mathf.Abs(s.y)));
            c.radius = Mathf.Max(.05f, Mathf.Min(b.size.x / Mathf.Max(.0001f, Mathf.Abs(s.x)), b.size.z / Mathf.Max(.0001f, Mathf.Abs(s.z))) * .32f);
        }
    }

    private static void CreateLighting(Transform world)
    {
        var sun = new GameObject("ValeSun").AddComponent<Light>(); sun.transform.SetParent(world, false);
        sun.type = LightType.Directional; sun.intensity = 1.15f; sun.color = new Color(.96f, .92f, .78f); sun.shadows = LightShadows.Soft;
        sun.transform.rotation = Quaternion.Euler(52f, -35f, 0f);
    }

    private static void CreateRestorableBushes(Transform world, Collider ground)
    {
        var holder = new GameObject("RestorableOrangeBushes").transform; holder.SetParent(world, false);
        for (int i = 0; i < 10; i++)
        {
            float angle = i * 137.50776f * Mathf.Deg2Rad, radius = 8f + (i % 3) * 6f;
            var pos = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
            var bush = new GameObject("OrangeBush_" + (i + 1).ToString("00")); bush.transform.SetParent(holder, false); bush.transform.position = pos;
            var orange = Place(Asset("drk_bush_orange"), bush.transform, "Orange", pos, i * 29f);
            var green = Place(Asset("drk_bush_green"), bush.transform, "Green", pos, i * 29f);
            var hit = bush.AddComponent<SphereCollider>(); hit.center = new Vector3(0f, .55f, 0f); hit.radius = .65f;
            bush.AddComponent<StagePurifiablePlant>().Configure(orange, green, "Restaurar arbusto");
        }
    }

    private static void CreateDrakelingNpcs(GameObject originalNpc, GameObject drakeling, AnimationClip walk, AnimationClip death, Collider ground)
    {
        var template = UnityEngine.Object.Instantiate(originalNpc); template.name = "DrakelingNpcTemplate";
        SceneManager.MoveGameObjectToScene(template, SceneManager.GetActiveScene()); template.SetActive(false);
        var oldVisual = template.transform.Find("VisualBody"); if (oldVisual != null) UnityEngine.Object.DestroyImmediate(oldVisual.gameObject);
        var oldPuddle = template.transform.Find("PuddleVisual"); if (oldPuddle != null) UnityEngine.Object.DestroyImmediate(oldPuddle.gameObject);
        var visual = Place(drakeling, template.transform, "DrakelingVisual", template.transform.position);
        visual.transform.localPosition = Vector3.zero; visual.transform.localRotation = drakeling.transform.localRotation;
        var npc = template.GetComponent<StageDropletNpcTarget>(); npc.ConfigurePresentation(visual.transform, null, walk, death); npc.ConfigureGroundCollider(ground);
        var data = new SerializedObject(npc); data.FindProperty("fadeRenderers").arraySize = 0; data.FindProperty("wanderSpeed").floatValue = .72f; data.FindProperty("wanderRadius").floatValue = 1.7f; data.ApplyModifiedPropertiesWithoutUndo();
        var spawner = new GameObject("DrakelingNpcSpawner").AddComponent<ProfaneNpcSpawner>(); spawner.Configure(template, 10, 15f);
    }
}
