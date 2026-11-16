using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

var trunkRenderers = UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None)
    .Where(renderer => renderer.name.IndexOf("Tree", StringComparison.OrdinalIgnoreCase) >= 0
        && renderer.name.IndexOf("Trunk", StringComparison.OrdinalIgnoreCase) >= 0)
    .ToArray();

int added = 0;
int updated = 0;
foreach (var renderer in trunkRenderers)
{
    var tree = renderer.gameObject;
    var collider = tree.GetComponent<CapsuleCollider>();
    if (collider == null)
    {
        collider = Undo.AddComponent<CapsuleCollider>(tree);
        added++;
    }
    else
    {
        updated++;
    }

    var bounds = renderer.bounds;
    var transform = tree.transform;
    var scale = transform.lossyScale;
    float safeX = Mathf.Max(0.0001f, Mathf.Abs(scale.x));
    float safeY = Mathf.Max(0.0001f, Mathf.Abs(scale.y));
    float safeZ = Mathf.Max(0.0001f, Mathf.Abs(scale.z));
    collider.center = transform.InverseTransformPoint(bounds.center);
    collider.direction = 1;
    collider.height = Mathf.Max(0.2f, bounds.size.y / safeY);
    collider.radius = Mathf.Max(0.05f, Mathf.Min(bounds.size.x / safeX, bounds.size.z / safeZ) * 0.32f);
    collider.isTrigger = false;
    EditorUtility.SetDirty(tree);
}

var nonTreePropsWithColliders = UnityEngine.Object.FindObjectsByType<Collider>(FindObjectsInactive.Include, FindObjectsSortMode.None)
    .Count(collider => collider.GetComponentInParent<Renderer>() != null
        && collider.GetComponentInParent<Renderer>().name.IndexOf("Tree", StringComparison.OrdinalIgnoreCase) < 0
        && collider.GetComponentInParent<Renderer>().name.IndexOf("Trunk", StringComparison.OrdinalIgnoreCase) < 0);

EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());

return new
{
    scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path,
    trunks = trunkRenderers.Length,
    added,
    updated,
    nonTreePropsWithColliders
};
