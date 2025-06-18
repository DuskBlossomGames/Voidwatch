using System.Collections.Generic;
using UnityEngine;
using Util;

namespace Spawnables.Controllers.Boids
{
    public class SmartBoidHandler : MonoBehaviour
    {
        public float turnAggr;
        public float maxVisionLen;
        public float turnRamp;
        public float turnSmoothness;
        public int numRays;
        public float visionAngleDeg;
        public float responseRate;

        public float turnTowardsPlayer;
        public float turnTowardsPlayerPerSec;
        public float commitAngle;

        public float speed;

        public GameObject target;
        public GameObject gravitySource;
        public float shootDist;
        private CustomRigidbody2D _rigidbody2D;
        private EnemyGunHandler _gun;
        public bool original = true;

        public GameObject collisionObj;
        private List<GameObject> _collisionObjs;

        private LayerMask _colmask;

        void Start()
        {
            _colmask = MaskUtil.COLLISION_MASKS[gameObject.layer];
            _gun = GetComponent<EnemyGunHandler>();
            _rigidbody2D = transform.GetComponent<CustomRigidbody2D>();
            if (target == null) target = GameObject.FindGameObjectWithTag("Player");
            if (gravitySource == null) gravitySource = GameObject.FindGameObjectWithTag("GravitySource");
            if (original)
            {
                original = false;
                transform.parent.GetComponent<BoidSpawner>().Ready();
            }
            //_collisionObjs = new List<GameObject>();
            //_collisionObjs.Add(collisionObj);
            //for (int i = 0; i < numRays - 1; i++)
            //{
            //    _collisionObjs.Add(Instantiate(collisionObj));
            //}
        }

        // Update is called once per frame
        void Update()
        {
            turnTowardsPlayer += turnTowardsPlayerPerSec * Time.deltaTime;
        
            var deltaAngle = new Vector2(Mathf.Cos(Mathf.Deg2Rad*(90 + transform.rotation.eulerAngles.z)), Mathf.Sin(Mathf.Deg2Rad * (90 + transform.rotation.eulerAngles.z)));
            var sumturn = 0f;
            for (int i = 0; i < numRays; i++)
            {
                var unrotAngle = i * visionAngleDeg / (numRays - 1) - visionAngleDeg / 2;
                var rayAngle = unrotAngle + transform.rotation.eulerAngles.z;
                var turnScale = -Mathf.Sign(unrotAngle) * (1 - Mathf.Abs(Mathf.Sin(Mathf.Deg2Rad * unrotAngle)));
                var a = new Vector2(Mathf.Cos(Mathf.Deg2Rad * (90 + rayAngle)), Mathf.Sin(Mathf.Deg2Rad * (90 + rayAngle)));
                //var turnScale = -Mathf.Sign(i - numRays / 2);
                var dist = maxVisionLen;
                var collision = Physics2D.Raycast(1 * (Vector3)a + transform.position, a, maxVisionLen, layerMask: _colmask);
                if (collision.collider != null)
                {
                    dist = collision.distance;
                    //_collisionObjs[i].transform.position = collision.transform.position;
                } else
                {
                    //var a = new Vector2(Mathf.Cos(Mathf.Deg2Rad * (90 + rayAngle)), Mathf.Sin(Mathf.Deg2Rad * (90 + rayAngle)));
                    //_collisionObjs[i].transform.position = maxVisionLen * (Vector3)a + transform.position;
                }
                //Debug.LogFormat("Distance is {0}", dist);
                var turnAmt = TurnAmt(dist);
                sumturn += turnAmt * turnScale * turnAggr;
            }
            var tarDelta = target.transform.position - transform.position;
            var tarAngle = Mathf.Atan2(tarDelta.y, tarDelta.x);
            if (Mathf.Abs(sumturn) <= commitAngle)
            {
                sumturn += turnTowardsPlayer * Mathf.DeltaAngle(transform.rotation.eulerAngles.z + 90, Mathf.Rad2Deg * tarAngle);
                Shoot();
            }
            //sumturn += turnTowardsPlayer * Vector2.SignedAngle(deltaAngle, (target.transform.position - transform.position));
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + Time.deltaTime * sumturn * Vector3.forward);
            //transform.rotation.eulerAngles += Time.deltaTime * sumturn * Vector3.forward;

            _rigidbody2D.velocity = speed * deltaAngle;
            _rigidbody2D.angularVelocity = 0;
            if (((Vector2)transform.position).sqrMagnitude < 15 * 15)
            {
                transform.position = (Vector3)(15 * ((Vector2)transform.position).normalized) + transform.position.z * Vector3.forward;
            }
        }

        float TurnAmt(float dist)
        {
            return (1 + turnSmoothness) / (turnSmoothness + Mathf.Pow(turnRamp, dist / responseRate));
        }

        void Shoot()
        {
            Vector2 diff = target.transform.position - transform.position;
            Vector2 relVel = target.GetComponent<CustomRigidbody2D>().velocity - _rigidbody2D.velocity;
            float angle = UtilFuncs.LeadShot(diff, relVel, _gun.ExpectedVelocity());


            if (diff.sqrMagnitude < shootDist * shootDist)
            {
                //_gun.Shoot(-transform.rotation.z);
                _gun.Shoot(-90 + Mathf.Rad2Deg * angle - transform.rotation.eulerAngles.z);
            }

        }
    }
}
