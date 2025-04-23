using System;
using System.Collections.Generic;
using System.Linq;
using Static_Info;
using UnityEngine;

namespace Static_Info
{
    public class StaticInfoHolder : MonoBehaviour
    {
        public static StaticInfoHolder instance;
        
        private readonly Dictionary<Type, MonoBehaviour> _behaviors = new();
        
        private void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);
                return;
            }
            
            instance = this;
            DontDestroyOnLoad(gameObject);
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