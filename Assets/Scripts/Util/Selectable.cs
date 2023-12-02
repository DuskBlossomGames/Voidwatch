using UnityEngine;

namespace Util
{
    public class Selectable : MonoBehaviour
    {
        public Selector selector;
        
        private void OnMouseUpAsButton()
        {
            selector.SetPosition(selector.Position != transform.position ?
                transform.position : null);
        }
    }
}