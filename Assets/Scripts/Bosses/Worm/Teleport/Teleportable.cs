using System;
using UnityEngine;
using Util;

namespace Bosses.Worm.Teleport
{
    public class Teleportable : MonoBehaviour
    {
        private SpriteRenderer _sRenderer;
        
        private void Start()
        {
            _sRenderer = GetComponent<SpriteRenderer>();
            _sRenderer.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
            _sRenderer.sortingLayerID = SortingLayer.NameToID("Temp Masks");
            _sRenderer.sortingOrder = 639;
        }

        public void Exit()
        {
            _sRenderer.maskInteraction = SpriteMaskInteraction.None;
            _sRenderer.sortingLayerID = SortingLayer.NameToID("Default");
            _sRenderer.sortingOrder = 0;
        }
    }
}