using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class StageEnergyLedger : MonoBehaviour
{
    public const int StageEnergyLimit = 10;

    private readonly HashSet<string> awardedSourceIds = new();

    private int positiveEnergy;
    private int negativeEnergy;
    [SerializeField] private StageProgressState progressState = StageProgressState.Collecting;
    [SerializeField] private StageEvolutionLineage confirmedLineage = StageEvolutionLineage.Undecided;

    public int PositiveEnergy => positiveEnergy;
    public int NegativeEnergy => negativeEnergy;
    public int TotalEnergy => positiveEnergy + negativeEnergy;
    public StageEnergySnapshot Snapshot => new(positiveEnergy, negativeEnergy);
    public StageProgressState ProgressState => progressState;
    public StageEvolutionLineage CurrentLineage => progressState == StageProgressState.Evolved
        ? confirmedLineage
        : StageEnergyRules.DetermineLineage(positiveEnergy, negativeEnergy);
    public StageProgressSnapshot ProgressSnapshot => new(Snapshot, progressState, CurrentLineage);
    public bool CanEvolve => progressState == StageProgressState.ReadyToEvolve;

    public event Action<StageEnergySnapshot> EnergyChanged;
    public event Action<StageProgressSnapshot> ProgressChanged;
    public event Action<StageEvolutionLineage> EvolutionConfirmed;

    public bool TryAward(string sourceId, StageEnergyKind kind)
    {
        if (progressState == StageProgressState.Evolved || string.IsNullOrWhiteSpace(sourceId) || !awardedSourceIds.Add(sourceId))
            return false;

        StageEnergyRules.ApplyAward(ref positiveEnergy, ref negativeEnergy, kind, StageEnergyLimit);
        if (positiveEnergy + negativeEnergy >= StageEnergyLimit)
            progressState = StageProgressState.ReadyToEvolve;

        EnergyChanged?.Invoke(Snapshot);
        ProgressChanged?.Invoke(ProgressSnapshot);
        return true;
    }

    /// <summary>
    /// Commits the lineage currently indicated by the stage distribution. This
    /// is deliberately explicit: reaching 10/10 never evolves automatically.
    /// </summary>
    public bool TryConfirmEvolution()
    {
        if (!CanEvolve)
            return false;

        confirmedLineage = StageEnergyRules.DetermineLineage(positiveEnergy, negativeEnergy);
        progressState = StageProgressState.Evolved;
        ProgressChanged?.Invoke(ProgressSnapshot);
        EvolutionConfirmed?.Invoke(confirmedLineage);
        return true;
    }
}

/// <summary>
/// Pure stage-energy rules. Step 6 can validate these without requiring a
/// scene, UI, or MonoBehaviour lifecycle.
/// </summary>
public static class StageEnergyRules
{
    public static void ApplyAward(ref int positive, ref int negative, StageEnergyKind kind, int limit)
    {
        limit = Mathf.Max(1, limit);
        if (positive + negative < limit)
        {
            if (kind == StageEnergyKind.Positive)
                positive++;
            else
                negative++;
            return;
        }

        if (kind == StageEnergyKind.Positive && negative > 0)
        {
            negative--;
            positive++;
        }
        else if (kind == StageEnergyKind.Negative && positive > 0)
        {
            positive--;
            negative++;
        }
    }

    public static StageEvolutionLineage DetermineLineage(int positive, int negative)
    {
        if (positive >= 8)
            return StageEvolutionLineage.Celestial;
        if (negative >= 8)
            return StageEvolutionLineage.Demonic;
        return StageEvolutionLineage.Draconic;
    }
}
