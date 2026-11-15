using System;
using System.Linq;
using System.Text;
using UnityEngine;

public static class DiagnoseBercarioRuntime
{
    public static object Run()
    {
        var player = GameObject.Find("VirginDropPlayer");
        if (player == null)
            throw new InvalidOperationException("VirginDropPlayer was not found in the active scene.");

        var controller = player.GetComponent<CharacterController>();
        var playerController = player.GetComponent<VirginDropPlayerController>();
        var camera = Camera.main;
        var npcs = UnityEngine.Object.FindObjectsByType<StageDropletNpcTarget>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        var report = new StringBuilder();
        report.AppendLine($"PLAYER {player.name} pos={player.transform.position} active={player.activeInHierarchy} cc={(controller != null && controller.enabled)} grounded={(controller != null && controller.isGrounded)} script={(playerController != null && playerController.enabled)}");
        report.AppendLine($"PLAYER_COMPONENTS {string.Join(",", player.GetComponents<Component>().Where(component => component != null).Select(component => component.GetType().Name))}");
        var playerRenderer = player.GetComponent<SkinnedMeshRenderer>();
        if (playerRenderer != null)
            report.AppendLine($"PLAYER_RENDERER mesh={playerRenderer.sharedMesh?.name} bones={playerRenderer.bones?.Length ?? 0} rootBone={playerRenderer.rootBone?.name} materials={string.Join(",", playerRenderer.sharedMaterials.Where(material => material != null).Select(material => material.name))}");
        foreach (var child in player.GetComponentsInChildren<Transform>(true))
            report.AppendLine($"PLAYER_CHILD {child.name} active={child.gameObject.activeInHierarchy} pos={child.position} renderers={child.GetComponents<Renderer>().Length}");
        report.AppendLine(camera == null ? "CAMERA null" : $"CAMERA {camera.name} pos={camera.transform.position} forward={camera.transform.forward}");
        report.AppendLine($"NPC_COUNT {npcs.Length}");
        foreach (var npc in npcs)
        {
            report.AppendLine($"NPC {npc.name} pos={npc.transform.position} state={npc.LifecycleState}");
            foreach (var child in npc.GetComponentsInChildren<Transform>(true))
                report.AppendLine($"NPC_CHILD {child.name} active={child.gameObject.activeInHierarchy} meshes={child.GetComponents<MeshFilter>().Length} renderers={child.GetComponents<Renderer>().Length}");
        }
        return report.ToString();
    }
}
