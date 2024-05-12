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

        public static float shipDoorAnimatorSpeed;
        public static Dictionary<string, Item> allItemsListDict = new Dictionary<string, Item>();

        //StartOfRoundPatch

        //RoundManagerPatch

        public static int randomizedWeatherIdx;
        public static float factorySizeMultiplierRand;

        //RoundManagerPatch

        //MouthDogAIPatch

        public static Dictionary<ulong, float> dogSpeedsDict = new Dictionary<ulong, float>();
        public static Vector3 defaultDogScale = new Vector3(1f, 1f, 1f);

        public static float dogSpeedClient;
        public static ulong dogIDClient;
        public static int dogEnemyHPClient;
        public static float dogScaleClient;

        //MouthDogAIPatch

        //GrabbableObjectPatch

        public static int scrapValue;
        public static ulong scrapReference;

        //GrabbableObjectPatch

        //SandSpiderPatch

        public static Dictionary<ulong, float> spiderSpeedsDict = new Dictionary<ulong, float>();
        public static Vector3 defaultSpiderScale = new Vector3(1f, 1f, 1f);

        public static float spiderSpeedClient;
        public static ulong spiderIDClient;
        public static int spiderHealthClient;
        public static float spiderScaleClient;

        //SandSpiderPatch

        public static void ClearLists()
        {
            dogSpeedsDict.Clear();
            spiderSpeedsDict.Clear();
        }
    }
}
