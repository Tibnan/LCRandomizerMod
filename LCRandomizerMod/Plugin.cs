using System;
using BepInEx;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using BepInEx.Logging;
using GameNetcodeStuff;
using LCRandomizerMod.Patches;
using UnityEngine;
using System.Reflection;
using System.IO;

namespace LCRandomizerMod
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class RandomizerModBase : BaseUnityPlugin
    {
        public const string modName = "Lethal Company Randomizer Mod";
        public const string modVersion = "0.8.2";
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


            mls.LogInfo("Patched all base classes.");
        }
    }
}
