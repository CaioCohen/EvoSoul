using System;
using UnityEngine;

public abstract class StageInteractionTarget : MonoBehaviour, IStageInteractionTarget
{
    [SerializeField] private string rewardSourceId;

    private bool isAvailable = true;
    private bool interactionReserved;
    private bool rewardClaimed;

    public abstract StageInteractionAction InteractionAction { get; }
    public string RewardSourceId => rewardSourceId;
    public bool IsAvailable => isAvailable && !rewardClaimed;
    public bool IsInteractionReserved => interactionReserved;
    public bool HasClaimedReward => rewardClaimed;

    public event Action<StageInteractionTarget> AvailabilityChanged;

    protected virtual void Awake()
    {
        if (string.IsNullOrWhiteSpace(rewardSourceId))
            rewardSourceId = Guid.NewGuid().ToString("N");
    }

    public bool TryReserveInteraction()
    {
        if (!IsAvailable || interactionReserved)
            return false;

        interactionReserved = true;
        return true;
    }

    public void ReleaseInteractionReservation()
    {
        if (!rewardClaimed)
            interactionReserved = false;
    }

    public bool TryClaimReward()
    {
        if (!IsAvailable || !interactionReserved)
            return false;

        rewardClaimed = true;
        interactionReserved = false;
        isAvailable = false;
        AvailabilityChanged?.Invoke(this);
        return true;
    }

    protected void SetAvailability(bool value)
    {
        if (isAvailable == value || rewardClaimed)
            return;

        isAvailable = value;
        if (!isAvailable)
            interactionReserved = false;
        AvailabilityChanged?.Invoke(this);
    }

    // A restored target is a new interaction with a new ledger identity.
    protected void ResetForNewInteraction()
    {
        rewardSourceId = Guid.NewGuid().ToString("N");
        rewardClaimed = false;
        interactionReserved = false;
        isAvailable = true;
        AvailabilityChanged?.Invoke(this);
    }
}
