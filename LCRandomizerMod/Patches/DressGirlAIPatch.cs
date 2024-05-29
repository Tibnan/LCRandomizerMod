using HarmonyLib;
using System;
using Unity.Netcode;
using UnityEngine;

namespace LCRandomizerMod.Patches
{
    [HarmonyPatch(typeof(DressGirlAI))]
    internal class DressGirlAIPatch
    {
        [HarmonyPatch(nameof(DressGirlAI.Start))]
        [HarmonyPostfix]
        public static void StatOverride(DressGirlAI __instance)
        {
            if (Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                float speed = Convert.ToSingle(new System.Random().Next(20, 200)) / 10f;
                float health = Convert.ToSingle(new System.Random().Next(1, 11));
                float scale = Convert.ToSingle(new System.Random().Next(5, 21)) / 10;

                RandomizerValues.dressGirlSpeedsDict.Add(__instance.NetworkObjectId, speed);

                __instance.enemyHP = (int)health;
                __instance.transform.localScale = new Vector3(scale, scale, scale);

                __instance.creatureAnimator.speed = speed / 10f;
                __instance.creatureSFX.pitch = Mathf.Lerp(3f, 0.01f, Mathf.InverseLerp(0.5f, 2f, scale));
                __instance.creatureVoice.pitch = Mathf.Lerp(3f, 0.01f, Mathf.InverseLerp(0.5f, 2f, scale));

                FastBufferWriter fastBufferWriter = new FastBufferWriter(sizeof(ulong) + sizeof(float) * 3, Unity.Collections.Allocator.Temp, -1);
                fastBufferWriter.WriteValueSafe<ulong>(__instance.NetworkObjectId);
                fastBufferWriter.WriteValueSafe<float>(speed);
                fastBufferWriter.WriteValueSafe<float>(health);
                fastBufferWriter.WriteValueSafe<float>(scale);

                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesDressGirlData", fastBufferWriter, NetworkDelivery.Reliable);
                fastBufferWriter.Dispose();
            }
        }

        [HarmonyPatch(nameof(DressGirlAI.Update))]
        [HarmonyPostfix]
        public static void SpeedOverride(DressGirlAI __instance)
        {
            if (!__instance.isEnemyDead)
            {
                __instance.agent.speed = RandomizerValues.dressGirlSpeedsDict.GetValueSafe(__instance.NetworkObjectId);
            }
        }

        public static void SetGirlData(ulong _, FastBufferReader reader)
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

                RandomizerValues.dressGirlSpeedsDict.Add(id, speed);

                NetworkObject networkObject = Unity.Netcode.NetworkManager.Singleton.SpawnManager.SpawnedObjects[id];
                DressGirlAI dressGirl = networkObject.gameObject.GetComponentInChildren<DressGirlAI>();
                dressGirl.enemyHP = (int)health;
                dressGirl.transform.localScale = new Vector3(scale, scale, scale);

                //dressGirl.creatureAnimator.speed = speed / 10f;
                dressGirl.creatureSFX.pitch = Mathf.Lerp(3f, 0.01f, Mathf.InverseLerp(0.5f, 2f, scale));
                dressGirl.creatureVoice.pitch = Mathf.Lerp(3f, 0.01f, Mathf.InverseLerp(0.5f, 2f, scale));

                RandomizerModBase.mls.LogInfo("RECEIVED GIRL STATS: " + id + ", " + speed + ", " + health + ", " + scale);
            }
        }
    }
}
