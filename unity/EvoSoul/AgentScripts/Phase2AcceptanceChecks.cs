using System;
using System.Collections.Generic;
using UnityEngine;

public static class Phase2AcceptanceChecks
{
    public static object Run()
    {
        var results = new List<string>();

        ValidateSelection(results);
        ValidateFruitReward(results);
        ValidateNpcRewardReservation(results);
        ValidateEnergyAndEvolution(results);

        return new { status = "passed", cases = results };
    }

    private static void ValidateSelection(List<string> results)
    {
        var fruitObject = new GameObject("AcceptanceFruit");
        var npcObject = new GameObject("AcceptanceNpc");
        try
        {
            var fruit = fruitObject.AddComponent<StageFruitTarget>();
            var npc = npcObject.AddComponent<StageDropletNpcTarget>();
            var origin = Vector3.zero;
            var forward = Vector3.forward;

            bool selectedBehind = StageInteractionSelector.TrySelectBest(
                origin, forward, 3f, 0.55f,
                new List<StageInteractionCandidate>
                {
                    new(fruit, new Vector3(0f, 0f, -1f))
                },
                out _);
            Require(!selectedBehind, "A target behind the player was selected.");

            bool selectedFront = StageInteractionSelector.TrySelectBest(
                origin, forward, 3f, 0.55f,
                new List<StageInteractionCandidate>
                {
                    new(fruit, new Vector3(0f, 0f, 2f)),
                    new(npc, new Vector3(0f, 0f, 1f))
                },
                out StageInteractionSelection selection);
            Require(selectedFront && ReferenceEquals(selection.Target, npc) && selection.Action == StageInteractionAction.Attack,
                "The closest equally aligned front target was not selected as the only action.");
            results.Add("selection: behind rejected; alignment then distance chooses one action");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(fruitObject);
            UnityEngine.Object.DestroyImmediate(npcObject);
        }
    }

    private static void ValidateFruitReward(List<string> results)
    {
        var ledgerObject = new GameObject("AcceptanceFruitLedger");
        var fruitObject = new GameObject("AcceptanceFruit");
        try
        {
            var ledger = ledgerObject.AddComponent<StageEnergyLedger>();
            var fruit = fruitObject.AddComponent<StageFruitTarget>();
            string sourceId = fruit.RewardSourceId;

            Require(fruit.TryReserveInteraction(), "Available fruit could not be reserved.");
            Require(ledger.TryAward(sourceId, StageEnergyKind.Positive), "Fruit did not award positive energy.");
            Require(fruit.TryClaimReward(), "Fruit reward could not be claimed after reservation.");
            Require(fruit.CompleteConsumption(), "Fruit consumption did not complete.");
            Require(!fruitObject.activeSelf && fruit.LifecycleState == FruitLifecycleState.Consumed,
                "Fruit visual remained active after consumption.");
            Require(!ledger.TryAward(sourceId, StageEnergyKind.Positive) && ledger.PositiveEnergy == 1,
                "Fruit awarded energy more than once.");
            results.Add("fruit: consumes once, hides only fruit, awards +1 positive once");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(ledgerObject);
            UnityEngine.Object.DestroyImmediate(fruitObject);
        }
    }

    private static void ValidateNpcRewardReservation(List<string> results)
    {
        var ledgerObject = new GameObject("AcceptanceNpcLedger");
        var npcObject = new GameObject("AcceptanceNpc");
        try
        {
            var ledger = ledgerObject.AddComponent<StageEnergyLedger>();
            var npc = npcObject.AddComponent<StageDropletNpcTarget>();
            string sourceId = npc.RewardSourceId;

            Require(npc.TryReserveInteraction(), "Available NPC could not be reserved.");
            Require(ledger.TryAward(sourceId, StageEnergyKind.Negative), "NPC did not award negative energy.");
            Require(npc.TryClaimReward(), "NPC reward could not be claimed after reservation.");
            Require(!npc.IsAvailable && !npc.TryReserveInteraction(), "Defeated NPC remained attackable.");
            Require(npc.BeginDefeat(10f) && npc.LifecycleState == DropletNpcLifecycleState.Hit,
                "NPC did not enter its single defeat sequence.");
            Require(!npc.BeginDefeat(0f), "NPC defeat sequence could be started twice.");
            Require(!ledger.TryAward(sourceId, StageEnergyKind.Negative) && ledger.NegativeEnergy == 1,
                "NPC awarded negative energy more than once.");
            results.Add("npc: defeat sequence starts once and awards +1 negative once");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(ledgerObject);
            UnityEngine.Object.DestroyImmediate(npcObject);
        }
    }

    private static void ValidateEnergyAndEvolution(List<string> results)
    {
        var celestialLedger = CreateLedger();
        var demonLedger = CreateLedger();
        var dragonLedger = CreateLedger();
        try
        {
            Award(celestialLedger, StageEnergyKind.Positive, 8, "celestial-positive");
            Award(celestialLedger, StageEnergyKind.Negative, 2, "celestial-negative");
            Require(celestialLedger.TotalEnergy == 10 && celestialLedger.ProgressState == StageProgressState.ReadyToEvolve,
                "10/10 did not expose ReadyToEvolve without auto-evolving.");
            Require(celestialLedger.CurrentLineage == StageEvolutionLineage.Celestial,
                "8/2 did not resolve to Celestial.");
            Require(celestialLedger.TryAward("post-cap-negative", StageEnergyKind.Negative)
                && celestialLedger.PositiveEnergy == 7
                && celestialLedger.NegativeEnergy == 3
                && celestialLedger.TotalEnergy == 10
                && celestialLedger.CurrentLineage == StageEvolutionLineage.Draconic,
                "Post-cap opposite replacement did not produce 7/3 Draconic at 10 total.");

            Award(demonLedger, StageEnergyKind.Positive, 2, "demon-positive");
            Award(demonLedger, StageEnergyKind.Negative, 8, "demon-negative");
            Require(demonLedger.CurrentLineage == StageEvolutionLineage.Demonic, "2/8 did not resolve to Demonic.");

            Award(dragonLedger, StageEnergyKind.Positive, 7, "dragon-positive");
            Award(dragonLedger, StageEnergyKind.Negative, 3, "dragon-negative");
            Require(dragonLedger.CurrentLineage == StageEvolutionLineage.Draconic, "7/3 did not resolve to Draconic.");
            Require(dragonLedger.TryConfirmEvolution() && dragonLedger.ProgressState == StageProgressState.Evolved,
                "Explicit evolution confirmation did not evolve a ready ledger.");
            Require(!dragonLedger.TryAward("after-evolution", StageEnergyKind.Positive),
                "Energy was accepted after evolution.");
            results.Add("energy: 10/10 stays manual; replacement, lineages, and post-evolution lock pass");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(celestialLedger.gameObject);
            UnityEngine.Object.DestroyImmediate(demonLedger.gameObject);
            UnityEngine.Object.DestroyImmediate(dragonLedger.gameObject);
        }
    }

    private static StageEnergyLedger CreateLedger()
    {
        return new GameObject("AcceptanceLedger").AddComponent<StageEnergyLedger>();
    }

    private static void Award(StageEnergyLedger ledger, StageEnergyKind kind, int count, string prefix)
    {
        for (int index = 0; index < count; index++)
            Require(ledger.TryAward($"{prefix}-{index}", kind), $"Failed to award {kind} energy at index {index}.");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
    }
}
