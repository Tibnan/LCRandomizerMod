using GameNetcodeStuff;
using HarmonyLib;
using System;
using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

namespace LCRandomizerMod.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRoundPatch
    {
        [HarmonyPatch(nameof(StartOfRound.StartGame))]
        [HarmonyPostfix]
        public static void RandomizePlayerStatsOnLevelLoadServer(StartOfRound __instance)
        {
            //RandomizerValues.ClearDicts();

            RandomizerValues.firstTimeShow = TimeOfDay.Instance.profitQuota == 130;
            RandomizerModBase.mls.LogInfo("First time? " + RandomizerValues.firstTimeShow);
            RandomizerValues.mapRandomizedInTerminal = false;

            if (!RandomizerValues.audioDictLoaded)
            {
                FastBufferWriter writer = new FastBufferWriter(4, Unity.Collections.Allocator.Temp, -1);
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "LoadAudioDicts", writer, NetworkDelivery.Reliable);
            }

            if (Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                if (RandomizerValues.allItemsListDict.Count == 0)
                {
                    foreach (var item in __instance.allItemsList.itemsList)
                    {
                        RandomizerValues.allItemsListDict.Add(item.name, item);
                    }

                    foreach (var item in RandomizerValues.allItemsListDict)
                    {
                        RandomizerModBase.mls.LogInfo(item.Key);
                    }
                }

                //Generate random values
                RandomizerValues.sprintRand = Convert.ToSingle(new System.Random().Next(1, 101)) / 10;
                RandomizerValues.healthRand = new System.Random().Next(1, 201);
                RandomizerValues.movementSpeedRand = new System.Random().Next(30, 101) / 10;
                RandomizerValues.sinkMultiplierRand = new System.Random().Next(100, 10000) / 10;
                RandomizerValues.factorySizeMultiplierRand = new System.Random().Next(2, 5);


                FastBufferWriter fastBufferFactoryWriter = new FastBufferWriter(sizeof(float), Unity.Collections.Allocator.Temp, -1);

                fastBufferFactoryWriter.WriteValueSafe<float>(RandomizerValues.factorySizeMultiplierRand);
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesFactoryData", fastBufferFactoryWriter, NetworkDelivery.Reliable);
                fastBufferFactoryWriter.Dispose();

                //Generate random deadline and quota values on new save load and sync with clients
                if (TimeOfDay.Instance.profitQuota == 130)
                {
                    RandomizerValues.deadlineRand = GenerateNewDeadline(); //For testing
                    RandomizerModBase.mls.LogInfo("New deadline time: " + RandomizerValues.deadlineRand +  " (" + RandomizerValues.deadlineRand / 1080 + ") days");
                    RandomizerValues.quotaRand = new System.Random().Next(500, 2001); //For testing
                    RandomizerModBase.mls.LogInfo("New quota: " + RandomizerValues.quotaRand);

                    RandomizeQuotaVariables();

                    SendQuotaValues(RandomizerValues.deadlineRand, RandomizerValues.quotaRand);

                    TimeOfDay.Instance.timeUntilDeadline = RandomizerValues.deadlineRand;
                    int daysUntilDeadline = (int)Mathf.Floor(TimeOfDay.Instance.timeUntilDeadline / TimeOfDay.Instance.totalTime);
                    StartOfRound.Instance.deadlineMonitorText.text = string.Format("DEADLINE:\n{0} Days", daysUntilDeadline);

                    TimeOfDay.Instance.profitQuota = RandomizerValues.quotaRand;
                    StartOfRound.Instance.profitQuotaMonitorText.text = string.Format("PROFIT QUOTA:\n${0} / ${1}", TimeOfDay.Instance.quotaFulfilled, TimeOfDay.Instance.profitQuota);
                }

                //Generate random weather values
                LevelWeatherType[] weatherTypes = Enum.GetValues(typeof(LevelWeatherType)) as LevelWeatherType[];
                RandomizerValues.randomizedWeatherIdx = __instance.currentLevel.levelID == 3 ? 0 : new System.Random().Next(0, weatherTypes.Length);
                /*RandomizerValues.randomizedWeatherIdx = 5;*/ //For testing

                RandomizerModBase.mls.LogInfo("Randomized weather index: " + RandomizerValues.randomizedWeatherIdx);

                FastBufferWriter fastBufferWeatherWriter = new FastBufferWriter(sizeof(int), Unity.Collections.Allocator.Temp, -1);
                fastBufferWeatherWriter.WriteValueSafe<int>(RandomizerValues.randomizedWeatherIdx);

                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceiveWeatherData", fastBufferWeatherWriter, NetworkDelivery.Reliable);
                fastBufferWeatherWriter.Dispose();

                //Set values on server
                GameNetworkManager.Instance.localPlayerController.sprintTime = RandomizerValues.sprintRand;
                RandomizerModBase.mls.LogInfo("Set sprint time to: " + RandomizerValues.sprintRand);
                GameNetworkManager.Instance.localPlayerController.health = RandomizerValues.healthRand;
                foreach (GameObject playerObj in __instance.allPlayerObjects)
                {
                    playerObj.GetComponent<PlayerControllerB>().health = RandomizerValues.healthRand;
                }
                RandomizerModBase.mls.LogInfo("Set health to: " + RandomizerValues.healthRand);
                GameNetworkManager.Instance.localPlayerController.movementSpeed = RandomizerValues.movementSpeedRand;
                RandomizerModBase.mls.LogInfo("Set movement speed to: " + RandomizerValues.movementSpeedRand);
                GameNetworkManager.Instance.localPlayerController.sinkingSpeedMultiplier = RandomizerValues.sinkMultiplierRand;
                GameNetworkManager.Instance.localPlayerController.sinkingValue = RandomizerValues.sinkMultiplierRand;
                RandomizerModBase.mls.LogInfo("Set sinking values to:  " + RandomizerValues.sinkMultiplierRand);

                RandomizerModBase.mls.LogInfo("Sending values to clients...");
                float[] randValues = new float[] { RandomizerValues.sprintRand, RandomizerValues.healthRand, RandomizerValues.movementSpeedRand, RandomizerValues.sinkMultiplierRand};

                FastBufferWriter fastBufferRvalueWriter = new FastBufferWriter(sizeof(float) * randValues.Length, Unity.Collections.Allocator.Temp, -1);
                fastBufferRvalueWriter.WriteValueSafe<float>(randValues[0]);
                fastBufferRvalueWriter.WriteValueSafe<float>(randValues[1]);
                fastBufferRvalueWriter.WriteValueSafe<float>(randValues[2]);
                fastBufferRvalueWriter.WriteValueSafe<float>(randValues[3]);

                
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "SetValuesSentByServer", fastBufferRvalueWriter, NetworkDelivery.Reliable);
                RandomizerModBase.mls.LogInfo("Values sent to clients.");
                fastBufferRvalueWriter.Dispose();
                RandomizerModBase.mls.LogInfo("Disposing fast buffer writer.");

                float[] modelValues = new float[4];
                FastBufferWriter fastBufferModelValueWriter = new FastBufferWriter(sizeof(float) * modelValues.Length, Unity.Collections.Allocator.Temp, -1);
                for (int i = 0; i < 4; i++)
                {
                    modelValues[i] = Convert.ToSingle(new System.Random().Next(5, 16)) / 10;
                    fastBufferModelValueWriter.WriteValueSafe<float>(modelValues[i]);
                }


                for (int i = 0; i < Unity.Netcode.NetworkManager.Singleton.ConnectedClientsList.Count; i++)
                {
                    StartOfRound.Instance.allPlayerObjects[i].GetComponent<PlayerControllerB>().thisPlayerBody.localScale = RandomizerValues.defaultPlayerScale * modelValues[i];
                    StartOfRound.Instance.allPlayerObjects[i].GetComponent<PlayerControllerB>().playerGlobalHead.localScale = RandomizerValues.defaultPlayerHeadScale * modelValues[i];

                    GiftBoxItemPatch.RecolorPlayerSync(StartOfRound.Instance.allPlayerScripts[i]);
                }

                RandomizerModBase.mls.LogInfo("Sending player model values...");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "SetPlayerModelValues", fastBufferModelValueWriter, NetworkDelivery.Reliable);
                fastBufferModelValueWriter.Dispose();

                if (RandomizerValues.healthRand < 20)
                {
                    GameNetworkManager.Instance.localPlayerController.DamagePlayer(0, true, false, CauseOfDeath.Unknown, 0, false);
                }

                //SET PLAYER PITCH W/ NETWORKING, DONT TOUCH IT CUZ IT WORKS

                //FastBufferWriter fastBufferPitchWriter = new FastBufferWriter(sizeof(float) * 4, Unity.Collections.Allocator.Temp, -1);
                //float[] pitchValues = new float[4];
                //for (int i = 0; i < 4; i++)
                //{
                //    pitchValues[i] = Convert.ToSingle(new System.Random().Next(5, 30)) / 10;
                //    fastBufferPitchWriter.WriteValueSafe<float>(pitchValues[i]);
                //    SoundManager.Instance.SetPlayerPitch(pitchValues[i], i);
                //}

                //Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesPitchData", fastBufferPitchWriter, NetworkDelivery.Reliable);
                //fastBufferPitchWriter.Dispose();

                //SET PLAYER PITCH W/ NETWORKING, DONT TOUCH IT CUZ IT WORKS

                for (int i = 0; i < Unity.Netcode.NetworkManager.Singleton.ConnectedClientsList.Count; i++)
                {

                    //modelValues[i] <= 1 ? Mathf.Lerp(1f, 1.5f, 1-(modelValues[i] - 0.5f) * 2) : Mathf.Lerp(0.5f, 1f, 1-(modelValues[i] - 1f) * 2)


                    RandomizerModBase.mls.LogInfo("Setting player pitch: " + (modelValues[i] <= 1 ? Mathf.Lerp(1f, 13f, 1-(modelValues[i] - 0.5f) * 2) : Mathf.Lerp(0.7f, 1f, 1-(modelValues[i] - 1f) * 2)) + " for player: " + i + " with size multiplier: " + modelValues[i] + " isServer? " + Unity.Netcode.NetworkManager.Singleton.IsServer);
                    SoundManager.Instance.SetPlayerPitch(modelValues[i] <= 1 ? Mathf.Lerp(1f, 13f, 1-(modelValues[i] - 0.5f) * 2) : Mathf.Lerp(0.7f, 1f, 1-(modelValues[i] - 1f) * 2), i);
                    //SoundManager.Instance.SetPlayerPitch(modelValues[i] <= 1 ? Mathf.Lerp(1f, 15f, 1 - (modelValues[i] - 0.5f) * 2) : Mathf.Lerp(0.5f, 1f, 1 - (modelValues[i] - 1f) * 2), i);
                }

                RandomizerValues.shipDoorAnimatorSpeed = Convert.ToSingle(new System.Random().Next(1, 15)) / 10;
                __instance.shipDoorsAnimator.speed = RandomizerValues.shipDoorAnimatorSpeed;

                FastBufferWriter fastBufferShipAnimatorWriter = new FastBufferWriter(sizeof(float), Unity.Collections.Allocator.Temp, -1);
                fastBufferShipAnimatorWriter.WriteValueSafe<float>(RandomizerValues.shipDoorAnimatorSpeed);

                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesShipAnimData", fastBufferShipAnimatorWriter, NetworkDelivery.Reliable);
                fastBufferShipAnimatorWriter.Dispose();

                RandomizerModBase.mls.LogInfo("Successfully randomized player stats on level load. Synced values across clients.");
            }
            else
            {
                return;
            }
        }

        [HarmonyPatch(nameof(StartOfRound.EndOfGameClientRpc))]
        [HarmonyPrefix]
        public static void ResetPlayers()
        {
            RandomizerValues.ClearDicts(false);
            RandomizerValues.spawnedMechCount = 0;
            RandomizerValues.mapRandomizedInTerminal = false;
            RandomizerValues.spawnedMechScales.Clear();

            ClearOrphanedDicts();

            try
            {
                for (int i = 0; i < StartOfRound.Instance.allPlayerObjects.Length; i++)
                {
                    StartOfRound.Instance.allPlayerObjects[i].GetComponent<PlayerControllerB>().thisPlayerBody.localScale = RandomizerValues.defaultPlayerScale;
                    StartOfRound.Instance.allPlayerObjects[i].GetComponent<PlayerControllerB>().playerGlobalHead.localScale = RandomizerValues.defaultPlayerHeadScale;
                    StartOfRound.Instance.allPlayerObjects[i].GetComponent<PlayerControllerB>().thisPlayerModel.material.color = RandomizerValues.defaultPlayerColor;
                    StartOfRound.Instance.allPlayerObjects[i].GetComponent<PlayerControllerB>().thisPlayerModelArms.material.color = RandomizerValues.defaultPlayerColor;
                    StartOfRound.Instance.allPlayerObjects[i].GetComponent<PlayerControllerB>().thisPlayerModelLOD1.material.color = RandomizerValues.defaultPlayerColor;
                    StartOfRound.Instance.allPlayerObjects[i].GetComponent<PlayerControllerB>().thisPlayerModelLOD2.material.color = RandomizerValues.defaultPlayerColor;
                    SoundManager.Instance.SetPlayerPitch(RandomizerValues.defaultPlayerPitch, i);
                }
            }
            catch (Exception ex)
            {
                return;
            }
        }

        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        public static void AwakePatch(StartOfRound __instance)
        {
            RandomizerModBase.mls.LogInfo("allPlayerObjects length: " + __instance.allPlayerObjects.Length);
            UnityEngine.Component[] components = __instance.allPlayerObjects[0].GetComponents(typeof(UnityEngine.Component));

            RandomizerModBase.mls.LogInfo("Components in list:");
            foreach (var item in components)
            {
                RandomizerModBase.mls.LogInfo(item);
            }

            RandomizerValues.defaultPlayerMaskLayer = StartOfRound.Instance.allPlayerObjects[0].GetComponent<PlayerControllerB>().thisPlayerModel.renderingLayerMask;
            RandomizerValues.defaultPlayerScale = StartOfRound.Instance.allPlayerObjects[0].GetComponent<PlayerControllerB>().thisPlayerBody.localScale;
            RandomizerValues.defaultPlayerHeadScale = StartOfRound.Instance.allPlayerObjects[0].GetComponent<PlayerControllerB>().playerGlobalHead.localScale;
            RandomizerValues.defaultPlayerBillboardScale = StartOfRound.Instance.allPlayerObjects[0].GetComponent<PlayerControllerB>().usernameBillboard.localScale;
            RandomizerValues.defaultPlayerBillboardPos = StartOfRound.Instance.allPlayerObjects[0].GetComponent<PlayerControllerB>().usernameBillboard.position;
            RandomizerValues.defaultPlayerColor = StartOfRound.Instance.allPlayerObjects[0].GetComponent<PlayerControllerB>().thisPlayerModel.material.color;
        }

        public static void SetValuesSentByServer(ulong _, FastBufferReader reader)
        {
            if (!Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                RandomizerValues.mapRandomizedInTerminal = false;

                RandomizerModBase.mls.LogInfo("MESSAGE RECEIVED FROM SERVER!");
                float[] randValues = new float[4];
                reader.ReadValueSafe<float>(out randValues[0], default);
                reader.ReadValueSafe<float>(out randValues[1], default);
                reader.ReadValueSafe<float>(out randValues[2], default);
                reader.ReadValueSafe<float>(out randValues[3], default);

                RandomizerModBase.mls.LogInfo("VALUES RECEIVED FROM SERVER: " + randValues[0] + ", " + randValues[1] + ", " + randValues[2] + ", " + randValues[3]);
                GameNetworkManager.Instance.localPlayerController.sprintTime = randValues[0];
                GameNetworkManager.Instance.localPlayerController.health = (int)randValues[1];
                foreach (GameObject playerObj in StartOfRound.Instance.allPlayerObjects)
                {
                    playerObj.GetComponent<PlayerControllerB>().health = (int)randValues[1];
                }
                GameNetworkManager.Instance.localPlayerController.movementSpeed = randValues[2];
                GameNetworkManager.Instance.localPlayerController.sinkingSpeedMultiplier = randValues[3];
                GameNetworkManager.Instance.localPlayerController.sinkingValue = randValues[3];

                if (randValues[1] < 20)
                {
                    GameNetworkManager.Instance.localPlayerController.DamagePlayer(0, true, false, CauseOfDeath.Unknown, 0, false);
                }
            }
            else
            {
                RandomizerModBase.mls.LogInfo("Received server message but we are server. Ignoring...");
                return;
            }
        }
        
        public static void SetPlayerModelValues(ulong _, FastBufferReader reader)
        {
            if (!Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                RandomizerModBase.mls.LogInfo("Received model value array.");
                float[] modelValues = new float[4];
                for (int i = 0; i < StartOfRound.Instance.allPlayerObjects.Length; i++)
                {
                    reader.ReadValueSafe<float>(out modelValues[i]);
                    RandomizerModBase.mls.LogInfo("Read out info: " + modelValues[i]);

                    StartOfRound.Instance.allPlayerObjects[i].GetComponent<PlayerControllerB>().thisPlayerBody.localScale = RandomizerValues.defaultPlayerScale * modelValues[i];
                    StartOfRound.Instance.allPlayerObjects[i].GetComponent<PlayerControllerB>().playerGlobalHead.localScale = RandomizerValues.defaultPlayerHeadScale * modelValues[i];

                    //RandomizerModBase.mls.LogInfo("Setting player pitch: " + Mathf.Lerp(3f, 0.7f, Mathf.InverseLerp(0.5f, 1.5f, modelValues[i])) + " for player: " + i + " with size multiplier: " + modelValues[i]);
                    //SoundManager.Instance.SetPlayerPitch(Mathf.Lerp(3f, 0.7f, Mathf.InverseLerp(0.5f, 1.5f, modelValues[i])), i);


                    RandomizerModBase.mls.LogInfo("Setting player pitch: " + (modelValues[i] <= 1 ? Mathf.Lerp(1f, 15f, 1-(modelValues[i] - 0.5f) * 2) : Mathf.Lerp(0.5f, 1f, 1-(modelValues[i] - 1f) * 2)) + " for player: " + i + " with size multiplier: " + modelValues[i] + " isServer? " + Unity.Netcode.NetworkManager.Singleton.IsServer);
                    SoundManager.Instance.SetPlayerPitch(modelValues[i] <= 1 ? Mathf.Lerp(1f, 15f, 1-(modelValues[i] - 0.5f) * 2) : Mathf.Lerp(0.5f, 1f, 1-(modelValues[i] - 1f) * 2), i);
                }
            }
        }

        public static void RandomizeQuotaVariables()
        {
            RandomizerModBase.mls.LogInfo("Randomized quota variables.");

            TimeOfDay.Instance.quotaVariables.baseIncrease = 700f;
            TimeOfDay.Instance.quotaVariables.increaseSteepness = new System.Random().Next(20, 50) / 10;
            TimeOfDay.Instance.quotaVariables.randomizerMultiplier = new System.Random().Next(10, 20) / 10;
        }

        public static int GenerateNewDeadline()
        {
            RandomizerModBase.mls.LogInfo("Generating new deadline...");
            return new System.Random().Next(2, 6) * 1080;
        }

        public static void SendQuotaValues(int deadLine, int quota)
        {
            FastBufferWriter fastBufferWriter = new FastBufferWriter(sizeof(int) * 2, Unity.Collections.Allocator.Temp, -1);
            fastBufferWriter.WriteValueSafe<int>(deadLine);
            fastBufferWriter.WriteValueSafe<int>(quota);

            Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesQuotaData", fastBufferWriter, NetworkDelivery.Reliable);
            fastBufferWriter.Dispose();
        }

        public static void SyncQuotaValuesWithServer(ulong _, FastBufferReader reader)
        {
            if (!Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                reader.ReadValueSafe<int>(out RandomizerValues.deadlineRand);
                reader.ReadValueSafe<int>(out RandomizerValues.quotaRand);
                RandomizerModBase.mls.LogInfo("Received deadline: " + RandomizerValues.deadlineRand + " quota: " + RandomizerValues.quotaRand);
                
                TimeOfDay.Instance.timeUntilDeadline = RandomizerValues.deadlineRand;
                int daysUntilDeadline = (int)Mathf.Floor(TimeOfDay.Instance.timeUntilDeadline / TimeOfDay.Instance.totalTime);
                StartOfRound.Instance.deadlineMonitorText.text = string.Format("DEADLINE:\n{0} Days", daysUntilDeadline);

                if (RandomizerValues.quotaRand == 0)
                {
                    return;
                }
                TimeOfDay.Instance.profitQuota = RandomizerValues.quotaRand;
                StartOfRound.Instance.profitQuotaMonitorText.text = string.Format("PROFIT QUOTA:\n${0} / ${1}", TimeOfDay.Instance.quotaFulfilled, TimeOfDay.Instance.profitQuota);
            }
        }

        [HarmonyPatch("OnShipLandedMiscEvents")]
        [HarmonyPostfix]
        public static void ShowNewQuotaOnLevelArrival()
        {
            RandomizerModBase.mls.LogInfo("Showing first time new deadline!!!!");

            if (RandomizerValues.firstTimeShow)
            {
                HUDManager.Instance.DisplayDaysLeft(RandomizerValues.deadlineRand / 1080);
                RandomizerValues.firstTimeShow = false;

                FastBufferWriter fastBufferFirstTimeShowWriter = new FastBufferWriter(sizeof(int), Unity.Collections.Allocator.Temp, -1);
                fastBufferFirstTimeShowWriter.WriteValueSafe<int>(RandomizerValues.deadlineRand / 1080);

                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesFirstTimeShow", fastBufferFirstTimeShowWriter, NetworkDelivery.Reliable);
            }
        }

        public static void ShowFirstTimeDeadlineClient(ulong _, FastBufferReader reader)
        {
            if (!Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                int temp;
                reader.ReadValueSafe<int>(out temp);
                HUDManager.Instance.DisplayDaysLeft(temp);

                if (!RandomizerValues.audioDictLoaded)
                {
                    foreach (AudioClip clip in Resources.FindObjectsOfTypeAll<AudioClip>())
                    {
                        if (clip == null) continue;

                        if (!RandomizerValues.audioDict.ContainsKey(clip.name))
                        {
                            RandomizerValues.audioDict.Add(clip.name, clip);
                        }
                    }
                    RandomizerValues.audioDictLoaded = true;
                }

                foreach (KeyValuePair<string, AudioClip> pair in RandomizerValues.audioDict)
                {
                    RandomizerModBase.mls.LogInfo(pair.Key);
                }

                RandomizerModBase.mls.LogError("DICT COUNT: " + RandomizerValues.audioDict.Count);
            }
        }

        public static void SetShipAnimatorSpeed(ulong _, FastBufferReader reader)
        {
            if (!Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                reader.ReadValueSafe<float>(out RandomizerValues.shipDoorAnimatorSpeed);
                RandomizerModBase.mls.LogInfo("Received ship door animator speed: " + RandomizerValues.shipDoorAnimatorSpeed);
                StartOfRound.Instance.shipDoorsAnimator.speed = RandomizerValues.shipDoorAnimatorSpeed;
            }
        }

        private static void ClearOrphanedDicts()
        {
            List<ulong> dictsToRemove = new List<ulong>();

            foreach (ulong id in RandomizerValues.jetpackPropertiesDict.Keys)
            {
                if (!Unity.Netcode.NetworkManager.Singleton.SpawnManager.SpawnedObjects.ContainsKey(id) || !Unity.Netcode.NetworkManager.Singleton.SpawnManager.SpawnedObjects[id].GetComponentInChildren<GrabbableObject>().isInShipRoom)
                {
                    dictsToRemove.Add(id);
                }
            }

            foreach (ulong id in dictsToRemove)
            {
                RandomizerValues.jetpackPropertiesDict.Remove(id);
            }
            dictsToRemove.Clear();

            foreach (ulong id in RandomizerValues.knifeDamageDict.Keys)
            {
                if (!Unity.Netcode.NetworkManager.Singleton.SpawnManager.SpawnedObjects.ContainsKey(id) || !Unity.Netcode.NetworkManager.Singleton.SpawnManager.SpawnedObjects[id].GetComponentInChildren<GrabbableObject>().isInShipRoom)
                {
                    dictsToRemove.Add(id);
                }
            }

            foreach (ulong id in dictsToRemove)
            {
                RandomizerValues.knifeDamageDict.Remove(id);
            }
            dictsToRemove.Clear();

            foreach (ulong id in RandomizerValues.shovelDamageDict.Keys)
            {
                if (!Unity.Netcode.NetworkManager.Singleton.SpawnManager.SpawnedObjects.ContainsKey(id) || !Unity.Netcode.NetworkManager.Singleton.SpawnManager.SpawnedObjects[id].GetComponentInChildren<GrabbableObject>().isInShipRoom)
                {
                    dictsToRemove.Add(id);
                }
            }

            foreach (ulong id in dictsToRemove)
            {
                RandomizerValues.shovelDamageDict.Remove(id);
            }
            dictsToRemove.Clear();

            foreach (ulong id in RandomizerValues.boomboxPitchDict.Keys)
            {
                if (!Unity.Netcode.NetworkManager.Singleton.SpawnManager.SpawnedObjects.ContainsKey(id) || !Unity.Netcode.NetworkManager.Singleton.SpawnManager.SpawnedObjects[id].GetComponentInChildren<GrabbableObject>().isInShipRoom)
                {
                    dictsToRemove.Add(id);
                }
            }

            foreach (ulong id in dictsToRemove)
            {
                RandomizerValues.boomboxPitchDict.Remove(id);
            }
            dictsToRemove.Clear();
        }

        public static void LoadAudioDict(ulong _, FastBufferReader __)
        {
            foreach (AudioClip clip in Resources.FindObjectsOfTypeAll<AudioClip>())
            {
                if (clip == null) continue;

                if (!RandomizerValues.audioDict.ContainsKey(clip.name))
                {
                    RandomizerValues.audioDict.Add(clip.name, clip);
                }
            }
            RandomizerValues.audioDictLoaded = true;


            foreach (KeyValuePair<string, AudioClip> pair in RandomizerValues.audioDict)
            {
                RandomizerModBase.mls.LogInfo(pair.Key);
            }

            RandomizerModBase.mls.LogError("DICT COUNT: " + RandomizerValues.audioDict.Count);
            RandomizerModBase.mls.LogInfo("Audio dictionary loaded.");
        }

        [HarmonyPatch("ResetMiscValues")]
        [HarmonyPrefix]
        public static bool RunNeeded(StartOfRound __instance)
        {
            if (RandomizerValues.unblockResetRun)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //[HarmonyPatch(nameof(StartOfRound.ChangeLevelServerRpc))]
        //[HarmonyPrefix]
        //public static bool ChangeLevelOverride(StartOfRound __instance)
        //{
        //    if (RandomizerValues.mapRandomizedInTerminal && StartOfRound.Instance.inShipPhase)
        //    {
        //        HUDManager.Instance.AddTextToChatOnServer("<color=red>You have already randomized the map, you can't route to a new planet until you go down.</color>", -1);
        //        RandomizerModBase.mls.LogInfo("RANDOMIZED STATE: " + RandomizerValues.mapRandomizedInTerminal);
        //    }
        //    return !RandomizerValues.mapRandomizedInTerminal;
        //}

        //SET PLAYER PITCH W/ NETWORKING, DONT TOUCH IT CUZ IT WORKS

        //public static void SetPitchDataSentByServer(ulong _, FastBufferReader reader)
        //{
        //    if (!Unity.Netcode.NetworkManager.Singleton.IsServer)
        //    {
        //        for (int i = 0; i < 4; i++)
        //        {
        //            float pitch;
        //            reader.ReadValueSafe<float>(out pitch);
        //            SoundManager.Instance.SetPlayerPitch(pitch, i);
        //        }
        //    }
        //}

        //SET PLAYER PITCH W/ NETWORKING, DONT TOUCH IT CUZ IT WORKS

        //TBD IF INCLUDED IN FINAL VERSION

        //[HarmonyPatch("Update")]
        //[HarmonyPostfix]
        //public static void PowerSurgeShipRandomly(StartOfRound __instance)
        //{
        //    if (!StartOfRound.Instance.inShipPhase && RoundManager.Instance.currentLevel.currentWeather != LevelWeatherType.Stormy)
        //    {
        //        if (new System.Random().Next(1, 10001) == 10000)
        //        {
        //            StartOfRound.Instance.PowerSurgeShip();
        //        }
        //    }
        //}
    }
}
