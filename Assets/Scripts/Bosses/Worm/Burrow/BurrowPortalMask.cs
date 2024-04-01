using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Util;
using UnityEngine.UI;

namespace Bosses.Worm
{
    public class BurrowPortalMask : MonoBehaviour
    {
        [NonSerialized] public bool enteringBurrow;
        
        private Collider2D _collider;
        private void Start()
        {
            _collider = GetComponent<Collider2D>();
            
            var mask = GetComponent<SpriteMask>();
            mask.isCustomRangeActive = true;
            mask.frontSortingOrder = 32001;
            mask.backSortingOrder = 31999;
        }

        private Dictionary<int, Collider2D> _contained = new();

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!IsBurrowable(other)) return;
            
            Debug.Log(other.transform.parent.gameObject.name + " " + other.GetInstanceID());
        }

        // on fully enter
        private void OnTriggerStay2D(Collider2D other)
        {
            if (!IsBurrowable(other) || _contained.ContainsKey(other.GetInstanceID()) || !UtilFuncs.FullyContains(_collider.bounds, other.bounds)) return;
            _contained[other.GetInstanceID()] = other;
            
            // if it's the end, destroy
            if (other.CompareTag("BurrowableEnd"))
            {
                foreach (var coll in _contained.Values.ToList())
                {
                    OnTriggerExit2D(coll);
                }
                OnTriggerExit2D(other);
            }
            else
            {
                var sRenderer = other.GetComponent<SpriteRenderer>();
                
                if (enteringBurrow) sRenderer.maskInteraction = SpriteMaskInteraction.None;
                else sRenderer.enabled = false;
            }
        }

        // on fully exit
        private void OnTriggerExit2D(Collider2D other)
        {
            if (!IsBurrowable(other) || !_contained.ContainsKey(other.GetInstanceID())) return;
            _contained.Remove(other.GetInstanceID());
            
            var sRenderer = other.GetComponent<SpriteRenderer>();
            
            sRenderer.maskInteraction = enteringBurrow
                ? SpriteMaskInteraction.VisibleOutsideMask
                : SpriteMaskInteraction.VisibleInsideMask;
            if (!enteringBurrow) sRenderer.enabled = true;
            
            if (other.CompareTag("BurrowableEnd")) Destroy(transform.parent.gameObject);
        }

        private static bool IsBurrowable(Component other)
        {
            return other.CompareTag("Burrowable") || other.CompareTag("BurrowableEnd");
        }
    }
}