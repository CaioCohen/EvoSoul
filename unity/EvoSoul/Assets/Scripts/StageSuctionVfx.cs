using UnityEngine;

/// <summary>Short source-to-mouth energy wisps, without textures or colliders.</summary>
public sealed class StageSuctionVfx : MonoBehaviour
{
    private Transform source, creature;
    private LineRenderer[] strands;
    private Material material;
    private float elapsed, duration;
    public void Configure(Transform target, Transform mouthOwner, Color color, float seconds)
    {
        source = target; creature = mouthOwner; duration = seconds;
        var shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default");
        material = new Material(shader); material.color = color;
        strands = new LineRenderer[3];
        for (int i = 0; i < strands.Length; i++)
        {
            var obj = new GameObject("Wisp_" + i); obj.transform.SetParent(transform, false);
            var line = obj.AddComponent<LineRenderer>(); line.sharedMaterial = material;
            line.useWorldSpace = true; line.positionCount = 16; line.numCapVertices = 2;
            // Readable from the third-person camera while retaining a tapered
            // mouth end. The three strands remain separate and lightweight.
            line.startWidth = .30f; line.endWidth = .065f;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; line.receiveShadows = false;
            strands[i] = line;
        }
    }
    private void Update()
    {
        if (source == null || creature == null || strands == null) { Destroy(gameObject); return; }
        elapsed += Time.deltaTime;
        Vector3 start = source.position + Vector3.up * .7f;
        Vector3 end = creature.position + Vector3.up * .48f + creature.forward * .42f;
        Vector3 direction = end - start;
        Vector3 side = Vector3.Cross(direction.normalized, Vector3.up).normalized;
        for (int s = 0; s < strands.Length; s++)
        for (int i = 0; i < 16; i++)
        {
            float t = i / 15f, phase = t * 9f + elapsed * 14f + s * 2.1f;
            float radius = (1f - t) * .30f;
            strands[s].SetPosition(i, Vector3.Lerp(start, end, t) + side * (Mathf.Sin(phase) * radius) + Vector3.up * (Mathf.Cos(phase) * radius));
        }
        if (elapsed >= duration) Destroy(gameObject);
    }
    private void OnDestroy() { if (material != null) Destroy(material); }
}
