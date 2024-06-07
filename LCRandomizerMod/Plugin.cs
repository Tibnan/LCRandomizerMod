using BepInEx;
using HarmonyLib;
using BepInEx.Logging;
using LCRandomizerMod.Patches;
using UnityEngine;
using UnityEngine.Video;
using System;
using System.IO;
using System.Reflection;

namespace LCRandomizerMod
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class RandomizerModBase : BaseUnityPlugin
    {
        public const string modName = "Lethal Company Randomizer Mod";
        public const string modVersion = "1.6.2";
        public const string modGUID = "Tibnan.lcrandomizermod";

        private readonly Harmony harmony = new Harmony(modGUID);

        public static ManualLogSource mls;
        
        private static RandomizerModBase Instance;

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

            mls.LogInfo("Patched all base classes.");

            mls.LogInfo("Loading asset bundle.");

            AssetBundle assetBundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "lcrm"));

            if (assetBundle != null)
            {
                AudioClip clip = assetBundle.LoadAsset<AudioClip>("Assets\\LCRM_Assets\\LCRM_audio.mp3");
                if (clip == null)
                {
                    mls.LogError("Failed to load audio file.");
                }
                else
                {
                    RandomizerValues.introAudio = clip;
                }

                VideoClip vClip = assetBundle.LoadAsset<VideoClip>("Assets\\LCRM_Assets\\LCRM_video.mp4");
                if (vClip == null)
                {
                    mls.LogError("Failed to load video file.");
                }
                else
                {
                    RandomizerValues.introVideo = vClip;
                }

                //for (int i = 1; i < 11; i++)
                //{
                //    RandomizerValues.modPrefabs.Add(String.Format("Crystal {0}", i), assetBundle.LoadAsset<GameObject>(String.Format(i < 10 ? "Assets\\LCRM_Assets\\Crystalsv0{0}.prefab" : "Assets\\LCRM_Assets\\Crystalsv{0}.prefab", i)));
                //}
            }
            else
            {
                mls.LogError("Failed to load asset bundle.");
            }

            mls.LogInfo("Lethal Company Randomizer Mod Initialized!");
        }
    }
}
