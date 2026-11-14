using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
var path="Assets/Art/Models/virgin_drop.fbx";
var preview=UnityEditor.SceneManagement.EditorSceneManager.NewPreviewScene();
var go=Object.Instantiate(UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path));
UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(go,preview);
try {
var animator=go.GetComponent<Animator>();
if(animator==null) animator=go.AddComponent<Animator>();
animator.applyRootMotion=false;
animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
var renderer=go.GetComponentInChildren<SkinnedMeshRenderer>();
var results=new System.Collections.Generic.List<object>();
    foreach(var clip in UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().Where(c=>!c.name.StartsWith("__preview"))) {
        var graph=PlayableGraph.Create("VirginDrop independent clip validation");
        try {
            graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
            var playable=AnimationClipPlayable.Create(graph,clip);
            var output=AnimationPlayableOutput.Create(graph,"Character",animator);
            output.SetSourcePlayable(playable);
            graph.Play();
            var sampleTime=clip.name=="Walk"?7f/30f:clip.name=="Attack"?7f/30f:clip.length;
            playable.SetTime(sampleTime);
            graph.Evaluate(0);
            var weights=Enumerable.Range(0,renderer.sharedMesh.blendShapeCount)
                .Select(i=>new{name=renderer.sharedMesh.GetBlendShapeName(i),weight=renderer.GetBlendShapeWeight(i)}).ToArray();
            if(weights.Max(v=>v.weight)<90) throw new System.Exception("Playable did not animate "+clip.name);
            results.Add(new{clip=clip.name,sampleTime,weights});
        } finally {graph.Destroy();}
    }
    return results.ToArray();
} finally {
    Object.DestroyImmediate(go);
    UnityEditor.SceneManagement.EditorSceneManager.ClosePreviewScene(preview);
}
