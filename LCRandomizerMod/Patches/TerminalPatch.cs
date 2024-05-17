using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Windows.Forms;
using System.Diagnostics.Eventing.Reader;

namespace LCRandomizerMod.Patches
{
    [HarmonyPatch(typeof(Terminal))]
    internal class TerminalPatch
    {
        [HarmonyPatch(nameof(Terminal.OnSubmit))]
        [HarmonyPrefix]
        public static bool CheckForRandom(Terminal __instance)
        {
            if (!RandomizerValues.mapRandomizedInTerminal && StartOfRound.Instance.inShipPhase)
            {
                RandomizerModBase.mls.LogInfo("PARSED SENTENCE:");
                RandomizerModBase.mls.LogInfo(__instance.screenText.text.Substring(__instance.screenText.text.Length - __instance.textAdded).ToLower());

                if (__instance.screenText.text.Substring(__instance.screenText.text.Length - __instance.textAdded).Contains("random"))
                {
                    int num = new System.Random().Next(1, 13);

                    while (num == 11)
                    {
                        num = new System.Random().Next(1, 13);
                    }

                    StartOfRound.Instance.ChangeLevelServerRpc(num, __instance.groupCredits);
                    __instance.screenText.text = "";
                    __instance.QuitTerminal();
                    RandomizerValues.mapRandomizedInTerminal = true;

                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                if (StartOfRound.Instance.inShipPhase)
                {
                    RandomizerModBase.mls.LogInfo("RANDOMIZED STATE: " + RandomizerValues.mapRandomizedInTerminal);
                    HUDManager.Instance.AddTextToChatOnServer("<color=red>You are only allowed to randomize once per round start.</color>", -1);
                }
                return true;
            }
        }

        [HarmonyPatch(nameof(Terminal.LoadNewNode))]
        [HarmonyPrefix]
        public static bool CheckForAlreadyRandomized(Terminal __instance)
        {
            return !RandomizerValues.mapRandomizedInTerminal;
        }
    }
}
