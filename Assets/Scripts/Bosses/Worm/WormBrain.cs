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

        private GameObject[] _segments;
        private Rigidbody2D _rigid;
        private float _speed;

        private void Start()
        {
            _segments = new GameObject[middleLength + 2];

            _segments[0] = head;
            _segments[1] = middle;
            _segments[^1] = tail;

            var tailPos = tail.transform.localPosition;
            tailPos.x -= middle.transform.localScale.x * (middleLength - 1);
            tail.transform.localPosition = tailPos;

            for (var i = 2; i < middleLength + 1; i++)
            {
                var segment = (_segments[i] = Instantiate(middle)).transform;
                segment.SetParent(transform);

                var pos = _segments[i - 1].transform.localPosition;
                pos.x -= segment.localScale.x;
                segment.localPosition = pos;
            }
            
            _speed = 2;
            _rigid = head.GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            // var dir = pathfinder.PathDirNorm(transform.position, targetPosition);
            //
            // var angle = Mathf.Atan2(dir.y, dir.x);
            // transform.rotation = Quaternion.Euler(0, 0, -90 + Mathf.Rad2Deg * angle);
            //
            // _rigid.velocity = _speed * dir;
        }
    }
}