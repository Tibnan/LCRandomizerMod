using HarmonyLib;

namespace LCRandomizerMod.Patches
{
    [HarmonyPatch(typeof(ExtensionLadderItem))]
    internal class ExtensionLadderItemPatch
    {
        [HarmonyPatch("LadderAnimation")]
        [HarmonyPrefix]
        public static void ExtensionAmountOverride(ExtensionLadderItem __instance)
        {
            if (Unity.Netcode.NetworkManager.Singleton.IsServer && !RandomizerValues.isRandomized)
            {
                Traverse.Create(__instance).Field("extendAmount").SetValue(999f);
                RandomizerModBase.mls.LogInfo("Set extend amount to: " + Traverse.Create(__instance).Field("extendAmount").GetValue<float>());
                RandomizerValues.isRandomized = true; //will need to work on this
            }
        }
    }
}
