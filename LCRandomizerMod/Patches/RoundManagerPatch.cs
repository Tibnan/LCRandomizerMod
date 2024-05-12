using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace LCRandomizerMod.Patches
{
    [HarmonyPatch(typeof(RoundManager))]
    internal class RoundManagerPatch
    {
        [HarmonyPatch(nameof(RoundManager.SpawnScrapInLevel))]
        [HarmonyPrefix]
        public static void RandomizeLevelProperties(RoundManager __instance)
        {
            if (Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                __instance.scrapAmountMultiplier = new System.Random().Next(1, 4);
                RandomizerModBase.mls.LogInfo("Scrap amount multiplier: " + __instance.scrapAmountMultiplier);
                __instance.currentLevel.maxScrap = new System.Random().Next(20, 51);
                RandomizerModBase.mls.LogInfo("Max scrap: " + __instance.currentLevel.maxScrap);
                __instance.currentLevel.minScrap = new System.Random().Next(1, 5);
                RandomizerModBase.mls.LogInfo("Min scrap: " + __instance.currentLevel.minScrap);
                __instance.currentLevel.maxTotalScrapValue = new System.Random().Next(100, 2000);
                __instance.currentLevel.minTotalScrapValue = new System.Random().Next(1, 100);
                RandomizerModBase.mls.LogInfo("Set max total scrap value to: " + __instance.currentLevel.maxTotalScrapValue + " and min total scrap value to: " + __instance.currentLevel.minTotalScrapValue);
                __instance.currentLevel.factorySizeMultiplier = new System.Random().Next(2, 5);

                FastBufferWriter fastBufferFactoryWriter = new FastBufferWriter(sizeof(float), Unity.Collections.Allocator.Temp, -1);

                fastBufferFactoryWriter.WriteValueSafe<float>(__instance.currentLevel.factorySizeMultiplier);
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesFactoryData", fastBufferFactoryWriter, NetworkDelivery.Reliable);
                fastBufferFactoryWriter.Dispose();

                RandomizerModBase.mls.LogInfo("Set factory size multiplier to: " + __instance.currentLevel.factorySizeMultiplier);
            }
            else
            {
                __instance.currentLevel.factorySizeMultiplier = RandomizerValues.factorySizeMultiplierRand;
            }
        }

        [HarmonyPatch(nameof(RoundManager.LoadNewLevel))]
        [HarmonyPrefix]
        public static void RandomizeSpawns(RoundManager __instance)
        {
            foreach (SpawnableMapObject spawnableMapObject in  __instance.currentLevel.spawnableMapObjects)
            {
                if (spawnableMapObject.prefabToSpawn.GetComponentInChildren<Landmine>() != null)
                {
                    spawnableMapObject.numberToSpawn = new AnimationCurve(new Keyframe[]
                    {
                        new Keyframe(0f, Convert.ToSingle(new System.Random().Next(0, 70))),
                        new Keyframe(1f, 25f)
                    });

                    RandomizerModBase.mls.LogError("FOUND LANDMINE");
                }

                if (spawnableMapObject.prefabToSpawn.GetComponentInChildren<Turret>() != null)
                {
                    spawnableMapObject.numberToSpawn = new AnimationCurve(new Keyframe[]
                    {
                        new Keyframe(0f, Convert.ToSingle(new System.Random().Next(0, 10))),
                        new Keyframe(1f, 25f)
                    });
                }
            }
        }

        [HarmonyPatch("SetToCurrentLevelWeather")]
        [HarmonyPrefix]
        public static void LevelWeatherOverride(RoundManager __instance)
        {
            LevelWeatherType[] weatherTypes = Enum.GetValues(typeof(LevelWeatherType)) as LevelWeatherType[];
            
            __instance.currentLevel.overrideWeather = true;
            __instance.currentLevel.overrideWeatherType = weatherTypes[RandomizerValues.randomizedWeatherIdx];
            __instance.currentLevel.currentWeather = weatherTypes[RandomizerValues.randomizedWeatherIdx];
        }

        public static void SetReceivedWeatherData(ulong _, FastBufferReader reader)
        {
            if (!Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                reader.ReadValueSafe<int>(out RandomizerValues.randomizedWeatherIdx, default);
                RandomizerModBase.mls.LogInfo("Received weather data: " + RandomizerValues.randomizedWeatherIdx);
            }
        }

        public static void SetReceivedFactoryData(ulong _, FastBufferReader reader)
        {
            if (!Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                reader.ReadValueSafe<float>(out RandomizerValues.factorySizeMultiplierRand, default);
                RandomizerModBase.mls.LogInfo("Received factory data: " + RandomizerValues.factorySizeMultiplierRand);
            }
        }
    }
}
