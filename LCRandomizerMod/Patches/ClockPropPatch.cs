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
                RandomizerValues.clockSecondsToAdd.Add(__instance.NetworkObjectId, new System.Random().Next(0, 6));
            }
        }

        [HarmonyPatch(nameof(ClockProp.Update))]
        [HarmonyPostfix]
        public static void AddSeconds(ClockProp __instance)
        {
            
        }
    }
}
