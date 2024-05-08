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

namespace LCRandomizerMod.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRoundPatch
    {
        static Vector3 defaultPlayerScale;
        static uint defaultPlayerMaskLayer;

        static float sprintRand;
        static int healthRand;
        static float movementSpeedRand;
        static float sinkMultiplierRand;

        static int quotaRand;
        static int deadlineRand;

        static bool firstTimeShow;

        [HarmonyPatch(nameof(StartOfRound.StartGame))]
        [HarmonyPostfix]
        public static void RandomizePlayerStatsOnLevelLoadServer(StartOfRound __instance)
        {
            firstTimeShow = TimeOfDay.Instance.profitQuota == 130;
            RandomizerModBase.mls.LogInfo("First time? " + firstTimeShow);

            if (Unity.Netcode.NetworkManager.Singleton.IsHost)
            {
                //Generate random values
                sprintRand = Convert.ToSingle(new System.Random().Next(1, 101)) / 10;
                healthRand = new System.Random().Next(1, 101);
                movementSpeedRand = new System.Random().Next(30, 101) / 10;
                sinkMultiplierRand = new System.Random().Next(100, 10000) / 10;

                //Generate random deadline and quota values on new save load and sync with clients
                if (TimeOfDay.Instance.profitQuota == 130)
                {
                    deadlineRand = 1080; /*GenerateNewDeadline();*/ //For testing
                    RandomizerModBase.mls.LogInfo("New deadline time: " + deadlineRand +  " (" + deadlineRand / 1080 + ") days");
                    quotaRand = 1; /*new System.Random().Next(500, 20000);*/ //For testing
                    RandomizerModBase.mls.LogInfo("New quota: " + quotaRand);

                    RandomizeQuotaVariables();

                    SendQuotaValues(deadlineRand, quotaRand);

                    TimeOfDay.Instance.timeUntilDeadline = deadlineRand;
                    int daysUntilDeadline = (int)Mathf.Floor(TimeOfDay.Instance.timeUntilDeadline / TimeOfDay.Instance.totalTime);
                    StartOfRound.Instance.deadlineMonitorText.text = string.Format("DEADLINE:\n{0} Days", daysUntilDeadline);

                    TimeOfDay.Instance.profitQuota = quotaRand;
                    StartOfRound.Instance.profitQuotaMonitorText.text = string.Format("PROFIT QUOTA:\n${0} / ${1}", TimeOfDay.Instance.quotaFulfilled, TimeOfDay.Instance.profitQuota);
                }

                //Generate random weather values
                LevelWeatherType[] weatherTypes = Enum.GetValues(typeof(LevelWeatherType)) as LevelWeatherType[];
                RoundManagerPatch.randomizedWeatherIdx = new System.Random().Next(0, weatherTypes.Length);
                RoundManagerPatch.randomizedWeatherIdx = 5; //For testing

                RandomizerModBase.mls.LogInfo("Randomized weather index: " + RoundManagerPatch.randomizedWeatherIdx);

                FastBufferWriter fastBufferWeatherWriter = new FastBufferWriter(sizeof(int), Unity.Collections.Allocator.Temp, -1);
                fastBufferWeatherWriter.WriteValueSafe<int>(RoundManagerPatch.randomizedWeatherIdx);

                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceiveWeatherData", fastBufferWeatherWriter, NetworkDelivery.Reliable);
                fastBufferWeatherWriter.Dispose();

                //Set values on server
                GameNetworkManager.Instance.localPlayerController.sprintTime = sprintRand;
                RandomizerModBase.mls.LogInfo("Set sprint time to: " + sprintRand);
                GameNetworkManager.Instance.localPlayerController.health = healthRand;
                RandomizerModBase.mls.LogInfo("Set health to: " + healthRand);
                GameNetworkManager.Instance.localPlayerController.movementSpeed = movementSpeedRand;
                RandomizerModBase.mls.LogInfo("Set movement speed to: " + movementSpeedRand);
                GameNetworkManager.Instance.localPlayerController.sinkingSpeedMultiplier = sinkMultiplierRand;
                GameNetworkManager.Instance.localPlayerController.sinkingValue = sinkMultiplierRand;
                RandomizerModBase.mls.LogInfo("Set sinking values to:  " + sinkMultiplierRand);

                RandomizerModBase.mls.LogInfo("Sending values to clients...");
                float[] randValues = new float[] { sprintRand, healthRand, movementSpeedRand, sinkMultiplierRand};

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
                    modelValues[i] = Convert.ToSingle(new System.Random().Next(1, 15)) / 10;
                    fastBufferModelValueWriter.WriteValueSafe<float>(modelValues[i]);
                }


                for (int i = 0; i < Unity.Netcode.NetworkManager.Singleton.ConnectedClientsList.Count; i++)
                {
                    StartOfRound.Instance.allPlayerObjects[i].GetComponent<PlayerControllerB>().thisPlayerBody.localScale = defaultPlayerScale * modelValues[i];
                    StartOfRound.Instance.allPlayerObjects[i].GetComponent<PlayerControllerB>().thisPlayerModel.renderingLayerMask = defaultPlayerMaskLayer * (uint)modelValues[i];
                }

                RandomizerModBase.mls.LogInfo("Sending player model values...");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "SetPlayerModelValues", fastBufferModelValueWriter, NetworkDelivery.Reliable);
                fastBufferModelValueWriter.Dispose();

                if (healthRand < 20)
                {
                    GameNetworkManager.Instance.localPlayerController.DamagePlayer(0, true, false, CauseOfDeath.Unknown, 0, false);
                }

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
                StartOfRound.Instance.allPlayerObjects[i].GetComponent<PlayerControllerB>().thisPlayerBody.localScale = defaultPlayerScale;
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

            defaultPlayerMaskLayer = __instance.allPlayerObjects[0].GetComponent<PlayerControllerB>().thisPlayerModel.renderingLayerMask;
            defaultPlayerScale = StartOfRound.Instance.allPlayerObjects[0].GetComponent<PlayerControllerB>().thisPlayerBody.localScale;
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

                    StartOfRound.Instance.allPlayerObjects[i].GetComponent<PlayerControllerB>().thisPlayerBody.localScale = defaultPlayerScale * modelValues[i];
                    StartOfRound.Instance.allPlayerObjects[i].GetComponent<PlayerControllerB>().thisPlayerModel.renderingLayerMask = defaultPlayerMaskLayer * (uint)modelValues[i];
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
                int deadline;
                int quota;
                reader.ReadValueSafe<int>(out deadlineRand); //deadline
                reader.ReadValueSafe<int>(out quotaRand); //quota
                RandomizerModBase.mls.LogInfo("Received deadline: " + deadlineRand + " quota: " + quotaRand);
                
                TimeOfDay.Instance.timeUntilDeadline = deadlineRand;
                int daysUntilDeadline = (int)Mathf.Floor(TimeOfDay.Instance.timeUntilDeadline / TimeOfDay.Instance.totalTime);
                StartOfRound.Instance.deadlineMonitorText.text = string.Format("DEADLINE:\n{0} Days", daysUntilDeadline);

                if (quotaRand == 0)
                {
                    return;
                }
                TimeOfDay.Instance.profitQuota = quotaRand;
                StartOfRound.Instance.profitQuotaMonitorText.text = string.Format("PROFIT QUOTA:\n${0} / ${1}", TimeOfDay.Instance.quotaFulfilled, TimeOfDay.Instance.profitQuota);
            }
        }

        [HarmonyPatch("OnShipLandedMiscEvents")]
        [HarmonyPostfix]
        public static void ShowNewQuotaOnLevelArrival()
        {
            RandomizerModBase.mls.LogInfo("Showing first time new deadline!!!!");

            if (firstTimeShow)
            {
                HUDManager.Instance.DisplayDaysLeft(deadlineRand / 1080);
                firstTimeShow = false;

                FastBufferWriter fastBufferFirstTimeShowWriter = new FastBufferWriter(sizeof(int), Unity.Collections.Allocator.Temp, -1);
                fastBufferFirstTimeShowWriter.WriteValueSafe<int>(deadlineRand / 1080);

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
    }
}
