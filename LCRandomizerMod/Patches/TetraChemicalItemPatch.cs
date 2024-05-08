using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;

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
