using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

                FastBufferWriter fastBufferSpiderWriter = new FastBufferWriter(sizeof(float) * 3, Unity.Collections.Allocator.Temp, -1);
                fastBufferSpiderWriter.WriteValueSafe<float>(spiderSpeedRand);
                fastBufferSpiderWriter.WriteValueSafe<float>(spiderHealthRand);
                fastBufferSpiderWriter.WriteValueSafe<float>(spiderScaleRand);

                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesSpiderData", fastBufferSpiderWriter, NetworkDelivery.Reliable);

                FastBufferWriter fastBufferWriter = new FastBufferWriter(sizeof(ulong), Unity.Collections.Allocator.Temp, -1);
                fastBufferWriter.WriteValueSafe<ulong>(__instance.NetworkObjectId);

                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesSpiderID", fastBufferWriter, NetworkDelivery.Reliable);
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

        public static void StoreSpiderValuesSentByServer(ulong _, FastBufferReader reader)
        {
            if (!Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                reader.ReadValueSafe<float>(out RandomizerValues.spiderSpeedClient);
                float spiderHealth;
                reader.ReadValueSafe<float>(out spiderHealth);
                RandomizerValues.spiderHealthClient = (int)spiderHealth;
                reader.ReadValueSafe<float>(out RandomizerValues.spiderScaleClient);
            }
        }

        public static void StoreSpiderID(ulong _, FastBufferReader reader)
        {
            if (!Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                reader.ReadValueSafe<ulong>(out RandomizerValues.spiderIDClient);

                FinalizeValues();
            }
        }

        public static void FinalizeValues()
        {
            RandomizerValues.spiderSpeedsDict.Add(RandomizerValues.spiderIDClient, RandomizerValues.spiderSpeedClient);

            NetworkObject networkObject = Unity.Netcode.NetworkManager.Singleton.SpawnManager.SpawnedObjects[RandomizerValues.spiderIDClient];
            SandSpiderAI spider = networkObject.gameObject.GetComponentInChildren<SandSpiderAI>();

            spider.agent.speed = RandomizerValues.spiderSpeedsDict.GetValueSafe(RandomizerValues.spiderIDClient);
            spider.enemyHP = RandomizerValues.spiderHealthClient;

            spider.transform.localScale = new Vector3(RandomizerValues.spiderScaleClient, RandomizerValues.spiderScaleClient, RandomizerValues.spiderScaleClient);
        }
    }
}
