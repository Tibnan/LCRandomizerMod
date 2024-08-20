using UnityEngine;

namespace LCRandomizerMod
{
    internal class RFlashlightProperties
    {
        [SerializeField]
        private Color bulbColor;
        [SerializeField]
        private Color flashlightBodyColor;
        [SerializeField]
        private float intensity;

        public RFlashlightProperties()
        {
            float r, g, b;

            r = new System.Random().Next(1, 101) / 100f;
            g = new System.Random().Next(1, 101) / 100f;
            b = new System.Random().Next(1, 101) / 100f;
            this.bulbColor = new Color(r, g, b);

            r = new System.Random().Next(1, 101) / 100f;
            g = new System.Random().Next(1, 101) / 100f;
            b = new System.Random().Next(1, 101) / 100f;
            this.flashlightBodyColor = new Color(r, g, b);

            this.intensity = new System.Random().Next(1, 21) / 10f;
        }

        public RFlashlightProperties(Color bulbColor, Color flashlightBodyColor, float intensity)
        {
            this.bulbColor = bulbColor;
            this.flashlightBodyColor = flashlightBodyColor;
            this.intensity = intensity;
        }

        public Color BulbColor
        {
            get
            {
                return this.bulbColor;
            }
        }

        public Color FlashlightBodyColor
        {
            get
            {
                return this.flashlightBodyColor;
            }
        }

        public float Intensity
        {
            get
            {
                return this.intensity;
            }
        }
    }
}
