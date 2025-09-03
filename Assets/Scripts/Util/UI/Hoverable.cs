using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Util
{
    public class Hoverable : UIBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public HoveredEvent onHover;
        
        private Func<bool> _interactable;
        protected override void Awake()
        {
            var selectable = GetComponent<Selectable>();
            if (selectable == null) _interactable = () => true;
            else _interactable = () => selectable.IsActive() && selectable.IsInteractable();
        }

        private bool _pointerInside;
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_interactable()) return;
            
            onHover.Invoke(_pointerInside = true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!_interactable()) return;

            onHover.Invoke(_pointerInside = false);
        }

        private void Update()
        {
            if (_pointerInside && !_interactable()) onHover.Invoke(_pointerInside = false);
        }

        [Serializable]
        public class HoveredEvent : UnityEvent<bool> {}
    }
}