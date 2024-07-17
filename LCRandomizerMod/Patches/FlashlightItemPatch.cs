using HarmonyLib;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;

namespace LCRandomizerMod.Patches
{
    [HarmonyPatch(typeof(FlashlightItem))]
    internal class FlashlightItemPatch : ICustomValue
    {
        [HarmonyPatch(nameof(FlashlightItem.Start))]
        [HarmonyPostfix]
        public static void ChangeFlashlightColor(FlashlightItem __instance)
        {
            if (!RandomizerValues.flashlightColorDict.ContainsKey(__instance.NetworkObjectId) && Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                float r = new System.Random().Next(0, 200) / 100f;
                float g = new System.Random().Next(0, 200) / 100f;
                float b = new System.Random().Next(0, 200) / 100f;

                __instance.flashlightBulb.color = new Color(r, g, b);

                RandomizerValues.flashlightColorDict.Add(__instance.NetworkObjectId, __instance.flashlightBulb.color);

                FastBufferWriter writer = new FastBufferWriter(sizeof(ulong) + sizeof(float) * 3, Unity.Collections.Allocator.Temp, -1);
                writer.WriteValueSafe<ulong>(__instance.NetworkObjectId);
                writer.WriteValueSafe<float>(r);
                writer.WriteValueSafe<float>(g);
                writer.WriteValueSafe<float>(b);

                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesFlashlightColor", writer, NetworkDelivery.Reliable);
            }
        }

        public static void SetFlashlightColor(ulong _, FastBufferReader reader)
        {
            if (!Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                ulong id;
                float r;
                float g;
                float b;

                reader.ReadValueSafe<ulong>(out id);

                if (RandomizerValues.flashlightColorDict.ContainsKey(id)) return;

                reader.ReadValueSafe<float>(out r);
                reader.ReadValueSafe<float>(out g);
                reader.ReadValueSafe<float>(out b);

                NetworkObject networkObject = Unity.Netcode.NetworkManager.Singleton.SpawnManager.SpawnedObjects[id];
                FlashlightItem flashlight = networkObject.gameObject.GetComponentInChildren<FlashlightItem>();

                flashlight.flashlightBulb.color = new Color(r, g, b);

                RandomizerValues.flashlightColorDict.Add(id, flashlight.flashlightBulb.color);
            }
        }

        public void SaveOnExit()
        {
            if (RandomizerValues.flashlightColorDict.Count > 0)
            {
                RandomizerModBase.mls.LogWarning(String.Format("Saving {0} flashlight entries", RandomizerValues.flashlightColorDict.Count));
                try
                {
                    ES3.Save("flashlightDict", RandomizerValues.flashlightColorDict, GameNetworkManager.Instance.currentSaveFileName);
                    if (!RandomizerValues.keysToLoad.Contains("flashlightDict"))
                    {
                        RandomizerValues.keysToLoad.Add("flashlightDict");
                    }
                }
                catch (Exception ex)
                {
                    RandomizerModBase.mls.LogError("Exception caught during custom value serialization. [FlahlightItem] " + ex.Message);
                }
            }
            else if (RandomizerValues.keysToLoad.Contains("flashlightDict"))
            {
                RandomizerValues.keysToLoad.Remove("flashlightDict");
            }
        }

        public void ReloadStats()
        {
            if (RandomizerValues.flashlightColorDict.Count > 0)
            {
                int idx = 0;
                RandomizerModBase.mls.LogInfo(String.Format("Reloading {0} flashlight color from dictionary. ", RandomizerValues.flashlightColorDict.Count));
                List<Color> temp = RandomizerValues.flashlightColorDict.Values.ToList();
                RandomizerValues.flashlightColorDict.Clear();

                List<UnityEngine.Object> flashlightsInLevel = GameObject.FindObjectsByType(typeof(FlashlightItem), FindObjectsSortMode.None).ToList();

                foreach (UnityEngine.Object obj in flashlightsInLevel)
                {
                    FlashlightItem flashlight = (FlashlightItem)obj;

                    RandomizerModBase.mls.LogInfo(flashlight.NetworkObjectId);

                    if (idx >= temp.Count) break;

                    flashlight.flashlightBulb.color = temp.ElementAt(idx);

                    RandomizerValues.flashlightColorDict.Add(flashlight.NetworkObjectId, flashlight.flashlightBulb.color);
                    idx++;
                }

                RandomizerModBase.mls.LogInfo("Reloaded flashlight colors from dictionary.");
            }
            else
            {
                RandomizerModBase.mls.LogInfo("No flashlight colors to reload.");
            }
        }
        
        public void SyncStatsWithClients()
        {
            foreach (KeyValuePair<ulong, Color> pair in RandomizerValues.flashlightColorDict)
            {
                FastBufferWriter writer = new FastBufferWriter(sizeof(ulong) + sizeof(float) * 3, Unity.Collections.Allocator.Temp, -1);
                writer.WriteValueSafe<ulong>(pair.Key);
                writer.WriteValueSafe<float>(pair.Value.r);
                writer.WriteValueSafe<float>(pair.Value.g);
                writer.WriteValueSafe<float>(pair.Value.b);

                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesFlashlightColor", writer, NetworkDelivery.Reliable);
            }
        }
    }
}
