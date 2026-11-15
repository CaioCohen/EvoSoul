using System;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class Stage3SceneSetup
{
    public static object Run()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != "Assets/Scenes/BercarioStage01.unity")
            throw new InvalidOperationException($"Expected BercarioStage01 scene, got '{scene.path}'.");

        RemovePreviousIntegration("StageGameplay");
        RemovePreviousIntegration("StageDebugCanvas");
        RemovePreviousIntegration("StageFruitTargets");
        RemovePreviousIntegration("NurseryDroplets");

        GameObject player = GameObject.Find("VirginDropPlayer") ?? throw new InvalidOperationException("VirginDropPlayer is missing.");
        GameObject gameplay = new GameObject("StageGameplay");
        StageEnergyLedger ledger = gameplay.AddComponent<StageEnergyLedger>();
        StageEvolutionAdvanceHook advanceHook = gameplay.AddComponent<StageEvolutionAdvanceHook>();

        StageInteractionSelector selector = player.GetComponent<StageInteractionSelector>() ?? player.AddComponent<StageInteractionSelector>();
        StageFruitInteractionController fruitController = player.GetComponent<StageFruitInteractionController>() ?? player.AddComponent<StageFruitInteractionController>();
        StageNpcInteractionController npcController = player.GetComponent<StageNpcInteractionController>() ?? player.AddComponent<StageNpcInteractionController>();
        if (player.GetComponent<StageSelectionVisualController>() == null)
            player.AddComponent<StageSelectionVisualController>();

        int fruitCount = SetupFruits();
        int npcCount = SetupNpcs();
        GameObject canvasRoot = BuildUi(player, selector, fruitController, npcController, ledger);

        SerializedObject hookData = new SerializedObject(advanceHook);
        hookData.FindProperty("energyLedger").objectReferenceValue = ledger;
        hookData.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        return new { status = "configured", scene = scene.path, fruits = fruitCount, npcs = npcCount, ui = canvasRoot.name };
    }

    private static int SetupFruits()
    {
        GameObject root = new GameObject("StageFruitTargets");
        int count = 0;
        for (int index = 1; index <= 5; index++)
        {
            string name = $"GEO-Fruit_{index:00}";
            GameObject fruit = GameObject.Find(name);
            if (fruit == null)
                continue;

            GameObject leaf = GameObject.Find($"{name}_Leaf");
            GameObject targetRoot = new GameObject($"FruitTarget_{index:00}");
            targetRoot.transform.SetParent(root.transform);
            targetRoot.transform.position = fruit.transform.position;
            SphereCollider trigger = targetRoot.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 0.42f;
            StageFruitTarget target = targetRoot.AddComponent<StageFruitTarget>();
            target.ConfigureVisuals(fruit, leaf != null ? new[] { leaf } : Array.Empty<GameObject>());
            StageTargetHighlight highlight = targetRoot.AddComponent<StageTargetHighlight>();
            Renderer[] renderers = leaf != null
                ? new[] { fruit.GetComponent<Renderer>(), leaf.GetComponent<Renderer>() }
                : new[] { fruit.GetComponent<Renderer>() };
            highlight.ConfigureRenderers(Array.FindAll(renderers, renderer => renderer != null));
            count++;
        }
        return count;
    }

    private static int SetupNpcs()
    {
        GameObject root = new GameObject("NurseryDroplets");
        int count = 0;
        for (int index = 1; index <= 4; index++)
        {
            GameObject marker = GameObject.Find($"NpcSpawn_{index:00}");
            if (marker == null)
                continue;

            GameObject npc = new GameObject($"DropletNpc_{index:00}");
            npc.transform.SetParent(root.transform);
            npc.transform.position = marker.transform.position + Vector3.up * 0.48f;
            SphereCollider trigger = npc.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 0.5f;

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            body.name = "VisualBody";
            DestroyCollider(body);
            body.transform.SetParent(npc.transform, false);
            body.transform.localScale = new Vector3(0.72f, 0.9f, 0.72f);
            body.transform.localPosition = Vector3.zero;
            ApplyNpcMaterial(body.GetComponent<Renderer>());

            GameObject puddle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            puddle.name = "PuddleVisual";
            DestroyCollider(puddle);
            puddle.transform.SetParent(npc.transform, false);
            puddle.transform.localScale = new Vector3(0.56f, 0.05f, 0.56f);
            puddle.transform.localPosition = new Vector3(0f, -0.42f, 0f);
            ApplyPuddleMaterial(puddle.GetComponent<Renderer>());
            puddle.SetActive(false);

            StageDropletNpcTarget target = npc.AddComponent<StageDropletNpcTarget>();
            npc.AddComponent<StageTargetHighlight>();
            SerializedObject targetData = new SerializedObject(target);
            targetData.FindProperty("visualRoot").objectReferenceValue = body.transform;
            targetData.FindProperty("puddleVisual").objectReferenceValue = puddle;
            targetData.FindProperty("fadeRenderers").arraySize = 2;
            targetData.FindProperty("fadeRenderers").GetArrayElementAtIndex(0).objectReferenceValue = body.GetComponent<Renderer>();
            targetData.FindProperty("fadeRenderers").GetArrayElementAtIndex(1).objectReferenceValue = puddle.GetComponent<Renderer>();
            targetData.ApplyModifiedPropertiesWithoutUndo();
            count++;
        }
        return count;
    }

    private static GameObject BuildUi(GameObject player, StageInteractionSelector selector, StageFruitInteractionController fruitController, StageNpcInteractionController npcController, StageEnergyLedger ledger)
    {
        GameObject canvasRoot = new GameObject("StageDebugCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasRoot.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasRoot.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasRoot.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920f, 1080f);

        EventSystem eventSystem = UnityEngine.Object.FindAnyObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            SceneManager.MoveGameObjectToScene(eventSystemObject, SceneManager.GetActiveScene());
        }
        else
        {
            StandaloneInputModule legacyModule = eventSystem.GetComponent<StandaloneInputModule>();
            if (legacyModule != null)
                UnityEngine.Object.DestroyImmediate(legacyModule);
            if (eventSystem.GetComponent<InputSystemUIInputModule>() == null)
                eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
        }

        GameObject debugRoot = CreatePanel(canvasRoot.transform, "DebugPanel", new Vector2(330f, 210f), new Vector2(20f, -20f), new Vector2(0f, 1f));
        TMP_Text positive = CreateLabel(debugRoot.transform, "PositiveLabel", new Vector2(10f, -15f));
        TMP_Text negative = CreateLabel(debugRoot.transform, "NegativeLabel", new Vector2(10f, -50f));
        TMP_Text total = CreateLabel(debugRoot.transform, "TotalLabel", new Vector2(10f, -85f));
        TMP_Text lineage = CreateLabel(debugRoot.transform, "LineageLabel", new Vector2(10f, -120f));
        Button evolveButton = CreateButton(debugRoot.transform, "EvolveButton", "Evoluir", new Vector2(110f, -170f), new Vector2(160f, 42f));

        StageDebugProgressView debugView = canvasRoot.AddComponent<StageDebugProgressView>();
        debugView.enabled = false;
        SerializedObject debugData = new SerializedObject(debugView);
        debugData.FindProperty("energyLedger").objectReferenceValue = ledger;
        debugData.FindProperty("positiveLabel").objectReferenceValue = positive;
        debugData.FindProperty("negativeLabel").objectReferenceValue = negative;
        debugData.FindProperty("totalLabel").objectReferenceValue = total;
        debugData.FindProperty("lineageLabel").objectReferenceValue = lineage;
        debugData.FindProperty("evolvePrompt").objectReferenceValue = evolveButton.gameObject;
        debugData.FindProperty("debugContentRoot").objectReferenceValue = debugRoot;
        debugData.ApplyModifiedPropertiesWithoutUndo();
        evolveButton.onClick.AddListener(debugView.ConfirmEvolution);
        debugView.enabled = true;

        GameObject promptRoot = CreatePanel(canvasRoot.transform, "InteractionPrompt", new Vector2(220f, 88f), new Vector2(-30f, 30f), new Vector2(1f, 0f));
        Button actionButton = CreateButton(promptRoot.transform, "ContextAction", "Comer", new Vector2(30f, -22f), new Vector2(160f, 48f));
        TMP_Text actionLabel = actionButton.GetComponentInChildren<TMP_Text>();
        StageInteractionPromptView promptView = canvasRoot.AddComponent<StageInteractionPromptView>();
        promptView.enabled = false;
        promptView.Configure(selector, fruitController, npcController, promptRoot, actionLabel, actionButton);
        promptRoot.SetActive(false);
        promptView.enabled = true;
        return canvasRoot;
    }

    private static GameObject CreatePanel(Transform parent, string name, Vector2 size, Vector2 position, Vector2 anchor)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = anchor;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        panel.GetComponent<Image>().color = new Color(0.04f, 0.08f, 0.12f, 0.72f);
        return panel;
    }

    private static TMP_Text CreateLabel(Transform parent, string name, Vector2 position)
    {
        GameObject label = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        label.transform.SetParent(parent, false);
        RectTransform rect = label.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(-20f, 30f);
        TextMeshProUGUI text = label.GetComponent<TextMeshProUGUI>();
        text.fontSize = 23f;
        text.color = Color.white;
        text.text = name;
        return text;
    }

    private static Button CreateButton(Transform parent, string name, string caption, Vector2 position, Vector2 size)
    {
        GameObject buttonRoot = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonRoot.transform.SetParent(parent, false);
        RectTransform rect = buttonRoot.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        buttonRoot.GetComponent<Image>().color = new Color(0.18f, 0.58f, 0.31f, 0.95f);
        TMP_Text text = CreateLabel(buttonRoot.transform, "Label", new Vector2(0f, 0f));
        RectTransform textRect = text.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = Vector2.zero;
        textRect.sizeDelta = Vector2.zero;
        text.alignment = TextAlignmentOptions.Center;
        text.text = caption;
        return buttonRoot.GetComponent<Button>();
    }

    private static void DestroyCollider(GameObject target)
    {
        Collider collider = target.GetComponent<Collider>();
        if (collider != null)
            UnityEngine.Object.DestroyImmediate(collider);
    }

    private static void ApplyNpcMaterial(Renderer renderer)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        Material material = new Material(shader) { color = new Color(0.48f, 0.52f, 0.58f, 1f) };
        renderer.sharedMaterial = material;
    }

    private static void ApplyPuddleMaterial(Renderer renderer)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        Material material = new Material(shader) { color = new Color(0.2f, 0.24f, 0.3f, 0.95f) };
        renderer.sharedMaterial = material;
    }

    private static void RemovePreviousIntegration(string name)
    {
        GameObject existing = GameObject.Find(name);
        if (existing != null)
            UnityEngine.Object.DestroyImmediate(existing);
    }
}
