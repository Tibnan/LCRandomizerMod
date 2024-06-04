using GameNetcodeStuff;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace LCRandomizerMod.Patches
{
    [HarmonyPatch(typeof(Terminal))]
    internal class TerminalPatch
    {
        [HarmonyPatch(nameof(Terminal.OnSubmit))]
        [HarmonyPrefix]
        public static bool CheckForRandom(Terminal __instance)
        {
            if (!RandomizerValues.mapRandomizedInTerminal && StartOfRound.Instance.inShipPhase)
            {
                string parsedText = __instance.screenText.text.Substring(__instance.screenText.text.Length - __instance.textAdded).ToLower();
                RandomizerModBase.mls.LogInfo("PARSED SENTENCE:");
                RandomizerModBase.mls.LogInfo(parsedText);

                if (parsedText.Contains("random"))
                {
                    int num = new System.Random().Next(1, 13);

                    while (num == 11 || num == 3)
                    {
                        num = new System.Random().Next(1, 13);

                        if (TimeOfDay.Instance.daysUntilDeadline < 1 || TimeOfDay.Instance.timeUntilDeadline < 1080)
                        {
                            num = 3;
                            break;
                        }
                    }

                    RandomizerModBase.mls.LogError("TIME UNTIL DEADLINE: " + TimeOfDay.Instance.timeUntilDeadline + " DAYS UNTIL DEADLINE: " + TimeOfDay.Instance.daysUntilDeadline + " switching to level: " + (num == 3 ? "company" : num.ToString()));

                    StartOfRound.Instance.ChangeLevelServerRpc(num, __instance.groupCredits);
                    __instance.screenText.text = "";
                    __instance.QuitTerminal();
                    RandomizerValues.mapRandomizedInTerminal = true;

                    if (Unity.Netcode.NetworkManager.Singleton.IsServer)
                    {
                        SendTerminalSwitchToClients(0, new FastBufferReader());
                    }
                    else
                    {
                        FastBufferWriter writer = new FastBufferWriter(4, Unity.Collections.Allocator.Temp, -1);
                        Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage("Tibnan.lcrandomizermod_" + "ServerInvokeTerminalSwitch", 0UL, writer, NetworkDelivery.Reliable);
                    }

                    return false;
                }
                else
                {
                    return true;
                }
            }
            else if (!StartOfRound.Instance.inShipPhase)
            {
                string parsedText = __instance.screenText.text.Substring(__instance.screenText.text.Length - __instance.textAdded).ToLower();
                RandomizerModBase.mls.LogInfo("PARSED SENTENCE:");
                RandomizerModBase.mls.LogInfo(parsedText);

                if (parsedText.Contains("revive"))
                {
                    if (Unity.Netcode.NetworkManager.Singleton.IsServer)
                    {
                        ServerProcessReviveRequest(0, new FastBufferReader());
                        __instance.screenText.text = "";
                        __instance.QuitTerminal();
                    }
                    else
                    {
                        FastBufferWriter writer = new FastBufferWriter(4, Unity.Collections.Allocator.Temp, -1);
                        Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage("Tibnan.lcrandomizermod_" + "ServerReceivesReviveRequest", 0UL, writer, NetworkDelivery.Reliable);
                        __instance.screenText.text = "";
                        __instance.QuitTerminal();
                    }

                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return true;
            }
        }

        [HarmonyPatch("Update")]
        [HarmonyPrefix]
        public static void CheckForAlreadyRandomized(Terminal __instance)
        {
            //&& StartOfRound.Instance.inShipPhase && GameNetworkManager.Instance.localPlayerController.inTerminalMenu
            if (RandomizerValues.mapRandomizedInTerminal && __instance.terminalInUse)
            {
                HUDManager.Instance.AddTextToChatOnServer("<color=red>Terminal locked due to level randomization. It will unlock once you land.</color>", -1);
                GameNetworkManager.Instance.localPlayerController.inTerminalMenu = false;
                __instance.QuitTerminal();
            }
        }

        public static void SwitchTerminalMode(ulong __, FastBufferReader _)
        {
            if (!Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                RandomizerModBase.mls.LogInfo("Switched terminal to non usable");
                RandomizerValues.mapRandomizedInTerminal = true;
            }
        }

        public static void SendTerminalSwitchToClients(ulong _, FastBufferReader __)
        {
            RandomizerValues.mapRandomizedInTerminal = true;
            FastBufferWriter writer = new FastBufferWriter(4, Unity.Collections.Allocator.Temp, -1);
            Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "TerminalRandomizationUsed", writer, NetworkDelivery.Reliable);
        }

        [HarmonyPatch(nameof(Terminal.BeginUsingTerminal))]
        [HarmonyPostfix]
        public static void ScaleTerminalOnUse(Terminal __instance)
        {
            foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
            {
                if (player.inTerminalMenu)
                {
                    Vector3 scale = new Vector3(player.transform.localScale.x + 0.03f, player.transform.localScale.y + 0.05f, player.transform.localScale.z + 0.07f);

                    RandomizerModBase.mls.LogError("Player using terminal: " + player.playerUsername + " setting scale: " + scale);
                    __instance.placeableObject.parentObject.transform.localScale = scale;
                }
            }
        }

        [HarmonyPatch(nameof(Terminal.QuitTerminal))]
        [HarmonyPostfix]
        public static void ResetTerminalSize(Terminal __instance)
        {
            __instance.placeableObject.parentObject.transform.localScale = RandomizerValues.defaultTerminalScale;
        }

        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        public static void SaveDefaultScale(Terminal __instance)
        {
            RandomizerValues.defaultTerminalScale = __instance.placeableObject.parentObject.transform.localScale;
        }
        
        public static void SetTerminalState(ulong _, FastBufferReader reader)
        {
            if (!Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                reader.ReadValueSafe<bool>(out RandomizerValues.mapRandomizedInTerminal);
                RandomizerModBase.mls.LogInfo("Terminal state now: " + RandomizerValues.mapRandomizedInTerminal);
            }
        }

        public static void ServerProcessReviveRequest(ulong _, FastBufferReader __)
        {
            bool revivablePlayer = false;
            List<PlayerControllerB> deadPlayers = new List<PlayerControllerB>();
            foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
            {
                if (player.isPlayerDead)
                {
                    revivablePlayer = true;
                    deadPlayers.Add(player);
                }
            }

            if (revivablePlayer)
            {
                PlayerControllerB playerToRevive = deadPlayers.ElementAt(new System.Random().Next(0, deadPlayers.Count));
                RevivePlayer(playerToRevive.playerClientId);

                FastBufferWriter writer = new FastBufferWriter(sizeof(ulong), Unity.Collections.Allocator.Temp, -1);
                writer.WriteValueSafe<ulong>(playerToRevive.playerClientId);
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ReviveSpecificPlayer", writer, NetworkDelivery.Reliable);

                GiftBoxItemPatch.RandomizePlayerStats(playerToRevive);
                GiftBoxItemPatch.RecolorPlayerSync(playerToRevive);
            }
        }

        public static void ClientProcessReviveRequest(ulong _, FastBufferReader reader)
        {
            ulong playerToRevive;
            reader.ReadValueSafe<ulong>(out playerToRevive);

            RevivePlayer(playerToRevive);
        }

        public static void RevivePlayer(ulong id)
        {
            PlayerControllerB component = StartOfRound.Instance.allPlayerScripts[id];
            Debug.Log("RevivePlayerClientRpc called for player: " + component.playerUsername);
            Debug.Log("Revive Player");
            int playerIndex = (int)component.playerClientId;
            Debug.Log(string.Format("Player Index: {0}", playerIndex));
            Debug.Log("Reviving players A");
            component.ResetPlayerBloodObjects(component.isPlayerDead);
            component.isClimbingLadder = false;
            component.ResetZAndXRotation();
            component.thisController.enabled = true;
            component.health = 100;
            component.disableLookInput = false;
            Debug.Log("Reviving players B");
            bool isPlayerDead = component.isPlayerDead;
            if (isPlayerDead)
            {
                component.isPlayerDead = false;
                component.isPlayerControlled = true;
                component.isInElevator = true;
                component.isInHangarShipRoom = true;
                component.isInsideFactory = false;
                component.wasInElevatorLastFrame = false;
                StartOfRound.Instance.SetPlayerObjectExtrapolate(false);
                component.TeleportPlayer(GetPlayerSpawnPosition((int)component.playerClientId, true), false, 0f, false, true);
                component.setPositionOfDeadPlayer = false;
                component.DisablePlayerModel(StartOfRound.Instance.allPlayerObjects[playerIndex], true, true);
                component.helmetLight.enabled = false;
                Debug.Log("Reviving players C");
                component.Crouch(false);
                component.criticallyInjured = false;
                bool flag = component.playerBodyAnimator != null;
                if (flag)
                {
                    component.playerBodyAnimator.SetBool("Limp", false);
                }
                component.bleedingHeavily = false;
                component.activatingItem = false;
                component.twoHanded = false;
                component.inSpecialInteractAnimation = false;
                component.disableSyncInAnimation = false;
                component.inAnimationWithEnemy = null;
                component.holdingWalkieTalkie = false;
                component.speakingToWalkieTalkie = false;
                Debug.Log("Reviving players D");
                component.isSinking = false;
                component.isUnderwater = false;
                component.sinkingValue = 0f;
                component.statusEffectAudio.Stop();
                component.DisableJetpackControlsLocally();
                component.health = 100;
                Debug.Log("Reviving players E");
                component.mapRadarDotAnimator.SetBool("dead", false);
                HUDManager.Instance.gasHelmetAnimator.SetBool("gasEmitting", false);
                component.hasBegunSpectating = false;
                HUDManager.Instance.RemoveSpectateUI();
                HUDManager.Instance.gameOverAnimator.SetTrigger("revive");
                component.hinderedMultiplier = 1f;
                component.isMovementHindered = 0;
                component.sourcesCausingSinking = 0;
                Debug.Log("Reviving players E2");
                component.reverbPreset = StartOfRound.Instance.shipReverb;
            }
            Debug.Log("Reviving players F");
            SoundManager.Instance.earsRingingTimer = 0f;
            component.voiceMuffledByEnemy = false;
            SoundManager.Instance.playerVoicePitchTargets[playerIndex] = 1f;
            SoundManager.Instance.SetPlayerPitch(1f, playerIndex);
            bool flag2 = component.currentVoiceChatIngameSettings == null;
            if (flag2)
            {
                StartOfRound.Instance.RefreshPlayerVoicePlaybackObjects();
            }
            bool flag3 = component.currentVoiceChatIngameSettings != null;
            if (flag3)
            {
                bool flag4 = component.currentVoiceChatIngameSettings.voiceAudio == null;
                if (flag4)
                {
                    component.currentVoiceChatIngameSettings.InitializeComponents();
                }
                bool flag5 = component.currentVoiceChatIngameSettings.voiceAudio == null;
                if (flag5)
                {
                    return;
                }
                component.currentVoiceChatIngameSettings.voiceAudio.GetComponent<OccludeAudio>().overridingLowPass = false;
            }
            Debug.Log("Reviving players G");
            PlayerControllerB localPlayerController = GameNetworkManager.Instance.localPlayerController;
            localPlayerController.bleedingHeavily = false;
            localPlayerController.criticallyInjured = false;
            localPlayerController.playerBodyAnimator.SetBool("Limp", false);
            localPlayerController.health = 100;
            if (localPlayerController.playerClientId == id)
            {
                HUDManager.Instance.UpdateHealthUI(100, false);
            }
            localPlayerController.spectatedPlayerScript = null;
            if (localPlayerController.playerClientId == id)
            {
                HUDManager.Instance.audioListenerLowPass.enabled = false;
            }
            Debug.Log("Reviving players H");
            StartOfRound.Instance.SetSpectateCameraToGameOverMode(false, localPlayerController);
            RagdollGrabbableObject[] array = Object.FindObjectsOfType<RagdollGrabbableObject>();
            for (int i = 0; i < array.Length; i++)
            {
                bool flag6 = !array[i].isHeld;
                if (flag6)
                {
                    bool isServer = Unity.Netcode.NetworkManager.Singleton.IsServer;
                    if (isServer)
                    {
                        bool isSpawned = array[i].NetworkObject.IsSpawned;
                        if (isSpawned)
                        {
                            array[i].NetworkObject.Despawn(true);
                        }
                        else
                        {
                            Object.Destroy(array[i].gameObject);
                        }
                    }
                }
                else
                {
                    bool flag7 = array[i].isHeld && array[i].playerHeldBy != null;
                    if (flag7)
                    {
                        array[i].playerHeldBy.DropAllHeldItems(true, false);
                    }
                }
            }
            DeadBodyInfo[] array2 = Object.FindObjectsOfType<DeadBodyInfo>();
            for (int j = 0; j < array2.Length; j++)
            {
                Object.Destroy(array2[j].gameObject);
            }
            StartOfRound.Instance.livingPlayers++;
            StartOfRound.Instance.allPlayersDead = false;
            StartOfRound.Instance.UpdatePlayerVoiceEffects();
        }

        private static Vector3 GetPlayerSpawnPosition(int playerNum, bool simpleTeleport = false)
        {
            Debug.Log("Get Player Spawn Position");
            Vector3 result;
            if (simpleTeleport)
            {
                result = StartOfRound.Instance.playerSpawnPositions[0].position;
            }
            else
            {
                Debug.DrawRay(StartOfRound.Instance.playerSpawnPositions[playerNum].position, Vector3.up, Color.red, 15f);
                bool flag = !Physics.CheckSphere(StartOfRound.Instance.playerSpawnPositions[playerNum].position, 0.2f, 67108864, QueryTriggerInteraction.Ignore);
                if (flag)
                {
                    result = StartOfRound.Instance.playerSpawnPositions[playerNum].position;
                }
                else
                {
                    bool flag2 = !Physics.CheckSphere(StartOfRound.Instance.playerSpawnPositions[playerNum].position + Vector3.up, 0.2f, 67108864, QueryTriggerInteraction.Ignore);
                    if (flag2)
                    {
                        result = StartOfRound.Instance.playerSpawnPositions[playerNum].position + Vector3.up * 0.5f;
                    }
                    else
                    {
                        for (int i = 0; i < StartOfRound.Instance.playerSpawnPositions.Length; i++)
                        {
                            bool flag3 = i != playerNum;
                            if (flag3)
                            {
                                Debug.DrawRay(StartOfRound.Instance.playerSpawnPositions[i].position, Vector3.up, Color.green, 15f);
                                bool flag4 = !Physics.CheckSphere(StartOfRound.Instance.playerSpawnPositions[i].position, 0.12f, -67108865, QueryTriggerInteraction.Ignore);
                                if (flag4)
                                {
                                    return StartOfRound.Instance.playerSpawnPositions[i].position;
                                }
                                bool flag5 = !Physics.CheckSphere(StartOfRound.Instance.playerSpawnPositions[i].position + Vector3.up, 0.12f, 67108864, QueryTriggerInteraction.Ignore);
                                if (flag5)
                                {
                                    return StartOfRound.Instance.playerSpawnPositions[i].position + Vector3.up * 0.5f;
                                }
                            }
                        }
                        System.Random random = new System.Random(65);
                        float y = StartOfRound.Instance.playerSpawnPositions[0].position.y;
                        for (int j = 0; j < 15; j++)
                        {
                            Vector3 vector = new Vector3((float)random.Next((int)StartOfRound.Instance.shipInnerRoomBounds.bounds.min.x, (int)StartOfRound.Instance.shipInnerRoomBounds.bounds.max.x), y, (float)random.Next((int)StartOfRound.Instance.shipInnerRoomBounds.bounds.min.z, (int)StartOfRound.Instance.shipInnerRoomBounds.bounds.max.z));
                            vector = StartOfRound.Instance.shipInnerRoomBounds.transform.InverseTransformPoint(vector);
                            Debug.DrawRay(vector, Vector3.up, Color.yellow, 15f);
                            bool flag6 = !Physics.CheckSphere(vector, 0.12f, 67108864, QueryTriggerInteraction.Ignore);
                            if (flag6)
                            {
                                return StartOfRound.Instance.playerSpawnPositions[j].position;
                            }
                        }
                        result = StartOfRound.Instance.playerSpawnPositions[0].position + Vector3.up * 0.5f;
                    }
                }
            }
            return result;
        }
    }
}
