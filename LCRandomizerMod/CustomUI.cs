using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using GameNetcodeStuff;

namespace LCRandomizerMod
{
    internal class CustomUI : NetworkBehaviour
    {
        private enum LookContext { InteractShipTP, InTerminalMenu, None }

        private GameObject canvasObject;
        private GameObject canvasTextObject;
        private Text canvasText;
        private Coroutine fadeCoroutine;
        private PlayerControllerB ownerPlayer;
        private LookContext context = LookContext.None;
        private bool updateOverridden = false;

        private void Awake()
        {
            this.canvasObject = new GameObject();
            this.canvasObject.transform.parent = base.transform;
            this.canvasObject.name = "CustomUICanvas";
            Canvas canvasText = this.canvasObject.AddComponent<Canvas>();
            this.canvasObject.SetActive(false);
            canvasText.renderMode = RenderMode.ScreenSpaceOverlay;
            this.canvasObject.AddComponent<CanvasScaler>();
            CanvasGroup canvasGroup = this.canvasObject.AddComponent<CanvasGroup>();
            canvasGroup.blocksRaycasts = false;
            this.canvasObject.AddComponent<GraphicRaycaster>();
            this.canvasTextObject = new GameObject();
            this.canvasTextObject.name = "CustomUICanvasText";
            Text text = this.canvasTextObject.AddComponent<Text>();
            this.canvasText = text;
            this.canvasTextObject.transform.localPosition = new Vector3(canvasText.GetComponent<RectTransform>().rect.width / 2f - 20f, canvasText.GetComponent<RectTransform>().rect.height / 2f - 50f, 0f);
            text.text = "";
            text.font = RandomizerModBase.modFont;
            text.alignment = TextAnchor.LowerCenter;
            text.rectTransform.sizeDelta = new Vector2(500f, 400f);
            text.fontSize = 26;
            text.transform.parent = this.canvasObject.transform;
            text.supportRichText = true;
            this.ownerPlayer = GameNetworkManager.Instance.localPlayerController;
        }

        public static void BroadcastMessage(string msg, int fadeDuration = 0)
        {
            if (GameNetworkManager.Instance.localPlayerController.IsServer)
            {
                char[] chars = msg.ToCharArray();

                FastBufferWriter fastBufferWriter = new FastBufferWriter(sizeof(int) + sizeof(char) * chars.Length + sizeof(int), Unity.Collections.Allocator.Temp, -1);
                fastBufferWriter.WriteValueSafe<int>(chars.Length);

                for (int i = 0; i < chars.Length; i++)
                {
                    fastBufferWriter.WriteValueSafe<char>(chars[i]);
                }

                fastBufferWriter.WriteValueSafe<int>(fadeDuration);

                NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Tibnan.lcrandomizermod_" + "ClientReceivesBroadcastMsg", fastBufferWriter, NetworkDelivery.Reliable);

                CustomUI playerUI = GameNetworkManager.Instance.localPlayerController.gameObject.GetComponent<CustomUI>();
                playerUI.ShowLocalMessage(msg, fadeDuration);
            }
        }

        public static void ProcessBroadcastMessage(ulong _, FastBufferReader reader)
        {
            int arrayLength;
            string msg = "";
            char c;
            int duration;

            reader.ReadValueSafe<int>(out arrayLength);

            for (int i = 0; i < arrayLength; i++)
            {
                reader.ReadValueSafe<char>(out c);
                msg += c;
            }

            reader.ReadValueSafe<int>(out duration);

            CustomUI playerUI = GameNetworkManager.Instance.localPlayerController.gameObject.GetComponent<CustomUI>();
            playerUI.ShowLocalMessage(msg, duration);
        }

        private void Update()
        {
            if (this.updateOverridden || !this.ownerPlayer.isInHangarShipRoom) return;

            RaycastHit hit;
            this.context = LookContext.None;
            if (Physics.Raycast(this.ownerPlayer.gameplayCamera.transform.position, this.ownerPlayer.gameplayCamera.transform.forward, out hit, 3f, 2816))
            {
                if (hit.transform.GetComponent<Terminal>() != null)
                {
                    this.context = LookContext.InTerminalMenu;
                    this.SetText(RandomizerValues.mapRandomizedInTerminal ? "<color=red>Terminal Recalculating...</color>" : "<color=white>Available Commands\nRandom\nRevive</color>", overrideUpdate: false);
                    this.Show(true);
                }
                else if (hit.transform.GetComponentInParent<ShipTeleporter>() != null && this.ownerPlayer.currentlyHeldObjectServer?.gameObject?.GetComponent<LungProp>() != null)
                {
                    this.context = LookContext.InteractShipTP;
                    this.SetText("<color=white>Insert Apparatus</color>", overrideUpdate: false);
                    this.Show(true);
                }
            }

            if (this.context == LookContext.None)
            {
                this.Show(false);
            }
        }

        public void SetText(string text, bool overrideUpdate = true)
        {
            this.canvasText.text = text;
            this.updateOverridden = overrideUpdate;
        }

        public void Show(bool show)
        {
            this.canvasObject.SetActive(show);
        }

        public void FadeOut(int duration = 0)
        {
            if (this.fadeCoroutine != null)
            {
                this.StopCoroutine(fadeCoroutine);
            }
            this.fadeCoroutine = this.StartCoroutine(FadeTextOutCoroutine(duration));
        }

        public void ShowLocalMessage(string msg, int duration = 0)
        {
            this.SetText(msg);
            this.Show(true);
            this.FadeOut(duration);
        }

        private IEnumerator FadeTextOutCoroutine(int duration)
        {
            yield return new WaitForSeconds(duration);
            float alpha = 1f;
            while(alpha > 0f)
            {
                alpha -= 0.01f;
                //RandomizerModBase.mls.LogError("SETTING ALPHA: " + alpha);
                this.canvasText.canvasRenderer.SetAlpha(alpha);
                yield return null;
            }
            this.Show(false);
            this.canvasText.canvasRenderer.SetAlpha(1f);
            //RandomizerModBase.mls.LogError("Fade coroutine ended.");
            this.updateOverridden = false;
            yield break;
        }
    }
}
