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

                StartOfRoundPatch.ResetPlayers();
                //RandomizerValues.ClearDicts();
                RandomizerValues.jetpackPropertiesDict.Clear();
                //RandomizerModBase.mls.LogInfo("Registering pitch data handler: " + "ClientReceivesPitchData");
                //Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesPitchData", new CustomMessagingManager.HandleNamedMessageDelegate(StartOfRoundPatch.SetPitchDataSentByServer));
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

                StartOfRoundPatch.ResetPlayers();
                //RandomizerValues.ClearDicts();
                RandomizerValues.jetpackPropertiesDict.Clear();
                //RandomizerModBase.mls.LogInfo("Registering pitch data handler: " + "ClientReceivesPitchData");
                //Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Tibnan.lcrandomizermod_" + "ClientReceivesPitchData", new CustomMessagingManager.HandleNamedMessageDelegate(StartOfRoundPatch.SetPitchDataSentByServer));
            }
        }

        [HarmonyPatch(nameof(PlayerControllerB.Crouch))]
        [HarmonyPostfix]
        public static void SpawnEnemy(PlayerControllerB __instance)
        {
            //TimeOfDay.Instance.currentDayTime = 18000;
            //TimeOfDay.Instance.totalTime = 18000;

            //if (StartOfRound.Instance.inShipPhase || !Unity.Netcode.NetworkManager.Singleton.IsServer)
            //{
            //    return;
            //}
            //RandomizerModBase.mls.LogInfo("Should spawn enemy");
            //EnemyType forestGiant = new EnemyType();

            //using (List<SpawnableEnemyWithRarity>.Enumerator enumerator = StartOfRound.Instance.currentLevel.OutsideEnemies.GetEnumerator())
            //{
            //    while (enumerator.MoveNext())
            //    {
            //        SpawnableEnemyWithRarity spawnableEnemyWithRarity = enumerator.Current;

            //        if (spawnableEnemyWithRarity.enemyType.enemyPrefab.GetComponent<ForestGiantAI>() != null)
            //        {
            //            forestGiant = spawnableEnemyWithRarity.enemyType;

            //            spawnableEnemyWithRarity.rarity = 999;
            //            RandomizerModBase.mls.LogInfo("Spawning enemy: " + spawnableEnemyWithRarity.enemyType.enemyPrefab.name);

            //            GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(spawnableEnemyWithRarity.enemyType.enemyPrefab, __instance.transform.position, Quaternion.identity);
            //            gameObject.GetComponent<NetworkObject>().Spawn(true);
            //            break;
            //        }
            //        else
            //        {
            //            forestGiant = null;
            //        }
            //    }

            //    if (forestGiant.enemyPrefab == null)
            //    {
            //        RandomizerModBase.mls.LogError("Forest giant not found!!!");
            //    }
            //}

            //RoundManager.Instance.SpawnEnemyGameObject(new Vector3(0f, 0f, 0f), 0f, 1, forestGiant);
        }
    }
}
