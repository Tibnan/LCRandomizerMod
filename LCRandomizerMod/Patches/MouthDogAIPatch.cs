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
using System.CodeDom.Compiler;
using Unity.Profiling;

namespace LCRandomizerMod.Patches
{
    [HarmonyPatch(typeof(MouthDogAI))]
    internal class MouthDogAIPatch
    {
        [HarmonyPatch(nameof(MouthDogAI.Start))]
        [HarmonyPostfix]
        public static void RandomizeDogStats(MouthDogAI __instance)
        {
            if (Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                float dogSpeed = Convert.ToSingle(new System.Random().Next(80, 200)) / 10f;
                float dogEnemyHP = Convert.ToSingle(new System.Random().Next(1, 6));
                float dogScale = Convert.ToSingle(new System.Random().Next(10, 20)) / 10;

                __instance.enemyHP = (int)dogEnemyHP;
                __instance.transform.localScale = new Vector3(dogScale, dogScale, dogScale);

                RandomizerValues.dogSpeeds.Add(dogSpeed);

                FastBufferWriter fastBufferStatWriter = new FastBufferWriter(sizeof(float) * 2, Unity.Collections.Allocator.Temp, -1);
                fastBufferStatWriter.WriteValueSafe<float>(dogSpeed);
                fastBufferStatWriter.WriteValueSafe<float>(dogEnemyHP);

                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesRandomDogStat", fastBufferStatWriter, NetworkDelivery.Reliable);

                RandomizerValues.dogIDs.Add(__instance.NetworkObjectId);
                FastBufferWriter fastBufferRefWriter = new FastBufferWriter(sizeof(ulong), Unity.Collections.Allocator.Temp, -1);
                fastBufferRefWriter.WriteValueSafe<ulong>(__instance.NetworkObjectId);

                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesDogID", fastBufferRefWriter, NetworkDelivery.Reliable);

                FastBufferWriter fastBufferScaleWriter = new FastBufferWriter(sizeof(float), Unity.Collections.Allocator.Temp, -1);
                fastBufferScaleWriter.WriteValueSafe<float>(dogScale);

                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesDogScale", fastBufferScaleWriter, NetworkDelivery.Reliable);
            }
        }

        [HarmonyPatch(nameof(MouthDogAI.Update))]
        [HarmonyPostfix]
        public static void StatOverride(MouthDogAI __instance)
        {
            if (!__instance.isEnemyDead)
            {
                int dogIndex = RandomizerValues.dogIDs.FindIndex(x => x == __instance.NetworkObjectId);

                __instance.agent.speed = RandomizerValues.dogSpeeds.ElementAt(dogIndex);

                RandomizerModBase.mls.LogInfo("Current dog stats: " + __instance.agent.speed + ", " + __instance.enemyHP);
                RandomizerModBase.mls.LogInfo("Dog list length: " + RandomizerValues.dogIDs.Count);
            }
        }

        public static void SetRandomDogSpeedClient(ulong _, FastBufferReader reader)
        {
            if (!Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                float dogSpeed;
                float enemyHP;
                reader.ReadValueSafe<float>(out dogSpeed);
                RandomizerValues.dogSpeeds.Add(dogSpeed);
                reader.ReadValueSafe<float>(out enemyHP);
                RandomizerValues.dogEnemyHP = (int)enemyHP;

                RandomizerModBase.mls.LogInfo("Received random dog stats: " + dogSpeed + ", " + enemyHP);
            }
        }

        public static void SetRandomDogSpeedOnID(ulong _, FastBufferReader reader)
        {
            if (!Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                ulong dogID;
                reader.ReadValueSafe<ulong>(out dogID);
                RandomizerValues.dogIDs.Add(dogID);
                RandomizerModBase.mls.LogInfo("Received dog ID: " + dogID);
            }
        }

        public static void SetRandomDogScale(ulong _, FastBufferReader reader)
        {
            if (!Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                float dogScale;
                reader.ReadValueSafe<float>(out dogScale);
                RandomizerValues.dogScale = dogScale;
                RandomizerModBase.mls.LogInfo("Received dog scale: " + dogScale);

                FinalizeValues();
            }
        }

        public static void FinalizeValues()
        {
            NetworkObject networkObject = Unity.Netcode.NetworkManager.Singleton.SpawnManager.SpawnedObjects[RandomizerValues.dogIDs.Last()];
            MouthDogAI dog = networkObject.gameObject.GetComponentInChildren<MouthDogAI>();

            dog.agent.speed = RandomizerValues.dogSpeeds.Last();
            dog.enemyHP = RandomizerValues.dogEnemyHP;
            
            float dogScale = RandomizerValues.dogScale;
            dog.transform.localScale = new Vector3(dogScale, dogScale, dogScale);
        }
    }
}
