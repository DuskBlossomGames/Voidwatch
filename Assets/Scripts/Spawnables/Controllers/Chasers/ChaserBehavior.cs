using Player;
using Spawnables.Controllers.Misslers;
using Spawnables.Damage;
using Spawnables.Pathfinding;
using UnityEngine;
using Util;

namespace Spawnables.Controllers.Chasers
{
    public class ChaserBehavior : MissleAim
    {
        public float explosionDamage;
        public float wakeupRange;

        private PositionHinter _ph;
        private bool _alive;
        
        private void Awake()
        {
            if (target == null) target = GameObject.FindGameObjectWithTag("Player");
            _ph = GetComponent<PositionHinter>();
            _ph.enabled = false;
            
            transform.GetChild(0).localScale = 2*wakeupRange / transform.lossyScale.x * Vector3.one;
        }

        protected override void FixedUpdate()
        { // ripped striaght from MissleAim#FixedUpdate
            if (!_alive)
            {
                if (Vector2.Distance(transform.position, target.transform.position) > wakeupRange) return;
                
                _alive = true;
                _ph.enabled = true;
                transform.GetChild(0).gameObject.SetActive(false);
            }
            
            _fuel -= accelforce * Time.fixedDeltaTime;
            _rigid.AddForce(accelforce * _nDir);
            _oVel = _rigid.linearVelocity;
            var tRot = Quaternion.Euler(0, 0, -90 + Mathf.Rad2Deg * Mathf.Atan2(_nDir.y, _nDir.x));
            _cRot = Quaternion.RotateTowards(_cRot, tRot, 360 * Time.deltaTime);
            transform.rotation = _cRot;
        }

        // don't use missile explode
        protected override void OnCollisionEnter2D(Collision2D collision)
        {
            if (!collision.gameObject.TryGetComponent<PlayerDamageable>(out _)) return;
            
            var ed = GetComponent<EnemyDamageable>();
            var explosionObj = Instantiate(ed.explosion, null, true);
            explosionObj.SetActive(true);
            explosionObj.transform.position = transform.position;
            explosionObj.transform.localScale = transform.lossyScale * 1.5f;
            explosionObj.GetComponent<ExplosionHandler>().Run(explosionDamage,
                explosionObj.transform.localScale.x, ed.gameObject);
            
            ed.explosion = null; // we're doing it ourselves
            ed.Variant = null; // disable scrap
            ed.Damage(float.MaxValue, null);
            
            var ea = transform.GetChild(1);
            ea.SetParent(null);
            ea.gameObject.SetActive(true);
            // idk why all of these start disabled :|
            ea.GetComponent<ElectricArea>().enabled = true;
            ea.GetComponent<DestroyAfterX>().enabled = true;
            ea.GetComponent<Collider2D>().enabled = true;
            ea.GetComponent<MiniMapIcon>().enabled = true;
        }
    }
}
