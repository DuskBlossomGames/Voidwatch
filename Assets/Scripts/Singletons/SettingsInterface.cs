using System;
using System.Collections.Generic;
using System.Linq;
using Analytics;
using Unity.Services.Analytics;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;
using UnityEngine.Audio;
using Util;

namespace Singletons
{
    public class SettingsInterface : MonoBehaviour
    {
        public enum Rank { Cadet, Lieutenant, Captain, General }
        public static Rank rank;

        public static bool isFirstTime;
        
        private static SettingsInterface _instance;
        public static List<Resolution> resolutions;
        private static int _currentResolution;
        
        public AudioMixer mixer;
        private void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            // ===== init on awake things =====
            AnalyticsManager.Init();
            // ================================
            
            resolutions = Screen.resolutions
                .Where(r => r.refreshRateRatio.value == Screen.currentResolution.refreshRateRatio.value)
                .OrderByDescending(r => r.width * r.height).ToList();

            // MISC
            isFirstTime = !PlayerPrefs.HasKey("check");
            if (isFirstTime)
            {
                // FIRST LAUNCH ITEMS
                PlayerPrefs.DeleteAll(); // clear preferences (should already be, but still)
                AnalyticsManager.LogEvent(new FirstLaunchEvent());
            }
            PlayerPrefs.SetInt("check", 1);
            
            SetRank((Rank) PlayerPrefs.GetInt("rank"));

            // CONTROLS
            for (var control = 0; control < InputAction.Count; control++)
            {
                SetKeybind(control, (KeyCode) PlayerPrefs.GetInt($"Control{control}", (int) InputManager.DEFAULT_ACTIONS[control]));
            }

            // VIDEO
            SetFullscreen(PlayerPrefs.GetInt("Fullscreen", 1) == 1);
            SetVsync(PlayerPrefs.GetInt("Vsync", 1) == 1);
            SetHUDSize(PlayerPrefs.GetFloat("HUDSize", 1));
            SetMinimapSize(PlayerPrefs.GetFloat("MinimapSize", 1));
            SetResolution(PlayerPrefs.GetInt("Resolution", resolutions.IndexOf(Screen.currentResolution)));
        }

        public void Start()
        {
            // AUDIO (has to be done in start for mixer to be initialized)
            foreach (SoundChannel channel in Enum.GetValues(typeof(SoundChannel)))
            {
                SetChannelVolume(channel, PlayerPrefs.GetFloat(channel+"Volume", 100));
            }
        }

        private static void SetRank(Rank rank)
        {
            PlayerPrefs.SetInt("rank", (int) rank);
            
            SettingsInterface.rank = rank;
        }

        public static void SetMinRank(Rank rank) // safer than exposing SetRank
        {
            if (SettingsInterface.rank < rank) SetRank(rank);
        }
        
        public static void SetChannelVolume(SoundChannel channel, float perc)
        {
            PlayerPrefs.SetFloat(channel+"Volume", perc);

            _instance.mixer.SetFloat(channel+"Volume", Mathf.Log10(Mathf.Max(perc, 0.01f)/100)*20);
        }

        public static void SetResolution(int idx)
        {
            if (idx == -1) idx = 0;
            PlayerPrefs.SetInt("Resolution", idx);

            _currentResolution = idx;
            var res = resolutions[idx];
            Screen.SetResolution(res.width, res.height, Screen.fullScreenMode);
        }
        
        public static void SetFullscreen(bool value)
        {
            PlayerPrefs.SetInt("Fullscreen", value ? 1 : 0);;
            
            var res = resolutions[_currentResolution];
            Screen.SetResolution(res.width, res.height, value ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
        }
        
        public static void SetVsync(bool value)
        {
            PlayerPrefs.SetInt("Vsync", value ? 1 : 0);;
            
            QualitySettings.vSyncCount = value ? 1 : 0;
        }

        public static float HUDSize { get; private set; }
        public static void SetHUDSize(float value)
        {
            PlayerPrefs.SetFloat("HUDSize", value);

            HUDSize = value;
        }
        
        public static float MinimapSize { get; private set; }
        public static void SetMinimapSize(float value)
        {
            PlayerPrefs.SetFloat("MinimapSize", value);

            MinimapSize = value;
        }
        
        public static void SetKeybind(int control, KeyCode key)
        {
            PlayerPrefs.SetInt($"Control{control}", (int) key);
            InputManager.InputActions[control] = key;
        }
    }

    public enum SoundChannel { Master, Music, Effects }
}