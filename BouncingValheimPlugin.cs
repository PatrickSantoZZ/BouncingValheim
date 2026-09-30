using System;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace BouncingValheim
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class BouncingValheimPlugin : BaseUnityPlugin
    {
        private const string ModName = "BouncingValheim";
        private const string ModVersion = "1.0.0";
        private const string Author = "com.nekonekoo.mod";
        private const string ModGUID = "com.nekonekoo.mod.BouncingValheim";

        private static string ConfigFileName = $"{ModGUID}.cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar.ToString() + ConfigFileName;

        private readonly Harmony HarmonyInstance = new Harmony(ModGUID);
        public static readonly ManualLogSource BouncingLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

        public static ConfigEntry<bool> CE_EnableSound;
        public static ConfigEntry<float> CE_AudioVolume;
        public static ConfigEntry<bool> CE_EnableForPlayer;
        public static ConfigEntry<bool> CE_EnableForNPCs;
        public static ConfigEntry<float> CE_BounceSpeed;

        private DateTime _lastReloadTime;
        private const long RELOAD_DELAY = 10000000L;

        private void AddConfig<T>(string key, string section, string description, T value, ref ConfigEntry<T> configEntry)
        {
            configEntry = base.Config.Bind<T>(section, key, value, new ConfigDescription(description, null, Array.Empty<object>()));
        }

        public void Awake()
        {
            BouncingLogger.LogInfo("BouncingValheim loaded successfully!");

            this.AddConfig<bool>("EnableSound", "Audio", "enables the background bouncing sound.", true, ref CE_EnableSound);
            this.AddConfig<float>("Volume", "Audio", "Volume of the sound (0.0 to 1.0).", 0.1f, ref CE_AudioVolume);
            this.AddConfig<bool>("EnableForPlayer", "Bounce", "enables the bounce effect for the player.", true, ref CE_EnableForPlayer);
            this.AddConfig<bool>("EnableForNPCs", "Bounce", "enables the bounce effect for NPCs.", true, ref CE_EnableForNPCs);
            this.AddConfig<float>("BounceSpeed", "Bounce", "speed of the bounce animation.", 14.0f, ref CE_BounceSpeed);
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            this.HarmonyInstance.PatchAll(executingAssembly);

            this.SetupWatcher();

            BouncingValheim.InitAudio();
        }

        private void OnDestroy()
        {
            base.Config.Save();
            BouncingValheim.UnloadAudio();
        }

        private void SetupWatcher()
        {
            this._lastReloadTime = DateTime.Now;
            FileSystemWatcher fileSystemWatcher = new FileSystemWatcher(Paths.ConfigPath, ConfigFileName);
            fileSystemWatcher.Changed += this.ReadConfigValues;
            fileSystemWatcher.Created += this.ReadConfigValues;
            fileSystemWatcher.Renamed += new RenamedEventHandler(this.ReadConfigValues);
            fileSystemWatcher.IncludeSubdirectories = true;
            fileSystemWatcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            fileSystemWatcher.EnableRaisingEvents = true;
        }

        private void ReadConfigValues(object sender, FileSystemEventArgs e)
        {
            DateTime now = DateTime.Now;
            long num = now.Ticks - this._lastReloadTime.Ticks;

            if (!File.Exists(ConfigFileFullPath) || num < RELOAD_DELAY)
            {
                return;
            }

            try
            {
                BouncingLogger.LogInfo("Attempting to reload configuration...");
                base.Config.Reload();
                BouncingValheim.UpdateVolume();
            }
            catch
            {
                BouncingLogger.LogError("There was an issue loading " + ConfigFileName);
                return;
            }

            this._lastReloadTime = now;
        }

        public static bool GetEnableSound() => CE_EnableSound != null && CE_EnableSound.Value;
        public static float GetAudioVolume() => CE_AudioVolume == null ? 0.1f : Mathf.Clamp01(CE_AudioVolume.Value);
        public static bool GetEnableForPlayer() => CE_EnableForPlayer == null || CE_EnableForPlayer.Value;
        public static bool GetEnableForNPCs() => CE_EnableForNPCs == null || CE_EnableForNPCs.Value;
        public static float GetBounceSpeed() => CE_BounceSpeed == null ? 14.0f : CE_BounceSpeed.Value;
    }
}