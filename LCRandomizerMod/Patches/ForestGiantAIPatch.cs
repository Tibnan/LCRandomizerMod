using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
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
                float giantSpeed = Convert.ToSingle(new System.Random().Next(20, 141)) / 10f;
                float giantHealth = Convert.ToSingle(new System.Random().Next(1, 11));
                float giantScale = Convert.ToSingle(new System.Random().Next(5, 401)) / 100;

                RandomizerValues.giantSpeedsDict.Add(__instance.NetworkObjectId, giantSpeed);

                __instance.enemyHP = (int)giantHealth;
                __instance.transform.localScale = new UnityEngine.Vector3(giantScale, giantScale, giantScale);

                __instance.creatureAnimator.speed = giantSpeed / 10;
                __instance.creatureSFX.pitch = Mathf.Lerp(3f, 0.1f, Mathf.InverseLerp(0.05f, 4f, giantScale));

                FastBufferWriter fastBufferWriter = new FastBufferWriter(sizeof(ulong) + sizeof(float) * 3, Unity.Collections.Allocator.Temp, -1);
                fastBufferWriter.WriteValueSafe<ulong>(__instance.NetworkObjectId);
                fastBufferWriter.WriteValueSafe<float>(giantSpeed);
                fastBufferWriter.WriteValueSafe<float>(giantHealth);
                fastBufferWriter.WriteValueSafe<float>(giantScale);

                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesGiantData", fastBufferWriter, NetworkDelivery.Reliable);

                RandomizerModBase.mls.LogError("FOREST GIANT SPAWN");
            }
        }

        [HarmonyPatch(nameof(ForestGiantAI.Update))]
        [HarmonyPostfix]
        public static void GiantSpeedOverride(ForestGiantAI __instance)
        {
            if (__instance.stunnedByPlayer)
            {
                __instance.agent.speed = 0f;
            }
            else if (!__instance.isEnemyDead)
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

                RandomizerModBase.mls.LogError("Read data");

                RandomizerValues.giantSpeedsDict.Add(id, speed);

                NetworkObject networkObject = Unity.Netcode.NetworkManager.Singleton.SpawnManager.SpawnedObjects[id];
                ForestGiantAI forestGiant = networkObject.gameObject.GetComponentInChildren<ForestGiantAI>();

                forestGiant.enemyHP = (int)health;
                forestGiant.transform.localScale = new UnityEngine.Vector3(scale, scale, scale);

                //forestGiant.creatureAnimator.speed = speed / 10;
                forestGiant.creatureSFX.pitch = Mathf.Lerp(3f, 0.1f, Mathf.InverseLerp(0.05f, 4f, scale));

                RandomizerModBase.mls.LogInfo("RECEIVED GIANT STATS: " + id + ", " + speed + ", " + health + ", " + scale);
            }
        }
    }
}
