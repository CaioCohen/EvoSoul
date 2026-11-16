using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ExpandBercarioStage
{
    private const float TileWidth = 32f;
    private const float TileDepth = 24f;

    public static object Run()
    {
        var scene = SceneManager.GetActiveScene();
        if (scene.path != "Assets/Scenes/BercarioStage01.unity")
            throw new InvalidOperationException($"Expected BercarioStage01, got {scene.path}.");

        var world = Require("BercarioWorld");
        var sourceEnvironment = Require("EnvironmentVisual");
        var fruitRoot = Require("StageFruitTargets");
        var npcRoot = Require("NurseryDroplets");
        var ground = sourceEnvironment.GetComponentInChildren<MeshCollider>();
        if (ground == null)
            throw new InvalidOperationException("Environment terrain MeshCollider is missing.");

        DestroyIfPresent("ExpandedEnvironmentTiles");
        DestroyIfPresent("ExpandedFruitVisuals");
        DestroyChildrenStartingWith(fruitRoot.transform, "FruitTarget_0", 6);
        DestroyChildrenStartingWith(npcRoot.transform, "DropletNpc_", 5);

        var tilesRoot = new GameObject("ExpandedEnvironmentTiles").transform;
        tilesRoot.SetParent(world.transform, false);
        var offsets = new[]
        {
            new Vector3(-TileWidth, 0f, -TileDepth), new Vector3(0f, 0f, -TileDepth), new Vector3(TileWidth, 0f, -TileDepth),
            new Vector3(-TileWidth, 0f, 0f),                                         new Vector3(TileWidth, 0f, 0f),
            new Vector3(-TileWidth, 0f, TileDepth),  new Vector3(0f, 0f, TileDepth),  new Vector3(TileWidth, 0f, TileDepth)
        };
        for (var index = 0; index < offsets.Length; index++)
        {
            var clone = UnityEngine.Object.Instantiate(sourceEnvironment, tilesRoot);
            clone.name = $"EnvironmentTile_{index + 1:00}";
            clone.transform.position = sourceEnvironment.transform.position + offsets[index];
        }

        UpdateBoundaries();
        var fruitVisualRoot = new GameObject("ExpandedFruitVisuals").transform;
        fruitVisualRoot.SetParent(world.transform, false);
        ExpandFruitTargets(fruitRoot.transform, fruitVisualRoot, ground);
        ExpandNpcTargets(npcRoot.transform, ground);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        return new
        {
            playableSize = "96 x 72 metres",
            environmentTiles = 9,
            fruitTargets = fruitRoot.GetComponentsInChildren<StageFruitTarget>(true).Length,
            npcTargets = npcRoot.GetComponentsInChildren<StageDropletNpcTarget>(true).Length,
            terrainColliders = world.GetComponentsInChildren<MeshCollider>(true).Length
        };
    }

    private static void ExpandFruitTargets(Transform root, Transform visualRoot, MeshCollider ground)
    {
        var source = root.Find("FruitTarget_01");
        if (source == null)
            throw new InvalidOperationException("FruitTarget_01 is missing.");
        var sourceTarget = source.GetComponent<StageFruitTarget>();
        var fruitVisualField = typeof(StageFruitTarget).GetField("fruitVisual", BindingFlags.Instance | BindingFlags.NonPublic);
        var sourceVisual = fruitVisualField?.GetValue(sourceTarget) as GameObject;
        if (sourceVisual == null)
            throw new InvalidOperationException("FruitTarget_01 has no assigned fruit visual.");

        var sourceLeaf = sourceVisual.transform.parent != null
            ? sourceVisual.transform.parent.Find(sourceVisual.name + "_Leaf")
            : null;
        var positions = new[]
        {
            new Vector3(-37f, 0f, -16f), new Vector3(-25f, 0f, 16f), new Vector3(24f, 0f, -17f),
            new Vector3(37f, 0f, 15f), new Vector3(40f, 0f, -4f)
        };
        var visualOffset = sourceVisual.transform.position - source.position;
        var leafOffset = sourceLeaf != null ? sourceLeaf.position - source.position : Vector3.zero;

        for (var index = 0; index < positions.Length; index++)
        {
            var position = SnapToGround(positions[index], ground) + Vector3.up * source.position.y;
            var target = UnityEngine.Object.Instantiate(source.gameObject, root);
            target.name = $"FruitTarget_{index + 6:00}";
            target.transform.position = position;
            ResetTargetId(target.GetComponent<StageInteractionTarget>());

            var visual = UnityEngine.Object.Instantiate(sourceVisual, visualRoot);
            visual.name = $"Expanded_{sourceVisual.name}_{index + 6:00}";
            visual.transform.position = position + visualOffset;
            fruitVisualField.SetValue(target.GetComponent<StageFruitTarget>(), visual);
            if (sourceLeaf != null)
            {
                var leaf = UnityEngine.Object.Instantiate(sourceLeaf.gameObject, visualRoot);
                leaf.name = $"Expanded_{sourceLeaf.name}_{index + 6:00}";
                leaf.transform.position = position + leafOffset;
            }
        }
    }

    private static void ExpandNpcTargets(Transform root, MeshCollider ground)
    {
        var source = root.Find("DropletNpc_01");
        if (source == null)
            throw new InvalidOperationException("DropletNpc_01 is missing.");
        var positions = new[]
        {
            new Vector3(-38f, 0f, -8f), new Vector3(-28f, 0f, 12f), new Vector3(-18f, 0f, -18f),
            new Vector3(18f, 0f, 18f), new Vector3(28f, 0f, -10f), new Vector3(40f, 0f, 5f)
        };
        for (var index = 0; index < positions.Length; index++)
        {
            var npc = UnityEngine.Object.Instantiate(source.gameObject, root);
            npc.name = $"DropletNpc_{index + 5:00}";
            var position = SnapToGround(positions[index], ground);
            npc.transform.position = position;
            var target = npc.GetComponent<StageDropletNpcTarget>();
            ResetTargetId(target);
            SetPrivate(target, "fallbackWanderCenter", position);
            SetPrivate(target, "nextWanderPosition", position);
        }
    }

    private static void UpdateBoundaries()
    {
        ConfigureBoundary(Require("NorthBoundary").GetComponent<BoxCollider>(), new Vector3(0f, 1f, 36.6f), new Vector3(96f, 3f, 1.2f));
        ConfigureBoundary(Require("SouthBoundary").GetComponent<BoxCollider>(), new Vector3(0f, 1f, -36.6f), new Vector3(96f, 3f, 1.2f));
        ConfigureBoundary(Require("EastBoundary").GetComponent<BoxCollider>(), new Vector3(48.6f, 1f, 0f), new Vector3(1.2f, 3f, 72f));
        ConfigureBoundary(Require("WestBoundary").GetComponent<BoxCollider>(), new Vector3(-48.6f, 1f, 0f), new Vector3(1.2f, 3f, 72f));
    }

    private static void ConfigureBoundary(BoxCollider collider, Vector3 position, Vector3 size)
    {
        collider.transform.position = position;
        collider.size = size;
    }

    private static Vector3 SnapToGround(Vector3 desired, MeshCollider fallbackGround)
    {
        var ray = new Ray(desired + Vector3.up * 20f, Vector3.down);
        if (Physics.Raycast(ray, out var hit, 60f, ~0, QueryTriggerInteraction.Ignore) && hit.collider is MeshCollider)
            return hit.point;
        return new Vector3(desired.x, fallbackGround.bounds.max.y, desired.z);
    }

    private static void ResetTargetId(StageInteractionTarget target)
    {
        SetPrivate(target, "rewardSourceId", Guid.NewGuid().ToString("N"));
        SetPrivate(target, "isAvailable", true);
        SetPrivate(target, "interactionReserved", false);
        SetPrivate(target, "rewardClaimed", false);
    }

    private static void SetPrivate(object instance, string name, object value)
    {
        var type = instance.GetType();
        while (type != null)
        {
            var field = type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field != null)
            {
                field.SetValue(instance, value);
                return;
            }
            type = type.BaseType;
        }
        throw new MissingFieldException(instance.GetType().Name, name);
    }

    private static void DestroyIfPresent(string name)
    {
        var item = GameObject.Find(name);
        if (item != null)
            UnityEngine.Object.DestroyImmediate(item);
    }

    private static void DestroyChildrenStartingWith(Transform root, string prefix, int firstIndexToRemove)
    {
        var toDestroy = new List<GameObject>();
        for (var index = 0; index < root.childCount; index++)
        {
            var child = root.GetChild(index).gameObject;
            if (!child.name.StartsWith(prefix, StringComparison.Ordinal))
                continue;
            var suffix = child.name.Substring(prefix.Length);
            if (int.TryParse(suffix, out var number) && number >= firstIndexToRemove)
                toDestroy.Add(child);
        }
        foreach (var child in toDestroy)
            UnityEngine.Object.DestroyImmediate(child);
    }

    private static GameObject Require(string name)
    {
        var item = GameObject.Find(name);
        if (item == null)
            throw new InvalidOperationException($"Missing required object: {name}");
        return item;
    }
}
