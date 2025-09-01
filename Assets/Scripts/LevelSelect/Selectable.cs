using UnityEngine;

namespace LevelSelect
{
    public class Selectable : MonoBehaviour
    {
        public Selector selector;
        public bool clickAgainDeselect = true;
        public bool clickable;
        
        private void OnMouseUpAsButton()
        {
            selector.SetPosition(clickable && (!clickAgainDeselect || selector.Position != transform.position) ?
                transform.position : null);
        }
    }
}
