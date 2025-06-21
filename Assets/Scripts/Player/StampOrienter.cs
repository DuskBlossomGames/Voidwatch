using UnityEngine;

namespace Player
{
    public class StampOrienter : MonoBehaviour
    {
        public float correctionPerSec;
        
        private Camera _cam;
        
        private void Start()
        {
            _cam = Camera.main!;
            transform.rotation = _cam.transform.rotation;
        }
        
        private void Update()
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, _cam.transform.rotation, correctionPerSec * Time.deltaTime);
        }
    }
}