using System;
using UnityEngine;

public static class VerifyExpandedStage
{
    public static object Run()
    {
        var fruits = UnityEngine.Object.FindObjectsByType<StageFruitTarget>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        var npcs = UnityEngine.Object.FindObjectsByType<StageDropletNpcTarget>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        var player = GameObject.Find("VirginDropPlayer");
        var controller = player != null ? player.GetComponent<CharacterController>() : null;
        if (controller == null)
            throw new InvalidOperationException("Player CharacterController is missing.");

        var before = player.transform.position;
        controller.Move(Vector3.right * 6f);
        var after = player.transform.position;
        var moved = Vector3.Distance(before, after);

        var groundedTargets = 0;
        foreach (var target in fruits)
            if (HasGroundBelow(target.transform.position)) groundedTargets++;
        foreach (var target in npcs)
            if (HasGroundBelow(target.transform.position)) groundedTargets++;

        var bounds = new Bounds(Vector3.zero, Vector3.zero);
        var hasBounds = false;
        foreach (var collider in UnityEngine.Object.FindObjectsByType<MeshCollider>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            if (!hasBounds)
            {
                bounds = collider.bounds;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(collider.bounds);
            }
        }

        return new
        {
            fruitTargets = fruits.Length,
            npcTargets = npcs.Length,
            groundedTargets,
            expectedGroundedTargets = fruits.Length + npcs.Length,
            playerTraversalDelta = moved,
            terrainSize = hasBounds ? new { x = bounds.size.x, z = bounds.size.z } : null
        };
    }

    private static bool HasGroundBelow(Vector3 position)
    {
        return Physics.Raycast(position + Vector3.up * 5f, Vector3.down, 10f, ~0, QueryTriggerInteraction.Ignore);
    }
}
