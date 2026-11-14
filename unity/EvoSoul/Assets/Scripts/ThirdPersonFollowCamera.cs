using UnityEngine;

public sealed class ThirdPersonFollowCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 3.2f, -6f);
    [SerializeField] private float followSharpness = 12f;
    [SerializeField] private float lookHeight = 1.15f;

    public void Configure(Transform followTarget)
    {
        target = followTarget;
    }

    private void LateUpdate()
    {
        if (target == null) return;
        Vector3 desired = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desired, 1f - Mathf.Exp(-followSharpness * Time.deltaTime));
        transform.LookAt(target.position + Vector3.up * lookHeight);
    }
}
