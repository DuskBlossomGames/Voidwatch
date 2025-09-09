using System;
using System.Collections;
using System.Linq;
using Singletons.Static_Info;
using TMPro;
using Tutorial;
using UnityEngine;
using UnityEngine.SceneManagement;
using Util;
using static Singletons.Static_Info.PlayerData;
using static Singletons.Static_Info.LevelSelectData;

namespace Menus.Pause
{
    public class PauseMenuController : OptionsHolder
    {
        public GameObject tip, canvas, buttons, upgrades;
        public TextMeshProUGUI mainMenu;
        public OptionsController options;
        
        private static PauseMenuController _instance;

        private void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            StartCoroutine(DontDestroyOnLoadAsync());
            
            SceneManager.sceneLoaded += (_, _) => Resume();
        }
        
        private IEnumerator DontDestroyOnLoadAsync()
        {
            DontDestroyOnLoad(gameObject);
            yield return null;
        }

        private void TryPause()
        {
            if (SceneManager.GetActiveScene().name == "TitleScreen") return;
            
            mainMenu.text = PlayerDataInstance.IsTutorial ? "Exit Tutorial" : "Main Menu";
                
            canvas.SetActive(true);
            Time.timeScale = 0;
            InputManager.isPaused = true;

            upgrades.SetActive(LevelSelectDataInstance.VisitedPlanets.Count > 1);
            FindAnyObjectByType<MusicTransitionManager>().enabled = false;
            foreach (var source in FindObjectsByType<AudioSource>(FindObjectsSortMode.None)) source.Pause();
        }
        
        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.Escape) || (options.gameObject.activeSelf && options.BindingKey)) return;
            
            if (canvas.activeSelf)
            {
                if (buttons.activeSelf) Resume();
            }
            else TryPause();
        }
        
        private void OnApplicationFocus(bool focused)
        {
#if !UNITY_EDITOR
            if (!focused) TryPause();
#endif
        }

        public void Resume()
        {
            Time.timeScale = 1;
            InputManager.isPaused = false;
            canvas.SetActive(false);
            foreach (var source in FindObjectsByType<AudioSource>(FindObjectsSortMode.None)) source.UnPause();
            FindAnyObjectByType<MusicTransitionManager>().enabled = true;
        }

        public void Options()
        {
            buttons.SetActive(false);
            tip.SetActive(false);
            options.gameObject.SetActive(true);
        }

        public void MainMenu()
        {
            Destroy(StaticInfoHolder.Instance.gameObject);
            if (PlayerDataInstance.IsTutorial) FindAnyObjectByType<TutorialController>().ExitTutorial();

            SceneManager.LoadScene("TitleScreen");
        }

        public override void ExitOptions()
        {
            options.gameObject.SetActive(false);
            buttons.SetActive(true);
            tip.SetActive(true);
        }
    }
}