using UnityEngine;

/// <summary>
/// Gives an available stage fruit a small, deterministic idle motion without
/// moving any of its local children independently (leaf, visual or collider).
/// Attach this to the fruit target root so the complete interactive object
/// keeps its relative layout while bobbing and rotating.
/// </summary>
public sealed class StageFruitIdleMotion : MonoBehaviour
{
    [SerializeField, Min(0f)] private float rotationDegreesPerSecond = 24f;
    [SerializeField, Min(0f)] private float bobAmplitude = 0.075f;
    [SerializeField, Min(0.01f)] private float bobFrequency = 1.2f;

    private Vector3 initialLocalPosition;
    private Quaternion initialLocalRotation;
    private float phase;

    private void Awake()
    {
        initialLocalPosition = transform.localPosition;
        initialLocalRotation = transform.localRotation;
        phase = Mathf.Repeat((transform.name.GetHashCode() & int.MaxValue) * 0.0001f, Mathf.PI * 2f);
    }

    private void OnEnable()
    {
        initialLocalPosition = transform.localPosition;
        initialLocalRotation = transform.localRotation;
    }

    private void Update()
    {
        float time = Time.time;
        float bob = Mathf.Sin(time * bobFrequency + phase) * bobAmplitude;
        transform.localPosition = initialLocalPosition + Vector3.up * bob;
        transform.localRotation = initialLocalRotation * Quaternion.Euler(0f, time * rotationDegreesPerSecond, 0f);
    }

    private void OnDisable()
    {
        transform.localPosition = initialLocalPosition;
        transform.localRotation = initialLocalRotation;
    }
}
