using HarmonyLib;
using System;
using Unity.Netcode;
using UnityEngine;

namespace LCRandomizerMod.Patches
{
    [HarmonyPatch(typeof(CentipedeAI))]
    internal class CentipedeAIPatch
    {
        [HarmonyPatch(nameof(CentipedeAI.Start))]
        [HarmonyPostfix]
        public static void StatOverride(CentipedeAI __instance)
        {
            if (Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                float speed = Convert.ToSingle(new System.Random().Next(20, 200)) / 10f;
                float health = Convert.ToSingle(new System.Random().Next(1, 11));
                float scale = Convert.ToSingle(new System.Random().Next(5, 21)) / 10;

                RandomizerValues.centipedeSpeedsDict.Add(__instance.NetworkObjectId, speed);

                __instance.enemyHP = (int)health;
                __instance.transform.localScale = new Vector3(scale, scale, scale);

                __instance.creatureAnimator.speed = speed / 10;
                __instance.creatureSFX.pitch = Mathf.Lerp(3f, 0.01f, Mathf.InverseLerp(0.5f, 2f, scale));
                __instance.creatureVoice.pitch = Mathf.Lerp(3f, 0.01f, Mathf.InverseLerp(0.5f, 2f, scale));

                FastBufferWriter fastBufferWriter = new FastBufferWriter(sizeof(ulong) + sizeof(float) * 3, Unity.Collections.Allocator.Temp, -1);
                fastBufferWriter.WriteValueSafe<ulong>(__instance.NetworkObjectId);
                fastBufferWriter.WriteValueSafe<float>(speed);
                fastBufferWriter.WriteValueSafe<float>(health);
                fastBufferWriter.WriteValueSafe<float>(scale);

                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesCentipedeData", fastBufferWriter, NetworkDelivery.Reliable);
                fastBufferWriter.Dispose();
            }
        }

        [HarmonyPatch(nameof(CentipedeAI.Update))]
        [HarmonyPostfix]
        public static void SpeedOverride(CentipedeAI __instance)
        {
            if (!__instance.isEnemyDead)
            {
                __instance.agent.speed = RandomizerValues.centipedeSpeedsDict.GetValueSafe(__instance.NetworkObjectId);
            }
        }

        public static void SetCentipedeData(ulong _, FastBufferReader reader)
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

                RandomizerValues.centipedeSpeedsDict.Add(id, speed);

                NetworkObject networkObject = Unity.Netcode.NetworkManager.Singleton.SpawnManager.SpawnedObjects[id];
                CentipedeAI centipede = networkObject.gameObject.GetComponentInChildren<CentipedeAI>();
                centipede.enemyHP = (int)health;
                centipede.transform.localScale = new Vector3(scale, scale, scale);

                centipede.creatureAnimator.speed = speed / 10;
                centipede.creatureSFX.pitch = Mathf.Lerp(3f, 0.01f, Mathf.InverseLerp(0.5f, 2f, scale));
                centipede.creatureVoice.pitch = Mathf.Lerp(3f, 0.01f, Mathf.InverseLerp(0.5f, 2f, scale));

                RandomizerModBase.mls.LogInfo("RECEIVED CENTIPEDE STATS: " + id + ", " + speed + ", " + health + ", " + scale);
            }
        }
    }
}
