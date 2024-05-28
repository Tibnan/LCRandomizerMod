using GameNetcodeStuff;
using HarmonyLib;
using System;
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
            if (Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                if (new System.Random().Next(1, 5) == 4) //SET BEFORE RELEASE
                {
                    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(RandomizerValues.allItemsListDict.GetValueSafe("Shotgun").spawnPrefab, __instance.transform.position, Quaternion.identity, RoundManager.Instance.spawnedScrapContainer);
                    gameObject.GetComponent<GrabbableObject>().fallTime = 0f;
                    gameObject.GetComponent<NetworkObject>().Spawn(false);
                    RandomizerModBase.mls.LogWarning("SPAWNED SHOTGUN");

                    Unity.Netcode.NetworkManager.Singleton.SpawnManager.SpawnedObjects[__instance.NetworkObjectId].Despawn(true);
                    return false;
                }
                else
                {
                    foreach (PlayerControllerB playerControllerB in StartOfRound.Instance.allPlayerScripts)
                    {
                        float dist = Vector3.Distance(__instance.transform.position, playerControllerB.transform.position);
                        RandomizerModBase.mls.LogError("MINE HAS EXPLODED. DISTANCE TO: " + playerControllerB.playerUsername + " IS: " + dist);

                        if (dist < 3f)
                        {
                            if (playerControllerB == GameNetworkManager.Instance.localPlayerController)
                            {
                                playerControllerB.KillPlayer(playerControllerB.velocityLastFrame, true, CauseOfDeath.Blast);
                                continue;
                            }

                            FastBufferWriter writer = new FastBufferWriter(4, Unity.Collections.Allocator.Temp, -1);
                            Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage("Tibnan.lcrandomizermod_" + "ClientHandlePlayerExploded", playerControllerB.actualClientId, writer, NetworkDelivery.Reliable);
                            playerControllerB.KillPlayer(playerControllerB.velocityLastFrame, true, CauseOfDeath.Blast);
                        }
                    }

                    //PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;
                    //localPlayer.KillPlayer(localPlayer.velocityLastFrame, true, CauseOfDeath.Blast);

                    return true;
                }
            }
            else
            {
                RandomizerModBase.mls.LogInfo("MINE ID: " + __instance.NetworkObjectId);
                return true;
            }
        }

        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        public static void SizeOverride(Landmine __instance)
        {
            if (Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                float scale = Convert.ToSingle(new System.Random().Next(5, 41)) / 10;
                __instance.transform.localScale = new Vector3(scale, scale, scale);

                FastBufferWriter fastBufferWriter = new FastBufferWriter(sizeof(ulong) + sizeof(float), Unity.Collections.Allocator.Temp, -1);

                fastBufferWriter.WriteValueSafe<ulong>(__instance.NetworkObjectId);
                fastBufferWriter.WriteValueSafe<float>(scale);

                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesMineData", fastBufferWriter, NetworkDelivery.Reliable);
            }
        }

        public static void SetMineSizeClient(ulong _, FastBufferReader reader)
        {
            if (!Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                ulong id;
                float scale;

                reader.ReadValueSafe<ulong>(out id);
                reader.ReadValueSafe<float>(out scale);

                NetworkObject networkObject = Unity.Netcode.NetworkManager.Singleton.SpawnManager.SpawnedObjects[id];
                Landmine landmine = networkObject.gameObject.GetComponentInChildren<Landmine>();

                landmine.transform.localScale = new Vector3(scale, scale, scale);
            }
        }
    }
}
