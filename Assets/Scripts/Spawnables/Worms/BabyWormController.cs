using Player;
using UnityEngine;
using Util;

namespace Spawnables.Worms
{
    public class BabyWormController : MonoBehaviour
    {
        public float juicePerSec; // how much it drains
        public int length;
        public float spread;
        public float acceleration;
        public float maxSpeed;
        public float feedTime;
        public float waitTime;
        public GameObject head, body, tail;

        private SnakePathfinder _pathfinder;
        private CustomRigidbody2D _rb;
        private BoxCollider2D _collider;

        private Vector2 _attachOffset;
        
        private float _segLength;
        
        private readonly Timer _feedTimer = new();
        private readonly Timer _waitTimer = new();

        private float _speed;
        private Quaternion _spreadRot;
        private Vector2 _target;

        private Movement _player;
        private Collider2D _playerCollider;
        
        private void Start()
        {
            _pathfinder = GetComponent<SnakePathfinder>();
            _rb = head.GetComponent<CustomRigidbody2D>();
            _collider = head.GetComponent<BoxCollider2D>();
            _player = GameObject.FindGameObjectWithTag("Player").GetComponent<Movement>();
            _playerCollider = _player.GetComponent<Collider2D>();
            
            _spreadRot = Quaternion.Euler(0, 0, (transform.GetSiblingIndex() - transform.parent.childCount / 2f) * spread);

            GetComponent<MultiDamageable>().HealthChanged += (o, n) =>
            {
                if (n < o && !_feedTimer.IsFinished)
                {
                    _feedTimer.Value = 0;
                    _waitTimer.Value = waitTime;
                }
            };
            
            _segLength = body.transform.lossyScale.y;

            var localPos = Vector3.zero;

            for (var i = 0; i < length - 2; i++)
            {
                localPos.y -= body.transform.localScale.y;

                var seg = Instantiate(body, transform);
                seg.transform.localPosition = localPos;
            }
            Destroy(body);
            
            localPos.y -= _segLength;
            tail.transform.localPosition = localPos;
            tail.transform.SetAsLastSibling();
        }

        private int _curlDir = -1;
        private void Update()
        {
            var dist = ((Vector2)(_player.transform.position - head.transform.position)).sqrMagnitude;
            if (dist < 1.5f*1.5f && !_player.Dodging)
            {
                if (_waitTimer.IsFinished)
                {
                    if (_feedTimer.IsFinished)
                    {
                        _feedTimer.Value = feedTime;
                        _attachOffset = head.transform.position - _player.transform.position;
                    }
                    _player.DrainDodgeJuice(juicePerSec * Time.deltaTime);
                }
            }
            else if ((dist > 2f*2f || _player.Dodging) && !_feedTimer.IsFinished)
            {
                _feedTimer.Value = 0;
                _waitTimer.Value = waitTime;
            }

            if (_waitTimer.IsFinished) _curlDir = -1;
            Vector2 dir;
            if (!_feedTimer.IsFinished)
            {
                _speed = _player.GetComponent<CustomRigidbody2D>().velocity.magnitude;
                
                _feedTimer.Update();
                if (_feedTimer.IsFinished) _waitTimer.Value = waitTime;

                head.transform.position = _player.transform.position + (Vector3) _attachOffset;
                dir = _pathfinder.PathDirNorm(head.transform.position, _player.transform.position);
            }
            else if (!_waitTimer.IsFinished)
            {
                if (_curlDir == -1) _curlDir = Mathf.RoundToInt(Random.value);
                _waitTimer.Update();
                
                _speed *= Mathf.Pow(0.2f, Time.deltaTime);

                var vectors = new Vector3[]
                {
                    head.transform.rotation * Vector3.up,
                    _pathfinder.PathDirNorm(head.transform.position, tail.transform.position)
                };
                dir = Vector3.RotateTowards(vectors[_curlDir],vectors[(_curlDir+1) % 2], Time.deltaTime, 0);
            }
            else
            {
                _speed = Mathf.Min(_speed + acceleration * Time.deltaTime, maxSpeed);
                dir = _spreadRot * _pathfinder.PathDirNorm(head.transform.position, _player.transform.position);
            }
            
            head.transform.rotation = Quaternion.Euler(0, 0, -90 + Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
            _rb.velocity = _feedTimer.IsFinished ? dir * _speed : Vector2.zero;
            
            RippleSegments();
        }

        private void RippleSegments()
        {
            for (var i = 1; i < transform.childCount; i++)
            {
                // make every segment point towards the previous one, then drag it up to connect
                var curr = transform.GetChild(i);
                var prev = transform.GetChild(i - 1);

                var prevEnd = prev.position - prev.rotation * Vector3.up * _segLength/2;
                Vector2 currToPrev = (curr.position - prevEnd).normalized;
                curr.rotation = Quaternion.Euler(0, 0, 90 + Mathf.Atan2(currToPrev.y, currToPrev.x) * Mathf.Rad2Deg);
                
                curr.position = prevEnd - curr.rotation * Vector3.up * _segLength/2;
            }
        }
    }
}