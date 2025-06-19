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
    public class OptionsController : MonoBehaviour
    {
        public OptionsHolder optionsExiter;
        
        public GameObject buttons;
        public Transform containers;

        public Slider masterSlider, musicSlider, effectsSlider;
        public AudioMixer mixer;

        public TMP_Dropdown resolution;
        public CheckboxController fullscreen, vsync;

        private KeybindController[] _keybinds;
        public bool BindingKey => _keybinds.Any(kc => kc.BindingKey);
        
        public void Awake()
        {
            // AUDIO
            masterSlider.value = PlayerPrefs.GetFloat("MasterVolume");
            masterSlider.onValueChanged.AddListener(v => SettingsInterface.SetChannelVolume(SoundChannel.Master, v));
            
            musicSlider.value = PlayerPrefs.GetFloat("MusicVolume");
            musicSlider.onValueChanged.AddListener(v => SettingsInterface.SetChannelVolume(SoundChannel.Music, v));
            
            effectsSlider.value = PlayerPrefs.GetFloat("EffectsVolume");
            effectsSlider.onValueChanged.AddListener(v => SettingsInterface.SetChannelVolume(SoundChannel.Effects, v));
            
            // VIDEO
            resolution.ClearOptions();
            resolution.AddOptions(SettingsInterface.resolutions.Select(r => $"{r.width} x {r.height}").ToList());
            resolution.value = PlayerPrefs.GetInt("Resolution");
            resolution.onValueChanged.AddListener(SettingsInterface.SetResolution);
            
            fullscreen.Setup();
            fullscreen.SetValue(PlayerPrefs.GetInt("Fullscreen") == 1);
            fullscreen.OnToggle += SettingsInterface.SetFullscreen;
            
            vsync.Setup();
            vsync.SetValue(PlayerPrefs.GetInt("Vsync") == 1);
            vsync.OnToggle += SettingsInterface.SetVsync;
            
            // CONTROLS
            _keybinds = GetComponentsInChildren<KeybindController>(true);
            for (var i = 0; i < _keybinds.Length; i++)
            {
                var constI = i;
                _keybinds[i].OnKeybindChange += k => SettingsInterface.SetKeybind(constI, k);
                
                _keybinds[i].Setup();
                _keybinds[i].DisplayKey((KeyCode) PlayerPrefs.GetInt($"Control{i}"));
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

        public void DefaultKeybinds()
        {
            for (var i = 0; i < _keybinds.Length; i++)
            {
                _keybinds[i].DisplayKey(InputManager.DEFAULT_ACTIONS[(InputAction) i]);
                SettingsInterface.SetKeybind(i, InputManager.DEFAULT_ACTIONS[(InputAction) i]);
            }
        }
    }
}