using System;
using System.Collections;
using System.Timers;
using Extensions;
using Player;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Timer = Util.Timer;

namespace Menus
{
    public class UpgradeController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public UpgradeHoverController hover;

        [NonSerialized] public UpgradePlayer.Upgrade Upgrade;

        private RectTransform _rect;

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            hover.PointerEntered(Upgrade, transform.GetSiblingIndex(), (_rect.anchorMax.x - _rect.anchorMin.x)/2 + _rect.anchorMin.x);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hover.PointerExited();
        }
    }
}