// Non-destructive playback sampling in a temporary preview scene; no gameplay changes.
var path = "Assets/Art/Models/virgin_drop.fbx";
var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>(path);
var clips = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path).OfType<UnityEngine.AnimationClip>()
    .Where(c=>!c.name.StartsWith("__preview")).ToDictionary(c=>c.name);
if (clips.Count!=3 || !clips.ContainsKey("Walk") || !clips.ContainsKey("Attack") || !clips.ContainsKey("Death"))
    throw new System.Exception("Missing or extra exported clips");
var dirtyBefore = UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty;
var preview = UnityEditor.SceneManagement.EditorSceneManager.NewPreviewScene();
var go = UnityEngine.Object.Instantiate(prefab);
UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(go,preview);
var r = go.GetComponentInChildren<UnityEngine.SkinnedMeshRenderer>();
var baked = new UnityEngine.Mesh();
var records = new System.Collections.Generic.List<object>();
UnityEngine.Vector3[] Sample(UnityEngine.AnimationClip clip, float time) {
    clip.SampleAnimation(go,time);
    r.BakeMesh(baked);
    return baked.vertices.Select(v=>r.transform.TransformPoint(v)).ToArray();
}
float Delta(UnityEngine.Vector3[] a, UnityEngine.Vector3[] b) {
    return a.Zip(b,(x,y)=>(x-y).magnitude).Max();
}
try {
    var neutral = Sample(clips["Walk"],0);
    foreach(var name in new[]{"Walk","Attack","Death"}) {
        var clip=clips[name];
        var expected = name=="Walk"?.7f:name=="Attack"?.5f:1.2f;
        if(UnityEngine.Mathf.Abs(clip.length-expected)>.001f) throw new System.Exception("Wrong duration "+name);
        var first=Sample(clip,0); var end=Sample(clip,clip.length);
        var beginError=Delta(first,neutral); var endError=Delta(end,neutral);
        var minY=0f; var maximumMovement=0f;
        for(int i=0;i<=60;i++) {
            var v=Sample(clip,clip.length*i/60f);
            minY=UnityEngine.Mathf.Min(minY,v.Min(p=>p.y));
            maximumMovement=UnityEngine.Mathf.Max(maximumMovement,Delta(v,neutral));
        }
        if(beginError>.0001f || minY<-.0001f || maximumMovement<.01f) throw new System.Exception("Invalid pose/motion "+name);
        if(name!="Death" && endError>.0001f) throw new System.Exception("Neutral return failed "+name);
        var height=end.Max(v=>v.y)-end.Min(v=>v.y);
        var heldDelta=Delta(end,Sample(clip,clip.length-.1f));
        if(name=="Death" && (height>.06f || heldDelta>.0001f)) throw new System.Exception("Unstable or thick puddle");
        records.Add(new {name,seconds=clip.length,loop=clip.isLooping,beginError,endError,minY,maximumMovement,
            endHeight=height,heldDelta,curves=UnityEditor.AnimationUtility.GetCurveBindings(clip)
            .Where(b=>b.propertyName.StartsWith("blendShape.")).Select(b=>new {
                name=b.propertyName,min=UnityEditor.AnimationUtility.GetEditorCurve(clip,b).keys.Min(k=>k.value),
                max=UnityEditor.AnimationUtility.GetEditorCurve(clip,b).keys.Max(k=>k.value)}).ToArray()});
    }
    // Test that any clip resets a previous Death pose, independent of Animator transition defaults.
    Sample(clips["Death"],clips["Death"].length);
    var deathToWalk=Delta(Sample(clips["Walk"],0),neutral);
    Sample(clips["Death"],clips["Death"].length);
    var deathToAttack=Delta(Sample(clips["Attack"],0),neutral);
    if(deathToWalk>.0001f || deathToAttack>.0001f) throw new System.Exception("Clip state leaks after Death");
    return new {
        clips=records.ToArray(), deathToWalk, deathToAttack, shapes=r.sharedMesh.blendShapeCount,
        triangles=Enumerable.Range(0,r.sharedMesh.subMeshCount).Sum(i=>(long)r.sharedMesh.GetIndexCount(i))/3,
        boneCount=r.bones.Length, materials=r.sharedMaterials.Select(m=>m.name).ToArray(),
        scale=go.transform.localScale.ToString(),rotation=go.transform.localEulerAngles.ToString(),
        localBounds=r.localBounds.ToString(),sceneDirtyBefore=dirtyBefore
    };
}
finally {
    UnityEngine.Object.DestroyImmediate(baked);
    UnityEngine.Object.DestroyImmediate(go);
    UnityEditor.SceneManagement.EditorSceneManager.ClosePreviewScene(preview);
    if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty!=dirtyBefore)
        throw new System.Exception("Unexpected scene dirty change");
}
