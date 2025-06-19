using System.Collections;
using UnityEngine;
using Util;

namespace Spawnables.Controllers.Misslers
{
    public class MissleAimOLD : MonoBehaviour
    {
        public GameObject target;
        public float accel;
        public float cruiseSpeed;
        public float diveSpeed;
        public float fuel;

        private CustomRigidbody2D _rb;
        private Vector2 _dir;
        private float _moveSpeed;

        private void Start()
        {
            _rb = GetComponent<CustomRigidbody2D>();
            StartCoroutine(FindDir());
        }

        private void FixedUpdate()
        {
            if (fuel > 0)
            {
                _rb.AddForce(accel * _moveSpeed / cruiseSpeed * _dir);
                _rb.linearVelocity = Vector2.ClampMagnitude(_rb.linearVelocity, _moveSpeed);
                fuel -= Time.fixedDeltaTime * _moveSpeed;
            }
        }

        IEnumerator FindDir()
        {
            while (true)
            {
                Vector2 relpos = target.transform.position - transform.position;
                if (relpos.sqrMagnitude < 30)
                {
                    _dir = leadShot(relpos, target.GetComponent<CustomRigidbody2D>().linearVelocity - _rb.linearVelocity, diveSpeed);
                    transform.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * Mathf.Atan2(_dir.y, _dir.x));
                    _moveSpeed = diveSpeed;
                    yield return new WaitForFixedUpdate();
                }
                else
                {
                    _dir = leadShot(relpos,target.GetComponent<CustomRigidbody2D>().linearVelocity - _rb.linearVelocity,cruiseSpeed);
                    transform.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * Mathf.Atan2(_dir.y, _dir.x));
                    _moveSpeed = cruiseSpeed;
                    yield return new WaitForFixedUpdate();
                }
            
            }
        }

        Vector2 leadShot(Vector2 relPos, Vector2 relVel, float bulletVel)
        {
            float a = bulletVel * bulletVel - relVel.sqrMagnitude;
            float b = 2 * Vector2.Dot(relPos, relVel);
            float c = relPos.sqrMagnitude;

            float colTime = (b + Mathf.Sqrt(b * b + 4 * a * c)) / (2 * a);
            Vector2 colPos = relPos + colTime * relVel;
            return colPos.normalized;
        }
    }
}
