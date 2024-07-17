using HarmonyLib;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;

namespace LCRandomizerMod.Patches
{
    [HarmonyPatch(typeof(KeyItem))]
    internal class KeyItemPatch : ICustomValue
    {
        //public static bool TP = false;

        [HarmonyPatch("__initializeVariables")]
        [HarmonyPostfix]
        public static void ChanceForKeyToBeSupercharged(KeyItem __instance)
        {
            if (!RandomizerValues.superchargedKeys.Contains(__instance.NetworkObjectId) && Unity.Netcode.NetworkManager.Singleton.IsServer && new System.Random().Next(0, 2) == 1)
            {
                RandomizerValues.superchargedKeys.Add(__instance.NetworkObjectId);

                __instance.mainObjectRenderer.material.color = new Color(0f, 0f, 0.5f);

                FastBufferWriter writer = new FastBufferWriter(sizeof(ulong), Unity.Collections.Allocator.Temp, -1);
                writer.WriteValueSafe<ulong>(__instance.NetworkObjectId);

                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesSuperKey", writer, NetworkDelivery.Reliable);
            }
        }

        [HarmonyPatch(nameof(KeyItem.ItemActivate))]
        [HarmonyPrefix]
        public static void BlockDespawnOnKey(KeyItem __instance)
        {
            //if (!TP)
            //{
            //    DoorLock[] locks = UnityEngine.Object.FindObjectsOfType<DoorLock>();
            //    if (locks.Length > 0)
            //    {
            //        foreach (DoorLock doorLock in locks)
            //        {
            //            if (doorLock.isLocked)
            //            {
            //                GameNetworkManager.Instance.localPlayerController.TeleportPlayer(doorLock.transform.position);
            //                TP = true;
            //            }
            //        }
            //        return;
            //    }
            //}

            if (__instance.playerHeldBy == null)
            {
                return;
            }

            RaycastHit raycastHit;
            if (GameNetworkManager.Instance.localPlayerController == __instance.playerHeldBy && Physics.Raycast(new Ray(__instance.playerHeldBy.gameplayCamera.transform.position, __instance.playerHeldBy.gameplayCamera.transform.forward), out raycastHit, 3f, 2816) && RandomizerValues.superchargedKeys.Contains(__instance.NetworkObjectId))
            {
                DoorLock component = raycastHit.transform.GetComponent<DoorLock>();
                if (component != null && component.isLocked && !component.isPickingLock)
                {
                    RandomizerValues.blockDespawn = true;
                    //RandomizerModBase.mls.LogError("BLOCKING DESPAWN");
                }
            }
        }

        public static void ClientReceivesSuperKey(ulong _, FastBufferReader reader)
        {
            if (!Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                ulong id;

                reader.ReadValueSafe<ulong>(out id);

                if (RandomizerValues.superchargedKeys.Contains(id)) return;

                RandomizerValues.superchargedKeys.Add(id);

                NetworkObject networkObject = Unity.Netcode.NetworkManager.Singleton.SpawnManager.SpawnedObjects[id];

                KeyItem key = networkObject.gameObject.GetComponentInChildren<KeyItem>();

                key.mainObjectRenderer.material.color = new Color(0f, 0f, 0.5f);
            }
        }

        public void SaveOnExit()
        {
            if (RandomizerValues.superchargedKeys.Count > 0)
            {
                RandomizerModBase.mls.LogWarning(String.Format("Saving {0} supercharged keys.", RandomizerValues.superchargedKeys.Count));
                try
                {
                    ES3.Save("superKeys", RandomizerValues.superchargedKeys, GameNetworkManager.Instance.currentSaveFileName);
                    if (!RandomizerValues.keysToLoad.Contains("superKeys"))
                    {
                        RandomizerValues.keysToLoad.Add("superKeys");
                    }
                }
                catch (Exception ex)
                {
                    RandomizerModBase.mls.LogError("Exception caught during custom value serialization. [KeyItem] " + ex.Message);
                }
            }
            else if (RandomizerValues.keysToLoad.Contains("superKeys"))
            {
                RandomizerValues.keysToLoad.Remove("superKeys");
            }
        }

        public void ReloadStats()
        {
            if (RandomizerValues.superchargedKeys.Count > 0)
            {
                int idx = 0;
                RandomizerModBase.mls.LogInfo(String.Format("Reloading {0} supercharged keys.", RandomizerValues.superchargedKeys.Count));
                List<ulong> temp = RandomizerValues.superchargedKeys;
                RandomizerValues.superchargedKeys.Clear();

                List<UnityEngine.Object> keysInLevel = GameObject.FindObjectsByType(typeof(KeyItem), FindObjectsSortMode.None).ToList();

                foreach (UnityEngine.Object obj in keysInLevel)
                {
                    KeyItem key = (KeyItem)obj;

                    RandomizerModBase.mls.LogInfo(key.NetworkObjectId);

                    if (idx >= temp.Count) break;

                    RandomizerValues.superchargedKeys.Add(key.NetworkObjectId);
                    idx++;
                }

                RandomizerModBase.mls.LogInfo("Reloaded supercharged keys.");
            }
            else
            {
                RandomizerModBase.mls.LogInfo("No supercharged keys to reload.");
            }
        }

        public void SyncStatsWithClients()
        {
            foreach (ulong id in RandomizerValues.superchargedKeys)
            {
                FastBufferWriter writer = new FastBufferWriter(sizeof(ulong), Unity.Collections.Allocator.Temp, -1);
                writer.WriteValueSafe<ulong>(id);

                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesSuperKey", writer, NetworkDelivery.Reliable);
            }
        }
    }
}
