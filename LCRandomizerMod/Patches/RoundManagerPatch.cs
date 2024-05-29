using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace LCRandomizerMod.Patches
{
    [HarmonyPatch(typeof(RoundManager))]
    internal class RoundManagerPatch
    {
        private protected enum LevelObjectRandomization { None, Landmine, Turret, SpikeRoofTrap }
        private protected enum EnemySpawnRandomization { None, Flowerman, Nutcracker, RadMech, Crawler, DressGirl, BaboonBird, Jester, Butler, HoarderBug, ForestGiant, MouthDog, SandSpider, Blob }
        
        private protected static Predicate<EnemyAINestSpawnObject> isMechSpawner = MechSpawnerPredicate;

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

                List<LevelObjectRandomization> alreadyRandomizedObject = new List<LevelObjectRandomization>();

                do
                {
                    LevelObjectRandomization randomizedObject = objRand[new System.Random().Next(0, objRand.Length)];

                    if (alreadyRandomizedObject.Contains(randomizedObject))
                    {
                        continue;
                    }

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
                                        new Keyframe(0f, Convert.ToSingle(new System.Random().Next(0, 51))),
                                        new Keyframe(1f, 25f)
                                        });

                                        alreadyRandomizedObject.Add(randomizedObject);
                                        RandomizerModBase.mls.LogError("Randomized landmines.");
                                    }
                                    break;
                                }
                            case LevelObjectRandomization.Turret:
                                {
                                    if (spawnableMapObject.prefabToSpawn.GetComponentInChildren<Turret>() != null)
                                    {
                                        spawnableMapObject.numberToSpawn = new AnimationCurve(new Keyframe[]
                                        {
                                        new Keyframe(0f, Convert.ToSingle(new System.Random().Next(0, 11))),
                                        new Keyframe(1f, 25f)
                                        });

                                        alreadyRandomizedObject.Add(randomizedObject);
                                        RandomizerModBase.mls.LogError("Randomized turrets.");
                                    }
                                    break;
                                }
                            case LevelObjectRandomization.SpikeRoofTrap:
                                {
                                    if (spawnableMapObject.prefabToSpawn.GetComponent<SpikeRoofTrap>() != null)
                                    {
                                        spawnableMapObject.numberToSpawn = new AnimationCurve(new Keyframe[]
                                        {
                                        new Keyframe(0f, Convert.ToSingle(new System.Random().Next(0, 11))),
                                        new Keyframe(1f, 25f)
                                        });

                                        alreadyRandomizedObject.Add(randomizedObject);
                                        RandomizerModBase.mls.LogError("Randomized turrets.");
                                    }
                                    break;
                                }
                            case LevelObjectRandomization.None:
                                {
                                    RandomizerModBase.mls.LogError("No level object randomized beyond this point.");
                                    goto EndObjectLoop;
                                }
                        }
                    }
                } while (new System.Random().Next(0, 5) != 4 || alreadyRandomizedObject.Count == objRand.Length);

                EndObjectLoop:

                List<EnemySpawnRandomization> alreadyRandomized = new List<EnemySpawnRandomization>();
                EnemySpawnRandomization[] enemyTypeArray = Enum.GetValues(typeof(EnemySpawnRandomization)) as EnemySpawnRandomization[];
                do
                {
                    EnemySpawnRandomization enemyToRandomize = enemyTypeArray[new System.Random().Next(0, enemyTypeArray.Length)];
                    if (enemyToRandomize == EnemySpawnRandomization.None)
                    {
                        goto EndLoop;
                    }

                    if (alreadyRandomized.Contains(enemyToRandomize))
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

                                        //spawnableEnemyWithRarity.enemyType.MaxCount = new System.Random().Next(4, 20);
                                        //spawnableEnemyWithRarity.enemyType.PowerLevel = new System.Random().Next(4, 20);
                                        //spawnableEnemyWithRarity.enemyType.probabilityCurve = new AnimationCurve(new Keyframe[]
                                        //{
                                        //new Keyframe(0f, Convert.ToSingle(new System.Random().Next(100, 101))),
                                        //new Keyframe(1f, 25f)
                                        //});
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

                                        //spawnableEnemyWithRarity.enemyType.MaxCount = new System.Random().Next(4, 20);
                                        //spawnableEnemyWithRarity.enemyType.PowerLevel = new System.Random().Next(4, 20);
                                        //spawnableEnemyWithRarity.enemyType.probabilityCurve = new AnimationCurve(new Keyframe[]
                                        //{
                                        //new Keyframe(0f, Convert.ToSingle(new System.Random().Next(100, 101))),
                                        //new Keyframe(1f, 25f)
                                        //});
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

                                        //spawnableEnemyWithRarity.enemyType.MaxCount = new System.Random().Next(4, 20);
                                        //spawnableEnemyWithRarity.enemyType.PowerLevel = new System.Random().Next(4, 20);
                                        //spawnableEnemyWithRarity.enemyType.probabilityCurve = new AnimationCurve(new Keyframe[]
                                        //{
                                        //new Keyframe(0f, Convert.ToSingle(new System.Random().Next(100, 101))),
                                        //new Keyframe(1f, 25f)
                                        //});
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

                                        //spawnableEnemyWithRarity.enemyType.MaxCount = new System.Random().Next(4, 20);
                                        //spawnableEnemyWithRarity.enemyType.PowerLevel = new System.Random().Next(4, 20);
                                        //spawnableEnemyWithRarity.enemyType.probabilityCurve = new AnimationCurve(new Keyframe[]
                                        //{
                                        //new Keyframe(0f, Convert.ToSingle(new System.Random().Next(100, 101))),
                                        //new Keyframe(1f, 25f)
                                        //});
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

                                        //spawnableEnemyWithRarity.enemyType.MaxCount = new System.Random().Next(4, 20);
                                        //spawnableEnemyWithRarity.enemyType.PowerLevel = new System.Random().Next(4, 20);
                                        //spawnableEnemyWithRarity.enemyType.probabilityCurve = new AnimationCurve(new Keyframe[]
                                        //{
                                        //new Keyframe(0f, Convert.ToSingle(new System.Random().Next(100, 101))),
                                        //new Keyframe(1f, 25f)
                                        //});
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

                                        //spawnableEnemyWithRarity.enemyType.MaxCount = new System.Random().Next(4, 20);
                                        //spawnableEnemyWithRarity.enemyType.PowerLevel = new System.Random().Next(4, 20);
                                        //spawnableEnemyWithRarity.enemyType.probabilityCurve = new AnimationCurve(new Keyframe[]
                                        //{
                                        //new Keyframe(0f, Convert.ToSingle(new System.Random().Next(100, 101))),
                                        //new Keyframe(1f, 25f)
                                        //});
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

                                        //spawnableEnemyWithRarity.enemyType.MaxCount = new System.Random().Next(4, 20);
                                        //spawnableEnemyWithRarity.enemyType.PowerLevel = new System.Random().Next(4, 20);
                                        //spawnableEnemyWithRarity.enemyType.probabilityCurve = new AnimationCurve(new Keyframe[]
                                        //{
                                        //new Keyframe(0f, Convert.ToSingle(new System.Random().Next(100, 101))),
                                        //new Keyframe(1f, 25f)
                                        //});
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

                                        //spawnableEnemyWithRarity.enemyType.MaxCount = new System.Random().Next(4, 20);
                                        //spawnableEnemyWithRarity.enemyType.PowerLevel = new System.Random().Next(4, 20);
                                        //spawnableEnemyWithRarity.enemyType.probabilityCurve = new AnimationCurve(new Keyframe[]
                                        //{
                                        //new Keyframe(0f, Convert.ToSingle(new System.Random().Next(100, 101))),
                                        //new Keyframe(1f, 25f)
                                        //});
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

                                        //spawnableEnemyWithRarity.enemyType.MaxCount = new System.Random().Next(4, 20);
                                        //spawnableEnemyWithRarity.enemyType.PowerLevel = new System.Random().Next(4, 20);
                                        //spawnableEnemyWithRarity.enemyType.probabilityCurve = new AnimationCurve(new Keyframe[]
                                        //{
                                        //new Keyframe(0f, Convert.ToSingle(new System.Random().Next(100, 101))),
                                        //new Keyframe(1f, 25f)
                                        //});
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

                                        //spawnableEnemyWithRarity.enemyType.MaxCount = new System.Random().Next(4, 20);
                                        //spawnableEnemyWithRarity.enemyType.PowerLevel = new System.Random().Next(4, 20);
                                        //spawnableEnemyWithRarity.enemyType.probabilityCurve = new AnimationCurve(new Keyframe[]
                                        //{
                                        //new Keyframe(0f, Convert.ToSingle(new System.Random().Next(100, 101))),
                                        //new Keyframe(1f, 25f)
                                        //});
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

                                        //spawnableEnemyWithRarity.enemyType.MaxCount = new System.Random().Next(4, 20);
                                        //spawnableEnemyWithRarity.enemyType.PowerLevel = new System.Random().Next(4, 20);
                                        //spawnableEnemyWithRarity.enemyType.probabilityCurve = new AnimationCurve(new Keyframe[]
                                        //{
                                        //new Keyframe(0f, Convert.ToSingle(new System.Random().Next(100, 101))),
                                        //new Keyframe(1f, 25f)
                                        //});
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

                                        //spawnableEnemyWithRarity.enemyType.MaxCount = new System.Random().Next(4, 20);
                                        //spawnableEnemyWithRarity.enemyType.PowerLevel = new System.Random().Next(4, 20);
                                        //spawnableEnemyWithRarity.enemyType.probabilityCurve = new AnimationCurve(new Keyframe[]
                                        //{
                                        //new Keyframe(0f, Convert.ToSingle(new System.Random().Next(100, 101))),
                                        //new Keyframe(1f, 25f)
                                        //});
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

                                        //spawnableEnemyWithRarity.enemyType.MaxCount = new System.Random().Next(4, 20);
                                        //spawnableEnemyWithRarity.enemyType.PowerLevel = new System.Random().Next(4, 20);
                                        //spawnableEnemyWithRarity.enemyType.probabilityCurve = new AnimationCurve(new Keyframe[]
                                        //{
                                        //new Keyframe(0f, Convert.ToSingle(new System.Random().Next(100, 101))),
                                        //new Keyframe(1f, 25f)
                                        //});
                                        alreadyRandomized.Add(EnemySpawnRandomization.SandSpider);
                                    }
                                    break;
                                }
                        }
                    }
                } while ((new System.Random().Next(0, 15) != 14) || alreadyRandomized.Count == enemyTypeArray.Length);

                EndLoop:
                RandomizerModBase.mls.LogInfo("No enemies were randomized beyond this point.");
                __instance.currentMaxInsidePower = new System.Random().Next(0, 2000);
                //__instance.currentDaytimeEnemyPower = new System.Random().Next(0, 2000);
                //__instance.currentEnemyPower = new System.Random().Next(0, 2000);
                __instance.currentMaxOutsidePower = new System.Random().Next(0, 2000);
                RandomizerModBase.mls.LogError("MaxInsidePower: " + __instance.currentMaxInsidePower + " MaxOutsidePower: " + __instance.currentMaxOutsidePower);
                return;
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

        [HarmonyPatch(nameof(RoundManager.PredictAllOutsideEnemies))]
        [HarmonyPostfix]
        public static void SyncMechSpawnNestScales()
        {
            if (Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                for (int i = 0; i < RoundManager.Instance.enemyNestSpawnObjects.Count; i++)
                {
                    if (isMechSpawner(RoundManager.Instance.enemyNestSpawnObjects.ElementAt(i)))
                    {
                        float scale = Convert.ToSingle(new System.Random().Next(1, 31)) / 10;
                        RandomizerModBase.mls.LogError("SCALING");
                        RoundManager.Instance.enemyNestSpawnObjects.ElementAt(i).transform.localScale = new Vector3(scale, scale, scale);
                    }
                }
            }
        }

        private static bool MechSpawnerPredicate(EnemyAINestSpawnObject spawner)
        {
            return spawner != null && spawner.enemyType != null && spawner.enemyType.enemyName == "RadMech";
        }
    }
}
