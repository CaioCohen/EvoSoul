using UnityEditor;
using UnityEngine;

public static class RemoveMissingMobileAttackScripts
{
    public static object Run()
    {
        GameObject target = GameObject.Find("MobileAttackPreview");
        if (target == null)
            return new { removed = false, reason = "target not found" };

        var serialized = new SerializedObject(target);
        SerializedProperty components = serialized.FindProperty("m_Component");
        int removed = 0;
        for (int index = components.arraySize - 1; index >= 0; index--)
        {
            SerializedProperty entry = components.GetArrayElementAtIndex(index);
            if (entry.FindPropertyRelative("component").objectReferenceValue != null)
                continue;
            components.DeleteArrayElementAtIndex(index);
            removed++;
        }
        serialized.ApplyModifiedProperties();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(target.scene);
        return new { removed };
    }
}
