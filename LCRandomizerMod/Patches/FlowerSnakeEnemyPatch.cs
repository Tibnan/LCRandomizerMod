using HarmonyLib;
using System;
using Unity.Netcode;
using UnityEngine;

namespace LCRandomizerMod.Patches
{
    [HarmonyPatch(typeof(FlowerSnakeEnemy))]
    internal class FlowerSnakeEnemyPatch
    {
        [HarmonyPatch(nameof(FlowerSnakeEnemy.Start))]
        [HarmonyPostfix]
        public static void StatOverride(FlowerSnakeEnemy __instance)
        {
            if (Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                float speed = Convert.ToSingle(new System.Random().Next(20, 200)) / 10f;
                float health = Convert.ToSingle(new System.Random().Next(1, 11));
                float scale = Convert.ToSingle(new System.Random().Next(5, 31)) / 10;

                RandomizerValues.flowerSnakeSpeedsDict.Add(__instance.NetworkObjectId, speed);

                __instance.enemyHP = (int)health;
                __instance.transform.localScale = new Vector3(scale, scale, scale);

                __instance.creatureAnimator.speed = speed / 10;
                __instance.creatureSFX.pitch = Mathf.Lerp(3f, 0.01f, Mathf.InverseLerp(0.5f, 3f, scale));
                __instance.creatureVoice.pitch = Mathf.Lerp(3f, 0.01f, Mathf.InverseLerp(0.5f, 3f, scale));
                __instance.flappingAudio.pitch = Mathf.Lerp(3f, 0.01f, Mathf.InverseLerp(0.5f, 3f, scale));

                FastBufferWriter fastBufferWriter = new FastBufferWriter(sizeof(ulong) + sizeof(float) * 3, Unity.Collections.Allocator.Temp, -1);
                fastBufferWriter.WriteValueSafe<ulong>(__instance.NetworkObjectId);
                fastBufferWriter.WriteValueSafe<float>(speed);
                fastBufferWriter.WriteValueSafe<float>(health);
                fastBufferWriter.WriteValueSafe<float>(scale);

                Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesFlowerSnakeData", fastBufferWriter, NetworkDelivery.Reliable);
                fastBufferWriter.Dispose();
            }
        }

        [HarmonyPatch(nameof(FlowerSnakeEnemy.Update))]
        [HarmonyPostfix]
        public static void SpeedOverride(FlowerSnakeEnemy __instance)
        {
            if (!__instance.isEnemyDead)
            {
                __instance.agent.speed = RandomizerValues.flowerSnakeSpeedsDict.GetValueSafe(__instance.NetworkObjectId);
            }
        }

        public static void SetFlowerSnakeStats(ulong _, FastBufferReader reader)
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

                RandomizerValues.flowerSnakeSpeedsDict.Add(id, speed);

                NetworkObject networkObject = Unity.Netcode.NetworkManager.Singleton.SpawnManager.SpawnedObjects[id];
                FlowerSnakeEnemy flowerSnake = networkObject.gameObject.GetComponentInChildren<FlowerSnakeEnemy>();
                flowerSnake.enemyHP = (int)health;
                flowerSnake.transform.localScale = new Vector3(scale, scale, scale);

                //flowerSnake.creatureAnimator.speed = speed / 10;
                flowerSnake.creatureSFX.pitch = Mathf.Lerp(3f, 0.01f, Mathf.InverseLerp(0.5f, 3f, scale));
                flowerSnake.creatureVoice.pitch = Mathf.Lerp(3f, 0.01f, Mathf.InverseLerp(0.5f, 3f, scale));
                flowerSnake.flappingAudio.pitch = Mathf.Lerp(3f, 0.01f, Mathf.InverseLerp(0.5f, 3f, scale));

                RandomizerModBase.mls.LogInfo("RECEIVED FLOWER SNAKE STATS: " + id + ", " + speed + ", " + health + ", " + scale);
            }
        }

        [HarmonyPatch("SetFlappingLocalClient")]
        [HarmonyPostfix]
        public static void FlapPitchOverride(FlowerSnakeEnemy __instance)
        {
            if (Traverse.Create(__instance).Field("flapping").GetValue<bool>())
            {
                __instance.flappingAudio.pitch = Mathf.Lerp(3f, 0.01f, Mathf.InverseLerp(0.5f, 3f, __instance.transform.localScale.x));
            }
        }
    }
}
