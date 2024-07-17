using Unity.Netcode;
using HarmonyLib;
using System;
using UnityEngine;
using System.Collections;

namespace LCRandomizerMod.Patches
{
    [HarmonyPatch(typeof(ClaySurgeonAI))]
    internal class ClaySurgeonAIPatch
    {
        [HarmonyPatch(nameof(ClaySurgeonAI.Start))]
        [HarmonyPostfix]
        public static void RandomizeSurgeon(ClaySurgeonAI __instance)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                GameNetworkManager.Instance.StartCoroutine(WaitUntilMasterIsChosen(__instance));
            }
        }

        public static void SetSurgeonData(ulong _, FastBufferReader reader)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                ulong id;
                float speed;
                float scale;
                float startInterval;

                reader.ReadValueSafe<ulong>(out id);
                reader.ReadValueSafe<float>(out speed);
                reader.ReadValueSafe<float>(out scale);
                reader.ReadValueSafe<float>(out startInterval);

                RandomizerValues.surgeonSpeedDict.Add(id, speed);

                NetworkObject networkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[id];
                ClaySurgeonAI surgeon = networkObject.gameObject.GetComponentInChildren<ClaySurgeonAI>();

                surgeon.transform.localScale = new Vector3(scale, scale, scale);
                surgeon.startingInterval = startInterval;
                surgeon.endingInterval = startInterval / 10f;
            }
        }

        public static IEnumerator WaitUntilMasterIsChosen(ClaySurgeonAI __instance)
        {
            yield return new WaitUntil(() => __instance.master != null);

            float speed = Convert.ToSingle(new System.Random().Next(20, 200)) / 10f;
            float scale = Convert.ToSingle(new System.Random().Next(5, 21)) / 10;
            float startInterval;
            if (__instance.master == __instance)
            {
                startInterval = new System.Random().Next(1, 6);

                __instance.startingInterval = startInterval;
                __instance.endingInterval = startInterval / 10f;
            }
            else
            {
                startInterval = __instance.master.startingInterval;
                __instance.endingInterval = __instance.master.endingInterval;
            }

            RandomizerValues.surgeonSpeedDict.Add(__instance.NetworkObjectId, speed);

            __instance.transform.localScale = new Vector3(scale, scale, scale);

            FastBufferWriter fastBufferWriter = new FastBufferWriter(sizeof(ulong) + sizeof(float) * 3, Unity.Collections.Allocator.Temp, -1);

            fastBufferWriter.WriteValueSafe<ulong>(__instance.NetworkObjectId);
            fastBufferWriter.WriteValueSafe<float>(speed);
            fastBufferWriter.WriteValueSafe<float>(scale);
            fastBufferWriter.WriteValueSafe<float>(startInterval);

            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesSurgeonData", fastBufferWriter, NetworkDelivery.Reliable);
        }
    }
}
