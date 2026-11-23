using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class StageEnergyLedger : MonoBehaviour
{
    // Kept for existing Stage 1 tooling. Runtime scoring always uses
    // EnergyMilestone, which is selected by stageNumber.
    public const int StageEnergyLimit = 10;
    private static readonly int[] EnergyMilestones = { 10, 50, 200, 1000, 5000, 10000 };

    private readonly HashSet<string> awardedSourceIds = new();

    [SerializeField, Range(1, 6)] private int stageNumber = 1;
    [SerializeField] private StageEvolutionLineage startingLineage = StageEvolutionLineage.Undecided;
    private int positiveEnergy;
    private int negativeEnergy;
    [SerializeField] private StageProgressState progressState = StageProgressState.Collecting;
    [SerializeField] private StageEvolutionLineage confirmedLineage = StageEvolutionLineage.Undecided;

    public int PositiveEnergy => positiveEnergy;
    public int NegativeEnergy => negativeEnergy;
    public int TotalEnergy => positiveEnergy + negativeEnergy;
    public int StageNumber => stageNumber;
    public int EnergyMilestone => GetMilestoneForStage(stageNumber);
    public StageEnergySnapshot Snapshot => new(positiveEnergy, negativeEnergy);
    public StageProgressState ProgressState => progressState;
    public StageEvolutionLineage CurrentLineage => progressState == StageProgressState.Evolved
        ? confirmedLineage
        : StageEnergyRules.ResolveTransition(startingLineage, positiveEnergy, negativeEnergy);
    public StageProgressSnapshot ProgressSnapshot => new(Snapshot, progressState, CurrentLineage);
    public bool CanEvolve => progressState == StageProgressState.ReadyToEvolve;

    public event Action<StageEnergySnapshot> EnergyChanged;
    public event Action<StageProgressSnapshot> ProgressChanged;
    public event Action<StageEvolutionLineage> EvolutionConfirmed;

    public bool TryAward(string sourceId, StageEnergyKind kind)
    {
        if (progressState == StageProgressState.Evolved || string.IsNullOrWhiteSpace(sourceId) || !awardedSourceIds.Add(sourceId))
            return false;

        StageEnergyRules.ApplyAward(ref positiveEnergy, ref negativeEnergy, kind, EnergyMilestone);
        if (positiveEnergy + negativeEnergy >= EnergyMilestone)
            progressState = StageProgressState.ReadyToEvolve;

        EnergyChanged?.Invoke(Snapshot);
        ProgressChanged?.Invoke(ProgressSnapshot);
        return true;
    }

    /// <summary>
    /// Grants one energy unit for development tooling. It deliberately follows
    /// the same cap and post-milestone replacement rules as world interactions.
    /// </summary>
    public bool TryAwardDebug(StageEnergyKind kind)
    {
        if (progressState == StageProgressState.Evolved)
            return false;

        StageEnergyRules.ApplyAward(ref positiveEnergy, ref negativeEnergy, kind, EnergyMilestone);
        if (positiveEnergy + negativeEnergy >= EnergyMilestone)
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

        confirmedLineage = StageEnergyRules.ResolveTransition(startingLineage, positiveEnergy, negativeEnergy);
        progressState = StageProgressState.Evolved;
        ProgressChanged?.Invoke(ProgressSnapshot);
        EvolutionConfirmed?.Invoke(confirmedLineage);
        return true;
    }

    public static int GetMilestoneForStage(int stage)
    {
        return EnergyMilestones[Mathf.Clamp(stage, 1, EnergyMilestones.Length) - 1];
    }

    // Configure before collecting any energy; each scene starts a fresh ledger.
    public void ConfigureStage(int stage, StageEvolutionLineage lineage)
    {
        if (TotalEnergy != 0 || progressState != StageProgressState.Collecting)
            throw new InvalidOperationException("Cannot reconfigure a stage in progress.");
        stageNumber = Mathf.Clamp(stage, 1, 6);
        startingLineage = lineage;
    }
}

/// <summary>
/// Pure stage-energy rules. Step 6 can validate these without requiring a
/// scene, UI, or MonoBehaviour lifecycle.
/// </summary>
public static class StageEnergyRules
{
    public static StageEvolutionLineage ResolveTransition(StageEvolutionLineage current, int positive, int negative)
    {
        var tendency = DetermineLineage(positive, negative);
        if (current == StageEvolutionLineage.Demonic)
            return tendency == StageEvolutionLineage.Celestial ? StageEvolutionLineage.Draconic : current;
        if (current == StageEvolutionLineage.Celestial)
            return tendency == StageEvolutionLineage.Demonic ? StageEvolutionLineage.Draconic : current;
        return tendency;
    }
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
        int total = positive + negative;
        if (total > 0 && positive * 100 >= total * 71)
            return StageEvolutionLineage.Celestial;
        if (total > 0 && negative * 100 >= total * 71)
            return StageEvolutionLineage.Demonic;
        return StageEvolutionLineage.Draconic;
    }
}
