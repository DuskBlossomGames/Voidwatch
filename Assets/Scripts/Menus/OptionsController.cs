using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using Util;

namespace Menus
{
    // TODO: PlayerPrefs?
    public class OptionsController : MonoBehaviour
    {
        public OptionsHolder optionsExiter;
        
        public GameObject buttons;
        public Transform containers;

        public Slider masterSlider, musicSlider, effectsSlider;
        public AudioMixer mixer;

        public TMP_Dropdown resolution;
        private List<Resolution> _resolutions;
        public CheckboxController fullscreen, vsync;

        private KeybindController[] _keybinds;
        public bool BindingKey => _keybinds.Any(kc => kc.BindingKey);
        
        public void Awake()
        {
            // AUDIO
            mixer.GetFloat("MasterVolume", out var vol);
            vol = PlayerPrefs.GetFloat("MasterVolume", vol);
            SetMasterVolume(vol);
            masterSlider.value = (int) (Mathf.Pow(10, vol/20)*100);
            
            mixer.GetFloat("MusicVolume", out vol);
            vol = PlayerPrefs.GetFloat("MusicVolume", vol);
            SetMusicVolume(vol);
            musicSlider.value = (int) (Mathf.Pow(10, vol/20)*100);
            
            mixer.GetFloat("EffectsVolume", out vol);
            vol = PlayerPrefs.GetFloat("EffectsVolume", vol);
            SetEffectsVolume(vol);
            effectsSlider.value = (int) (Mathf.Pow(10, vol/20)*100);
            
            // VIDEO
            resolution.ClearOptions();
            _resolutions = Screen.resolutions
                .Where(r => r.refreshRateRatio.value == Screen.currentResolution.refreshRateRatio.value)
                .OrderByDescending(r => r.width * r.height).ToList();
            resolution.AddOptions(_resolutions.Select(r => $"{r.width} x {r.height}").ToList());
            resolution.value = _resolutions.IndexOf(Screen.currentResolution);
            SetResolution(PlayerPrefs.GetInt("Resolution", resolution.value));

            
            fullscreen.Awake();
            bool isFullscreen;
            SetVsync(isFullscreen = PlayerPrefs.GetInt("Fullscreen", Screen.fullScreenMode == FullScreenMode.MaximizedWindow ? 1 : 0) == 1);
            if (isFullscreen) fullscreen.Toggle();
            fullscreen.OnToggle += SetFullscreen;
            
            vsync.Awake();
            bool isVsync;
            SetVsync(isVsync = PlayerPrefs.GetInt("Vsync", QualitySettings.vSyncCount == 1 ? 1 : 0) == 1);
            if (isVsync) vsync.Toggle();
            vsync.OnToggle += SetVsync;
            
            // CONTROLS
            _keybinds = GetComponentsInChildren<KeybindController>(true);
            for (var i = 0; i < _keybinds.Length; i++)
            {
                var constI = i;
                _keybinds[i].OnKeybindChange += k => SetKeybind(constI, k);
                
                var key = (KeyCode) PlayerPrefs.GetInt($"Control{i}", (int) _keybinds[i].defaultKey);
                _keybinds[i].Awake();
                _keybinds[i].DisplayKey(key);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) && !BindingKey)
            {
                if (_inGroup) Back();
                else optionsExiter.ExitOptions();
            }
        }

        public void BackButton()
        {
            optionsExiter.ExitOptions();
        }

        private bool _inGroup;
        public void SelectGroup(int idx)
        {
            containers.GetChild(idx).gameObject.SetActive(true);
            buttons.SetActive(false);
            _inGroup = true;
        }
        
        public void Back()
        {
            for (var i = 0; i < containers.childCount; i++) containers.GetChild(i).gameObject.SetActive(false);
            buttons.SetActive(true);
            _inGroup = false;
        }

        // AUDIO
        public void SetMasterVolume(float perc)
        {
            PlayerPrefs.SetFloat("MasterVolume", perc);
            mixer.SetFloat("MasterVolume", Mathf.Log10(Mathf.Max(perc, 0.01f)/100)*20);
        }

        public void SetMusicVolume(float perc)
        {
            PlayerPrefs.SetFloat("MusicVolume", perc);
            mixer.SetFloat("MusicVolume", Mathf.Log10(Mathf.Max(perc, 0.01f)/100)*20);
        }

        public void SetEffectsVolume(float perc)
        {
            PlayerPrefs.SetFloat("EffectsVolume", perc);
            mixer.SetFloat("EffectsVolume", Mathf.Log10(Mathf.Max(perc, 0.01f)/100)*20);
        }
        
        // VIDEO
        public void SetResolution(int idx)
        {
            PlayerPrefs.SetInt("Resolution", idx);
            
            var res = _resolutions[idx];
            Screen.SetResolution(res.width, res.height, Screen.fullScreenMode);
        }
        public void SetFullscreen(bool value)
        {
            PlayerPrefs.SetInt("Fullscreen", value ? 1 : 0);;
            
            Screen.fullScreenMode = value ? FullScreenMode.MaximizedWindow : FullScreenMode.Windowed;
        }
        public void SetVsync(bool value)
        {
            PlayerPrefs.SetInt("Vsync", value ? 1 : 0);;
            
            QualitySettings.vSyncCount = value ? 1 : 0;
        }
        
        // CONTROLS
        public void DefaultKeybinds()
        {
            for (var i = 0; i < _keybinds.Length; i++)
            {
                SetKeybind(i, _keybinds[i].defaultKey);
                _keybinds[i].DisplayKey(_keybinds[i].defaultKey);
            }
        }
        public void SetKeybind(int control, KeyCode key)
        {
            PlayerPrefs.SetInt($"Control{control}", (int) key);
            InputManager.InputActions[(InputAction) control] = key;
        }
    }
}