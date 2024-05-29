using System;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace LCRandomizerMod.Patches
{
    [HarmonyPatch(typeof(RedLocustBees))]
    internal class RedLocustBeesPatch
    {
        [HarmonyPatch(nameof(RedLocustBees.Start))]
        [HarmonyPostfix]
        public static void StatOverride(RedLocustBees __instance)
        {
            if (Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                float speed = Convert.ToSingle(new System.Random().Next(20, 200)) / 10f;
                float health = Convert.ToSingle(new System.Random().Next(1, 11));
                float scale = Convert.ToSingle(new System.Random().Next(5, 21)) / 10;

                RandomizerValues.redLocustSpeedsDict.Add(__instance.NetworkObjectId, speed);

                __instance.enemyHP = (int)health;
                __instance.transform.localScale = new Vector3(scale, scale, scale);

                //__instance.creatureAnimator.speed = speed / 10f;
                __instance.beesAngry.pitch = Mathf.Lerp(3f, 0.1f, Mathf.InverseLerp(0.5f, 2f, scale));
                __instance.beesDefensive.pitch = Mathf.Lerp(3f, 0.1f, Mathf.InverseLerp(0.5f, 2f, scale));
                __instance.beesIdle.pitch = Mathf.Lerp(3f, 0.1f, Mathf.InverseLerp(0.5f, 2f, scale));

                FastBufferWriter fastBufferWriter = new FastBufferWriter(sizeof(ulong) + sizeof(float) * 3, Unity.Collections.Allocator.Temp, -1);
                fastBufferWriter.WriteValueSafe<ulong>(__instance.NetworkObjectId);
                fastBufferWriter.WriteValueSafe<float>(speed);
                fastBufferWriter.WriteValueSafe<float>(health);
                fastBufferWriter.WriteValueSafe<float>(scale);

                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesBeeData", fastBufferWriter, NetworkDelivery.Reliable);
                fastBufferWriter.Dispose();
            }
        }

        [HarmonyPatch(nameof(RedLocustBees.Update))]
        [HarmonyPostfix]
        public static void SpeedOverride(RedLocustBees __instance)
        {
            if (!__instance.isEnemyDead)
            {
                __instance.agent.speed = RandomizerValues.redLocustSpeedsDict.GetValueSafe(__instance.NetworkObjectId);
            }
        }

        public static void SetBeeStats(ulong _, FastBufferReader reader)
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

                RandomizerValues.redLocustSpeedsDict.Add(id, speed);

                NetworkObject networkObject = Unity.Netcode.NetworkManager.Singleton.SpawnManager.SpawnedObjects[id];
                RedLocustBees bee = networkObject.gameObject.GetComponentInChildren<RedLocustBees>();
                bee.enemyHP = (int)health;
                bee.transform.localScale = new Vector3(scale, scale, scale);

                //bee.creatureAnimator.speed = speed / 10f;
                bee.beesAngry.pitch = Mathf.Lerp(3f, 0.1f, Mathf.InverseLerp(0.5f, 2f, scale));
                bee.beesDefensive.pitch = Mathf.Lerp(3f, 0.1f, Mathf.InverseLerp(0.5f, 2f, scale));
                bee.beesIdle.pitch = Mathf.Lerp(3f, 0.1f, Mathf.InverseLerp(0.5f, 2f, scale));

                RandomizerModBase.mls.LogInfo("RECEIVED BEE STATS: " + id + ", " + speed + ", " + health + ", " + scale);
            }
        }
    }
}
