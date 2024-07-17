using HarmonyLib;

namespace LCRandomizerMod.Patches
{
    [HarmonyPatch(typeof(SprayPaintItem))]
    internal class SprayPaintItemPatch
    {
        [HarmonyPatch(nameof(SprayPaintItem.Start))]
        [HarmonyPostfix]
        public static void AddFlowermanKillPropery(SprayPaintItem __instance)
        {
            if (__instance.isWeedKillerSprayBottle) __instance.gameObject.AddComponent<WeedKillerFlowermanInteract>();
        }
    }
}
