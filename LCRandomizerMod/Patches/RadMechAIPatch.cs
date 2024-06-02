using HarmonyLib;
using System;
using Unity.Netcode;
using UnityEngine;

namespace LCRandomizerMod.Patches
{
    [HarmonyPatch(typeof(RadMechAI))]
    internal class RadMechAIPatch
    {
        [HarmonyPatch(nameof(RadMechAI.Start))]
        [HarmonyPostfix]
        public static void StatOverride(RadMechAI __instance)
        {
            if (Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                float speed = Convert.ToSingle(new System.Random().Next(20, 200)) / 10f;
                float health = Convert.ToSingle(new System.Random().Next(1, 11));
                float scale = Convert.ToSingle(new System.Random().Next(1, 31)) / 10;
                //try
                //{
                //    scale = RandomizerValues.spawnedMechScales.ElementAt(RandomizerValues.spawnedMechCount++);
                //}catch (Exception ex)
                //{
                //    scale = Convert.ToSingle(new System.Random().Next(1, 31)) / 10;
                //    RandomizerModBase.mls.LogError("Index out of range exception caught! Are you trying to set a mech's scale which didn't get a scale assigned on spawn?");
                //}

                RandomizerValues.radMechSpeedsDict.Add(__instance.NetworkObjectId, speed);

                __instance.enemyHP = (int)health;
                __instance.transform.localScale = new Vector3(scale, scale, scale);

                __instance.creatureAnimator.speed = speed / 10f;
                __instance.creatureSFX.pitch = Mathf.Lerp(3f, 0.01f, Mathf.InverseLerp(0.1f, 3f, scale));
                __instance.creatureVoice.pitch = Mathf.Lerp(3f, 0.01f, Mathf.InverseLerp(0.1f, 3f, scale));
                __instance.LocalLRADAudio.pitch = Mathf.Lerp(3f, 0.01f, Mathf.InverseLerp(0.1f, 3f, scale));
                __instance.LocalLRADAudio2.pitch = Mathf.Lerp(3f, 0.01f, Mathf.InverseLerp(0.1f, 3f, scale));
                __instance.missilePrefab.gameObject.transform.localScale = new Vector3(scale, scale, scale);

                FastBufferWriter fastBufferWriter = new FastBufferWriter(sizeof(ulong) + sizeof(float) * 3, Unity.Collections.Allocator.Temp, -1);
                fastBufferWriter.WriteValueSafe<ulong>(__instance.NetworkObjectId);
                fastBufferWriter.WriteValueSafe<float>(speed);
                fastBufferWriter.WriteValueSafe<float>(health);
                fastBufferWriter.WriteValueSafe<float>(scale);

                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesRadMechData", fastBufferWriter, NetworkDelivery.Reliable);

                RandomizerModBase.mls.LogError("MECH SIZE: " + scale);

                fastBufferWriter.Dispose();
            }
        }

        [HarmonyPatch("DoFootstepCycle")]
        [HarmonyPostfix]
        public static void SpeedOverride(RadMechAI __instance)
        {
            if (!__instance.isEnemyDead)
            {
                __instance.agent.speed = RandomizerValues.radMechSpeedsDict.GetValueSafe(__instance.NetworkObjectId);
            }
        }

        public static void SetMechData(ulong _, FastBufferReader reader)
        {
            if (!Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                ulong id;
                float speed;
                float health;
                float scale;

                reader.ReadValueSafe<ulong>(out id);
                reader.ReadValueSafe<float>(out speed);
                reader.ReadValueSafe<float>(out health);
                reader.ReadValueSafe<float>(out scale);

                RandomizerValues.radMechSpeedsDict.Add(id, speed);

                NetworkObject networkObject = Unity.Netcode.NetworkManager.Singleton.SpawnManager.SpawnedObjects[id];
                RadMechAI radMech = networkObject.gameObject.GetComponentInChildren<RadMechAI>();

                radMech.enemyHP = (int)health;
                radMech.transform.localScale = new Vector3(scale, scale, scale);

                //CHECK FOR NRE!!!
                //radMech.creatureAnimator.speed = speed / 10f;
                radMech.creatureSFX.pitch = Mathf.Lerp(3f, 0.01f, Mathf.InverseLerp(0.1f, 3f, scale));
                radMech.creatureVoice.pitch = Mathf.Lerp(3f, 0.01f, Mathf.InverseLerp(0.1f, 3f, scale));
                radMech.LocalLRADAudio.pitch = Mathf.Lerp(3f, 0.01f, Mathf.InverseLerp(0.1f, 3f, scale));
                radMech.LocalLRADAudio2.pitch = Mathf.Lerp(3f, 0.01f, Mathf.InverseLerp(0.1f, 3f, scale));
                radMech.missilePrefab.gameObject.transform.localScale = new Vector3(scale, scale, scale);

                RandomizerModBase.mls.LogInfo("RECEIVED MECH STATS: " + id + ", " + speed + ", " + health + ", " + scale);
            }
        }

        public static void SetMechSpawnerScale(ulong _, FastBufferReader reader)
        {
            if (!Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                ulong id;
                float scale;

                reader.ReadValueSafe<ulong>(out id);
                reader.ReadValueSafe<float>(out scale);

                RandomizerValues.spawnedMechScales.Add(scale);

                NetworkObject nObject = Unity.Netcode.NetworkManager.Singleton.SpawnManager.SpawnedObjects[id];
                nObject.transform.localScale = new Vector3(scale, scale, scale);

                RandomizerModBase.mls.LogInfo("Set component scales.");
            }
        }
    }
}
