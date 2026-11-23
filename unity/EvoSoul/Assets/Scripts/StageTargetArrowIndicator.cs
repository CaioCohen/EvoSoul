using UnityEngine;

/// <summary>
/// One lightweight world-space arrow that is parented to the selector's
/// current target. Parenting keeps the indicator with a fruit or droplet when
/// that target is repositioned in the scene; the late update only maintains
/// its height above animated renderer bounds.
/// </summary>
public sealed class StageTargetArrowIndicator : MonoBehaviour
{
    [SerializeField] private StageInteractionSelector selector;
    [SerializeField, Min(0.05f)] private float heightOffset = 0.35f;
    [SerializeField, Min(0f)] private float bobAmplitude = 0.08f;
    [SerializeField, Min(0f)] private float bobFrequency = 3f;
    [SerializeField] private Color arrowColor = new(1f, 1f, 1f, 1f);

    private GameObject arrowRoot;
    private StageInteractionTarget currentTarget;
    private Renderer[] currentRenderers;
    private float initialY;

    private void Awake()
    {
        if (selector == null)
            selector = GetComponent<StageInteractionSelector>();
        CreateArrow();
    }

    private void OnEnable()
    {
        if (selector == null)
            return;

        selector.SelectionChanged += SetSelection;
        SetSelection(selector.CurrentSelection);
    }

    private void OnDisable()
    {
        if (selector != null)
            selector.SelectionChanged -= SetSelection;
        if (arrowRoot != null)
            arrowRoot.SetActive(false);
    }

    private void LateUpdate()
    {
        if (currentTarget == null || !currentTarget.IsAvailable)
        {
            HideArrow();
            return;
        }

        Vector3 anchor = currentTarget.transform.position;
        float top = float.NegativeInfinity;
        foreach (Renderer renderer in currentRenderers)
        {
            if (renderer != null && renderer.enabled && renderer.gameObject.activeInHierarchy)
                top = Mathf.Max(top, renderer.bounds.max.y);
        }

        if (float.IsNegativeInfinity(top))
            top = anchor.y;

        float bob = Mathf.Sin((Time.time - initialY) * bobFrequency) * bobAmplitude;
        arrowRoot.transform.position = new Vector3(anchor.x, top + heightOffset + bob, anchor.z);
        arrowRoot.transform.rotation = Quaternion.identity;
    }

    private void SetSelection(StageInteractionSelection selection)
    {
        currentTarget = selection.Target as StageInteractionTarget;
        if (currentTarget == null || !selection.IsValid || !currentTarget.IsAvailable)
        {
            HideArrow();
            return;
        }

        // Fruit colliders are intentionally separate from their visual branch.
        // Read their actual mesh bounds so a repositioned fruit cannot leave
        // the selection arrow behind at the collider's old height.
        currentRenderers = currentTarget is StageFruitTarget fruit
            ? fruit.GetIndicatorRenderers()
            : currentTarget.GetComponentsInChildren<Renderer>(true);
        initialY = Time.time;
        // Keep the indicator in the target's hierarchy while it is visible.
        // World coordinates are retained here because LateUpdate computes the
        // exact vertical anchor from the target's current renderer bounds.
        arrowRoot.transform.SetParent(currentTarget.transform, true);
        arrowRoot.SetActive(true);
    }

    private void HideArrow()
    {
        currentTarget = null;
        currentRenderers = null;
        if (arrowRoot != null)
        {
            arrowRoot.SetActive(false);
            // A consumed/removed target can later be disabled. Return the
            // pooled indicator to its stable host rather than leaving it in a
            // disabled target subtree between selections.
            arrowRoot.transform.SetParent(transform, true);
        }
    }

    private void CreateArrow()
    {
        if (arrowRoot != null)
            return;

        arrowRoot = new GameObject("StageTargetArrow");
        arrowRoot.transform.SetParent(transform, false);
        arrowRoot.layer = gameObject.layer;

        Material material = new(Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default"));
        material.color = arrowColor;

        GameObject shaft = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        shaft.name = "Shaft";
        shaft.transform.SetParent(arrowRoot.transform, false);
        shaft.transform.localPosition = new Vector3(0f, 0.12f, 0f);
        shaft.transform.localScale = new Vector3(0.045f, 0.16f, 0.045f);
        Destroy(shaft.GetComponent<Collider>());
        shaft.GetComponent<Renderer>().sharedMaterial = material;

        GameObject head = new("Head");
        head.transform.SetParent(arrowRoot.transform, false);
        head.transform.localPosition = new Vector3(0f, -0.12f, 0f);
        MeshFilter filter = head.AddComponent<MeshFilter>();
        filter.sharedMesh = CreateDownwardPyramid();
        MeshRenderer renderer = head.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = material;

        arrowRoot.SetActive(false);
    }

    private static Mesh CreateDownwardPyramid()
    {
        Mesh mesh = new() { name = "StageTargetArrowHead" };
        mesh.vertices = new[]
        {
            new Vector3(-0.15f, 0.12f, -0.15f), new Vector3(0.15f, 0.12f, -0.15f),
            new Vector3(0.15f, 0.12f, 0.15f), new Vector3(-0.15f, 0.12f, 0.15f),
            new Vector3(0f, -0.20f, 0f)
        };
        mesh.triangles = new[] { 0, 1, 2, 0, 2, 3, 0, 4, 1, 1, 4, 2, 2, 4, 3, 3, 4, 0 };
        mesh.RecalculateNormals();
        return mesh;
    }
}
