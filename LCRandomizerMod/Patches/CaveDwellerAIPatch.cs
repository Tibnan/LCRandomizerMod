using Unity.Netcode;
using HarmonyLib;
using System;
using UnityEngine;

namespace LCRandomizerMod.Patches
{
    [HarmonyPatch(typeof(CaveDwellerAI))]
    internal class CaveDwellerAIPatch
    {
        [HarmonyPatch(nameof(CaveDwellerAI.Start))]
        [HarmonyPostfix]
        public static void RandomizeBaby(CaveDwellerAI __instance)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                float scale = Convert.ToSingle(new System.Random().Next(25, 231)) / 100;
                float speed = Convert.ToSingle(new System.Random().Next(3, 121)) / 10f;

                RandomizerValues.babySpeedDict.Add(__instance.NetworkObjectId, speed);
                __instance.transform.localScale = new Vector3(scale, scale, scale);

                FastBufferWriter writer = new FastBufferWriter(sizeof(ulong) + sizeof(float) * 2, Unity.Collections.Allocator.Temp, -1);
                writer.WriteValueSafe<ulong>(__instance.NetworkObjectId);
                writer.WriteValueSafe<float>(scale);
                writer.WriteValueSafe<float>(speed);

                NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesBabyStat", writer, NetworkDelivery.Reliable);
            }
        }

        [HarmonyPatch(nameof(CaveDwellerAI.Update))]
        [HarmonyPostfix]
        public static void SpeedOverride(CaveDwellerAI __instance)
        {
            if (!__instance.sittingDown && !__instance.isEnemyDead)
            {
                __instance.agent.speed *= RandomizerValues.babySpeedDict.GetValueSafe(__instance.NetworkObjectId);
            }
        }

        public static void ClientSetBabyStats(ulong _, FastBufferReader reader)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                ulong id;
                float scale;
                float speed;

                reader.ReadValueSafe<ulong>(out id);
                reader.ReadValueSafe<float>(out scale);
                reader.ReadValueSafe<float>(out speed);

                RandomizerValues.babySpeedDict.Add(id, speed);
                NetworkObject networkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[id];
                CaveDwellerAI baby = networkObject.gameObject.GetComponentInChildren<CaveDwellerAI>();
                baby.transform.localScale = new Vector3(scale, scale, scale);
            }
        }

    }
}
