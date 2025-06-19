using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Util;
using static Static_Info.PlayerData;

namespace Static_Info
{
    public class StaticInfoHolder : MonoBehaviour
    {
        private static StaticInfoHolder _instance;
        private static string _returnScene;
        
        public static StaticInfoHolder Instance
        {
            get
            {
#if UNITY_EDITOR
                if (_instance == null)
                {
                    // very scuffed way of initializing it .-. TODO: REMOVE IN PRODUCTION
                    _returnScene = SceneManager.GetActiveScene().name;
                    SceneManager.LoadScene("TitleScreen");
                }
#endif
                
                return _instance;
            }
        }

        private static IEnumerator LoadInfo()
        {
            SceneManager.LoadScene("LevelSelect");
            yield return new WaitForSeconds(0.1f);
            SceneManager.LoadScene(_returnScene);
        }
        
        private readonly Dictionary<Type, MonoBehaviour> _behaviors = new();
        
        private void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            PlayerDataInstance.Health = PlayerDataInstance.maxHealth;
            
            if (_returnScene != null) StartCoroutine(LoadInfo());
        }

        // TODO DEBUG: remove
#if UNITY_EDITOR
        private void Update() { if (Time.timeScale != 0) Time.timeScale = InputManager.GetKey(KeyCode.F2) ? 5 : 1; }
#endif
        
        public T GetCachedComponent<T>() where T : MonoBehaviour
        {
            if (!_behaviors.TryGetValue(typeof(T), out var ret))
            {
                ret = GetComponent<T>();
                _behaviors.Add(typeof(T), ret);
            }
            
            return (T)ret;
        }
    }
}