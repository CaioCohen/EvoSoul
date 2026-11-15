using UnityEngine;
using UnityEngine.SceneManagement;

public static class Stage3PerformanceCheck
{
    public static object Run()
    {
        int meshCount = 0;
        int triangles = 0;
        foreach (var filter in Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Exclude))
        {
            if (filter.sharedMesh == null)
                continue;
            meshCount++;
            triangles += filter.sharedMesh.triangles.Length / 3;
        }

        int renderers = Object.FindObjectsByType<Renderer>(FindObjectsInactive.Exclude).Length;
        int lights = Object.FindObjectsByType<Light>(FindObjectsInactive.Exclude).Length;
        int colliders = Object.FindObjectsByType<Collider>(FindObjectsInactive.Exclude).Length;
        int targets = Object.FindObjectsByType<StageInteractionTarget>(FindObjectsInactive.Exclude).Length;
        return new { scene = SceneManager.GetActiveScene().path, meshCount, triangles, renderers, lights, colliders, targets };
    }
}
