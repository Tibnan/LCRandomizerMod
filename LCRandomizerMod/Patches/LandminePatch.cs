using HarmonyLib;
using System;
using Unity.Netcode;
using UnityEngine;

namespace LCRandomizerMod.Patches
{
    [HarmonyPatch(typeof(Landmine))]
    internal class LandminePatch
    {
        [HarmonyPatch(nameof(Landmine.SetOffMineAnimation))]
        [HarmonyPrefix]
        public static bool RandomlySpawnShotgun(Landmine __instance)
        {
            if (Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                if (new System.Random().Next(1, 5) == 4) //SET BEFORE RELEASE
                {
                    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(RandomizerValues.allItemsListDict.GetValueSafe("Shotgun").spawnPrefab, __instance.transform.position, Quaternion.identity, RoundManager.Instance.spawnedScrapContainer);
                    gameObject.GetComponent<GrabbableObject>().fallTime = 0f;
                    gameObject.GetComponent<NetworkObject>().Spawn(false);
                    RandomizerModBase.mls.LogWarning("SPAWNED SHOTGUN");

                    Unity.Netcode.NetworkManager.Singleton.SpawnManager.SpawnedObjects[__instance.NetworkObjectId].Despawn(true);
                    return false;
                }
                return true;
            }
            else
            {
                RandomizerModBase.mls.LogInfo("MINE ID: " + __instance.NetworkObjectId);
                return true;
            }
        }

        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        public static void SizeOverride(Landmine __instance)
        {
            if (Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                float scale = Convert.ToSingle(new System.Random().Next(5, 31)) / 10;
                __instance.transform.localScale = new Vector3(scale, scale, scale);

                FastBufferWriter fastBufferWriter = new FastBufferWriter(sizeof(ulong) + sizeof(float), Unity.Collections.Allocator.Temp, -1);

                fastBufferWriter.WriteValueSafe<ulong>(__instance.NetworkObjectId);
                fastBufferWriter.WriteValueSafe<float>(scale);

                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesMineData", fastBufferWriter, NetworkDelivery.Reliable);
            }
        }

        public static void SetMineSizeClient(ulong _, FastBufferReader reader)
        {

        }
    }
}
