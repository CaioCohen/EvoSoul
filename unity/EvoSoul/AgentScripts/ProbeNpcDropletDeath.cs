using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public static class ProbeNpcDropletDeath
{
    public static async Task<object> Run()
    {
        var npc = UnityEngine.Object.FindObjectsByType<StageDropletNpcTarget>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
            .FirstOrDefault(target => target.LifecycleState == DropletNpcLifecycleState.Wandering);
        if (npc == null)
            throw new InvalidOperationException("No wandering NPC is available.");

        var visual = npc.transform.Find("VisualBody");
        var renderer = visual != null ? visual.GetComponent<Renderer>() : null;
        var meshFilter = visual != null ? visual.GetComponent<MeshFilter>() : null;
        if (visual == null || renderer == null || meshFilter == null || meshFilter.sharedMesh == null)
            throw new InvalidOperationException("The NPC does not have a renderable droplet visual.");

        bool reserved = npc.TryReserveInteraction();
        bool claimed = reserved && npc.TryClaimReward();
        bool started = claimed && npc.BeginDefeat(0f);
        await Task.Delay(1200);

        var puddle = npc.transform.Find("PuddleVisual");
        bool puddlePhase = npc.LifecycleState == DropletNpcLifecycleState.Puddle;
        bool puddleVisible = puddle != null && puddle.gameObject.activeSelf;
        bool visualHidden = !visual.gameObject.activeSelf;

        await Task.Delay(3000);
        bool removed = !npc.gameObject.activeSelf && npc.LifecycleState == DropletNpcLifecycleState.Removed;

        return new
        {
            status = reserved && claimed && started && puddlePhase && puddleVisible && visualHidden && removed ? "passed" : "failed",
            visualMesh = meshFilter.sharedMesh.name,
            materials = renderer.sharedMaterials.Where(material => material != null).Select(material => material.name).ToArray(),
            reserved,
            claimed,
            started,
            puddlePhase,
            puddleVisible,
            visualHidden,
            removed
        };
    }
}
