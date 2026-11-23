using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Player-facing evolution affordance. It is independent of the debug panel
/// and becomes visible only when the current stage ledger is ready to evolve.
/// The click is intentionally a no-op until the evolution flow is implemented.
/// </summary>
public sealed class StageEvolutionPromptView : MonoBehaviour
{
    [SerializeField] private StageEnergyLedger energyLedger;
    [SerializeField] private GameObject promptRoot;

    private const string PromptName = "StageEvolutionPrompt";
    private bool isReadyToEvolve;

    private void Awake()
    {
        if (energyLedger == null)
            energyLedger = FindAnyObjectByType<StageEnergyLedger>();

        EnsurePrompt();
    }

    private void OnEnable()
    {
        if (energyLedger == null)
            return;

        energyLedger.ProgressChanged += Refresh;
        Refresh(energyLedger.ProgressSnapshot);
    }

    private void OnDisable()
    {
        if (energyLedger != null)
            energyLedger.ProgressChanged -= Refresh;
    }

    private void EnsurePrompt()
    {
        if (promptRoot != null)
            return;

        Transform controls = transform.Find("MobileControls");
        if (controls == null)
        {
            GameObject controlsObject = new GameObject("MobileControls", typeof(RectTransform));
            controlsObject.transform.SetParent(transform, false);
            RectTransform controlsRect = controlsObject.GetComponent<RectTransform>();
            controlsRect.anchorMin = Vector2.zero;
            controlsRect.anchorMax = Vector2.one;
            controlsRect.offsetMin = Vector2.zero;
            controlsRect.offsetMax = Vector2.zero;
            controls = controlsObject.transform;
        }

        Transform existing = controls.Find(PromptName);
        promptRoot = existing != null ? existing.gameObject : CreatePrompt(controls);
    }

    private static GameObject CreatePrompt(Transform parent)
    {
        GameObject root = new GameObject(PromptName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        root.transform.SetParent(parent, false);
        RectTransform rect = root.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.one;
        rect.anchorMax = Vector2.one;
        rect.pivot = Vector2.one;
        // Directly above the mobile attack button, which occupies the bottom-right corner.
        rect.anchoredPosition = new Vector2(-40f, 116f);
        rect.sizeDelta = new Vector2(170f, 64f);

        Image image = root.GetComponent<Image>();
        image.color = new Color(0.25f, 0.55f, 0.95f, 0.95f);
        root.GetComponent<Button>().targetGraphic = image;

        GameObject labelRoot = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        labelRoot.transform.SetParent(root.transform, false);
        TextMeshProUGUI label = labelRoot.GetComponent<TextMeshProUGUI>();
        RectTransform labelRect = label.rectTransform;
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
        label.alignment = TextAlignmentOptions.Center;
        label.fontSize = 28f;
        label.color = Color.white;
        label.text = "Evoluir";
        return root;
    }

    private void Refresh(StageProgressSnapshot progress)
    {
        isReadyToEvolve = progress.State == StageProgressState.ReadyToEvolve;

        // The scene's legacy MobileControls hierarchy is currently not submitted
        // by the Canvas (the same issue hides its attack button).  Keep the
        // object around for future migration, but draw the player-facing prompt
        // through OnGUI so the milestone is never invisible to the player.
        if (promptRoot != null)
            promptRoot.SetActive(false);
    }

    private void OnGUI()
    {
        if (!isReadyToEvolve)
            return;

        const float width = 220f;
        const float height = 72f;
        const float rightInset = 36f;
        const float bottomInset = 132f;

        Rect area = new Rect(
            Screen.width - rightInset - width,
            Screen.height - bottomInset - height,
            width,
            height);

        GUIStyle style = new GUIStyle(GUI.skin.button)
        {
            fontSize = Mathf.Clamp(Mathf.RoundToInt(Screen.height * 0.04f), 22, 36),
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };

        // Intentionally no action yet: evolution mechanics are outside this
        // milestone affordance. Drawing it as a button makes the future action
        // discoverable while preserving the requested no-op behavior today.
        GUI.Button(area, "Evoluir", style);
    }
}
