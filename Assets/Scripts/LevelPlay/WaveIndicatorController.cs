using System.Collections;
using UnityEngine;

namespace LevelPlay
{
    public class WaveIndicatorController : MonoBehaviour
    {
        public float flashTime;
        public int numFlashes;
        
        private CanvasGroup _cg;

        private void Awake()
        {
            _cg = GetComponent<CanvasGroup>();
        }
        
        public IEnumerator Flash()
        {
            for (var i = 0; i < numFlashes; i++)
            {
                for (float t = 0; t < flashTime; t += Time.fixedDeltaTime)
                {
                    yield return new WaitForFixedUpdate();
                    
                    _cg.alpha = Mathf.SmoothStep(t < flashTime / 2 ? 0 : 1, t < flashTime / 2 ? 1 : 0, (2*t/flashTime)%1);
                }
            }
        }
    }
}