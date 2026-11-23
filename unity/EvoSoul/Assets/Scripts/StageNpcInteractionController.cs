using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Handles only a selected Droplet NPC attack. A source can receive its
/// negative reward once because the target and the central ledger both claim it.
/// </summary>
public sealed class StageNpcInteractionController : MonoBehaviour
{
    [SerializeField] private StageInteractionSelector selector;
    [SerializeField] private StageEnergyLedger energyLedger;
    [SerializeField] private VirginDropPlayerController player;
    [SerializeField, Min(0f)] private float attackImpactDelay = 0.18f;

    private bool attackQueued;

    private void Awake()
    {
        if (selector == null)
            selector = GetComponent<StageInteractionSelector>();
        if (player == null)
            player = GetComponent<VirginDropPlayerController>();
        if (energyLedger == null)
            energyLedger = FindAnyObjectByType<StageEnergyLedger>();
    }

    private void Update()
    {
        bool requested = attackQueued || ReadEditorAttackPressed();
        attackQueued = false;
        if (requested)
            TryAttackCurrentSelection();
    }

    /// <summary>Public hook for a future mobile Attack button.</summary>
    public void RequestAttack()
    {
        attackQueued = true;
    }

    public bool TryAttackCurrentSelection()
    {
        if (selector == null || energyLedger == null || player == null)
            return false;

        selector.RefreshSelection();
        StageInteractionSelection selection = selector.CurrentSelection;
        if (selection.Action != StageInteractionAction.Attack || selection.Target is not StageDropletNpcTarget npc)
            return false;

        if (!npc.IsAvailable || !npc.TryReserveInteraction())
            return false;

        if (!energyLedger.TryAward(npc.RewardSourceId, StageEnergyKind.Negative))
        {
            npc.ReleaseInteractionReservation();
            selector.RefreshSelection();
            return false;
        }

        if (!npc.TryClaimReward())
        {
            Debug.LogError($"NPC '{npc.name}' accepted energy after its reservation was lost.", npc);
            selector.RefreshSelection();
            return false;
        }

        // Request the existing player attack animation before scheduling the NPC impact.
        player.RequestAttack();
        if (!npc.BeginDefeat(attackImpactDelay))
        {
            Debug.LogError($"NPC '{npc.name}' failed to start its approved defeat sequence.", npc);
            selector.RefreshSelection();
            return false;
        }

        StageInteractionAudio.PlayAttack();
        selector.RefreshSelection();
        return true;
    }

    private static bool ReadEditorAttackPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(KeyCode.F);
#else
        return false;
#endif
    }
}
