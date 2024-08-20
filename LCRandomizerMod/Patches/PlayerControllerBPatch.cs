using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;

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
                RandomizerModBase.mls.LogInfo("Registering quota data handler: " + "ClientReceiveQuotaData");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesQuotaData", new CustomMessagingManager.HandleNamedMessageDelegate(StartOfRoundPatch.SyncQuotaValuesWithServer));
                RandomizerModBase.mls.LogInfo("Registering factory data handler: " + "ClientReceivesFactoryData");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesFactoryData", new CustomMessagingManager.HandleNamedMessageDelegate(RoundManagerPatch.SetReceivedFactoryData));
                RandomizerModBase.mls.LogInfo("Registering first time deadline show handler: " + "ClientReceivesFirstTimeShow");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesFirstTimeShow", new CustomMessagingManager.HandleNamedMessageDelegate(StartOfRoundPatch.ShowFirstTimeDeadlineClient));
                RandomizerModBase.mls.LogInfo("Registering dog handler: " + "ClientReceivesDogStats");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesDogStats", new CustomMessagingManager.HandleNamedMessageDelegate(MouthDogAIPatch.SetDogValuesSentByServer));
                RandomizerModBase.mls.LogInfo("Registering spider handler: " + "ClientReceivesSpiderData ");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesSpiderData", new CustomMessagingManager.HandleNamedMessageDelegate(SandSpiderAIPatch.SetSpiderDataSentByServer));
                RandomizerModBase.mls.LogInfo("Registering ship animator handlers: " + "ClientReceivesShipAnimData");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesShipAnimData", new CustomMessagingManager.HandleNamedMessageDelegate(StartOfRoundPatch.SetShipAnimatorSpeed));
                RandomizerModBase.mls.LogInfo("Registering giant handler: " + "ClientReceivesGiantData");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesGiantData", new CustomMessagingManager.HandleNamedMessageDelegate(ForestGiantAIPatch.SetGiantValuesSentByServer));
                RandomizerModBase.mls.LogInfo("Registering jetpack handler: " + "ClientReceivesJetpackData");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesJetpackData", new CustomMessagingManager.HandleNamedMessageDelegate(JetpackItemPatch.SetJetpackStatsSentByServer));
                RandomizerModBase.mls.LogInfo("Registering hoarder bug handler: " + "ClientReceivesHoarderBugData");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesHoarderBugData", new CustomMessagingManager.HandleNamedMessageDelegate(HoarderBugAIPatch.SetBugData));
                RandomizerModBase.mls.LogInfo("Registering butler handler: " + "ClientReceivesButlerData");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesButlerData", new CustomMessagingManager.HandleNamedMessageDelegate(ButlerAIPatch.SetButlerData));
                RandomizerModBase.mls.LogInfo("Registering jester handler: " + "ClientReceivesJesterData");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesJesterData", new CustomMessagingManager.HandleNamedMessageDelegate(JesterAIPatch.SetJesterData));
                RandomizerModBase.mls.LogInfo("Registering baboon hawk handler: " + "ClientReceivesBaboonData");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesBaboonData", new CustomMessagingManager.HandleNamedMessageDelegate(BaboonBirdAIPatch.SetBaboonData));
                RandomizerModBase.mls.LogInfo("Registering blob handler: " + "ClientReceivesBlobData");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesBlobData", new CustomMessagingManager.HandleNamedMessageDelegate(BlobAIPatch.SetBlobData));
                RandomizerModBase.mls.LogInfo("Registering crawler handler: " + "ClientReceivesCrawlerData");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesCrawlerData", new CustomMessagingManager.HandleNamedMessageDelegate(CrawlerAIPatch.SetCrawlerData));
                RandomizerModBase.mls.LogInfo("Registering dress girl handler: " + "ClientReceivesDressGirlData");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesDressGirlData", new CustomMessagingManager.HandleNamedMessageDelegate(DressGirlAIPatch.SetGirlData));
                RandomizerModBase.mls.LogInfo("Registering mech handlers: " + "ClientReceivesRadMechData" + " ClientReceivesMechScaleArray");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesRadMechData", new CustomMessagingManager.HandleNamedMessageDelegate(RadMechAIPatch.SetMechData));
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesMechScaleArray", new CustomMessagingManager.HandleNamedMessageDelegate(RadMechAIPatch.SetMechSpawnerScale));
                RandomizerModBase.mls.LogInfo("Registering nutcracker handler: " + "ClientReceivesNutcrackerData");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesNutcrackerData", new CustomMessagingManager.HandleNamedMessageDelegate(NutcrackerAIPatch.SetNutcrackerData));
                RandomizerModBase.mls.LogInfo("Registering flowerman handler: " + "ClientReceivesFlowermanData");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesFlowermanData", new CustomMessagingManager.HandleNamedMessageDelegate(FlowermanAIPatch.SetFlowermanStats));
                RandomizerModBase.mls.LogInfo("Registering puffer handler: " + "ClientReceivesPufferData");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesPufferData", new CustomMessagingManager.HandleNamedMessageDelegate(PufferAIPatch.SetPufferData));
                RandomizerModBase.mls.LogInfo("Registering centipede handler: " + "ClientReceivesCentipedeData");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesCentipedeData", new CustomMessagingManager.HandleNamedMessageDelegate(CentipedeAIPatch.SetCentipedeData));
                RandomizerModBase.mls.LogInfo("Registering flower snake handler: " + "ClientReceivesFlowerSnakeData");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesFlowerSnakeData", new CustomMessagingManager.HandleNamedMessageDelegate(FlowerSnakeEnemyPatch.SetFlowerSnakeStats));
                RandomizerModBase.mls.LogInfo("Registering spring man handler: " + "ClientReceivesSpringManData");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesSpringManData", new CustomMessagingManager.HandleNamedMessageDelegate(SpringManAIPatch.SetSpringManStats));
                RandomizerModBase.mls.LogInfo("Registering doublewing handler: " + "ClientReceivesDoublewingData");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesDoublewingData", new CustomMessagingManager.HandleNamedMessageDelegate(DoublewingAIPatch.SetDoublewingData));
                RandomizerModBase.mls.LogInfo("Registering bee handler: " + "ClientReceivesBeeData");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesBeeData", new CustomMessagingManager.HandleNamedMessageDelegate(RedLocustBeesPatch.SetBeeStats));
                RandomizerModBase.mls.LogInfo("Registering terminal switch handlers: " + "TerminalRandomizationUsed" + "ServerInvokeTerminalSwitch " + "ServerReceivesReviveRequest ");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "TerminalRandomizationUsed", new CustomMessagingManager.HandleNamedMessageDelegate(TerminalPatch.SwitchTerminalMode));
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ServerInvokeTerminalSwitch", new CustomMessagingManager.HandleNamedMessageDelegate(TerminalPatch.SendTerminalSwitchToClients));
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ServerReceivesReviveRequest", new CustomMessagingManager.HandleNamedMessageDelegate(TerminalPatch.ServerProcessReviveRequest));
                RandomizerModBase.mls.LogInfo("Registering mine handlers: " + "ClientReceivesMineData " + "ClientHandlePlayerExploded");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesMineData", new CustomMessagingManager.HandleNamedMessageDelegate(LandminePatch.SetMineSizeClient));
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientHandlePlayerExploded", new CustomMessagingManager.HandleNamedMessageDelegate(PlayerControllerBPatch.KillLocalPlayerByExp));
                RandomizerModBase.mls.LogInfo("Registering knife and shovel damage handlers: " + "ClientReceivesKnifeData");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesKnifeData", new CustomMessagingManager.HandleNamedMessageDelegate(KnifeItemPatch.SetKnifeData));
                RandomizerModBase.mls.LogInfo("Registering server item data request handler: " + "ServerReceivesItemDataRequest");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ServerReceivesItemDataRequest", new CustomMessagingManager.HandleNamedMessageDelegate(PlayerControllerBPatch.ServerBeginItemDataTransfer));
                RandomizerModBase.mls.LogInfo("Registering shovel damage handler: " + "ClientReceivesShovelData");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesShovelData", new CustomMessagingManager.HandleNamedMessageDelegate(ShovelPatch.SetShovelData));
                RandomizerModBase.mls.LogInfo("Registering turret handler: " + "ClientReceivesTurretData");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesTurretData", new CustomMessagingManager.HandleNamedMessageDelegate(TurretPatch.SetTurretStats));
                RandomizerModBase.mls.LogInfo("Registering boombox handlers: " + "ClientReceivesBoomboxPitch" + " ClientReceivesBoomboxMChange" + " ServerInvokeMusicChange");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesBoomboxPitch", new CustomMessagingManager.HandleNamedMessageDelegate(BoomboxItemPatch.SetBoomboxPitch));
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesBoomboxMChange", new CustomMessagingManager.HandleNamedMessageDelegate(BoomboxItemPatch.ClientChangeMusic));
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ServerInvokeMusicChange", new CustomMessagingManager.HandleNamedMessageDelegate(BoomboxItemPatch.ServerReceivesMusicChangeRequest));
                RandomizerModBase.mls.LogInfo("Registering spike roof trap handler: " + "ClientReceivesSpikeData");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesSpikeData", new CustomMessagingManager.HandleNamedMessageDelegate(SpikeRoofTrapPatch.SetSpikeStats));
                RandomizerModBase.mls.LogInfo("Registering whoopie cushion handler: " + "ClientReceivesWhoopieCData");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesWhoopieCData", new CustomMessagingManager.HandleNamedMessageDelegate(WhoopieCushionItemPatch.SetPitchClientAndExplode));
                RandomizerModBase.mls.LogInfo("Registering giftbox handlers: " + "ClientReceivesExplodedGiftbox" + " ServerReceivesGiftboxInteraction " + "ClientReceivesDoubleDeadline " + "ClientReceivesHalveDeadline " + "ClientReceivesDoubleQuota " + "ClientReceivesHalveQuota " + "ClientReceivesEntranceTP " + "PlayerHealthDoubled");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesExplodedGiftbox", new CustomMessagingManager.HandleNamedMessageDelegate(GiftBoxItemPatch.GiftboxHasExplodedClient));
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ServerReceivesGiftboxInteraction", new CustomMessagingManager.HandleNamedMessageDelegate(GiftBoxItemPatch.ServerReceivesGiftboxInteraction));
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesDoubleDeadline", new CustomMessagingManager.HandleNamedMessageDelegate(GiftBoxItemPatch.DoubleDeadline));
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesHalveDeadline", new CustomMessagingManager.HandleNamedMessageDelegate(GiftBoxItemPatch.HalveDeadline));
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesDoubleQuota", new CustomMessagingManager.HandleNamedMessageDelegate(GiftBoxItemPatch.DoubleQuota));
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesHalveQuota", new CustomMessagingManager.HandleNamedMessageDelegate(GiftBoxItemPatch.HalveQuota));
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesEntranceTP", new CustomMessagingManager.HandleNamedMessageDelegate(GiftBoxItemPatch.TeleportPlayerToEntrance));
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "PlayerHealthDoubled", new CustomMessagingManager.HandleNamedMessageDelegate(GiftBoxItemPatch.DoublePlayerHP));
                RandomizerModBase.mls.LogInfo("Registering audio dictionary handler: " + "LoadAudioDicts");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "LoadAudioDicts", new CustomMessagingManager.HandleNamedMessageDelegate(StartOfRoundPatch.LoadAudioDict));
                RandomizerModBase.mls.LogInfo("Registering tzp chemical handler: " + "ClientReceivesChemColor");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesChemColor", new CustomMessagingManager.HandleNamedMessageDelegate(TetraChemicalItemPatch.ClientSetChemColor));
                RandomizerModBase.mls.LogInfo("Registering ship TP handler: " + "ServerReceivesTPModifyRequest");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ServerReceivesTPModifyRequest", new CustomMessagingManager.HandleNamedMessageDelegate(ShipTeleporterPatch.ServerReceivesTPModifyRequest));
                RandomizerModBase.mls.LogInfo("Registering entrance tp handler: " + "ServerBlockExit");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ServerBlockExit", new CustomMessagingManager.HandleNamedMessageDelegate(EntranceTeleportPatch.ServerBlockExitAndSync));
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ServerReceivesPlayerExit", new CustomMessagingManager.HandleNamedMessageDelegate(VehicleControllerPatch.ServerHandlePlayerExit));

                if (ES3.FileExists(GameNetworkManager.Instance.currentSaveFileName))
                {
                    if (!ES3.KeyExists("keysToLoad", GameNetworkManager.Instance.currentSaveFileName))
                    {
                        RandomizerModBase.mls.LogWarning("Master key not found within save file. Is your save corrupt? Skipping load.");
                        goto ResetAndEnd;
                    }

                    try
                    {
                        RandomizerValues.keysToLoad = ES3.Load("keysToLoad", GameNetworkManager.Instance.currentSaveFileName) as List<string>;
                        if (RandomizerValues.keysToLoad.Count == 0)
                        {
                            goto ResetAndEnd;
                        }

                        foreach (string key in RandomizerValues.keysToLoad)
                        {
                            switch (key)
                            {
                                case "knifeStatsDict":
                                    {
                                        RandomizerValues.knifeDamageDict = ES3.Load<Dictionary<ulong, int>>(key, GameNetworkManager.Instance.currentSaveFileName);
                                        break;
                                    }
                                case "jetpackDict":
                                    {
                                        RandomizerValues.jetpackPropertiesDict = ES3.Load<Dictionary<ulong, Tuple<float, float>>>(key, GameNetworkManager.Instance.currentSaveFileName);
                                        break;
                                    }
                                case "shovelStatsDict":
                                    {
                                        RandomizerValues.shovelDamageDict = ES3.Load<Dictionary<ulong, int>>(key, GameNetworkManager.Instance.currentSaveFileName);
                                        break;
                                    }
                                case "boomboxStatsDict":
                                    {
                                        RandomizerValues.boomboxPitchDict = ES3.Load<Dictionary<ulong, float>>(key, GameNetworkManager.Instance.currentSaveFileName);
                                        break;
                                    }
                                case "flashlightDict":
                                    {
                                        RandomizerValues.flashlightPropertyDict = ES3.Load<Dictionary<ulong, RFlashlightProperties>>(key, GameNetworkManager.Instance.currentSaveFileName);
                                        break;
                                    }
                                case "tzpChemDict":
                                    {
                                        RandomizerValues.chemicalEffectsDict = ES3.Load<Dictionary<ulong, ChemicalEffects>>(key, GameNetworkManager.Instance.currentSaveFileName);
                                        break;
                                    }
                                case "superKeys":
                                    {
                                        RandomizerValues.superchargedKeys = ES3.Load<List<ulong>>(key, GameNetworkManager.Instance.currentSaveFileName);
                                        break;
                                    }
                                case "tpCooldowns":
                                    {
                                        RandomizerValues.teleporterCooldowns = ES3.Load<Dictionary<bool, float>>(key, GameNetworkManager.Instance.currentSaveFileName);
                                        break;
                                    }
                                case "randomCarProps":
                                    {
                                        RandomizerValues.randomCarProperties = ES3.Load<RandomCarProperties>(key, GameNetworkManager.Instance.currentSaveFileName);
                                        RandomizerValues.randomizedCar = true;
                                        break;
                                    }
                            }
                        }
                        RandomizerModBase.mls.LogInfo("Loaded dictionaries.");
                    }
                    catch (Exception ex)
                    {
                        RandomizerModBase.mls.LogError("Exception caught during custom value deserialization. " + ex.Message);
                    }
                }
                else
                {
                    RandomizerModBase.mls.LogWarning("Save file not found.");
                    goto ResetAndEnd;
                }

                var runnableTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => typeof(ICustomValue).IsAssignableFrom(t) && !t.IsInterface);

                foreach (var type in runnableTypes)
                {
                    var instance = (ICustomValue)Activator.CreateInstance(type);
                    instance.ReloadStats();
                }

                ResetAndEnd:
                StartOfRoundPatch.ResetPlayers();
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
                RandomizerModBase.mls.LogInfo("Registering quota data handler: " + "ClientReceiveQuotaData");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesQuotaData", new CustomMessagingManager.HandleNamedMessageDelegate(StartOfRoundPatch.SyncQuotaValuesWithServer));
                RandomizerModBase.mls.LogInfo("Registering factory data handler: " + "ClientReceivesFactoryData");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesFactoryData", new CustomMessagingManager.HandleNamedMessageDelegate(RoundManagerPatch.SetReceivedFactoryData));
                RandomizerModBase.mls.LogInfo("Registering first time deadline show handler: " + "ClientReceivesFirstTimeShow");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesFirstTimeShow", new CustomMessagingManager.HandleNamedMessageDelegate(StartOfRoundPatch.ShowFirstTimeDeadlineClient));
                RandomizerModBase.mls.LogInfo("Registering dog handler: " + "ClientReceivesDogStats");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesDogStats", new CustomMessagingManager.HandleNamedMessageDelegate(MouthDogAIPatch.SetDogValuesSentByServer));
                RandomizerModBase.mls.LogInfo("Registering spider handler: " + "ClientReceivesSpiderData ");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesSpiderData", new CustomMessagingManager.HandleNamedMessageDelegate(SandSpiderAIPatch.SetSpiderDataSentByServer));
                RandomizerModBase.mls.LogInfo("Registering ship animator handlers: " + "ClientReceivesShipAnimData");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesShipAnimData", new CustomMessagingManager.HandleNamedMessageDelegate(StartOfRoundPatch.SetShipAnimatorSpeed));
                RandomizerModBase.mls.LogInfo("Registering giant handler: " + "ClientReceivesGiantData");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesGiantData", new CustomMessagingManager.HandleNamedMessageDelegate(ForestGiantAIPatch.SetGiantValuesSentByServer));
                RandomizerModBase.mls.LogInfo("Registering jetpack handler: " + "ClientReceivesJetpackData");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesJetpackData", new CustomMessagingManager.HandleNamedMessageDelegate(JetpackItemPatch.SetJetpackStatsSentByServer));
                RandomizerModBase.mls.LogInfo("Registering hoarder bug handler: " + "ClientReceivesHoarderBugData");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesHoarderBugData", new CustomMessagingManager.HandleNamedMessageDelegate(HoarderBugAIPatch.SetBugData));
                RandomizerModBase.mls.LogInfo("Registering butler handler: " + "ClientReceivesButlerData");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesButlerData", new CustomMessagingManager.HandleNamedMessageDelegate(ButlerAIPatch.SetButlerData));
                RandomizerModBase.mls.LogInfo("Registering jester handler: " + "ClientReceivesJesterData");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesJesterData", new CustomMessagingManager.HandleNamedMessageDelegate(JesterAIPatch.SetJesterData));
                RandomizerModBase.mls.LogInfo("Registering baboon hawk handler: " + "ClientReceivesBaboonData");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesBaboonData", new CustomMessagingManager.HandleNamedMessageDelegate(BaboonBirdAIPatch.SetBaboonData));
                RandomizerModBase.mls.LogInfo("Registering blob handler: " + "ClientReceivesBlobData");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesBlobData", new CustomMessagingManager.HandleNamedMessageDelegate(BlobAIPatch.SetBlobData));
                RandomizerModBase.mls.LogInfo("Registering crawler handler: " + "ClientReceivesCrawlerData");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesCrawlerData", new CustomMessagingManager.HandleNamedMessageDelegate(CrawlerAIPatch.SetCrawlerData));
                RandomizerModBase.mls.LogInfo("Registering dress girl handler: " + "ClientReceivesDressGirlData");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesDressGirlData", new CustomMessagingManager.HandleNamedMessageDelegate(DressGirlAIPatch.SetGirlData));
                RandomizerModBase.mls.LogInfo("Registering mech handlers: " + "ClientReceivesRadMechData" + " ClientReceivesMechScaleArray");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesRadMechData", new CustomMessagingManager.HandleNamedMessageDelegate(RadMechAIPatch.SetMechData));
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesMechScaleArray", new CustomMessagingManager.HandleNamedMessageDelegate(RadMechAIPatch.SetMechSpawnerScale));
                RandomizerModBase.mls.LogInfo("Registering nutcracker handler: " + "ClientReceivesNutcrackerData");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesNutcrackerData", new CustomMessagingManager.HandleNamedMessageDelegate(NutcrackerAIPatch.SetNutcrackerData));
                RandomizerModBase.mls.LogInfo("Registering flowerman handler: " + "ClientReceivesFlowermanData");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesFlowermanData", new CustomMessagingManager.HandleNamedMessageDelegate(FlowermanAIPatch.SetFlowermanStats));
                RandomizerModBase.mls.LogInfo("Registering puffer handler: " + "ClientReceivesPufferData");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesPufferData", new CustomMessagingManager.HandleNamedMessageDelegate(PufferAIPatch.SetPufferData));
                RandomizerModBase.mls.LogInfo("Registering centipede handler: " + "ClientReceivesCentipedeData");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesCentipedeData", new CustomMessagingManager.HandleNamedMessageDelegate(CentipedeAIPatch.SetCentipedeData));
                RandomizerModBase.mls.LogInfo("Registering flower snake handler: " + "ClientReceivesFlowerSnakeData");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesFlowerSnakeData", new CustomMessagingManager.HandleNamedMessageDelegate(FlowerSnakeEnemyPatch.SetFlowerSnakeStats));
                RandomizerModBase.mls.LogInfo("Registering spring man handler: " + "ClientReceivesSpringManData");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesSpringManData", new CustomMessagingManager.HandleNamedMessageDelegate(SpringManAIPatch.SetSpringManStats));
                RandomizerModBase.mls.LogInfo("Registering doublewing handler: " + "ClientReceivesDoublewingData");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesDoublewingData", new CustomMessagingManager.HandleNamedMessageDelegate(DoublewingAIPatch.SetDoublewingData));
                RandomizerModBase.mls.LogInfo("Registering bee handler: " + "ClientReceivesBeeData");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesBeeData", new CustomMessagingManager.HandleNamedMessageDelegate(RedLocustBeesPatch.SetBeeStats));
                RandomizerModBase.mls.LogInfo("Registering terminal switch handler: " + "TerminalRandomizationUsed " + "ReviveSpecificPlayer");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "TerminalRandomizationUsed", new CustomMessagingManager.HandleNamedMessageDelegate(TerminalPatch.SwitchTerminalMode));
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ReviveSpecificPlayer", new CustomMessagingManager.HandleNamedMessageDelegate(TerminalPatch.ClientProcessReviveMessage));
                RandomizerModBase.mls.LogInfo("Registering mine handlers: " + "ClientReceivesMineData " + "ClientHandlePlayerExploded");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesMineData", new CustomMessagingManager.HandleNamedMessageDelegate(LandminePatch.SetMineSizeClient));
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientHandlePlayerExploded", new CustomMessagingManager.HandleNamedMessageDelegate(PlayerControllerBPatch.KillLocalPlayerByExp));
                RandomizerModBase.mls.LogInfo("Registering knife and shovel damage handlers: " + "ClientReceivesKnifeData");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesKnifeData", new CustomMessagingManager.HandleNamedMessageDelegate(KnifeItemPatch.SetKnifeData));
                RandomizerModBase.mls.LogInfo("Registering client sync handler: " + "DeclareClientAsSynced" + " ClientReceivesTerminalState");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesTerminalState", new CustomMessagingManager.HandleNamedMessageDelegate(TerminalPatch.SetTerminalState));
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "DeclareClientAsSynced", new CustomMessagingManager.HandleNamedMessageDelegate(PlayerControllerBPatch.SetClientAsSynced));
                RandomizerModBase.mls.LogInfo("Registering shovel damage handler: " + "ClientReceivesShovelData");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesShovelData", new CustomMessagingManager.HandleNamedMessageDelegate(ShovelPatch.SetShovelData));
                RandomizerModBase.mls.LogInfo("Registering turret handler: " + "ClientReceivesTurretData");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesTurretData", new CustomMessagingManager.HandleNamedMessageDelegate(TurretPatch.SetTurretStats));
                RandomizerModBase.mls.LogInfo("Registering boombox handlers: " + "ClientReceivesBoomboxPitch" + " ClientReceivesBoomboxMChange" + " ServerInvokeMusicChange");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesBoomboxPitch", new CustomMessagingManager.HandleNamedMessageDelegate(BoomboxItemPatch.SetBoomboxPitch));
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesBoomboxMChange", new CustomMessagingManager.HandleNamedMessageDelegate(BoomboxItemPatch.ClientChangeMusic));
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ServerInvokeMusicChange", new CustomMessagingManager.HandleNamedMessageDelegate(BoomboxItemPatch.ServerReceivesMusicChangeRequest));
                RandomizerModBase.mls.LogInfo("Registering spike roof trap handler: " + "ClientReceivesSpikeData");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesSpikeData", new CustomMessagingManager.HandleNamedMessageDelegate(SpikeRoofTrapPatch.SetSpikeStats));
                RandomizerModBase.mls.LogInfo("Registering whoopie cushion handler: " + "ClientReceivesWhoopieCData");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesWhoopieCData", new CustomMessagingManager.HandleNamedMessageDelegate(WhoopieCushionItemPatch.SetPitchClientAndExplode));
                RandomizerModBase.mls.LogInfo("Registering giftbox handlers: " + "ClientReceivesExplodedGiftbox " + "ClientDespawnGiftbox " + "ClientReceivesPlayedSoundGift " + "ServerReceivesGiftboxInteraction " + "ClientSetTeleportPlayerGift " + "ClientReceivesPlayerTeleport " + "ClientReceivesResetPlayer " + "ClientReceivesDoubleDeadline " + "ClientReceivesHalveDeadline " + "ClientReceivesDoubleQuota " + "ClientReceivesHalveQuota " + "ClientReceivesEntranceTP " + "ClientReceivesPlayerRecolor " + "PlayerHealthDoubled");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesExplodedGiftbox", new CustomMessagingManager.HandleNamedMessageDelegate(GiftBoxItemPatch.GiftboxHasExplodedClient));
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesPlayedSoundGift", new CustomMessagingManager.HandleNamedMessageDelegate(GiftBoxItemPatch.PlaySoundGiftboxClient));
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ServerReceivesGiftboxInteraction", new CustomMessagingManager.HandleNamedMessageDelegate(GiftBoxItemPatch.ServerReceivesGiftboxInteraction));
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientSetTeleportPlayerGift", new CustomMessagingManager.HandleNamedMessageDelegate(GiftBoxItemPatch.SetTeleportPlayer));
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesPlayerTeleport", new CustomMessagingManager.HandleNamedMessageDelegate(GiftBoxItemPatch.ClientTeleportPlayer));
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesResetPlayer", new CustomMessagingManager.HandleNamedMessageDelegate(GiftBoxItemPatch.ClientResetPlayer));
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesDoubleDeadline", new CustomMessagingManager.HandleNamedMessageDelegate(GiftBoxItemPatch.DoubleDeadline));
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesHalveDeadline", new CustomMessagingManager.HandleNamedMessageDelegate(GiftBoxItemPatch.HalveDeadline));
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesDoubleQuota", new CustomMessagingManager.HandleNamedMessageDelegate(GiftBoxItemPatch.DoubleQuota));
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesHalveQuota", new CustomMessagingManager.HandleNamedMessageDelegate(GiftBoxItemPatch.HalveQuota));
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesEntranceTP", new CustomMessagingManager.HandleNamedMessageDelegate(GiftBoxItemPatch.TeleportPlayerToEntrance));
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesPlayerRecolor", new CustomMessagingManager.HandleNamedMessageDelegate(GiftBoxItemPatch.SetPlayerColor));
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "PlayerHealthDoubled", new CustomMessagingManager.HandleNamedMessageDelegate(GiftBoxItemPatch.DoublePlayerHP));
                RandomizerModBase.mls.LogInfo("Registering audio dictionary handler: " + "LoadAudioDicts");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "LoadAudioDicts", new CustomMessagingManager.HandleNamedMessageDelegate(StartOfRoundPatch.LoadAudioDict));
                RandomizerModBase.mls.LogInfo("Registering flashlight handler: " + "ClientReceivesFlashlightColor");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesFlashlightColor", new CustomMessagingManager.HandleNamedMessageDelegate(FlashlightItemPatch.SetFlashlightColor));
                RandomizerModBase.mls.LogInfo("Registering tzp chemical handler: " + "ClientReceivesChemColor");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesChemColor", new CustomMessagingManager.HandleNamedMessageDelegate(TetraChemicalItemPatch.ClientSetChemColor));
                RandomizerModBase.mls.LogInfo("Registering superkey handler:" + "ClientReceivesSuperKey");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesSuperKey", new CustomMessagingManager.HandleNamedMessageDelegate(KeyItemPatch.ClientReceivesSuperKey));
                RandomizerModBase.mls.LogInfo("Registering ship TP handler: " + "ClientsStartTPModCoroutine");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientsStartTPModCoroutine", new CustomMessagingManager.HandleNamedMessageDelegate(ShipTeleporterPatch.StartConnectCoroutineClient));
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientSetTPCooldown", new CustomMessagingManager.HandleNamedMessageDelegate(ShipTeleporterPatch.SetTpCooldownClient));
                RandomizerModBase.mls.LogInfo("Registering ship cord handler: " + "ClientReceivesHornPitch");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesHornPitch", new CustomMessagingManager.HandleNamedMessageDelegate(ShipAlarmCordPatch.SetPitch));
                RandomizerModBase.mls.LogInfo("Registering entrance tp handler: " + "ExitHasBeenBlocked");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ExitHasBeenBlocked", new CustomMessagingManager.HandleNamedMessageDelegate(EntranceTeleportPatch.ClientExitIDResolver));
                RandomizerModBase.mls.LogInfo("Registering custom UI handler: " + "ClientReceivesBroadcastMsg");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesBroadcastMsg", new CustomMessagingManager.HandleNamedMessageDelegate(CustomUI.ProcessBroadcastMessage));
                RandomizerModBase.mls.LogInfo("Registering radmech missile handler: " + "ClientReceivesMissileScale");
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesMissileScale", new CustomMessagingManager.HandleNamedMessageDelegate(RadMechMissilePatch.SetMissileSize));
                RandomizerModBase.mls.LogInfo("Registering car handlers: " + "ClientReceivesCarScale " + "ClientHasBeenResized");
                NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesCarData", new CustomMessagingManager.HandleNamedMessageDelegate(VehicleControllerPatch.ClientSetCarProperties));
                NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesDriverExit", new CustomMessagingManager.HandleNamedMessageDelegate(VehicleControllerPatch.HandlePlayerExit));
                RandomizerModBase.mls.LogInfo("Registering bush wolf handler: " + "ClientReceivesWolfStats");
                NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesWolfStats", new CustomMessagingManager.HandleNamedMessageDelegate(BushWolfEnemyPatch.ClientSetWolfStats));
                RandomizerModBase.mls.LogInfo("Tibnan.lcrandomizermod_" + "ClientReceivesSurgeonData");
                NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesSurgeonData", new CustomMessagingManager.HandleNamedMessageDelegate(ClaySurgeonAIPatch.SetSurgeonData));
                RandomizerModBase.mls.LogInfo("Registering baby handler: " + "ClientReceivesBabyStat");
                NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesBabyStat", new CustomMessagingManager.HandleNamedMessageDelegate(CaveDwellerAIPatch.ClientSetBabyStats));

                StartOfRoundPatch.ResetPlayers();

                RequestSyncDataOnSpawn();
            }

            __instance.gameObject.AddComponent<CustomUI>();
        }

        public static void RequestSyncDataOnSpawn()
        {
            Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage("Tibnan.lcrandomizermod_" + "ServerReceivesItemDataRequest", 0UL, new FastBufferWriter(4, Unity.Collections.Allocator.Temp, -1), NetworkDelivery.Reliable);
        }

        public static void ServerBeginItemDataTransfer(ulong sender, FastBufferReader __)
        {
            var runnableTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => typeof(ICustomValue).IsAssignableFrom(t) && !t.IsInterface);

            foreach (var type in runnableTypes)
            {
                var instance = (ICustomValue)Activator.CreateInstance(type);
                instance.SyncStatsWithClients();
            }

            FastBufferWriter writer = new FastBufferWriter(sizeof(bool), Unity.Collections.Allocator.Temp, -1);
            writer.WriteValueSafe<bool>(RandomizerValues.mapRandomizedInTerminal);

            Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage("Tibnan.lcrandomizermod_" + "ClientReceivesTerminalState", sender, writer, NetworkDelivery.Reliable);

            Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage("Tibnan.lcrandomizermod_" + "DeclareClientAsSynced", sender, new FastBufferWriter(4, Unity.Collections.Allocator.Temp, -1), NetworkDelivery.Reliable);
        }

        public static void SetClientAsSynced(ulong _, FastBufferReader reader)
        {
            RandomizerValues.isClientSynced = true;
            RandomizerModBase.mls.LogInfo("Client synced. " + RandomizerValues.isClientSynced);
        }

        public static void KillLocalPlayerByExp(ulong _, FastBufferReader __)
        {
            PlayerControllerB player = GameNetworkManager.Instance.localPlayerController;
            player.KillPlayer(player.velocityLastFrame, true, CauseOfDeath.Blast);
        }

        [HarmonyPatch(nameof(PlayerControllerB.PlayQuickSpecialAnimation))]
        [HarmonyPrefix]
        public static bool OverrideAnim(PlayerControllerB __instance)
        {
            if (RandomizerValues.blockAnims)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        [HarmonyPatch(nameof(PlayerControllerB.DespawnHeldObject))]
        [HarmonyPrefix]
        public static bool OverrideDespawn(PlayerControllerB __instance)
        {
            if (RandomizerValues.blockDespawn)
            {
                RandomizerValues.blockDespawn = false;
                return false;
            }
            else
            {
                return true;
            }
        }

        [HarmonyPatch("OnEnable")]
        [HarmonyPostfix]
        public static void SubscribeToEvent(PlayerControllerB __instance)
        {
            RandomizerModBase.mls.LogInfo("Subscribing to mod events.");
            InputActionAsset actions = IngamePlayerSettings.Instance.playerInput.actions;
            actions.FindAction("Interact").performed += ShipTeleporterPatch.CheckForTeleporterLOS;
            actions.FindAction("Jump", false).performed += MineshaftElevatorControllerPatch.ResetElevatorSpeedOnJump;
        }

        [HarmonyPatch("OnDisable")]
        [HarmonyPostfix]
        public static void UnsubscribeFromEvent(PlayerControllerB __instance)
        {
            RandomizerModBase.mls.LogInfo("Unsubscribing from mod events.");
            InputActionAsset actions = IngamePlayerSettings.Instance.playerInput.actions;
            actions.FindAction("Interact").performed -= ShipTeleporterPatch.CheckForTeleporterLOS;
            actions.FindAction("Jump", false).performed -= MineshaftElevatorControllerPatch.ResetElevatorSpeedOnJump;
        }

        [HarmonyPatch(nameof(PlayerControllerB.DiscardHeldObject))]
        [HarmonyPrefix]
        public static bool BlockItemDrop(PlayerControllerB __instance)
        {
            return !RandomizerValues.blockDrop;
        }
    }
}
