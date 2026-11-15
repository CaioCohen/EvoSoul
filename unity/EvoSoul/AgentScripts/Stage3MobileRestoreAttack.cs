using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

public static class Stage3MobileRestoreAttack
{
    public static object Run()
    {
        var player = Object.FindAnyObjectByType<VirginDropPlayerController>();
        var buttonObject = GameObject.Find("MobileAttackPreview");
        if (player == null || buttonObject == null)
            throw new System.InvalidOperationException("Missing player or mobile attack button.");
        var attack = buttonObject.GetComponent<MobileAttackButton>() ?? buttonObject.AddComponent<MobileAttackButton>();
        attack.Configure(player);
        var scene = SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        return new { component = attack.GetType().Name, scene = scene.path };
    }
}
