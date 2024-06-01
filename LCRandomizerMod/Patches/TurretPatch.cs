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
                float scale = Convert.ToSingle(new System.Random().Next(1, 71)) / 10;
                float rotSpeed = Convert.ToSingle(new System.Random().Next(1, 401)) / 10;
                float rotAngle = Convert.ToSingle(new System.Random().Next(100, 901)) / 10;

                __instance.rotationSpeed = rotSpeed;
                __instance.rotationRange = rotAngle;
                __instance.centerPoint.transform.localScale += Vector3.down * 1.5f;
                //__instance.tempTransform.localScale = new Vector3(scale, scale, scale);
                //__instance.transform.localScale = new Vector3(scale, scale, scale);
                //__instance.turretRod.localScale = new Vector3(scale, scale, scale);

                FastBufferWriter fastBufferWriter = new FastBufferWriter(sizeof(ulong) + sizeof(float) * 2, Unity.Collections.Allocator.Temp, -1);
                fastBufferWriter.WriteValueSafe<ulong>(__instance.NetworkObjectId);
                //fastBufferWriter.WriteValueSafe<float>(scale);
                fastBufferWriter.WriteValueSafe<float>(rotSpeed);
                fastBufferWriter.WriteValueSafe<float>(rotAngle);

                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesTurretData", fastBufferWriter, NetworkDelivery.Reliable);
            }   
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        public static void Debug(Turret __instance)
        {
            if (__instance.targetPlayerWithRotation != null)
            {
                RandomizerModBase.mls.LogInfo("My target is: " + __instance.targetPlayerWithRotation.playerUsername);
            }
        }

        public static void SetTurretStats(ulong _, FastBufferReader reader)
        {
            if (!Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                ulong id;
                //float scale;
                float rotSpeed;
                float rotAngle;

                reader.ReadValueSafe<ulong>(out id);
                //reader.ReadValueSafe<float>(out scale);
                reader.ReadValueSafe<float>(out rotSpeed);
                reader.ReadValueSafe<float>(out rotAngle);

                NetworkObject networkObject = Unity.Netcode.NetworkManager.Singleton.SpawnManager.SpawnedObjects[id];
                Turret turret = networkObject.gameObject.GetComponentInChildren<Turret>();

                //turret.transform.localScale = new Vector3(scale, scale, scale);

                turret.rotationSpeed = rotSpeed;
                turret.rotationRange = rotAngle;

                turret.centerPoint.transform.localScale += Vector3.down * 1.5f;
                //turret.turretRod.localScale = new Vector3(scale, scale, scale);
            }
        }
    }
}