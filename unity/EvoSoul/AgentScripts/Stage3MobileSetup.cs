using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class Stage3MobileSetup
{
    public static object Run()
    {
        var scene = SceneManager.GetActiveScene();
        if (scene.path != "Assets/Scenes/BercarioStage01.unity")
            throw new System.InvalidOperationException("BercarioStage01 must be the active scene.");

        var player = Object.FindAnyObjectByType<VirginDropPlayerController>();
        var selector = Object.FindAnyObjectByType<StageInteractionSelector>();
        var canvas = Object.FindAnyObjectByType<Canvas>();
        if (player == null || selector == null || canvas == null)
            throw new System.InvalidOperationException("Expected player, selector, and canvas were not found.");

        var selectorProperties = new SerializedObject(selector);
        selectorProperties.FindProperty("interactionDistance").floatValue = 2.4f;
        selectorProperties.FindProperty("minimumForwardDot").floatValue = 0.65f;
        selectorProperties.ApplyModifiedPropertiesWithoutUndo();

        foreach (var npc in Object.FindObjectsByType<StageDropletNpcTarget>(FindObjectsInactive.Exclude))
        {
            var npcProperties = new SerializedObject(npc);
            npcProperties.FindProperty("wanderSpeed").floatValue = 0.9f;
            npcProperties.FindProperty("puddleSeconds").floatValue = 1.8f;
            npcProperties.FindProperty("fadeSeconds").floatValue = 0.8f;
            npcProperties.ApplyModifiedPropertiesWithoutUndo();
        }

        var canvasTransform = (RectTransform)canvas.transform;
        var controls = GameObject.Find("MobileControls") ?? new GameObject("MobileControls", typeof(RectTransform));
        controls.transform.SetParent(canvasTransform, false);
        var controlsRect = (RectTransform)controls.transform;
        controlsRect.anchorMin = Vector2.zero;
        controlsRect.anchorMax = Vector2.one;
        controlsRect.offsetMin = Vector2.zero;
        controlsRect.offsetMax = Vector2.zero;

        var joystickRoot = GetOrCreateImage(controls.transform, "MobileJoystick", new Color(0.06f, 0.14f, 0.20f, 0.55f));
        Anchor(joystickRoot.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(155f, 155f), new Vector2(150f, 150f));
        var joystickHandle = GetOrCreateImage(joystickRoot.transform, "Handle", new Color(0.55f, 0.9f, 1f, 0.8f));
        Anchor(joystickHandle.rectTransform, new Vector2(.5f, .5f), new Vector2(.5f, .5f), Vector2.zero, new Vector2(70f, 70f));
        var joystick = joystickRoot.GetComponent<MobileJoystick>() ?? joystickRoot.gameObject.AddComponent<MobileJoystick>();
        joystick.Configure(player, joystickHandle.rectTransform);

        var prompt = GameObject.Find("InteractionPrompt");
        if (prompt != null)
        {
            var promptRect = (RectTransform)prompt.transform;
            Anchor(promptRect, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-132f, 135f), new Vector2(230f, 100f));
        }

        var evolve = GameObject.Find("EvolveButton");
        if (evolve != null)
        {
            var evolveRect = (RectTransform)evolve.transform;
            Anchor(evolveRect, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-145f, -70f), new Vector2(220f, 78f));
        }

        var previewAttack = GetOrCreateImage(controls.transform, "MobileAttackPreview", new Color(0.64f, 0.17f, 0.18f, 0.85f));
        Anchor(previewAttack.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-115f, 20f), new Vector2(150f, 76f));
        var attackLabel = GetOrCreateText(previewAttack.transform, "Label", "Golpe");
        Stretch(attackLabel.rectTransform);
        var previewButton = previewAttack.GetComponent<Button>() ?? previewAttack.gameObject.AddComponent<Button>();
        var mobileAttack = previewAttack.GetComponent<MobileAttackButton>() ?? previewAttack.gameObject.AddComponent<MobileAttackButton>();
        mobileAttack.Configure(player);
        previewButton.targetGraphic = previewAttack;

        var debugView = Object.FindAnyObjectByType<StageDebugProgressView>();
        if (debugView != null)
            debugView.SetVisible(true);

        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        PlayerSettings.allowedAutorotateToLandscapeLeft = true;
        PlayerSettings.allowedAutorotateToLandscapeRight = true;
        PlayerSettings.allowedAutorotateToPortrait = false;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        return new {
            scene = scene.path,
            interactionDistance = 2.4f,
            minimumForwardDot = 0.65f,
            npcWanderSpeed = 0.9f,
            puddleSeconds = 1.8f,
            fadeSeconds = 0.8f,
            orientation = "LandscapeLeft / LandscapeRight"
        };
    }

    private static Image GetOrCreateImage(Transform parent, string name, Color color)
    {
        var existing = parent.Find(name);
        var image = existing != null ? existing.GetComponent<Image>() : null;
        if (image == null)
        {
            var created = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            created.transform.SetParent(parent, false);
            image = created.GetComponent<Image>();
        }
        image.color = color;
        return image;
    }

    private static TMPro.TextMeshProUGUI GetOrCreateText(Transform parent, string name, string text)
    {
        var existing = parent.Find(name);
        var label = existing != null ? existing.GetComponent<TMPro.TextMeshProUGUI>() : null;
        if (label == null)
        {
            var created = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TMPro.TextMeshProUGUI));
            created.transform.SetParent(parent, false);
            label = created.GetComponent<TMPro.TextMeshProUGUI>();
        }
        label.text = text;
        label.alignment = TMPro.TextAlignmentOptions.Center;
        label.fontSize = 28f;
        label.color = Color.white;
        return label;
    }

    private static void Anchor(RectTransform rect, Vector2 min, Vector2 max, Vector2 position, Vector2 size)
    {
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.pivot = new Vector2(.5f, .5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
