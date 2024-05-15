using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LCRandomizerMod.Patches
{
    //[HarmonyPatch(typeof(Terminal))]
    //internal class TerminalPatch
    //{
    //    [HarmonyPatch(nameof(Terminal.SetItemSales))]
    //    [HarmonyPrefix]
    //    public static void ItemSalesOverride(Terminal __instance)
    //    {
    //        if (__instance.itemSalesPercentages == null || __instance.itemSalesPercentages.Length == 0)
    //        {
    //            __instance.itemSalesPercentages = new int[__instance.buyableItemsList.Length];
    //            for (int i = 0; i < __instance.itemSalesPercentages.Length; i++)
    //            {
    //                Debug.Log(string.Format("Item sales percentages #{0}: {1}", i, __instance.itemSalesPercentages[i]));
    //                __instance.itemSalesPercentages[i] = 100;
    //            }
    //        }
    //        System.Random random = new System.Random(StartOfRound.Instance.randomMapSeed + 90);
    //        int num = Mathf.Clamp(random.Next(-10, 5), 0, 5);
    //        if (num <= 0)
    //        {
    //            return;
    //        }
    //        List<int> list = new List<int>();
    //        for (int i = 0; i < __instance.buyableItemsList.Length; i++)
    //        {
    //            list.Add(i);
    //            __instance.itemSalesPercentages[i] = 100;
    //        }
    //        int num2 = 0;
    //        while (num2 < num && list.Count > 0)
    //        {
    //            int num3 = random.Next(0, list.Count);
    //            int maxValue = Mathf.Clamp(__instance.buyableItemsList[num3].highestSalePercentage, 0, 90);
    //            int num4 = 100 - random.Next(0, maxValue);
    //            num4 = RoundToNearestTen(num4);
    //            __instance.itemSalesPercentages[num3] = num4;
    //            list.RemoveAt(num3);
    //            num2++;
    //        }
    //    }

    //    private static int RoundToNearestTen(int i)
    //    {
    //        return (int)Math.Round((double)i / 10.0) * 10;
    //    }
}
