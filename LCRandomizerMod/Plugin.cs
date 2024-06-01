using BepInEx;
using HarmonyLib;
using BepInEx.Logging;
using LCRandomizerMod.Patches;
using System.Collections.Generic;
using UnityEngine;

namespace LCRandomizerMod
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class RandomizerModBase : BaseUnityPlugin
    {
        public const string modName = "Lethal Company Randomizer Mod";
        public const string modVersion = "1.2.6";
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

            mls.LogInfo("Patched all base classes.");

            mls.LogInfo("Lethal Company Randomizer Mod Initialized!");
        }
    }
}
