using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Util;
using Random = UnityEngine.Random;

namespace Spawnables.Carcadon
{
    public class SpitController : MonoBehaviour
    {
        public float minWidth, maxWidth;
        public float minHeight, maxHeight;
        public float minSpeed, maxSpeed;
        public float minAngle, maxAngle;
        public float minTime, maxTime;
        public float travelTime;

        private Timer _active = new();
        private Timer _wait = new();

        public void Spit(float duration)
        {
            _active.Value = duration;
        }
        
        private Dictionary<GameObject, float> _spawned = new();
        
        public void Update()
        {
            _active.Update();
            if (_active.IsFinished) return;

            _wait.Update();
            if (_wait.IsFinished)
            {
                _wait.Value = Random.Range(minTime, maxTime);
                var obj = Instantiate(transform.GetChild(0).gameObject, null);
                obj.transform.localScale = new Vector3(Random.Range(minWidth, maxWidth), Random.Range(minHeight, maxHeight), 1);
                obj.transform.position = transform.position;
                obj.transform.rotation = transform.rotation * Quaternion.Euler(0, 0, Random.Range(minAngle, maxAngle));
                obj.GetComponent<SpitFade>().TimeToLive = travelTime;
                obj.SetActive(true);
                
                _spawned.Add(obj, Random.Range(minSpeed, maxSpeed) * Time.deltaTime);
            }
            
            foreach (var kvp in new Dictionary<GameObject, float>(_spawned))
            {
                if (kvp.Key == null)
                {
                    _spawned.Remove(kvp.Key);
                    continue;
                }
                
                kvp.Key.transform.position += kvp.Value * kvp.Key.transform.up;
            }
        }
    }
}