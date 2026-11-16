using UnityEngine;

/// <summary>Keeps target highlights in sync without duplicating selection rules.</summary>
public sealed class StageSelectionVisualController : MonoBehaviour
{
    [SerializeField] private StageInteractionSelector selector;
    [SerializeField] private StageTargetArrowIndicator arrowIndicator;

    private void Awake()
    {
        if (selector == null)
            selector = GetComponent<StageInteractionSelector>();
        if (arrowIndicator == null)
            arrowIndicator = GetComponent<StageTargetArrowIndicator>();
    }

    private void OnEnable()
    {
        if (selector != null)
        {
            selector.SelectionChanged += Refresh;
            Refresh(selector.CurrentSelection);
        }
    }

    private void OnDisable()
    {
        if (selector != null)
            selector.SelectionChanged -= Refresh;
    }

    private static void Refresh(StageInteractionSelection selection)
    {
        StageTargetHighlight[] highlights = FindObjectsByType<StageTargetHighlight>(FindObjectsInactive.Include);
        foreach (StageTargetHighlight highlight in highlights)
        {
            IStageInteractionTarget target = highlight.GetComponent<StageInteractionTarget>();
            highlight.SetSelected(target != null && ReferenceEquals(target, selection.Target));
        }
    }
}
