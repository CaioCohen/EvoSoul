// Execute through the connected Unity Editor (script eval). Does not edit scenes/gameplay.
if (UnityEngine.Application.dataPath != "D:/games/low-poly-game/unity/EvoSoul/Assets")
    throw new System.Exception("Wrong Unity project");
var path = "Assets/Art/Models/virgin_drop.fbx";
UnityEditor.AssetDatabase.ImportAsset(path, UnityEditor.ImportAssetOptions.ForceSynchronousImport | UnityEditor.ImportAssetOptions.ForceUpdate);
var importer = (UnityEditor.ModelImporter)UnityEditor.AssetImporter.GetAtPath(path);
importer.importBlendShapes = true;
importer.importAnimation = true;
importer.animationType = UnityEditor.ModelImporterAnimationType.Generic;
importer.animationCompression = UnityEditor.ModelImporterAnimationCompression.Off;
importer.importNormals = UnityEditor.ModelImporterNormals.Import;
var takeName = importer.defaultClipAnimations.First().takeName;
importer.clipAnimations = new[] {
    new UnityEditor.ModelImporterClipAnimation { name="Walk", takeName=takeName, firstFrame=0, lastFrame=21, loopTime=true, loopPose=false },
    new UnityEditor.ModelImporterClipAnimation { name="Attack", takeName=takeName, firstFrame=30, lastFrame=45, loopTime=false },
    new UnityEditor.ModelImporterClipAnimation { name="Death", takeName=takeName, firstFrame=60, lastFrame=96, loopTime=false }
};
importer.SaveAndReimport();
return UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path).OfType<UnityEngine.AnimationClip>()
    .Where(c => !c.name.StartsWith("__preview"))
    .Select(c => new {name=c.name, seconds=c.length, loop=c.isLooping,
        blendShapeCurves=UnityEditor.AnimationUtility.GetCurveBindings(c).Count(b=>b.propertyName.StartsWith("blendShape."))})
    .ToArray();
