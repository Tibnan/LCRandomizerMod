using HarmonyLib;
using System;
using UnityEngine;
using Unity.Netcode;

namespace LCRandomizerMod.Patches
{
    [HarmonyPatch(typeof(SoccerBallProp))]
    internal class SoccerBallPropPatch
    {
        [HarmonyPatch("__initializeVariables")]
        [HarmonyPostfix]
        public static void RandomizeSoccerBall(SoccerBallProp __instance)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                float hitUpwardAmount = Convert.ToSingle(new System.Random().Next(1, 35) / 10);
                float scale = Convert.ToSingle(new System.Random().Next(1, 71) / 10);

                __instance.transform.localScale = new Vector3(scale, scale, scale);
                __instance.ballHitUpwardAmount = hitUpwardAmount;

                FastBufferWriter fastBufferWriter = new FastBufferWriter(sizeof(ulong) + sizeof(float) * 2, Unity.Collections.Allocator.Temp, -1);
                fastBufferWriter.WriteValueSafe<ulong>(__instance.NetworkObjectId);
                fastBufferWriter.WriteValueSafe<float>(hitUpwardAmount);
                fastBufferWriter.WriteValueSafe<float>(scale);

                NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesSoccerStats", fastBufferWriter, NetworkDelivery.Reliable);
            }
        }

        public static void ClientSetSoccerStats(ulong _, FastBufferReader reader)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                ulong id;
                float hitUpwardAmount;
                float scale;

                reader.ReadValueSafe<ulong>(out id);
                reader.ReadValueSafe<float>(out hitUpwardAmount);
                reader.ReadValueSafe<float>(out scale);

                NetworkObject networkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[id];
                SoccerBallProp ball = networkObject.gameObject.GetComponentInChildren<SoccerBallProp>();
                ball.transform.localScale = new Vector3(scale, scale, scale);
                ball.ballHitUpwardAmount = hitUpwardAmount;
            }
        }
    }
}
