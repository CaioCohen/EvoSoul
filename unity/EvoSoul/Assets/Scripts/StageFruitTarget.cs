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
    /// Supplies the renderers that define the fruit's visible bounds. The
    /// interaction collider can deliberately live lower than the mesh, so an
    /// indicator must anchor to this presentation instead of the collider's
    /// transform.
    /// </summary>
    public Renderer[] GetIndicatorRenderers()
    {
        return fruitVisual != null
            ? fruitVisual.GetComponentsInChildren<Renderer>(true)
            : GetComponentsInChildren<Renderer>(true);
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
