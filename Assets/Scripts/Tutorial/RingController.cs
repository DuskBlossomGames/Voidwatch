using Player;
using UnityEngine;

namespace Tutorial
{
    public class RingController : MonoBehaviour
    {
        public Color completedColor;

        private bool _completed;
        public bool Completed
        {
            get => _completed;
            set
            {
                _completed = value;
                
                if (_sprites == null) return;
                foreach (var sr in _sprites) sr.color = _completed ? completedColor : Color.white;
            }
        }

        private SpriteRenderer[] _sprites;

        private void Start()
        {
            _sprites = GetComponentsInChildren<SpriteRenderer>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<Movement>() == null) return;

            Completed = true;
        }
    }
}