using System;
using System.Collections;
using System.IO;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Networking;

namespace BouncingValheim
{
    public class BouncingValheim
    {
        private static GameObject audioHolder;
        private static AudioSource audioSource;
        private static AudioClip loadedClip;

        private static bool GetBoolConfig(string fieldName, bool defaultValue)
        {
            try
            {
                FieldInfo field = typeof(BouncingValheimPlugin).GetField(fieldName, BindingFlags.Static | BindingFlags.Public);
                if (field != null)
                {
                    object value = field.GetValue(null);
                    if (value != null)
                    {
                        PropertyInfo property = value.GetType().GetProperty("Value");
                        if (property != null)
                        {
                            return (bool)property.GetValue(value);
                        }
                    }
                }
            }
            catch { }
            return defaultValue;
        }

        private static float GetFloatConfig(string fieldName, float defaultValue)
        {
            try
            {
                FieldInfo field = typeof(BouncingValheimPlugin).GetField(fieldName, BindingFlags.Static | BindingFlags.Public);
                if (field != null)
                {
                    object value = field.GetValue(null);
                    if (value != null)
                    {
                        PropertyInfo property = value.GetType().GetProperty("Value");
                        if (property != null)
                        {
                            return (float)property.GetValue(value);
                        }
                    }
                }
            }
            catch { }
            return defaultValue;
        }

        public static void InitAudio()
        {
            if (audioHolder != null) return;

            audioHolder = new GameObject("BouncingValheim_AudioHolder");
            UnityEngine.Object.DontDestroyOnLoad(audioHolder);

            var runner = audioHolder.AddComponent<AudioRunner>();
            runner.StartCoroutine(LoadWavRoutine());
        }

        private static IEnumerator LoadWavRoutine()
        {
            string modFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string audioPath = Path.Combine(modFolder, "Audio", "bounce.wav");

            if (!File.Exists(audioPath))
            {
                BouncingValheimPlugin.BouncingLogger.LogWarning($"Soundfile 'bounce.wav' not found in path: {audioPath}");
                yield break;
            }

            audioSource = audioHolder.AddComponent<AudioSource>();
            audioSource.loop = true;
            audioSource.volume = GetFloatConfig("CE_AudioVolume", 0.1f);

            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + audioPath, AudioType.WAV))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    if (loadedClip != null) UnityEngine.Object.Destroy(loadedClip);

                    loadedClip = DownloadHandlerAudioClip.GetContent(www);
                    audioSource.clip = loadedClip;

                    // check mute or we get audio while loading the game xd
                    CheckAudioMute();
                    audioSource.Play();

                    BouncingValheimPlugin.BouncingLogger.LogInfo("WAV-Sound successfully loaded!");
                }
            }
        }

        public static void UpdateVolume()
        {
            if (audioSource != null)
            {
                audioSource.volume = GetFloatConfig("CE_AudioVolume", 0.5f);
            }
        }

        public static void CheckAudioMute()
        {
            if (audioSource != null)
            {
                // check sound
                bool soundEnabled = GetBoolConfig("CE_EnableSound", true);

                // menu visible?
                bool isMenuOpen = Menu.IsVisible();

                // are we lOwOded into the world?
                bool isInGame = Player.m_localPlayer != null && !Player.m_localPlayer.IsDead();

                // mute when not ingame
                audioSource.mute = !soundEnabled || isMenuOpen || !isInGame;
            }
        }

        public static void UnloadAudio()
        {
            if (audioSource != null) audioSource.Stop();
            if (loadedClip != null) UnityEngine.Object.Destroy(loadedClip);
            if (audioHolder != null) UnityEngine.Object.Destroy(audioHolder);
        }

        [HarmonyPatch(typeof(Character), "UpdateMotion")]
        public static class Patch_Character_UpdateMotion
        {
            private static readonly AccessTools.FieldRef<Character, GameObject> VisualField =
                AccessTools.FieldRefAccess<Character, GameObject>("m_visual");

            public static void Postfix(Character __instance)
            {
                try
                {
                    if (__instance == null) return;

                    GameObject visualObj = VisualField(__instance);
                    if (visualObj == null) return;

                    bool isPlayer = __instance.IsPlayer();

                    if (isPlayer && !BouncingValheim.GetBoolConfig("CE_EnableForPlayer", true)) return;
                    if (!isPlayer && !BouncingValheim.GetBoolConfig("CE_EnableForNPCs", true)) return;

                    float speed = BouncingValheim.GetFloatConfig("CE_BounceSpeed", 14.0f);
                    float rawSin = Mathf.Sin(Time.time * speed);
                    float yOffset = Mathf.Abs(rawSin) * 0.4f;

                    float squashY = 1f - ((1f - Mathf.Abs(rawSin)) * 0.25f);
                    float squashXZ = 1f + ((1f - Mathf.Abs(rawSin)) * 0.125f);

                    Transform visualTransform = visualObj.transform;
                    Vector3 localPos = visualTransform.localPosition;

                    visualTransform.localPosition = new Vector3(localPos.x, yOffset, localPos.z);
                    visualTransform.localScale = new Vector3(squashXZ, squashY, squashXZ);
                }
                catch { }
            }
        }
    }

    public class AudioRunner : MonoBehaviour
    {
        private void Update()
        {
            BouncingValheim.CheckAudioMute();
        }
    }
}