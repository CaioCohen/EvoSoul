using UnityEngine;

public static class CaptureSeamCheck
{
    public static object Capture()
    {
        var path = "AgentScripts/seam_check.png";
        ScreenCapture.CaptureScreenshot(path, 1280);
        return new { path, camera = Camera.main != null ? Camera.main.name : "none" };
    }
}
