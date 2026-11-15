using UnityEngine;

/// <summary>Small mobile-safe toggle for the optional development HUD.</summary>
public sealed class StageDebugToggleButton : MonoBehaviour
{
    [SerializeField] private StageDebugProgressView debugView;

    public void Configure(StageDebugProgressView target) => debugView = target;

    public void Toggle()
    {
        if (debugView != null)
            debugView.SetVisible(!debugView.IsVisible);
    }
}
