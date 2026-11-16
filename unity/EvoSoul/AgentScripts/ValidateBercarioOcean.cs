using System;
using UnityEditor;
using UnityEngine;

public static class ValidateBercarioOcean
{
    public static object Main()
    {
        GameObject ocean = GameObject.Find("OceanSurface");
        if (ocean == null)
            throw new InvalidOperationException("OceanSurface is missing.");

        Renderer renderer = ocean.GetComponent<Renderer>();
        if (renderer == null || renderer.sharedMaterial == null)
            throw new InvalidOperationException("Ocean material is missing.");

        string shaderAsset = AssetDatabase.GetAssetPath(renderer.sharedMaterial.shader);
        string[] boundaries = { "NorthBoundary", "SouthBoundary", "EastBoundary", "WestBoundary" };
        foreach (string boundaryName in boundaries)
        {
            BoxCollider boundary = GameObject.Find(boundaryName)?.GetComponent<BoxCollider>();
            if (boundary == null || !boundary.enabled || !boundary.gameObject.activeInHierarchy)
                throw new InvalidOperationException("Inactive or missing boundary: " + boundaryName);
        }

        GameObject player = GameObject.Find("VirginDropPlayer");
        CharacterController controller = player != null ? player.GetComponent<CharacterController>() : null;
        if (controller == null)
            throw new InvalidOperationException("Player CharacterController is missing.");

        Vector3 original = player.transform.position;
        player.transform.position = new Vector3(46f, original.y, 0f);
        Physics.SyncTransforms();
        controller.Move(Vector3.right * 10f);
        float stoppedAtX = player.transform.position.x;
        player.transform.position = original;
        Physics.SyncTransforms();
        if (stoppedAtX > 48f)
            throw new InvalidOperationException("Player crossed EastBoundary: " + stoppedAtX);

        return new
        {
            shaderGraph = shaderAsset,
            shader = renderer.sharedMaterial.shader.name,
            oceanColliderPresent = ocean.GetComponent<Collider>() != null,
            stoppedAtX,
            boundaryCollisionVerified = true
        };
    }
}
