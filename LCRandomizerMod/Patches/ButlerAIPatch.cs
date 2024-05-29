using HarmonyLib;
using System;
using Unity.Netcode;
using UnityEngine;

namespace LCRandomizerMod.Patches
{
    [HarmonyPatch(typeof(ButlerEnemyAI))]
    internal class ButlerAIPatch
    {
        [HarmonyPatch(nameof(ButlerEnemyAI.Start))]
        [HarmonyPostfix]
        public static void StatOverride(ButlerEnemyAI __instance)
        {
            if (Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                float speed = Convert.ToSingle(new System.Random().Next(5, 200)) / 10f;
                float health = Convert.ToSingle(new System.Random().Next(1, 11));
                float scale = Convert.ToSingle(new System.Random().Next(5, 301)) / 100;

                RandomizerValues.butlerSpeedsDict.Add(__instance.NetworkObjectId, speed);

                __instance.enemyHP = (int)health;
                __instance.transform.localScale = new Vector3(scale, scale, scale);

                __instance.creatureAnimator.speed = speed / 10;
                __instance.creatureSFX.pitch = Mathf.Lerp(3f, 0.01f, Mathf.InverseLerp(0.05f, 3f, scale));
                __instance.creatureVoice.pitch = Mathf.Lerp(3f, 0.01f, Mathf.InverseLerp(0.05f, 3f, scale));

                FastBufferWriter fastBufferWriter = new FastBufferWriter(sizeof(ulong) + sizeof(float) * 3, Unity.Collections.Allocator.Temp, -1);
                fastBufferWriter.WriteValueSafe<ulong>(__instance.NetworkObjectId);
                fastBufferWriter.WriteValueSafe<float>(speed);
                fastBufferWriter.WriteValueSafe<float>(health);
                fastBufferWriter.WriteValueSafe<float>(scale);

                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesButlerData", fastBufferWriter, NetworkDelivery.Reliable);
                fastBufferWriter.Dispose();
            }
        }

        [HarmonyPatch(nameof(ButlerEnemyAI.Update))]
        [HarmonyPostfix]
        public static void SpeedOverride(ButlerEnemyAI __instance)
        {
            if (!__instance.isEnemyDead)
            {
                __instance.agent.speed = RandomizerValues.butlerSpeedsDict.GetValueSafe(__instance.NetworkObjectId);
            }
        }

        public static void SetButlerData(ulong _, FastBufferReader reader)
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

                RandomizerValues.butlerSpeedsDict.Add(id, speed);

                NetworkObject networkObject = Unity.Netcode.NetworkManager.Singleton.SpawnManager.SpawnedObjects[id];
                ButlerEnemyAI butler = networkObject.gameObject.GetComponentInChildren<ButlerEnemyAI>();
                butler.enemyHP = (int)health;
                butler.transform.localScale = new Vector3(scale, scale, scale);

                butler.creatureAnimator.speed = speed / 10;
                butler.creatureSFX.pitch = Mathf.Lerp(3f, 0.01f, Mathf.InverseLerp(0.05f, 3f, scale));
                butler.creatureVoice.pitch = Mathf.Lerp(3f, 0.01f, Mathf.InverseLerp(0.05f, 3f, scale));

                RandomizerModBase.mls.LogInfo("RECEIVED BUTLER STATS: " + id + ", " + speed + ", " + health + ", " + scale);
            }
        }
    }
}
