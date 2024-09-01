using Unity.Netcode;
using HarmonyLib;
using System.Collections;
using UnityEngine;
using GameNetcodeStuff;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace LCRandomizerMod.Patches
{
    [HarmonyPatch(typeof(MineshaftElevatorController))]
    internal class MineshaftElevatorControllerPatch
    {
        //private static Predicate<MineshaftElevatorController> ElevatorAccessible = (elevator) => {

        //    PlayerControllerB playerInRange = null;
        //    foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
        //    {
        //        if (Vector3.Distance(player.transform.position, elevator.elevatorPoint.transform.position) < 1f)
        //        {
        //            playerInRange = player;
        //            break;
        //        }
        //    }
        //    return ((elevator.elevatorPoint.position == elevator.elevatorBottomPoint.position) || (elevator.elevatorPoint.position == elevator.elevatorTopPoint.position)) && (!RandomizerValues.elevatorMalfunctioned || playerInRange != null);
        //};

        [HarmonyPatch(nameof(MineshaftElevatorController.PressElevatorButtonOnServer))]
        [HarmonyPostfix]
        public static void RandomlyStopElevator(MineshaftElevatorController __instance)
        {
            if (NetworkManager.Singleton.IsServer) 
            {
                GameNetworkManager.Instance.StartCoroutine(RollElevatorMalfunctionChance(__instance));
            }
        }

        private static IEnumerator RollElevatorMalfunctionChance(MineshaftElevatorController elevator)
        {
            yield return new WaitForSeconds(2.5f);

            while (!elevator.elevatorFinishedMoving)
            {
                yield return new WaitForSeconds(1f);
                if (new System.Random().Next(0, 4) == 3)
                {
                    elevator.elevatorAnimator.speed = 0f;
                    elevator.SetElevatorMusicServerRpc(false);
                    elevator.elevatorAudio.PlayOneShot(elevator.elevatorMovingDown ? elevator.elevatorFinishDownSFX : elevator.elevatorFinishUpSFX);
                    RandomizerValues.elevatorMalfunctioned = true;
                    SendElevatorMalfunction();
                    RandomizerValues.waitJumpCoroutine = GameNetworkManager.Instance.StartCoroutine(WaitUntilJumpCoroutine()); //Store coroutine so it can be stopped
                    GameNetworkManager.Instance.StartCoroutine(ElevatorFailsafeCoroutine()); //Run failsafe coroutine (REQUIRES WORK)

                    List<ulong> playersInElevator = new List<ulong>();
                    foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
                    {
                        if (player.transform.localScale.x >= 1f ? Vector3.Distance(player.transform.position, elevator.elevatorPoint.transform.position) < 1f : Vector3.Distance(player.transform.position, elevator.elevatorPoint.transform.position) < 2f)
                        {
                            playersInElevator.Add(player.playerClientId);
                        }
                    }
                    CustomUI.BroadcastMessage("<color=red>The elevator has stuck. Maybe you could force it to move again?</color>", 5, includeServer: playersInElevator.Contains(GameNetworkManager.Instance.localPlayerController.playerClientId), playersInElevator.ToArray());
                    RandomizerModBase.mls.LogError("Elevator malfunctioned, waiting for reset via jumping...");
                    yield break;
                }
            }
        }

        private static void SendElevatorMalfunction()
        {
            FastBufferWriter writer = new FastBufferWriter(4, Unity.Collections.Allocator.Temp, -1);
            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ElevatorHasMalfunctioned", writer, NetworkDelivery.Reliable);
        }

        public static void ClientStopElevatorOnServerMsg(ulong _, FastBufferReader __)
        {
            MineshaftElevatorController elevator = GameObject.FindObjectOfType<MineshaftElevatorController>();
            elevator.elevatorAnimator.speed = 0f;
            elevator.elevatorAudio.PlayOneShot(elevator.elevatorMovingDown ? elevator.elevatorFinishDownSFX : elevator.elevatorFinishUpSFX);
            RandomizerValues.elevatorMalfunctioned = true;
        }

        public static void ServerUnstuckElevator(ulong _, FastBufferReader __)
        {
            RandomizerValues.elevatorMalfunctioned = false;
        }

        public static void ClientUnstuckElevator(ulong _, FastBufferReader __)
        {
            MineshaftElevatorController elevator = GameObject.FindObjectOfType<MineshaftElevatorController>();

            elevator.elevatorAnimator.speed = 1f;
        }

        private static IEnumerator WaitUntilJumpCoroutine()
        {
            MineshaftElevatorController elevator = GameObject.FindObjectOfType<MineshaftElevatorController>();
            yield return new WaitUntil(() => !RandomizerValues.elevatorMalfunctioned);

            elevator.elevatorAnimator.speed = 1f;
            elevator.SetElevatorMusicServerRpc(true);

            SendUnstuckMsgToClients();
            yield break;
        }

        private static void SendUnstuckMsgToClients()
        {
            FastBufferWriter writer = new FastBufferWriter(4, Unity.Collections.Allocator.Temp, -1);
            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ElevatorHasBeenUnstuck", writer, NetworkDelivery.Reliable);
        }

        private static IEnumerator ElevatorFailsafeCoroutine()
        {
            yield return new WaitForSeconds(10f);

            MineshaftElevatorController elevator = GameObject.FindObjectOfType<MineshaftElevatorController>();

            if ((elevator.elevatorPoint.position == elevator.elevatorBottomPoint.position) || (elevator.elevatorPoint.position == elevator.elevatorTopPoint.position))
            {
                RandomizerModBase.mls.LogError("Failsafe exiting because elevator has already stopped.");
                yield break;
            }

            foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
            {
                if (player.transform.localScale.x >= 1f ? Vector3.Distance(player.transform.position, elevator.elevatorPoint.transform.position) < 1f : Vector3.Distance(player.transform.position, elevator.elevatorPoint.transform.position) < 2f)
                {
                    RandomizerModBase.mls.LogError("Failsafe exiting because player is inside elevator.");
                    yield break;
                } 
            }

            if (RandomizerValues.waitJumpCoroutine != null) GameNetworkManager.Instance.StopCoroutine(RandomizerValues.waitJumpCoroutine); //Stop waiting for jump because it won't happen

            RandomizerModBase.mls.LogError("Failsafe triggered, resuming elevator.");
            RandomizerValues.elevatorMalfunctioned = false;
            elevator.elevatorAnimator.speed = 1f;
            elevator.SetElevatorMusicServerRpc(true);

            SendUnstuckMsgToClients();
            yield break;
        }

        private static IEnumerator WaitUntilLandCoroutine()
        {
            yield return new WaitForSeconds(1f);
            PlayerControllerB player = GameNetworkManager.Instance.localPlayerController;
            RandomizerModBase.mls.LogWarning("Jumped");
            RaycastHit hit;
            if (Physics.Raycast(player.gameplayCamera.transform.position, player.gameplayCamera.transform.up * -1, out hit, 5f, 2816))
            {
                RandomizerModBase.mls.LogError("Raycast hit " + hit.collider.name);
                MineshaftElevatorController elevator = hit.transform.GetComponentInParent<MineshaftElevatorController>();
                if (elevator != null && RandomizerValues.elevatorMalfunctioned)
                {
                    HUDManager.Instance.ShakeCamera(ScreenShakeType.Big);
                    if (new System.Random().Next(0, 5) == 0)
                    {
                        if (NetworkManager.Singleton.IsServer)
                        {
                            RandomizerValues.elevatorMalfunctioned = false;
                        }
                        else
                        {
                            FastBufferWriter writer = new FastBufferWriter(4, Unity.Collections.Allocator.Temp, -1);
                            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage("Tibnan.lcrandomizermod_" + "ServerUnstuckElevator", 0UL, writer, NetworkDelivery.Reliable);
                            RandomizerValues.elevatorMalfunctioned = false;
                        }
                    }
                }
            }

            RandomizerValues.waitJumpLandCoroutine = null;
        }

        public static void ResetElevatorSpeedOnJump(InputAction.CallbackContext context)
        {
            if (RandomizerValues.waitJumpLandCoroutine == null)
            {
                RandomizerValues.waitJumpLandCoroutine = GameNetworkManager.Instance.StartCoroutine(WaitUntilLandCoroutine());
            }
        }
    }
}
