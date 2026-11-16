using UnityEngine;

/// <summary>
/// Compatibility component retained on authored targets. Selection is now
/// expressed by the single arrow indicator, never by tinting target materials.
/// </summary>
public sealed class StageTargetHighlight : MonoBehaviour
{
    [SerializeField] private Renderer[] targetRenderers;
    private MaterialPropertyBlock propertyBlock;

    public void ConfigureRenderers(Renderer[] renderers)
    {
        targetRenderers = renderers;
    }

    private void Awake()
    {
        if (targetRenderers == null || targetRenderers.Length == 0)
            targetRenderers = GetComponentsInChildren<Renderer>(true);
        propertyBlock = new MaterialPropertyBlock();
    }

    public void SetSelected(bool value)
    {
        foreach (Renderer targetRenderer in targetRenderers)
        {
            if (targetRenderer == null)
                continue;
            propertyBlock.Clear();
            targetRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}
