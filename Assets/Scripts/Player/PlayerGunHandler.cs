using System;
using System.Collections;
using System.Linq;
using Player;
using ProgressBars;
using Scriptable_Objects;
using Scriptable_Objects.Upgrades;
using UnityEngine;
using Util;
using static Static_Info.GunInfo;
using Random = UnityEngine.Random;

public class PlayerGunHandler : MonoBehaviour
{
    public ProgressBar ammoBar;

    public GameObject bulletPrefab;
    public float playRadius;
    public GameObject gravitySource;

    private float _curAmmo;

    private readonly Timer _noShootRefillTimer = new();
    private readonly Timer _emptyRefillTimer = new();

    private bool _emptyRefilling;
    private bool _isFiring;
    private Vector2 _mPos;
    private CustomRigidbody2D _rb;
    public AudioSource audioPlayer;
    public AudioClip PlayerShootLaserGeneric;

    private void Start()
    {
        if (gravitySource == null) gravitySource = GameObject.FindGameObjectWithTag("GravitySource");
        _curAmmo = GunInfoInstance.ammoCount;
        _rb = GetComponent<CustomRigidbody2D>();
    }

    public void Shoot(Vector2 worldMousePos)//returns if could start the shoot coroutine
    {
        _emptyRefilling &= _curAmmo < GunInfoInstance.ammoCount;
        if (_isFiring || _curAmmo <= 0 || _emptyRefilling) return;

        _mPos = worldMousePos;
        StartCoroutine(_Fire());
    }

    private void Update()
    {
        _noShootRefillTimer.Update();
        _emptyRefillTimer.Update();

        if (_noShootRefillTimer.IsFinished || (_emptyRefilling && _emptyRefillTimer.IsFinished))
        {
            _curAmmo = Mathf.Clamp(_curAmmo + GunInfoInstance.ammoCount / GunInfoInstance.timeToRefillFully * Time.deltaTime, 0, GunInfoInstance.ammoCount);
            ammoBar.UpdatePercentage(_curAmmo, GunInfoInstance.ammoCount);
        }
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
        float angCorr = UtilFuncs.LeadShot(relPos, mVel - _rb.velocity, ExpectedVelocity());

        var evt = new ShootEvent
        {
            bulletsPerShot = GunInfoInstance.bulletsPerShot,
            bulletsPerShotVarience = GunInfoInstance.bulletsPerShotVarience,
            shotForce = GunInfoInstance.shotForce,
            forceVarience = GunInfoInstance.forceVarience,
            lateralSeperation = GunInfoInstance.lateralSeperation,
            verticalSeperation = GunInfoInstance.verticalSeperation,
            misfireChance = GunInfoInstance.misfireChance,
            repeats = GunInfoInstance.repeats,
            repeatSeperation = GunInfoInstance.repeatSeperation
        };

        for (int rep = 0; rep <= evt.repeats; rep++)
        {
            if (rep > 0)//only delay between repeats
            {
                yield return new WaitForSeconds(evt.repeatSeperation);
            }

            _curAmmo = (int)_curAmmo;
            int bullets = Mathf.Min((int) _curAmmo, evt.bulletsPerShot + Random.Range(-evt.bulletsPerShotVarience, evt.bulletsPerShotVarience + 1));
            _curAmmo -= bullets;
            ammoBar.UpdatePercentage(_curAmmo, GunInfoInstance.ammoCount);

            for (int i = 0; i < bullets; i++)
            {
                float latOff, verOff;
                if (bullets > 1)
                {
                    latOff = evt.lateralSeperation * (2 * i - bullets + 1) / (bullets - 1);
                    verOff = evt.verticalSeperation * (1 - Mathf.Abs(2 * ((float)i / (bullets - 1)) - 1));
                }
                else
                {
                    latOff = verOff = 0;
                }

                Quaternion rot = Quaternion.Euler(transform.rotation.eulerAngles.x,
                    transform.rotation.eulerAngles.x,
                    transform.rotation.eulerAngles.z + angCorr);
                if (Random.Range(0f, 1f) > evt.misfireChance)
                {
                    var bullet = Instantiate(bulletPrefab, transform.position, rot);

                    bullet.GetComponent<CustomRigidbody2D>().velocity = GetComponent<CustomRigidbody2D>().velocity;
                    bullet.GetComponent<DestroyOffScreen>().playRadius = playRadius;
                    bullet.GetComponent<Gravitatable>().gravitySource = gravitySource;

                    float vertForce = evt.shotForce + Random.Range(-evt.forceVarience, evt.forceVarience) + verOff;
                    float latForce = Random.Range(-evt.forceVarience, evt.forceVarience) + latOff;
                    bullet.GetComponent<CustomRigidbody2D>().AddRelativeForce(new Vector2(latForce, vertForce));
                    bullet.GetComponent<BulletCollision>().dmg = GunInfoInstance.dmgMod;
                    bullet.GetComponent<BulletCollision>().owner = gameObject;


                }

            }
            audioPlayer.PlayOneShot(PlayerShootLaserGeneric);
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
