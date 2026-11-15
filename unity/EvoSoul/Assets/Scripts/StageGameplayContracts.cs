using System;

public enum StageEnergyKind
{
    Positive,
    Negative
}

public enum StageInteractionAction
{
    None,
    Eat,
    Attack
}

public enum FruitLifecycleState
{
    Available,
    Consumed
}

public enum DropletNpcLifecycleState
{
    Wandering,
    Hit,
    Dying,
    Puddle,
    Removed
}

public enum StageProgressState
{
    Collecting,
    ReadyToEvolve,
    Evolved
}

public enum StageEvolutionLineage
{
    Undecided,
    Celestial,
    Demonic,
    Draconic
}

public readonly struct StageEnergySnapshot
{
    public int Positive { get; }
    public int Negative { get; }
    public int Total => Positive + Negative;

    public StageEnergySnapshot(int positive, int negative)
    {
        Positive = positive;
        Negative = negative;
    }
}

public readonly struct StageProgressSnapshot
{
    public StageEnergySnapshot Energy { get; }
    public StageProgressState State { get; }
    public StageEvolutionLineage Lineage { get; }

    public StageProgressSnapshot(StageEnergySnapshot energy, StageProgressState state, StageEvolutionLineage lineage)
    {
        Energy = energy;
        State = state;
        Lineage = lineage;
    }
}

public interface IStageInteractionTarget
{
    string RewardSourceId { get; }
    StageInteractionAction InteractionAction { get; }
    bool IsAvailable { get; }
    bool TryReserveInteraction();
    void ReleaseInteractionReservation();
    bool TryClaimReward();
}
