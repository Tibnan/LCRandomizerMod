using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;

namespace LCRandomizerMod
{
    public class PandorasBoxItem : GrabbableObject
    {
        public void Awake()
        {
            this.itemProperties = RandomizerValues.modItemsDict.GetValueSafe("PandorasBoxItem");
        }

        public override void DiscardItem()
        {
            base.DiscardItem();
            this.EnableItemMeshes(true);
            this.StartCoroutine(TeleportPlayer(this));
        }

        public override void EquipItem()
        {
            RandomizerModBase.mls.LogError("Picked up");
            TestClientRpc();
        }

        [ClientRpc]
        public void TestClientRpc()
        {
            if (Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                RandomizerModBase.mls.LogError("CLIENTRPC CALLED ON SERVER");
            }
            else
            {
                RandomizerModBase.mls.LogError("CLIENTRPC CALLED ON CLIENT");
            }
        }

        public static IEnumerator TeleportPlayer(PandorasBoxItem pandorasBoxItem)
        {
            for (int i = 0; i < 10; i++)
            {
                GameNetworkManager.Instance.localPlayerController.TeleportPlayer(pandorasBoxItem.transform.position, false, 0f, true, true);
                yield return null;
            }
            yield break;
        }
    }
}
