using HarmonyLib;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace LCRandomizerMod
{
    internal class WeedKillerFlowermanInteract : NetworkBehaviour
    {
        private SprayPaintItem wkScript;
        private float cooldown = 0f;

        public void Start()
        {
            this.wkScript = this.gameObject.GetComponent<SprayPaintItem>();
        }

        public void Update()
        {
            Traverse traverse = Traverse.Create(this.wkScript);
            if (traverse.Field("sprayCanTank").GetValue<float>() <= 0f || traverse.Field("sprayCanShakeMeter").GetValue<float>() <= 0f || this.wkScript.playerHeldBy == null) return;

            if (this.wkScript.isBeingUsed && this.cooldown <= 0f)
            {
                this.DoFlowermanHit();
            }

            if (this.cooldown > 0f)
            {
                this.cooldown -= Time.deltaTime;
            }
        }

        private void DoFlowermanHit()
        {
            RaycastHit hit;
            if (Physics.Raycast(this.wkScript.playerHeldBy.gameplayCamera.transform.position, this.wkScript.playerHeldBy.gameplayCamera.transform.forward, out hit, 5f, 524288))
            {
                Collider[] colliders = Physics.OverlapSphere(hit.point, 2f, 524288, QueryTriggerInteraction.Collide);
                FlowermanAI hitFlowerman = null;
                foreach (Collider collider in colliders)
                {
                    FlowermanAI flowerman = collider.transform.GetComponentInParent<FlowermanAI>();
                    if (flowerman != null && !flowerman.isEnemyDead)
                    {
                        hitFlowerman = flowerman;
                        break;
                    }
                }

                if (hitFlowerman != null && !hitFlowerman.isEnemyDead)
                {
                    hitFlowerman.HitEnemy(1, this.wkScript.playerHeldBy, false);

                    if (!RandomizerValues.slowedFlowermen.ContainsKey(hitFlowerman))
                    {
                        RandomizerValues.slowedFlowermen.Add(hitFlowerman, this.StartCoroutine(this.RemoveFlowermanWithDelay(hitFlowerman)));
                    }
                    else
                    {
                        this.StopCoroutine(RandomizerValues.slowedFlowermen.GetValueSafe(hitFlowerman));
                        RandomizerValues.slowedFlowermen[hitFlowerman] = this.StartCoroutine(this.RemoveFlowermanWithDelay(hitFlowerman));
                    }
                    this.cooldown = 1f;
                }
            }
        }

        private IEnumerator RemoveFlowermanWithDelay(FlowermanAI flowerman)
        {
            yield return new WaitForSeconds(2f);
            RandomizerValues.slowedFlowermen.Remove(flowerman);
        }
    }
}
