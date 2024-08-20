using Unity.Netcode;
using HarmonyLib;
using System;
using UnityEngine;

namespace LCRandomizerMod.Patches
{
    [HarmonyPatch(typeof(BushWolfEnemy))]
    internal class BushWolfEnemyPatch
    {
        //DONT FORGET TO UNCOMMENT LINES WHEN THIS GETS REIMPLEMENTED

        [HarmonyPatch(nameof(BushWolfEnemy.Start))]
        [HarmonyPostfix]
        public static void RandomizeWolf(BushWolfEnemy __instance)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                float scale = Convert.ToSingle(new System.Random().Next(5, 131)) / 100;
                float speed = Convert.ToSingle(new System.Random().Next(30, 201)) / 10f;
                int health = new System.Random().Next(1, 11);

                __instance.transform.localScale = new Vector3(scale, scale, scale);
                __instance.enemyHP = health;

                //RandomizerValues.wolfSpeedDict.Add(__instance.NetworkObjectId, speed);

                FastBufferWriter fastBufferWriter = new FastBufferWriter(sizeof(ulong) + sizeof(float) * 2 + sizeof(int), Unity.Collections.Allocator.Temp, -1);

                fastBufferWriter.WriteValueSafe<ulong>(__instance.NetworkObjectId);
                fastBufferWriter.WriteValueSafe<float>(scale);
                fastBufferWriter.WriteValueSafe<float>(speed);
                fastBufferWriter.WriteValueSafe<int>(health);

                NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesWolfStats", fastBufferWriter, NetworkDelivery.Reliable);
            }
        }

        [HarmonyPatch(nameof(BushWolfEnemy.Update))]
        [HarmonyPostfix]
        public static void SpeedOverride(BushWolfEnemy __instance)
        {
            if (!__instance.isEnemyDead && !__instance.inSpecialAnimation)
            {
                //__instance.agent.speed = RandomizerValues.wolfSpeedDict.GetValueSafe(__instance.NetworkObjectId);
            }
        }

        public static void ClientSetWolfStats(ulong _, FastBufferReader reader)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                ulong id;
                float scale;
                float speed;
                int health;

                reader.ReadValueSafe<ulong>(out id);
                reader.ReadValueSafe<float>(out scale);
                reader.ReadValueSafe<float>(out speed);
                reader.ReadValueSafe<int>(out health);

                //RandomizerValues.wolfSpeedDict.Add(id, speed);

                NetworkObject networkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[id];
                BushWolfEnemy wolf = networkObject.gameObject.GetComponentInChildren<BushWolfEnemy>();

                wolf.transform.localScale = new Vector3(scale, scale, scale);
                wolf.enemyHP = health;
            }
        }
    }
}
