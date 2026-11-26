using UnityEngine;
using System.Collections;

/// <summary>A one-time positive-energy target whose visual changes in place.</summary>
public sealed class StagePurifiablePlant : StageInteractionTarget
{
    [SerializeField] private GameObject corruptedVisual;
    [SerializeField] private GameObject purifiedVisual;
    [SerializeField] private string interactionLabel = "Golpear";
    [SerializeField] private bool allowRecorruption = true;
    [SerializeField, Min(0f)] private float meanRecorruptionSeconds = 600f;
    [SerializeField, Min(0f)] private float recorruptionStandardDeviation = 30f;
    public override StageInteractionAction InteractionAction => StageInteractionAction.Attack;
    public string InteractionLabel => string.IsNullOrWhiteSpace(interactionLabel) ? "Golpear" : interactionLabel;
    public GameObject CorruptedVisual => corruptedVisual;
    public GameObject PurifiedVisual => purifiedVisual;

    public void Configure(GameObject corrupted, GameObject purified, string prompt = "Golpear")
    {
        corruptedVisual = corrupted;
        purifiedVisual = purified;
        interactionLabel = prompt;
        if (corruptedVisual != null) corruptedVisual.SetActive(true);
        if (purifiedVisual != null) purifiedVisual.SetActive(false);
    }

    public bool TryPurify(StageEnergyLedger ledger)
    {
        if (ledger == null || corruptedVisual == null || purifiedVisual == null || !TryReserveInteraction())
            return false;
        if (!ledger.TryAward(RewardSourceId, StageEnergyKind.Positive))
        {
            ReleaseInteractionReservation();
            return false;
        }
        if (!TryClaimReward())
        {
            Debug.LogError("Plant reward was awarded but the reservation could not be claimed.", this);
            return false;
        }
        corruptedVisual.SetActive(false);
        purifiedVisual.SetActive(true);
        if (allowRecorruption)
            StartCoroutine(RecorruptAfterDelay());
        return true;
    }

    private IEnumerator RecorruptAfterDelay()
    {
        yield return new WaitForSeconds(ProfaneRespawnTiming.Sample(meanRecorruptionSeconds, recorruptionStandardDeviation));
        if (corruptedVisual == null || purifiedVisual == null) yield break;
        purifiedVisual.SetActive(false);
        corruptedVisual.SetActive(true);
        ResetForNewInteraction();
    }
}
