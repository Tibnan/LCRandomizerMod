using System;
using UnityEngine;

namespace LCRandomizerMod
{
    internal class RandomCarProperties
    {
        [SerializeField]
        private Vector3 scale;
        [SerializeField]
        private Color headlightColor;
        [SerializeField]
        private Color carColor;

        public RandomCarProperties()
        {
            this.Regenerate();
        }

        public void Regenerate()
        {
            float scale = Convert.ToSingle(new System.Random().Next(50, 151)) / 100;
            this.scale = new Vector3(scale, scale, scale);

            float r = new System.Random().Next(1, 101) / 100f;
            float g = new System.Random().Next(1, 101) / 100f;
            float b = new System.Random().Next(1, 101) / 100f;
            this.headlightColor = new Color(r, g, b);

            r = new System.Random().Next(1, 301) / 100f;
            g = new System.Random().Next(1, 301) / 100f;
            b = new System.Random().Next(1, 301) / 100f;
            this.carColor = new Color(r, g, b);

            RandomizerValues.randomCarProperties = this;
            RandomizerValues.randomizedCar = true;
        }

        public Vector3 Scale
        {
            get { return this.scale; }
        }

        public Color HeadlightColor
        {
            get { return this.headlightColor; }
        }

        public Color CarColor
        {
            get { return this.carColor; }
        }
    }
}
