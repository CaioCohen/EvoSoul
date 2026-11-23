using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>Combines each static garden visual into one renderer while keeping
/// interactive plant state roots separate. Shared source layouts reuse a mesh.</summary>
public sealed class ProfaneGardenVisualBatcher : MonoBehaviour
{
    private readonly Dictionary<string, Mesh> cache = new();
    private readonly Dictionary<Material, Material> twoSidedLeaves = new();

    private void Awake()
    {
        Transform world = transform;
        Transform plants = world.Find("PurifiablePlants");
        if (plants != null)
            foreach (Transform plant in plants)
                foreach (Transform state in plant)
                    if (state.name == "Corrupted" || state.name == "Purified") Combine(state);
        Transform decor = world.Find("GardenDecoration");
        if (decor != null)
            foreach (Transform item in decor) Combine(item);
    }

    private void Combine(Transform root)
    {
        var filters = root.GetComponentsInChildren<MeshFilter>(true);
        var groups = new Dictionary<Material, List<CombineInstance>>();
        var originals = new List<MeshRenderer>();
        var key = new StringBuilder();
        foreach (var filter in filters)
        {
            var renderer = filter.GetComponent<MeshRenderer>();
            if (renderer == null || !renderer.enabled || filter.sharedMesh == null) continue;
            var mesh = filter.sharedMesh;
            var transformToRoot = root.worldToLocalMatrix * filter.transform.localToWorldMatrix;
            key.Append(mesh.GetEntityId().ToString()).Append(':');
            for (int i = 0; i < 16; i++) key.Append(Mathf.RoundToInt(transformToRoot[i] * 10000f)).Append(',');
            var materials = renderer.sharedMaterials;
            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                if (i >= materials.Length || materials[i] == null) return;
                Material material = materials[i];
                if (!groups.TryGetValue(material, out var list)) groups.Add(material, list = new List<CombineInstance>());
                list.Add(new CombineInstance { mesh = mesh, subMeshIndex = i, transform = transformToRoot });
                key.Append(material.GetEntityId().ToString()).Append('/').Append(i).Append(';');
            }
            originals.Add(renderer);
        }
        if (originals.Count < 2) return;
        string signature = key.ToString();
        if (!cache.TryGetValue(signature, out var combined))
        {
            var parts = new List<CombineInstance>();
            foreach (var group in groups.Values)
            {
                var part = new Mesh();
                part.CombineMeshes(group.ToArray(), true, true);
                parts.Add(new CombineInstance { mesh = part, transform = Matrix4x4.identity });
            }
            combined = new Mesh { name = "ProfaneGarden_Combined" };
            combined.CombineMeshes(parts.ToArray(), false, false);
            foreach (var part in parts) Destroy(part.mesh);
            cache.Add(signature, combined);
        }
        var child = new GameObject("CombinedVisual");
        child.transform.SetParent(root, false);
        child.AddComponent<MeshFilter>().sharedMesh = combined;
        var output = child.AddComponent<MeshRenderer>();
        var mats = new Material[groups.Count];
        groups.Keys.CopyTo(mats, 0);
        for (int i = 0; i < mats.Length; i++)
        {
            var original = mats[i];
            if (!original.name.Contains("Leaf") || !original.HasProperty("_Cull")) continue;
            if (!twoSidedLeaves.TryGetValue(original, out var doubleSided))
            {
                doubleSided = new Material(original) { name = original.name + " (two-sided)" };
                doubleSided.SetFloat("_Cull", (float)UnityEngine.Rendering.CullMode.Off);
                twoSidedLeaves.Add(original, doubleSided);
            }
            mats[i] = doubleSided;
        }
        output.sharedMaterials = mats;
        foreach (var renderer in originals) renderer.enabled = false;
    }
}
