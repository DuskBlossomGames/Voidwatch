using System;
using UnityEngine;

namespace Level_Select
{
    public class Selectable : MonoBehaviour
    {
        public Selector selector;
        
        private void Start()
        {
            if (GetComponent<Collider>() == null)
            {
                Debug.LogWarning("Selectable object has no collider!");
            }
        }

        private void OnMouseUpAsButton()
        {
            selector.SetPosition(selector.Position != transform.position ?
                transform.position : null);
        }
    }
}