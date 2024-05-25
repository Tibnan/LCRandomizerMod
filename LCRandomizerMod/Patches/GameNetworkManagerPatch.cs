using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;

namespace LCRandomizerMod.Patches
{
    [HarmonyPatch(typeof(GameNetworkManager))]
    internal class GameNetworkManagerPatch
    {
        [HarmonyPatch(nameof(GameNetworkManager.Disconnect))]
        [HarmonyPostfix]
        public static void SaveCustomValues(GameNetworkManager __instance)
        {
            if (Unity.Netcode.NetworkManager.Singleton.IsServer && StartOfRound.Instance.inShipPhase && __instance != null)
            {
                try
                {
                    var runnableTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => typeof(ICustomValue).IsAssignableFrom(t) && !t.IsInterface);

                    foreach (var type in runnableTypes)
                    {
                        var instance = (ICustomValue)Activator.CreateInstance(type);
                        instance.SaveOnExit();
                    }
                    ES3.Save("keysToLoad", RandomizerValues.keysToLoad, GameNetworkManager.Instance.currentSaveFileName);

                    RandomizerModBase.mls.LogInfo("Saved dicts.");
                } catch (Exception ex)
                {
                    RandomizerModBase.mls.LogError("Exception caught during custom value serialization. " + ex.Message);
                    return;
                }

                RandomizerValues.ClearDicts();
            }
        }
    }
}
