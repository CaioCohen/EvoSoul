using UnityEngine;

/// <summary>Small material-color highlight for the target chosen by the selector.</summary>
public sealed class StageTargetHighlight : MonoBehaviour
{
    [SerializeField] private Renderer[] targetRenderers;
    [SerializeField] private Color selectedColor = new(1f, 0.88f, 0.2f, 1f);

    private MaterialPropertyBlock propertyBlock;
    private bool selected;

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
        if (selected == value)
            return;
        selected = value;
        foreach (Renderer targetRenderer in targetRenderers)
        {
            if (targetRenderer == null)
                continue;
            targetRenderer.GetPropertyBlock(propertyBlock);
            if (selected)
            {
                propertyBlock.SetColor("_BaseColor", selectedColor);
                propertyBlock.SetColor("_Color", selectedColor);
            }
            else
            {
                propertyBlock.Clear();
            }
            targetRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}
