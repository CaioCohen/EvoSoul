using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class AddBercarioObstacleColliders
{
    public static object Run()
    {
        var world = GameObject.Find("BercarioWorld");
        if (world == null)
            throw new System.InvalidOperationException("BercarioWorld was not found in the active scene.");

        int created = 0;
        foreach (var filter in world.GetComponentsInChildren<MeshFilter>(true))
        {
            string name = filter.name;
            bool isObstacle = name.Contains("Tree_") && name.EndsWith("_Trunk")
                || name.Contains("FruitBush_") && name.EndsWith("_Base")
                || name.StartsWith("GEO-Rock_");
            if (!isObstacle || filter.GetComponent<Collider>() != null)
                continue;

            filter.gameObject.AddComponent<BoxCollider>();
            created++;
        }

        Scene scene = SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        return new { collidersCreated = created, scene = scene.path };
    }
}
