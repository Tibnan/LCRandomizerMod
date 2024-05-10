using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LCRandomizerMod
{
    internal static class RandomizerValues
    {
        //StartOfRoundPatch

        public static Vector3 defaultPlayerScale;
        public static uint defaultPlayerMaskLayer;
        public const float defaultPlayerPitch = 1f;

        public static float sprintRand;
        public static int healthRand;
        public static float movementSpeedRand;
        public static float sinkMultiplierRand;

        public static int quotaRand;
        public static int deadlineRand;

        public static bool firstTimeShow;

        //StartOfRoundPatch

        //RoundManagerPatch

        public static int randomizedWeatherIdx;
        public static float factorySizeMultiplierRand;

        //RoundManagerPatch

        //MouthDogAIPatch

        public static List<float> dogSpeeds = new List<float>();
        public static int dogEnemyHP;
        public static List<ulong> dogIDs = new List<ulong>();
        public static Vector3 defaultDogScale = new Vector3(1f, 1f, 1f);
        public static float dogScale;

        //MouthDogAIPatch

        //GrabbableObjectPatch

        public static int scrapValue;
        public static ulong scrapReference;

        //GrabbableObjectPatch

        //SandSpiderPatch

        public static List<float> spiderSpeedRands = new List<float>();
        public static int spiderHealthRand;
        public static List<ulong> spiderID = new List<ulong>();
        public static Vector3 defaultSpiderScale = new Vector3(1f, 1f, 1f);
        public static float spiderScaleRand;

        //SandSpiderPatch

        public static void ClearLists()
        {
            dogSpeeds.Clear();
            dogIDs.Clear();
        }
    }
}
