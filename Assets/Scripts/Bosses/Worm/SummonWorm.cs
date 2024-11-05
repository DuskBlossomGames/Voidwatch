using System;
using TMPro;
using UnityEngine;
using Util;
using Random = UnityEngine.Random;

namespace Bosses.Worm
{
    public class SummonWorm : MonoBehaviour
    {
        public GameObject wormPrefab;
        public GameObject rift;

        public float emergeTime;
        
        private readonly Timer _summonTimer = new();

        private float _segLength;
        private int _numSegs;

        private System.Collections.Generic.List<Vector3> _oldSegPos;

        private Transform _summoning;
        
        private void Start()
        {
            var builder = wormPrefab.GetComponent<WormSegmentBuilder>();
            _segLength = builder.segmentPrefab.GetComponent<WormSegment>().segLength;
            _numSegs = builder.length;
        }

        private void SetupSegment(int idx, bool enabled, bool masked)
        {
            var obj = _summoning.GetChild(idx).gameObject;
            foreach (var sRenderer in obj.GetComponentsInChildren<SpriteRenderer>())
            {
                sRenderer.enabled = enabled;
                sRenderer.maskInteraction = masked
                    ? SpriteMaskInteraction.VisibleOutsideMask
                    : SpriteMaskInteraction.None;
                sRenderer.sortingOrder = masked ? 639 : 0;

            }
            obj.GetComponent<Collider2D>().enabled = enabled;
            obj.GetComponent<MiniMapIcon>().enabled = enabled;
        }

        private void UpdateWorm()
        {
            var head = _summoning.GetChild(0).GetComponent<WormSegment>();
            if (head.enabled)
            {
                _oldSegPos = new System.Collections.Generic.List<Vector3>();
                for (var i = 0; i < _numSegs; i++) { 
                    SetupSegment(i, enabled: false, masked: true);
                    _summoning.GetChild(i).GetComponent<WormSegment>().enabled = false;
                    _oldSegPos.Add(_summoning.GetChild(i).transform.position);
                }
            }
            head.enabled = false;

            var headRigid = head.GetComponent<CustomRigidbody2D>();
            var angle = transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
            int segID = Mathf.FloorToInt((1 - _summonTimer.Progress) * _numSegs);
            float segProg = (1-_summonTimer.Progress) * _numSegs % 1;

            if (_summonTimer.IsFinished)
            {
                for (var i = 0; i < _numSegs; i++)
                {
                    _summoning.GetChild(i).GetComponent<WormSegment>().enabled = true;
                }

                headRigid.velocity = Vector2.zero;
                
                _summoning.SetParent(null, true);
                _summonTimer.Value = 0;
                rift.SetActive(false);
                return;
            }

            _summoning.GetChild(segID).localPosition = new Vector3(-.2f - _segLength * segProg, 0, 0);
            for (int i = segID - 1; i >= 0; i--)
            {
                var lpi = _summoning.GetChild(i).position;
                var lpj = _summoning.GetChild(i + 1).position;
                var prevrot = (_summoning.GetChild(i+1).rotation.eulerAngles.z-90) * Mathf.Deg2Rad;

                var tarpot = lpj + -_segLength * (Vector3)UtilFuncs.AngleToVector(prevrot + .5f * Mathf.Cos(2 * Time.time));
                var rpot = UtilFuncs.LerpSafe(lpi, tarpot, Time.deltaTime);
                rpot += .1f * (_oldSegPos[i] - _summoning.GetChild(i).position);

                _summoning.GetChild(i).position = lpi = lpj + _segLength * (rpot-lpj).normalized;
                _summoning.GetChild(i).rotation = Quaternion.Euler(0, 0, Mathf.Atan2(-(rpot - lpj).y, -(rpot - lpj).x) * Mathf.Rad2Deg + 90);

            }

            SetupSegment(segID, enabled: false, masked: true);
            if(segID+1 < _numSegs) SetupSegment(segID+1, enabled: true, masked: false);

            for (var i = 0; i < _numSegs; i++)
            {
                SetupSegment(i, enabled: (float)i / _numSegs < 1 - _summonTimer.Progress, masked: !_summonTimer.IsFinished);
                _oldSegPos[i] = _summoning.GetChild(i).position;
            }
        }
        
        private void Update()
        {
            _summonTimer.Update();

            if (_summonTimer.IsActive)
            {
                if (_summoning != null)
                {
                    UpdateWorm();
                }
                else
                {
                    _summonTimer.Value = 0;
                    rift.SetActive(false);
                }
            }
        }

        public void TrySummon()
        {
            if (_summoning != null) return;
            
            _summonTimer.Value = emergeTime;

            rift.SetActive(true);
                    
            var scale = transform.lossyScale;

            _summoning = Instantiate(wormPrefab, transform).transform;
            _summoning.localRotation = Quaternion.Euler(0, 0, 180);
            _summoning.localScale = new Vector3(1 / scale.x, 1 / scale.y, 0);
            _summoning.localPosition = new Vector3(scale.x / 4 - _segLength / scale.x / 2, 0, 0);
            _summoning.localPosition = Vector3.zero; //Dont remove, breaks everything

            _summoning.GetComponent<WormSegmentBuilder>().buildCallback = UpdateWorm;
        }
    }
}