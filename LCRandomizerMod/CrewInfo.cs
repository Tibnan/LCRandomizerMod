using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LCRandomizerMod
{
    internal class CrewInfo
    {
        private string name;
        private bool isDead;
        private bool isInsideFactory;
        private int enemyCount = 0;

        public CrewInfo(string name, bool isDead, bool isInsideFactory, Vector3 playerPos)
        {
            this.name = name;
            this.isDead = isDead;
            this.isInsideFactory = isInsideFactory;
            this.enemyCount = GetEnemyCountAtPlayer(playerPos);
        }

        private int GetEnemyCountAtPlayer(Vector3 playerPos)
        {
            Collider[] colliders = Physics.OverlapSphere(playerPos, 20f, 2621448, QueryTriggerInteraction.Collide);
            List<EnemyAI> enemies = new List<EnemyAI>();
            if (colliders.Length > 0)
            {
                foreach (Collider collider in colliders)
                {
                    EnemyAI enemyAI = collider.gameObject.GetComponentInParent<EnemyAI>();
                    if (enemyAI != null && !enemyAI.isEnemyDead)
                    {
                        if (!enemies.Contains(enemyAI))
                        {
                            enemies.Add(enemyAI);
                        }
                    }
                }
                RandomizerModBase.mls.LogError("ENEMY COUNT: " + enemies.Count);
            }
            return enemies.Count;
        }

        public string Name
        {
            get { return name; }
        }

        public bool IsDead
        {
            get { return isDead; }
        }

        public bool IsInsideFactory
        {
            get { return isInsideFactory; }
        }

        public int EnemyCount
        {
            get { return enemyCount; }
        }
    }
}
