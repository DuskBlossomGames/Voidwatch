using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player;
using Scriptable_Objects;
using Scriptable_Objects.Upgrades;
using Util;

public class KineticWeapon : IBaseWeapon
{
    public KineticSettings kineticSettings;

    private int _currClipCount;
    private int _currClipCap;

    private bool _readyToFire;
    private float _bulletAngle;

    public GameObject gravitySource;
    public string status;

    private void Start()
    {
        if (gravitySource == null) gravitySource = GameObject.FindGameObjectWithTag("GravitySource");
        _currClipCount = kineticSettings.clipCount;
        _currClipCap = kineticSettings.clipCap;
        _readyToFire = true;
        status = "Ready To Fire";

        _upgradeable = GetComponent<Player.Upgradeable>();
    }

    public override void OnGetFocus() { return; }
    public override void OnLoseFocus() { return; }

    public bool Shoot(float angle)//returns if could start the shoot coroutine
    {
        if (_readyToFire)
        {
            _bulletAngle = angle;
            StartCoroutine(_Fire());
            return true;
        }
        else
        {
            return false;
        }
    }

    public float ExpectedVelocity()
    {
        return kineticSettings.shotForce / kineticSettings.bulletPrefab.GetComponent<Util.CustomRigidbody2D>().mass * Time.fixedDeltaTime;
    }
    IEnumerator _Fire()
    {
        _readyToFire = false;
        status = "Shooting";
        //Debug.Log("started");

        var ks = kineticSettings;//Just aliasing things that make this more legible
        var evt = new ShootEvent
        {
            bulletsPerShot = ks.bulletsPerShot,
            bulletsPerShotVarience = ks.bulletsPerShotVarience,
            shotForce = ks.shotForce,
            forceVarience = ks.forceVarience,
            lateralSeperation = ks.lateralSeperation,
            verticalSeperation = ks.verticalSeperation,
            misfireChance = ks.misfireChance,
            repeats = ks.repeats,
            repeatSeperation = ks.repeatSeperation
        };
        if (_upgradeable) HandleEvent(evt);

        for (int rep = 0; rep < evt.repeats + 1; rep++)
        {
            if (rep > 0)//only delay between repeats
            {
                yield return new WaitForSeconds(evt.repeatSeperation);
            }

            int bullets = Mathf.Min(_currClipCap, evt.bulletsPerShot + Random.Range(-evt.bulletsPerShotVarience, evt.bulletsPerShotVarience + 1));
            //Debug.Log(string.Format("bullets: {0}",bullets));
            _currClipCap -= bullets;
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
                    transform.rotation.eulerAngles.z + _bulletAngle);
                if (Random.Range(0f, 1f) > evt.misfireChance)
                {
                    var bullet = Instantiate(kineticSettings.bulletPrefab, transform.position, rot);

                    bullet.GetComponent<CustomRigidbody2D>().velocity = GetComponent<CustomRigidbody2D>().velocity;
                    bullet.GetComponent<DestroyOffScreen>().playRadius = kineticSettings.playRadius;
                    bullet.GetComponent<Gravitatable>().gravitySource = gravitySource;

                    float vertForce = evt.shotForce + Random.Range(-evt.forceVarience, evt.forceVarience) + verOff;
                    float latForce = Random.Range(-evt.forceVarience, evt.forceVarience) + latOff;
                    bullet.GetComponent<CustomRigidbody2D>().AddRelativeForce(new Vector2(latForce, vertForce));
                    bullet.GetComponent<BulletCollision>().dmg = kineticSettings.dmgMod;
                    bullet.GetComponent<BulletCollision>().owner = gameObject;
                }

            }
        }

        if (_currClipCap <= 0)//should never be less than 0
        {
            //Debug.Log("Reloading");
            status = "Reloading";
            yield return new WaitForSeconds(kineticSettings.reloadTime);
            _currClipCount -= 1;
            _currClipCap = kineticSettings.clipCap;
            //Debug.Log("Reloaded");
        }


        if (_currClipCount <= 0)//should never be less than 0
        {
            //Debug.Log("Refilling");
            status = "Refilling";
            yield return new WaitForSeconds(kineticSettings.refillTime);
            _currClipCount = kineticSettings.clipCount;
            //Debug.Log("Refilled");
        }

        _readyToFire = true;
        status = "Ready To Fire";
        //Debug.Log("ended");
    }
    public int CurrClipCount()
    {
        return _currClipCount;
    }
    public int CurrClipCap()
    {
        return _currClipCap;
    }
}
