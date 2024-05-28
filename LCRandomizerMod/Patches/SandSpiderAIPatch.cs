using HarmonyLib;
using System;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace LCRandomizerMod.Patches
{
    [HarmonyPatch(typeof(SandSpiderAI))]
    internal class SandSpiderAIPatch
    {
        [HarmonyPatch(nameof(SandSpiderAI.Start))]
        [HarmonyPostfix]
        public static void SpiderWebAmountOverride(SandSpiderAI __instance)
        {
            if (Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                __instance.maxWebTrapsToPlace = new System.Random().Next(4, 14);
                float spiderHealthRand = new System.Random().Next(1, 7);
                float spiderSpeedRand = Convert.ToSingle(new System.Random().Next(35, 101)) / 10;
                float spiderScaleRand = Convert.ToSingle(new System.Random().Next(1, 31)) / 10;

                RandomizerValues.spiderSpeedsDict.Add(__instance.NetworkObjectId, spiderSpeedRand);

                __instance.enemyHP = (int)spiderHealthRand;
                __instance.transform.localScale = new Vector3(spiderScaleRand, spiderScaleRand, spiderScaleRand);
                __instance.creatureAnimator.speed = spiderSpeedRand / 10f;
                __instance.creatureSFX.pitch = Mathf.Lerp(3f, 0.01f, Mathf.InverseLerp(0.1f, 3f, spiderScaleRand));

                FastBufferWriter fastBufferSpiderWriter = new FastBufferWriter(sizeof(ulong) + sizeof(float) * 3, Unity.Collections.Allocator.Temp, -1);
                fastBufferSpiderWriter.WriteValueSafe<ulong>(__instance.NetworkObjectId);
                fastBufferSpiderWriter.WriteValueSafe<float>(spiderSpeedRand);
                fastBufferSpiderWriter.WriteValueSafe<float>(spiderHealthRand);
                fastBufferSpiderWriter.WriteValueSafe<float>(spiderScaleRand);

                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesSpiderData", fastBufferSpiderWriter, NetworkDelivery.Reliable);
            }
        }

        [HarmonyPatch(nameof(SandSpiderAI.Update))]
        [HarmonyPostfix]
        public static void SpiderSpeedOverride(SandSpiderAI __instance)
        {
            if (!__instance.isEnemyDead)
            {
                __instance.spiderSpeed = RandomizerValues.spiderSpeedsDict.GetValueSafe(__instance.NetworkObjectId);
                __instance.agent.speed = RandomizerValues.spiderSpeedsDict.GetValueSafe(__instance.NetworkObjectId);
            }
        }

        public static void SetSpiderDataSentByServer(ulong _, FastBufferReader reader)
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

                RandomizerValues.spiderSpeedsDict.Add(id, speed);

                NetworkObject networkObject = Unity.Netcode.NetworkManager.Singleton.SpawnManager.SpawnedObjects[id];
                SandSpiderAI spider = networkObject.gameObject.GetComponentInChildren<SandSpiderAI>();

                spider.enemyHP = (int)health;
                spider.transform.localScale = new Vector3(scale, scale, scale);
                spider.creatureAnimator.speed = speed / 10f;

                RandomizerModBase.mls.LogInfo("RECEIVED SPIDER STATS: " + id + ", " + speed + ", " + health + ", " + scale);
            }
        }

        [HarmonyPatch(nameof(SandSpiderAI.MoveLegsProcedurally))]
        [HarmonyPostfix]
        public static void SoundOverride(SandSpiderAI __instance)
        {
            __instance.footstepAudio.Stop();

            float scale = __instance.transform.localScale.x;
            __instance.footstepAudio.pitch = Mathf.Lerp(3f, 0.01f, Mathf.InverseLerp(0.1f, 3f, scale));
            __instance.footstepAudio.PlayOneShot(__instance.footstepSFX[UnityEngine.Random.Range(0, __instance.footstepSFX.Length)], UnityEngine.Random.Range(0.1f, 1f));
        }
    }
}
