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
        public const string modVersion = "0.6.0";
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
            harmony.PatchAll(typeof(HUDManagerPatch));
            harmony.PatchAll(typeof(ShotgunItemPatch));
            harmony.PatchAll(typeof(MouthDogAIPatch));


            mls.LogInfo("Patched all base classes.");
        }
    }
}
