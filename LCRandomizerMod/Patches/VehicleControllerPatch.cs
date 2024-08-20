using HarmonyLib;
using Unity.Netcode;
using System;
using UnityEngine;
using GameNetcodeStuff;
using System.Collections;
using LethalLib.Modules;

namespace LCRandomizerMod.Patches
{
    [HarmonyPatch(typeof(VehicleController))]
    internal class VehicleControllerPatch : ICustomValue
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        public static void RandomizeVehicleProperties(VehicleController __instance)
        {
            if (NetworkManager.Singleton.IsServer && !RandomizerValues.randomizedCar)
            {
                FastBufferWriter writer = new FastBufferWriter(sizeof(ulong) + sizeof(float) * 7, Unity.Collections.Allocator.Temp, -1);
                writer.WriteValueSafe<ulong>(__instance.NetworkObjectId);

                if (RandomizerValues.randomCarProperties == null)
                {
                    RandomizerValues.randomCarProperties = new RandomCarProperties();
                }
                else
                {
                    RandomizerValues.randomCarProperties.Regenerate();
                }

                writer.WriteValueSafe<float>(RandomizerValues.randomCarProperties.Scale.x);

                writer.WriteValue<float>(RandomizerValues.randomCarProperties.HeadlightColor.r);
                writer.WriteValue<float>(RandomizerValues.randomCarProperties.HeadlightColor.g);
                writer.WriteValue<float>(RandomizerValues.randomCarProperties.HeadlightColor.b);

                __instance.transform.localScale = RandomizerValues.randomCarProperties.Scale;

                Light[] headlights = __instance.headlightsContainer.GetComponentsInChildren<Light>();
                foreach (Light light in headlights)
                {
                    light.color = RandomizerValues.randomCarProperties.HeadlightColor;
                }

                writer.WriteValueSafe<float>(RandomizerValues.randomCarProperties.CarColor.r);
                writer.WriteValueSafe<float>(RandomizerValues.randomCarProperties.CarColor.g);
                writer.WriteValueSafe<float>(RandomizerValues.randomCarProperties.CarColor.b);

                __instance.mainBodyMesh.material.color = RandomizerValues.randomCarProperties.CarColor;
                __instance.lod1Mesh.material.color = RandomizerValues.randomCarProperties.CarColor;
                __instance.lod2Mesh.material.color = RandomizerValues.randomCarProperties.CarColor;
                __instance.carHoodAnimator.gameObject.GetComponent<MeshRenderer>().material.color = RandomizerValues.randomCarProperties.CarColor;
                __instance.driverSideDoor.GetComponentsInParent<MeshRenderer>()[1].material.color = RandomizerValues.randomCarProperties.CarColor;
                __instance.passengerSideDoor.GetComponentsInParent<MeshRenderer>()[1].material.color = RandomizerValues.randomCarProperties.CarColor;
                __instance.driverSeatSpringAnimator.gameObject.GetComponentInChildren<MeshRenderer>().material.color = RandomizerValues.randomCarProperties.CarColor;

                NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesCarData", writer, NetworkDelivery.Reliable);
            }
        }

        [HarmonyPatch(nameof(VehicleController.DestroyCarClientRpc))]
        [HarmonyPostfix]
        public static void ResetCarState(VehicleController __instance)
        {
            RandomizerValues.randomizedCar = false;
        }

        //TODO: Smooth out resizing somehow
        [HarmonyPatch(nameof(VehicleController.SetPlayerInCar))]
        [HarmonyPostfix]
        public static void ResizePlayerToFitInCar(VehicleController __instance, ref PlayerControllerB player)
        {
            RandomizerModBase.mls.LogWarning("Car scale: " + __instance.transform.localScale);

            float scale = Mathf.Lerp(0.865f, 0.87f, Mathf.InverseLerp(0.5f, 1.5f, __instance.transform.localScale.x));

            Vector3 scaleVec = new Vector3(scale, scale, scale);

            RandomizerModBase.mls.LogError("Resizing player to: " + scaleVec);
            GameNetworkManager.Instance.StartCoroutine(ResizeCoroutine(player, scaleVec));

            //RandomizerModBase.mls.LogError(String.Format("Transform size: {0}, player model sizes: {1}, {2}", GameNetworkManager.Instance.localPlayerController.transform.localScale, GameNetworkManager.Instance.localPlayerController.thisPlayerBody.localScale, GameNetworkManager.Instance.localPlayerController.playerGlobalHead.localScale));
            //GameNetworkManager.Instance.StartCoroutine(TestCoroutine());
        }

        //public static IEnumerator TestCoroutine()
        //{
        //    int c = 0;
        //    while (c < 5)
        //    {
        //        yield return new WaitForSeconds(1f);
        //        c++;
        //        RandomizerModBase.mls.LogError(String.Format("Transform size: {0}, player model sizes: {1}, {2}", GameNetworkManager.Instance.localPlayerController.transform.localScale, GameNetworkManager.Instance.localPlayerController.thisPlayerBody.localScale, GameNetworkManager.Instance.localPlayerController.playerGlobalHead.localScale));
        //    }
        //}

        //Server side
        public static void SendPlayerExitToClients(PlayerControllerB player)
        {
            FastBufferWriter writer = new FastBufferWriter(sizeof(ulong), Unity.Collections.Allocator.Temp, -1);
            writer.WriteValueSafe<ulong>(player.playerClientId);

            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesDriverExit", writer, NetworkDelivery.Reliable);
        }

        public static void ServerHandlePlayerExit(ulong _, FastBufferReader reader)
        {
            ulong id;

            reader.ReadValueSafe<ulong>(out id);

            PlayerControllerB player = StartOfRound.Instance.allPlayerScripts[id];
            SendPlayerExitToClients(player);
            GameNetworkManager.Instance.StartCoroutine(ResizeCoroutine(player, RandomizerValues.playerScaleDict.GetValueSafe(player.playerClientId)));
        }

        //Client side
        public static void HandlePlayerExit(ulong _, FastBufferReader reader)
        {
            ulong id;

            reader.ReadValueSafe<ulong>(out id);

            PlayerControllerB player = StartOfRound.Instance.allPlayerScripts[id];
            RandomizerModBase.mls.LogError("Resizing player " + player.playerUsername + " to: " + RandomizerValues.playerScaleDict.GetValueSafe(id));
            GameNetworkManager.Instance.StartCoroutine(ResizeCoroutine(player, RandomizerValues.playerScaleDict.GetValueSafe(id)));
        }
        

        [HarmonyPatch(nameof(VehicleController.RemovePlayerControlOfVehicleClientRpc))]
        [HarmonyPrefix]
        public static void DriverExitResizeHandler(VehicleController __instance, ref int playerId)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                RandomizerModBase.mls.LogWarning("Running server side logic...");
                PlayerControllerB player = StartOfRound.Instance.allPlayerScripts[playerId];
                SendPlayerExitToClients(player);
                GameNetworkManager.Instance.StartCoroutine(ResizeCoroutine(player, RandomizerValues.playerScaleDict.GetValueSafe((ulong)playerId)));
            }
            else
            {
                RandomizerModBase.mls.LogWarning("Running client side logic...");
                FastBufferWriter writer = new FastBufferWriter(sizeof(ulong), Unity.Collections.Allocator.Temp, -1);

                writer.WriteValueSafe<ulong>((ulong)playerId);

                NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage("Tibnan.lcrandomizermod_" + "ServerReceivesPlayerExit", 0UL, writer, NetworkDelivery.Reliable);
            }
        }

        [HarmonyPatch(nameof(VehicleController.OnPassengerExit))]
        [HarmonyPrefix]
        public static void PassengerExitResizeHandler(VehicleController __instance)
        {
            if (!__instance.localPlayerInPassengerSeat) return;

            PlayerControllerB player = GameNetworkManager.Instance.localPlayerController;
            if (NetworkManager.Singleton.IsServer)
            {
                RandomizerModBase.mls.LogWarning("Running server side logic...");
                SendPlayerExitToClients(player);
                GameNetworkManager.Instance.StartCoroutine(ResizeCoroutine(player, RandomizerValues.playerScaleDict.GetValueSafe((ulong)player.playerClientId)));
            }
            else
            {
                RandomizerModBase.mls.LogWarning("Running client side logic...");
                FastBufferWriter writer = new FastBufferWriter(sizeof(ulong), Unity.Collections.Allocator.Temp, -1);

                writer.WriteValueSafe<ulong>((ulong)player.playerClientId);

                NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage("Tibnan.lcrandomizermod_" + "ServerReceivesPlayerExit", 0UL, writer, NetworkDelivery.Reliable);
            }
        } 

        private static IEnumerator ResizeCoroutine(PlayerControllerB player, Vector3 scale)
        {
            yield return new WaitForSeconds(0.3f); //0.3f
            RandomizerModBase.mls.LogError("PLAYER CLIENT ID!!! " + player.playerClientId);
            player.transform.localScale = scale;
            RandomizerModBase.mls.LogError("Resized to: " + player.transform.localScale);
        }

        public static void ClientSetCarProperties(ulong _, FastBufferReader reader)
        {
            if (!NetworkManager.Singleton.IsServer && !RandomizerValues.randomizedCar)
            {
                ulong id;
                float scale;
                float hR;
                float hG;
                float hB;
                float cR;
                float cG;
                float cB;

                reader.ReadValueSafe<ulong>(out id);
                reader.ReadValueSafe<float>(out scale);
                reader.ReadValueSafe<float>(out hR);
                reader.ReadValueSafe<float>(out hG);
                reader.ReadValueSafe<float>(out hB);
                reader.ReadValueSafe<float>(out cR);
                reader.ReadValueSafe<float>(out cG);
                reader.ReadValueSafe<float>(out cB);

                Color headlightColor = new Color(hR, hG, hB);

                NetworkObject networkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[id];
                VehicleController vehicleController = networkObject.gameObject.GetComponentInChildren<VehicleController>();

                Light[] headlights = vehicleController.headlightsContainer.GetComponentsInChildren<Light>();
                foreach (Light light in headlights)
                {
                    light.color = headlightColor;
                }

                Color carColor = new Color(cR, cG, cB);

                vehicleController.mainBodyMesh.material.color = carColor;
                vehicleController.lod1Mesh.material.color = carColor;
                vehicleController.lod2Mesh.material.color = carColor;
                vehicleController.carHoodAnimator.gameObject.GetComponent<MeshRenderer>().material.color = carColor;
                vehicleController.driverSideDoor.GetComponentsInParent<MeshRenderer>()[1].material.color = carColor;
                vehicleController.passengerSideDoor.GetComponentsInParent<MeshRenderer>()[1].material.color = carColor;
                vehicleController.driverSeatSpringAnimator.gameObject.GetComponentInChildren<MeshRenderer>().material.color = carColor;

                vehicleController.gameObject.transform.localScale = new Vector3(scale, scale, scale);

                RandomizerValues.randomizedCar = true;
            }
        }

        public void SaveOnExit()
        {
            if (RandomizerValues.randomizedCar)
            {
                try
                {
                    RandomizerModBase.mls.LogWarning("Saving car randomization...");
                    ES3.Save("randomCarProps", RandomizerValues.randomCarProperties, GameNetworkManager.Instance.currentSaveFileName);
                    if (!RandomizerValues.keysToLoad.Contains("randomCarProps"))
                    {
                        RandomizerValues.keysToLoad.Add("randomCarProps");
                    }
                }
                catch (Exception ex)
                {
                    RandomizerModBase.mls.LogError("Exception caught during custom value serialization. [VehicleController] " + ex.Message);
                }
            }
            else if (RandomizerValues.keysToLoad.Contains("randomCarProps"))
            {
                RandomizerValues.keysToLoad.Remove("randomCarProps");
            }
        }

        public void ReloadStats()
        {
            if (RandomizerValues.randomizedCar)
            {
                VehicleController vehicleController = GameObject.FindObjectOfType<VehicleController>();

                if (vehicleController is null)
                {
                    RandomizerModBase.mls.LogError("Tried to reload car stats, but your crew does not have a car. Is your save corrupt? Returning...");
                    return;
                }

                Light[] headlights = vehicleController.headlightsContainer.GetComponentsInChildren<Light>();
                foreach (Light light in headlights)
                {
                    light.color = RandomizerValues.randomCarProperties.HeadlightColor;
                }

                vehicleController.mainBodyMesh.material.color = RandomizerValues.randomCarProperties.CarColor;
                vehicleController.lod1Mesh.material.color = RandomizerValues.randomCarProperties.CarColor;
                vehicleController.lod2Mesh.material.color = RandomizerValues.randomCarProperties.CarColor;
                vehicleController.carHoodAnimator.gameObject.GetComponent<MeshRenderer>().material.color = RandomizerValues.randomCarProperties.CarColor;
                vehicleController.driverSideDoor.GetComponentsInParent<MeshRenderer>()[1].material.color = RandomizerValues.randomCarProperties.CarColor;
                vehicleController.passengerSideDoor.GetComponentsInParent<MeshRenderer>()[1].material.color = RandomizerValues.randomCarProperties.CarColor;
                vehicleController.driverSeatSpringAnimator.gameObject.GetComponentInChildren<MeshRenderer>().material.color = RandomizerValues.randomCarProperties.CarColor;

                vehicleController.gameObject.transform.localScale = RandomizerValues.randomCarProperties.Scale;
            }
            else
            {
                RandomizerModBase.mls.LogInfo("No car to reload randomization of.");
            }
        }

        public void SyncStatsWithClients()
        {
            VehicleController vehicle = GameObject.FindObjectOfType<VehicleController>();

            if (vehicle == null) return;

            FastBufferWriter writer = new FastBufferWriter(sizeof(ulong) + sizeof(float) * 7, Unity.Collections.Allocator.Temp, -1);

            writer.WriteValueSafe<ulong>(vehicle.NetworkObjectId);
            writer.WriteValueSafe<float>(RandomizerValues.randomCarProperties.Scale.x);
            writer.WriteValueSafe<float>(RandomizerValues.randomCarProperties.HeadlightColor.r);
            writer.WriteValueSafe<float>(RandomizerValues.randomCarProperties.HeadlightColor.g);
            writer.WriteValueSafe<float>(RandomizerValues.randomCarProperties.HeadlightColor.b);
            writer.WriteValueSafe<float>(RandomizerValues.randomCarProperties.CarColor.r);
            writer.WriteValueSafe<float>(RandomizerValues.randomCarProperties.CarColor.g);
            writer.WriteValueSafe<float>(RandomizerValues.randomCarProperties.CarColor.b);

            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesCarData", writer, NetworkDelivery.Reliable);
        }
    }
}
