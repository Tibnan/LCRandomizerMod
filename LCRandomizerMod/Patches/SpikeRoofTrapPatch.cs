using HarmonyLib;
using System;
using Unity.Netcode;
using UnityEngine;

namespace LCRandomizerMod.Patches
{
    [HarmonyPatch(typeof(SpikeRoofTrap))]
    internal class SpikeRoofTrapPatch
    {
        [HarmonyPatch(nameof(SpikeRoofTrap.Start))]
        [HarmonyPostfix]
        public static void SlamIntervalOverride(SpikeRoofTrap __instance)
        {
            if (Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                float slamInterval = Convert.ToSingle(new System.Random().Next(1, 51)) / 10;
                float scale = Convert.ToSingle(new System.Random().Next(5, 16)) / 10;

                Traverse.Create(__instance).Field("slamInterval").SetValue(slamInterval);
                __instance.transform.localScale = new Vector3(scale, scale, scale);

                FastBufferWriter fastBufferWriter = new FastBufferWriter(sizeof(ulong) + sizeof(float) * 2, Unity.Collections.Allocator.Temp, -1);

                fastBufferWriter.WriteValueSafe<ulong>(__instance.NetworkObjectId);
                fastBufferWriter.WriteValueSafe<float>(slamInterval);
                fastBufferWriter.WriteValueSafe<float>(scale);

                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesSpikeData", fastBufferWriter, NetworkDelivery.Reliable);
            }
        }

        public static void SetSpikeStats(ulong _, FastBufferReader reader)
        {
            if (!Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                ulong id;
                float slamInterval;
                float scale;

                reader.ReadValueSafe<ulong>(out id);
                reader.ReadValueSafe<float>(out slamInterval);
                reader.ReadValueSafe<float>(out scale);

                NetworkObject networkObject = Unity.Netcode.NetworkManager.Singleton.SpawnManager.SpawnedObjects[id];
                SpikeRoofTrap trap = networkObject.gameObject.GetComponentInChildren<SpikeRoofTrap>();

                Traverse.Create(trap).Field("slamInterval").SetValue(slamInterval);
                trap.transform.localScale = new Vector3(scale, scale, scale);
            }
        }
    }
}
