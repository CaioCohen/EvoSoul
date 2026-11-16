using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A peaceful nursery droplet. It wanders slowly inside a configurable radius,
/// never pursues the player, and can only complete one defeat/reward sequence.
/// </summary>
public sealed class StageDropletNpcTarget : StageInteractionTarget
{
    [Header("Wandering")]
    [SerializeField] private Transform wanderCenter;
    [SerializeField] private Collider groundCollider;
    [SerializeField, Min(0.1f)] private float wanderRadius = 3f;
    [SerializeField, Min(0.01f)] private float wanderSpeed = 1.1f;
    [SerializeField, Min(0f)] private float minimumIdleSeconds = 0.6f;
    [SerializeField, Min(0f)] private float maximumIdleSeconds = 1.5f;

    [Header("Defeat presentation")]
    [SerializeField] private Transform visualRoot;
    [SerializeField] private GameObject puddleVisual;
    [SerializeField] private AnimationClip walkClip;
    [SerializeField] private AnimationClip deathClip;
    [SerializeField] private Renderer[] fadeRenderers;
    [SerializeField, Min(0f)] private float puddleSeconds = 2f;
    [SerializeField, Min(0.01f)] private float fadeSeconds = 1f;
    [SerializeField] private UnityEvent defeated;
    [SerializeField] private UnityEvent removed;

    private Vector3 nextWanderPosition;
    private Vector3 fallbackWanderCenter;
    private float idleUntil;
    private float walkTime;
    private Coroutine defeatRoutine;

    public override StageInteractionAction InteractionAction => StageInteractionAction.Attack;
    public DropletNpcLifecycleState LifecycleState { get; private set; } = DropletNpcLifecycleState.Wandering;
    public float WanderSpeed => wanderSpeed;

    /// <summary>
    /// Configures the reusable Virgin Drop visuals from the scene builder while
    /// keeping this behaviour independent from any particular prefab layout.
    /// </summary>
    public void ConfigurePresentation(Transform targetVisualRoot, GameObject targetPuddle, AnimationClip walk, AnimationClip death)
    {
        visualRoot = targetVisualRoot;
        puddleVisual = targetPuddle;
        // The imported death clip already ends in the intended puddle-like
        // silhouette. A legacy primitive puddle was a black disk, so it must
        // never replace the final animated frame.
        if (puddleVisual != null)
            puddleVisual.SetActive(false);
        walkClip = walk;
        deathClip = death;
    }

    public void ConfigureGroundCollider(Collider terrainCollider)
    {
        groundCollider = terrainCollider;
        SnapToGround();
    }

    protected override void Awake()
    {
        base.Awake();
        // Do not fall back to transform as a live center: this NPC moves its
        // own transform, so doing that would let the wandering radius drift.
        fallbackWanderCenter = transform.position;
        if (visualRoot == null)
            visualRoot = transform;
        if (puddleVisual != null)
            puddleVisual.SetActive(false);
        if (fadeRenderers == null || fadeRenderers.Length == 0)
            fadeRenderers = GetComponentsInChildren<Renderer>(true);
        SnapToGround();
        PickNextWanderPosition();
    }

    private void Update()
    {
        if (LifecycleState != DropletNpcLifecycleState.Wandering)
            return;

        // Even while idling, retain contact with the terrain after the last
        // evaluated walk pose; otherwise a blendshape bounds change can leave
        // the droplet visually hovering until it picks its next destination.
        SnapToGround();

        if (Time.time < idleUntil)
            return;

        Vector3 current = transform.position;
        Vector3 destination = nextWanderPosition;
        destination.y = current.y;
        transform.position = Vector3.MoveTowards(current, destination, wanderSpeed * Time.deltaTime);
        SnapToGround();

        Vector3 direction = destination - current;
        direction.y = 0f;
        if (direction.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(direction), 240f * Time.deltaTime);
            if (walkClip != null && visualRoot != null)
            {
                walkTime = Mathf.Repeat(walkTime + Time.deltaTime, walkClip.length);
                SampleVisualClip(walkClip, walkTime);
            }
        }
        else if (walkClip != null && visualRoot != null)
        {
            walkTime = 0f;
            SampleVisualClip(walkClip, 0f);
        }

        // Walk blendshapes can change the evaluated renderer bounds. Snap a
        // second time after sampling so the animated droplet never appears to
        // float while wandering across uneven terrain.
        SnapToGround();

        if ((transform.position - destination).sqrMagnitude <= 0.0025f)
        {
            idleUntil = Time.time + Random.Range(minimumIdleSeconds, Mathf.Max(minimumIdleSeconds, maximumIdleSeconds));
            PickNextWanderPosition();
        }
    }

    /// <summary>
    /// Starts the only valid defeat path. The caller must already have claimed
    /// the target reward, which makes the NPC unavailable immediately.
    /// </summary>
    public bool BeginDefeat(float attackImpactDelay)
    {
        if (LifecycleState != DropletNpcLifecycleState.Wandering || !HasClaimedReward)
            return false;

        LifecycleState = DropletNpcLifecycleState.Hit;
        if (defeatRoutine != null)
            return false;
        defeatRoutine = StartCoroutine(DefeatSequence(Mathf.Max(0f, attackImpactDelay)));
        return true;
    }

    private IEnumerator DefeatSequence(float attackImpactDelay)
    {
        if (attackImpactDelay > 0f)
            yield return new WaitForSeconds(attackImpactDelay);

        LifecycleState = DropletNpcLifecycleState.Dying;
        defeated?.Invoke();

        if (deathClip != null && visualRoot != null)
        {
            float time = 0f;
            while (time < deathClip.length)
            {
                SampleVisualClip(deathClip, time);
                time += Time.deltaTime;
                yield return null;
            }
            // Sample exactly the final pose once; the death animation never loops.
            SampleVisualClip(deathClip, deathClip.length);
        }

        LifecycleState = DropletNpcLifecycleState.Puddle;
        // Keep the exact final death pose visible throughout the puddle hold.
        // Do not activate the legacy primitive puddle: it renders as an
        // unrelated black disk under URP lighting.
        if (puddleVisual != null)
            puddleVisual.SetActive(false);
        if (puddleSeconds > 0f)
            yield return new WaitForSeconds(puddleSeconds);

        yield return FadeOut();
        LifecycleState = DropletNpcLifecycleState.Removed;
        removed?.Invoke();
        gameObject.SetActive(false);
    }

    private IEnumerator FadeOut()
    {
        if (fadeRenderers == null || fadeRenderers.Length == 0)
            yield break;

        Material[][] materials = new Material[fadeRenderers.Length][];
        Color[][] baseColors = new Color[fadeRenderers.Length][];
        for (int rendererIndex = 0; rendererIndex < fadeRenderers.Length; rendererIndex++)
        {
            Renderer renderer = fadeRenderers[rendererIndex];
            if (renderer == null || !renderer.gameObject.activeInHierarchy)
                continue;
            materials[rendererIndex] = renderer.materials;
            baseColors[rendererIndex] = new Color[materials[rendererIndex].Length];
            for (int materialIndex = 0; materialIndex < materials[rendererIndex].Length; materialIndex++)
            {
                Material material = materials[rendererIndex][materialIndex];
                ConfigureTransparent(material);
                baseColors[rendererIndex][materialIndex] = ReadColor(material);
            }
        }

        float elapsed = 0f;
        while (elapsed < fadeSeconds)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - Mathf.Clamp01(elapsed / fadeSeconds);
            for (int rendererIndex = 0; rendererIndex < materials.Length; rendererIndex++)
            {
                if (materials[rendererIndex] == null)
                    continue;
                for (int materialIndex = 0; materialIndex < materials[rendererIndex].Length; materialIndex++)
                {
                    Color color = baseColors[rendererIndex][materialIndex];
                    color.a *= alpha;
                    WriteColor(materials[rendererIndex][materialIndex], color);
                }
            }
            yield return null;
        }
    }

    private void PickNextWanderPosition()
    {
        Vector2 offset = Random.insideUnitCircle * wanderRadius;
        Vector3 center = wanderCenter != null ? wanderCenter.position : fallbackWanderCenter;
        nextWanderPosition = new Vector3(center.x + offset.x, transform.position.y, center.z + offset.y);
    }

    private void SnapToGround()
    {
        if (groundCollider == null || visualRoot == null)
            return;

        Ray ray = new Ray(new Vector3(transform.position.x, 100f, transform.position.z), Vector3.down);
        if (!groundCollider.Raycast(ray, out RaycastHit hit, 200f))
            return;

        Renderer renderer = visualRoot.GetComponentInChildren<Renderer>(true);
        if (renderer == null)
            return;

        // Do not use the cached offset here: an evaluated walk/death pose may
        // have a different bounds minimum from the idle pose.
        float evaluatedVisualOffset = renderer.bounds.min.y - transform.position.y;
        Vector3 position = transform.position;
        position.y = hit.point.y - evaluatedVisualOffset;
        transform.position = position;
    }

    /// <summary>
    /// Imported clips have root transform curves. The NPC root owns world
    /// motion, so preserve the visual's local placement and rotation while
    /// retaining animated scale/deformation from the clip.
    /// </summary>
    private void SampleVisualClip(AnimationClip clip, float time)
    {
        if (clip == null || visualRoot == null)
            return;

        Vector3 localPosition = visualRoot.localPosition;
        Quaternion localRotation = visualRoot.localRotation;
        clip.SampleAnimation(visualRoot.gameObject, time);
        visualRoot.SetLocalPositionAndRotation(localPosition, localRotation);
    }

    private static void ConfigureTransparent(Material material)
    {
        if (material == null)
            return;
        if (material.HasProperty("_Surface"))
            material.SetFloat("_Surface", 1f);
        if (material.HasProperty("_Blend"))
            material.SetFloat("_Blend", 0f);
        material.SetOverrideTag("RenderType", "Transparent");
        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
    }

    private static Color ReadColor(Material material)
    {
        if (material != null && material.HasProperty("_BaseColor"))
            return material.GetColor("_BaseColor");
        if (material != null && material.HasProperty("_Color"))
            return material.GetColor("_Color");
        return Color.white;
    }

    private static void WriteColor(Material material, Color color)
    {
        if (material == null)
            return;
        if (material.HasProperty("_BaseColor"))
            material.SetColor("_BaseColor", color);
        if (material.HasProperty("_Color"))
            material.SetColor("_Color", color);
    }
}
