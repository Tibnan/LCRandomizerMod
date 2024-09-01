using UnityEngine;

namespace LCRandomizerMod
{
    internal class LidSaveData
    {
        [SerializeField]
        private bool isBroken;
        [SerializeField]
        private int hp;

        public LidSaveData()
        {
            this.isBroken = false;
            this.hp = new System.Random().Next(1, 6);
        }

        public LidSaveData(bool broken, int hp)
        {
            this.isBroken = broken;
            this.hp = hp;
        }

        public bool IsBroken
        {
            get { return this.isBroken; }
            set { this.isBroken = value; }
        }

        public int HP
        {
            get { return this.hp; }
            set { this.hp = value; }
        }
    }
}
