using System;
using System.Collections;
using Static_Info;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Util;
using static Static_Info.Statistics;

namespace Menus
{
    public class PauseMenuController : OptionsHolder
    {
        public GameObject canvas, buttons;
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

        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.Escape) || (options.gameObject.activeSelf && options.BindingKey)) return;
            
            if (canvas.activeSelf)
            {
                if (buttons.activeSelf) Resume();
            }
            else if (SceneManager.GetActiveScene().name != "TitleScreen")
            {
                if (SceneManager.GetActiveScene().name == "Tutorial") mainMenu.text = "Exit Tutorial";
                else mainMenu.text = "Main Menu";
                
                canvas.SetActive(true);
                Time.timeScale = 0;
                InputManager.isPaused = true;
            }
        }

        public void Resume()
        {
            Time.timeScale = 1;
            InputManager.isPaused = false;
            canvas.SetActive(false);
        }

        public void Options()
        {
            buttons.SetActive(false);
            options.gameObject.SetActive(true);
        }

        public void MainMenu()
        {
            Destroy(StaticInfoHolder.Instance.gameObject);

            SceneManager.LoadScene("TitleScreen");
        }

        public override void ExitOptions()
        {
            options.gameObject.SetActive(false);
            buttons.SetActive(true);
        }
    }
}