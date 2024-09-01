using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace LCRandomizerMod.Patches
{
    [HarmonyPatch(typeof(ClockProp))]
    internal class ClockPropPatch
    {
        [HarmonyPatch("__initializeVariables")]
        [HarmonyPostfix]
        public static void SpeedUpClockRandomly(ClockProp __instance)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                RandomizerModBase.mls.LogError("Speeding up 1 clock");
                if (!RandomizerValues.clockSecondsToAdd.ContainsKey(__instance.NetworkObjectId)) 
                {
                    __instance.gameObject.AddComponent<ClockDataContainer>();
                    RandomizerValues.clockSecondsToAdd.Add(__instance.NetworkObjectId, new System.Random().Next(0, 2));
                }
            }
        }

        [HarmonyPatch(nameof(ClockProp.Update))]
        [HarmonyPrefix]
        public static bool AddSeconds(ClockProp __instance)
        {
            __instance.gameObject.GetComponentInChildren<GrabbableObject>().Update(); //Test this
            ClockDataContainer clockData = __instance.gameObject.GetComponent<ClockDataContainer>();

            if (Time.realtimeSinceStartup - clockData.timeOfLastSecond > 0.5f)
            {
                __instance.secondHand.Rotate(-6f, 0f, 0f, Space.Self);
                clockData.secondsPassed++;
                if (clockData.secondsPassed >= 60)
                {
                    clockData.secondsPassed = 0;
                    clockData.minutesPassed++;
                    __instance.minuteHand.Rotate(-6f, 0f, 0f, Space.Self);
                }
                if (clockData.minutesPassed > 60)
                {
                    clockData.minutesPassed = 0;
                    __instance.hourHand.Rotate(-30f, 0f, 0f, Space.Self);
                }
                clockData.timeOfLastSecond = Time.realtimeSinceStartup;
                clockData.tickOrTock = !clockData.tickOrTock;

                __instance.tickAudio.PlayOneShot(clockData.tickOrTock ? __instance.tickSFX : __instance.tockSFX);
            }
            return false;
        }
    }
}
