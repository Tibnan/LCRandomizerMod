using HarmonyLib;
using System;
using Unity.Netcode;
using UnityEngine;

namespace LCRandomizerMod.Patches
{
    [HarmonyPatch(typeof(DoublewingAI))]
    internal class DoublewingAIPatch
    {
        [HarmonyPatch(nameof(DoublewingAI.Start))]
        [HarmonyPostfix]
        public static void StatOverride(DoublewingAI __instance)
        {
            if (Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                float speed = Convert.ToSingle(new System.Random().Next(20, 200)) / 10f;
                float health = Convert.ToSingle(new System.Random().Next(1, 11));
                float scale = Convert.ToSingle(new System.Random().Next(5, 21)) / 10;

                RandomizerValues.doublewingSpeedsDict.Add(__instance.NetworkObjectId, speed);

                __instance.enemyHP = (int)health;
                __instance.transform.localScale = new Vector3(scale, scale, scale);

                __instance.creatureAnimator.speed = speed / 10f;
                __instance.creatureSFX.pitch = Mathf.Lerp(3f, 0.1f, Mathf.InverseLerp(0.5f, 2f, scale));
                __instance.creatureVoice.pitch = Mathf.Lerp(3f, 0.1f, Mathf.InverseLerp(0.5f, 2f, scale));

                FastBufferWriter fastBufferWriter = new FastBufferWriter(sizeof(ulong) + sizeof(float) * 3, Unity.Collections.Allocator.Temp, -1);
                fastBufferWriter.WriteValueSafe<ulong>(__instance.NetworkObjectId);
                fastBufferWriter.WriteValueSafe<float>(speed);
                fastBufferWriter.WriteValueSafe<float>(health);
                fastBufferWriter.WriteValueSafe<float>(scale);

                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesDoublewingData", fastBufferWriter, NetworkDelivery.Reliable);
                fastBufferWriter.Dispose();
            }
        }

        [HarmonyPatch(nameof(DoublewingAI.Update))]
        [HarmonyPostfix]
        public static void SpeedOverride(DoublewingAI __instance)
        {
            if (!__instance.isEnemyDead)
            {
                __instance.agent.speed = RandomizerValues.doublewingSpeedsDict.GetValueSafe(__instance.NetworkObjectId);
            }
        }

        public static void SetDoublewingData(ulong _, FastBufferReader reader)
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

                RandomizerValues.doublewingSpeedsDict.Add(id, speed);

                NetworkObject networkObject = Unity.Netcode.NetworkManager.Singleton.SpawnManager.SpawnedObjects[id];
                DoublewingAI doublewing = networkObject.gameObject.GetComponentInChildren<DoublewingAI>();
                doublewing.enemyHP = (int)health;
                doublewing.transform.localScale = new Vector3(scale, scale, scale);

                doublewing.creatureAnimator.speed = speed / 10f;
                doublewing.creatureSFX.pitch = Mathf.Lerp(3f, 0.1f, Mathf.InverseLerp(0.5f, 2f, scale));
                doublewing.creatureVoice.pitch = Mathf.Lerp(3f, 0.1f, Mathf.InverseLerp(0.5f, 2f, scale));

                RandomizerModBase.mls.LogInfo("RECEIVED DOUBLEWING STATS: " + id + ", " + speed + ", " + health + ", " + scale);
            }
        }
    }
}
