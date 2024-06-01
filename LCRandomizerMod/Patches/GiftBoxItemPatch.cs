using GameNetcodeStuff;
using HarmonyLib;
using LethalLib.Modules;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace LCRandomizerMod.Patches
{

    [HarmonyPatch(typeof(GiftBoxItem))]
    internal class GiftBoxItemPatch
    {
        private protected static Predicate<AudioClip> teleporterBeamUpPredicate = (audioClip) => audioClip.name.Contains("teleporterBeamUpSFX");
        private protected enum GiftBoxBehaviour { SpawnItem, SpawnEnemy, Explode, None, PlaySound, Teleport, InverseTeleport, SpawnLandmine, GiveGroupCredits, 
                                                  ChangeLevelWeather, RandomizePlayerStats, DoubleDeadline, HalveDeadline, DoubleQuota, HalveQuota, TeleportToEntrance, KillEnemiesAround }

        [HarmonyPatch(nameof(GiftBoxItem.OpenGiftBoxServerRpc))]
        [HarmonyPrefix]
        public static bool RandomBehaviourOnOpen(GiftBoxItem __instance)
        {
            if (!Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                FastBufferWriter writer = new FastBufferWriter(sizeof(ulong), Unity.Collections.Allocator.Temp, -1);
                writer.WriteValueSafe<ulong>(__instance.NetworkObjectId);

                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage("Tibnan.lcrandomizermod_" + "ServerReceivesGiftboxInteraction", 0UL, writer, NetworkDelivery.Reliable);

                __instance.PoofParticle.Play();
                __instance.presentAudio.PlayOneShot(__instance.openGiftAudio);
                WalkieTalkie.TransmitOneShotAudio(__instance.presentAudio, __instance.openGiftAudio, 1f);
                RoundManager.Instance.PlayAudibleNoise(__instance.presentAudio.transform.position, 8f, 0.5f, 0, __instance.isInShipRoom && StartOfRound.Instance.hangarDoorsClosed, 0);
                if (__instance.playerHeldBy != null)
                {
                    __instance.playerHeldBy.activatingItem = false;
                    __instance.DestroyObjectInHand(__instance.playerHeldBy);
                }

                return false;
            }

            if (new System.Random().Next(1, 2) == 1)
            {
                PlayerControllerB player = __instance.playerHeldBy;

                __instance.PoofParticle.Play();
                __instance.presentAudio.PlayOneShot(__instance.openGiftAudio);
                WalkieTalkie.TransmitOneShotAudio(__instance.presentAudio, __instance.openGiftAudio, 1f);
                RoundManager.Instance.PlayAudibleNoise(__instance.presentAudio.transform.position, 8f, 0.5f, 0, __instance.isInShipRoom && StartOfRound.Instance.hangarDoorsClosed, 0);
                if (__instance.playerHeldBy != null)
                {
                    __instance.playerHeldBy.activatingItem = false;
                    __instance.DestroyObjectInHand(__instance.playerHeldBy);
                }

                Traverse.Create(__instance).Field("objectInPresent").SetValue(null);
                GiftBoxBehaviour[] boxBehaviours = Enum.GetValues(typeof(GiftBoxBehaviour)) as GiftBoxBehaviour[];
                //boxBehaviours[new System.Random().Next(0, boxBehaviours.Length)]   <--- switch
                Reroll:

                switch (GiftBoxBehaviour.InverseTeleport)
                {
                    case GiftBoxBehaviour.SpawnItem:
                        {
                            GameObject item = RandomizerValues.allItemsListDict.ElementAt(new System.Random().Next(0, RandomizerValues.allItemsListDict.Count)).Value.spawnPrefab;

                            GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(item, __instance.transform.position, Quaternion.identity, RoundManager.Instance.spawnedScrapContainer);
                            gameObject.GetComponent<GrabbableObject>().fallTime = 0f;
                            gameObject.GetComponent<NetworkObject>().Spawn(false);

                            if (gameObject.GetComponent<GrabbableObject>() != null)
                            {
                                gameObject.GetComponent<GrabbableObject>().SetScrapValue(0);
                            }

                            RandomizerModBase.mls.LogWarning("SPAWNED " + item.name);

                            break;
                        }
                    case GiftBoxBehaviour.SpawnEnemy:
                        {
                            break;
                        }
                    case GiftBoxBehaviour.Explode:
                        {
                            Landmine.SpawnExplosion(__instance.transform.position, true, 1, 1, 50, 2);

                            __instance.GetComponentInChildren<NetworkObject>().Despawn(true);

                            FastBufferWriter writer = new FastBufferWriter(sizeof(float) * 3, Unity.Collections.Allocator.Temp, -1);
                            writer.WriteValueSafe<float>(__instance.transform.position.x);
                            writer.WriteValueSafe<float>(__instance.transform.position.y);
                            writer.WriteValueSafe<float>(__instance.transform.position.z);

                            Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesExplodedGiftbox", writer, NetworkDelivery.Reliable);
                            break;
                        }
                    case GiftBoxBehaviour.None:
                        {
                            break;
                        }
                    case GiftBoxBehaviour.PlaySound:
                        {
                            int idx = new System.Random().Next(0, RandomizerValues.audioDict.Count);
                            AudioClip clip = RandomizerValues.audioDict.ElementAt(idx).Value;
                            RandomizerModBase.mls.LogError("Playing " + clip.name);
                            __instance.presentAudio.PlayOneShot(clip);

                            FastBufferWriter writer = new FastBufferWriter(sizeof(ulong) + sizeof(int), Unity.Collections.Allocator.Temp, -1);
                            writer.WriteValueSafe<ulong>(__instance.NetworkObjectId);
                            writer.WriteValueSafe<int>(idx);

                            Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesPlayedSoundGift", writer, NetworkDelivery.Reliable);
                            break;
                        }
                    case GiftBoxBehaviour.Teleport:
                        {
                            RandomizerModBase.mls.LogError("PlayerHeldBy: " + player.playerUsername + " " + player.actualClientId);
                            RandomizerModBase.mls.LogError("local: " + GameNetworkManager.Instance.localPlayerController.playerUsername + " " + GameNetworkManager.Instance.localPlayerController.actualClientId);

                            ShipTeleporter[] teleporters = GameObject.FindObjectsOfType<ShipTeleporter>();

                            if (teleporters.Length > 0)
                            {
                                StartOfRound.Instance.mapScreen.targetedPlayer = player;

                                FastBufferWriter writer = new FastBufferWriter(sizeof(ulong), Unity.Collections.Allocator.Temp, -1);
                                writer.WriteValueSafe<ulong>(player.actualClientId);
                                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientSetTeleportPlayerGift", writer, NetworkDelivery.Reliable);

                                if (!teleporters[0].isInverseTeleporter)
                                {
                                    teleporters[0].PressTeleportButtonClientRpc();
                                }
                                else if (!teleporters[1].isInverseTeleporter)
                                {
                                    teleporters[1].PressTeleportButtonClientRpc();
                                }
                            }
                            else
                            {
                                goto Reroll;
                            }

                            break;
                        }
                    case GiftBoxBehaviour.InverseTeleport:
                        {
                            ShipTeleporter[] teleporters = GameObject.FindObjectsOfType<ShipTeleporter>();

                            if (teleporters.Length > 0)
                            {
                                if (teleporters[0].isInverseTeleporter || teleporters[1] != null && teleporters[1].isInverseTeleporter)
                                {
                                    ShipTeleporter inverseTeleporter = teleporters[0].isInverseTeleporter ? teleporters[0] : teleporters[1];

                                    System.Random shipTeleporterSeed = Traverse.Create(inverseTeleporter).Field("shipTeleporterSeed").GetValue() as System.Random;

                                    Vector3 vector = RoundManager.Instance.insideAINodes[shipTeleporterSeed.Next(0, RoundManager.Instance.insideAINodes.Length)].transform.position;
                                    vector = RoundManager.Instance.GetRandomNavMeshPositionInBoxPredictable(vector, 10f, default(NavMeshHit), shipTeleporterSeed, -1);

                                    GameNetworkManager.Instance.StartCoroutine(TeleportPlayerCoroutine(player, vector));

                                    FastBufferWriter writer = new FastBufferWriter(sizeof(ulong) + sizeof(float) * 3, Unity.Collections.Allocator.Temp, -1);
                                    RandomizerModBase.mls.LogError("PLAYER CLIENT ID: " + player.playerClientId);
                                    writer.WriteValueSafe<ulong>(player.playerClientId);
                                    writer.WriteValueSafe<float>(vector.x);
                                    writer.WriteValueSafe<float>(vector.y);
                                    writer.WriteValueSafe<float>(vector.z);

                                    Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesPlayerTeleport", writer, NetworkDelivery.Reliable);
                                }
                            }
                            else
                            {
                                goto Reroll;
                            }

                            break;
                        }
                    case GiftBoxBehaviour.SpawnLandmine:
                        {
                            foreach (SpawnableMapObject obj in RoundManager.Instance.currentLevel.spawnableMapObjects)
                            {
                                if (obj.prefabToSpawn.name == "Landmine")
                                {
                                    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(obj.prefabToSpawn, player.transform.position, Quaternion.identity);
                                    gameObject.GetComponent<NetworkObject>().Spawn(true);
                                }
                            }

                            break;
                        }
                    case GiftBoxBehaviour.GiveGroupCredits:
                        {
                            break;
                        }
                    case GiftBoxBehaviour.ChangeLevelWeather:
                        {
                            break;
                        }
                    case GiftBoxBehaviour.RandomizePlayerStats:
                        {
                            break;
                        }
                    case GiftBoxBehaviour.DoubleDeadline:
                        {
                            break;
                        }
                    case GiftBoxBehaviour.HalveDeadline:
                        {
                            break;
                        }
                    case GiftBoxBehaviour.DoubleQuota:
                        {
                            break;
                        }
                    case GiftBoxBehaviour.HalveQuota:
                        {
                            break;
                        }
                    case GiftBoxBehaviour.TeleportToEntrance:
                        {
                            break;
                        }
                    case GiftBoxBehaviour.KillEnemiesAround:
                        {
                            break;
                        }
                }

                GameNetworkManager.Instance.StartCoroutine(WaitForGiftToBeDiscarded(__instance));

                return false;
            }
            else
            {
                return true;
            }
        }
        
        public static void GiftboxHasExplodedClient(ulong _, FastBufferReader reader)
        {
            if (!Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                float x;
                float y;
                float z;

                reader.ReadValueSafe<float>(out x);
                reader.ReadValueSafe<float>(out y);
                reader.ReadValueSafe<float>(out z);

                Landmine.SpawnExplosion(new Vector3(x, y, z), true, 1, 1, 50, 2);
            }
        }

        public static void PlaySoundGiftboxClient(ulong _, FastBufferReader reader)
        {
            if (!Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                ulong id;
                int idx;

                reader.ReadValueSafe<ulong>(out id);
                reader.ReadValueSafe<int>(out idx);

                RandomizerModBase.mls.LogError("Playing " + RandomizerValues.audioDict.ElementAt(idx).Value.name);

                NetworkObject networkObject = Unity.Netcode.NetworkManager.Singleton.SpawnManager.SpawnedObjects[id];
                GiftBoxItem giftBox = networkObject.gameObject.GetComponentInChildren<GiftBoxItem>();

                giftBox.presentAudio.PlayOneShot(RandomizerValues.audioDict.ElementAt(idx).Value);
            }
        }

        public static void ServerReceivesGiftboxInteraction(ulong _, FastBufferReader reader)
        {
            ulong id;
            reader.ReadValueSafe<ulong>(out id);

            NetworkObject networkObject = Unity.Netcode.NetworkManager.Singleton.SpawnManager.SpawnedObjects[id];
            GiftBoxItem gift = networkObject.gameObject.GetComponentInChildren<GiftBoxItem>();

            RandomBehaviourOnOpen(gift);
        }

        public static void SetTeleportPlayer(ulong _, FastBufferReader reader)
        {
            if (!Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                ulong id;
                reader.ReadValueSafe<ulong>(out id);

                RandomizerModBase.mls.LogError("SET TELEPORT TARGET: " + id);
                StartOfRound.Instance.mapScreen.targetedPlayer = StartOfRound.Instance.allPlayerScripts[id];
            }
        }

        public static void ClientTeleportPlayer(ulong _, FastBufferReader reader)
        {
            if (!Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                ulong id;

                reader.ReadValueSafe<ulong>(out id);
                
                if (id != GameNetworkManager.Instance.localPlayerController.playerClientId)
                {
                    return;
                }

                float x;
                float y;
                float z;

                reader.ReadValueSafe<float>(out x);
                reader.ReadValueSafe<float>(out y);
                reader.ReadValueSafe<float>(out z);

                GameNetworkManager.Instance.StartCoroutine(TeleportPlayerCoroutine(GameNetworkManager.Instance.localPlayerController, new Vector3(x, y, z)));
            }
        }

        private static IEnumerator TeleportPlayerCoroutine(PlayerControllerB player, Vector3 pos)
        {
            player.DropAllHeldItems(true, false);
            player.isInElevator = false;
            player.isInHangarShipRoom = false;
            player.isInsideFactory = true;
            player.averageVelocity = 0f;
            player.velocityLastFrame = Vector3.zero;
            player.beamOutBuildupParticle.Play();
            player.movementAudio.PlayOneShot(RandomizerValues.audioDict.GetValueSafe("ShipTeleporterBeamPlayerBody"));

            yield return new WaitWhile(() => player.beamOutBuildupParticle.isPlaying);

            player.TeleportPlayer(pos, false, 0f, false, true);
            if (player.beamOutParticle != null)
            {
                player.beamOutParticle.Play();
            }

            GameNetworkManager.Instance.localPlayerController.movementAudio.PlayOneShot(RandomizerValues.audioDict.GetValueSafe("ShipTeleporterBeam"));
        }

        private static IEnumerator WaitForGiftToBeDiscarded(GiftBoxItem __instance)
        {
            yield return new WaitForSeconds(0.5f);
            NetworkManager.Singleton.SpawnManager.SpawnedObjects[__instance.NetworkObjectId].Despawn(true);
        }
    }
}
