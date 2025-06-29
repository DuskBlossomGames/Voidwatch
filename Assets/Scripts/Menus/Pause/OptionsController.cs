using System;
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
        public Slider hudSizeSlider, minimapSizeSlider;

        private KeybindController[] _keybinds;
        public bool BindingKey => _keybinds.Any(kc => kc.BindingKey);

        private void Awake() // setup listeners
        {
            // AUDIO
            masterSlider.onValueChanged.AddListener(v => SettingsInterface.SetChannelVolume(SoundChannel.Master, v));
            musicSlider.onValueChanged.AddListener(v => SettingsInterface.SetChannelVolume(SoundChannel.Music, v));
            effectsSlider.onValueChanged.AddListener(v => SettingsInterface.SetChannelVolume(SoundChannel.Effects, v));

            // VIDEO
            resolution.onValueChanged.AddListener(SettingsInterface.SetResolution);
            
            fullscreen.Setup();
            fullscreen.OnToggle += SettingsInterface.SetFullscreen;
            
            vsync.Setup();
            vsync.OnToggle += SettingsInterface.SetVsync;
            
            hudSizeSlider.onValueChanged.AddListener(v => SettingsInterface.SetHUDSize(v/10));
            minimapSizeSlider.onValueChanged.AddListener(v => SettingsInterface.SetMinimapSize(v/10));

            // CONTROLS
            _keybinds = GetComponentsInChildren<KeybindController>(true);
            for (var i = 0; i < _keybinds.Length; i++)
            {
                var constI = i;
                
                _keybinds[i].Setup();
                _keybinds[i].OnKeybindChange += k => SettingsInterface.SetKeybind(constI, k);
            }
        }

        public void OnEnable() // setup values based on prefs
        {
            // AUDIO
            masterSlider.value = PlayerPrefs.GetFloat("MasterVolume");
            musicSlider.value = PlayerPrefs.GetFloat("MusicVolume");
            effectsSlider.value = PlayerPrefs.GetFloat("EffectsVolume");
            
            // VIDEO
            resolution.ClearOptions();
            resolution.AddOptions(SettingsInterface.resolutions.Select(r => $"{r.width} x {r.height}").ToList());
            resolution.value = PlayerPrefs.GetInt("Resolution");
            
            fullscreen.SetValue(PlayerPrefs.GetInt("Fullscreen") == 1);
            vsync.SetValue(PlayerPrefs.GetInt("Vsync") == 1);
            hudSizeSlider.value = PlayerPrefs.GetFloat("HUDSize")*10;
            minimapSizeSlider.value = PlayerPrefs.GetFloat("MinimapSize")*10;
            
            // CONTROLS
            for (var i = 0; i < _keybinds.Length; i++) _keybinds[i].DisplayKey((KeyCode) PlayerPrefs.GetInt($"Control{i}"));
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
                _keybinds[i].DisplayKey(InputManager.DEFAULT_ACTIONS[i]);
                SettingsInterface.SetKeybind(i, InputManager.DEFAULT_ACTIONS[i]);
            }
        }
    }
}