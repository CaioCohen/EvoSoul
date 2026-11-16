using UnityEngine;
using System.Reflection;

var npc = Object.FindFirstObjectByType<StageDropletNpcTarget>();
var ledger = Object.FindFirstObjectByType<StageEnergyLedger>();
if (npc == null || ledger == null)
    throw new System.Exception("NPC or ledger not found");
npc.gameObject.name = "ValidationDeathNPC";
typeof(StageDropletNpcTarget)
    .GetField("puddleSeconds", BindingFlags.Instance | BindingFlags.NonPublic)
    ?.SetValue(npc, 8f);
if (!npc.TryReserveInteraction())
    throw new System.Exception("NPC not reservable");
if (!ledger.TryAward(npc.RewardSourceId, StageEnergyKind.Negative))
    throw new System.Exception("Reward failed");
if (!npc.TryClaimReward())
    throw new System.Exception("Claim failed");
if (!npc.BeginDefeat(0f))
    throw new System.Exception("Defeat did not start");

return new { state = npc.LifecycleState.ToString(), active = npc.gameObject.activeSelf };
