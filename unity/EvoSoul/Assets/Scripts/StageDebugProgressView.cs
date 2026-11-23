using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

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

        EnsureEnergyButtons();
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

    /// <summary>Development HUD hook: awards positive energy through the shared ledger rules.</summary>
    public void AddPositiveEnergy()
    {
        energyLedger?.TryAwardDebug(StageEnergyKind.Positive);
    }

    /// <summary>Development HUD hook: awards negative energy through the shared ledger rules.</summary>
    public void AddNegativeEnergy()
    {
        energyLedger?.TryAwardDebug(StageEnergyKind.Negative);
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
            totalLabel.text = $"Total: {progress.Energy.Total}/{energyLedger.EnergyMilestone}";
        if (lineageLabel != null)
            lineageLabel.text = $"Tendência: {progress.Lineage}";
        // Evolution is presented by the player-facing prompt above Attack.
        // Keep this legacy debug-panel control hidden so there is a single,
        // clearly positioned affordance during gameplay.
        if (evolvePrompt != null)
            evolvePrompt.SetActive(false);
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

    private void EnsureEnergyButtons()
    {
        if (debugContentRoot == null)
            return;

        RectTransform panelRect = debugContentRoot.GetComponent<RectTransform>();
        if (panelRect != null && panelRect.sizeDelta.y < 270f)
            panelRect.sizeDelta = new Vector2(panelRect.sizeDelta.x, 270f);

        CreateEnergyButton("AddPositiveEnergyButton", "+ Energia Positiva", new Vector2(10f, -155f), AddPositiveEnergy);
        CreateEnergyButton("AddNegativeEnergyButton", "+ Energia Negativa", new Vector2(170f, -155f), AddNegativeEnergy);

        Transform evolveButton = debugContentRoot.transform.Find("EvolveButton");
        if (evolveButton != null)
            evolveButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(85f, -212f);
    }

    private void CreateEnergyButton(string buttonName, string caption, Vector2 position, UnityAction action)
    {
        Transform existing = debugContentRoot.transform.Find(buttonName);
        GameObject root = existing != null
            ? existing.gameObject
            : new GameObject(buttonName, typeof(RectTransform), typeof(Image), typeof(Button));
        if (existing == null)
            root.transform.SetParent(debugContentRoot.transform, false);

        RectTransform rect = root.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.up;
        rect.anchorMax = Vector2.up;
        rect.pivot = Vector2.up;
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(150f, 42f);

        Image image = root.GetComponent<Image>();
        image.color = new Color(0.18f, 0.58f, 0.31f, 0.95f);
        Button button = root.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);

        TextMeshProUGUI label = root.GetComponentInChildren<TextMeshProUGUI>();
        if (label == null)
        {
            GameObject labelRoot = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelRoot.transform.SetParent(root.transform, false);
            label = labelRoot.GetComponent<TextMeshProUGUI>();
        }

        RectTransform labelRect = label.rectTransform;
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
        label.alignment = TextAlignmentOptions.Center;
        label.fontSize = 18f;
        label.color = Color.white;
        label.text = caption;
    }
}
