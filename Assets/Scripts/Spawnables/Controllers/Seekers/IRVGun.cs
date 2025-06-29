using UnityEngine;
using Util;

namespace Spawnables.Controllers.Seekers
{
    public class IRVGun : MonoBehaviour
    {
        // Start is called before the first frame update

        public GameObject target;
        float _bulletvel;
        CustomRigidbody2D _crb;
        public EnemyGunHandler gun;

        void Start()
        {
            target = GameObject.FindGameObjectWithTag("Player");
            _bulletvel = gun.ExpectedVelocity();
            _crb = GetComponent<CustomRigidbody2D>();
        }

        // Update is called once per frame
        void Update()
        {
            var ppos = target.transform.position;
            var pvel = UtilFuncs.GetTargetVel(target);

            float dir = UtilFuncs.LeadShot(ppos - transform.position, pvel - _crb.linearVelocity, _bulletvel);
            float tardir = Mathf.Rad2Deg * dir - 90;

            gun.Shoot(tardir - transform.rotation.eulerAngles.z);
        }
    }
}
