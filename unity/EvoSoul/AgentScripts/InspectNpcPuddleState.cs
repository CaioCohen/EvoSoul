using UnityEngine;

StageDropletNpcTarget npc = null;
foreach (var candidate in Resources.FindObjectsOfTypeAll<StageDropletNpcTarget>())
{
    if (candidate.name == "ValidationDeathNPC")
    {
        npc = candidate;
        break;
    }
}

if (npc == null)
    return new { found = false };

var renderers = npc.GetComponentsInChildren<Renderer>(true);
var activeRenderers = 0;
foreach (var renderer in renderers)
{
    if (renderer.gameObject.activeInHierarchy)
        activeRenderers++;
}

return new
{
    found = true,
    state = npc.LifecycleState.ToString(),
    active = npc.gameObject.activeInHierarchy,
    activeRenderers
};
