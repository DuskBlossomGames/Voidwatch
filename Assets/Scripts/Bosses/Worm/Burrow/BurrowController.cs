using System;
using UnityEngine;
using UnityEngine.Serialization;
using Util;

namespace Bosses.Worm
{
    public class BurrowController : MonoBehaviour
    {
        public GameObject riftPrefab;

        private GameObject _rift;
        
        public void MakeRift(bool enteringBurrow, int riftDist)
        {
            var angle = transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
            _rift = Instantiate(riftPrefab);
            _rift.GetComponentsInChildren<BurrowPortalMask>()[0].enteringBurrow = enteringBurrow;
            
            _rift.transform.localRotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg);
            _rift.transform.localPosition =
                transform.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * riftDist;
        }

        public bool test;
        public bool entering;
        public int dist;

        private void Update()
        {
            if (test)
            {
                test = false;
                MakeRift(entering, dist);
            }
        }
    }
}