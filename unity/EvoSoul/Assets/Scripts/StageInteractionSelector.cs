using System;
using System.Collections.Generic;
using UnityEngine;

public readonly struct StageInteractionCandidate
{
    public IStageInteractionTarget Target { get; }
    public Vector3 Position { get; }

    public StageInteractionCandidate(IStageInteractionTarget target, Vector3 position)
    {
        Target = target;
        Position = position;
    }
}

public readonly struct StageInteractionSelection
{
    public IStageInteractionTarget Target { get; }
    public StageInteractionAction Action { get; }
    public float ForwardDot { get; }
    public float Distance { get; }

    public bool IsValid => Target != null && Action != StageInteractionAction.None;

    public StageInteractionSelection(IStageInteractionTarget target, float forwardDot, float distance)
    {
        Target = target;
        Action = target != null ? target.InteractionAction : StageInteractionAction.None;
        ForwardDot = forwardDot;
        Distance = distance;
    }

    public static StageInteractionSelection None => new(null, 0f, 0f);
}

/// <summary>
/// Chooses exactly one available interaction target in front of the player.
/// UI and gameplay consume SelectionChanged instead of duplicating selection rules.
/// </summary>
public sealed class StageInteractionSelector : MonoBehaviour
{
    [SerializeField] private Transform selectionOrigin;
    [SerializeField] private LayerMask candidateLayers = ~0;
    [SerializeField, Min(0.01f)] private float interactionDistance = 2.25f;
    [SerializeField, Range(-1f, 1f)] private float minimumForwardDot = 0.55f;
    [SerializeField, Min(1)] private int maxColliderCandidates = 32;

    private Collider[] colliderBuffer;
    private readonly List<StageInteractionCandidate> candidates = new();

    public StageInteractionSelection CurrentSelection { get; private set; }
    public IStageInteractionTarget CurrentTarget => CurrentSelection.Target;
    public StageInteractionAction CurrentAction => CurrentSelection.Action;

    public event Action<StageInteractionSelection> SelectionChanged;

    private void Awake()
    {
        if (selectionOrigin == null)
            selectionOrigin = transform;
        EnsureColliderBuffer();
    }

    private void OnValidate()
    {
        interactionDistance = Mathf.Max(0.01f, interactionDistance);
        maxColliderCandidates = Mathf.Max(1, maxColliderCandidates);
        EnsureColliderBuffer();
    }

    private void Update()
    {
        RefreshSelection();
    }

    public void RefreshSelection()
    {
        if (selectionOrigin == null)
            selectionOrigin = transform;

        EnsureColliderBuffer();
        candidates.Clear();

        int colliderCount = Physics.OverlapSphereNonAlloc(
            selectionOrigin.position,
            interactionDistance,
            colliderBuffer,
            candidateLayers,
            QueryTriggerInteraction.Collide);

        for (int index = 0; index < colliderCount; index++)
        {
            StageInteractionTarget target = colliderBuffer[index].GetComponentInParent<StageInteractionTarget>();
            if (target != null)
                candidates.Add(new StageInteractionCandidate(target, target.transform.position));
        }

        TrySelectBest(
            selectionOrigin.position,
            selectionOrigin.forward,
            interactionDistance,
            minimumForwardDot,
            candidates,
            out StageInteractionSelection nextSelection);

        if (HasSameTarget(CurrentSelection, nextSelection))
        {
            CurrentSelection = nextSelection;
            return;
        }

        CurrentSelection = nextSelection;
        SelectionChanged?.Invoke(CurrentSelection);
    }

    public static bool TrySelectBest(
        Vector3 origin,
        Vector3 forward,
        float maximumDistance,
        float minimumDot,
        IReadOnlyList<StageInteractionCandidate> availableCandidates,
        out StageInteractionSelection selection)
    {
        selection = StageInteractionSelection.None;
        if (availableCandidates == null || maximumDistance <= 0f)
            return false;

        origin.y = 0f;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.0001f)
            forward = Vector3.forward;
        else
            forward.Normalize();

        float maximumDistanceSqr = maximumDistance * maximumDistance;
        float bestDot = float.NegativeInfinity;
        float bestDistanceSqr = float.PositiveInfinity;
        IStageInteractionTarget bestTarget = null;

        for (int index = 0; index < availableCandidates.Count; index++)
        {
            StageInteractionCandidate candidate = availableCandidates[index];
            if (candidate.Target == null || !candidate.Target.IsAvailable || candidate.Target.InteractionAction == StageInteractionAction.None)
                continue;

            Vector3 direction = candidate.Position;
            direction.y = 0f;
            direction -= origin;
            float distanceSqr = direction.sqrMagnitude;
            if (distanceSqr > maximumDistanceSqr)
                continue;

            float forwardDot = distanceSqr < 0.0001f
                ? 1f
                : Vector3.Dot(forward, direction / Mathf.Sqrt(distanceSqr));
            if (forwardDot < minimumDot)
                continue;

            bool isMoreAligned = forwardDot > bestDot + 0.0001f;
            bool isEquallyAlignedAndCloser = Mathf.Abs(forwardDot - bestDot) <= 0.0001f && distanceSqr < bestDistanceSqr;
            if (!isMoreAligned && !isEquallyAlignedAndCloser)
                continue;

            bestTarget = candidate.Target;
            bestDot = forwardDot;
            bestDistanceSqr = distanceSqr;
        }

        if (bestTarget == null)
            return false;

        selection = new StageInteractionSelection(bestTarget, bestDot, Mathf.Sqrt(bestDistanceSqr));
        return true;
    }

    private void EnsureColliderBuffer()
    {
        if (colliderBuffer == null || colliderBuffer.Length != maxColliderCandidates)
            colliderBuffer = new Collider[maxColliderCandidates];
    }

    private static bool HasSameTarget(StageInteractionSelection first, StageInteractionSelection second)
    {
        return ReferenceEquals(first.Target, second.Target);
    }

    private void OnDrawGizmosSelected()
    {
        Transform origin = selectionOrigin != null ? selectionOrigin : transform;
        Gizmos.color = CurrentSelection.IsValid ? Color.yellow : Color.white;
        Gizmos.DrawWireSphere(origin.position, interactionDistance);
    }
}
