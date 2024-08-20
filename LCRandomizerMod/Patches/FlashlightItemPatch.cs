using HarmonyLib;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
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
            if (!RandomizerValues.flashlightPropertyDict.ContainsKey(__instance.NetworkObjectId) && Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                RFlashlightProperties properties = new RFlashlightProperties();

                RandomizerValues.flashlightPropertyDict.Add(__instance.NetworkObjectId, properties);

                __instance.flashlightBulb.color = properties.BulbColor;
                __instance.flashlightBulb.intensity = properties.Intensity;
                __instance.mainObjectRenderer.material.color = properties.FlashlightBodyColor;

                FastBufferWriter writer = new FastBufferWriter(sizeof(ulong) + sizeof(float) * 7, Unity.Collections.Allocator.Temp, -1);
                writer.WriteValueSafe<ulong>(__instance.NetworkObjectId);
                writer.WriteValueSafe<float>(properties.BulbColor.r);
                writer.WriteValueSafe<float>(properties.BulbColor.g);
                writer.WriteValueSafe<float>(properties.BulbColor.b);
                writer.WriteValueSafe<float>(properties.Intensity);
                writer.WriteValueSafe<float>(properties.FlashlightBodyColor.r);
                writer.WriteValueSafe<float>(properties.FlashlightBodyColor.g);
                writer.WriteValueSafe<float>(properties.FlashlightBodyColor.b);

                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesFlashlightColor", writer, NetworkDelivery.Reliable);
            }
        }

        public static void SetFlashlightColor(ulong _, FastBufferReader reader)
        {
            if (!Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                ulong id;

                reader.ReadValueSafe<ulong>(out id);

                if (RandomizerValues.flashlightPropertyDict.ContainsKey(id)) return;

                float bR, bG, bB, intensity, fR, fG, fB;

                reader.ReadValueSafe<float>(out bR);
                reader.ReadValueSafe<float>(out bG);
                reader.ReadValueSafe<float>(out bB);
                reader.ReadValueSafe<float>(out intensity);
                reader.ReadValueSafe<float>(out fR);
                reader.ReadValueSafe<float>(out fB);
                reader.ReadValueSafe<float>(out fG);

                NetworkObject networkObject = Unity.Netcode.NetworkManager.Singleton.SpawnManager.SpawnedObjects[id];
                FlashlightItem flashlight = networkObject.gameObject.GetComponentInChildren<FlashlightItem>();
                RFlashlightProperties properties = new RFlashlightProperties(new Color(bR, bG, bB), new Color (fR, fG, fB), intensity);

                RandomizerValues.flashlightPropertyDict.Add(id, properties);

                flashlight.flashlightBulb.color = properties.BulbColor;
                flashlight.flashlightBulb.intensity = properties.Intensity;
                flashlight.mainObjectRenderer.material.color = properties.FlashlightBodyColor;
            }
        }

        public void SaveOnExit()
        {
            if (RandomizerValues.flashlightPropertyDict.Count > 0)
            {
                RandomizerModBase.mls.LogWarning(String.Format("Saving {0} flashlight entries", RandomizerValues.flashlightPropertyDict.Count));
                try
                {
                    ES3.Save("flashlightDict", RandomizerValues.flashlightPropertyDict, GameNetworkManager.Instance.currentSaveFileName);
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
            if (RandomizerValues.flashlightPropertyDict.Count > 0)
            {
                int idx = 0;
                RandomizerModBase.mls.LogInfo(String.Format("Reloading {0} flashlight color from dictionary. ", RandomizerValues.flashlightPropertyDict.Count));
                List<RFlashlightProperties> temp = RandomizerValues.flashlightPropertyDict.Values.ToList();
                RandomizerValues.flashlightPropertyDict.Clear();

                List<UnityEngine.Object> flashlightsInLevel = GameObject.FindObjectsByType(typeof(FlashlightItem), FindObjectsSortMode.None).ToList();
                RandomizerModBase.mls.LogError(flashlightsInLevel.Count);

                foreach (UnityEngine.Object obj in flashlightsInLevel)
                {
                    FlashlightItem flashlight = (FlashlightItem)obj;

                    RandomizerModBase.mls.LogInfo(flashlight.NetworkObjectId);

                    if (idx >= temp.Count) break;

                    RFlashlightProperties properties = temp.ElementAt(idx);
                    flashlight.flashlightBulb.color = properties.BulbColor;
                    flashlight.flashlightBulb.intensity = properties.Intensity;
                    flashlight.mainObjectRenderer.material.color = properties.FlashlightBodyColor;

                    RandomizerValues.flashlightPropertyDict.Add(flashlight.NetworkObjectId, properties);
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
            foreach (KeyValuePair<ulong, RFlashlightProperties> pair in RandomizerValues.flashlightPropertyDict)
            {
                FastBufferWriter writer = new FastBufferWriter(sizeof(ulong) + sizeof(float) * 7, Unity.Collections.Allocator.Temp, -1);
                writer.WriteValueSafe<ulong>(pair.Key);
                writer.WriteValueSafe<float>(pair.Value.BulbColor.r);
                writer.WriteValueSafe<float>(pair.Value.BulbColor.g);
                writer.WriteValueSafe<float>(pair.Value.BulbColor.b);
                writer.WriteValueSafe<float>(pair.Value.Intensity);
                writer.WriteValueSafe<float>(pair.Value.FlashlightBodyColor.r);
                writer.WriteValueSafe<float>(pair.Value.FlashlightBodyColor.g);
                writer.WriteValueSafe<float>(pair.Value.FlashlightBodyColor.b);

                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesFlashlightColor", writer, NetworkDelivery.Reliable);
            }
        }
    }
}
