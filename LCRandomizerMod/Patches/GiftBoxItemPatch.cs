using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace LCRandomizerMod.Patches
{

    [HarmonyPatch(typeof(GiftBoxItem))]
    internal class GiftBoxItemPatch
    {
        private protected enum GiftBoxBehaviour { SpawnItem, SpawnEnemy, Explode, None, PlaySound, Teleport, InverseTeleport, SpawnLandmine, GiveGroupCredits, RemoveGroupCredits, 
                                                  ChangeLevelWeather, RandomizePlayerStats, DoubleDeadline, HalveDeadline, DoubleQuota, HalveQuota, TeleportToEntrance, KillEnemiesAround, RecolorPlayer, DoubleHP }

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

            if (new System.Random().Next(0, 2) == 1)
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

                Reroll:
                switch (boxBehaviours[new System.Random().Next(0, boxBehaviours.Length)])
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
                            if (player.isInsideFactory)
                            {
                                RoundManager.Instance.SpawnEnemyOnServer(player.transform.position, 0, new System.Random().Next(0, StartOfRound.Instance.currentLevel.Enemies.Count));
                            }
                            else
                            {
                                if (new System.Random().Next(0, 2) == 0)
                                {
                                    try
                                    {
                                        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(RoundManager.Instance.currentLevel.DaytimeEnemies[new System.Random().Next(0, RoundManager.Instance.currentLevel.DaytimeEnemies.Count)].enemyType.enemyPrefab, player.transform.position, Quaternion.Euler(new Vector3(0f, 0f, 0f)));
                                        gameObject.GetComponentInChildren<NetworkObject>().Spawn(true);
                                        RoundManager.Instance.SpawnedEnemies.Add(gameObject.GetComponentInChildren<EnemyAI>());
                                    } catch (Exception ex)
                                    {
                                        RandomizerModBase.mls.LogError("Error occured when spawning random daytime enemy from giftbox. Rerolling... " + ex.Message);
                                        goto Reroll;
                                    }
                                }
                                else
                                {
                                    try
                                    {
                                        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(RoundManager.Instance.currentLevel.OutsideEnemies[new System.Random().Next(0, RoundManager.Instance.currentLevel.OutsideEnemies.Count)].enemyType.enemyPrefab, player.transform.position, Quaternion.Euler(new Vector3(0f, 0f, 0f)));
                                        gameObject.GetComponentInChildren<NetworkObject>().Spawn(true);
                                        RoundManager.Instance.SpawnedEnemies.Add(gameObject.GetComponentInChildren<EnemyAI>());
                                    } catch (Exception ex)
                                    {
                                        RandomizerModBase.mls.LogError("Error occured when spawning random outside enemy from giftbox. Rerolling... " + ex.Message);
                                        goto Reroll;
                                    }
                                }
                            }

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
                                    teleporters[0].cooldownAmount = 0f;
                                }
                                else if (!teleporters[1].isInverseTeleporter)
                                {
                                    teleporters[1].PressTeleportButtonClientRpc();
                                    teleporters[1].cooldownAmount = 0f;
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
                                else
                                {
                                    goto Reroll;
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
                            int cred = new System.Random().Next(1, 1001);
                            string msg = String.Format("<color=green>Your crew has been given {0} credits! </color>", cred);
                            CustomUI.BroadcastMessage(msg, 2);

                            //HUDManager.Instance.AddTextToChatOnServer(String.Format("<color=green>Your crew has been given {0} credits! </color>", cred), -1);

                            Terminal terminal = GameObject.FindObjectOfType<Terminal>();
                            terminal.groupCredits += cred;
                            terminal.SyncGroupCreditsClientRpc(terminal.groupCredits, terminal.numberOfItemsInDropship);

                            break;
                        }
                    case GiftBoxBehaviour.RemoveGroupCredits:
                        {
                            int cred = new System.Random().Next(1, 1001);
                            CustomUI.BroadcastMessage(String.Format("<color=red>Your crew lost {0} credits! </color>", cred), 2);
                            //HUDManager.Instance.AddTextToChatOnServer(String.Format("<color=red>Your crew lost {0} credits! </color>", cred), -1);

                            Terminal terminal = GameObject.FindObjectOfType<Terminal>();
                            terminal.groupCredits -= cred;
                            terminal.SyncGroupCreditsClientRpc(terminal.groupCredits, terminal.numberOfItemsInDropship);

                            break;
                        }
                    case GiftBoxBehaviour.ChangeLevelWeather:
                        {
                            goto Reroll;
                        }
                    case GiftBoxBehaviour.RandomizePlayerStats:
                        {
                            RandomizePlayerStats(player);
                            break;
                        }
                    case GiftBoxBehaviour.DoubleDeadline:
                        {
                            FastBufferWriter writer = new FastBufferWriter(4, Unity.Collections.Allocator.Temp, -1);
                            Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesDoubleDeadline", writer, NetworkDelivery.Reliable);

                            break;
                        }
                    case GiftBoxBehaviour.HalveDeadline:
                        {
                            FastBufferWriter writer = new FastBufferWriter(4, Unity.Collections.Allocator.Temp, -1);
                            Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesHalveDeadline", writer, NetworkDelivery.Reliable);

                            break;
                        }
                    case GiftBoxBehaviour.DoubleQuota:
                        {
                            FastBufferWriter writer = new FastBufferWriter(4, Unity.Collections.Allocator.Temp, -1);
                            Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesDoubleQuota", writer, NetworkDelivery.Reliable);

                            break;
                        }
                    case GiftBoxBehaviour.HalveQuota:
                        {
                            FastBufferWriter writer = new FastBufferWriter(4, Unity.Collections.Allocator.Temp, -1);
                            Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesHalveQuota", writer, NetworkDelivery.Reliable);

                            break;
                        }
                    case GiftBoxBehaviour.TeleportToEntrance:
                        {
                            if (!player.isInsideFactory)
                            {
                                goto Reroll;
                            }

                            FastBufferWriter writer = new FastBufferWriter(sizeof(ulong), Unity.Collections.Allocator.Temp, -1);
                            writer.WriteValueSafe<ulong>(player.playerClientId);

                            Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesEntranceTP", writer, NetworkDelivery.Reliable);

                            break;
                        }
                    case GiftBoxBehaviour.KillEnemiesAround:
                        {
                            Collider[] array = Physics.OverlapSphere(player.transform.position, 20f, 2621448, QueryTriggerInteraction.Collide);

                            RandomizerModBase.mls.LogError("COLLIDER COUNT: " + array.Length);

                            List<EnemyAI> killCount = new List<EnemyAI>();

                            foreach (Collider collider in array)
                            {
                                EnemyAI enemyAI = collider.gameObject.GetComponentInParent<EnemyAI>();
                                if ((enemyAI != null && enemyAI.isEnemyDead) || enemyAI == null)
                                {
                                    continue;
                                }

                                if (enemyAI != null && enemyAI.enemyType.isOutsideEnemy)
                                {
                                    if (collider.gameObject.GetComponentInParent<RadMechAI>() != null)
                                    {
                                        RandomizerModBase.mls.LogError("Killing enemy ai " + enemyAI.name + " WITH DESTROY");
                                        enemyAI.KillEnemyClientRpc(true);

                                        if (!killCount.Contains(enemyAI))
                                        {
                                            killCount.Add(enemyAI);
                                        }
                                    }
                                    else
                                    {
                                        RandomizerModBase.mls.LogError("Killing enemy ai " + enemyAI.name + " WITHOUT DESTROY");
                                        enemyAI.KillEnemyClientRpc(false);

                                        if (!killCount.Contains(enemyAI))
                                        {
                                            killCount.Add(enemyAI);
                                        }
                                    }
                                }
                                else if (enemyAI != null)
                                {
                                    if (collider.gameObject.GetComponentInParent<SpringManAI>() != null || collider.gameObject.GetComponentInParent<PufferAI>() != null || collider.gameObject.GetComponentInParent<BlobAI>() != null || collider.gameObject.GetComponentInParent<JesterAI>() != null || collider.gameObject.GetComponentInParent<DressGirlAI>() != null || collider.gameObject.GetComponentInParent<ClaySurgeonAI>() != null) 
                                    {
                                        RandomizerModBase.mls.LogError("Killing enemy ai " + enemyAI.name + " WITH DESTROY");
                                        enemyAI.KillEnemyClientRpc(true);

                                        if (!killCount.Contains(enemyAI))
                                        {
                                            killCount.Add(enemyAI);
                                        }
                                    }
                                    else
                                    {
                                        RandomizerModBase.mls.LogError("Killing enemy ai " + enemyAI.name + " WITHOUT DESTROY");
                                        enemyAI.KillEnemyClientRpc(false);

                                        if (!killCount.Contains(enemyAI))
                                        {
                                            killCount.Add(enemyAI);
                                        }
                                    }
                                }

                                if (killCount.Count == 0)
                                {
                                    RandomizerModBase.mls.LogInfo(String.Format("No enemies around {0}, rerolling.", player.playerUsername));
                                    goto Reroll;
                                }
                            }
                            CustomUI.BroadcastMessage(String.Format("<color=white>{0} has been saved by some unknown force. Casualties: {1} {2}.</color>", player.playerUsername, killCount.Count, killCount.Count > 1 ? "enemy" : "enemies"), 2);
                            //HUDManager.Instance.AddTextToChatOnServer();

                            break;
                        }
                    case GiftBoxBehaviour.RecolorPlayer:
                        {
                            CustomUI.BroadcastMessage(String.Format("<color=yellow>A bucket of paint has spilled on {0}</color>", player.playerUsername), 2);
                            RecolorPlayerSync(player);
                            break;
                        }
                    case GiftBoxBehaviour.DoubleHP:
                        {
                            CustomUI.BroadcastMessage(String.Format("<color=green>{0} is feeling vitalized.</color>", player.playerUsername), 2);
                            FastBufferWriter writer = new FastBufferWriter(sizeof(ulong), Unity.Collections.Allocator.Temp, -1);
                            writer.WriteValueSafe<ulong>(player.playerClientId);
                            Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "PlayerHealthDoubled", writer, NetworkDelivery.Reliable);
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

        public static void ClientResetPlayer(ulong _, FastBufferReader reader)
        {
            if (!Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                ulong id;
                float sprintTime;
                float movementSpeed;
                float sinkingSpeed;
                float scale;
                int health;

                reader.ReadValueSafe<ulong>(out id);
                reader.ReadValueSafe<float>(out sprintTime);
                reader.ReadValueSafe<float>(out movementSpeed);
                reader.ReadValueSafe<float>(out sinkingSpeed);
                reader.ReadValueSafe<float>(out scale);
                reader.ReadValueSafe<int>(out health);

                PlayerControllerB player = StartOfRound.Instance.allPlayerScripts[id];

                player.sprintTime = sprintTime;
                player.movementSpeed = movementSpeed;
                player.sinkingSpeedMultiplier = sinkingSpeed;
                player.transform.localScale = new Vector3(scale, scale, scale);
                player.health = health;

                SoundManager.Instance.SetPlayerPitch(scale <= 1 ? Mathf.Lerp(1f, 13f, 1 - (scale - 0.5f) * 2) : Mathf.Lerp(0.7f, 1f, 1 - (scale - 1f) * 2), (int)player.playerClientId);
            }
        }

        public static void DoubleDeadline(ulong _, FastBufferReader __)
        {
            TimeOfDay.Instance.timeUntilDeadline *= 2;

            int daysUntilDeadline = (int)Mathf.Floor(TimeOfDay.Instance.timeUntilDeadline / TimeOfDay.Instance.totalTime);
            HUDManager.Instance.DisplayDaysLeft(daysUntilDeadline);
            StartOfRound.Instance.deadlineMonitorText.text = string.Format("DEADLINE:\n{0} Days", daysUntilDeadline);
        }

        public static void HalveDeadline(ulong _, FastBufferReader __)
        {
            TimeOfDay.Instance.timeUntilDeadline /= 2;

            int daysUntilDeadline = TimeOfDay.Instance.timeUntilDeadline < 1080 ? 0 : (int)Mathf.Floor(TimeOfDay.Instance.timeUntilDeadline / TimeOfDay.Instance.totalTime);
            TimeOfDay.Instance.daysUntilDeadline = daysUntilDeadline;

            HUDManager.Instance.DisplayDaysLeft(daysUntilDeadline);
            StartOfRound.Instance.deadlineMonitorText.text = string.Format("DEADLINE:\n{0} Days", daysUntilDeadline);
        }

        public static void DoubleQuota(ulong _, FastBufferReader __)
        {
            TimeOfDay.Instance.profitQuota *= 2;

            NetworkManager.Singleton.StartCoroutine(RackUpQuotaTextCoroutine());
        }

        public static void HalveQuota(ulong _, FastBufferReader __)
        {
            TimeOfDay.Instance.profitQuota /= 2;

            NetworkManager.Singleton.StartCoroutine(LowerQuotaTextCoroutine());
        }

        private static IEnumerator RackUpQuotaTextCoroutine()
        {
            HUDManager.Instance.AddTextToChatOnServer("<color=red>Rolling quota...</color>", -1);
            Vector3 defPos = HUDManager.Instance.reachedProfitQuotaAnimator.transform.position;
            HUDManager.Instance.reachedProfitQuotaAnimator.transform.position = new Vector3(-1000, -1000, -1000);
            HUDManager.Instance.reachedProfitQuotaAnimator.SetBool("display", true);

            yield return new WaitForSeconds(3.5f);
            HUDManager.Instance.reachedProfitQuotaAnimator.transform.position = defPos;
            int quotaTextAmount = TimeOfDay.Instance.profitQuota / 2;
            while (quotaTextAmount < TimeOfDay.Instance.profitQuota)
            {
                quotaTextAmount = (int)Mathf.Clamp((float)quotaTextAmount + Time.deltaTime * 250f, (float)(quotaTextAmount + 3), (float)(TimeOfDay.Instance.profitQuota + 10));
                HUDManager.Instance.newProfitQuotaText.text = "$" + quotaTextAmount.ToString();
                yield return null;
            }
            HUDManager.Instance.newProfitQuotaText.text = "$" + TimeOfDay.Instance.profitQuota.ToString();
            TimeOfDay.Instance.UpdateProfitQuotaCurrentTime();
            HUDManager.Instance.UIAudio.PlayOneShot(HUDManager.Instance.newProfitQuotaSFX);
            yield return new WaitForSeconds(1.25f);
            HUDManager.Instance.displayingNewQuota = false;
            HUDManager.Instance.reachedProfitQuotaAnimator.SetBool("display", false);
            yield break;
        }

        private static IEnumerator LowerQuotaTextCoroutine()
        {
            HUDManager.Instance.AddTextToChatOnServer("<color=yellow>Rolling quota...</color>", -1);
            Vector3 defPos = HUDManager.Instance.reachedProfitQuotaAnimator.transform.position;
            HUDManager.Instance.reachedProfitQuotaAnimator.transform.position = new Vector3(-1000, -1000, -1000);
            HUDManager.Instance.reachedProfitQuotaAnimator.SetBool("display", true);

            yield return new WaitForSeconds(3.5f);
            HUDManager.Instance.reachedProfitQuotaAnimator.transform.position = defPos;
            int quotaTextAmount = TimeOfDay.Instance.profitQuota * 2;
            RandomizerModBase.mls.LogError("quotaTextAmount: " + quotaTextAmount + " TimeOfDay.Instance: " + TimeOfDay.Instance.profitQuota);
            while (quotaTextAmount > TimeOfDay.Instance.profitQuota)
            {
                quotaTextAmount = (int)Mathf.Clamp((float)quotaTextAmount - Time.deltaTime * 250f, (float)(TimeOfDay.Instance.profitQuota - 10), (float)(quotaTextAmount - 3));
                HUDManager.Instance.newProfitQuotaText.text = "$" + quotaTextAmount.ToString();
                yield return null;
            }
            HUDManager.Instance.newProfitQuotaText.text = "$" + TimeOfDay.Instance.profitQuota.ToString();
            TimeOfDay.Instance.UpdateProfitQuotaCurrentTime();
            HUDManager.Instance.UIAudio.PlayOneShot(HUDManager.Instance.newProfitQuotaSFX);
            yield return new WaitForSeconds(1.25f);
            HUDManager.Instance.displayingNewQuota = false;
            HUDManager.Instance.reachedProfitQuotaAnimator.SetBool("display", false);
            yield break;
        }

        public static void TeleportPlayerToEntrance(ulong _, FastBufferReader reader)
        {
            ulong playerID;
            reader.ReadValueSafe<ulong>(out playerID);

            if (playerID != GameNetworkManager.Instance.localPlayerController.playerClientId)
            {
                return;
            }

            PlayerControllerB player = GameNetworkManager.Instance.localPlayerController;

            int id = 0;
            foreach (EntranceTeleport entrance in UnityEngine.Object.FindObjectsByType<EntranceTeleport>(FindObjectsSortMode.None))
            {
                if (entrance.entranceId == id && player.isInsideFactory != entrance.isEntranceToBuilding)
                {
                    entrance.TeleportPlayer();
                    break;
                }
            }
        }

        public static void RecolorPlayerSync(PlayerControllerB player)
        {
            float r = new System.Random().Next(0, 200) / 100f;
            float g = new System.Random().Next(0, 200) / 100f;
            float b = new System.Random().Next(0, 200) / 100f;

            player.thisPlayerModel.material.color = new Color(r, g, b);
            player.thisPlayerModelLOD1.material.color = new Color(r, g, b);
            player.thisPlayerModelLOD2.material.color = new Color(r, g, b);
            player.thisPlayerModelArms.material.color = new Color(r, g, b);

            FastBufferWriter writer = new FastBufferWriter(sizeof(ulong) + sizeof(float) * 3, Unity.Collections.Allocator.Temp, -1);
            writer.WriteValueSafe<ulong>(player.playerClientId);
            writer.WriteValueSafe<float>(r);
            writer.WriteValueSafe<float>(g);
            writer.WriteValueSafe<float>(b);

            Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesPlayerRecolor", writer, NetworkDelivery.Reliable);
        }

        public static void SetPlayerColor(ulong _, FastBufferReader reader)
        {
            if (!Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                ulong id;
                float r;
                float g;
                float b;

                reader.ReadValueSafe<ulong>(out id);
                reader.ReadValueSafe<float>(out r);
                reader.ReadValueSafe<float>(out g);
                reader.ReadValueSafe<float>(out b);

                PlayerControllerB player = StartOfRound.Instance.allPlayerScripts[id];

                player.thisPlayerModel.material.color = new Color(r, g, b);
                player.thisPlayerModelLOD1.material.color = new Color(r, g, b);
                player.thisPlayerModelLOD2.material.color = new Color(r, g, b);
                player.thisPlayerModelArms.material.color = new Color(r, g, b);
            }
        }

        public static void RandomizePlayerStats(PlayerControllerB player)
        {
            player.sprintTime = Convert.ToSingle(new System.Random().Next(1, 101)) / 10;
            player.movementSpeed = new System.Random().Next(30, 101) / 10;
            player.sinkingSpeedMultiplier = new System.Random().Next(100, 10000) / 10;
            player.health = new System.Random().Next(1, 201);

            float scale = Convert.ToSingle(new System.Random().Next(5, 16)) / 10;

            player.transform.localScale = new Vector3(scale, scale, scale);
            SoundManager.Instance.SetPlayerPitch(scale <= 1 ? Mathf.Lerp(1f, 13f, 1 - (scale - 0.5f) * 2) : Mathf.Lerp(0.7f, 1f, 1 - (scale - 1f) * 2), (int)player.playerClientId);

            FastBufferWriter writer = new FastBufferWriter(sizeof(ulong) + sizeof(float) * 4 + sizeof(int), Unity.Collections.Allocator.Temp, -1);

            writer.WriteValueSafe<ulong>(player.playerClientId);
            writer.WriteValueSafe<float>(player.sprintTime);
            writer.WriteValueSafe<float>(player.movementSpeed);
            writer.WriteValueSafe<float>(player.sinkingSpeedMultiplier);
            writer.WriteValueSafe<float>(scale);
            writer.WriteValueSafe<int>(player.health);

            Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesResetPlayer", writer, NetworkDelivery.Reliable);
        }

        public static void DoublePlayerHP(ulong _, FastBufferReader reader)
        {
            ulong id;
            
            reader.ReadValueSafe<ulong>(out id);

            PlayerControllerB player = StartOfRound.Instance.allPlayerScripts[id];
            player.health *= 2;
        }
    }
}
