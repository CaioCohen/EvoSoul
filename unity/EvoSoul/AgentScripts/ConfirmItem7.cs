using System.Linq;
using UnityEngine;

var world = GameObject.Find("BercarioWorld");
var covers = world.transform.Find("TerrainSeamCovers");
var remoteFruits = Object.FindObjectsByType<StageFruitTarget>(FindObjectsInactive.Include, FindObjectsSortMode.None)
    .Count(target => Mathf.Abs(target.transform.position.x) > 16f || Mathf.Abs(target.transform.position.z) > 12f);
var remoteNpcs = Object.FindObjectsByType<StageDropletNpcTarget>(FindObjectsInactive.Include, FindObjectsSortMode.None)
    .Count(target => Mathf.Abs(target.transform.position.x) > 16f || Mathf.Abs(target.transform.position.z) > 12f);
return new {
    covers = covers == null ? 0 : covers.childCount,
    coverColliders = covers == null ? -1 : covers.GetComponentsInChildren<Collider>(true).Length,
    remoteFruits,
    remoteNpcs
};
