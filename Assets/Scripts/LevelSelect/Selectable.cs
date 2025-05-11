using UnityEngine;

namespace Util
{
    public class Selectable : MonoBehaviour
    {
        public Selector selector;
        public bool clickable;
        
        private void OnMouseUpAsButton()
        {
            if (!clickable) return;
            
            selector.SetPosition(selector.Position != transform.position ?
                transform.position : null);
        }
    }
}