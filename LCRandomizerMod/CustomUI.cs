using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace LCRandomizerMod
{
    internal class CustomUI : NetworkBehaviour
    {
        private GameObject canvasObject;
        private GameObject canvasTextObject;
        private Text canvasText;
        private Coroutine fadeCoroutine;

        private void Awake()
        {
            this.canvasObject = new GameObject();
            this.canvasObject.transform.parent = base.transform;
            this.canvasObject.name = "EntranceTeleportCanvas";
            Canvas canvasText = this.canvasObject.AddComponent<Canvas>();
            this.canvasObject.SetActive(false);
            canvasText.renderMode = RenderMode.ScreenSpaceOverlay;
            this.canvasObject.AddComponent<CanvasScaler>();
            this.canvasObject.AddComponent<GraphicRaycaster>();
            this.canvasTextObject = new GameObject();
            this.canvasTextObject.name = "EntranceTeleportCanvasText";
            Text text = this.canvasTextObject.AddComponent<Text>();
            this.canvasText = text;
            this.canvasTextObject.transform.localPosition = new Vector3(canvasText.GetComponent<RectTransform>().rect.width / 2f - 20f, canvasText.GetComponent<RectTransform>().rect.height / 2f - 50f, 0f);
            text.text = "";
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.alignment = TextAnchor.LowerCenter;
            text.rectTransform.sizeDelta = new Vector2(500f, 400f);
            text.fontSize = 26;
            text.transform.parent = this.canvasObject.transform;
            text.supportRichText = true;
            //text.color = new Color(1f, 1f, 1f, 0.5f);
            //this.canvasObject.SetActive(true);
        }

        public void SetText(string text)
        {
            this.canvasText.text = text;
        }

        public void Show(bool show)
        {
            this.canvasObject.SetActive(show);
        }

        public void FadeOut()
        {
            if (this.fadeCoroutine != null)
            {
                this.StopCoroutine(fadeCoroutine);
                RandomizerModBase.mls.LogError("Stopped already running fade coroutine. Starting new one.");
            }
            this.fadeCoroutine = this.StartCoroutine(FadeTextOutCoroutine());
        }

        private IEnumerator FadeTextOutCoroutine()
        {
            float alpha = 1f;
            while(alpha > 0f)
            {
                alpha -= 0.01f;
                RandomizerModBase.mls.LogError("SETTING ALPHA: " + alpha);
                this.canvasText.canvasRenderer.SetAlpha(alpha);
                yield return null;
            }
            this.Show(false);
            this.canvasText.canvasRenderer.SetAlpha(1f);
            RandomizerModBase.mls.LogError("Fade coroutine ended.");
            yield break;
        }
    }
}
