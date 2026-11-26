using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Small animated flame attached to each burning Draconic bush.</summary>
public sealed class DraconicBushFire : MonoBehaviour
{
    private Transform[] flames;
    private Material[] materials;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Install()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        Attach(SceneManager.GetActiveScene());
    }
    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => Attach(scene);
    private static void Attach(Scene scene)
    {
        if (scene.name != "DraconicValeStage02") return;
        foreach (var plant in FindObjectsByType<StagePurifiablePlant>(FindObjectsInactive.Include))
        {
            var visual = plant.CorruptedVisual;
            if (visual == null || visual.GetComponent<DraconicBushFire>() != null) continue;
            // Keep the healthy plant silhouette under the burning state; the
            // orange placeholder is hidden rather than shown beneath the fire.
            if (plant.PurifiedVisual != null)
            {
                ReplaceBushVisual(visual.transform);
                ReplaceBushVisual(plant.PurifiedVisual.transform);
            }
            visual.AddComponent<DraconicBushFire>();
        }
    }
    private static void ReplaceBushVisual(Transform original)
    {
        foreach (var renderer in original.GetComponentsInChildren<Renderer>(true)) renderer.enabled = false;
        // FBX roots carry a 100x scale and axis conversion. Build under the
        // unscaled gameplay holder and reparent with world transforms intact.
        var root = new GameObject("AlignedBushVisual").transform;
        root.SetParent(original.parent, false);
        root.SetParent(original, true);
        var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        var leaf = new Material(shader) { color = new Color(.18f, .55f, .12f) };
        var bark = new Material(shader) { color = new Color(.27f, .14f, .06f) };
        var stem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        stem.name = "Stem"; stem.transform.SetParent(root, false); stem.transform.localPosition = new Vector3(0,.32f,0); stem.transform.localScale = new Vector3(.16f,.32f,.16f);
        Destroy(stem.GetComponent<Collider>()); stem.GetComponent<Renderer>().material = bark;
        for (int i = 0; i < 6; i++)
        {
            float a = i * Mathf.PI * 2f / 6f;
            var foliage = GameObject.CreatePrimitive(PrimitiveType.Sphere); foliage.name = "Foliage";
            foliage.transform.SetParent(root, false); foliage.transform.localPosition = new Vector3(Mathf.Cos(a)*.24f,.64f,Mathf.Sin(a)*.24f);
            foliage.transform.localScale = new Vector3(.65f,.57f,.65f);
            Destroy(foliage.GetComponent<Collider>()); foliage.GetComponent<Renderer>().sharedMaterial = leaf;
        }
    }
    private void Awake()
    {
        var shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");
        if (shader == null) return;
        materials = new Material[3];
        var colors = new[] { new Color(1f,.2f,.01f), new Color(1f,.48f,.015f), new Color(1f,.83f,.15f) };
        for (int i = 0; i < materials.Length; i++) materials[i] = new Material(shader) { color = colors[i] };
        // The reusable fire is parented under the initial state: normal state
        // toggling extinguishes it and scheduled reactivation restores it.
        var root = new GameObject("BushFireVFX").transform;
        root.SetParent(transform.parent, false);
        root.position = transform.parent.position + Vector3.up * .55f;
        root.SetParent(transform, true);
        flames = new Transform[5];
        for (int i = 0; i < flames.Length; i++)
        {
            var flame = new GameObject("Flame_" + i, typeof(MeshFilter), typeof(MeshRenderer));
            flame.transform.SetParent(root, false);
            flame.transform.localPosition = new Vector3((i - 2) * .12f, 0f, (i % 2) * .10f);
            flame.transform.localRotation = Quaternion.Euler(0f, i * 72f, 0f);
            var mesh = new Mesh { name = "Stylized flame" };
            mesh.vertices = new[] { new Vector3(-.18f,0,0), new Vector3(.18f,0,0), new Vector3(.13f,.32f,0), new Vector3(0,.75f,0), new Vector3(-.14f,.30f,0) };
            mesh.triangles = new[] { 0,1,2, 0,2,4, 4,2,3, 2,1,0, 4,2,0, 3,2,4 };
            mesh.RecalculateBounds(); mesh.RecalculateNormals();
            flame.GetComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = flame.GetComponent<MeshRenderer>(); renderer.sharedMaterial = materials[i % materials.Length];
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; renderer.receiveShadows = false;
            flames[i] = flame.transform;
        }
    }
    private void Update()
    {
        if (flames == null) return;
        for (int i = 0; i < flames.Length; i++)
            flames[i].localScale = new Vector3(1f, .85f + .15f * Mathf.Sin(Time.time * 6f + i), 1f);
    }
    private void OnDestroy()
    {
        if (flames != null) foreach (var flame in flames) if (flame != null) Destroy(flame.GetComponent<MeshFilter>().sharedMesh);
        if (materials != null) foreach (var material in materials) if (material != null) Destroy(material);
    }
}
