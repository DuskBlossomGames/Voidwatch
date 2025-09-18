using System.Linq;
using Analytics;
using Menus.Util;
using Singletons;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using Util;

namespace Menus.Pause
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

        int _sentMasterVolume, _sentMusicVolume, _sentEffectsVolume, _sentHUDSize, _sentMinimapSize;
        private void Awake() // setup listeners
        {
            // AUDIO
            masterSlider.onValueChanged.AddListener(v =>
            {
                SettingsInterface.SetChannelVolume(SoundChannel.Master, v);
                if (_sentMasterVolume++ == 0) AnalyticsManager.LogEvent(new EditOptionEvent { OptionId = "master_volume" });
            });
            musicSlider.onValueChanged.AddListener(v =>
            {
                SettingsInterface.SetChannelVolume(SoundChannel.Music, v);
                if (_sentMusicVolume++ == 0) AnalyticsManager.LogEvent(new EditOptionEvent { OptionId = "music_volume" });
            });
            effectsSlider.onValueChanged.AddListener(v =>
            {
                SettingsInterface.SetChannelVolume(SoundChannel.Effects, v);
                if (_sentEffectsVolume++ == 0) AnalyticsManager.LogEvent(new EditOptionEvent { OptionId = "effects_volume" });
            });

            // VIDEO
            resolution.onValueChanged.AddListener(r =>
            {
                SettingsInterface.SetResolution(r);
                AnalyticsManager.LogEvent(new EditOptionEvent { OptionId = "resolution" });
            });
            
            fullscreen.Setup();
            fullscreen.OnToggle += f =>
            {
                SettingsInterface.SetFullscreen(f);
                AnalyticsManager.LogEvent(new EditOptionEvent { OptionId = "fullscreen" });
            };
            
            vsync.Setup();
            vsync.OnToggle += v =>
            {
                SettingsInterface.SetVsync(v);
                AnalyticsManager.LogEvent(new EditOptionEvent { OptionId = "vsync" });
            };
            
            hudSizeSlider.onValueChanged.AddListener(v =>
            {
                SettingsInterface.SetHUDSize(v / 10);
                if (_sentHUDSize++ == 0) AnalyticsManager.LogEvent(new EditOptionEvent { OptionId = "hud_size" });
            });
            minimapSizeSlider.onValueChanged.AddListener(v =>
            {
                SettingsInterface.SetMinimapSize(v / 10);
                if (_sentMinimapSize++ == 0) AnalyticsManager.LogEvent(new EditOptionEvent { OptionId = "minimap_size" });
            });

            // CONTROLS
            _keybinds = GetComponentsInChildren<KeybindController>(true);
            for (var i = 0; i < _keybinds.Length; i++)
            {
                var constI = i;
                
                _keybinds[i].Setup();
                _keybinds[i].OnKeybindChange += k =>
                {
                    SettingsInterface.SetKeybind(constI, k);
                    AnalyticsManager.LogEvent(new EditOptionEvent { OptionId = "keybinds" });
                };
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
            
            _sentMasterVolume = _sentMusicVolume = _sentEffectsVolume = _sentHUDSize = _sentMinimapSize = 0;
            AnalyticsManager.LogEvent(new VisitScreenEvent { ScreenId = "options_base" });
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
            
            AnalyticsManager.LogEvent(new VisitScreenEvent { ScreenId = $"options_{containers.GetChild(idx).gameObject.name.ToLower()}" }); // audio, video, controls
        }
        
        public void Back()
        {
            for (var i = 0; i < containers.childCount; i++) containers.GetChild(i).gameObject.SetActive(false);
            buttons.SetActive(true);
            _inGroup = false;
            _sentMasterVolume = _sentMusicVolume = _sentEffectsVolume = _sentHUDSize = _sentMinimapSize = 0;
        }

        public void DefaultKeybinds()
        {
            for (var i = 0; i < _keybinds.Length; i++)
            {
                _keybinds[i].DisplayKey(InputManager.DEFAULT_ACTIONS[i]);
                SettingsInterface.SetKeybind(i, InputManager.DEFAULT_ACTIONS[i]);
            }
        }

        public void ResetAllData()
        {
            SettingsInterface.ResetData();
        }
    }
}