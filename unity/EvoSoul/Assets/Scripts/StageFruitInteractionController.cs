using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Handles the Eat action only. Attack is intentionally left to the NPC step.
/// UI buttons may call RequestEat; E is available for Editor testing.
/// </summary>
public sealed class StageFruitInteractionController : MonoBehaviour
{
    [SerializeField] private StageInteractionSelector selector;
    [SerializeField] private StageEnergyLedger energyLedger;

    private bool eatQueued;

    private void Awake()
    {
        if (selector == null)
            selector = GetComponent<StageInteractionSelector>();
        if (energyLedger == null)
            energyLedger = FindAnyObjectByType<StageEnergyLedger>();
    }

    private void Update()
    {
        bool requested = eatQueued || ReadEditorEatPressed();
        eatQueued = false;
        if (requested)
            TryEatCurrentSelection();
    }

    /// <summary>Public hook for a future mobile Eat button.</summary>
    public void RequestEat()
    {
        eatQueued = true;
    }

    public bool TryEatCurrentSelection()
    {
        if (selector == null || energyLedger == null)
            return false;

        // Update first because Update ordering between input and the selector is
        // not guaranteed; this prevents consuming a target that just left range.
        selector.RefreshSelection();
        StageInteractionSelection selection = selector.CurrentSelection;
        if (selection.Action != StageInteractionAction.Eat || selection.Target is not StageFruitTarget fruit)
            return false;

        // Revalidate availability and claim an exclusive reservation before
        // changing either visual state or the stage's energy ledger.
        if (!fruit.IsAvailable || !fruit.TryReserveInteraction())
            return false;

        if (!energyLedger.TryAward(fruit.RewardSourceId, StageEnergyKind.Positive))
        {
            fruit.ReleaseInteractionReservation();
            selector.RefreshSelection();
            return false;
        }

        if (!fruit.TryClaimReward())
        {
            Debug.LogError($"Fruit '{fruit.name}' accepted energy after its reservation was lost.", fruit);
            selector.RefreshSelection();
            return false;
        }

        fruit.CompleteConsumption();
        selector.RefreshSelection();
        return true;
    }

    private static bool ReadEditorEatPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(KeyCode.E);
#else
        return false;
#endif
    }
}
