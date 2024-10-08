﻿using BepInEx;
using HarmonyLib;
using BepInEx.Logging;
using LCRandomizerMod.Patches;
using UnityEngine;
using UnityEngine.Video;
using System.IO;
using System.Reflection;

namespace LCRandomizerMod
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class RandomizerModBase : BaseUnityPlugin
    {
        public const string modName = "Lethal Company Randomizer Mod";
        public const string modVersion = "1.10.6";
        public const string modGUID = "Tibnan.lcrandomizermod";

        private readonly Harmony harmony = new Harmony(modGUID);

        public static ManualLogSource mls;
        
        private static RandomizerModBase Instance;

        public static Font modFont;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            mls = BepInEx.Logging.Logger.CreateLogSource(modName);

            harmony.PatchAll(typeof(RandomizerModBase));
            harmony.PatchAll(typeof(PlayerControllerBPatch));
            harmony.PatchAll(typeof(RoundManagerPatch));
            harmony.PatchAll(typeof(TimeOfDayPatch));
            harmony.PatchAll(typeof(StartOfRoundPatch));
            harmony.PatchAll(typeof(GrabbableObjectPatch));
            harmony.PatchAll(typeof(TetraChemicalItemPatch));
            harmony.PatchAll(typeof(ShotgunItemPatch));
            harmony.PatchAll(typeof(MouthDogAIPatch));
            harmony.PatchAll(typeof(SandSpiderAIPatch));
            harmony.PatchAll(typeof(LandminePatch));
            harmony.PatchAll(typeof(ForestGiantAIPatch));
            harmony.PatchAll(typeof(JetpackItemPatch));
            harmony.PatchAll(typeof(HoarderBugAIPatch));
            harmony.PatchAll(typeof(ButlerAIPatch));
            harmony.PatchAll(typeof(JesterAIPatch));
            harmony.PatchAll(typeof(BaboonBirdAIPatch));
            harmony.PatchAll(typeof(BlobAIPatch));
            harmony.PatchAll(typeof(CrawlerAIPatch));
            harmony.PatchAll(typeof(DressGirlAIPatch));
            harmony.PatchAll(typeof(RadMechAIPatch));
            harmony.PatchAll(typeof(NutcrackerAIPatch));
            harmony.PatchAll(typeof(FlowermanAIPatch));
            harmony.PatchAll(typeof(PufferAIPatch));
            harmony.PatchAll(typeof(CentipedeAIPatch));
            harmony.PatchAll(typeof(FlowerSnakeEnemyPatch));
            harmony.PatchAll(typeof(SpringManAIPatch));
            harmony.PatchAll(typeof(DoublewingAIPatch));
            harmony.PatchAll(typeof(RedLocustBeesPatch));
            harmony.PatchAll(typeof(ExtensionLadderItemPatch));
            harmony.PatchAll(typeof(TerminalPatch));
            harmony.PatchAll(typeof(KnifeItemPatch));
            harmony.PatchAll(typeof(GameNetworkManagerPatch));
            harmony.PatchAll(typeof(ShovelPatch));
            harmony.PatchAll(typeof(TurretPatch));
            harmony.PatchAll(typeof(WhoopieCushionItemPatch));
            harmony.PatchAll(typeof(BoomboxItemPatch));
            harmony.PatchAll(typeof(SpikeRoofTrapPatch));
            harmony.PatchAll(typeof(GiftBoxItemPatch));
            harmony.PatchAll(typeof(DeadBodyInfoPatch));
            harmony.PatchAll(typeof(PreInitSceneScriptPatch));
            harmony.PatchAll(typeof(FlashlightItemPatch));
            harmony.PatchAll(typeof(KeyItemPatch));
            harmony.PatchAll(typeof(ShipTeleporterPatch));
            harmony.PatchAll(typeof(ShipAlarmCordPatch));
            harmony.PatchAll(typeof(EntranceTeleportPatch));
            harmony.PatchAll(typeof(RadMechMissilePatch));
            harmony.PatchAll(typeof(VehicleControllerPatch));
            harmony.PatchAll(typeof(ClaySurgeonAIPatch));
            //harmony.PatchAll(typeof(BushWolfEnemyPatch)); --Will be reworked in a later update according to dev
            harmony.PatchAll(typeof(SprayPaintItemPatch));
            harmony.PatchAll(typeof(CaveDwellerAIPatch));
            harmony.PatchAll(typeof(MineshaftElevatorControllerPatch));
            //harmony.PatchAll(typeof(SoccerBallPropPatch)); --Must refine
            //harmony.PatchAll(typeof(ClockPropPatch)); --Must come up with something else

            mls.LogInfo("Patched all base classes.");

            mls.LogInfo("Loading asset bundle.");

            AssetBundle assetBundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "lcrm"));

            if (assetBundle != null)
            {
                AudioClip clip = assetBundle.LoadAsset<AudioClip>("Assets\\lcrm\\LCRM_audio.mp3");
                if (clip == null)
                {
                    mls.LogError("Failed to load audio file.");
                }
                else
                {
                    RandomizerValues.introAudio = clip;
                }

                VideoClip vClip = assetBundle.LoadAsset<VideoClip>("Assets\\lcrm\\LCRM_video.mp4");
                if (vClip == null)
                {
                    mls.LogError("Failed to load video file.");
                }
                else
                {
                    RandomizerValues.introVideo = vClip;
                }

                modFont = assetBundle.LoadAsset<Font>("Assets\\lcrm\\PerfectDOSVGA437.ttf");
            }
            else
            {
                mls.LogError("Failed to load asset bundle.");
            }

            //AssetBundle itemBundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "lcrm_itembundle"));

            //Item pandorasBox = itemBundle.LoadAsset<Item>("Assets\\Unity\\Native\\PandorasBoxItem.asset");
            //RandomizerValues.modItemsDict.Add(pandorasBox.name, pandorasBox);

            //PandorasBoxItem boxItem = pandorasBox.spawnPrefab.AddComponent<PandorasBoxItem>();
            ////boxItem.fallTime = 0f;
            //////boxItem.itemProperties = pandorasBox;

            //int rarity = 100;
            //LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(pandorasBox.spawnPrefab);
            //LethalLib.Modules.Items.RegisterScrap(pandorasBox, rarity, LethalLib.Modules.Levels.LevelTypes.All);
            ////LethalLib.Modules.Items.RegisterItem(pandorasBox);

            mls.LogInfo("Lethal Company Randomizer Mod Initialized!");
        }
    }
}
