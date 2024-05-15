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
    [HarmonyPatch(typeof(PufferAI))]
    internal class PufferAIPatch
    {
        [HarmonyPatch(nameof(PufferAI.Start))]
        [HarmonyPostfix]
        public static void StatOverride(PufferAI __instance)
        {
            float speed = Convert.ToSingle(new System.Random().Next(20, 200)) / 10f;
            float health = Convert.ToSingle(new System.Random().Next(1, 11));
            float scale = Convert.ToSingle(new System.Random().Next(5, 21)) / 10;

            RandomizerValues.pufferSpeedsDict.Add(__instance.NetworkObjectId, speed);

            __instance.enemyHP = (int)health;
            __instance.transform.localScale = new Vector3(scale, scale, scale);

            FastBufferWriter fastBufferWriter = new FastBufferWriter(sizeof(ulong) + sizeof(float) * 3, Unity.Collections.Allocator.Temp, -1);
            fastBufferWriter.WriteValueSafe<ulong>(__instance.NetworkObjectId);
            fastBufferWriter.WriteValueSafe<float>(speed);
            fastBufferWriter.WriteValueSafe<float>(health);
            fastBufferWriter.WriteValueSafe<float>(scale);

            Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesPufferData", fastBufferWriter, NetworkDelivery.Reliable);
            fastBufferWriter.Dispose();
        }

        [HarmonyPatch(nameof(PufferAI.Update))]
        [HarmonyPostfix]
        public static void SpeedOverride(PufferAI __instance)
        {
            if (!__instance.isEnemyDead)
            {
                __instance.agent.speed = RandomizerValues.pufferSpeedsDict.GetValueSafe(__instance.NetworkObjectId);
            }
        }

        public static void SetPufferData(ulong _, FastBufferReader reader)
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

                RandomizerValues.pufferSpeedsDict.Add(id, speed);

                NetworkObject networkObject = Unity.Netcode.NetworkManager.Singleton.SpawnManager.SpawnedObjects[id];
                PufferAI puffer = networkObject.gameObject.GetComponentInChildren<PufferAI>();
                puffer.enemyHP = (int)health;
                puffer.transform.localScale = new Vector3(scale, scale, scale);

                RandomizerModBase.mls.LogInfo("RECEIVED PUFFER STATS: " + id + ", " + speed + ", " + health + ", " + scale);
            }
        }
    }
}
