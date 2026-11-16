using System;
using UnityEngine;

var player = GameObject.Find("VirginDropPlayer");
var controller = player.GetComponent<CharacterController>();
var selector = player.GetComponent<StageInteractionSelector>();
var fruitController = player.GetComponent<StageFruitInteractionController>();
var npcController = player.GetComponent<StageNpcInteractionController>();

void PlaceFacing(Transform target)
{
    var position = target.position - Vector3.forward * 1.15f;
    controller.enabled = false;
    player.transform.position = position;
    player.transform.rotation = Quaternion.LookRotation(target.position - position);
    controller.enabled = true;
}

// Crossing one former internal tile boundary with the actual CharacterController.
controller.enabled = false;
player.transform.position = new Vector3(-16.8f, 1.0f, 0f);
controller.enabled = true;
controller.Move(Vector3.right * 2.0f);
var crossedSeam = player.transform.position.x > -15.1f;

var remoteFruit = GameObject.Find("FruitTarget_06").GetComponent<StageFruitTarget>();
PlaceFacing(remoteFruit.transform);
selector.RefreshSelection();
var fruitSelected = selector.CurrentTarget == remoteFruit && selector.CurrentAction == StageInteractionAction.Eat;
var ateRemoteFruit = fruitSelected && fruitController.TryEatCurrentSelection();

var remoteNpc = GameObject.Find("DropletNpc_06").GetComponent<StageDropletNpcTarget>();
PlaceFacing(remoteNpc.transform);
selector.RefreshSelection();
var npcSelected = selector.CurrentTarget == remoteNpc && selector.CurrentAction == StageInteractionAction.Attack;
var attackedRemoteNpc = npcSelected && npcController.TryAttackCurrentSelection();

return new {
    crossedSeam,
    playerXAfterMove = player.transform.position.x,
    fruitSelected,
    ateRemoteFruit,
    fruitActiveAfter = remoteFruit.gameObject.activeInHierarchy,
    npcSelected,
    attackedRemoteNpc,
    npcAvailableAfter = remoteNpc.IsAvailable
};
