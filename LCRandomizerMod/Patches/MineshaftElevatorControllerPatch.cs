using Unity.Netcode;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using GameNetcodeStuff;
using UnityEngine.InputSystem;

namespace LCRandomizerMod.Patches
{
    [HarmonyPatch(typeof(MineshaftElevatorController))]
    internal class MineshaftElevatorControllerPatch
    {
        [HarmonyPatch("OnEnable")]
        [HarmonyPostfix]
        public static void SetRandomElevatorSpeed(MineshaftElevatorController __instance)
        {
            //float speed = new System.Random().Next(3, 61) / 10f;

            //__instance.elevatorAnimator.speed = speed;
            //__instance.elevatorAudio.pitch = Mathf.Lerp(0.5f, 2.5f, Mathf.InverseLerp(0.3f, 6f, speed));
            //__instance.elevatorJingleMusic.pitch = Mathf.Lerp(0.5f, 2.5f, Mathf.InverseLerp(0.3f, 6f, speed));
        }

        [HarmonyPatch(nameof(MineshaftElevatorController.PressElevatorButtonOnServer))]
        [HarmonyPostfix]
        public static void RandomlyStopElevator(MineshaftElevatorController __instance)
        {
            if (NetworkManager.Singleton.IsServer) GameNetworkManager.Instance.StartCoroutine(RollElevatorMalfunctionChance(__instance));
        }

        public static IEnumerator RollElevatorMalfunctionChance(MineshaftElevatorController elevator)
        {
            yield return new WaitForSeconds(2.5f);

            while (!elevator.elevatorFinishedMoving)
            {
                yield return new WaitForSeconds(1f);
                if (new System.Random().Next(0, 4) == 3) //IMPORTANT TODO: IMPLEMENT FAILSAFE WHEN NOONE IS INSIDE ELEVATOR!!!!
                {
                    elevator.elevatorAnimator.speed = 0f;
                    elevator.elevatorJingleMusic.Stop();
                    elevator.elevatorAudio.PlayOneShot(!elevator.elevatorMovingDown ? elevator.elevatorFinishDownSFX : elevator.elevatorFinishUpSFX);
                    RandomizerValues.elevatorMalfunctioned = true;
                    RandomizerModBase.mls.LogError("ELEVATOR MALFUNCTION");
                    yield break;
                }
            }
        }

        public static void ResetElevatorSpeedOnJump(InputAction.CallbackContext context)
        {
            PlayerControllerB player = GameNetworkManager.Instance.localPlayerController;
            RandomizerModBase.mls.LogWarning("Jumped");
            RaycastHit hit;
            if (Physics.Raycast(player.gameplayCamera.transform.position, player.gameplayCamera.transform.up * -1, out hit, 5f, 2816))
            {
                RandomizerModBase.mls.LogError("Raycast hit " + hit.collider.name);
                MineshaftElevatorController elevator = hit.transform.GetComponentInParent<MineshaftElevatorController>();
                if (elevator != null)
                {
                    HUDManager.Instance.ShakeCamera(ScreenShakeType.Small);
                    if (new System.Random().Next(0, 5) == 0 && RandomizerValues.elevatorMalfunctioned)
                    {
                        RandomizerModBase.mls.LogError("Resetting elevator speed");
                        elevator.elevatorAnimator.speed = 1f;
                        RandomizerValues.elevatorMalfunctioned = false;
                    }
                }
            }
        }
    }
}
