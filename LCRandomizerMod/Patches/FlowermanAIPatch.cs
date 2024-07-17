using HarmonyLib;
using System;
using Unity.Netcode;
using UnityEngine;

namespace LCRandomizerMod.Patches
{
    [HarmonyPatch(typeof(FlowermanAI))]
    internal class FlowermanAIPatch
    {
        [HarmonyPatch(nameof(FlowermanAI.Start))]
        [HarmonyPostfix]
        public static void StatOverride(FlowermanAI __instance)
        {
            if (Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                float speed = Convert.ToSingle(new System.Random().Next(20, 200)) / 10f;
                float health = Convert.ToSingle(new System.Random().Next(1, 11));
                float scale = Convert.ToSingle(new System.Random().Next(5, 21)) / 10;

                RandomizerValues.flowermanSpeedsDict.Add(__instance.NetworkObjectId, speed);

                __instance.enemyHP = (int)health;
                __instance.transform.localScale = new Vector3(scale, scale, scale);

                __instance.creatureAnimator.speed = speed / 10;
                __instance.creatureSFX.pitch = Mathf.Lerp(2f, 0.5f, Mathf.Lerp(0.5f, 2f, scale));
                if (__instance.creatureVoice != null)
                {
                    __instance.creatureVoice.pitch = Mathf.Lerp(2f, 0.5f, Mathf.Lerp(0.5f, 2f, scale));
                }
                else
                {
                    RandomizerModBase.mls.LogError("creatureVoice is null!");
                }

                FastBufferWriter fastBufferWriter = new FastBufferWriter(sizeof(ulong) + sizeof(float) * 3, Unity.Collections.Allocator.Temp, -1);
                fastBufferWriter.WriteValueSafe<ulong>(__instance.NetworkObjectId);
                fastBufferWriter.WriteValueSafe<float>(speed);
                fastBufferWriter.WriteValueSafe<float>(health);
                fastBufferWriter.WriteValueSafe<float>(scale);

                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesFlowermanData", fastBufferWriter, NetworkDelivery.Reliable);
            }
        }

        [HarmonyPatch(nameof(FlowermanAI.Update))]
        [HarmonyPostfix]
        public static void SpeedOverride(FlowermanAI __instance)
        {
            if (!__instance.isEnemyDead)
            {
                float speed = RandomizerValues.flowermanSpeedsDict.GetValueSafe(__instance.NetworkObjectId);
                if (RandomizerValues.slowedFlowermen.ContainsKey(__instance))
                {
                    speed /= 10f;
                }

                __instance.agent.speed = speed;
            }
        }

        public static void SetFlowermanStats(ulong _, FastBufferReader reader)
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

                RandomizerValues.flowermanSpeedsDict.Add(id, speed);

                NetworkObject networkObject = Unity.Netcode.NetworkManager.Singleton.SpawnManager.SpawnedObjects[id];
                FlowermanAI flowerman = networkObject.gameObject.GetComponentInChildren<FlowermanAI>();

                flowerman.enemyHP = (int)health;
                flowerman.transform.localScale = new Vector3(scale, scale, scale);

                //CHECK FOR NRE!!!
                //flowerman.creatureAnimator.speed = speed / 10;
                flowerman.creatureSFX.pitch = Mathf.Lerp(2f, 0.5f, Mathf.Lerp(0.5f, 2f, scale));
                flowerman.creatureVoice.pitch = Mathf.Lerp(2f, 0.5f, Mathf.Lerp(0.5f, 2f, scale));
                

                RandomizerModBase.mls.LogInfo("RECEIVED FLOWERMAN STATS: " + id + ", " + speed + ", " + health + ", " + scale);
            }
        }
    }
}
