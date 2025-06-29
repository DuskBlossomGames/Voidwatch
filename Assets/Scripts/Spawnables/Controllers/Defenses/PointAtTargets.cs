using Player;
using Spawnables.Controllers.Bullets;
using UnityEngine;
using Util;

namespace Spawnables.Controllers.Defenses
{
    public class PointAtTargets : MonoBehaviour
    {
        public GameObject bulletPrefab;
        public float playRadius;
        public GameObject gravitySource;
        public Transform target;
        float _rot = 0;
        float _countdown = 0;

        public float turretBulletSpeed = 5000;
    
        Vector2 rot(Vector2 vec, float angle)
        {
            return new Vector2(vec.x * Mathf.Cos(angle) + vec.y * Mathf.Sin(angle), -vec.x * Mathf.Sin(angle) + vec.y * Mathf.Cos(angle));
        }

        // TODO: sooo many GetComponent calls here, fix
        void Update()
        {
            Vector2 diff = new Vector2(target.position.x - transform.parent.position.x, target.position.y - transform.parent.position.y);

            if((diff.x*transform.position.x + diff.y * transform.position.y) > 0)
            {
                _countdown -= Time.deltaTime;
                float bulletVel = turretBulletSpeed / bulletPrefab.GetComponent<CustomRigidbody2D>().mass * Time.fixedDeltaTime;
                float globalrot = Mathf.Deg2Rad * transform.parent.rotation.eulerAngles.z;
                float angle = UtilFuncs.LeadShot(rot(diff,globalrot), rot(UtilFuncs.GetTargetVel(target), globalrot), bulletVel);
                _rot = angle;
                //_rot = Mathf.Atan2(diff.y, diff.x);
            } else
            {
                _countdown = 1;
                _rot = Mathf.Pow(.5f,Time.deltaTime) * (_rot - Mathf.PI / 2)+Mathf.PI/2;
            }

            if (_countdown < 0 && target.GetComponent<EnforcePlayArea>().attackable)
            {
                _countdown = .05f;
                var bullet = Instantiate(bulletPrefab, transform.position, transform.rotation);
                bullet.GetComponent<DestroyOffScreen>().playRadius = 200; // player gets destroyed at 200
                bullet.GetComponent<Gravitatable>().gravitySource = gravitySource;
                bullet.GetComponent<CustomRigidbody2D>().AddRelativeForce(new Vector2(0, turretBulletSpeed));
                bullet.GetComponent<BulletCollision>().owner = gameObject;
            }

        
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, transform.parent.rotation.eulerAngles.z - 90 + Mathf.Rad2Deg * _rot));
            transform.localPosition = new Vector3(.211f * Mathf.Cos(_rot), .211f * Mathf.Sin(_rot) +.099f, transform.localPosition.z);
        }
    }
}
