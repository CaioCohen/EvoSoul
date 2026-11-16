using System.Linq;
using UnityEngine;

var world = GameObject.Find("BercarioWorld");
var terrains = world.GetComponentsInChildren<MeshRenderer>(true).Where(r => r.gameObject.name == "GEO-ENV-Terrain").ToArray();
var oceans = Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None).Where(r => r.name.ToLower().Contains("ocean")).ToArray();
return string.Join("\n", terrains.Take(3).Select(r => $"terrain {r.transform.position} bounds {r.bounds.min} {r.bounds.max}").Concat(oceans.Select(r => $"ocean {r.name} {r.transform.position} bounds {r.bounds.min} {r.bounds.max}")));
