using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace LCRandomizerMod.Patches
{
    public enum ChemicalEffects { ShrinkPlayer, HealPlayer, MakePlayerGlow, DamagePlayer }

    [HarmonyPatch(typeof(TetraChemicalItem))]
    internal class TetraChemicalItemPatch : ICustomValue
    {
        public static bool inverseCoroutineRunning = false;
        public static Coroutine inverseCoroutine;

        [HarmonyPatch(nameof(TetraChemicalItem.EquipItem))]
        [HarmonyPostfix]
        public static void RandomlyAssignCoroutine(TetraChemicalItem __instance)
        {
            if (!RandomizerValues.chemicalEffectsDict.ContainsKey(__instance.NetworkObjectId) && NetworkManager.Singleton.IsServer)
            {
                ChemicalEffects[] effects = Enum.GetValues(typeof(ChemicalEffects)) as ChemicalEffects[];

                ChemicalEffects effect = effects[new System.Random().Next(0, effects.Length)];
                RandomizerValues.chemicalEffectsDict.Add(__instance.NetworkObjectId, effect);

                switch (effect)
                {
                    case ChemicalEffects.ShrinkPlayer:
                        {
                            __instance.mainObjectRenderer.material.color = new Color(0, 0, 0.5f);
                            break;
                        }
                    case ChemicalEffects.HealPlayer:
                        {
                            __instance.mainObjectRenderer.material.color = new Color(0.5f, 0, 0);
                            break;
                        }
                    case ChemicalEffects.MakePlayerGlow:
                        {
                            __instance.mainObjectRenderer.material.color = new Color(0.5f, 0.5f, 0);
                            break;
                        }
                    case ChemicalEffects.DamagePlayer:
                        {
                            __instance.mainObjectRenderer.material.color = new Color(0.5f, 0, 0.5f);
                            break;
                        }
                }

                FastBufferWriter writer = new FastBufferWriter(sizeof(ulong) + sizeof(float) * 3 + sizeof(ChemicalEffects), Unity.Collections.Allocator.Temp, -1);
                writer.WriteValueSafe<ulong>(__instance.NetworkObjectId);
                writer.WriteValueSafe<float>(__instance.mainObjectRenderer.material.color.r);
                writer.WriteValueSafe<float>(__instance.mainObjectRenderer.material.color.g);
                writer.WriteValueSafe<float>(__instance.mainObjectRenderer.material.color.b);
                writer.WriteValueSafe<ChemicalEffects>(effect);

                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesChemColor", writer, NetworkDelivery.Reliable);
            }
        }

        [HarmonyPatch(nameof(TetraChemicalItem.ItemActivate))]
        [HarmonyPostfix]
        public static void StartRandomizedCoroutine(TetraChemicalItem __instance, ref bool used, ref bool buttonDown)
        {
            //bal klikk: false, true
            //jobb klikk: true, false
            if (buttonDown)
            {
                __instance.playerHeldBy.playerBodyAnimator.SetBool("useTZPItem", buttonDown);
                switch (RandomizerValues.chemicalEffectsDict.GetValueSafe(__instance.NetworkObjectId))
                {
                    case ChemicalEffects.ShrinkPlayer:
                        {
                            if (!RandomizerValues.coroutineStorage.ContainsKey(__instance.playerHeldBy.playerClientId))
                            {
                                RandomizerValues.coroutineStorage.Add(__instance.playerHeldBy.playerClientId, __instance.playerHeldBy.transform.localScale);
                            }
                            if (inverseCoroutineRunning)
                            {
                                GameNetworkManager.Instance.StopCoroutine(inverseCoroutine);
                                inverseCoroutineRunning = false;
                                RandomizerModBase.mls.LogError("STOPPING INVERSE COROUTINE, AND STARTING SHRINK COROUTINE");
                            }
                            GameNetworkManager.Instance.StartCoroutine(ShrinkPlayerCoroutine(__instance.playerHeldBy, __instance));
                            break;
                        }
                    case ChemicalEffects.HealPlayer:
                        {
                            GameNetworkManager.Instance.StartCoroutine(HealPlayerCoroutine(__instance.playerHeldBy, __instance));
                            break;
                        }
                    case ChemicalEffects.MakePlayerGlow:
                        {
                            if (inverseCoroutineRunning)
                            {
                                GameNetworkManager.Instance.StopCoroutine(inverseCoroutine);
                                inverseCoroutineRunning = false;
                                RandomizerModBase.mls.LogError("STOPPING INVERSE COROUTINE, AND STARTING GLOW COROUTINE");
                            }
                            GameNetworkManager.Instance.StartCoroutine(AddGlowTimeCoroutine(__instance.playerHeldBy, __instance));
                            break;
                        }
                    case ChemicalEffects.DamagePlayer:
                        {
                            RandomizerValues.blockAnims = true;
                            GameNetworkManager.Instance.StartCoroutine(DamagePlayerCoroutine(__instance.playerHeldBy, __instance));
                            break;
                        }
                }
            }
            else
            {
                switch (RandomizerValues.chemicalEffectsDict.GetValueSafe(__instance.NetworkObjectId))
                {
                    case ChemicalEffects.ShrinkPlayer:
                        {
                            if (!inverseCoroutineRunning)
                            {
                                inverseCoroutine = GameNetworkManager.Instance.StartCoroutine(ShrinkPlayerCoroutine(__instance.playerHeldBy, __instance, true));
                                RandomizerModBase.mls.LogError("RUNNING INVERSE COROUTINE");
                                inverseCoroutineRunning = true;
                            }
                            else
                            {
                                RandomizerModBase.mls.LogWarning("TRIED TO START INVERSE COROUTINE");
                            }
                            break;
                        }
                    case ChemicalEffects.MakePlayerGlow:
                        {
                            if (!inverseCoroutineRunning)
                            {
                                inverseCoroutine = GameNetworkManager.Instance.StartCoroutine(AddGlowTimeCoroutine(__instance.playerHeldBy, __instance, true));
                                RandomizerModBase.mls.LogError("RUNNING INVERSE COROUTINE");
                                inverseCoroutineRunning = true;
                            }
                            else
                            {
                                RandomizerModBase.mls.LogWarning("TRIED TO START INVERSE COROUTINE");
                            }
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }
        }

        public static IEnumerator ShrinkPlayerCoroutine(PlayerControllerB playerToShrink, TetraChemicalItem __instance, bool isInverse = false)
        {
            if (isInverse)
            {
                yield return new WaitForSeconds(10f);
                Vector3 origPlayerScale = (Vector3)RandomizerValues.coroutineStorage.GetValueSafe(playerToShrink.playerClientId);
                while (!__instance.isBeingUsed && playerToShrink.thisPlayerBody.localScale.x < origPlayerScale.x)
                {
                    playerToShrink.thisPlayerBody.localScale += new Vector3(0.001f, 0.001f, 0.001f);
                    playerToShrink.playerGlobalHead.localScale += new Vector3(0.001f, 0.001f, 0.001f);
                    yield return null;
                }
                if (playerToShrink.thisPlayerBody.localScale == origPlayerScale) 
                {
                    inverseCoroutineRunning = false;
                    RandomizerValues.coroutineStorage.Remove(playerToShrink.playerClientId);
                } 
            }
            else
            {
                while (__instance.isBeingUsed && playerToShrink.thisPlayerBody.localScale.x > 0.1f && Traverse.Create(__instance).Field("fuel").GetValue<float>() > 0f)
                {
                    playerToShrink.thisPlayerBody.localScale -= new Vector3(0.001f, 0.001f, 0.001f);
                    playerToShrink.playerGlobalHead.localScale -= new Vector3(0.001f, 0.001f, 0.001f);
                    yield return null;
                }
            }
        }

        public static IEnumerator HealPlayerCoroutine(PlayerControllerB playerToHeal, TetraChemicalItem __instance)
        {
            while (__instance.isBeingUsed && playerToHeal.health < RandomizerValues.currentMaxHP && Traverse.Create(__instance).Field("fuel").GetValue<float>() > 0f)
            {
                playerToHeal.health++;
                yield return null;
            }
        }

        public static IEnumerator AddGlowTimeCoroutine(PlayerControllerB playerToGlow, TetraChemicalItem __instance, bool isInverse = false)
        {
            Light playerLight = RandomizerValues.playerLightsDict.GetValueSafe(playerToGlow.playerClientId);

            if (isInverse)
            {
                yield return new WaitForSeconds(10f);
                while (!__instance.isBeingUsed && playerLight.intensity > 0f)
                {
                    playerLight.intensity -= 0.1f;
                    RandomizerModBase.mls.LogError("PLAYER LIGHT INTENSITY: " + playerLight.intensity);
                    yield return null;
                }
            }
            else
            {
                while (__instance.isBeingUsed && playerLight.intensity < 100f && Traverse.Create(__instance).Field("fuel").GetValue<float>() > 0f)
                {
                    playerLight.intensity += 0.1f;
                    RandomizerModBase.mls.LogError("PLAYER LIGHT INTENSITY: " + playerLight.intensity);
                    yield return null;
                }
            }
            yield break;
        }

        public static IEnumerator DamagePlayerCoroutine(PlayerControllerB playerToDamage, TetraChemicalItem __instance)
        {
            while (__instance.isBeingUsed && playerToDamage.health > 0 && Traverse.Create(__instance).Field("fuel").GetValue<float>() > 0f)
            {
                playerToDamage.DamagePlayer(1);
                yield return new WaitForSeconds(0.3f);
            }
            RandomizerValues.blockAnims = false;
        }

        public static void ClientSetChemColor(ulong _, FastBufferReader reader)
        {
            if (!Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                ulong id;
                float r;
                float g;
                float b;
                ChemicalEffects effect;

                reader.ReadValueSafe<ulong>(out id);

                if (RandomizerValues.chemicalEffectsDict.ContainsKey(id)) return;

                reader.ReadValueSafe<float>(out r);
                reader.ReadValueSafe<float>(out g);
                reader.ReadValueSafe<float>(out b);
                reader.ReadValueSafe<ChemicalEffects>(out effect);

                NetworkObject networkObject = Unity.Netcode.NetworkManager.Singleton.SpawnManager.SpawnedObjects[id];
                TetraChemicalItem chemicalItem = networkObject.gameObject.GetComponentInChildren<TetraChemicalItem>();

                chemicalItem.mainObjectRenderer.material.color = new Color(r, g, b);

                RandomizerValues.chemicalEffectsDict.Add(id, effect);
            }
        }

        public void SaveOnExit()
        {
            if (RandomizerValues.chemicalEffectsDict.Count > 0)
            {
                RandomizerModBase.mls.LogWarning(String.Format("Saving {0} tzp chemical entries", RandomizerValues.chemicalEffectsDict.Count));
                try
                {
                    ES3.Save("tzpChemDict", RandomizerValues.chemicalEffectsDict, GameNetworkManager.Instance.currentSaveFileName);
                    if (!RandomizerValues.keysToLoad.Contains("tzpChemDict"))
                    {
                        RandomizerValues.keysToLoad.Add("tzpChemDict");
                    }
                }
                catch (Exception ex)
                {
                    RandomizerModBase.mls.LogError("Exception caught during custom value serialization. [TetraChemicalItem] " + ex.Message);
                }
            }
            else if (RandomizerValues.keysToLoad.Contains("tzpChemDict"))
            {
                RandomizerValues.keysToLoad.Remove("tzpChemDict");
            }
        }

        public void ReloadStats()
        {
            if (RandomizerValues.chemicalEffectsDict.Count > 0)
            {
                int idx = 0;
                RandomizerModBase.mls.LogInfo(String.Format("Reloading {0} tzp effects from dictionary.", RandomizerValues.chemicalEffectsDict.Count));
                List<ChemicalEffects> temp = RandomizerValues.chemicalEffectsDict.Values.ToList();
                RandomizerValues.chemicalEffectsDict.Clear();

                List<UnityEngine.Object> tzpItemsInLevel = GameObject.FindObjectsByType(typeof(TetraChemicalItem), FindObjectsSortMode.None).ToList();

                foreach (UnityEngine.Object obj in tzpItemsInLevel)
                {
                    TetraChemicalItem tzpChemical = (TetraChemicalItem)obj;

                    RandomizerModBase.mls.LogInfo(tzpChemical.NetworkObjectId);

                    if (idx >= temp.Count) break;

                    ChemicalEffects effect = temp.ElementAt(idx);

                    switch (effect)
                    {
                        case ChemicalEffects.ShrinkPlayer:
                            {
                                tzpChemical.mainObjectRenderer.material.color = new Color(0, 0, 0.5f);
                                break;
                            }
                        case ChemicalEffects.HealPlayer:
                            {
                                tzpChemical.mainObjectRenderer.material.color = new Color(0.5f, 0, 0);
                                break;
                            }
                        case ChemicalEffects.MakePlayerGlow:
                            {
                                tzpChemical.mainObjectRenderer.material.color = new Color(0.5f, 0.5f, 0);
                                break;
                            }
                        case ChemicalEffects.DamagePlayer:
                            {
                                tzpChemical.mainObjectRenderer.material.color = new Color(0.5f, 0, 0.5f);
                                break;
                            }
                    }

                    RandomizerValues.chemicalEffectsDict.Add(tzpChemical.NetworkObjectId, temp.ElementAt(idx));
                    idx++;
                }

                RandomizerModBase.mls.LogInfo("Reloaded tzp effects from dictionary.");
            }
            else
            {
                RandomizerModBase.mls.LogInfo("No tzp effects to reload.");
            }
        }

        public void SyncStatsWithClients()
        {
            foreach (KeyValuePair<ulong, ChemicalEffects> pair in RandomizerValues.chemicalEffectsDict)
            {
                FastBufferWriter writer = new FastBufferWriter(sizeof(ulong) + sizeof(float) * 3 + sizeof(ChemicalEffects), Unity.Collections.Allocator.Temp, -1);
                writer.WriteValueSafe<ulong>(pair.Key);

                NetworkObject networkObject = Unity.Netcode.NetworkManager.Singleton.SpawnManager.SpawnedObjects[pair.Key];
                TetraChemicalItem chemicalItem = networkObject.gameObject.GetComponentInChildren<TetraChemicalItem>();

                writer.WriteValueSafe<float>(chemicalItem.mainObjectRenderer.material.color.r);
                writer.WriteValueSafe<float>(chemicalItem.mainObjectRenderer.material.color.g);
                writer.WriteValueSafe<float>(chemicalItem.mainObjectRenderer.material.color.b);
                writer.WriteValueSafe<ChemicalEffects>(pair.Value);

                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesChemColor", writer, NetworkDelivery.Reliable);
            }
        }
    }
}
