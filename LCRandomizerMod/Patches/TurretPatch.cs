using HarmonyLib;
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;

namespace LCRandomizerMod.Patches
{
    [HarmonyPatch(typeof(Turret))]
    internal class TurretPatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        public static void StatOverride(Turret __instance)
        {
            if (Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                float scale = Convert.ToSingle(new System.Random().Next(1, 31)) / 10;
                float rotSpeed = Convert.ToSingle(new System.Random().Next(10, 401)) / 10;
                float rotAngle = Convert.ToSingle(new System.Random().Next(100, 901)) / 10;

                __instance.rotationSpeed = rotSpeed;
                __instance.rotationRange = rotAngle;
                __instance.transform.localScale = new Vector3(scale, scale, scale);
                //__instance.turretRod.localScale = new Vector3(scale, scale, scale);

                FastBufferWriter fastBufferWriter = new FastBufferWriter(sizeof(ulong) + sizeof(float), Unity.Collections.Allocator.Temp, -1);
                fastBufferWriter.WriteValueSafe<ulong>(__instance.NetworkObjectId);
                fastBufferWriter.WriteValueSafe<float>(scale);

                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesTurretData", fastBufferWriter, NetworkDelivery.Reliable);
            }   
        }

        public static void SetTurretStats(ulong _, FastBufferReader reader)
        {
            if (!Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                ulong id;
                float scale;

                reader.ReadValueSafe(out id);
                reader.ReadValueSafe(out scale);

                NetworkObject networkObject = Unity.Netcode.NetworkManager.Singleton.SpawnManager.SpawnedObjects[id];
                Turret turret = networkObject.gameObject.GetComponentInChildren<Turret>();

                turret.transform.localScale = new Vector3(scale, scale, scale);
                //turret.turretRod.localScale = new Vector3(scale, scale, scale);
            }
        }
    }
}