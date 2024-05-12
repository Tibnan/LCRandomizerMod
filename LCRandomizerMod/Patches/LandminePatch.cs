using HarmonyLib;
using LethalLib.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace LCRandomizerMod.Patches
{
    [HarmonyPatch(typeof(Landmine))]
    internal class LandminePatch
    {
        [HarmonyPatch(nameof(Landmine.SetOffMineAnimation))]
        [HarmonyPrefix]
        public static bool RandomlySpawnShotgun(Landmine __instance)
        {
            if (new System.Random().Next(1, 2) == 1)
            {
                GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(RandomizerValues.allItemsListDict.GetValueSafe("Shotgun").spawnPrefab, __instance.transform.position, Quaternion.identity, RoundManager.Instance.spawnedScrapContainer);
                gameObject.GetComponent<GrabbableObject>().fallTime = 0f;
                gameObject.GetComponent<NetworkObject>().Spawn(false);
                RandomizerModBase.mls.LogWarning("SPAWNED SHOTGUN");
                
                Unity.Netcode.NetworkManager.Singleton.SpawnManager.SpawnedObjects[__instance.NetworkObjectId].Despawn(true);
                return false;
            }
            return true;
        }
    }
}
