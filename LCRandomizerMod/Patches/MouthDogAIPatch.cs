using HarmonyLib;
using System;
using UnityEngine;
using Unity.Netcode;

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
                float dogSpeed = Convert.ToSingle(new System.Random().Next(30, 200)) / 10f;
                float dogEnemyHP = Convert.ToSingle(new System.Random().Next(1, 6));
                float dogScale = Convert.ToSingle(new System.Random().Next(10, 20)) / 10;

                __instance.enemyHP = (int)dogEnemyHP;
                __instance.transform.localScale = new Vector3(dogScale, dogScale, dogScale);

                RandomizerValues.dogSpeedsDict.Add(__instance.NetworkObjectId, dogSpeed);

                FastBufferWriter fastBufferStatWriter = new FastBufferWriter(sizeof(ulong) + sizeof(float) * 3, Unity.Collections.Allocator.Temp, -1);
                fastBufferStatWriter.WriteValueSafe<ulong>(__instance.NetworkObjectId);
                fastBufferStatWriter.WriteValueSafe<float>(dogSpeed);
                fastBufferStatWriter.WriteValueSafe<float>(dogEnemyHP);
                fastBufferStatWriter.WriteValueSafe<float>(dogScale);

                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesDogStats", fastBufferStatWriter, NetworkDelivery.Reliable);
            }
        }

        [HarmonyPatch(nameof(MouthDogAI.Update))]
        [HarmonyPostfix]
        public static void StatOverride(MouthDogAI __instance)
        {
            if (!__instance.isEnemyDead)
            {
                __instance.agent.speed = RandomizerValues.dogSpeedsDict.GetValueSafe(__instance.NetworkObjectId);
            }
        }

        public static void SetDogValuesSentByServer(ulong _, FastBufferReader reader)
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

                RandomizerValues.dogSpeedsDict.Add(id, speed);

                NetworkObject networkObject = Unity.Netcode.NetworkManager.Singleton.SpawnManager.SpawnedObjects[id];
                MouthDogAI dog = networkObject.gameObject.GetComponentInChildren<MouthDogAI>();

                dog.enemyHP = (int)health;
                dog.transform.localScale = new Vector3(scale, scale, scale);

                RandomizerModBase.mls.LogInfo("RECEIVED DOG STATS: " + id + ", " + speed + ", " + health + ", " + scale);
            }
        }
    }
}
