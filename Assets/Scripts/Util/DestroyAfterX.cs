using UnityEngine;

namespace Util
{
    public class DestroyAfterX : MonoBehaviour
    {
        public float killTime;
        private float _timeToDeath;

        private void Start()
        {
            _timeToDeath = killTime;
        }

        private void Update()
        {
            _timeToDeath -= Time.deltaTime;
            if(_timeToDeath <= 0)
            {
                Destroy(gameObject);
            }
        
        }
    }
}
