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

        static float sprintRand;
        static int healthRand;
        static float movementSpeedRand;
        static float sinkMultiplierRand;
        static int weatherIndex;

        [HarmonyPatch(nameof(StartOfRound.StartGame))]
        [HarmonyPostfix]
        public static void RandomizePlayerStatsOnLevelLoadServer(StartOfRound __instance)
        {
            if (Unity.Netcode.NetworkManager.Singleton.IsHost)
            {
                //Generate random values
                sprintRand = Convert.ToSingle(new System.Random().Next(1, 101)) / 10;
                healthRand = new System.Random().Next(1, 101);
                movementSpeedRand = new System.Random().Next(30, 101) / 10;
                sinkMultiplierRand = new System.Random().Next(100, 10000) / 10;

                //Generate random weather values
                LevelWeatherType[] weatherTypes = Enum.GetValues(typeof(LevelWeatherType)) as LevelWeatherType[];
                weatherIndex = new System.Random().Next(0, weatherTypes.Length - 1);

                FastBufferWriter fastBufferWeatherWriter = new FastBufferWriter(sizeof(int), Unity.Collections.Allocator.Temp, -1);
                fastBufferWeatherWriter.WriteValueSafe<int>(weatherIndex);

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
                    StartOfRound.Instance.allPlayerObjects[i].GetComponent<PlayerControllerB>().thisPlayerBody.localScale = StartOfRound.Instance.allPlayerObjects[i].GetComponent<PlayerControllerB>().thisPlayerBody.localScale * modelValues[i];
                    StartOfRound.Instance.allPlayerObjects[i].GetComponent<PlayerControllerB>().thisPlayerModel.renderingLayerMask = StartOfRound.Instance.allPlayerObjects[i].GetComponent<PlayerControllerB>().thisPlayerModel.renderingLayerMask * (uint)modelValues[i];
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
        public static void Test(StartOfRound __instance)
        {
            RandomizerModBase.mls.LogInfo("allPlayerObjects length: " + __instance.allPlayerObjects.Length);
            UnityEngine.Component[] components = __instance.allPlayerObjects[0].GetComponents(typeof(UnityEngine.Component));

            RandomizerModBase.mls.LogInfo("Components in list:");
            foreach (var item in components)
            {
                RandomizerModBase.mls.LogInfo(item);
            }

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

                    StartOfRound.Instance.allPlayerObjects[i].GetComponent<PlayerControllerB>().thisPlayerBody.localScale = StartOfRound.Instance.allPlayerObjects[i].GetComponent<PlayerControllerB>().thisPlayerBody.localScale * modelValues[i];
                    StartOfRound.Instance.allPlayerObjects[i].GetComponent<PlayerControllerB>().thisPlayerModel.renderingLayerMask = (uint)modelValues[i];
                }
            }
        }

        public static int GetWeatherIndex
        {
            get { return weatherIndex; }
            set { weatherIndex = value; }
        }
    }
}
