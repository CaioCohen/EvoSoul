using UnityEngine;

/// <summary>
/// Plays the short, non-spatial feedback sounds for successful stage interactions.
/// The clips live in Resources so gameplay code can remain scene-independent.
/// </summary>
public static class StageInteractionAudio
{
    private const string AttackClipPath = "Audio/Stage/Impact";
    private const string EatClipPath = "Audio/Stage/fruit_eating";

    private static AudioSource source;
    private static AudioClip attackClip;
    private static AudioClip eatClip;

    public static void PlayAttack() => Play(ref attackClip, AttackClipPath);

    public static void PlayEat() => Play(ref eatClip, EatClipPath);

    private static void Play(ref AudioClip clip, string clipPath)
    {
        if (clip == null)
            clip = Resources.Load<AudioClip>(clipPath);

        if (clip == null)
        {
            Debug.LogWarning($"Stage interaction audio clip was not found at Resources/{clipPath}.");
            return;
        }

        if (source == null)
        {
            GameObject audioObject = new GameObject("Stage Interaction Audio");
            Object.DontDestroyOnLoad(audioObject);
            source = audioObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
        }

        source.PlayOneShot(clip);
    }
}
