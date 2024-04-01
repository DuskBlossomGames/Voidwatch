using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bosses.Worm
{
    public class Burrowable : MonoBehaviour
    {
        public bool tail;
        public string sortingLayer;
        public Sprite squareSprite;
        
        public void Start()
        {
            foreach (var t in GetComponentsInChildren<Transform>())
            {
                if (t.GetComponent<SpriteRenderer>() == null) continue;
                
                var obj = new GameObject("Overlay");
                obj.transform.SetParent(t, false);
                obj.layer = LayerMask.NameToLayer("Triggers");
                obj.tag = tail ? "BurrowableEnd" : "Burrowable";
                
                obj.AddComponent<BoxCollider2D>();
                obj.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
                
                var overlay = obj.AddComponent<SpriteRenderer>();
                overlay.sprite = squareSprite;
                overlay.color = new Color(0.34f, 0.34f, 0.34f, 0.76f);
                overlay.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
                overlay.sortingLayerID = SortingLayer.NameToID(sortingLayer);
                overlay.sortingOrder = 32000;
            }
        }
    }
}