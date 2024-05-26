using HarmonyLib;
using UnityEngine;

namespace LCRandomizerMod.Patches
{
    [HarmonyPatch(typeof(TimeOfDay))]
    internal class TimeOfDayPatch : ICustomValue
    {
        [HarmonyPatch(nameof(TimeOfDay.SetNewProfitQuota))]
        [HarmonyPostfix]
        public static void PostNewQuotaRandomization()
        {
            if (Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                StartOfRoundPatch.RandomizeQuotaVariables();
                int newDeadline = StartOfRoundPatch.GenerateNewDeadline();

                TimeOfDay.Instance.timeUntilDeadline = newDeadline;
                int daysUntilDeadline = (int)Mathf.Floor(TimeOfDay.Instance.timeUntilDeadline / TimeOfDay.Instance.totalTime);
                StartOfRound.Instance.deadlineMonitorText.text = string.Format("DEADLINE:\n{0} Days", daysUntilDeadline);

                StartOfRoundPatch.SendQuotaValues(newDeadline, 0);
            }
        }

        public void ReloadStats()
        {
            TimeOfDay.Instance.quotaVariables.baseIncrease = (float)ES3.Load("quotaBaseIncrease", GameNetworkManager.Instance.currentSaveFileName);
            TimeOfDay.Instance.quotaVariables.increaseSteepness = (float)ES3.Load("quotaIncreaseSteepness", GameNetworkManager.Instance.currentSaveFileName);
            TimeOfDay.Instance.quotaVariables.randomizerMultiplier = (float)ES3.Load("quotaRandomizerMultiplier", GameNetworkManager.Instance.currentSaveFileName);
        }

        public void SaveOnExit()
        {
            ES3.Save("quotaBaseIncrease", TimeOfDay.Instance.quotaVariables.baseIncrease, GameNetworkManager.Instance.currentSaveFileName);
            ES3.Save("quotaIncreaseSteepness", TimeOfDay.Instance.quotaVariables.increaseSteepness, GameNetworkManager.Instance.currentSaveFileName);
            ES3.Save("quotaRandomizerMultiplier", TimeOfDay.Instance.quotaVariables.randomizerMultiplier, GameNetworkManager.Instance.currentSaveFileName);

            if (!RandomizerValues.keysToLoad.Contains("quotaBaseIncrease"))
            {
                RandomizerValues.keysToLoad.Add("quotaBaseIncrease");
                RandomizerValues.keysToLoad.Add("quotaIncreaseSteepness");
                RandomizerValues.keysToLoad.Add("quotaRandomizerMultiplier");
            }
        }

        public void SyncStatsWithClients()
        {
            return;
        }
    }
}
