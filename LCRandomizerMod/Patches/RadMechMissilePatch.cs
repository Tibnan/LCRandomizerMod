using HarmonyLib;
using Unity.Netcode;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Animations;

namespace LCRandomizerMod.Patches
{
    [HarmonyPatch(typeof(RadMechMissile))]
    internal class RadMechMissilePatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        public static void ResizeRocket(RadMechMissile __instance)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                float scale = Convert.ToSingle(new System.Random().Next(50, 1750)) / 100f;

                __instance.gameObject.transform.localScale = new Vector3(scale, scale, scale);

                FastBufferWriter fastBufferWriter = new FastBufferWriter(sizeof(ulong) + sizeof(float), Unity.Collections.Allocator.Temp, -1);
                fastBufferWriter.WriteValueSafe<ulong>(__instance.GetComponentInParent<NetworkObject>().NetworkObjectId);
                fastBufferWriter.WriteValue<float>(scale);

                NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesMissileScale", fastBufferWriter, NetworkDelivery.Reliable);
            }
        }

        public static void SetMissileSize(ulong _, FastBufferReader reader)
        {
            ulong id;
            float scale;

            reader.ReadValue<ulong>(out id);
            reader.ReadValueSafe<float>(out scale);

            NetworkObject networkObject = Unity.Netcode.NetworkManager.Singleton.SpawnManager.SpawnedObjects[id];
            RadMechMissile missile = networkObject.gameObject.GetComponentInChildren<RadMechMissile>();

            missile.transform.localScale = new Vector3(scale, scale, scale);
        }
    }
}
