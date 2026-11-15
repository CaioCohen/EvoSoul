using System;
using System.Threading.Tasks;
using UnityEngine;

public static class ProbePlayerMovement
{
    public static async Task<object> Run()
    {
        var player = GameObject.Find("VirginDropPlayer");
        var playerController = player != null ? player.GetComponent<VirginDropPlayerController>() : null;
        var characterController = player != null ? player.GetComponent<CharacterController>() : null;
        if (player == null || playerController == null || characterController == null)
            throw new InvalidOperationException("Player controller unavailable.");

        var before = player.transform.position;
        var flags = characterController.Move(Vector3.forward * 1f);
        var directAfter = player.transform.position;
        player.transform.position = before;
        playerController.SetTouchMoveInput(Vector2.up);
        await Task.Delay(1000);
        playerController.SetTouchMoveInput(Vector2.zero);
        await Task.Delay(100);
        var after = player.transform.position;
        return $"before={before}; directAfter={directAfter}; directFlags={flags}; after={after}; delta={after - before}; ccEnabled={characterController.enabled}; grounded={characterController.isGrounded}";
    }
}
