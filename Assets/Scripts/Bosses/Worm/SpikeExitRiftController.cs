using System;
using UnityEngine;
using Util;

namespace Bosses.Worm
{
    public class SpikeExitRiftController : MonoBehaviour
    {
        private Action _onDestroy;
        
        private GameObject _spike;
        public GameObject Spike
        {
            get => _spike;
            set
            {
                _spike = value;
                value.GetComponent<DestroyCallback>().Destroyed += _onDestroy;
            }
        }

        private void Awake()
        {
            _onDestroy = () => Destroy(gameObject);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.gameObject == Spike) Destroy(gameObject);
        }

        private void OnDestroy()
        {
            Spike.GetComponent<DestroyCallback>().Destroyed -= _onDestroy;
        }
    }
}