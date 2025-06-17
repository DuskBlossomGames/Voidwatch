using UnityEngine;

namespace LevelSelect
{
    public class Selectable : MonoBehaviour
    {
        public Selector selector;
        public bool clickable;
        
        private void OnMouseUpAsButton()
        {
            selector.SetPosition(clickable && selector.Position != transform.position ?
                transform.position : null);
        }
    }
}
