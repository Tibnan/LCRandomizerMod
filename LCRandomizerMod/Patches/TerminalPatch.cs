using HarmonyLib;
using Unity.Netcode;

namespace LCRandomizerMod.Patches
{
    [HarmonyPatch(typeof(Terminal))]
    internal class TerminalPatch
    {
        [HarmonyPatch(nameof(Terminal.OnSubmit))]
        [HarmonyPrefix]
        public static bool CheckForRandom(Terminal __instance)
        {
            if (!RandomizerValues.mapRandomizedInTerminal && StartOfRound.Instance.inShipPhase)
            {
                RandomizerModBase.mls.LogInfo("PARSED SENTENCE:");
                RandomizerModBase.mls.LogInfo(__instance.screenText.text.Substring(__instance.screenText.text.Length - __instance.textAdded).ToLower());

                if (__instance.screenText.text.Substring(__instance.screenText.text.Length - __instance.textAdded).Contains("random"))
                {
                    int num = new System.Random().Next(1, 13);

                    while (num == 11 || num == 3)
                    {
                        num = new System.Random().Next(1, 13);

                        if (TimeOfDay.Instance.timeUntilDeadline < 1080)
                        {
                            num = 3;
                            break;
                        }
                    }

                    StartOfRound.Instance.ChangeLevelServerRpc(num, __instance.groupCredits);
                    __instance.screenText.text = "";
                    __instance.QuitTerminal();
                    RandomizerValues.mapRandomizedInTerminal = true;

                    if (Unity.Netcode.NetworkManager.Singleton.IsServer)
                    {
                        SendTerminalSwitchToClients(0, new FastBufferReader());
                    }
                    else
                    {
                        FastBufferWriter writer = new FastBufferWriter(4, Unity.Collections.Allocator.Temp, -1);
                        Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage("Tibnan.lcrandomizermod_" + "ServerInvokeTerminalSwitch", 0UL, writer, NetworkDelivery.Reliable);
                    }

                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return true;
            }
        }

        [HarmonyPatch("Update")]
        [HarmonyPrefix]
        public static void CheckForAlreadyRandomized(Terminal __instance)
        {
            if (RandomizerValues.mapRandomizedInTerminal && StartOfRound.Instance.inShipPhase && GameNetworkManager.Instance.localPlayerController.inTerminalMenu)
            {
                HUDManager.Instance.AddTextToChatOnServer("<color=red>Terminal locked due to level randomization. It will unlock once you land.</color>", -1);
                GameNetworkManager.Instance.localPlayerController.inTerminalMenu = false;
                __instance.QuitTerminal();
            }
        }

        public static void SwitchTerminalMode(ulong __, FastBufferReader _)
        {
            if (!Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                RandomizerModBase.mls.LogInfo("Switched terminal to non usable");
                RandomizerValues.mapRandomizedInTerminal = true;
            }
        }

        public static void SendTerminalSwitchToClients(ulong _, FastBufferReader __)
        {
            RandomizerValues.mapRandomizedInTerminal = true;
            FastBufferWriter writer = new FastBufferWriter(4, Unity.Collections.Allocator.Temp, -1);
            Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "TerminalRandomizationUsed", writer, NetworkDelivery.Reliable);
        }
    }
}
