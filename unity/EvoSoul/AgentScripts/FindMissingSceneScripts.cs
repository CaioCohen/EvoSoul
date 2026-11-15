using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class FindMissingSceneScripts
{
    public static object Run()
    {
        var missing = new List<string>();
        foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
            Collect(root.transform, root.name, missing);
        return missing.ToArray();
    }

    private static void Collect(Transform node, string path, List<string> missing)
    {
        Component[] components = node.GetComponents<Component>();
        for (int index = 0; index < components.Length; index++)
            if (components[index] == null)
                missing.Add(path + " component index " + index);

        for (int index = 0; index < node.childCount; index++)
        {
            Transform child = node.GetChild(index);
            Collect(child, path + "/" + child.name, missing);
        }
    }
}
