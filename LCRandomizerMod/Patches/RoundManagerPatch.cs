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
        private enum LevelObjectRandomization { None, Landmine, Turret, SpikeRoofTrap }
        private enum EnemySpawnRandomization { None, Flowerman, Nutcracker, RadMech, Crawler, DressGirl, BaboonBird, Jester, Butler, HoarderBug, ForestGiant, MouthDog, SandSpider, Blob }

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
            }
            else
            {
                return;
            }
        }

        [HarmonyPatch(nameof(RoundManager.LoadNewLevel))]
        [HarmonyPrefix]
        public static void RandomizeSpawns(RoundManager __instance)
        {
            if (Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                LevelObjectRandomization[] objRand = Enum.GetValues(typeof(LevelObjectRandomization)) as LevelObjectRandomization[];
                LevelObjectRandomization randomizedObject = objRand[new System.Random().Next(0, objRand.Length)];

                foreach (SpawnableMapObject spawnableMapObject in __instance.currentLevel.spawnableMapObjects)
                {
                    switch (randomizedObject)
                    {
                        case LevelObjectRandomization.Landmine:
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
                                break;
                            }
                        case LevelObjectRandomization.Turret:
                            {
                                if (spawnableMapObject.prefabToSpawn.GetComponentInChildren<Turret>() != null)
                                {
                                    spawnableMapObject.numberToSpawn = new AnimationCurve(new Keyframe[]
                                    {
                                        new Keyframe(0f, Convert.ToSingle(new System.Random().Next(0, 10))),
                                        new Keyframe(1f, 25f)
                                    });
                                }
                                break;
                            }
                        case LevelObjectRandomization.SpikeRoofTrap:
                            {
                                if (spawnableMapObject.prefabToSpawn.GetComponent<SpikeRoofTrap>() != null)
                                {
                                    spawnableMapObject.numberToSpawn = new AnimationCurve(new Keyframe[]
                                    {
                                        new Keyframe(0f, Convert.ToSingle(new System.Random().Next(0, 10))),
                                        new Keyframe(1f, 25f)
                                    });
                                }
                                break;
                            }
                        case LevelObjectRandomization.None:
                            {
                                break;
                            }
                    }
                }

                List<EnemySpawnRandomization> alreadyRandomized = new List<EnemySpawnRandomization>();
                EnemySpawnRandomization[] enemyTypeArray = Enum.GetValues(typeof(EnemySpawnRandomization)) as EnemySpawnRandomization[];
                do
                {
                    EnemySpawnRandomization enemyToRandomize = enemyTypeArray[new System.Random().Next(0, enemyTypeArray.Length)];
                    if (enemyToRandomize == EnemySpawnRandomization.None || alreadyRandomized.Contains(enemyToRandomize))
                    {
                        RandomizerModBase.mls.LogError("Enemy to randomize: " + enemyToRandomize + ", list already contains it so continuing...");
                        continue;
                    }

                    foreach (SpawnableEnemyWithRarity spawnableEnemyWithRarity in __instance.currentLevel.Enemies)
                    {
                        switch (enemyToRandomize)
                        {
                            case EnemySpawnRandomization.BaboonBird:
                                {
                                    if (spawnableEnemyWithRarity.enemyType.enemyPrefab.GetComponent<BaboonBirdAI>() != null)
                                    {
                                        spawnableEnemyWithRarity.rarity = new System.Random().Next(0, 101);
                                        RandomizerModBase.mls.LogInfo("Randomized Baboon bird. Rarity: " + spawnableEnemyWithRarity.rarity);
                                        alreadyRandomized.Add(EnemySpawnRandomization.BaboonBird);
                                    }
                                    break;
                                }
                            case EnemySpawnRandomization.Blob:
                                {
                                    if (spawnableEnemyWithRarity.enemyType.enemyPrefab.GetComponent<BlobAI>() != null)
                                    {
                                        spawnableEnemyWithRarity.rarity = new System.Random().Next(0, 101);
                                        RandomizerModBase.mls.LogInfo("Randomized Blob. Rarity: " + spawnableEnemyWithRarity.rarity);
                                        alreadyRandomized.Add(EnemySpawnRandomization.Blob);
                                    }
                                    break;
                                }
                            case EnemySpawnRandomization.Butler:
                                {
                                    if (spawnableEnemyWithRarity.enemyType.enemyPrefab.GetComponent<ButlerEnemyAI>() != null)
                                    {
                                        spawnableEnemyWithRarity.rarity = new System.Random().Next(0, 101);
                                        RandomizerModBase.mls.LogInfo("Randomized Butler. Rarity: " + spawnableEnemyWithRarity.rarity);
                                        alreadyRandomized.Add(EnemySpawnRandomization.Butler);
                                    }
                                    break;
                                }
                            case EnemySpawnRandomization.Crawler:
                                {
                                    if (spawnableEnemyWithRarity.enemyType.enemyPrefab.GetComponent<CrawlerAI>() != null)
                                    {
                                        spawnableEnemyWithRarity.rarity = new System.Random().Next(0, 101);
                                        RandomizerModBase.mls.LogInfo("Randomized Crawler. Rarity: " + spawnableEnemyWithRarity.rarity);
                                        alreadyRandomized.Add(EnemySpawnRandomization.Crawler);
                                    }
                                    break;
                                }
                            case EnemySpawnRandomization.DressGirl:
                                {
                                    if (spawnableEnemyWithRarity.enemyType.enemyPrefab.GetComponent<DressGirlAI>() != null)
                                    {
                                        spawnableEnemyWithRarity.rarity = new System.Random().Next(0, 101);
                                        RandomizerModBase.mls.LogInfo("Randomized DressGirl. Rarity: " + spawnableEnemyWithRarity.rarity);
                                        alreadyRandomized.Add(EnemySpawnRandomization.DressGirl);
                                    }
                                    break;
                                }
                            case EnemySpawnRandomization.Flowerman:
                                {
                                    if (spawnableEnemyWithRarity.enemyType.enemyPrefab.GetComponent<FlowermanAI>() != null)
                                    {
                                        spawnableEnemyWithRarity.rarity = new System.Random().Next(0, 101);
                                        RandomizerModBase.mls.LogInfo("Randomized Flowerman. Rarity: " + spawnableEnemyWithRarity.rarity);
                                        alreadyRandomized.Add(EnemySpawnRandomization.Flowerman);
                                    }
                                    break;
                                }
                            case EnemySpawnRandomization.ForestGiant:
                                {
                                    if (spawnableEnemyWithRarity.enemyType.enemyPrefab.GetComponent<ForestGiantAI>() != null)
                                    {
                                        spawnableEnemyWithRarity.rarity = new System.Random().Next(0, 101);
                                        RandomizerModBase.mls.LogInfo("Randomized ForestGiant. Rarity: " + spawnableEnemyWithRarity.rarity);
                                        alreadyRandomized.Add(EnemySpawnRandomization.ForestGiant);
                                    }
                                    break;
                                }
                            case EnemySpawnRandomization.HoarderBug:
                                {
                                    if (spawnableEnemyWithRarity.enemyType.enemyPrefab.GetComponent<HoarderBugAI>() != null)
                                    {
                                        spawnableEnemyWithRarity.rarity = new System.Random().Next(0, 101);
                                        RandomizerModBase.mls.LogInfo("Randomized HoarderBug. Rarity: " + spawnableEnemyWithRarity.rarity);
                                        alreadyRandomized.Add(EnemySpawnRandomization.HoarderBug);
                                    }
                                    break;
                                }
                            case EnemySpawnRandomization.Jester:
                                {
                                    if (spawnableEnemyWithRarity.enemyType.enemyPrefab.GetComponent<JesterAI>() != null)
                                    {
                                        spawnableEnemyWithRarity.rarity = new System.Random().Next(0, 101);
                                        RandomizerModBase.mls.LogInfo("Randomized Jester. Rarity: " + spawnableEnemyWithRarity.rarity);
                                        alreadyRandomized.Add(EnemySpawnRandomization.Jester);
                                    }
                                    break;
                                }
                            case EnemySpawnRandomization.MouthDog:
                                {
                                    if (spawnableEnemyWithRarity.enemyType.enemyPrefab.GetComponent<MouthDogAI>() != null)
                                    {
                                        spawnableEnemyWithRarity.rarity = new System.Random().Next(0, 101);
                                        RandomizerModBase.mls.LogInfo("Randomized MouthDog. Rarity: " + spawnableEnemyWithRarity.rarity);
                                        alreadyRandomized.Add(EnemySpawnRandomization.MouthDog);
                                    }
                                    break;
                                }
                            case EnemySpawnRandomization.Nutcracker:
                                {
                                    if (spawnableEnemyWithRarity.enemyType.enemyPrefab.GetComponent<NutcrackerEnemyAI>() != null)
                                    {
                                        spawnableEnemyWithRarity.rarity = new System.Random().Next(0, 101);
                                        RandomizerModBase.mls.LogInfo("Randomized Nutcracker. Rarity: " + spawnableEnemyWithRarity.rarity);
                                        alreadyRandomized.Add(EnemySpawnRandomization.Nutcracker);
                                    }
                                    break;
                                }
                            case EnemySpawnRandomization.RadMech:
                                {
                                    if (spawnableEnemyWithRarity.enemyType.enemyPrefab.GetComponent<RadMechAI>() != null)
                                    {
                                        spawnableEnemyWithRarity.rarity = new System.Random().Next(0, 101);
                                        RandomizerModBase.mls.LogInfo("Randomized RadMech. Rarity: " + spawnableEnemyWithRarity.rarity);
                                        alreadyRandomized.Add(EnemySpawnRandomization.RadMech);
                                    }
                                    break;
                                }
                            case EnemySpawnRandomization.SandSpider:
                                {
                                    if (spawnableEnemyWithRarity.enemyType.enemyPrefab.GetComponent<SandSpiderAI>() != null)
                                    {
                                        spawnableEnemyWithRarity.rarity = new System.Random().Next(0, 101);
                                        RandomizerModBase.mls.LogInfo("Randomized SandSpider. Rarity: " + spawnableEnemyWithRarity.rarity);
                                        alreadyRandomized.Add(EnemySpawnRandomization.SandSpider);
                                    }
                                    break;
                                }
                        }
                    }
                } while ((new System.Random().Next(1, 16) != 15) || alreadyRandomized.Count == enemyTypeArray.Length);
            }
            else
            {
                return;
            }
        }

        [HarmonyPatch(nameof(RoundManager.GenerateNewFloor))]
        [HarmonyPrefix]
        public static void RandomizeFactorySize(RoundManager __instance)
        {
            __instance.currentLevel.factorySizeMultiplier = RandomizerValues.factorySizeMultiplierRand;
            RandomizerModBase.mls.LogInfo("Set factory size multiplier to: " + __instance.currentLevel.factorySizeMultiplier);
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

        [HarmonyPatch(nameof(RoundManager.SyncNestSpawnPositionsClientRpc))]
        [HarmonyPostfix]
        public static void SyncMechSpawnNestScales(RoundManager __instance)
        {
            if (Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                EnemyAINestSpawnObject[] array = RoundManager.Instance.enemyNestSpawnObjects.ToArray();

                for (int i = 0; i < array.Length; i++)
                {
                    FastBufferWriter fastBufferMechScaleWriter = new FastBufferWriter(sizeof(ulong) + sizeof(float), Unity.Collections.Allocator.Temp, -1);
                    if (array[i].enemyType.name == "RadMech")
                    {
                        float scale = Convert.ToSingle(new System.Random().Next(1, 31)) / 10;
                        array[i].gameObject.transform.localScale = new Vector3(scale, scale, scale);
                        RandomizerValues.spawnedMechScales.Add(scale);

                        NetworkObject networkObject;
                        array[i].TryGetComponent<NetworkObject>(out networkObject);
                        if (networkObject == null)
                        {
                            RandomizerModBase.mls.LogError("COULDN'T GET NETWORK OBJECT!!!");
                            RandomizerModBase.mls.LogError("COULDN'T GET NETWORK OBJECT!!!");
                            RandomizerModBase.mls.LogError("COULDN'T GET NETWORK OBJECT!!!");
                        }

                        fastBufferMechScaleWriter.WriteValueSafe<ulong>(networkObject.NetworkObjectId);
                        fastBufferMechScaleWriter.WriteValueSafe<float>(scale);

                        Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesMechScaleArray", fastBufferMechScaleWriter, NetworkDelivery.Reliable);
                        fastBufferMechScaleWriter.Dispose();
                    }
                }
            }
        }
    }
}
