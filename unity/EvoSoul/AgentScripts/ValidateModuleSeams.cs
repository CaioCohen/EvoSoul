using System;
using System.Linq;
using UnityEngine;

public static class ValidateModuleSeams
{
    public static object Run()
    {
        var fruits = UnityEngine.Object.FindObjectsByType<StageFruitTarget>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        var npcs = UnityEngine.Object.FindObjectsByType<StageDropletNpcTarget>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        var player = GameObject.Find("VirginDropPlayer");
        var selector = player != null ? player.GetComponent<StageInteractionSelector>() : null;
        if (player == null || selector == null)
            throw new InvalidOperationException("Player selector was not found.");

        bool GroundAt(float x, float z)
        {
            return Physics.Raycast(new Vector3(x, 20f, z), Vector3.down, out _, 40f, ~0, QueryTriggerInteraction.Ignore);
        }

        bool SelectTarget(Component target)
        {
            var flat = target.transform.position - player.transform.position;
            flat.y = 0f;
            if (flat.sqrMagnitude < 0.01f)
                flat = Vector3.forward;
            player.transform.position = target.transform.position - flat.normalized * 1.1f;
            var look = target.transform.position - player.transform.position;
            look.y = 0f;
            player.transform.rotation = Quaternion.LookRotation(look.normalized, Vector3.up);
            selector.RefreshSelection();
            return ReferenceEquals(selector.CurrentTarget, target);
        }

        var remoteFruit = fruits.OrderByDescending(item => Mathf.Abs(item.transform.position.x) + Mathf.Abs(item.transform.position.z)).FirstOrDefault();
        var remoteNpc = npcs.OrderByDescending(item => Mathf.Abs(item.transform.position.x) + Mathf.Abs(item.transform.position.z)).FirstOrDefault();
        bool fruitSelected = remoteFruit != null && remoteFruit.GetComponent<Collider>() != null && SelectTarget(remoteFruit);
        bool npcSelected = remoteNpc != null && remoteNpc.GetComponent<Collider>() != null && SelectTarget(remoteNpc);

        return new
        {
            fruits = fruits.Length,
            npcs = npcs.Length,
            seamGround = new[] { GroundAt(0f, 0f), GroundAt(32f, 0f), GroundAt(-32f, 0f), GroundAt(0f, 24f), GroundAt(0f, -24f) },
            remoteFruit = remoteFruit != null ? remoteFruit.name : null,
            remoteNpc = remoteNpc != null ? remoteNpc.name : null,
            fruitSelected,
            npcSelected
        };
    }
}
