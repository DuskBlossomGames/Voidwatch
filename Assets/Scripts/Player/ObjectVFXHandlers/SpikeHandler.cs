using System.Collections;
using UnityEngine;

namespace Player.ObjectVFXHandlers
{
    public class SpikeHandler : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            StartCoroutine(Kill(1.0f));
        }

        // Update is called once per frame
        void Update()
        {

        }

        private IEnumerator Kill(float time)
        {
            yield return new WaitForSeconds(time);
            Destroy(gameObject);
        }
    }
}
