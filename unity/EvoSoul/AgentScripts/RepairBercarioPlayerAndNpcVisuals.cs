using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class RepairBercarioPlayerAndNpcVisuals
{
    public static object Run()
    {
        if (EditorApplication.isPlaying)
            throw new InvalidOperationException("Exit Play Mode before applying this persistent scene repair.");

        var player = GameObject.Find("VirginDropPlayer");
        var playerRenderer = player != null ? player.GetComponent<SkinnedMeshRenderer>() : null;
        if (playerRenderer == null || playerRenderer.sharedMesh == null)
            throw new InvalidOperationException("The player Virgin Drop renderer is unavailable.");

        var deathClip = AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Models/virgin_drop.fbx")
            .OfType<AnimationClip>()
            .FirstOrDefault(clip => string.Equals(clip.name, "Death", StringComparison.OrdinalIgnoreCase));
        if (deathClip == null)
            throw new InvalidOperationException("The Virgin Drop Death clip could not be found.");

        var deathField = typeof(StageDropletNpcTarget).GetField("deathClip", BindingFlags.Instance | BindingFlags.NonPublic);
        if (deathField == null)
            throw new InvalidOperationException("StageDropletNpcTarget.deathClip field is unavailable.");

        var npcs = UnityEngine.Object.FindObjectsByType<StageDropletNpcTarget>(FindObjectsInactive.Exclude);
        foreach (var npc in npcs)
        {
            var visual = npc.transform.Find("VisualBody");
            if (visual == null)
                throw new InvalidOperationException($"{npc.name} is missing VisualBody.");

            var meshFilter = visual.GetComponent<MeshFilter>();
            var meshRenderer = visual.GetComponent<MeshRenderer>();
            if (meshFilter == null || meshRenderer == null)
                throw new InvalidOperationException($"{npc.name}/VisualBody lacks a MeshFilter or MeshRenderer.");

            meshFilter.sharedMesh = playerRenderer.sharedMesh;
            meshRenderer.sharedMaterials = playerRenderer.sharedMaterials;
            visual.localPosition = Vector3.zero;
            visual.localRotation = Quaternion.identity;
            visual.localScale = Vector3.one;
            deathField.SetValue(npc, deathClip);
            EditorUtility.SetDirty(npc);
            EditorUtility.SetDirty(meshFilter);
            EditorUtility.SetDirty(meshRenderer);
        }

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        return new
        {
            npcCount = npcs.Length,
            mesh = playerRenderer.sharedMesh.name,
            materials = playerRenderer.sharedMaterials.Where(material => material != null).Select(material => material.name).ToArray(),
            deathClip = deathClip.name,
            scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path
        };
    }
}
