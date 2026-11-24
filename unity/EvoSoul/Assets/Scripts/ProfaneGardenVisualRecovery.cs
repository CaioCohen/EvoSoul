using UnityEngine;

/// <summary>
/// Defensive Stage 02 startup repair for legacy batched FBX prop meshes.
/// If a transition loads a zero-bounds CombinedVisual, restore the original
/// low-poly renderers instead of leaving the garden visually empty.
/// </summary>
[DefaultExecutionOrder(100)]
public sealed class ProfaneGardenVisualRecovery : MonoBehaviour
{
    private void Awake()
    {
        foreach (var combined in GetComponentsInChildren<MeshRenderer>(true))
        {
            if (combined.gameObject.name != "CombinedVisual") continue;
            var filter = combined.GetComponent<MeshFilter>();
            if (filter == null || filter.sharedMesh == null || filter.sharedMesh.bounds.size.sqrMagnitude > 0.000001f)
                continue;

            combined.enabled = false;
            foreach (var source in combined.transform.parent.GetComponentsInChildren<MeshRenderer>(true))
                if (source != combined) source.enabled = true;
        }
    }
}
