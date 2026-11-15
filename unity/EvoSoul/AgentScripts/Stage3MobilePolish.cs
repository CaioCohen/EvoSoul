using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class Stage3MobilePolish
{
    public static object Run()
    {
        var scene = SceneManager.GetActiveScene();
        var canvas = Object.FindAnyObjectByType<Canvas>();
        var debug = Object.FindAnyObjectByType<StageDebugProgressView>();
        if (canvas == null || debug == null)
            throw new System.InvalidOperationException("Missing stage canvas or debug view.");

        var scaler = canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        var controls = GameObject.Find("MobileControls");
        if (controls == null)
            throw new System.InvalidOperationException("MobileControls was not created.");
        var existing = controls.transform.Find("DebugToggle");
        var toggleObject = existing != null ? existing.gameObject : new GameObject("DebugToggle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(StageDebugToggleButton));
        if (existing == null)
            toggleObject.transform.SetParent(controls.transform, false);

        var rect = (RectTransform)toggleObject.transform;
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(.5f, .5f);
        rect.anchoredPosition = new Vector2(110f, -60f);
        rect.sizeDelta = new Vector2(160f, 58f);
        var image = toggleObject.GetComponent<Image>();
        image.color = new Color(.15f, .2f, .27f, .88f);
        var toggle = toggleObject.GetComponent<StageDebugToggleButton>();
        toggle.Configure(debug);
        var button = toggleObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(toggle.Toggle);

        var labelTransform = toggleObject.transform.Find("Label");
        var label = labelTransform != null ? labelTransform.GetComponent<TMPro.TextMeshProUGUI>() : null;
        if (label == null)
        {
            var labelObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TMPro.TextMeshProUGUI));
            labelObject.transform.SetParent(toggleObject.transform, false);
            label = labelObject.GetComponent<TMPro.TextMeshProUGUI>();
        }
        var labelRect = label.rectTransform;
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
        label.text = "Debug";
        label.fontSize = 24;
        label.alignment = TMPro.TextAlignmentOptions.Center;
        label.color = Color.white;

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        return new { canvasScaleMode = scaler.uiScaleMode.ToString(), referenceResolution = "1920x1080", debugToggle = true };
    }
}
