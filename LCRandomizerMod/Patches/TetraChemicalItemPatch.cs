using HarmonyLib;

namespace LCRandomizerMod.Patches
{
    [HarmonyPatch(typeof(TetraChemicalItem))]
    internal class TetraChemicalItemPatch
    {
        [HarmonyPatch("Update")]
        [HarmonyPrefix]
        public static void PitchUpdate(TetraChemicalItem __instance)
        {
        }
    }
}
