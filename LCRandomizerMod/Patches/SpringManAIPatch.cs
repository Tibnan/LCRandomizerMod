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
    [HarmonyPatch(typeof(SpringManAI))]
    internal class SpringManAIPatch
    {
        [HarmonyPatch(nameof(SpringManAI.Update))]
        [HarmonyPostfix]
        public static void SpeedOverride(SpringManAI __instance)
        {
            if (!RandomizerValues.springManSpeedsDict.ContainsKey(__instance.NetworkObjectId))
            {
                if (Unity.Netcode.NetworkManager.Singleton.IsServer)
                {
                    float speed = Convert.ToSingle(new System.Random().Next(20, 200)) / 10f;
                    float health = Convert.ToSingle(new System.Random().Next(1, 11));
                    float scale = Convert.ToSingle(new System.Random().Next(5, 21)) / 10;

                    RandomizerValues.springManSpeedsDict.Add(__instance.NetworkObjectId, speed);

                    __instance.enemyHP = (int)health;
                    __instance.transform.localScale = new Vector3(scale, scale, scale);

                    FastBufferWriter fastBufferWriter = new FastBufferWriter(sizeof(ulong) + sizeof(float) * 3, Unity.Collections.Allocator.Temp, -1);
                    fastBufferWriter.WriteValueSafe<ulong>(__instance.NetworkObjectId);
                    fastBufferWriter.WriteValueSafe<float>(speed);
                    fastBufferWriter.WriteValueSafe<float>(health);
                    fastBufferWriter.WriteValueSafe<float>(scale);

                    Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesSpringManData", fastBufferWriter, NetworkDelivery.Reliable);
                    fastBufferWriter.Dispose();
                }
            }

            if (!__instance.isEnemyDead)
            {
                __instance.agent.speed = RandomizerValues.springManSpeedsDict.GetValueSafe(__instance.NetworkObjectId);
            }
        }

        public static void SetSpringManStats(ulong _, FastBufferReader reader)
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

                RandomizerValues.springManSpeedsDict.Add(id, speed);

                NetworkObject networkObject = Unity.Netcode.NetworkManager.Singleton.SpawnManager.SpawnedObjects[id];
                SpringManAI springMan = networkObject.gameObject.GetComponentInChildren<SpringManAI>();
                springMan.enemyHP = (int)health;
                springMan.transform.localScale = new Vector3(scale, scale, scale);

                RandomizerModBase.mls.LogInfo("RECEIVED SPRINGMAN STATS: " + id + ", " + speed + ", " + health + ", " + scale);
            }
        }
    }
}
