using HarmonyLib;
using System;
using Unity.Netcode;
using UnityEngine;

namespace LCRandomizerMod.Patches
{
    [HarmonyPatch(typeof(SandSpiderAI))]
    internal class SandSpiderAIPatch
    {
        [HarmonyPatch(nameof(SandSpiderAI.Start))]
        [HarmonyPostfix]
        public static void SpiderWebAmountOverride(SandSpiderAI __instance)
        {
            if (Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                __instance.maxWebTrapsToPlace = new System.Random().Next(4, 14);
                float spiderHealthRand = new System.Random().Next(1, 7);
                float spiderSpeedRand = Convert.ToSingle(new System.Random().Next(40, 81)) / 10;
                float spiderScaleRand = Convert.ToSingle(new System.Random().Next(5, 21)) / 10;

                RandomizerValues.spiderSpeedsDict.Add(__instance.NetworkObjectId, spiderSpeedRand);

                __instance.enemyHP = (int)spiderHealthRand;
                __instance.transform.localScale = new Vector3(spiderScaleRand, spiderScaleRand, spiderScaleRand);

                FastBufferWriter fastBufferSpiderWriter = new FastBufferWriter(sizeof(ulong) + sizeof(float) * 3, Unity.Collections.Allocator.Temp, -1);
                fastBufferSpiderWriter.WriteValueSafe<ulong>(__instance.NetworkObjectId);
                fastBufferSpiderWriter.WriteValueSafe<float>(spiderSpeedRand);
                fastBufferSpiderWriter.WriteValueSafe<float>(spiderHealthRand);
                fastBufferSpiderWriter.WriteValueSafe<float>(spiderScaleRand);

                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesSpiderData", fastBufferSpiderWriter, NetworkDelivery.Reliable);
            }
        }

        [HarmonyPatch(nameof(SandSpiderAI.Update))]
        [HarmonyPostfix]
        public static void SpiderSpeedOverride(SandSpiderAI __instance)
        {
            if (!__instance.isEnemyDead)
            {
                __instance.spiderSpeed = RandomizerValues.spiderSpeedsDict.GetValueSafe(__instance.NetworkObjectId);
                __instance.agent.speed = RandomizerValues.spiderSpeedsDict.GetValueSafe(__instance.NetworkObjectId);
            }
        }

        public static void SetSpiderDataSentByServer(ulong _, FastBufferReader reader)
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

                RandomizerValues.spiderSpeedsDict.Add(id, speed);

                NetworkObject networkObject = Unity.Netcode.NetworkManager.Singleton.SpawnManager.SpawnedObjects[id];
                SandSpiderAI spider = networkObject.gameObject.GetComponentInChildren<SandSpiderAI>();

                spider.enemyHP = (int)health;
                spider.transform.localScale = new Vector3(scale, scale, scale);

                RandomizerModBase.mls.LogInfo("RECEIVED SPIDER STATS: " + id + ", " + speed + ", " + health + ", " + scale);
            }
        }
    }
}
