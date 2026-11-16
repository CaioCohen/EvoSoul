using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class InspectStage01
{
    public static object Run()
    {
        var scene = SceneManager.GetActiveScene();
        var roots = scene.GetRootGameObjects();
        var lines = new System.Collections.Generic.List<string>();
        for (var index = 0; index < roots.Length; index++)
        {
            var root = roots[index];
            AddTree(root.transform, 0, lines);
        }

        return new { scene = scene.path, roots = lines };
    }

    private static void AddTree(Transform item, int depth, System.Collections.Generic.List<string> lines)
    {
        var components = item.GetComponents<Component>();
        var names = new System.Collections.Generic.List<string>();
        foreach (var component in components)
        {
            if (component != null)
                names.Add(component.GetType().Name);
        }

        lines.Add($"{new string(' ', depth * 2)}{item.name}|pos={item.position}|scale={item.lossyScale}|components={string.Join(',', names)}");
        for (var index = 0; index < item.childCount; index++)
            AddTree(item.GetChild(index), depth + 1, lines);
    }
}
