using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LCRandomizerMod.Patches
{
    [HarmonyPatch(typeof(WhoopieCushionItem))]
    internal class WhoopieCushionItemPatch
    {
        [HarmonyPatch(nameof(WhoopieCushionItem.Fart))]
        [HarmonyPrefix]
        public static void PitchOverride(WhoopieCushionItem __instance)
        {
            __instance.whoopieCushionAudio.pitch = Convert.ToSingle(new System.Random().Next(1, 201)) / 10;

            if (new System.Random().Next(1, 5) == 4)
            {
                Landmine.SpawnExplosion(__instance.transform.position, true);
            }
        }
    }
}
