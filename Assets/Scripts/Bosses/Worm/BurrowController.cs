using UnityEngine;

namespace Bosses.Worm
{
    public class BurrowController : MonoBehaviour
    {
        public Sprite burrowSprite;

        private bool _burrowing;

        public void SetBurrowing(bool burrowing)
        {
            _burrowing = burrowing;
            
            // TODO: toggle visual
        }
    }
}