﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Netcode;

namespace LCRandomizerMod.Patches
{
    [HarmonyPatch(typeof(TimeOfDay))]
    internal class TimeOfDayPatch
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
    }
}
