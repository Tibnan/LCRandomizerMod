using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;

namespace LCRandomizerMod.Patches
{
    [HarmonyPatch(typeof(RoundManager))]
    internal class RoundManagerPatch
    {
        static public int randomizedWeatherIdx;

        [HarmonyPatch(nameof(RoundManager.SpawnScrapInLevel))]
        [HarmonyPrefix]
        public static void RandomizeLevelProperties(RoundManager __instance)
        {
            //if (Unity.Netcode.NetworkManager.Singleton.IsServer)
            //{
                __instance.scrapAmountMultiplier = new Random().Next(1, 4);
                RandomizerModBase.mls.LogInfo("Scrap amount multiplier: " + __instance.scrapAmountMultiplier);
                __instance.currentLevel.maxScrap = new Random().Next(20, 51);
                RandomizerModBase.mls.LogInfo("Max scrap: " + __instance.currentLevel.maxScrap);
                __instance.currentLevel.minScrap = new Random().Next(1, 5);
                RandomizerModBase.mls.LogInfo("Min scrap: " + __instance.currentLevel.minScrap);
                __instance.currentLevel.maxTotalScrapValue = new Random().Next(100, 2000);
                __instance.currentLevel.minTotalScrapValue = new Random().Next(1, 100);
                RandomizerModBase.mls.LogInfo("Set max total scrap value to: " + __instance.currentLevel.maxTotalScrapValue + " and min total scrap value to: " + __instance.currentLevel.minTotalScrapValue);
                __instance.currentLevel.factorySizeMultiplier = new Random().Next(2, 5);
                RandomizerModBase.mls.LogInfo("Set factory size multiplier to: " + __instance.currentLevel.factorySizeMultiplier);
            //}
        }

        [HarmonyPatch(nameof(RoundManager.SpawnEnemyOnServer))]
        [HarmonyPrefix]
        public static void RandomizeEnemyProperties(RoundManager __instance)
        {
            if (Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                __instance.currentLevel.maxEnemyPowerCount = new Random().Next(1, 5);
                RandomizerModBase.mls.LogInfo("Set max enemy power count to: " + __instance.currentLevel.maxEnemyPowerCount);
                __instance.currentEnemyPower = Convert.ToSingle(new Random().Next(1, 51)) / 10;
                RandomizerModBase.mls.LogInfo("Set current enemy power to: " + __instance.currentEnemyPower);
            }
        }

        [HarmonyPatch("SetToCurrentLevelWeather")]
        [HarmonyPrefix]
        public static void RandomizeWeather(RoundManager __instance)
        {
            if (Unity.Netcode.NetworkManager.Singleton.IsHost)
            {
                LevelWeatherType[] weatherTypes = Enum.GetValues(typeof(LevelWeatherType)) as LevelWeatherType[];
                TimeOfDay.Instance.currentLevelWeather = LevelWeatherType.None;
                int rndWeather = StartOfRoundPatch.GetWeatherIndex;

                TimeOfDay.Instance.currentLevelWeather = weatherTypes[rndWeather];
                TimeOfDay.Instance.currentLevel.currentWeather = weatherTypes[rndWeather];

                __instance.currentLevel.currentWeather = weatherTypes[rndWeather];
                __instance.currentLevel.overrideWeatherType = weatherTypes[rndWeather];

                for (int i = 0; i < TimeOfDay.Instance.effects.Length; i++)
                {
                    RandomizerModBase.mls.LogInfo(TimeOfDay.Instance.effects[i].name + " " + i);
                }

                foreach (LevelWeatherType type in weatherTypes)
                {
                    RandomizerModBase.mls.LogInfo(type);
                }

                randomizedWeatherIdx = rndWeather;

                RandomizerModBase.mls.LogInfo("Current level weather set to: " + TimeOfDay.Instance.currentLevelWeather);
                RandomizerModBase.mls.LogInfo("Rolled number: " + rndWeather);
            }
            else
            {
                LevelWeatherType[] weatherTypes = Enum.GetValues(typeof(LevelWeatherType)) as LevelWeatherType[];
                TimeOfDay.Instance.currentLevelWeather = LevelWeatherType.None;
                int rndWeather = randomizedWeatherIdx;

                TimeOfDay.Instance.currentLevelWeather = weatherTypes[rndWeather];
                TimeOfDay.Instance.currentLevel.currentWeather = weatherTypes[rndWeather];

                __instance.currentLevel.currentWeather = weatherTypes[rndWeather];
                __instance.currentLevel.overrideWeatherType = weatherTypes[rndWeather];

                for (int i = 0; i < TimeOfDay.Instance.effects.Length; i++)
                {
                    RandomizerModBase.mls.LogInfo(TimeOfDay.Instance.effects[i].name + " " + i);
                }

                foreach (LevelWeatherType type in weatherTypes)
                {
                    RandomizerModBase.mls.LogInfo(type);
                }

                randomizedWeatherIdx = rndWeather;

                RandomizerModBase.mls.LogInfo("Current level weather set to: " + TimeOfDay.Instance.currentLevelWeather);
                RandomizerModBase.mls.LogInfo("Rolled number: " + rndWeather);
                return;
            }
        }

        public static void SetReceivedWeatherData(ulong _, FastBufferReader reader)
        {
            if (!Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                reader.ReadValueSafe<int>(out randomizedWeatherIdx, default);
                RandomizerModBase.mls.LogInfo("Received weather data: " + randomizedWeatherIdx);
            }
        }
    }
}
