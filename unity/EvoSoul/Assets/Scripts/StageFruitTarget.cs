using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A directly consumable fruit. The bush that visually hosts it must remain a
/// separate object, so only this target is hidden after a successful consume.
/// </summary>
public sealed class StageFruitTarget : StageInteractionTarget
{
    [SerializeField] private GameObject fruitVisual;
    [SerializeField] private GameObject[] additionalVisuals;
    [SerializeField] private UnityEvent consumed;

    public override StageInteractionAction InteractionAction => StageInteractionAction.Eat;
    public FruitLifecycleState LifecycleState { get; private set; } = FruitLifecycleState.Available;

    public event System.Action<StageFruitTarget> Consumed;

    public void ConfigureVisuals(GameObject primaryVisual, GameObject[] extraVisuals)
    {
        fruitVisual = primaryVisual;
        additionalVisuals = extraVisuals;
    }

    /// <summary>
    /// Finishes the presentation after the target reward has been claimed.
    /// This is intentionally separate from reward claiming so feedback can be
    /// replaced later without duplicating scoring rules.
    /// </summary>
    public bool CompleteConsumption()
    {
        if (LifecycleState != FruitLifecycleState.Available || !HasClaimedReward)
            return false;

        LifecycleState = FruitLifecycleState.Consumed;
        consumed?.Invoke();
        Consumed?.Invoke(this);

        GameObject visualToHide = fruitVisual != null ? fruitVisual : gameObject;
        visualToHide.SetActive(false);
        if (additionalVisuals != null)
        {
            foreach (GameObject extraVisual in additionalVisuals)
            {
                if (extraVisual != null)
                    extraVisual.SetActive(false);
            }
        }
        return true;
    }
}
