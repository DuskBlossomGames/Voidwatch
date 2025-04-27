using System;
using System.Collections.Generic;
using System.Linq;
using Static_Info;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
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
                if (_instance == null)
                {
                    // very scuffed way of initializing it .-. TODO: REMOVE IN PRODUCTION
                    _returnScene = SceneManager.GetActiveScene().name;
                    SceneManager.LoadScene("TitleScreen");
                }
                return _instance;
            }
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
            
            if (_returnScene != null) SceneManager.LoadScene(_returnScene);
        }

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