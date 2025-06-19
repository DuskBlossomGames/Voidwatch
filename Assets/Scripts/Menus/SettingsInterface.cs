using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using Util;

namespace Menus
{
    public class SettingsInterface : MonoBehaviour
    {
        private static SettingsInterface _instance;
        public static List<Resolution> resolutions;

        public AudioMixer mixer;
        private void Start()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            resolutions = Screen.resolutions
                .Where(r => r.refreshRateRatio.value == Screen.currentResolution.refreshRateRatio.value)
                .OrderByDescending(r => r.width * r.height).ToList();

            // AUDIO
            foreach (SoundChannel channel in Enum.GetValues(typeof(SoundChannel)))
            {
                SetChannelVolume(channel, PlayerPrefs.GetFloat(channel+"Volume", 100));
            }
            
            // VIDEO
            SetResolution(PlayerPrefs.GetInt("Resolution", resolutions.IndexOf(Screen.currentResolution)));
            SetFullscreen(PlayerPrefs.GetInt("Fullscreen", 1) == 1);
            SetVsync(PlayerPrefs.GetInt("Vsync", 1) == 1);
            
            // CONTROLS
            foreach (InputAction control in Enum.GetValues(typeof(InputAction)))
            {
                SetKeybind((int) control, (KeyCode) PlayerPrefs.GetInt($"Control{control}", (int) InputManager.DEFAULT_ACTIONS[(InputAction) control]));
            }
        }
        
        public static void SetChannelVolume(SoundChannel channel, float perc)
        {
            PlayerPrefs.SetFloat(channel+"Volume", perc);

            _instance.mixer.SetFloat(channel+"Volume", Mathf.Log10(Mathf.Max(perc, 0.01f)/100)*20);
        }

        public static void SetResolution(int idx)
        {
            PlayerPrefs.SetInt("Resolution", idx);
            
            var res = resolutions[idx];
            Screen.SetResolution(res.width, res.height, Screen.fullScreenMode);
        }
        
        public static void SetFullscreen(bool value)
        {
            PlayerPrefs.SetInt("Fullscreen", value ? 1 : 0);;
            
            Screen.fullScreenMode = value ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        }
        
        public static void SetVsync(bool value)
        {
            PlayerPrefs.SetInt("Vsync", value ? 1 : 0);;
            
            QualitySettings.vSyncCount = value ? 1 : 0;
        }
        
        public static void SetKeybind(int control, KeyCode key)
        {
            PlayerPrefs.SetInt($"Control{control}", (int) key);
            InputManager.InputActions[(InputAction) control] = key;
        }
    }

    public enum SoundChannel { Master, Music, Effects }
}