using GameNetcodeStuff;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace LCRandomizerMod
{
    internal class LidBehaviorCustom : MonoBehaviour, ICustomValue
    {
        //SaveData
        private LidSaveData lidProperties = new LidSaveData();
        //EndSaveData

        private static Vector3 DefaultOffset = Vector3.zero;

        private GrabbableObject objectScript;
        private Item properties;
        private bool heldUp = false;
        private bool negatedDamage = false;
        private bool isBeingRaised = false;
        private bool isCurrentlyDiscarded = true;
        private Coroutine transitionCoroutine;
        private PlayerControllerB previousGuardedPlayer;

        private void Awake()
        {
            this.objectScript = this.gameObject.GetComponent<GrabbableObject>();
            this.properties = this.objectScript.itemProperties;

            if (!RandomizerValues.customGLids.ContainsKey(this.objectScript.NetworkObjectId) && !RandomizerValues.startupInitializing)
            {
                RandomizerValues.customGLids.Add(this.objectScript.NetworkObjectId, this.lidProperties);
            }

            if (DefaultOffset == Vector3.zero)
            {
                DefaultOffset = this.properties.positionOffset;
            }
        }

        private void Update()
        {
            if (this.negatedDamage)
            {
                this.lidProperties.HP--;
                this.negatedDamage = !this.negatedDamage;
                this.SyncNewHP();

                if (this.lidProperties.HP < 1)
                {
                    this.BreakLid();
                }
            }

            if (this.objectScript.isBeingUsed)
            {
                if (!this.lidProperties.IsBroken)
                {
                    this.SetGarbageLidUsed(!this.heldUp);
                    this.isCurrentlyDiscarded = false;
                }
                else if (this.objectScript.playerHeldBy == GameNetworkManager.Instance.localPlayerController)
                {
                    CustomUI playerUI = GameNetworkManager.Instance.localPlayerController.gameObject.GetComponent<CustomUI>();
                    playerUI.ShowLocalMessage("<color=red>Garbage Lid is broken!</color>", 2);
                }
                this.objectScript.isBeingUsed = false;
            }

            if (this.objectScript.playerHeldBy == null && !this.isCurrentlyDiscarded)
            {
                this.heldUp = false;
                if (this.previousGuardedPlayer == GameNetworkManager.Instance.localPlayerController)
                {
                    RandomizerModBase.mls.LogWarning("Invalidated protection of: " + this.previousGuardedPlayer.playerUsername);
                    RandomizerValues.guardedByLid = false;
                }
                if (this.transitionCoroutine != null) this.StopCoroutine(this.transitionCoroutine);
                this.isBeingRaised = false;
                this.SetGarbageLidUsed(up: false, smoothRaise: false);
                this.isCurrentlyDiscarded = true;
            }
        }

        private void SetGarbageLidUsed(bool up, bool smoothRaise = true)
        {
            if (this.isBeingRaised) return;

            if (smoothRaise)
            {
                this.transitionCoroutine = this.StartCoroutine(SmoothTransitionToNewPos(translateUp: up));
            }
            else
            {
                this.properties.positionOffset = DefaultOffset;
            }
            this.SetLidAsGuardingPlayer(up);

            this.heldUp = up;
        }

        private void SyncNewHP()
        {
            if (NetworkManager.Singleton.IsServer)
            {
                SendHPToClients(this.objectScript.NetworkObjectId, this.lidProperties.HP);
            }
            else
            {
                FastBufferWriter writer = new FastBufferWriter(sizeof(ulong) + sizeof(int), Unity.Collections.Allocator.Temp, -1);
                writer.WriteValueSafe<ulong>(this.objectScript.NetworkObjectId);
                writer.WriteValueSafe<int>(this.lidProperties.HP);
                NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage("Tibnan.lcrandomizermod_" + "ServerReceivesLidHPSyncRQ", 0UL, writer, NetworkDelivery.Reliable);
            }
        }

        private void SyncBrokenState()
        {
            if (NetworkManager.Singleton.IsServer)
            {
                SendBrokenStateToClients(this.objectScript.NetworkObjectId);
            }
            else
            {
                FastBufferWriter writer = new FastBufferWriter(sizeof(ulong), Unity.Collections.Allocator.Temp, -1);
                writer.WriteValueSafe<ulong>(this.objectScript.NetworkObjectId);
                NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage("Tibnan.lcrandomizermod_" + "ServerReceivesLidBrokenSyncRQ", 0UL, writer, NetworkDelivery.Reliable);
            }
        }

        private IEnumerator SmoothTransitionToNewPos(bool translateUp)
        {
            this.isBeingRaised = true;
            
            if (translateUp)
            {
                float limit = this.properties.positionOffset.y + 0.3f;

                while (this.properties.positionOffset.y <= limit)
                {
                    this.properties.positionOffset += new Vector3(0f, 0.1f, 0f) * Time.deltaTime * 3.5f;
                    yield return null;
                }
            }
            else
            {
                while (this.properties.positionOffset.y >= 0.2f)
                {
                    this.properties.positionOffset -= new Vector3(0f, 0.1f, 0f) * Time.deltaTime * 3.5f;
                    yield return null;
                }
            }

            this.isBeingRaised = false;
            yield break;
        }

        private void BreakLid(bool sync = true)
        {
            if (!this.lidProperties.IsBroken)
            {
                this.SetGarbageLidUsed(this.lidProperties.IsBroken);
                this.lidProperties.IsBroken = true;
                if (this.objectScript.playerHeldBy == GameNetworkManager.Instance.localPlayerController)
                {
                    CustomUI playerUI = GameNetworkManager.Instance.localPlayerController.gameObject.GetComponent<CustomUI>();
                    playerUI.ShowLocalMessage("<color=red>You have broken your garbage lid.</color>", 2);
                }
                RandomizerModBase.mls.LogError("LID BROKE");
                if (sync) this.SyncBrokenState();
            }
        }

        private void SetLidAsGuardingPlayer(bool active)
        {
            PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;

            if (localPlayer == this.objectScript.playerHeldBy)
            {
                RandomizerValues.guardedByLid = active;
                this.previousGuardedPlayer = localPlayer;
            }
        }

        public void NegateDamage()
        {
            if (!this.negatedDamage)
            {
                this.negatedDamage = true;
            }
        }

        //Networking

        public static void SetReceivedHPClient(ulong _, FastBufferReader reader)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                ulong id;
                int hp;

                reader.ReadValueSafe<ulong>(out id);
                reader.ReadValueSafe<int>(out hp);

                NetworkObject networkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[id];
                GrabbableObject grabbableObject = networkObject.GetComponentInChildren<GrabbableObject>();

                grabbableObject.gameObject.GetComponent<LidBehaviorCustom>().HP = hp;
                RandomizerModBase.mls.LogError("Set new hp of lid to: " + hp);
            }
        }

        public static void SendReceivedHPToClients(ulong _, FastBufferReader reader)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                ulong id;
                int hp;

                reader.ReadValueSafe<ulong>(out id);
                reader.ReadValueSafe<int>(out hp);

                NetworkObject networkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[id];
                GrabbableObject grabbableObject = networkObject.GetComponentInChildren<GrabbableObject>();
                grabbableObject.gameObject.GetComponent<LidBehaviorCustom>().HP = hp;

                RandomizerModBase.mls.LogError("Received hp from client: " + hp + " sent over network");
                SendHPToClients(id, hp);
            }
        }

        private static void SendHPToClients(ulong id, int hp)
        {
            FastBufferWriter writer = new FastBufferWriter(sizeof(ulong) + sizeof(int), Unity.Collections.Allocator.Temp, -1);
            writer.WriteValueSafe<ulong>(id);
            writer.WriteValueSafe<int>(hp);
            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesLidHPSync", writer, NetworkDelivery.Reliable);
        }

        private static void SendBrokenStateToClients(ulong id)
        {
            FastBufferWriter writer = new FastBufferWriter(sizeof(ulong), Unity.Collections.Allocator.Temp, -1);
            writer.WriteValueSafe<ulong>(id); 
            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesLidBrokenSync", writer, NetworkDelivery.Reliable);
        }

        public static void SetLidBrokenSync(ulong _, FastBufferReader reader)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                ulong id;

                reader.ReadValueSafe<ulong>(out id);

                NetworkObject networkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[id];
                GrabbableObject grabbableObject = networkObject.GetComponentInChildren<GrabbableObject>();

                grabbableObject.gameObject.GetComponent<LidBehaviorCustom>().BreakLid(sync: false);
            }
        }

        public static void ServerHandleLidBroken(ulong _, FastBufferReader reader)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                ulong id;

                reader.ReadValueSafe<ulong>(out id);

                NetworkObject networkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[id];
                GrabbableObject grabbableObject = networkObject.GetComponentInChildren<GrabbableObject>();

                LidBehaviorCustom lid = grabbableObject.gameObject.GetComponent<LidBehaviorCustom>();
                lid.BreakLid(sync: false);
                SendBrokenStateToClients(lid.objectScript.NetworkObjectId);
            }
        }

        public static void PostLoginSyncClient(ulong _, FastBufferReader reader)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                ulong id;
                bool isBroken;
                int hp;

                reader.ReadValueSafe<ulong>(out id);
                reader.ReadValueSafe<bool>(out isBroken);
                reader.ReadValueSafe<int>(out hp);

                NetworkObject networkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[id];
                GrabbableObject grabbableObject = networkObject.GetComponentInChildren<GrabbableObject>();

                LidBehaviorCustom script = grabbableObject.gameObject.GetComponent<LidBehaviorCustom>();
                script.HP = hp;
                script.IsBroken = isBroken;
            }
        }

        //EndNetworking

        public void SaveOnExit()
        {
            if (RandomizerValues.customGLids.Count > 0)
            {
                try
                {
                    RandomizerModBase.mls.LogWarning(String.Format("Saving {0} garbage lid states...", RandomizerValues.customGLids.Count));
                    ES3.Save("glidCustom", RandomizerValues.customGLids, GameNetworkManager.Instance.currentSaveFileName);
                    if (!RandomizerValues.keysToLoad.Contains("glidCustom"))
                    {
                        RandomizerValues.keysToLoad.Add("glidCustom");
                    }
                }
                catch (Exception ex)
                {
                    RandomizerModBase.mls.LogError("Exception caught during custom value serialization. [LidBehaviorCustom] " + ex.Message);
                }
            }
            else if (RandomizerValues.keysToLoad.Contains("glidCustom"))
            {
                RandomizerValues.keysToLoad.Remove("glidCustom");
            }
        }

        public void ReloadStats()
        {
            if (RandomizerValues.customGLids.Count > 0)
            {
                RandomizerModBase.mls.LogInfo(String.Format("Reloading {0} custom garbage lid states from list.", RandomizerValues.customGLids.Count));
                List<LidSaveData> temp = RandomizerValues.customGLids.Values.ToList();
                RandomizerValues.customGLids.Clear();

                List<GrabbableObject> gObjects = GameObject.FindObjectsOfType<GrabbableObject>().ToList();

                List<GrabbableObject> filteredList = gObjects.FindAll(x => x.name.Contains("GarbageLid"));
                foreach (GrabbableObject obj in filteredList)
                {
                    obj.gameObject.AddComponent<LidBehaviorCustom>(); //Need to add script because at this point, object has not yet been initialized.
                }

                int idx = 0;
                foreach (GrabbableObject obj in filteredList)
                {
                    if (idx >= temp.Count) break;

                    LidBehaviorCustom script = obj.gameObject.GetComponent<LidBehaviorCustom>();
                    script.lidProperties = new LidSaveData(temp[idx].IsBroken, temp[idx].HP);

                    RandomizerValues.customGLids.Add(script.objectScript.NetworkObjectId, script.lidProperties);
                    idx++;
                }
            }
            else
            {
                RandomizerModBase.mls.LogInfo("No custom garbage lid states to reload.");
            }
        }

        public void SyncStatsWithClients()
        {
            foreach (KeyValuePair<ulong, LidSaveData> kvp in RandomizerValues.customGLids)
            {
                FastBufferWriter writer = new FastBufferWriter(sizeof(ulong) + sizeof(bool) + sizeof(int), Unity.Collections.Allocator.Temp, -1);
                writer.WriteValueSafe<ulong>(kvp.Key);
                writer.WriteValueSafe<bool>(kvp.Value.IsBroken);
                writer.WriteValueSafe<int>(kvp.Value.HP);

                NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesLidLoginSync", writer, NetworkDelivery.Reliable);
            }
        }

        public PlayerControllerB PreviousGuardedPlayer
        {
            get { return this.previousGuardedPlayer; }
        }

        public int HP
        {
            get { return this.lidProperties.HP; }
            set { this.lidProperties.HP = value; }
        }

        public bool IsBroken
        {
            get { return this.lidProperties.IsBroken; }
            set { this.lidProperties.IsBroken = value; }
        }
    }
}
