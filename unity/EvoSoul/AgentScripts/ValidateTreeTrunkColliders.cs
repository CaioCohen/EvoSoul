using System;
using System.Linq;
using UnityEngine;

var trunks = UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Exclude)
    .Where(renderer => renderer.name.IndexOf("Tree", StringComparison.OrdinalIgnoreCase) >= 0
        && renderer.name.IndexOf("Trunk", StringComparison.OrdinalIgnoreCase) >= 0)
    .ToArray();
var trunkCapsules = trunks.Count(renderer => renderer.GetComponent<CapsuleCollider>() != null);
var intangibleProps = UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Exclude)
    .Where(renderer => renderer.name.IndexOf("Bush", StringComparison.OrdinalIgnoreCase) >= 0
        || renderer.name.IndexOf("Plant", StringComparison.OrdinalIgnoreCase) >= 0
        || renderer.name.IndexOf("Rock", StringComparison.OrdinalIgnoreCase) >= 0)
    .ToArray();
var propsWithOwnCollider = intangibleProps.Count(renderer => renderer.GetComponent<Collider>() != null);

var player = UnityEngine.Object.FindFirstObjectByType<VirginDropPlayerController>();
if (player == null) throw new InvalidOperationException("Player controller was not found in Play Mode.");
var controller = player.GetComponent<CharacterController>();
if (controller == null) throw new InvalidOperationException("Player CharacterController was not found.");
if (trunks.Length == 0) throw new InvalidOperationException("No tree trunks found.");

var firstBounds = trunks[0].bounds;
bool wasEnabled = controller.enabled;
controller.enabled = false;
player.transform.position = new Vector3(
    firstBounds.center.x - firstBounds.extents.x - controller.radius - 0.1f,
    firstBounds.min.y + controller.height * 0.5f,
    firstBounds.center.z);
controller.enabled = wasEnabled;
controller.Move(Vector3.right * 10f);
float blockedX = player.transform.position.x;
bool blockedByTrunk = blockedX < firstBounds.center.x;
Vector3 beforeAround = player.transform.position;
controller.Move(Vector3.forward * 2f);
bool canGoAround = Vector3.Distance(beforeAround, player.transform.position) > 0.5f;

return new {
    trunks = trunks.Length,
    trunkCapsules,
    intangibleProps = intangibleProps.Length,
    propsWithOwnCollider,
    blockedByTrunk,
    canGoAround,
    playerPosition = player.transform.position
};
