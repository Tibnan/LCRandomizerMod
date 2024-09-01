using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LCRandomizerMod
{
    internal class ClockDataContainer : MonoBehaviour
    {
        public int secondsPassed = 0;
        public int minutesPassed = 0;
        public bool tickOrTock = true;
        public float timeOfLastSecond;
    }
}
