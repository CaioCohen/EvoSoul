using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Stops further nursery interactions after evolution and provides a scene hook
/// for the future map transition, without implementing later maps.
/// </summary>
public sealed class StageEvolutionAdvanceHook : MonoBehaviour
{
    [SerializeField] private StageEnergyLedger energyLedger;
    [SerializeField] private UnityEvent<StageEvolutionLineage> advanceRequested;

    private void Awake()
    {
        if (energyLedger == null)
            energyLedger = FindAnyObjectByType<StageEnergyLedger>();
    }

    private void OnEnable()
    {
        if (energyLedger != null)
            energyLedger.EvolutionConfirmed += StopNurseryCollection;
    }

    private void OnDisable()
    {
        if (energyLedger != null)
            energyLedger.EvolutionConfirmed -= StopNurseryCollection;
    }

    private void StopNurseryCollection(StageEvolutionLineage lineage)
    {
        foreach (StageInteractionTarget target in FindObjectsByType<StageInteractionTarget>(FindObjectsInactive.Exclude))
        {
            foreach (Collider targetCollider in target.GetComponents<Collider>())
                targetCollider.enabled = false;
        }

        advanceRequested?.Invoke(lineage);
        Debug.Log($"Stage 1 evolution confirmed: {lineage}. Advance transition is ready for a future map.", this);
    }
}
