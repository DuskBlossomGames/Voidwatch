using System.Collections;
using Singletons.Static_Info;
using TMPro;
using Tutorial;
using UnityEngine;
using UnityEngine.SceneManagement;
using Util;
using static Singletons.Static_Info.PlayerData;

namespace Menus.Pause
{
    public class PauseMenuController : OptionsHolder
    {
        public GameObject tip, canvas, buttons;
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
                mainMenu.text = PlayerDataInstance.IsTutorial ? "Exit Tutorial" : "Main Menu";
                
                canvas.SetActive(true);
                Time.timeScale = 0;
                InputManager.isPaused = true;

                FindAnyObjectByType<MusicTransitionManager>().enabled = false;
                foreach (var source in FindObjectsByType<AudioSource>(FindObjectsSortMode.None)) source.Pause();
            }
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