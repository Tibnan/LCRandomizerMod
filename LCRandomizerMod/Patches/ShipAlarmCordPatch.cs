using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace LCRandomizerMod.Patches
{
    [HarmonyPatch(typeof(ShipAlarmCord))]
    internal class ShipAlarmCordPatch
    {
        [HarmonyPatch(nameof(ShipAlarmCord.PullCordClientRpc))]
        [HarmonyPrefix]
        public static void GenerateRandomPitch(ShipAlarmCord __instance)
        {
            if (Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                RandomizerValues.cordPitch = new System.Random().Next(4, 35) / 10f;

                FastBufferWriter writer = new FastBufferWriter(sizeof(float), Unity.Collections.Allocator.Temp, -1);
                writer.WriteValueSafe<float>(RandomizerValues.cordPitch);

                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesHornPitch", writer, NetworkDelivery.Reliable);
            }
        }

        public static void SetPitch(ulong _, FastBufferReader reader)
        {
            if (!Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                float pitch;

                reader.ReadValueSafe<float>(out pitch);

                ShipAlarmCord shipAlarm = GameObject.FindObjectOfType<ShipAlarmCord>();
                shipAlarm.cordAudio.pitch = pitch;
                shipAlarm.hornClose.pitch = pitch;
                shipAlarm.hornFar.pitch = pitch;

            }
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        public static void UpdatePitchWithRGen(ShipAlarmCord __instance)
        {
            __instance.cordAudio.pitch = RandomizerValues.cordPitch;
            __instance.hornClose.pitch = RandomizerValues.cordPitch;
            __instance.hornFar.pitch = RandomizerValues.cordPitch;
        }
    }
}
