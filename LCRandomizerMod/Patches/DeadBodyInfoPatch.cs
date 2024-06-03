using HarmonyLib;
using UnityEngine;

namespace LCRandomizerMod.Patches
{
    [HarmonyPatch(typeof(DeadBodyInfo))]
    internal class DeadBodyInfoPatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        public static void ResizeDeadBody(DeadBodyInfo __instance)
        {
            __instance.gameObject.transform.localScale = __instance.playerScript.gameObject.transform.localScale;
            __instance.gameObject.GetComponentInChildren<Renderer>().material.color = __instance.playerScript.thisPlayerModel.material.color;
        }
    }
}
