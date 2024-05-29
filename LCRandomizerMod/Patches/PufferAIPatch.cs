using HarmonyLib;
using System;
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
            if (Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                float speed = Convert.ToSingle(new System.Random().Next(20, 200)) / 10f;
                float health = Convert.ToSingle(new System.Random().Next(1, 11));
                float scale = Convert.ToSingle(new System.Random().Next(5, 21)) / 10;

                RandomizerValues.pufferSpeedsDict.Add(__instance.NetworkObjectId, speed);

                __instance.enemyHP = (int)health;
                __instance.transform.localScale = new Vector3(scale, scale, scale);

                __instance.creatureAnimator.speed = speed / 10f;
                __instance.creatureSFX.pitch = Mathf.Lerp(3f, 0.001f, Mathf.InverseLerp(0.5f, 2f, scale));
                __instance.creatureVoice.pitch = Mathf.Lerp(3f, 0.001f, Mathf.InverseLerp(0.5f, 2f, scale));

                FastBufferWriter fastBufferWriter = new FastBufferWriter(sizeof(ulong) + sizeof(float) * 3, Unity.Collections.Allocator.Temp, -1);
                fastBufferWriter.WriteValueSafe<ulong>(__instance.NetworkObjectId);
                fastBufferWriter.WriteValueSafe<float>(speed);
                fastBufferWriter.WriteValueSafe<float>(health);
                fastBufferWriter.WriteValueSafe<float>(scale);

                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesPufferData", fastBufferWriter, NetworkDelivery.Reliable);
                fastBufferWriter.Dispose();
            }
        }

        [HarmonyPatch(nameof(PufferAI.Update))]
        [HarmonyPostfix]
        public static void SpeedOverride(PufferAI __instance)
        {
            if (!__instance.isEnemyDead)
            {
                if (Traverse.Create(__instance).Field("inPuffingAnimation").GetValue<bool>())
                {
                    __instance.agent.speed = 0f;
                }else
                {
                    __instance.agent.speed = RandomizerValues.pufferSpeedsDict.GetValueSafe(__instance.NetworkObjectId);
                }
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

                //puffer.creatureAnimator.speed = speed / 10f;
                puffer.creatureSFX.pitch = Mathf.Lerp(3f, 0.001f, Mathf.InverseLerp(0.5f, 2f, scale));
                puffer.creatureVoice.pitch = Mathf.Lerp(3f, 0.001f, Mathf.InverseLerp(0.5f, 2f, scale));

                RandomizerModBase.mls.LogInfo("RECEIVED PUFFER STATS: " + id + ", " + speed + ", " + health + ", " + scale);
            }
        }
    }
}
