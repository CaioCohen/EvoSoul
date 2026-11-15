using UnityEditor;
using UnityEngine;

AssetDatabase.ImportAsset(
    "Assets/Art/Models/Environment/BercarioEnvironment.fbx",
    ImportAssetOptions.ForceUpdate);

return new
{
    exists = AssetDatabase.LoadAssetAtPath<GameObject>(
        "Assets/Art/Models/Environment/BercarioEnvironment.fbx") != null
};
