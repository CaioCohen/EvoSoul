using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Opt in the nursery at runtime without changing its scene asset.</summary>
public static class NurseryEvolutionBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Register()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        Install(SceneManager.GetActiveScene());
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => Install(scene);

    private static void Install(Scene scene)
    {
        if (scene.name != "BercarioStage01" || !scene.isLoaded ||
            Object.FindAnyObjectByType<StageEvolutionSequence>() != null) return;
        var ledger = Object.FindAnyObjectByType<StageEnergyLedger>();
        var player = Object.FindAnyObjectByType<VirginDropPlayerController>();
        var camera = Camera.main;
        var catalog = Resources.Load<StageEvolutionCatalog>("NurseryEvolutionCatalog");
        if (ledger == null || player == null || camera == null || catalog == null)
        {
            Debug.LogError("Nursery evolution: required scene components or catalog are missing.");
            return;
        }

        var host = new GameObject("NurseryEvolutionRuntime");
        var portalObject = new GameObject("NurseryEvolutionPortal");
        var origin = Vector3.zero;
        if (Physics.Raycast(new Vector3(0f, 30f, 0f), Vector3.down, out var hit, 60f))
            origin.y = hit.point.y;
        portalObject.transform.position = origin;
        var portal = portalObject.AddComponent<StageEvolutionPortal>();
        portal.Configure(player.transform, CreatePortalVisual(portalObject.transform));
        var controls = new List<Behaviour>();
        controls.AddRange(Object.FindObjectsByType<StageNpcInteractionController>(FindObjectsInactive.Exclude, FindObjectsSortMode.None));
        controls.AddRange(Object.FindObjectsByType<StageFruitInteractionController>(FindObjectsInactive.Exclude, FindObjectsSortMode.None));
        controls.AddRange(Object.FindObjectsByType<StageInteractionSelector>(FindObjectsInactive.Exclude, FindObjectsSortMode.None));
        controls.AddRange(Object.FindObjectsByType<MobileJoystick>(FindObjectsInactive.Exclude, FindObjectsSortMode.None));
        controls.AddRange(Object.FindObjectsByType<MobileAttackButton>(FindObjectsInactive.Exclude, FindObjectsSortMode.None));
        controls.AddRange(Object.FindObjectsByType<StageInteractionPromptView>(FindObjectsInactive.Exclude, FindObjectsSortMode.None));
        var sequence = host.AddComponent<StageEvolutionSequence>();
        sequence.Configure(ledger, player, player.transform, camera,
            camera.GetComponent<ThirdPersonFollowCamera>(), controls.ToArray(), catalog.forms, portal);
    }

    private static GameObject CreatePortalVisual(Transform parent)
    {
        const int segments = 48;
        var visual = new GameObject("PortalGlow", typeof(MeshFilter), typeof(MeshRenderer));
        visual.transform.SetParent(parent, false);
        visual.transform.localPosition = Vector3.up * .025f;
        var vertices = new Vector3[segments * 2];
        var triangles = new int[segments * 6];
        for (int i = 0; i < segments; i++)
        {
            float angle = Mathf.PI * 2f * i / segments;
            var dir = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
            vertices[i * 2] = dir * 1.10f;
            vertices[i * 2 + 1] = dir * .94f;
            int next = (i + 1) % segments;
            int j = i * 6;
            triangles[j] = i * 2; triangles[j + 1] = i * 2 + 1; triangles[j + 2] = next * 2;
            triangles[j + 3] = next * 2; triangles[j + 4] = i * 2 + 1; triangles[j + 5] = next * 2 + 1;
        }
        var mesh = new Mesh { name = "Evolution portal ring" };
        mesh.vertices = vertices; mesh.triangles = triangles; mesh.RecalculateNormals();
        visual.GetComponent<MeshFilter>().sharedMesh = mesh;
        var shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");
        var material = new Material(shader) { color = new Color(1f, .55f, .06f, 1f) };
        visual.GetComponent<MeshRenderer>().sharedMaterial = material;
        return visual;
    }
}
