using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace LCRandomizerMod
{
    internal static class RandomizerValues
    {
        //Mod specific

        public static bool isClientSynced = false;
        public static List<string> keysToLoad = new List<string>();

        //Mod specific

        //StartOfRoundPatch

        public static Vector3 defaultPlayerScale;
        public static uint defaultPlayerMaskLayer;
        public const float defaultPlayerPitch = 1f;
        public static Vector3 defaultPlayerHeadScale;
        public static Vector3 defaultPlayerBillboardScale;
        public static Vector3 defaultPlayerBillboardPos;

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
        public static float scrapWeight;

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

        //public static Tuple<float, float> jetpackProperties; 
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

        //FlowerSnakeEnemyPatch

        public static Dictionary<ulong, float> flowerSnakeSpeedsDict = new Dictionary<ulong, float>();

        //FlowerSnakeEnemyPatch

        //SpringManAIPatch

        public static Dictionary<ulong, float> springManSpeedsDict = new Dictionary<ulong, float>();

        //SpringManAIPatch

        //DoublewingAIPatch

        public static Dictionary<ulong, float> doublewingSpeedsDict = new Dictionary<ulong, float>();

        //DoublewingAIPatch

        //RedLocustBessPatch

        public static Dictionary<ulong, float> redLocustSpeedsDict = new Dictionary<ulong, float>();

        //RedLocustBeesPatch

        //ExtensionLadderPatch

        public static bool isRandomized = false;

        //ExtensionLadderPatch

        //TerminalPatch

        public static bool mapRandomizedInTerminal = false;
        public static Vector3 defaultTerminalScale;

        //TerminalPatch

        //KnifeItemPatch

        public static Dictionary<ulong, int> knifeDamageDict = new Dictionary<ulong, int>();
        
        //KnifeItemPatch

        //ShovelPatch

        public static Dictionary<ulong, int> shovelDamageDict = new Dictionary<ulong, int>();

        //ShovelPatch

        //BoomboxItemPatch

        public static Dictionary<ulong, float> boomboxPitchDict = new Dictionary<ulong, float>();

        //BoomboxItemPatch

        public static void ClearDicts(bool deleteAll)
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
            flowerSnakeSpeedsDict.Clear();
            springManSpeedsDict.Clear();
            doublewingSpeedsDict.Clear();
            redLocustSpeedsDict.Clear();
            spawnedMechScales.Clear();

            if (deleteAll)
            {
                knifeDamageDict.Clear();
                shovelDamageDict.Clear();
                jetpackPropertiesDict.Clear();
                boomboxPitchDict.Clear();
            }
        }
    }
}
