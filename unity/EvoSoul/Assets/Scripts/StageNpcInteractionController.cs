using UnityEngine;
using System.Collections;
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
    private Coroutine plantSuction;
    private StagePurifiablePlant reservedPlant;

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
        if (selection.Action != StageInteractionAction.Attack)
            return false;

        if (selection.Target is StagePurifiablePlant plant)
        {
            if (plantSuction != null || !plant.TryReserveInteraction()) return false;
            reservedPlant = plant;
            plantSuction = StartCoroutine(SuckPlant(plant));
            return true;
        }

        if (selection.Target is not StageDropletNpcTarget npc)
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

    private IEnumerator SuckPlant(StagePurifiablePlant plant)
    {
        Color color = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.Contains("Draconic")
            ? new Color(1f, .35f, .035f) : new Color(1f, .04f, .12f);
        var fx = new GameObject("PlantSuctionVFX").AddComponent<StageSuctionVfx>();
        fx.Configure(plant.transform, player.transform, color, .9f);
        yield return new WaitForSeconds(.9f);
        if (plant != null)
        {
            plant.ReleaseInteractionReservation();
            plant.TryPurify(energyLedger);
        }
        reservedPlant = null;
        plantSuction = null;
        if (selector != null) selector.RefreshSelection();
    }

    private void OnDisable()
    {
        if (plantSuction != null) StopCoroutine(plantSuction);
        if (reservedPlant != null) reservedPlant.ReleaseInteractionReservation();
        plantSuction = null;
        reservedPlant = null;
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
