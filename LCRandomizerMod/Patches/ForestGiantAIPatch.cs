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
    [HarmonyPatch(typeof(ForestGiantAI))]
    internal class ForestGiantAIPatch
    {
        [HarmonyPatch(nameof(ForestGiantAI.Start))]
        [HarmonyPostfix]
        public static void StatOverride(ForestGiantAI __instance)
        {
            if (Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                float giantSpeed = Convert.ToSingle(new System.Random().Next(20, 200)) / 10f;
                float giantHealth = Convert.ToSingle(new System.Random().Next(1, 11));
                float giantScale = Convert.ToSingle(new System.Random().Next(5, 21)) / 10;

                RandomizerValues.giantSpeedsDict.Add(__instance.NetworkObjectId, giantSpeed);

                __instance.enemyHP = (int)giantHealth;
                __instance.transform.localScale = new UnityEngine.Vector3(giantScale, giantScale, giantScale);

                FastBufferWriter fastBufferWriter = new FastBufferWriter(sizeof(ulong) + sizeof(float) * 3, Unity.Collections.Allocator.Temp, -1);
                fastBufferWriter.WriteValueSafe<ulong>(__instance.NetworkObjectId);
                fastBufferWriter.WriteValueSafe<float>(giantSpeed);
                fastBufferWriter.WriteValueSafe<float>(giantHealth);
                fastBufferWriter.WriteValueSafe<float>(giantScale);

                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesGiantData", fastBufferWriter, NetworkDelivery.Reliable);
                fastBufferWriter.Dispose();

                RandomizerModBase.mls.LogError("FOREST GIANT SPAWN");
            }
        }

        [HarmonyPatch(nameof(ForestGiantAI.Update))]
        [HarmonyPostfix]
        public static void GiantSpeedOverride(ForestGiantAI __instance)
        {
            if (!__instance.isEnemyDead)
            {
                __instance.agent.speed = RandomizerValues.giantSpeedsDict.GetValueSafe(__instance.NetworkObjectId);
            }
        }

        public static void SetGiantValuesSentByServer(ulong _, FastBufferReader reader)
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

                RandomizerValues.giantSpeedsDict.Add(id, speed);

                NetworkObject networkObject = Unity.Netcode.NetworkManager.Singleton.SpawnManager.SpawnedObjects[id];
                ForestGiantAI forestGiant = networkObject.gameObject.GetComponentInChildren<ForestGiantAI>();

                forestGiant.enemyHP = (int)health;
                forestGiant.transform.localScale = new UnityEngine.Vector3(scale, scale, scale);

                RandomizerModBase.mls.LogInfo("RECEIVED GIANT STATS: " + id + ", " + speed + ", " + health + ", " + scale);
            }
        }
    }
}
