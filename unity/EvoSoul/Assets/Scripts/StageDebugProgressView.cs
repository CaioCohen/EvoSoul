using TMPro;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Optional development-only presentation of the ledger. It owns no scoring
/// rules and can be disabled or omitted from a player-facing scene.
/// </summary>
public sealed class StageDebugProgressView : MonoBehaviour
{
    [SerializeField] private StageEnergyLedger energyLedger;
    [SerializeField] private TMP_Text positiveLabel;
    [SerializeField] private TMP_Text negativeLabel;
    [SerializeField] private TMP_Text totalLabel;
    [SerializeField] private TMP_Text lineageLabel;
    [SerializeField] private GameObject evolvePrompt;
    [SerializeField] private GameObject debugContentRoot;
    [SerializeField] private bool visibleOnEnable = true;
    [SerializeField] private UnityEvent<StageEvolutionLineage> evolutionConfirmed;

    public bool IsVisible => debugContentRoot != null && debugContentRoot.activeSelf;

    private void Awake()
    {
        if (energyLedger == null)
            energyLedger = FindAnyObjectByType<StageEnergyLedger>();
    }

    private void OnEnable()
    {
        if (energyLedger == null)
            return;

        ApplyVisibility();
        energyLedger.ProgressChanged += Refresh;
        energyLedger.EvolutionConfirmed += NotifyEvolutionConfirmed;
        Refresh(energyLedger.ProgressSnapshot);
    }

    private void OnDisable()
    {
        if (energyLedger == null)
            return;

        energyLedger.ProgressChanged -= Refresh;
        energyLedger.EvolutionConfirmed -= NotifyEvolutionConfirmed;
    }

    /// <summary>Hook for the optional debug Evoluir button.</summary>
    public void ConfirmEvolution()
    {
        energyLedger?.TryConfirmEvolution();
    }

    public void SetVisible(bool value)
    {
        visibleOnEnable = value;
        ApplyVisibility();
    }

    private void Refresh(StageProgressSnapshot progress)
    {
        if (positiveLabel != null)
            positiveLabel.text = $"Positiva: {progress.Energy.Positive}";
        if (negativeLabel != null)
            negativeLabel.text = $"Negativa: {progress.Energy.Negative}";
        if (totalLabel != null)
            totalLabel.text = $"Total: {progress.Energy.Total}/{StageEnergyLedger.StageEnergyLimit}";
        if (lineageLabel != null)
            lineageLabel.text = $"Tendência: {progress.Lineage}";
        if (evolvePrompt != null)
            evolvePrompt.SetActive(progress.State == StageProgressState.ReadyToEvolve);
    }

    private void NotifyEvolutionConfirmed(StageEvolutionLineage lineage)
    {
        evolutionConfirmed?.Invoke(lineage);
    }

    private void ApplyVisibility()
    {
        if (debugContentRoot != null)
            debugContentRoot.SetActive(visibleOnEnable);
    }
}
