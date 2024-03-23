using System;
using UnityEngine;

namespace Bosses.Worm
{
    public class WormBrain : MonoBehaviour
    {
        public GameObject head, middle, tail;
        public int middleLength;
        public SnakePathfinder pathfinder;

        public Vector3 targetPosition;
        public GameObject player;

        public float maxTurnAngleDeg;

        private GameObject[] _segments;
        private Rigidbody2D _headRigid;
        private float _speed;
        private Vector2 _targetMovePos;
        private float _segmentDist;
        private float _ouroborosRadius;
        private Vector2 _currdir;

        public enum MoveMode
        {
            Direct,
            Circle,
            Wander,
        } 
        private MoveMode _moveMode;

        private void Start()
        {
            _segments = new GameObject[middleLength + 2];

            _segments[0] = head;
            _segments[1] = middle;
            _segments[^1] = tail;

            var tailPos = tail.transform.localPosition;
            tailPos.x -= middle.transform.localScale.x * (middleLength - 1);
            tail.transform.localPosition = tailPos;

            float totallength = head.transform.localScale.x + tail.transform.localScale.x;

            for (var i = 2; i < middleLength + 1; i++)
            {
                var segment = (_segments[i] = Instantiate(middle)).transform;
                segment.SetParent(transform);

                var pos = _segments[i - 1].transform.localPosition;

                totallength += _segmentDist = segment.localScale.x;
                pos.x -= _segmentDist;
                segment.localPosition = pos;
            }

            _ouroborosRadius = totallength / (2 * Mathf.PI);
            
            _headRigid = head.GetComponent<Rigidbody2D>();
            _moveMode = MoveMode.Direct;
        }

        private void Update()
        {

            _speed = 50;

            {
                switch (_moveMode)
                {
                    case MoveMode.Wander:
                        if ((head.transform.position - targetPosition).sqrMagnitude < 120)
                        {
                            while ((head.transform.position - targetPosition).sqrMagnitude < 4000)
                                targetPosition = UnityEngine.Random.Range(20, 70) * pathfinder.AngleToVector(UnityEngine.Random.Range(0, 6.28f));
                        }
                        break;
                    case MoveMode.Direct:
                        targetPosition = player.transform.position;
                        break;
                    case MoveMode.Circle:
                        targetPosition = Util.UtilFuncs.TangentPointOnCircleFromPoint(Vector2.zero, 20, head.transform.position);
                        break;
                }
                _targetMovePos = targetPosition;
                var dir = pathfinder.PathDirNorm(_segments[0].transform.position, _targetMovePos);
                Vector2 prevAngle = pathfinder.AngleToVector(Mathf.Deg2Rad * _segments[1].transform.rotation.eulerAngles.z);
                dir = pathfinder.ClampAngle(dir, prevAngle, Mathf.Deg2Rad * maxTurnAngleDeg);

                _currdir = dir = (.1f / Time.deltaTime * _currdir + dir).normalized;

                _headRigid.velocity = _speed * dir;

                var angle = Mathf.Atan2(dir.y, dir.x);
                _segments[0].transform.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * angle);
                RippleSegments();
            }
        }

        private void RippleSegments()
        {
            for (var i = 1; i < middleLength + 1; i++)
            {
                Vector3 nextSegmentPos = _segments[i - 1].transform.position;
                Vector3 currSegmentPos = _segments[i].transform.position;
                Vector3 prevSegmentPos = _segments[i + 1].transform.position;

                Vector3 curr2next = (nextSegmentPos - currSegmentPos).normalized;
                currSegmentPos = nextSegmentPos + -_segmentDist * curr2next;
                if(((Vector2)currSegmentPos).sqrMagnitude <= _ouroborosRadius * _ouroborosRadius)
                {
                    currSegmentPos += .1f * (Vector3)((Vector2)currSegmentPos).normalized;
                }
                _segments[i].transform.position = currSegmentPos;

                Vector3 prev2curr = (currSegmentPos - prevSegmentPos).normalized;
                Vector3 meanDir = .5f * (prev2curr + curr2next);

                var angle = Mathf.Atan2(meanDir.y, meanDir.x);
                _segments[i].transform.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * angle);
            }

            { //Scope naming stuffs
                Vector3 nextSegmentPos = _segments[^2].transform.position;
                Vector3 currSegmentPos = _segments[^1].transform.position;

                Vector3 curr2next = (nextSegmentPos - currSegmentPos).normalized;
                _segments[^1].transform.position = currSegmentPos = nextSegmentPos + -_segmentDist * curr2next;

                var angle = Mathf.Atan2(curr2next.y, curr2next.x);
                _segments[^1].transform.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * angle);
            }
        }
    }
}