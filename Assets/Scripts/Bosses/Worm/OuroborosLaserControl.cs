using System;
using System.Collections;
using System.Collections.Generic;
using Spawnables;
using UnityEngine;
using Util;
using Random = UnityEngine.Random;

namespace Bosses.Worm
{
    public class OuroborosLaserControl : MonoBehaviour
    {
        [NonSerialized] public bool isShooting;

        private Transform _laser;
        private void Start()
        {
            _laser = transform.GetChild(0);
        }

        public IEnumerator Shoot(float length, float timeToLive)
        {
            isShooting = true;

            _laser.localScale = new Vector3(length/transform.lossyScale.x, _laser.localScale.y, 1);
            _laser.position = transform.position + transform.rotation * new Vector3(length/2, 0, 0);
            
            yield return new WaitForSeconds(timeToLive);

            _laser.localScale = new Vector3(0, _laser.localScale.y, 1);
            isShooting = false;
        }
    }
}