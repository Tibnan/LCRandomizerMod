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

        //MouthDogAIPatch

        //GrabbableObjectPatch

        public static int scrapValue;
        public static ulong scrapReference;

        //GrabbableObjectPatch

        //SandSpiderPatch

        public static Dictionary<ulong, float> spiderSpeedsDict = new Dictionary<ulong, float>();
        public static Vector3 defaultSpiderScale = new Vector3(1f, 1f, 1f);

        //SandSpiderPatch

        //ForestGiantAIPatch

        public static Dictionary<ulong, float> giantSpeedsDict = new Dictionary<ulong, float>();
        public static Vector3 defaultGiantScale = new Vector3(1f, 1f, 1f);

        //ForestGiantAIPatch

        //JetpackItemPatch

        public static Dictionary<ulong, Tuple<float, float>> jetpackPropertiesDict = new Dictionary<ulong, Tuple<float, float>>();

        //JetpackItemPatch

        //HoarderBugAIPatch

        public static Dictionary<ulong, float> hoarderBugSpeedsDict = new Dictionary<ulong, float>();

        //HoarderBugAIPatch

        //ButlerAIPatch

        public static Dictionary<ulong, float> butlerSpeedsDict = new Dictionary<ulong, float>();

        //ButlerAIPatch

        //JesterAIPatch

        public static Dictionary<ulong, float> jesterSpeedsDict = new Dictionary<ulong, float>();

        //JesterAIPatch

        //BaboonBirdAIPatch

        public static Dictionary<ulong, float> baboonSpeedsDict = new Dictionary<ulong, float>();

        //BaboonBirdAIPatch

        //BlobAIPatch

        public static Dictionary<ulong, float> blobSpeedsDict = new Dictionary<ulong, float>();

        //BlobAIPatch

        //CrawlerAIPatch

        public static Dictionary<ulong, float> crawlerSpeedsDict = new Dictionary<ulong, float>();

        //CrawlerAIPatch

        //DressGirlAIPatch

        public static Dictionary<ulong, float> dressGirlSpeedsDict = new Dictionary<ulong, float>();

        //DressGirlAIPatch

        //RadMechAIPatch

        public static Dictionary<ulong, float> radMechSpeedsDict = new Dictionary<ulong, float>();
        public static int spawnedMechCount = 0;
        public static List<float> spawnedMechScales = new List<float>();

        //RadMechAIPatch

        //NutcrackerAIPatch

        public static Dictionary<ulong, float> nutcrackerSpeedsDict = new Dictionary<ulong, float>();

        //NutcrackerAIPatch

        //FlowermanAIPatch

        public static Dictionary<ulong, float> flowermanSpeedsDict = new Dictionary<ulong, float>();

        //FlowermanAIPatch

        //PufferAIPatch

        public static Dictionary<ulong, float> pufferSpeedsDict = new Dictionary<ulong, float>();

        //PufferAIPatch

        //CentipedeAIPatch

        public static Dictionary<ulong, float> centipedeSpeedsDict = new Dictionary<ulong, float>();

        //CentipedeAIPatch

        public static void ClearDicts()
        {
            dogSpeedsDict.Clear();
            spiderSpeedsDict.Clear();
            giantSpeedsDict.Clear();
            hoarderBugSpeedsDict.Clear();
            butlerSpeedsDict.Clear();
            jesterSpeedsDict.Clear();
            baboonSpeedsDict.Clear();
            blobSpeedsDict.Clear();
            crawlerSpeedsDict.Clear();
            dressGirlSpeedsDict.Clear();
            radMechSpeedsDict.Clear();
            nutcrackerSpeedsDict.Clear();
            flowermanSpeedsDict.Clear();
            pufferSpeedsDict.Clear();
            centipedeSpeedsDict.Clear();
        }
    }
}
