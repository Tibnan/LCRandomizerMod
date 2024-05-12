using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Threading.Tasks;
using Unity.Netcode;
using System.Runtime.CompilerServices;
using System.CodeDom.Compiler;
using UnityEngine.Assertions.Must;

namespace LCRandomizerMod.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRoundPatch
    {
        [HarmonyPatch(nameof(StartOfRound.StartGame))]
        [HarmonyPostfix]
        public static void RandomizePlayerStatsOnLevelLoadServer(StartOfRound __instance)
        {
            RandomizerValues.firstTimeShow = TimeOfDay.Instance.profitQuota == 130;
            RandomizerModBase.mls.LogInfo("First time? " + RandomizerValues.firstTimeShow);

            if (Unity.Netcode.NetworkManager.Singleton.IsHost)
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

                RandomizerValues.ClearLists();

                //Generate random values
                RandomizerValues.sprintRand = Convert.ToSingle(new System.Random().Next(1, 101)) / 10;
                RandomizerValues.healthRand = new System.Random().Next(1, 101);
                RandomizerValues.movementSpeedRand = new System.Random().Next(30, 101) / 10;
                RandomizerValues.sinkMultiplierRand = new System.Random().Next(100, 10000) / 10;

                //Generate random deadline and quota values on new save load and sync with clients
                if (TimeOfDay.Instance.profitQuota == 130)
                {
                    RandomizerValues.deadlineRand = 1080; /*GenerateNewDeadline();*/ //For testing
                    RandomizerModBase.mls.LogInfo("New deadline time: " + RandomizerValues.deadlineRand +  " (" + RandomizerValues.deadlineRand / 1080 + ") days");
                    RandomizerValues.quotaRand = 1; /*new System.Random().Next(500, 20000);*/ //For testing
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
                RandomizerValues.randomizedWeatherIdx = new System.Random().Next(0, weatherTypes.Length);
                RandomizerValues.randomizedWeatherIdx = 5; //For testing

                RandomizerModBase.mls.LogInfo("Randomized weather index: " + RandomizerValues.randomizedWeatherIdx);

                FastBufferWriter fastBufferWeatherWriter = new FastBufferWriter(sizeof(int), Unity.Collections.Allocator.Temp, -1);
                fastBufferWeatherWriter.WriteValueSafe<int>(RandomizerValues.randomizedWeatherIdx);

                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceiveWeatherData", fastBufferWeatherWriter, NetworkDelivery.Reliable);
                fastBufferWeatherWriter.Dispose();

                //Set values on server
                GameNetworkManager.Instance.localPlayerController.sprintTime = RandomizerValues.sprintRand;
                RandomizerModBase.mls.LogInfo("Set sprint time to: " + RandomizerValues.sprintRand);
                GameNetworkManager.Instance.localPlayerController.health = RandomizerValues.healthRand;
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
                    modelValues[i] = Convert.ToSingle(new System.Random().Next(5, 15)) / 10;
                    fastBufferModelValueWriter.WriteValueSafe<float>(modelValues[i]);
                }


                for (int i = 0; i < Unity.Netcode.NetworkManager.Singleton.ConnectedClientsList.Count; i++)
                {
                    StartOfRound.Instance.allPlayerObjects[i].GetComponent<PlayerControllerB>().thisPlayerBody.localScale = RandomizerValues.defaultPlayerScale * modelValues[i];
                    StartOfRound.Instance.allPlayerObjects[i].GetComponent<PlayerControllerB>().thisPlayerModel.renderingLayerMask = RandomizerValues.defaultPlayerMaskLayer * (uint)modelValues[i];
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
                    
                    RandomizerModBase.mls.LogInfo("Setting player pitch: " + (modelValues[i] <= 1 ? Mathf.Lerp(1f, 15f, 1-(modelValues[i] - 0.5f) * 2) : Mathf.Lerp(0.5f, 1f, 1-(modelValues[i] - 1f) * 2)) + " for player: " + i + " with size multiplier: " + modelValues[i] + " isServer? " + Unity.Netcode.NetworkManager.Singleton.IsServer);
                    SoundManager.Instance.SetPlayerPitch(modelValues[i] <= 1 ? Mathf.Lerp(1f, 15f, 1-(modelValues[i] - 0.5f) * 2) : Mathf.Lerp(0.5f, 1f, 1-(modelValues[i] - 1f) * 2), i);
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
            for (int i = 0; i < StartOfRound.Instance.allPlayerObjects.Length; i++)
            {
                StartOfRound.Instance.allPlayerObjects[i].GetComponent<PlayerControllerB>().thisPlayerBody.localScale = RandomizerValues.defaultPlayerScale;
                SoundManager.Instance.SetPlayerPitch(RandomizerValues.defaultPlayerPitch, i);
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

            RandomizerValues.defaultPlayerMaskLayer = __instance.allPlayerObjects[0].GetComponent<PlayerControllerB>().thisPlayerModel.renderingLayerMask;
            RandomizerValues.defaultPlayerScale = StartOfRound.Instance.allPlayerObjects[0].GetComponent<PlayerControllerB>().thisPlayerBody.localScale;
        }

        public static void SetValuesSentByServer(ulong _, FastBufferReader reader)
        {
            if (!Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                RandomizerModBase.mls.LogInfo("MESSAGE RECEIVED FROM SERVER!");
                float[] randValues = new float[4];
                reader.ReadValueSafe<float>(out randValues[0], default);
                reader.ReadValueSafe<float>(out randValues[1], default);
                reader.ReadValueSafe<float>(out randValues[2], default);
                reader.ReadValueSafe<float>(out randValues[3], default);

                RandomizerModBase.mls.LogInfo("VALUES RECEIVED FROM SERVER: " + randValues[0] + ", " + randValues[1] + ", " + randValues[2] + ", " + randValues[3]);
                GameNetworkManager.Instance.localPlayerController.sprintTime = randValues[0];
                GameNetworkManager.Instance.localPlayerController.health = (int)randValues[1];
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
                    StartOfRound.Instance.allPlayerObjects[i].GetComponent<PlayerControllerB>().thisPlayerModel.renderingLayerMask = RandomizerValues.defaultPlayerMaskLayer * (uint)modelValues[i];


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
        //        if (new System.Random().Next(1, 10001) == 10000) StartOfRound.Instance.PowerSurgeShip();
        //    }
        //}
    }
}
