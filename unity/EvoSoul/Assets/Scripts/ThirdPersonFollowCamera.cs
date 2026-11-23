using UnityEngine;

public sealed class ThirdPersonFollowCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 3.2f, -6f);
    [SerializeField] private float followSharpness = 12f;
    // Kept intentionally gentle for a readable third-person follow on touch.
    [SerializeField, Min(0.01f)] private float yawFollowSharpness = 2.75f;
    [SerializeField] private float lookHeight = 1.15f;

    private float smoothedYaw;
    private bool hasYaw;

    public void Configure(Transform followTarget)
    {
        target = followTarget;
        hasYaw = false;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // Orbit the offset around the player's horizontal facing direction.
        // A separate yaw damping gives the third-person camera a small,
        // deliberate follow delay without allowing it to stay beside the
        // character after a turn.
        float desiredYaw = target.eulerAngles.y;
        if (!hasYaw)
        {
            smoothedYaw = desiredYaw;
            hasYaw = true;
        }
        smoothedYaw = Mathf.LerpAngle(smoothedYaw, desiredYaw,
            1f - Mathf.Exp(-yawFollowSharpness * Time.deltaTime));

        Vector3 desired = target.position + Quaternion.Euler(0f, smoothedYaw, 0f) * offset;
        transform.position = Vector3.Lerp(transform.position, desired, 1f - Mathf.Exp(-followSharpness * Time.deltaTime));
        transform.LookAt(target.position + Vector3.up * lookHeight);
    }
}
