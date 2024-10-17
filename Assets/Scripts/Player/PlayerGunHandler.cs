using System.Collections;
using System.Linq;
using Player;
using Scriptable_Objects;
using Scriptable_Objects.Upgrades;
using UnityEngine;
using Util;

public class PlayerGunHandler : MonoBehaviour
{
    public GameObject bulletPrefab;
    public float playRadius;
    public GameObject gravitySource;

    public int clipCount;
    public int clipCap;
    public int bulletsPerShot;
    public int bulletsPerShotVarience;
    public float reloadTime;
    public float refillTime;
    public float shotForce;
    public float forceVarience;
    public float lateralSeperation;
    public float verticalSeperation;
    public float misfireChance;
    public int repeats;
    public float repeatSeperation;

    public float dmgMod;

    private int _currClipCount;
    private int _currClipCap;

    private bool _readyToFire;
    private Vector2 _mPos;
    public string status;

    private Upgradeable _upgradeable;
    private CustomRigidbody2D _rb;

    private void Start()
    {
        if (gravitySource == null) gravitySource = GameObject.FindGameObjectWithTag("GravitySource");
        _currClipCount = clipCount;
        _currClipCap = clipCap;
        _readyToFire = true;
        status = "Ready To Fire";
        _rb = GetComponent<CustomRigidbody2D>();

        _upgradeable = GetComponent<Upgradeable>();
    }

    public bool Shoot(Vector2 worldMousePos)//returns if could start the shoot coroutine
    {
        if (_readyToFire)
        {
            _mPos = worldMousePos;
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
        return shotForce / bulletPrefab.GetComponent<CustomRigidbody2D>().mass * Time.fixedDeltaTime;
    }
    IEnumerator _Fire()
    {
        _readyToFire = false;
        status = "Shooting";
        //Debug.Log("started");

        Vector2 mVel = Vector2.zero;
        Vector2 relPos = _mPos - (Vector2)transform.position;
        float angCorr = UtilFuncs.LeadShot(relPos, mVel - _rb.velocity, ExpectedVelocity());
        Debug.LogFormat("Expected velocity: {0}", ExpectedVelocity());

        var evt = new ShootEvent
        {
            bulletsPerShot = bulletsPerShot,
            bulletsPerShotVarience = bulletsPerShotVarience,
            shotForce = shotForce,
            forceVarience = forceVarience,
            lateralSeperation = lateralSeperation,
            verticalSeperation = verticalSeperation,
            misfireChance = misfireChance,
            repeats = repeats,
            repeatSeperation = repeatSeperation
        };
        //if (_upgradeable) _upgradeable.HandleEvent(evt, null);

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
                    bullet.GetComponent<BulletCollision>().dmg = dmgMod;
                    bullet.GetComponent<BulletCollision>().owner = gameObject;

                    Debug.LogFormat("Actual velocity: {0}", bullet.GetComponent<CustomRigidbody2D>().velocity.magnitude);
                }

            }
        }

        if (_currClipCap <= 0)//should never be less than 0
        {
            //Debug.Log("Reloading");
            status = "Reloading";
            yield return new WaitForSeconds(reloadTime);
            _currClipCount -= 1;
            _currClipCap = clipCap;
            //Debug.Log("Reloaded");
        }


        if (_currClipCount <= 0)//should never be less than 0
        {
            //Debug.Log("Refilling");
            status = "Refilling";
            yield return new WaitForSeconds(refillTime);
            _currClipCount = clipCount;
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
