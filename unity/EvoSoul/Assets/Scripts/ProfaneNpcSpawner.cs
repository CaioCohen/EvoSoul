using System.Collections.Generic;
using UnityEngine;

/// <summary>Replenishes each defeated NPC after a normally distributed delay.</summary>
public sealed class ProfaneNpcSpawner : MonoBehaviour
{
    [SerializeField] private GameObject npcTemplate;
    [SerializeField, Range(1, 10)] private int population = 10;
    [SerializeField] private float radius = 10f;
    [SerializeField, Min(0f)] private float meanRespawnSeconds = 600f;
    [SerializeField, Min(0f)] private float respawnStandardDeviation = 30f;
    private readonly List<GameObject> alive = new();
    private readonly Dictionary<GameObject, float> deadlines = new();
    private bool initialized;

    public void Configure(GameObject template, int activePopulation, float spawnRadius)
    {
        npcTemplate = template;
        population = Mathf.Clamp(activePopulation, 1, 10);
        radius = Mathf.Max(1f, spawnRadius);
    }

    private void Update()
    {
        if (npcTemplate == null) return;
        if (!initialized)
        {
            initialized = true;
            while (alive.Count < population) Spawn();
        }
        for (int i = alive.Count - 1; i >= 0; i--)
        {
            var npc = alive[i];
            if (npc == null) { alive.RemoveAt(i); continue; }
            var target = npc.GetComponent<StageDropletNpcTarget>();
            if (target != null && target.LifecycleState != DropletNpcLifecycleState.Wandering &&
                !deadlines.ContainsKey(npc))
                deadlines.Add(npc, Time.time + ProfaneRespawnTiming.Sample(meanRespawnSeconds, respawnStandardDeviation));
            if (!deadlines.TryGetValue(npc, out float deadline) || Time.time < deadline || npc.activeSelf)
                continue;
            deadlines.Remove(npc);
            alive.RemoveAt(i);
            Destroy(npc);
            if (alive.Count < population) Spawn();
        }
    }

    private void Spawn()
    {
        Vector2 point = Random.insideUnitCircle * radius;
        var npc = Instantiate(npcTemplate, transform);
        npc.transform.position = transform.position + new Vector3(point.x, 0f, point.y);
        npc.name = "ProfaneWanderer";
        npc.SetActive(true);
        alive.Add(npc);
    }
}
