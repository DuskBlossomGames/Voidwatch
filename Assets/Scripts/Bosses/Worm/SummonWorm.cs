using System;
using UnityEngine;
using Util;
using Random = UnityEngine.Random;

namespace Bosses.Worm
{
    public class SummonWorm : MonoBehaviour
    {
        public GameObject wormPrefab;

        public float emergeTime;
        
        public float summonCooldown;
        public float summonChance;
        
        public bool active;

        public Sprite squareSprite;

        private readonly Timer _summonTimer = new();
        private readonly Timer _cooldownTimer = new();
        private readonly Timer _summonChanceCooldown = new();

        private float _wormLength;
        private float _segLength;
        private int _numSegs;

        private Transform _maskObj;
        private Transform _summoning;

        private void Start()
        {
            var builder = wormPrefab.GetComponent<WormSegmentBuilder>();
            var segScale = builder.segmentPrefab.transform.localScale;
            _segLength = segScale.x;
            _numSegs = builder.length;
            _wormLength = builder.segmentPrefab.GetComponent<WormSegment>().segLength * (_numSegs+1);

            _maskObj = new GameObject("Worm mask").transform;
            _maskObj.SetParent(transform);
            var scale = transform.lossyScale;
            _maskObj.localScale = new Vector3(segScale.x / scale.x, segScale.y / scale.y, 1);
            _maskObj.localPosition = new Vector3(scale.x / 4 - _segLength / scale.x / 2, 0, 0);
            
            var mask = _maskObj.gameObject.AddComponent<SpriteMask>();
            mask.sprite = squareSprite;
            mask.backSortingOrder = (mask.frontSortingOrder = 639) - 1;
            mask.isCustomRangeActive = true;
        }

        private void SetupSegment(int idx, bool enabled, bool masked)
        {
            var seg = _summoning.GetChild(idx).gameObject;
            foreach (var sRenderer in seg.GetComponentsInChildren<SpriteRenderer>())
            {
                sRenderer.enabled = enabled;
                sRenderer.maskInteraction = masked
                    ? SpriteMaskInteraction.VisibleOutsideMask
                    : SpriteMaskInteraction.None;
                sRenderer.sortingOrder = masked ? 639 : 0;

            }
            seg.GetComponent<Collider2D>().enabled = enabled;
        }

        private void UpdateWorm()
        {
            var head = _summoning.GetChild(0).GetComponent<WormSegment>();
            head.enabled = false;

            var rigid = head.GetComponent<CustomRigidbody2D>();
            var angle = transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
            var velocity = _wormLength / emergeTime;
            rigid.velocity = velocity * new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            for (var i = 0; i < _numSegs; i++)
            {
                SetupSegment(i, (float)i / _numSegs < 1 - _summonTimer.Progress, !_summonTimer.IsFinished);
            }

            if (_summonTimer.IsFinished)
            {
                head.enabled = true;
                rigid.velocity = Vector2.zero;

                _summoning.SetParent(null, true);
                _summoning = null;
                _summonTimer.Value = 0;
            }
        }
        
        private void Update()
        {
            _cooldownTimer.Update();
            _summonTimer.Update();
            _summonChanceCooldown.Update();

            if (_summonTimer.IsActive)
            {
                if (_summoning != null)
                {
                    UpdateWorm();
                }
                else
                {
                    _summonTimer.Value = 0;
                }
            }
            
            if (_summonChanceCooldown.IsFinished && _cooldownTimer.IsFinished && active)
            {
                _summonChanceCooldown.Value = 1;

                if (Random.value < summonChance)
                {
                    _summonTimer.Value = emergeTime;
                    _cooldownTimer.Value = summonCooldown + emergeTime;

                    var scale = transform.lossyScale;

                    _summoning = Instantiate(wormPrefab, transform).transform;
                    _summoning.localRotation = Quaternion.Euler(0, 0, 180);
                    _summoning.localScale = new Vector3(1 / scale.x, 1 / scale.y, 0);
                    _summoning.localPosition = new Vector3(scale.x / 4 - _segLength / scale.x / 2, 0, 0);

                    _summoning.GetComponent<WormSegmentBuilder>().buildCallback = UpdateWorm;
                }
            }
        }
    }
}