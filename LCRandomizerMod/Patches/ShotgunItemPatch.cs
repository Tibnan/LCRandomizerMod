using HarmonyLib;

namespace LCRandomizerMod.Patches
{
    [HarmonyPatch(typeof(ShotgunItem))]
    internal class ShotgunItemPatch
    {
        [HarmonyPatch(nameof(ShotgunItem.Start))]
        [HarmonyPostfix]
        public static void RandomlyAddBullet(ShotgunItem __instance)
        {
            if (!__instance.isInShipRoom)
            {
                __instance.shellsLoaded = new System.Random().Next(1, 5);
                __instance.useCooldown = 0f;
            }
        }

        [HarmonyPatch(nameof(ShotgunItem.ShootGun))]
        [HarmonyPostfix]
        public static void ShootGunOverride(ShotgunItem __instance)
        {
            if (new System.Random().Next(1, 5) == 4) __instance.shellsLoaded++;
        }
    }
}
