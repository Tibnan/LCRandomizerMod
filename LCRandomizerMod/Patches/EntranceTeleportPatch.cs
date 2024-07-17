using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections;
using Unity.Netcode;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LCRandomizerMod.Patches
{
    [HarmonyPatch(typeof(EntranceTeleport))]
    internal class EntranceTeleportPatch
    {
        private enum FireExitBehaviour { ShowShipSafetyStatus, TeleportPlayerToShip, TempStaminaBoost, DropAllHeldItems, PermanentlyBlockExit, ReceiveCrewData, KillPlayer, TempBlindness }
        private enum ShipSafetyState { Safe, Dangerous, Caution }

        [HarmonyPatch(nameof(EntranceTeleport.TeleportPlayer))]
        [HarmonyPrefix]
        public static bool RunRandomBehaviourOnUse(EntranceTeleport __instance)
        {
            if (RandomizerValues.blockedFireExits.Contains(__instance))
            {
                HUDManager.Instance.DisplayTip("Obstructed", "The other end appears to be blocked.", false, false, "LC_Tip1");
                return false;
            }
            else if (RandomizerValues.entranceTPCoroutinePlaying)
            {
                return true;
            }

            RandomizerModBase.mls.LogWarning("Current state:");
            RandomizerModBase.mls.LogWarning("entrance id: " + __instance.entranceId);
            RandomizerModBase.mls.LogWarning("player in factory? " + GameNetworkManager.Instance.localPlayerController.isInsideFactory);

            if (__instance.entranceId != 0 && GameNetworkManager.Instance.localPlayerController.isInsideFactory && new System.Random().Next(1, 2) == 1)
            {
                FireExitBehaviour[] behaviours = Enum.GetValues(typeof(FireExitBehaviour)) as FireExitBehaviour[];

                switch (behaviours[new System.Random().Next(0, behaviours.Length)])
                {
                    case FireExitBehaviour.ShowShipSafetyStatus:
                        {
                            if (!RandomizerValues.entranceTPCoroutinePlaying)
                            {
                                RandomizerValues.entranceTPCoroutinePlaying = true;
                                GameNetworkManager.Instance.StartCoroutine(ShowShipSafetyStatusCoroutine());
                            }
                            break;
                        }
                    case FireExitBehaviour.TeleportPlayerToShip:
                        {
                            GameNetworkManager.Instance.localPlayerController.TeleportPlayer(TerminalPatch.GetPlayerSpawnPosition((int)GameNetworkManager.Instance.localPlayerController.playerClientId));
                            break;
                        }
                    case FireExitBehaviour.TempStaminaBoost:
                        {
                            if (!RandomizerValues.entranceTPCoroutinePlaying) 
                            {
                                RandomizerValues.entranceTPCoroutinePlaying = true;
                                GameNetworkManager.Instance.StartCoroutine(GiveTemporaryStaminaBoost());
                            }
                            break;
                        }
                    case FireExitBehaviour.DropAllHeldItems:
                        {
                            GameNetworkManager.Instance.localPlayerController.DropAllHeldItems(true);
                            break;
                        }
                    case FireExitBehaviour.PermanentlyBlockExit:
                        {
                            if (NetworkManager.Singleton.IsServer)
                            {
                                PermanentlyBlockExitLocal(__instance);
                                SendBlockedExit(__instance);
                            }
                            else
                            {
                                FastBufferWriter writer = new FastBufferWriter(sizeof(ulong), Unity.Collections.Allocator.Temp, -1);
                                writer.WriteValueSafe<ulong>(__instance.NetworkObjectId);

                                NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage("Tibnan.lcrandomizermod_" + "ServerBlockExit", 0UL, writer, NetworkDelivery.Reliable);
                            }
                            break;
                        }
                    case FireExitBehaviour.ReceiveCrewData:
                        {
                            if (!RandomizerValues.entranceTPCoroutinePlaying)
                            {
                                RandomizerValues.entranceTPCoroutinePlaying = true;
                                GameNetworkManager.Instance.StartCoroutine(DisplayCrewStatusCoroutine());
                            }
                            break;
                        }
                    case FireExitBehaviour.KillPlayer:
                        {
                            GameNetworkManager.Instance.localPlayerController.KillPlayer(GameNetworkManager.Instance.localPlayerController.velocityLastFrame, true, CauseOfDeath.Unknown);
                            break;
                        }
                    case FireExitBehaviour.TempBlindness:
                        {
                            if (!RandomizerValues.entranceTPCoroutinePlaying)
                            {
                                RandomizerValues.entranceTPCoroutinePlaying = true;
                                GameNetworkManager.Instance.StartCoroutine(GiveTemporaryBlindness());
                            }
                            break;
                        }
                }
            }

            return true;
        }

        //[HarmonyPatch(nameof(EntranceTeleport.TeleportPlayer))]
        //[HarmonyPrefix]
        //public static bool StopTPIfBlocked(EntranceTeleport __instance)
        //{
        //    if (RandomizerValues.blockedFireExits.Contains(__instance))
        //    {
        //        HUDManager.Instance.DisplayTip("Obstructed", "The other end appears to be blocked.", false, false, "LC_Tip1");
        //        return false;
        //    }
        //    else return true;
        //}

        public static IEnumerator ShowShipSafetyStatusCoroutine()
        {
            CustomUI playerUI = GameNetworkManager.Instance.localPlayerController.gameObject.GetComponent<CustomUI>();

            switch (GetShipSafety())
            {
                case ShipSafetyState.Safe:
                    {
                        playerUI.ShowLocalMessage("<color=blue>Ship Status</color>\n<color=green>Safe</color>", 3);
                        RandomizerValues.entranceTPCoroutinePlaying = false;
                        yield break;
                    }
                case ShipSafetyState.Caution:
                    {
                        playerUI.ShowLocalMessage("<color=blue>Ship Status</color>\n<color=yellow>Excercise caution</color>", 3);
                        RandomizerValues.entranceTPCoroutinePlaying = false;
                        yield break;
                    }
                case ShipSafetyState.Dangerous:
                    {
                        playerUI.ShowLocalMessage("<color=blue>Ship Status</color>\n<color=red>Dangerous</color>", 3);
                        //playerUI.SetText();
                        //playerUI.Show(true);
                        //playerUI.FadeOut(3);
                        RandomizerValues.entranceTPCoroutinePlaying = false;
                        yield break;
                    }
            }
        }

        private static ShipSafetyState GetShipSafety()
        {
            Collider[] colliders = Physics.OverlapSphere(GameObject.FindObjectOfType<Terminal>().transform.position, 50f, 2621448, QueryTriggerInteraction.Collide);
            List<EnemyAI> enemies = new List<EnemyAI>();
            if (colliders.Length > 0)
            {
                foreach (Collider collider in colliders)
                {
                    EnemyAI enemyAI = collider.gameObject.GetComponentInParent<EnemyAI>();
                    if (enemyAI != null && !enemyAI.isEnemyDead && !enemyAI.enemyType.isDaytimeEnemy)
                    {
                        RandomizerModBase.mls.LogError(enemyAI.enemyType.enemyName);
                        if (!enemies.Contains(enemyAI))
                        {
                            enemies.Add(enemyAI);
                        }
                    }
                }

                if (enemies.Count == 1)
                {
                    return ShipSafetyState.Caution;
                }
                else if (enemies.Count > 1)
                {
                    return ShipSafetyState.Dangerous;
                }
                else
                {
                    return ShipSafetyState.Safe;
                }
            }
            else
            {
                return ShipSafetyState.Safe;
            }
        }

        public static IEnumerator GiveTemporaryStaminaBoost()
        {
            RandomizerModBase.mls.LogWarning("RUNNING COROUTINE");
            PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;
            float origSpeed = localPlayer.movementSpeed;
            localPlayer.movementSpeed *= 2;
            CustomUI playerUI = localPlayer.gameObject.GetComponent<CustomUI>();
            int timer = 30;
            playerUI.SetText(String.Format("Second Wind: {0}", timer));
            playerUI.Show(true);
            while (timer > 0)
            {
                timer -= 1;
                playerUI.SetText(String.Format("Second Wind: {0}", timer));
                yield return new WaitForSeconds(1);
            }

            localPlayer.movementSpeed = origSpeed;
            playerUI.FadeOut();
            RandomizerModBase.mls.LogError("Coroutine ended");
            RandomizerValues.entranceTPCoroutinePlaying = false;
            yield break;
        }

        public static void ServerBlockExitAndSync(ulong _, FastBufferReader reader)
        {
            ulong id;

            reader.ReadValueSafe<ulong>(out id);

            NetworkObject networkObject = Unity.Netcode.NetworkManager.Singleton.SpawnManager.SpawnedObjects[id];

            EntranceTeleport entrance = networkObject.gameObject.GetComponentInChildren<EntranceTeleport>();

            PermanentlyBlockExitLocal(entrance);
            SendBlockedExit(entrance);
        }

        public static void SendBlockedExit(EntranceTeleport __instance)
        {
            FastBufferWriter writer = new FastBufferWriter(sizeof(ulong), Unity.Collections.Allocator.Temp, -1);
            writer.WriteValueSafe<ulong>(__instance.NetworkObjectId);

            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ExitHasBeenBlocked", writer, NetworkDelivery.Reliable);
        }

        public static void ClientExitIDResolver(ulong _, FastBufferReader reader)
        {
            ulong id;

            reader.ReadValueSafe<ulong>(out id);

            NetworkObject networkObject = Unity.Netcode.NetworkManager.Singleton.SpawnManager.SpawnedObjects[id];
            EntranceTeleport entrance = networkObject.gameObject.GetComponentInChildren<EntranceTeleport>();

            PermanentlyBlockExitLocal(entrance);
        }

        public static void PermanentlyBlockExitLocal(EntranceTeleport __instance)
        {
            RandomizerModBase.mls.LogError("BEFORE: " + RandomizerValues.blockedFireExits.Count);
            RandomizerModBase.mls.LogWarning("ID: " + __instance.entranceId);

            RandomizerValues.blockedFireExits.Add(__instance);

            EntranceTeleport[] entrances = GameObject.FindObjectsOfType<EntranceTeleport>();

            foreach (EntranceTeleport entrance in entrances)
            {
                if (entrance.entranceId == __instance.entranceId && !RandomizerValues.blockedFireExits.Contains(entrance))
                {
                    RandomizerModBase.mls.LogError("Blocked entrance of exit point: " + __instance.entranceId);
                    RandomizerValues.blockedFireExits.Add(entrance);
                    break;
                }
            }
            RandomizerModBase.mls.LogError("AFTER: " + RandomizerValues.blockedFireExits.Count);
        }

        public static IEnumerator DisplayCrewStatusCoroutine()
        {
            RandomizerValues.entranceTPCoroutinePlaying = true;
            PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;
            CustomUI playerUI = localPlayer.gameObject.GetComponent<CustomUI>();

            //Dictionary<string, bool[]> crewInfo = CollectCrewInfo();
            List<CrewInfo> crewInfo = CollectCrewInfo();

            StringBuilder sb = new StringBuilder();

            foreach (CrewInfo member in crewInfo)
            {
                //RandomizerModBase.mls.LogError(kvp.Key + " " + kvp.Value[0] + " " + kvp.Value[1]);

                sb.Append(member.Name);
                sb.Append(": ");
                sb.Append(member.IsDead ? "<color=red>DECEASED</color>" : "<color=green>ALIVE</color>");
                sb.Append(", ");
                sb.Append(member.IsInsideFactory ? "IN BUILDING, " : "OUTSIDE, ");
                sb.Append(member.EnemyCount > 1 ? "<color=red>IN DANGER</color>" : "<color=green>SAFE</color>");
                sb.Append("\n");
            }

            RandomizerModBase.mls.LogWarning("RESULT: " + sb.ToString());

            playerUI.SetText(String.Format("<color=blue>Crew Status:</color>\n{0}", sb.ToString()));

            playerUI.Show(true);
            int timer = 10;
            while (timer > 0)
            {
                timer -= 1;
                yield return new WaitForSeconds(1);
            }
            playerUI.FadeOut();
            RandomizerValues.entranceTPCoroutinePlaying = false;
            yield break;
        }

        private static List<CrewInfo> CollectCrewInfo()
        {
            List<CrewInfo> crewInfo = new List<CrewInfo>();

            RandomizerModBase.mls.LogInfo("ALL PLAYER SCRIPTS: " + StartOfRound.Instance.allPlayerScripts.Length);

            foreach (PlayerControllerB crewmate in StartOfRound.Instance.allPlayerScripts)
            {
                if (!crewmate.isPlayerControlled && !crewmate.isPlayerDead) continue;

                crewInfo.Add(new CrewInfo(crewmate.playerUsername, crewmate.isPlayerDead, crewmate.isInsideFactory, crewmate.transform.position));
            }
            RandomizerModBase.mls.LogInfo("Returning dict with " + crewInfo.Count + " entries");
            return crewInfo;
        }

        public static IEnumerator GiveTemporaryBlindness()
        {
            RandomizerModBase.mls.LogWarning("RUNNING COROUTINE");

            CustomUI playerUI = GameNetworkManager.Instance.localPlayerController.gameObject.GetComponent<CustomUI>();

            Color origDirectColor = TimeOfDay.Instance.sunDirect.color;
            Color origIndirectColor = TimeOfDay.Instance.sunIndirect.color;

            TimeOfDay.Instance.sunIndirect.color = Color.black;
            TimeOfDay.Instance.sunDirect.color = Color.black;
            float origPlaneValue = GameNetworkManager.Instance.localPlayerController.gameplayCamera.farClipPlane;
            GameNetworkManager.Instance.localPlayerController.gameplayCamera.farClipPlane = 60f;

            int timer = 10;
            playerUI.SetText("Blindness: " + timer);
            playerUI.Show(true);
            while (timer > 0)
            {
                timer -= 1;
                playerUI.SetText("Blindness: " + timer);
                yield return new WaitForSeconds(1);
            }

            TimeOfDay.Instance.sunIndirect.color = origIndirectColor;
            TimeOfDay.Instance.sunDirect.color = origDirectColor;
            GameNetworkManager.Instance.localPlayerController.gameplayCamera.farClipPlane = origPlaneValue;

            playerUI.FadeOut();
            RandomizerValues.entranceTPCoroutinePlaying = false;
            yield break;
        }
    }
}
