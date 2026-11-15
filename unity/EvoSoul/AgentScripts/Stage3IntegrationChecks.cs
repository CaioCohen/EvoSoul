using System;
using System.Threading.Tasks;
using UnityEngine;

public static class Stage3IntegrationChecks
{
    public static async Task<object> Run()
    {
        var player = UnityEngine.Object.FindAnyObjectByType<VirginDropPlayerController>();
        var selector = UnityEngine.Object.FindAnyObjectByType<StageInteractionSelector>();
        var fruitController = UnityEngine.Object.FindAnyObjectByType<StageFruitInteractionController>();
        var npcController = UnityEngine.Object.FindAnyObjectByType<StageNpcInteractionController>();
        var prompt = GameObject.Find("InteractionPrompt");
        var fruit = UnityEngine.Object.FindAnyObjectByType<StageFruitTarget>();
        var npc = UnityEngine.Object.FindAnyObjectByType<StageDropletNpcTarget>();

        Require(player != null && selector != null && fruitController != null && npcController != null, "Player interaction stack is incomplete.");
        Require(fruit != null && npc != null, "Scene targets are missing.");

        AimAt(player.transform, fruit.transform.position);
        selector.RefreshSelection();
        await Task.Delay(100);
        bool fruitPrompt = prompt != null && prompt.activeInHierarchy && selector.CurrentAction == StageInteractionAction.Eat;
        bool fruitConsumed = fruitController.TryEatCurrentSelection();
        bool fruitHidden = fruit.LifecycleState == FruitLifecycleState.Consumed && !fruit.IsAvailable;

        AimAt(player.transform, npc.transform.position);
        selector.RefreshSelection();
        await Task.Delay(100);
        bool npcPrompt = prompt != null && prompt.activeInHierarchy && selector.CurrentAction == StageInteractionAction.Attack;
        bool npcStarted = npcController.TryAttackCurrentSelection();
        await Task.Delay(500);
        bool puddleVisible = npc.LifecycleState == DropletNpcLifecycleState.Puddle && npc.transform.Find("PuddleVisual")?.gameObject.activeSelf == true;
        await Task.Delay(3000);
        bool npcRemoved = !npc.gameObject.activeSelf && npc.LifecycleState == DropletNpcLifecycleState.Removed;

        return new
        {
            status = fruitPrompt && fruitConsumed && fruitHidden && npcPrompt && npcStarted && puddleVisible && npcRemoved ? "passed" : "failed",
            fruitPrompt,
            fruitConsumed,
            fruitHidden,
            npcPrompt,
            npcStarted,
            puddleVisible,
            npcRemoved
        };
    }

    private static void AimAt(Transform player, Vector3 target)
    {
        player.position = target + Vector3.back;
        Vector3 direction = target - player.position;
        direction.y = 0f;
        player.rotation = Quaternion.LookRotation(direction);
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
    }
}
