using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Util;

namespace Bosses.Worm
{
    public class TailController : MonoBehaviour
    {
        public GameObject riftPrefab;
        public float riftDist;
        public float riftScale;

        public Vector2[] positionOffsets;
        public float[] rotationDegrees;
        
        public GameObject[] spikes;
        public Sprite[] partialSprites;
        public int shootSpeed;
        public int growFrames;

        private Sprite[] _origSprites;
        
        private void OnEnable()
        {
            _origSprites = spikes.Select(o => o.GetComponent<SpriteRenderer>().sprite).ToArray();
            positionOffsets = positionOffsets.Select(v => transform.localScale * v).ToArray();
        }
        
        public void ReleaseSpike(int index)
        {
            var orig = spikes[index];

            var spike = Instantiate(orig, null);
            spike.transform.localScale = orig.transform.lossyScale;
            spike.transform.localRotation = orig.transform.rotation;
            spike.transform.localPosition = orig.transform.position;
            var velocity = spike.GetComponent<CustomRigidbody2D>().velocity = shootSpeed * (orig.transform.rotation * 
                                                                             new Vector2(-Mathf.Cos(rotationDegrees[index] * Mathf.Deg2Rad),
                                                                                 Mathf.Sin(rotationDegrees[index] * Mathf.Deg2Rad)));

            var rift = Instantiate(riftPrefab, null);
            
            var src = rift.GetComponent<SpikeRiftController>();
            src.Spike = spike;
            src.Index = index;
            
            rift.SetActive(true);
            rift.transform.localPosition = spike.transform.localPosition + orig.transform.rotation * positionOffsets[index] + riftDist/shootSpeed * (Vector3) velocity;
            rift.transform.eulerAngles = spike.transform.eulerAngles - new Vector3(0, 0, rotationDegrees[index]);
            rift.transform.localScale = new Vector3(riftScale, riftScale, 1);
            
            orig.SetActive(false);
        }

        public IEnumerator RegrowSpikes()
        {
            for (var i = 0; i < spikes.Length; i++)
            {
                var spike = spikes[i];
                
                spike.SetActive(true);
                spike.GetComponent<SpriteRenderer>().sprite = partialSprites[i];
            }
            
            yield return new WaitForSeconds(Time.deltaTime * growFrames);
            
            for (var i = 0; i < spikes.Length; i++)
            {
                spikes[i].GetComponent<SpriteRenderer>().sprite = _origSprites[i];
            }
        }
    }
}