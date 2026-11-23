using UnityEngine;

public static class ProfaneRespawnTiming
{
    // Box-Muller normal sample; a negative duration is clamped to zero.
    public static float Sample(float meanSeconds, float standardDeviationSeconds)
    {
        float u1 = Mathf.Max(Random.value, 1e-7f);
        float u2 = Random.value;
        float normal = Mathf.Sqrt(-2f * Mathf.Log(u1)) * Mathf.Cos(2f * Mathf.PI * u2);
        return Mathf.Max(0f, meanSeconds + Mathf.Max(0f, standardDeviationSeconds) * normal);
    }
}
