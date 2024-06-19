using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace LCRandomizerMod.Patches
{
    [HarmonyPatch(typeof(JetpackItem))]
    internal class JetpackItemPatch : ICustomValue
    {
        [HarmonyPatch(nameof(JetpackItem.EquipItem))]
        [HarmonyPostfix]
        public static void JetpackStatOverride(JetpackItem __instance)
        {
            if (!RandomizerValues.jetpackPropertiesDict.ContainsKey(__instance.NetworkObjectId) && Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                RandomizerModBase.mls.LogInfo("ID: " + __instance.NetworkObjectId);
                RandomizerModBase.mls.LogError("BATTERY USAGE: " + __instance.itemProperties.batteryUsage);

                float jetpackAccel = Convert.ToSingle(new System.Random().Next(10, 301)) / 10f;
                float jetpackDecel = Convert.ToSingle(new System.Random().Next(10, 301)) / 10f;
                //float bUsage = Convert.ToSingle(new System.Random().Next(10, 201)) / 10f;

                __instance.jetpackAcceleration = jetpackAccel;
                __instance.jetpackDeaccelleration = jetpackDecel;
                //__instance.itemProperties.batteryUsage = bUsage;

                RandomizerValues.jetpackPropertiesDict.Add(__instance.NetworkObjectId, new Tuple<float, float>(jetpackAccel, jetpackDecel));

                FastBufferWriter fastBufferWriter = new FastBufferWriter(sizeof(ulong) + sizeof(float) * 2, Unity.Collections.Allocator.Temp, -1);
                fastBufferWriter.WriteValueSafe<ulong>(__instance.NetworkObjectId);
                fastBufferWriter.WriteValueSafe<float>(jetpackAccel);
                fastBufferWriter.WriteValueSafe<float>(jetpackDecel);

                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesJetpackData", fastBufferWriter, NetworkDelivery.Reliable);

                //HUDManager.Instance.AddTextToChatOnServer("<color=red>WARNING: Jetpack stat saving is not yet implemented! They will behave differently each time you restart the server.</color>", -1);
            }
        }

        public static void SetJetpackStatsSentByServer(ulong _, FastBufferReader reader)
        {
            if (!Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                ulong id;
                float acc;
                float dec;

                reader.ReadValueSafe<ulong>(out id);
                if (RandomizerValues.jetpackPropertiesDict.ContainsKey(id))
                {
                    RandomizerModBase.mls.LogInfo("CONTAINS KEY");
                    return;
                }
                reader.ReadValueSafe<float>(out acc);
                reader.ReadValueSafe<float>(out dec);

                RandomizerModBase.mls.LogInfo("ADDING DICT");
                RandomizerValues.jetpackPropertiesDict.Add(id, new Tuple<float, float>(acc, dec));

                RandomizerModBase.mls.LogInfo("CONVERTING ID");
                NetworkObject networkObject = Unity.Netcode.NetworkManager.Singleton.SpawnManager.SpawnedObjects[id];
                JetpackItem jetpackItem = networkObject.gameObject.GetComponentInChildren<JetpackItem>();

                jetpackItem.jetpackAcceleration = acc;
                jetpackItem.jetpackDeaccelleration = dec;

                RandomizerModBase.mls.LogInfo("RECEIVED JETPACK STATS: " + id + ", " + acc + ", " + dec);
            }
        }

        public void ReloadStats()
        {
            if (RandomizerValues.jetpackPropertiesDict.Count > 0)
            {
                int idx = 0;
                RandomizerModBase.mls.LogInfo(String.Format("Reloading {0} jetpack stat from dictionary. ", RandomizerValues.jetpackPropertiesDict.Count));
                List<Tuple<float, float>> temp = RandomizerValues.jetpackPropertiesDict.Values.ToList();
                RandomizerValues.jetpackPropertiesDict.Clear();

                List<UnityEngine.Object> jetpacksInLevel = GameObject.FindObjectsByType(typeof(JetpackItem), FindObjectsSortMode.None).ToList();

                foreach (UnityEngine.Object obj in jetpacksInLevel)
                {
                    JetpackItem jetpack = (JetpackItem)obj;

                    RandomizerModBase.mls.LogInfo(jetpack.NetworkObjectId);

                    if (idx >= temp.Count) break;

                    jetpack.jetpackAcceleration = temp.ElementAt(idx).Item1;
                    jetpack.jetpackDeaccelleration = temp.ElementAt(idx).Item2;

                    RandomizerValues.jetpackPropertiesDict.Add(jetpack.NetworkObjectId, new Tuple<float, float>(temp.ElementAt(idx).Item1, temp.ElementAt(idx).Item2));
                    idx++;
                }

                RandomizerModBase.mls.LogInfo("Reloaded jetpack stats from dictionary.");
            }
            else
            {
                RandomizerModBase.mls.LogInfo("No jetpack stats to reload.");
            }
        }

        public void SyncStatsWithClients()
        {
            foreach (KeyValuePair<ulong, Tuple<float, float>> pair in RandomizerValues.jetpackPropertiesDict)
            {
                FastBufferWriter writer = new FastBufferWriter(sizeof(ulong) + sizeof(float) * 2, Unity.Collections.Allocator.Temp, -1);
                writer.WriteValueSafe<ulong>(pair.Key);
                writer.WriteValueSafe<float>(pair.Value.Item1);
                writer.WriteValueSafe<float>(pair.Value.Item2);

                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesJetpackData", writer, NetworkDelivery.Reliable);
            }
        }

        public void SaveOnExit()
        {
            if (RandomizerValues.jetpackPropertiesDict.Count > 0)
            {
                RandomizerModBase.mls.LogWarning(String.Format("Saving {0} jetpack entries", RandomizerValues.jetpackPropertiesDict.Count));
                try
                {
                    ES3.Save("jetpackDict", RandomizerValues.jetpackPropertiesDict, GameNetworkManager.Instance.currentSaveFileName);
                    if (!RandomizerValues.keysToLoad.Contains("jetpackDict"))
                    {
                        RandomizerValues.keysToLoad.Add("jetpackDict");
                    }

                } catch (Exception ex)
                {
                    RandomizerModBase.mls.LogError("Exception caught during custom value serialization. [JetpackItem] " + ex.Message);
                }
            }
            else if (RandomizerValues.keysToLoad.Contains("jetpackDict"))
            {
                RandomizerValues.keysToLoad.Remove("jetpackDict");
            }
        }
    }
}
