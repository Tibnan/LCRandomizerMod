using HarmonyLib;
using UnityEngine;
using Unity.Netcode;
using System;

namespace LCRandomizerMod.Patches
{
    [HarmonyPatch(typeof(GrabbableObject))]
    internal class GrabbableObjectPatch
    {
        [HarmonyPatch(nameof(GrabbableObject.SetScrapValue))]
        [HarmonyPrefix]
        public static bool SetScrapValue(GrabbableObject __instance, int setValueTo)
        {
            if (Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                RandomizerModBase.mls.LogError(__instance.itemProperties.weight);
                if (!__instance.isInShipRoom)
                {
                    //float weight = Convert.ToSingle(new System.Random().Next(51, 201)) / 100;
                    int rndVal;

                    if (TimeOfDay.Instance.timesFulfilledQuota > 0)
                    {
                        rndVal = new System.Random().Next(new System.Random().Next(1, 20), new System.Random().Next(100, 501)) * TimeOfDay.Instance.timesFulfilledQuota;
                    }
                    else
                    {
                        rndVal = new System.Random().Next(new System.Random().Next(1, 20), new System.Random().Next(100, 501));
                    }

                    setValueTo = rndVal;
                    __instance.scrapValue = setValueTo;
                    ScanNodeProperties componentInChildren = __instance.gameObject.GetComponentInChildren<ScanNodeProperties>();

                    if (componentInChildren == null)
                    {
                        Debug.LogError("Scan node is missing for item!: " + __instance.name);
                        return true;
                    }
                    componentInChildren.subText = string.Format("Value: ${0}", setValueTo);
                    componentInChildren.scrapValue = setValueTo;

                    //__instance.itemProperties.weight = weight;

                    RandomizerModBase.mls.LogInfo("Set scrap value of: " + __instance.name + " to: " + setValueTo + " sending over network...");
                    //RandomizerModBase.mls.LogInfo("Set scrap value of: " + __instance.name + " to: " + setValueTo + " and assigned weight of: " + weight + " sending over network...");

                    FastBufferWriter fastBufferWriterScrapValue = new FastBufferWriter(sizeof(int) + sizeof(float), Unity.Collections.Allocator.Temp, -1);
                    fastBufferWriterScrapValue.WriteValueSafe<int>(rndVal);
                    //fastBufferWriterScrapValue.WriteValueSafe<float>(weight);
                    Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "SetScrapValueTo", fastBufferWriterScrapValue, NetworkDelivery.Reliable);
                    fastBufferWriterScrapValue.Dispose();
                    RandomizerModBase.mls.LogInfo("Sent value of " + rndVal + " to clients");

                    FastBufferWriter fastBuferWriterNetworkRef = new FastBufferWriter(sizeof(ulong), Unity.Collections.Allocator.Temp, -1);
                    fastBuferWriterNetworkRef.WriteValueSafe(__instance.NetworkObjectId);
                    Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "SetValueOn", fastBuferWriterNetworkRef, NetworkDelivery.Reliable);
                    fastBuferWriterNetworkRef.Dispose();
                    RandomizerModBase.mls.LogInfo("Sent network reference.");
                    return false;
                }
                else return true;
            }
            else
            {
                return true;
            }
        }

        public static void SetScrapValueTo(ulong _, FastBufferReader reader)
        {
            if (!Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                reader.ReadValueSafe<int>(out RandomizerValues.scrapValue, default);
                //reader.ReadValueSafe<float>(out RandomizerValues.scrapWeight, default);
                RandomizerModBase.mls.LogInfo("Got random value: " + RandomizerValues.scrapValue + " and weight: " + RandomizerValues.scrapWeight);
            }
        }

        public static void SetValueOn(ulong _, FastBufferReader reader)
        {
            if (!Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                reader.ReadValueSafe<ulong>(out RandomizerValues.scrapReference, default);
                FinalizeValue();
            }
        }

        public static void FinalizeValue()
        {
            RandomizerModBase.mls.LogInfo("Got scrap identifier: " + RandomizerValues.scrapReference);
            NetworkObject networkObject = Unity.Netcode.NetworkManager.Singleton.SpawnManager.SpawnedObjects[RandomizerValues.scrapReference];
            GrabbableObject scrap = networkObject.gameObject.GetComponentInChildren<GrabbableObject>();

            RandomizerModBase.mls.LogInfo("Scrap name that was received from server: " + scrap.name);

            scrap.scrapValue = RandomizerValues.scrapValue;
            ScanNodeProperties componentInChildren = scrap.gameObject.GetComponentInChildren<ScanNodeProperties>();
            if (componentInChildren == null)
            {
                Debug.LogError("Scan node is missing for item!: " + scrap.gameObject.name);
            }
            componentInChildren.subText = string.Format("Value: ${0}", RandomizerValues.scrapValue);
            componentInChildren.scrapValue = RandomizerValues.scrapValue;

           //RandomizerModBase.mls.LogInfo("Got weight: " + RandomizerValues.scrapWeight);
           //scrap.itemProperties.weight = RandomizerValues.scrapWeight;
        }

        [HarmonyPatch(nameof(GrabbableObject.GrabItem))]
        [HarmonyPostfix]
        public static void ResizeGrabbedItem(GrabbableObject __instance)
        {
            if (__instance.playerHeldBy == GameNetworkManager.Instance.localPlayerController && GameNetworkManager.Instance.localPlayerController.gameObject.transform.localScale.x < 1f)
            {
                if (!RandomizerValues.itemResizeDict.ContainsKey(__instance.NetworkObjectId)) 
                {
                    RandomizerValues.itemResizeDict.Add(__instance.NetworkObjectId, __instance.gameObject.transform.localScale);
                }
                __instance.gameObject.transform.localScale /= 2.7f;
            }
        }

        [HarmonyPatch(nameof(GrabbableObject.DiscardItem))]
        [HarmonyPostfix]
        public static void SizeBackToNormal(GrabbableObject __instance)
        {
            if (__instance.playerHeldBy == GameNetworkManager.Instance.localPlayerController && RandomizerValues.itemResizeDict.ContainsKey(__instance.NetworkObjectId))
            {
                __instance.gameObject.transform.localScale = RandomizerValues.itemResizeDict.GetValueSafe(__instance.NetworkObjectId);
                RandomizerValues.itemResizeDict.Remove(__instance.NetworkObjectId);
            }
        }
    }
}
