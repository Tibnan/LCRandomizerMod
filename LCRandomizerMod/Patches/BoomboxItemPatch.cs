using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace LCRandomizerMod.Patches
{
    [HarmonyPatch(typeof(BoomboxItem))]
    internal class BoomboxItemPatch : ICustomValue
    {
        [HarmonyPatch("StartMusic")]
        [HarmonyPostfix]
        public static void PitchOverride(BoomboxItem __instance)
        {
            __instance.boomboxAudio.Stop();
            __instance.boomboxAudio.PlayOneShot(__instance.stopAudios[UnityEngine.Random.Range(0, __instance.stopAudios.Length)]);

            if (Unity.Netcode.NetworkManager.Singleton.IsServer && !RandomizerValues.boomboxPitchDict.ContainsKey(__instance.NetworkObjectId))
            {
                __instance.boomboxAudio.pitch = new System.Random().Next(50, 301) / 100f;
                int num = __instance.musicRandomizer.Next(0, __instance.musicAudios.Length);
                __instance.boomboxAudio.clip = __instance.musicAudios[num];

                RandomizerValues.boomboxPitchDict.Add(__instance.NetworkObjectId, __instance.boomboxAudio.pitch);

                FastBufferWriter writer = new FastBufferWriter(sizeof(ulong) + sizeof(float), Unity.Collections.Allocator.Temp, -1);
                writer.WriteValueSafe<ulong>(__instance.NetworkObjectId);
                writer.WriteValueSafe<float>(__instance.boomboxAudio.pitch);

                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesBoomboxPitch", writer, NetworkDelivery.Reliable);

                FastBufferWriter musicChangeWriter = new FastBufferWriter(sizeof(ulong) + sizeof(int), Unity.Collections.Allocator.Temp, -1);
                musicChangeWriter.WriteValueSafe<ulong>(__instance.NetworkObjectId);
                musicChangeWriter.WriteValueSafe<int>(num);

                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesBoomboxMChange", musicChangeWriter, NetworkDelivery.Reliable);

                __instance.boomboxAudio.Play();
            }
            else
            {
                __instance.boomboxAudio.pitch = RandomizerValues.boomboxPitchDict.GetValueSafe(__instance.NetworkObjectId);
                if (Unity.Netcode.NetworkManager.Singleton.IsServer && __instance.isBeingUsed)
                {
                    RandomizerModBase.mls.LogError("SENDING MUSIC SWITCH!!" + __instance.isBeingUsed + " " + __instance.isPlayingMusic);
                    ChangeMusicAndSend(__instance.NetworkObjectId);
                }
                else if (__instance.playerHeldBy == GameNetworkManager.Instance.localPlayerController && __instance.isBeingUsed)
                {
                    FastBufferWriter writer = new FastBufferWriter(sizeof(ulong), Unity.Collections.Allocator.Temp, -1);
                    writer.WriteValueSafe<ulong>(__instance.NetworkObjectId);
                    Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage("Tibnan.lcrandomizermod_" + "ServerInvokeMusicChange", 0UL, writer, NetworkDelivery.Reliable);
                }
            }
        }

        public static void SetBoomboxPitch(ulong _, FastBufferReader reader)
        {
            if (!Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                ulong id;
                float pitch;

                reader.ReadValueSafe<ulong>(out id);
                reader.ReadValueSafe<float>(out pitch);

                if (!RandomizerValues.boomboxPitchDict.ContainsKey(id))
                {
                    RandomizerValues.boomboxPitchDict.Add(id, pitch);
                }

                NetworkObject networkObject = Unity.Netcode.NetworkManager.Singleton.SpawnManager.SpawnedObjects[id];
                BoomboxItem boombox = networkObject.gameObject.GetComponentInChildren<BoomboxItem>();

                boombox.boomboxAudio.pitch = pitch;
            }
        }

        public void SaveOnExit()
        {
            if (RandomizerValues.boomboxPitchDict.Count > 0)
            {
                try
                {
                    ES3.Save("boomboxStatsDict", RandomizerValues.boomboxPitchDict, GameNetworkManager.Instance.currentSaveFileName);

                    if (!RandomizerValues.keysToLoad.Contains("boomboxStatsDict"))
                    {
                        RandomizerValues.keysToLoad.Add("boomboxStatsDict");
                    }
                }
                catch (Exception ex)
                {
                    RandomizerModBase.mls.LogError("Exception caught during custom value serialization. [BoomboxItem] " + ex.Message);
                }
            }
            else if (RandomizerValues.keysToLoad.Contains("boomboxStatsDict"))
            {
                RandomizerValues.keysToLoad.Remove("boomboxStatsDict");
            }
        }

        public void ReloadStats()
        {
            if (RandomizerValues.boomboxPitchDict.Count > 0)
            {
                int idx = 0;
                RandomizerModBase.mls.LogInfo(String.Format("Reloading {0} boombox entry from dictionary. ", RandomizerValues.boomboxPitchDict.Count));
                List<float> temp = RandomizerValues.boomboxPitchDict.Values.ToList();
                RandomizerValues.boomboxPitchDict.Clear();

                List<UnityEngine.Object> boomboxesInLevel = GameObject.FindObjectsByType(typeof(BoomboxItem), FindObjectsSortMode.None).ToList();

                foreach (UnityEngine.Object obj in boomboxesInLevel)
                {
                    BoomboxItem boomboxItem = (BoomboxItem)obj;

                    RandomizerModBase.mls.LogInfo(boomboxItem.NetworkObjectId);

                    if (idx >= temp.Count) break;

                    boomboxItem.boomboxAudio.pitch = temp.ElementAt(idx);

                    RandomizerValues.boomboxPitchDict.Add(boomboxItem.NetworkObjectId, temp.ElementAt(idx));
                    idx++;
                }

                RandomizerModBase.mls.LogInfo("Reloaded boombox stats from dictionary.");
            }
            else
            {
                RandomizerModBase.mls.LogInfo("No boombox stats to reload.");
            }
        }

        public void SyncStatsWithClients()
        {
            foreach (KeyValuePair<ulong, float> pair in RandomizerValues.boomboxPitchDict)
            {
                FastBufferWriter writer = new FastBufferWriter(sizeof(ulong) + sizeof(int), Unity.Collections.Allocator.Temp, -1);
                writer.WriteValueSafe<ulong>(pair.Key);
                writer.WriteValueSafe<float>(pair.Value);

                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesBoomboxPitch", writer, NetworkDelivery.Reliable);
            }
        }

        public static void ClientChangeMusic(ulong _, FastBufferReader reader)
        {
            if (!Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                ulong id;
                int num;

                reader.ReadValueSafe<ulong>(out id);
                reader.ReadValueSafe<int>(out num);

                NetworkObject networkObject = Unity.Netcode.NetworkManager.Singleton.SpawnManager.SpawnedObjects[id];
                BoomboxItem boombox = networkObject.gameObject.GetComponentInChildren<BoomboxItem>();

                RandomizerModBase.mls.LogError("CLIENT RECEIVED MUSIC CHANGE!!" + boombox.isBeingUsed + " " + boombox.isPlayingMusic);

                boombox.boomboxAudio.clip = boombox.musicAudios[num];

                boombox.boomboxAudio.Play();
            }
        }

        public static void ChangeMusicAndSend(ulong boomboxID)
        {
            RandomizerModBase.mls.LogError("CHANGING MUSIC AND SENDING");

            NetworkObject networkObject = Unity.Netcode.NetworkManager.Singleton.SpawnManager.SpawnedObjects[boomboxID];
            BoomboxItem __instance = networkObject.gameObject.GetComponentInChildren<BoomboxItem>();

            int num = __instance.musicRandomizer.Next(0, __instance.musicAudios.Length);
            __instance.boomboxAudio.clip = __instance.musicAudios[num];

            __instance.boomboxAudio.Play();

            FastBufferWriter musicChangeWriter = new FastBufferWriter(sizeof(ulong) + sizeof(int), Unity.Collections.Allocator.Temp, -1);
            musicChangeWriter.WriteValueSafe<ulong>(__instance.NetworkObjectId);
            musicChangeWriter.WriteValueSafe<int>(num);

            Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesBoomboxMChange", musicChangeWriter, NetworkDelivery.Reliable);
        }

        public static void ServerReceivesMusicChangeRequest(ulong _, FastBufferReader reader)
        {
            ulong id;
            reader.ReadValueSafe<ulong>(out id);

            RandomizerModBase.mls.LogError("SERVER RECEIVED MUSIC CHANGE REQUEST");

            ChangeMusicAndSend(id);
        }
    }
}
