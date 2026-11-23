using System.Collections;
using UnityEngine;

/// <summary>Opt-in evolution presentation. Assign the next-stage models before enabling.</summary>
public sealed class StageEvolutionSequence : MonoBehaviour
{
    [System.Serializable] public sealed class Form
    {
        public StageEvolutionLineage lineage;
        public GameObject prefab;
        public AnimationClip walk;
        public AnimationClip attack;
    }

    [SerializeField] private StageEnergyLedger ledger;
    [SerializeField] private VirginDropPlayerController player;
    [SerializeField] private Transform visualRoot;
    [SerializeField] private Camera sceneCamera;
    [SerializeField] private ThirdPersonFollowCamera followCamera;
    [SerializeField] private Behaviour[] controlsToSuspend;
    [SerializeField] private Form[] forms;
    [SerializeField] private AudioClip evolutionSound;
    [SerializeField] private ParticleSystem effect;
    [SerializeField] private StageEvolutionPortal portal;
    private bool running;

    public void Configure(StageEnergyLedger stageLedger, VirginDropPlayerController stagePlayer,
        Transform visual, Camera camera, ThirdPersonFollowCamera follow, Behaviour[] controls,
        Form[] evolutionForms, StageEvolutionPortal stagePortal, AudioClip sound = null, ParticleSystem particles = null)
    {
        ledger = stageLedger; player = stagePlayer; visualRoot = visual; sceneCamera = camera;
        followCamera = follow; controlsToSuspend = controls; forms = evolutionForms;
        portal = stagePortal; evolutionSound = sound; effect = particles;
    }

    public bool RequestEvolution()
    {
        if (running || ledger == null || !ledger.CanEvolve || player == null || visualRoot == null || sceneCamera == null)
            return false;
        Form form = System.Array.Find(forms ?? System.Array.Empty<Form>(), f => f != null && f.lineage == ledger.CurrentLineage);
        // Never consume progress when a later form is unavailable.
        if (form == null || form.prefab == null)
        {
            Debug.LogWarning($"Evolution model for {ledger.CurrentLineage} is not configured; evolution remains available.", this);
            return false;
        }
        running = true;
        StartCoroutine(Present(form));
        return true;
    }

    private IEnumerator Present(Form form)
    {
        bool playerEnabled = player.enabled;
        bool followEnabled = followCamera != null && followCamera.enabled;
        bool[] previous = new bool[controlsToSuspend != null ? controlsToSuspend.Length : 0];
        for (int i = 0; i < previous.Length; i++)
            if (controlsToSuspend[i] != null) { previous[i] = controlsToSuspend[i].enabled; controlsToSuspend[i].enabled = false; }
        player.SetTouchMoveInput(Vector2.zero);
        player.enabled = false;
        if (followCamera != null) followCamera.enabled = false;
        if (!ledger.TryConfirmEvolution())
        {
            player.enabled = playerEnabled;
            if (followCamera != null) followCamera.enabled = followEnabled;
            for (int i = 0; i < previous.Length; i++) if (controlsToSuspend[i] != null) controlsToSuspend[i].enabled = previous[i];
            running = false;
            yield break;
        }

        Vector3 oldPosition = sceneCamera.transform.position;
        Quaternion oldRotation = sceneCamera.transform.rotation;
        Vector3 target = player.transform.position + Vector3.up * 0.55f;
        Vector3 close = target + player.transform.forward * 2.7f + Vector3.up * 0.35f;
        Quaternion look = Quaternion.LookRotation(target - close);
        float elapsed = 0f;
        while (elapsed < 0.55f)
        {
            elapsed += Time.deltaTime; float t = Mathf.SmoothStep(0f, 1f, elapsed / 0.55f);
            sceneCamera.transform.SetPositionAndRotation(Vector3.Lerp(oldPosition, close, t), Quaternion.Slerp(oldRotation, look, t));
            yield return null;
        }
        if (effect == null) effect = CreateEffect();
        effect.transform.position = player.transform.position + Vector3.up * .45f;
        effect.Play();
        if (evolutionSound == null) evolutionSound = Resources.Load<AudioClip>("Audio/Stage/Impact");
        if (evolutionSound != null) AudioSource.PlayClipAtPoint(evolutionSound, player.transform.position);
        Quaternion initialRotation = visualRoot.localRotation;
        elapsed = 0f; float angle = 0f;
        while (elapsed < 1.2f)
        {
            elapsed += Time.deltaTime; angle += Mathf.Lerp(90f, 1100f, Mathf.Clamp01(elapsed / 1.2f)) * Time.deltaTime;
            visualRoot.localRotation = initialRotation * Quaternion.Euler(0f, angle, 0f); yield return null;
        }
        bool rootIsPlayer = visualRoot == player.transform;
        if (rootIsPlayer) player.transform.localRotation = initialRotation;
        // Imported character FBXs can carry an axis-conversion rotation (the Profane
        // Drop uses one to map Blender's Z-up coordinates to Unity).  Do not replace
        // that rest rotation with an identity/Y rotation: doing so lays the model over
        // or spins it around its own converted axis after the swap.
        Quaternion importedRestRotation = form.prefab.transform.localRotation;
        Vector3 importedRestScale = form.prefab.transform.localScale;
        GameObject next = Instantiate(form.prefab, rootIsPlayer ? player.transform : visualRoot.parent);
        next.transform.localPosition = rootIsPlayer ? Vector3.zero : visualRoot.localPosition;
        // Pre-multiply so the evolution spin is around the player's vertical axis,
        // then retain the FBX's import-axis correction underneath it.
        next.transform.localRotation = rootIsPlayer
            ? Quaternion.Euler(0f, angle, 0f) * importedRestRotation
            : visualRoot.localRotation;
        next.transform.localScale = rootIsPlayer ? importedRestScale : visualRoot.localScale;
        if (rootIsPlayer)
        {
            // Some Stage01 versions place the old renderers on the player root.
            // Do not deactivate that root or the controller and camera target vanish.
            foreach (Renderer renderer in player.GetComponentsInChildren<Renderer>(true))
                if (!renderer.transform.IsChildOf(next.transform)) renderer.enabled = false;
        }
        else visualRoot.gameObject.SetActive(false);
        visualRoot = next.transform;
        if (rootIsPlayer) initialRotation = importedRestRotation;
        elapsed = 0f;
        Quaternion spinStart = visualRoot.localRotation;
        while (elapsed < 0.7f)
        {
            elapsed += Time.deltaTime; float t = Mathf.Clamp01(elapsed / 0.7f);
            visualRoot.localRotation = Quaternion.Slerp(spinStart, initialRotation, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
        visualRoot.localRotation = initialRotation;
        if (effect != null) effect.Stop();
        yield return new WaitForSeconds(0.65f);
        player.Configure(visualRoot, next.GetComponentInChildren<Animator>(), form.walk, form.attack);
        elapsed = 0f;
        while (elapsed < 0.55f)
        {
            elapsed += Time.deltaTime; float t = Mathf.SmoothStep(0f, 1f, elapsed / 0.55f);
            sceneCamera.transform.SetPositionAndRotation(Vector3.Lerp(close, oldPosition, t), Quaternion.Slerp(look, oldRotation, t));
            yield return null;
        }
        if (followCamera != null) followCamera.enabled = followEnabled;
        player.enabled = playerEnabled;
        for (int i = 0; i < previous.Length; i++) if (controlsToSuspend[i] != null) controlsToSuspend[i].enabled = previous[i];
        if (portal != null) portal.Show(form.lineage);
        running = false;
    }

    private ParticleSystem CreateEffect()
    {
        var effectObject = new GameObject("Evolution Sparkles");
        effectObject.transform.SetParent(transform, false);
        var particles = effectObject.AddComponent<ParticleSystem>();
        var main = particles.main;
        main.playOnAwake = false;
        main.loop = true;
        main.startLifetime = .55f;
        main.startSpeed = .4f;
        main.startSize = .12f;
        main.startColor = new Color(1f, .75f, .3f, .85f);
        main.maxParticles = 35;
        var emission = particles.emission;
        emission.rateOverTime = 25;
        var shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = .45f;
        return particles;
    }
}
