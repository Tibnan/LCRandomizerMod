using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace LCRandomizerMod.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        [HarmonyPatch(nameof(PlayerControllerB.ConnectClientToPlayerObject))]
        [HarmonyPostfix]
        public static void RegisterMessageHandlers(PlayerControllerB __instance)
        {
            if (Unity.Netcode.NetworkManager.Singleton.IsHost)
            {
                RandomizerModBase.mls.LogInfo("Registering host random value message handler: " + "SetValuesSentByServer");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "SetValuesSentByServer", new CustomMessagingManager.HandleNamedMessageDelegate(StartOfRoundPatch.SetValuesSentByServer));
                RandomizerModBase.mls.LogInfo("Registering scrap value handlers: " + "SetScrapValue, " + "SetValueOn");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "SetScrapValueTo", new CustomMessagingManager.HandleNamedMessageDelegate(GrabbableObjectPatch.SetScrapValueTo));
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "SetValueOn", new CustomMessagingManager.HandleNamedMessageDelegate(GrabbableObjectPatch.SetValueOn));
                RandomizerModBase.mls.LogInfo("Registering model value handler: " + "SetPlayerModelValues");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "SetPlayerModelValues", new CustomMessagingManager.HandleNamedMessageDelegate(StartOfRoundPatch.SetPlayerModelValues));
                RandomizerModBase.mls.LogInfo("Registering weather data handler: " + "ClientReceiveWeatherData");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceiveWeatherData", new CustomMessagingManager.HandleNamedMessageDelegate(RoundManagerPatch.SetReceivedWeatherData));
            }
            else
            {
                RandomizerModBase.mls.LogInfo("Registering client random value message handler: " + "SetValuesSentByServer");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "SetValuesSentByServer", new CustomMessagingManager.HandleNamedMessageDelegate(StartOfRoundPatch.SetValuesSentByServer));
                RandomizerModBase.mls.LogInfo("Registering scrap value handlers: " + "SetScrapValue, " + "SetValueOn");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "SetScrapValueTo", new CustomMessagingManager.HandleNamedMessageDelegate(GrabbableObjectPatch.SetScrapValueTo));
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "SetValueOn", new CustomMessagingManager.HandleNamedMessageDelegate(GrabbableObjectPatch.SetValueOn));
                RandomizerModBase.mls.LogInfo("Registering model value handler: " + "SetPlayerModelValues");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "SetPlayerModelValues", new CustomMessagingManager.HandleNamedMessageDelegate(StartOfRoundPatch.SetPlayerModelValues));
                RandomizerModBase.mls.LogInfo("Registering weather data handler: " + "ClientReceiveWeatherData");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceiveWeatherData", new CustomMessagingManager.HandleNamedMessageDelegate(RoundManagerPatch.SetReceivedWeatherData));
            }
        }
    }
}
