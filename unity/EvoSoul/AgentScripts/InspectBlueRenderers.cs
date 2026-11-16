using System.Linq;
using UnityEngine;

var world = GameObject.Find("BercarioWorld");
return string.Join("\n", world.GetComponentsInChildren<Renderer>(true)
    .Where(r => r.sharedMaterial != null && r.sharedMaterial.name.ToLower().Contains("ocean"))
    .Select(r => $"{r.name} p={r.transform.position} b={r.bounds.min}/{r.bounds.max} mat={r.sharedMaterial.name}"));
