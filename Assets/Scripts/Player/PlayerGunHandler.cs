using System;
using System.Collections;
using ProgressBars;
using Singletons;
using Spawnables;
using Spawnables.Controllers.Bullets;
using UnityEngine;
using Util;
using static Singletons.Static_Info.GunInfo;
using static Singletons.Static_Info.PlayerData;
using static Singletons.Static_Info.Statistics;
using Random = UnityEngine.Random;

namespace Player
{
    public class PlayerGunHandler : MonoBehaviour
    {
        [NonSerialized]
        public bool Shootable = true;
        public bool HasDodgePowerAttack;
    
        public ProgressBar ammoBar;

        public GameObject bulletPrefab;
        public float playRadius;
        public GameObject gravitySource;

        public bool EmptyRefilling => _emptyRefilling;
    
        private float _curAmmo;

        private readonly Timer _noShootRefillTimer = new();
        private readonly Timer _emptyRefillTimer = new();

        private bool _emptyRefilling;
        private bool _isFiring;
        private Vector2 _mPos;
        private CustomRigidbody2D _rb;
        public AudioClip laserClip;

        private void Start()
        {
            if (gravitySource == null) gravitySource = GameObject.FindGameObjectWithTag("GravitySource");
            _curAmmo = GunInfoInstance.ammoCount;
            _rb = GetComponent<CustomRigidbody2D>();
        }

        public void Shoot(Vector2 worldMousePos)//returns if could start the shoot coroutine
        {
            if (_isFiring || _curAmmo <= 0 || _emptyRefilling) return;

            _mPos = worldMousePos;
            StartCoroutine(_Fire());
        }

        private void Update()
        {
            if (Shootable && InputManager.GetKeyDown(InputAction.PrimaryWeapon))
            {
                Shoot(Camera.main.ScreenToWorldPoint(InputManager.mousePosition));
            }
        
            _emptyRefilling &= _curAmmo < GunInfoInstance.ammoCount;
        
            _noShootRefillTimer.Update();
            _emptyRefillTimer.Update();

            if (_curAmmo < GunInfoInstance.ammoCount && (_noShootRefillTimer.IsFinished || (_emptyRefilling && _emptyRefillTimer.IsFinished)))
            {
                _curAmmo = Mathf.Clamp(_curAmmo + (int) GunInfoInstance.ammoCount / GunInfoInstance.timeToRefillFully * Time.deltaTime, 0, GunInfoInstance.ammoCount);
                ammoBar.UpdatePercentage(_curAmmo, GunInfoInstance.ammoCount);
            }

            _curAmmo = Mathf.Clamp(_curAmmo, 0, GunInfoInstance.ammoCount);
        }

        public float ExpectedVelocity()
        {
            return GunInfoInstance.shotForce / bulletPrefab.GetComponent<CustomRigidbody2D>().mass * Time.fixedDeltaTime;
        }
        IEnumerator _Fire()
        {
            _noShootRefillTimer.Value = GunInfoInstance.noShootRefillTime;

            _isFiring = true;

            Vector2 mVel = Vector2.zero;
            Vector2 relPos = _mPos - (Vector2)transform.position;
            float angCorr = UtilFuncs.LeadShot(relPos, mVel - UtilFuncs.GetTargetVel(gameObject), ExpectedVelocity());
        
            for (int rep = 0; rep <= GunInfoInstance.repeats; rep++)
            {
                if (rep > 0)//only delay between repeats
                {
                    yield return new WaitForSeconds(GunInfoInstance.repeatSeperation);
                }

                _curAmmo = (int)_curAmmo;
                int bullets = Mathf.Min((int) _curAmmo, GunInfoInstance.bulletsPerShot + Random.Range(-GunInfoInstance.bulletsPerShotVarience, GunInfoInstance.bulletsPerShotVarience + 1));
                _curAmmo -= bullets;
                ammoBar.UpdatePercentage(_curAmmo, GunInfoInstance.ammoCount);

                for (int i = 0; i < bullets; i++)
                {
                    float latOff, verOff;
                    if (bullets > 1)
                    {
                        latOff = GunInfoInstance.lateralSeperation * (2 * i - bullets + 1) / (bullets - 1);
                        verOff = GunInfoInstance.verticalSeperation * (1 - Mathf.Abs(2 * ((float)i / (bullets - 1)) - 1));
                    }
                    else
                    {
                        latOff = verOff = 0;
                    }

                    Quaternion rot = Quaternion.Euler(transform.rotation.eulerAngles.x,
                        transform.rotation.eulerAngles.x,
                        transform.rotation.eulerAngles.z + angCorr);
                    if (Random.Range(0f, 1f) > GunInfoInstance.misfireChance)
                    {
                        StatisticsInstance.bulletsShot++;
                        var bullet = Instantiate(bulletPrefab, transform.position, rot);

                        bullet.GetComponent<CustomRigidbody2D>().linearVelocity = GetComponent<CustomRigidbody2D>().linearVelocity;
                        bullet.GetComponent<DestroyOffScreen>().playRadius = playRadius;
                        bullet.GetComponent<Gravitatable>().gravitySource = gravitySource;

                        float vertForce = GunInfoInstance.shotForce + Random.Range(-GunInfoInstance.forceVarience, GunInfoInstance.forceVarience) + verOff;
                        float latForce = Random.Range(-GunInfoInstance.forceVarience, GunInfoInstance.forceVarience) + latOff;
                        Vector2 dir = transform.up;
                        bullet.GetComponent<CustomRigidbody2D>().linearVelocity = bullet.GetComponent<CustomRigidbody2D>().linearVelocity.magnitude * dir;
                        bullet.GetComponent<CustomRigidbody2D>().AddRelativeForce(new Vector2(latForce, vertForce));
                        bullet.GetComponent<BulletCollision>().dmg = GunInfoInstance.dmgMod * (HasDodgePowerAttack ? PlayerDataInstance.postDodgeMult : 1);
                        bullet.GetComponent<BulletCollision>().owner = gameObject;
                        bullet.GetComponent<BulletCollision>().chains = PlayerDataInstance.bulletChains;
                    }
                }
                
                AudioPlayer.Play(laserClip, Random.Range(0.5f, 0.7f), 0.45f);
                HasDodgePowerAttack = false;
            }

            yield return new WaitForSeconds(GunInfoInstance.fireTime);

            if (_curAmmo <= 0)
            {
                _emptyRefillTimer.Value = GunInfoInstance.emptyRefillTime;
                _emptyRefilling = true;
            }

            _isFiring = false;
        }
    }
}
