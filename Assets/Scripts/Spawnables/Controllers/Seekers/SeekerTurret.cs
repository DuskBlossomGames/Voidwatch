using UnityEngine;

namespace Spawnables.Controllers.Seekers
{
    public class SeekerTurret : MonoBehaviour
    {
        private GameObject _player;
        private EnemyGunHandler _gun;
        public int id;
        void Start()
        {
            _player = GameObject.FindGameObjectWithTag("Player");
            _gun = GetComponent<EnemyGunHandler>();
            _gun.gravitySource = GameObject.Find("Planet");
        }

        // Update is called once per frame
        void Update()
        {
            var rot = id / 4f * 2 * Mathf.PI + Mathf.Deg2Rad * transform.parent.rotation.eulerAngles.z;
            transform.position = transform.parent.position + 12 * .5f * (Vector3)new Vector2(Mathf.Cos(rot), Mathf.Sin(rot));
            var relpos = transform.position - transform.parent.position;
            var tarpos = _player.transform.position - transform.position;
            if (Vector2.Dot(relpos, tarpos) > 0)
            {
                var _rot = Mathf.LerpAngle(transform.rotation.eulerAngles.z, -90 + Mathf.Rad2Deg * Mathf.Atan2(tarpos.y, tarpos.x), .5f);
                transform.rotation = Quaternion.Euler(0, 0, _rot);
                _gun.Shoot(0);
            }
        }
    }
}
