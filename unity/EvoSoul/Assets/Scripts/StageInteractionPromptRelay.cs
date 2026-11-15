using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Optional bridge for a future HUD or world-space highlight. It owns no UI,
/// so selection can be tested without a Canvas in the scene.
/// </summary>
public sealed class StageInteractionPromptRelay : MonoBehaviour
{
    [SerializeField] private StageInteractionSelector selector;
    [SerializeField] private UnityEvent<StageInteractionAction> actionChanged;
    [SerializeField] private UnityEvent<bool> visibilityChanged;

    private void Awake()
    {
        if (selector == null)
            selector = GetComponent<StageInteractionSelector>();
    }

    private void OnEnable()
    {
        if (selector != null)
        {
            selector.SelectionChanged += NotifySelectionChanged;
            NotifySelectionChanged(selector.CurrentSelection);
        }
    }

    private void OnDisable()
    {
        if (selector != null)
            selector.SelectionChanged -= NotifySelectionChanged;
    }

    private void NotifySelectionChanged(StageInteractionSelection selection)
    {
        actionChanged?.Invoke(selection.Action);
        visibilityChanged?.Invoke(selection.IsValid);
    }
}
