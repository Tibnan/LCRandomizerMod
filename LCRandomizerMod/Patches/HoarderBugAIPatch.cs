using HarmonyLib;
using System;
using Unity.Netcode;
using UnityEngine;

namespace LCRandomizerMod.Patches
{
    [HarmonyPatch(typeof(HoarderBugAI))]
    internal class HoarderBugAIPatch
    {
        [HarmonyPatch(nameof(HoarderBugAI.Start))]
        [HarmonyPostfix]
        public static void StatOverride(HoarderBugAI __instance)
        {
            if (Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                float speed = Convert.ToSingle(new System.Random().Next(20, 200)) / 10f;
                float health = Convert.ToSingle(new System.Random().Next(1, 11));
                float scale = Convert.ToSingle(new System.Random().Next(5, 21)) / 10;

                RandomizerValues.hoarderBugSpeedsDict.Add(__instance.NetworkObjectId, speed);

                __instance.enemyHP = (int)health;
                __instance.transform.localScale = new Vector3(scale, scale, scale);

                FastBufferWriter fastBufferWriter = new FastBufferWriter(sizeof(ulong) + sizeof(float) * 3, Unity.Collections.Allocator.Temp, -1);
                fastBufferWriter.WriteValueSafe<ulong>(__instance.NetworkObjectId);
                fastBufferWriter.WriteValueSafe<float>(speed);
                fastBufferWriter.WriteValueSafe<float>(health);
                fastBufferWriter.WriteValueSafe<float>(scale);

                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesHoarderBugData", fastBufferWriter, NetworkDelivery.Reliable);
                fastBufferWriter.Dispose();
            }
        }

        [HarmonyPatch(nameof(HoarderBugAI.Update))]
        [HarmonyPostfix]
        public static void BugSpeedOverride(HoarderBugAI __instance)
        {
            if (!__instance.isEnemyDead)
            {
                __instance.agent.speed = RandomizerValues.hoarderBugSpeedsDict.GetValueSafe(__instance.NetworkObjectId);
            }
        }

        public static void SetBugData(ulong _, FastBufferReader reader)
        {
            if (!Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                ulong id;
                float speed;
                float health;
                float scale;

                reader.ReadValueSafe<ulong>(out id);
                reader.ReadValueSafe<float>(out speed);
                reader.ReadValueSafe<float>(out health);
                reader.ReadValueSafe<float>(out scale);

                RandomizerValues.hoarderBugSpeedsDict.Add(id, speed);

                NetworkObject networkObject = Unity.Netcode.NetworkManager.Singleton.SpawnManager.SpawnedObjects[id];
                HoarderBugAI hoarderBug = networkObject.gameObject.GetComponentInChildren<HoarderBugAI>();
                hoarderBug.enemyHP = (int)health;
                hoarderBug.transform.localScale = new Vector3(scale, scale, scale);

                RandomizerModBase.mls.LogInfo("RECEIVED BUG STATS: " + id + ", " + speed + ", " + health + ", " + scale);
            }
        }
    }
}
