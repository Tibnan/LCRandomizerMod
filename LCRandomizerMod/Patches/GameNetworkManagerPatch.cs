using Unity.Netcode;
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
        public static void ClearDicts()
        {
            RandomizerModBase.mls.LogWarning("Clearing dicts");
            RandomizerValues.ReleaseResources(true);
            RandomizerValues.mapRandomizedInTerminal = false;
        }

        [HarmonyPatch(nameof(GameNetworkManager.SaveGame))]
        [HarmonyPostfix]
        public static void AutoSaveCustomItemData(GameNetworkManager __instance)
        {
            if (Unity.Netcode.NetworkManager.Singleton.IsServer && StartOfRound.Instance.inShipPhase && __instance != null)
            {
                RandomizerModBase.mls.LogWarning("Beginning custom data saving...");
                CustomUI playerUI = GameNetworkManager.Instance.localPlayerController.gameObject.GetComponent<CustomUI>();
                playerUI.SetText("<color=yellow>Saving...</color>");
                playerUI.Show(true);

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
                    playerUI.SetText("<color=green>Saved game!</color>");
                }
                catch (Exception ex)
                {
                    RandomizerModBase.mls.LogError("Exception caught during custom value serialization. " + ex.Message);
                    playerUI.SetText("<color=red>Failed to save game!</color>");
                    return;
                }
                playerUI.FadeOut(1);
            }
            else
            {
                return;
            }
        } 
    }
}
