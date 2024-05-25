using HarmonyLib;
using UnityEngine;

namespace LCRandomizerMod.Patches
{
    [HarmonyPatch(typeof(ExtensionLadderItem))]
    internal class ExtensionLadderItemPatch
    {
        [HarmonyPatch(nameof(ExtensionLadderItem.Update))]
        [HarmonyPostfix]
        public static void ExtensionAmountOverride(ExtensionLadderItem __instance)
        {
            if (Unity.Netcode.NetworkManager.Singleton.IsServer && !RandomizerValues.isRandomized)
            {
                __instance.ladderAnimator.SetFloat("extensionAmount", new System.Random().Next(10, 900));
            }
        }
    }
}
