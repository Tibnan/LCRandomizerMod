using HarmonyLib;
using System;
using Unity.Netcode;
using UnityEngine;

namespace LCRandomizerMod.Patches
{
    [HarmonyPatch(typeof(BaboonBirdAI))]
    internal class BaboonBirdAIPatch
    {
        [HarmonyPatch(nameof(BaboonBirdAI.Start))]
        [HarmonyPostfix]
        public static void StatOverride(BaboonBirdAI __instance)
        {
            if (Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                float speed = Convert.ToSingle(new System.Random().Next(20, 200)) / 10f;
                float health = Convert.ToSingle(new System.Random().Next(1, 11));
                float scale = Convert.ToSingle(new System.Random().Next(5, 21)) / 10;

                RandomizerValues.baboonSpeedsDict.Add(__instance.NetworkObjectId, speed);

                __instance.enemyHP = (int)health;
                __instance.transform.localScale = new Vector3(scale, scale, scale);

                FastBufferWriter fastBufferWriter = new FastBufferWriter(sizeof(ulong) + sizeof(float) * 3, Unity.Collections.Allocator.Temp, -1);
                fastBufferWriter.WriteValueSafe<ulong>(__instance.NetworkObjectId);
                fastBufferWriter.WriteValueSafe<float>(speed);
                fastBufferWriter.WriteValueSafe<float>(health);
                fastBufferWriter.WriteValueSafe<float>(scale);

                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesBaboonData", fastBufferWriter, NetworkDelivery.Reliable);
                fastBufferWriter.Dispose();
            }
        }

        [HarmonyPatch(nameof(BaboonBirdAI.Update))]
        [HarmonyPostfix]
        public static void SpeedOverride(BaboonBirdAI __instance)
        {
            if (!__instance.isEnemyDead)
            {
                __instance.agent.speed = RandomizerValues.baboonSpeedsDict.GetValueSafe(__instance.NetworkObjectId);
            }
        }

        public static void SetBaboonData(ulong _, FastBufferReader reader)
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

                RandomizerValues.baboonSpeedsDict.Add(id, speed);

                NetworkObject networkObject = Unity.Netcode.NetworkManager.Singleton.SpawnManager.SpawnedObjects[id];
                BaboonBirdAI baboon = networkObject.gameObject.GetComponentInChildren<BaboonBirdAI>();
                baboon.enemyHP = (int)health;
                baboon.transform.localScale = new Vector3(scale, scale, scale);

                RandomizerModBase.mls.LogInfo("RECEIVED BABOON STATS: " + id + ", " + speed + ", " + health + ", " + scale);
            }
        }
    }
}
