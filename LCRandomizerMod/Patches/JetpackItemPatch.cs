using HarmonyLib;
using System;
using Unity.Netcode;

namespace LCRandomizerMod.Patches
{
    [HarmonyPatch(typeof(JetpackItem))]
    internal class JetpackItemPatch
    {
        [HarmonyPatch(nameof(JetpackItem.EquipItem))]
        [HarmonyPostfix]
        public static void JetpackStatOverride(JetpackItem __instance)
        {
            if (!RandomizerValues.jetpackPropertiesDict.ContainsKey(__instance.NetworkObjectId) && Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                float jetpackAccel = Convert.ToSingle(new System.Random().Next(10, 301)) / 10f;
                float jetpackDecel = Convert.ToSingle(new System.Random().Next(10, 301)) / 10f;

                __instance.jetpackAcceleration = jetpackAccel;
                __instance.jetpackDeaccelleration = jetpackDecel;

                RandomizerValues.jetpackPropertiesDict.Add(__instance.NetworkObjectId, new Tuple<float, float>(jetpackAccel, jetpackDecel));

                FastBufferWriter fastBufferWriter = new FastBufferWriter(sizeof(ulong) + sizeof(float) * 2, Unity.Collections.Allocator.Temp, -1);
                fastBufferWriter.WriteValueSafe<ulong>(__instance.NetworkObjectId);
                fastBufferWriter.WriteValueSafe<float>(jetpackAccel);
                fastBufferWriter.WriteValueSafe<float>(jetpackDecel);

                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesJetpackData", fastBufferWriter, NetworkDelivery.Reliable);

                HUDManager.Instance.AddTextToChatOnServer("<color=red>WARNING: Jetpack stat saving is not yet implemented! They will behave differently each time you restart the server.</color>", -1);
            }
        }

        public static void SetJetpackStatsSentByServer(ulong _, FastBufferReader reader)
        {
            if (!Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                ulong id;
                float acc;
                float dec;

                reader.ReadValueSafe<ulong>(out id);
                if (RandomizerValues.jetpackPropertiesDict.ContainsKey(id))
                {
                    RandomizerModBase.mls.LogInfo("CONTAINS KEY");
                    return;
                }
                reader.ReadValueSafe<float>(out acc);
                reader.ReadValueSafe<float>(out dec);

                RandomizerModBase.mls.LogInfo("ADDING DICT");
                RandomizerValues.jetpackPropertiesDict.Add(id, new Tuple<float, float>(acc, dec));

                RandomizerModBase.mls.LogInfo("CONVERTING ID");
                NetworkObject networkObject = Unity.Netcode.NetworkManager.Singleton.SpawnManager.SpawnedObjects[id];
                JetpackItem jetpackItem = networkObject.gameObject.GetComponentInChildren<JetpackItem>();

                jetpackItem.jetpackAcceleration = acc;
                jetpackItem.jetpackDeaccelleration = dec;

                RandomizerModBase.mls.LogInfo("RECEIVED JETPACK STATS: " + id + ", " + acc + ", " + dec);
            }
        }
    }
}
