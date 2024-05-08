using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Netcode;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine.AI;

namespace LCRandomizerMod.Patches
{
    [HarmonyPatch(typeof(MouthDogAI))]
    internal class MouthDogAIPatch
    {
        static float speedRand;
        static float enemyHPRand;
        static ulong dogID;

        [HarmonyPatch(nameof(MouthDogAI.Start))]
        [HarmonyPostfix]
        public static void RandomizeDogStats(MouthDogAI __instance)
        {
            if (Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                speedRand = Convert.ToSingle(new System.Random().Next(80, 200)) / 10f;
                enemyHPRand = Convert.ToSingle(new System.Random().Next(1, 15));

                FastBufferWriter fastBufferStatWriter = new FastBufferWriter(sizeof(float) * 2, Unity.Collections.Allocator.Temp, -1);
                fastBufferStatWriter.WriteValueSafe<float>(speedRand);
                fastBufferStatWriter.WriteValueSafe<float>(enemyHPRand);

                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesRandomDogStat", fastBufferStatWriter, NetworkDelivery.Reliable);

                FastBufferWriter fastBufferRefWriter = new FastBufferWriter(sizeof(ulong), Unity.Collections.Allocator.Temp, -1);
                fastBufferRefWriter.WriteValueSafe<ulong>(__instance.NetworkObjectId);

                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesDogID", fastBufferRefWriter, NetworkDelivery.Reliable);
            }
        }

        [HarmonyPatch(nameof(MouthDogAI.Update))]
        [HarmonyPostfix]
        public static void StatOverride(MouthDogAI __instance)
        {
            __instance.agent.speed = speedRand;
            __instance.enemyHP = (int)enemyHPRand;
        }

        public static void SetRandomDogSpeedClient(ulong _, FastBufferReader reader)
        {
            reader.ReadValueSafe<float>(out speedRand);
            reader.ReadValueSafe<float>(out enemyHPRand);
            RandomizerModBase.mls.LogInfo("Received random dog speed: " + speedRand + ", " + enemyHPRand);
        }

        public static void SetRandomDogSpeedOnID(ulong _, FastBufferReader reader)
        {
            reader.ReadValueSafe<ulong>(out dogID);
            RandomizerModBase.mls.LogInfo("Received dog ID: " + dogID);

            FinalizeValues();
        }

        public static void FinalizeValues()
        {
            NetworkObject networkObject = Unity.Netcode.NetworkManager.Singleton.SpawnManager.SpawnedObjects[dogID];
            MouthDogAI dog = networkObject.gameObject.GetComponentInChildren<MouthDogAI>();

            dog.agent.speed = speedRand;
            dog.enemyHP = (int)enemyHPRand;
        }
    }
}
