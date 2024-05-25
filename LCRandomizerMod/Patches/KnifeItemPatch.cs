using HarmonyLib;
using Unity.Netcode;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LCRandomizerMod.Patches
{
    [HarmonyPatch(typeof(KnifeItem))]
    internal class KnifeItemPatch : ICustomValue
    {
        [HarmonyPatch(nameof(KnifeItem.EquipItem))]
        [HarmonyPostfix]
        public static void RandomizeDamage(KnifeItem __instance)
        {
            if (Unity.Netcode.NetworkManager.Singleton.IsServer && !RandomizerValues.knifeDamageDict.ContainsKey(__instance.NetworkObjectId))
            {
                __instance.knifeHitForce = new System.Random().Next(1, 11);

                RandomizerValues.knifeDamageDict.Add(__instance.NetworkObjectId, __instance.knifeHitForce);

                FastBufferWriter fastBufferWriter = new FastBufferWriter(sizeof(ulong) + sizeof(int), Unity.Collections.Allocator.Temp, -1);
                fastBufferWriter.WriteValueSafe<ulong>(__instance.NetworkObjectId);
                fastBufferWriter.WriteValueSafe<int>(__instance.knifeHitForce);

                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesKnifeData", fastBufferWriter, NetworkDelivery.Reliable);
            }
        }

        public static void SetKnifeData(ulong _, FastBufferReader reader)
        {
            if (!Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                ulong id;
                int damage;

                reader.ReadValueSafe(out id);
                reader.ReadValueSafe(out damage);

                if (RandomizerValues.knifeDamageDict.ContainsKey(id)) return;

                RandomizerValues.knifeDamageDict.Add(id, damage);

                NetworkObject networkObject = Unity.Netcode.NetworkManager.Singleton.SpawnManager.SpawnedObjects[id];
                KnifeItem knife = networkObject.gameObject.GetComponentInChildren<KnifeItem>();
                knife.knifeHitForce = damage;

                RandomizerModBase.mls.LogInfo("RECEIVED KNIFE STATS: " + id + ", " + damage);
            }
        }

        public void ReloadStats()
        {
            if (RandomizerValues.knifeDamageDict.Count > 0)
            {
                int idx = 0;
                RandomizerModBase.mls.LogInfo(String.Format("Reloading {0} knife stat from dictionary. ", RandomizerValues.knifeDamageDict.Count));
                List<int> temp = RandomizerValues.knifeDamageDict.Values.ToList();
                RandomizerValues.knifeDamageDict.Clear();

                List<UnityEngine.Object> knivesInLevel = GameObject.FindObjectsByType(typeof(KnifeItem), FindObjectsSortMode.None).ToList();

                foreach (UnityEngine.Object obj in knivesInLevel)
                {
                    KnifeItem knife = (KnifeItem)obj;

                    RandomizerModBase.mls.LogInfo(knife.NetworkObjectId);

                    if (idx >= temp.Count) break;

                    knife.knifeHitForce = temp.ElementAt(idx);

                    RandomizerValues.knifeDamageDict.Add(knife.NetworkObjectId, temp.ElementAt(idx));
                    idx++;
                }

                RandomizerModBase.mls.LogInfo("Reloaded knife stats from dictionary.");
            }
            else
            {
                RandomizerModBase.mls.LogInfo("No knife stats to reload.");
            }
        }

        public void SyncStatsWithClients()
        {
            foreach (KeyValuePair<ulong, int> pair in RandomizerValues.knifeDamageDict)
            {
                FastBufferWriter writer = new FastBufferWriter(sizeof(ulong) + sizeof(int), Unity.Collections.Allocator.Temp, -1);
                writer.WriteValueSafe<ulong>(pair.Key);
                writer.WriteValueSafe<int>(pair.Value);

                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesKnifeData", writer, NetworkDelivery.Reliable);
            }
        }

        public void SaveOnExit()
        {
            if (RandomizerValues.knifeDamageDict.Count > 0)
            {
                try
                {
                    ES3.Save("knifeStatsDict", RandomizerValues.knifeDamageDict, GameNetworkManager.Instance.currentSaveFileName);
                    if (!RandomizerValues.keysToLoad.Contains("knifeStatsDict"))
                    {
                        RandomizerValues.keysToLoad.Add("knifeStatsDict");
                    }
                }
                catch (Exception ex)
                {
                    RandomizerModBase.mls.LogError("Exception caught during custom value serialization. [KnifeItem] " + ex.Message);
                }
            }
            else if (RandomizerValues.keysToLoad.Contains("knifeStatsDict"))
            {
                RandomizerValues.keysToLoad.Remove("knifeStatsDict");
            }
        }
    }
}
