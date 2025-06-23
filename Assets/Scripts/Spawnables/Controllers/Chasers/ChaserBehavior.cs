using Spawnables.Controllers.Misslers;
using Spawnables.Pathfinding;
using UnityEngine;
using Util;

namespace Spawnables.Controllers.Chasers
{
    public class ChaserBehavior : MissleAim
    {
        public float explosionDamage;
        
        private void Start()
        {
            if (target == null) target = GameObject.FindGameObjectWithTag("Player");
            base.Start();
        }

        protected override void FixedUpdate()
        { // ripped striaght from MissleAim#FixedUpdate
            _fuel -= accelforce * Time.fixedDeltaTime;
            _rigid.AddForce(accelforce * _nDir);
            _oVel = _rigid.linearVelocity;
            var tRot = Quaternion.Euler(0, 0, -90 + Mathf.Rad2Deg * Mathf.Atan2(_nDir.y, _nDir.x));
            _cRot = Quaternion.RotateTowards(_cRot, tRot, 360 * Time.deltaTime);
            transform.rotation = _cRot;
        }

        // don't use missile explode
        protected override void OnCollisionEnter2D(Collision2D collision) { }
    }
}
