using UnityEngine;

/// <summary>
/// Keeps the low-cost ocean Shader Graph material moving by scrolling its
/// repeated base texture. The water has no collider and never participates in
/// gameplay physics.
/// </summary>
[RequireComponent(typeof(Renderer))]
public sealed class OceanSurfaceMotion : MonoBehaviour
{
    [SerializeField] private Vector2 scrollSpeed = new(0.007f, 0.012f);
    [SerializeField] private Vector2 textureScale = new(48f, 48f);

    private Material runtimeMaterial;
    private static readonly int BaseMap = Shader.PropertyToID("_BaseMap");

    private void Awake()
    {
        Renderer oceanRenderer = GetComponent<Renderer>();
        runtimeMaterial = oceanRenderer != null ? oceanRenderer.material : null;
        if (runtimeMaterial != null && runtimeMaterial.HasProperty(BaseMap))
            runtimeMaterial.SetTextureScale(BaseMap, textureScale);
    }

    private void Update()
    {
        if (runtimeMaterial == null || !runtimeMaterial.HasProperty(BaseMap))
            return;

        runtimeMaterial.SetTextureOffset(BaseMap, Time.time * scrollSpeed);
    }
}
