using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Scene-level, single-action interaction prompt. The selector remains the
/// authority for the current target; this view only displays and dispatches it.
/// </summary>
public sealed class StageInteractionPromptView : MonoBehaviour
{
    [SerializeField] private StageInteractionSelector selector;
    [SerializeField] private StageFruitInteractionController fruitController;
    [SerializeField] private StageNpcInteractionController npcController;
    [SerializeField] private GameObject promptRoot;
    [SerializeField] private TMP_Text label;
    [SerializeField] private Button actionButton;

    private StageInteractionAction currentAction;

    public void Configure(
        StageInteractionSelector targetSelector,
        StageFruitInteractionController targetFruitController,
        StageNpcInteractionController targetNpcController,
        GameObject root,
        TMP_Text actionLabel,
        Button button)
    {
        selector = targetSelector;
        fruitController = targetFruitController;
        npcController = targetNpcController;
        promptRoot = root;
        label = actionLabel;
        actionButton = button;
    }

    private void OnEnable()
    {
        if (actionButton != null)
            actionButton.onClick.AddListener(DispatchCurrentAction);
        if (selector != null)
        {
            selector.SelectionChanged += Refresh;
            Refresh(selector.CurrentSelection);
        }
    }

    private void OnDisable()
    {
        if (actionButton != null)
            actionButton.onClick.RemoveListener(DispatchCurrentAction);
        if (selector != null)
            selector.SelectionChanged -= Refresh;
    }

    private void Update()
    {
        if (selector == null)
            return;

        // A selector can already have a target before this view is enabled, or
        // refresh its current selection without emitting a change event for the
        // same target. Keep the visual state aligned in either case.
        StageInteractionSelection selection = selector.CurrentSelection;
        bool shouldBeVisible = selection.IsValid;
        bool visibilityIsStale = promptRoot != null && promptRoot.activeSelf != shouldBeVisible;
        if (currentAction != selection.Action || visibilityIsStale)
            Refresh(selection);
    }

    private void Refresh(StageInteractionSelection selection)
    {
        currentAction = selection.Action;
        bool visible = selection.IsValid;
        if (promptRoot != null)
            promptRoot.SetActive(visible);
        if (label != null)
            label.text = currentAction == StageInteractionAction.Eat ? "Comer" :
                selection.Target is StagePurifiablePlant ? "Golpear" : "Atacar";
    }

    private void DispatchCurrentAction()
    {
        if (currentAction == StageInteractionAction.Eat)
            fruitController?.RequestEat();
        else if (currentAction == StageInteractionAction.Attack)
            npcController?.RequestAttack();
    }
}
