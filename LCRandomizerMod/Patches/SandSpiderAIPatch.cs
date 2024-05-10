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
            __instance.maxWebTrapsToPlace = new System.Random().Next(4, 14);
            float spiderHealthRand = new System.Random().Next(1, 7);
            float spiderSpeedRand = Convert.ToSingle(new System.Random().Next(40, 81)) / 10;
            float spiderScaleRand = Convert.ToSingle(new System.Random().Next(5, 21)) / 10;

            RandomizerValues.spiderSpeedRands.Add(spiderSpeedRand);
            RandomizerValues.spiderID.Add(__instance.NetworkObjectId);

            __instance.enemyHP = (int)spiderHealthRand;
            __instance.transform.localScale = new Vector3(RandomizerValues.spiderScaleRand, RandomizerValues.spiderScaleRand, RandomizerValues.spiderScaleRand);

            FastBufferWriter fastBufferSpiderWriter = new FastBufferWriter(sizeof(float) * 3, Unity.Collections.Allocator.Temp, -1);
            fastBufferSpiderWriter.WriteValueSafe<float>(spiderSpeedRand);
            fastBufferSpiderWriter.WriteValueSafe<float>(RandomizerValues.spiderHealthRand);
            fastBufferSpiderWriter.WriteValueSafe<float>(RandomizerValues.spiderScaleRand);

            Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesSpiderData", fastBufferSpiderWriter, NetworkDelivery.Reliable);

            FastBufferWriter fastBufferWriter = new FastBufferWriter(sizeof(ulong), Unity.Collections.Allocator.Temp, -1);
            fastBufferWriter.WriteValueSafe<ulong>(__instance.NetworkObjectId);

            Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesSpiderID", fastBufferWriter, NetworkDelivery.Reliable);
        }

        [HarmonyPatch(nameof(SandSpiderAI.Update))]
        [HarmonyPostfix]
        public static void SpiderSpeedOverride(SandSpiderAI __instance)
        {
            if (!__instance.isEnemyDead)
            {
                int spiderIndex = RandomizerValues.spiderID.FindIndex(x => x == __instance.NetworkObjectId);

                __instance.spiderSpeed = RandomizerValues.spiderSpeedRands.ElementAt(spiderIndex);
                __instance.agent.speed = RandomizerValues.spiderSpeedRands.ElementAt(spiderIndex);
            }
        }

        public static void StoreSpiderValuesSentByServer(ulong _, FastBufferReader reader)
        {
            float data;
            reader.ReadValueSafe<float>(out data);
            RandomizerValues.spiderSpeedRands.Add(data);
            reader.ReadValueSafe<float>(out data);
            RandomizerValues.spiderHealthRand = (int)data;
            reader.ReadValueSafe<float>(out RandomizerValues.spiderScaleRand);
            RandomizerValues.spiderScaleRand = data;
        }

        public static void StoreSpiderID(ulong _, FastBufferReader reader)
        {
            ulong id;
            reader.ReadValueSafe<ulong>(out id);
            RandomizerValues.spiderID.Add(id);
            FinalizeValues();
        }

        public static void FinalizeValues()
        {
            NetworkObject networkObject = Unity.Netcode.NetworkManager.Singleton.SpawnManager.SpawnedObjects[RandomizerValues.spiderID.Last()];
            SandSpiderAI spider = networkObject.gameObject.GetComponentInChildren<SandSpiderAI>();

            spider.agent.speed = RandomizerValues.spiderSpeedRands.Last();
            spider.enemyHP = RandomizerValues.spiderHealthRand;

            spider.transform.localScale = new Vector3(RandomizerValues.spiderScaleRand, RandomizerValues.spiderScaleRand, RandomizerValues.spiderScaleRand);
        }
    }
}
