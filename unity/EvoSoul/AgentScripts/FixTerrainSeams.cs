using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

var world = GameObject.Find("BercarioWorld");
var sourceTerrain = world.GetComponentsInChildren<MeshRenderer>(true)
    .First(renderer => renderer.gameObject.name == "GEO-ENV-Terrain");
var grassMaterial = sourceTerrain.sharedMaterial;

var existing = world.transform.Find("TerrainSeamCovers");
if (existing != null)
    Object.DestroyImmediate(existing.gameObject);

var covers = new GameObject("TerrainSeamCovers");
covers.transform.SetParent(world.transform, false);

void AddCover(string name, Vector3 position, Vector3 scale)
{
    var cover = GameObject.CreatePrimitive(PrimitiveType.Cube);
    cover.name = name;
    cover.transform.SetParent(covers.transform, false);
    cover.transform.position = position;
    cover.transform.localScale = scale;
    Object.DestroyImmediate(cover.GetComponent<Collider>());
    var renderer = cover.GetComponent<MeshRenderer>();
    renderer.sharedMaterial = grassMaterial;
    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    renderer.receiveShadows = true;
}

// Tiles cover 32x24 metres at centres -32/0/32 and -24/0/24.
// The strips overlap their edges to prevent the ocean plane leaking through.
// Terrain peaks at y=0.26; place covers just above it so the water cannot show through.
AddCover("TerrainSeam_X_Negative", new Vector3(-16f, 0.285f, 0f), new Vector3(0.40f, 0.02f, 72.40f));
AddCover("TerrainSeam_X_Positive", new Vector3(16f, 0.285f, 0f), new Vector3(0.40f, 0.02f, 72.40f));
AddCover("TerrainSeam_Z_Negative", new Vector3(0f, 0.285f, -12f), new Vector3(96.40f, 0.02f, 0.40f));
AddCover("TerrainSeam_Z_Positive", new Vector3(0f, 0.285f, 12f), new Vector3(96.40f, 0.02f, 0.40f));

var allTargets = Object.FindObjectsByType<StageInteractionTarget>(FindObjectsInactive.Include);
var targetsMissingCollider = allTargets.Count(target => target.GetComponentsInChildren<Collider>(true).Length == 0);

EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
return new
{
    covers = covers.transform.childCount,
    targetCount = allTargets.Length,
    targetsMissingCollider,
    material = grassMaterial != null ? grassMaterial.name : "missing"
};
