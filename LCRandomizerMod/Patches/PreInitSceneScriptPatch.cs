using HarmonyLib;
using UnityEngine.Video;
using UnityEngine;
using System.Collections;

namespace LCRandomizerMod.Patches
{
    [HarmonyPatch(typeof(PreInitSceneScript))]
    internal class PreInitSceneScriptPatch : MonoBehaviour
    {
        private static GameObject gameObject = new GameObject();

        [HarmonyPatch("Awake")]
        [HarmonyPrefix]
        public static void PlayIntro(PreInitSceneScript __instance)
        {
            VideoPlayer videoPlayer = gameObject.AddComponent<VideoPlayer>();
            videoPlayer.clip = RandomizerValues.introVideo;
            __instance.mainAudio.clip = RandomizerValues.introAudio;
            videoPlayer.SetTargetAudioSource(2, __instance.mainAudio);
            videoPlayer.playOnAwake = false;
            videoPlayer.renderMode = VideoRenderMode.CameraNearPlane;
            videoPlayer.targetCamera = GameObject.FindObjectOfType<Camera>();
            videoPlayer.isLooping = false;
            gameObject.transform.position = __instance.continueButton.transform.position;

            __instance.StartCoroutine(PlayIntroCoroutine(__instance, videoPlayer));
        }

        public static IEnumerator PlayIntroCoroutine(PreInitSceneScript __instance, VideoPlayer videoPlayer)
        {
            __instance.continueButton.SetActive(true);
            videoPlayer.Prepare();
            while (!videoPlayer.isPrepared)
            {
                yield return null;
            }

            videoPlayer.enabled = true;
            videoPlayer.Play();

            __instance.mainAudio.Play();

            yield return new WaitWhile(() => videoPlayer.isPlaying);

            videoPlayer.enabled = false;
            gameObject = null;
        }

        [HarmonyPatch(nameof(PreInitSceneScript.HoverButton))]
        [HarmonyPrefix]
        public static bool SFXFix(PreInitSceneScript __instance)
        {
            if (gameObject != null)
            {
                if (gameObject.GetComponent<VideoPlayer>().isPlaying)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return true;
            }
        }
    }
}
