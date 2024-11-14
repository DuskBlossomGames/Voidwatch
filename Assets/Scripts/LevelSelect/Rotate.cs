using System;
using UnityEngine;

namespace LevelSelect
{
    public class Rotate : MonoBehaviour
    {
        public float secPerRot;
        
        private void Update()
        {
            transform.Rotate(Vector3.forward, -360/secPerRot * Time.deltaTime);
        }
    }
}