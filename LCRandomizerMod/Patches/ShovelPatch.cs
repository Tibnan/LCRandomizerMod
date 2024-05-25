using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace LCRandomizerMod.Patches
{
    [HarmonyPatch(typeof(Shovel))]
    internal class ShovelPatch : ICustomValue
    {
        [HarmonyPatch(nameof(Shovel.HitShovel))]
        [HarmonyPrefix]
        public static void RandomizeShovelDamage(Shovel __instance)
        {
            if (Unity.Netcode.NetworkManager.Singleton.IsServer && !RandomizerValues.shovelDamageDict.ContainsKey(__instance.NetworkObjectId))
            {
                __instance.shovelHitForce = new System.Random().Next(1, 11);

                RandomizerValues.shovelDamageDict.Add(__instance.NetworkObjectId, __instance.shovelHitForce);

                FastBufferWriter fastBufferWriter = new FastBufferWriter(sizeof(ulong) + sizeof(int), Unity.Collections.Allocator.Temp, -1);
                fastBufferWriter.WriteValueSafe<ulong>(__instance.NetworkObjectId);
                fastBufferWriter.WriteValueSafe<int>(__instance.shovelHitForce);

                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesShovelData", fastBufferWriter, NetworkDelivery.Reliable);
            }
        }

        public static void SetShovelData(ulong _, FastBufferReader reader)
        {
            if (!Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                ulong id;
                int damage;

                reader.ReadValueSafe(out id);
                reader.ReadValueSafe(out damage);

                if (RandomizerValues.shovelDamageDict.ContainsKey(id)) return;

                RandomizerValues.shovelDamageDict.Add(id, damage);

                NetworkObject networkObject = Unity.Netcode.NetworkManager.Singleton.SpawnManager.SpawnedObjects[id];
                Shovel shovel = networkObject.gameObject.GetComponentInChildren<Shovel>();
                shovel.shovelHitForce = damage;

                RandomizerModBase.mls.LogInfo("RECEIVED SHOVEL STATS: " + id + ", " + damage);
            }
        }

        public void ReloadStats()
        {
            if (RandomizerValues.shovelDamageDict.Count > 0)
            {
                int idx = 0;
                RandomizerModBase.mls.LogInfo(String.Format("Reloading {0} shovel stat from dictionary. ", RandomizerValues.shovelDamageDict.Count));
                List<int> temp = RandomizerValues.shovelDamageDict.Values.ToList();
                RandomizerValues.shovelDamageDict.Clear();

                List<UnityEngine.Object> shovelsInLevel = GameObject.FindObjectsByType(typeof(Shovel), FindObjectsSortMode.None).ToList();

                foreach (UnityEngine.Object obj in shovelsInLevel)
                {
                    Shovel shovel = (Shovel)obj;

                    RandomizerModBase.mls.LogInfo(shovel.NetworkObjectId);

                    if (idx >= temp.Count) break;

                    shovel.shovelHitForce = temp.ElementAt(idx);

                    RandomizerValues.shovelDamageDict.Add(shovel.NetworkObjectId, temp.ElementAt(idx));
                    idx++;
                }

                RandomizerModBase.mls.LogInfo("Reloaded shovel stats from dictionary.");
            }
            else
            {
                RandomizerModBase.mls.LogInfo("No shovel stats to reload.");
            }
        }

        public void SyncStatsWithClients()
        {
            foreach (KeyValuePair<ulong, int> pair in RandomizerValues.shovelDamageDict)
            {
                FastBufferWriter writer = new FastBufferWriter(sizeof(ulong) + sizeof(int), Unity.Collections.Allocator.Temp, -1);
                writer.WriteValueSafe<ulong>(pair.Key);
                writer.WriteValueSafe<int>(pair.Value);

                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesShovelData", writer, NetworkDelivery.Reliable);
            }
        }

        public void SaveOnExit()
        {
            if (RandomizerValues.shovelDamageDict.Count > 0)
            {
                try
                {
                    ES3.Save("shovelStatsDict", RandomizerValues.shovelDamageDict, GameNetworkManager.Instance.currentSaveFileName);
                    
                    if (!RandomizerValues.keysToLoad.Contains("shovelStatsDict"))
                    {
                        RandomizerValues.keysToLoad.Add("shovelStatsDict");
                    }
                }
                catch (Exception ex)
                {
                    RandomizerModBase.mls.LogError("Exception caught during custom value serialization. [Shovel] " + ex.Message);
                }
            }
            else if (RandomizerValues.keysToLoad.Contains("shovelStatsDict"))
            {
                RandomizerValues.keysToLoad.Remove("shovelStatsDict");
            }
        }
    }
}
