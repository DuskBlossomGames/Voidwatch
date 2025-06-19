using Spawnables.Pathfinding;
using UnityEngine;
using Util;

namespace Spawnables.Controllers.Chasers
{
    public class ChaserBehavior : MonoBehaviour
    {
        public GameObject target;
        public DirectPathfinding pathfinder;
        public float attackDist;
        public float accel;
        public float maxSpeed;
        public float movVal;
        public float driftTime;
        public AnimationCurve maxSpeedCurve;

        private Vector2 _movDir;
        private Vector2 _oldMovDir;
        private float _currSpeed;
        private EnemyGunHandler _gun;
        private Timer _driftTimer;

        private void Start()
        {
            _driftTimer = new Timer();
            if (target == null) target = GameObject.FindGameObjectWithTag("Player");
            _gun = GetComponent<EnemyGunHandler>();
        }

        private void Update()
        {
            _driftTimer.Update();
            if (_driftTimer.IsFinished)
            {
                Vector2 diff = (target.transform.position - transform.position);
                if (diff.sqrMagnitude < attackDist * attackDist)
                {
                    _currSpeed *= Mathf.Pow(.9f, Time.deltaTime);
                    Vector2 tarmovDir = UtilFuncs.LeadShotNorm(diff, target.GetComponent<Rigidbody2D>().linearVelocity - GetComponent<Rigidbody2D>().linearVelocity, _gun.ExpectedVelocity());
                    _movDir = UtilFuncs.LerpSafe(_movDir, tarmovDir, 10 * Time.deltaTime);
                    _gun.Shoot(0);
                    //Debug.Log(_gun.status);
                }
                else
                {
                    float halfDistV = diff.magnitude * movVal;
                    Vector2 e1 = (Vector2)transform.position + halfDistV * GetComponent<Rigidbody2D>().linearVelocity.normalized;
                    Vector2 e2 = (Vector2)target.transform.position - halfDistV * target.GetComponent<Rigidbody2D>().linearVelocity.normalized;
                    Vector2 em = (e1 + e2) / 2;

                    Vector2 tarmovDir = pathfinder.PathDirNorm(transform.position, em);
                    _movDir = UtilFuncs.LerpSafe(_movDir, tarmovDir, 10 * Time.deltaTime);
                    _currSpeed = Mathf.Min(maxSpeed, _currSpeed + accel * Time.deltaTime);
                }
                var vel = GetComponent<CustomRigidbody2D>().linearVelocity;
                GetComponent<CustomRigidbody2D>().linearVelocity = UtilFuncs.LerpSafe(vel,_movDir * _currSpeed, 10 * Time.deltaTime);
                transform.rotation = UtilFuncs.RotFromNorm(_movDir);
            }
        }

        public void TakeDamage()
        {
            _driftTimer.Value = driftTime;
        }
    }
}
