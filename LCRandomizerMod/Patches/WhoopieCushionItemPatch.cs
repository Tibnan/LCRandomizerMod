using HarmonyLib;
using System;
using Unity.Netcode;

namespace LCRandomizerMod.Patches
{
    [HarmonyPatch(typeof(WhoopieCushionItem))]
    internal class WhoopieCushionItemPatch
    {
        [HarmonyPatch(nameof(WhoopieCushionItem.FartWithDebounce))]
        [HarmonyPrefix]
        public static void PitchOverride(WhoopieCushionItem __instance)
        {
            if (Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                FastBufferWriter fastBufferWriter = new FastBufferWriter(sizeof(ulong) + sizeof(float) + sizeof(bool), Unity.Collections.Allocator.Temp, -1);

                __instance.whoopieCushionAudio.pitch = Convert.ToSingle(new System.Random().Next(1, 201)) / 10;
                fastBufferWriter.WriteValueSafe<ulong>(__instance.NetworkObjectId);
                fastBufferWriter.WriteValueSafe<float>(__instance.whoopieCushionAudio.pitch);

                if (new System.Random().Next(1, 26) == 5)
                {
                    Landmine.SpawnExplosion(__instance.transform.position, true);
                    fastBufferWriter.WriteValueSafe<bool>(true);
                }
                else
                {
                    fastBufferWriter.WriteValueSafe<bool>(false);
                }
                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesWhoopieCData", fastBufferWriter, NetworkDelivery.Reliable);
            }
        }

        public static void SetPitchClientAndExplode(ulong _, FastBufferReader reader)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                ulong id;
                float pitch;
                bool hasExploded;

                reader.ReadValueSafe<ulong>(out id);
                reader.ReadValueSafe<float>(out pitch);
                reader.ReadValueSafe<bool>(out hasExploded);

                NetworkObject networkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[id];
                WhoopieCushionItem whoopieCushionItem = networkObject.gameObject.GetComponentInChildren<WhoopieCushionItem>();

                whoopieCushionItem.whoopieCushionAudio.pitch = pitch;

                if (hasExploded)
                {
                    Landmine.SpawnExplosion(whoopieCushionItem.transform.position, true);
                }
            }
        }
    }
}
