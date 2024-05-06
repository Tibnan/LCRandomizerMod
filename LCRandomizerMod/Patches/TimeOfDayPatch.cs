using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using System.Threading.Tasks;

namespace LCRandomizerMod.Patches
{
    [HarmonyPatch(typeof(TimeOfDay))]
    internal class TimeOfDayPatch
    {
        static public int randomizedWeatherIdx;

        [HarmonyPatch(nameof(TimeOfDay.SetWeatherBasedOnVariables))]
        [HarmonyPrefix]
        public static void RandomizeWeatherVariables(TimeOfDay __instance)
        {
            __instance.DisableAllWeather();
            
            foreach(WeatherEffect effect in __instance.effects)
            {
                effect.effectEnabled = false;
                if (effect.effectPermanentObject != null)
                {
                    effect.effectPermanentObject.SetActive(false);
                }
                if (effect.effectObject != null)
                {
                    effect.effectObject.SetActive(false);
                }
            }
        }

        [HarmonyPatch(nameof(TimeOfDay.SetWeatherBasedOnVariables))]
        [HarmonyPostfix]
        public static void SetRandomizedWeatherEffects(TimeOfDay __instance)
        {
            foreach (WeatherEffect effect in __instance.effects)
            {
                effect.effectEnabled = false;
                if (effect.effectPermanentObject != null)
                {
                    effect.effectPermanentObject.SetActive(false);
                }
                if (effect.effectObject != null)
                {
                    effect.effectObject.SetActive(false);
                }
            }

            if (!GameNetworkManager.Instance.localPlayerController.isInsideFactory)
            {
                if (RoundManagerPatch.randomizedWeatherIdx == 2 || RoundManagerPatch.randomizedWeatherIdx == 4)
                {
                    RandomizerModBase.mls.LogInfo("Enabling both effect object and perm. obj. with index: " + RoundManagerPatch.randomizedWeatherIdx);
                    __instance.effects[RoundManagerPatch.randomizedWeatherIdx].effectEnabled = true;
                    __instance.effects[RoundManagerPatch.randomizedWeatherIdx].effectObject.SetActive(true);
                    RandomizerModBase.mls.LogInfo("Enabling weather object: " + __instance.effects[RoundManagerPatch.randomizedWeatherIdx].effectObject.name);
                    __instance.effects[RoundManagerPatch.randomizedWeatherIdx].effectPermanentObject.SetActive(true);
                    RandomizerModBase.mls.LogInfo("Enabling permanent weather object: " + __instance.effects[RoundManagerPatch.randomizedWeatherIdx].effectPermanentObject.name);
                }
                else
                {
                    RandomizerModBase.mls.LogInfo("Enabling weather effect: " + __instance.effects[RoundManagerPatch.randomizedWeatherIdx].name);
                    RandomizerModBase.mls.LogInfo("Enabling only weather effect with index: " + RoundManagerPatch.randomizedWeatherIdx);
                    __instance.effects[RoundManagerPatch.randomizedWeatherIdx].effectEnabled = true;
                }
            }
        }
    }
}
