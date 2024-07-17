using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LCRandomizerMod.Patches
{
    [HarmonyPatch(typeof(ShipTeleporter))]
    internal class ShipTeleporterPatch : ICustomValue
    {
        public static void CheckForTeleporterLOS(InputAction.CallbackContext context)
        {
            if (GameNetworkManager.Instance.localPlayerController.currentlyHeldObjectServer == null) return;

            RaycastHit hit;
            ShipTeleporter teleporter;
            LungProp lungProp = GameNetworkManager.Instance.localPlayerController.currentlyHeldObjectServer.gameObject?.GetComponent<LungProp>();
            if (lungProp != null && Physics.Raycast(GameNetworkManager.Instance.localPlayerController.gameplayCamera.transform.position + new Vector3(0f, 0f, 0.4f), GameNetworkManager.Instance.localPlayerController.gameplayCamera.transform.forward, out hit, 3f))
            {
                RandomizerModBase.mls.LogError("Raycast hit " + hit.collider.name);
                teleporter = hit.collider.GetComponentInParent<ShipTeleporter>();
                if (teleporter != null && teleporter.cooldownAmount > 0f)
                {
                    if (Unity.Netcode.NetworkManager.Singleton.IsServer)
                    {
                        if (RandomizerValues.connectCoroutinePlaying) return;

                        RandomizerModBase.mls.LogWarning("Sending coroutine start");
                        SendCoroutineStartToClients(lungProp, teleporter);
                    }
                    else
                    {
                        if (RandomizerValues.connectCoroutinePlaying) return;

                        FastBufferWriter writer = new FastBufferWriter(sizeof(ulong) * 2, Unity.Collections.Allocator.Temp, -1);
                        writer.WriteValueSafe<ulong>(lungProp.NetworkObjectId);
                        writer.WriteValueSafe<ulong>(teleporter.NetworkObjectId);

                        Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage("Tibnan.lcrandomizermod_" + "ServerReceivesTPModifyRequest", 0UL, writer, NetworkDelivery.Reliable);
                    }
                }
                else
                {
                    RandomizerModBase.mls.LogError("Not interacted with ship tp");
                }
            }
        }

        public static void ServerReceivesTPModifyRequest(ulong _, FastBufferReader reader)
        {
            ulong propID;
            ulong tpID;

            reader.ReadValueSafe<ulong>(out propID);
            reader.ReadValueSafe<ulong>(out tpID);

            NetworkObject nObject = Unity.Netcode.NetworkManager.Singleton.SpawnManager.SpawnedObjects[propID];
            LungProp lungProp = nObject.gameObject.GetComponentInChildren<LungProp>();

            nObject = Unity.Netcode.NetworkManager.Singleton.SpawnManager.SpawnedObjects[tpID];
            ShipTeleporter teleporter = nObject.gameObject.GetComponentInChildren<ShipTeleporter>();

            SendCoroutineStartToClients(lungProp, teleporter);
        }

        public static void SendCoroutineStartToClients(LungProp lungProp, ShipTeleporter teleporter)
        {
            FastBufferWriter writer = new FastBufferWriter(sizeof(ulong) * 2, Unity.Collections.Allocator.Temp, -1);
            writer.WriteValueSafe<ulong>(lungProp.NetworkObjectId);
            writer.WriteValueSafe<ulong>(teleporter.NetworkObjectId);

            GameNetworkManager.Instance.StartCoroutine(ConnectLungPropCoroutine(lungProp, teleporter));

            Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientsStartTPModCoroutine", writer, NetworkDelivery.Reliable);
        }

        public static void StartConnectCoroutineClient(ulong _, FastBufferReader reader)
        {
            if (!Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                ulong propID;
                ulong tpID;

                reader.ReadValueSafe<ulong>(out propID);
                reader.ReadValueSafe<ulong>(out tpID);

                NetworkObject nObject = Unity.Netcode.NetworkManager.Singleton.SpawnManager.SpawnedObjects[propID];
                LungProp lungProp = nObject.gameObject.GetComponentInChildren<LungProp>();

                nObject = Unity.Netcode.NetworkManager.Singleton.SpawnManager.SpawnedObjects[tpID];
                ShipTeleporter teleporter = nObject.gameObject.GetComponentInChildren<ShipTeleporter>();

                GameNetworkManager.Instance.StartCoroutine(ConnectLungPropCoroutine(lungProp, teleporter));
            }
        }

        public static IEnumerator ConnectLungPropCoroutine(LungProp lungProp, ShipTeleporter teleporter)
        {
            if (lungProp.playerHeldBy == GameNetworkManager.Instance.localPlayerController)
            {
                RandomizerValues.blockDrop = true;
            }

            RandomizerValues.connectCoroutinePlaying = true;
            AudioClip origAudio = teleporter.shipTeleporterAudio.clip;
            teleporter.shipTeleporterAudio.clip = lungProp.connectSFX;
            teleporter.shipTeleporterAudio.Play();
            GameObject newSparkParticle = GameObject.Instantiate<GameObject>(lungProp.sparkParticle, teleporter.transform.position, Quaternion.identity, null);
            newSparkParticle.SetActive(true);
            yield return new WaitWhile(() => teleporter.shipTeleporterAudio.isPlaying);
            teleporter.shipTeleporterAudio.clip = origAudio;
            if (lungProp.playerHeldBy == GameNetworkManager.Instance.localPlayerController)
            {
                GameNetworkManager.Instance.localPlayerController.DespawnHeldObject();
            }
            if (teleporter.isInverseTeleporter)
            {
                teleporter.cooldownAmount -= 10;
                Traverse.Create(teleporter).Field("cooldownTime").SetValue(1f);
            }
            else
            {
                teleporter.cooldownAmount -= 1;
            }

            RandomizerValues.connectCoroutinePlaying = false;
            RandomizerValues.blockDrop = false;

            string msg = String.Format("<color=green>An apparatus was inserted into the {0}. Current cooldown: {1} seconds.</color>", teleporter.isInverseTeleporter ? "inverse teleporter" : "teleporter", teleporter.cooldownAmount);
            CustomUI.BroadcastMessage(msg, 3);
        }

        public void SaveOnExit()
        {
            ShipTeleporter[] teleporters = GameObject.FindObjectsOfType<ShipTeleporter>();
            foreach (ShipTeleporter tp in teleporters)
            {
                if (RandomizerValues.teleporterCooldowns.ContainsKey(tp.isInverseTeleporter))
                {
                    RandomizerValues.teleporterCooldowns.Remove(tp.isInverseTeleporter);
                    RandomizerValues.teleporterCooldowns.Add(tp.isInverseTeleporter, tp.cooldownAmount);
                }
                else
                {
                    RandomizerValues.teleporterCooldowns.Add(tp.isInverseTeleporter, tp.cooldownAmount);
                }
            }

            if (RandomizerValues.teleporterCooldowns.Count > 0)
            {
                RandomizerModBase.mls.LogWarning(String.Format("Saving {0} teleporter cooldowns.", RandomizerValues.teleporterCooldowns.Count));
                try
                {
                    ES3.Save("tpCooldowns", RandomizerValues.teleporterCooldowns, GameNetworkManager.Instance.currentSaveFileName);
                    if (!RandomizerValues.keysToLoad.Contains("tpCooldowns"))
                    {
                        RandomizerValues.keysToLoad.Add("tpCooldowns");
                    }
                }
                catch (Exception ex)
                {
                    RandomizerModBase.mls.LogError("Exception caught during custom value serialization. [ShipTeleporter] " + ex.Message);
                }
            }
            else if (RandomizerValues.keysToLoad.Contains("tpCooldowns"))
            {
                RandomizerValues.keysToLoad.Remove("tpCooldowns");
            }
        }

        public void ReloadStats()
        {
            if (RandomizerValues.teleporterCooldowns.Count > 0)
            {
                RandomizerModBase.mls.LogInfo(String.Format("Reloading {0} teleporter cooldowns.", RandomizerValues.teleporterCooldowns.Count));

                ShipTeleporter[] teleporters = GameObject.FindObjectsOfType<ShipTeleporter>();
                
                foreach (ShipTeleporter teleporter in teleporters)
                {
                    teleporter.cooldownAmount = RandomizerValues.teleporterCooldowns.GetValueSafe(teleporter.isInverseTeleporter);
                }

                RandomizerModBase.mls.LogInfo("Reloaded teleporter cooldowns.");
            }
            else
            {
                RandomizerModBase.mls.LogInfo("No teleporter cooldowns to reload.");
            }
        }

        public void SyncStatsWithClients()
        {
            foreach (KeyValuePair<bool, float> pair in RandomizerValues.teleporterCooldowns)
            {
                FastBufferWriter writer = new FastBufferWriter(sizeof(bool) + sizeof(float), Unity.Collections.Allocator.Temp, -1);
                writer.WriteValueSafe<bool>(pair.Key);
                writer.WriteValueSafe<float>(pair.Value);

                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientSetTPCooldown", writer, NetworkDelivery.Reliable);
            }
        }

        public static void SetTpCooldownClient(ulong _, FastBufferReader reader)
        {
            if (!Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                bool isInverse;
                float amount;

                reader.ReadValueSafe<bool>(out isInverse);

                if (RandomizerValues.teleporterCooldowns.ContainsKey(isInverse)) return;

                reader.ReadValueSafe<float>(out amount);

                ShipTeleporter[] teleporters = GameObject.FindObjectsOfType<ShipTeleporter>();

                foreach (ShipTeleporter tp in teleporters)
                {
                    if (tp.isInverseTeleporter == isInverse)
                    {
                        tp.cooldownAmount = amount;
                        RandomizerValues.teleporterCooldowns.Add(isInverse, amount);
                    }
                }
            }
        }
    }
}
