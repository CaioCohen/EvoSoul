using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

var material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Environment/Ocean/BercarioOceanWater.mat");
return new
{
    material = material != null ? material.name : null,
    shader = material != null && material.shader != null ? material.shader.name : null,
    properties = material != null && material.shader != null
        ? Enumerable.Range(0, material.shader.GetPropertyCount())
            .Select(index => new
            {
                name = material.shader.GetPropertyName(index),
                type = material.shader.GetPropertyType(index).ToString(),
            })
            .ToArray()
        : null,
    scene = SceneManager.GetActiveScene().path,
};
