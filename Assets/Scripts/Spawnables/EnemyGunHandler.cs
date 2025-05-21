using System.Collections;
using UnityEngine;
using Util;

public class EnemyGunHandler : MonoBehaviour
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
    public float shieldMult, bleedPerc;

    private int _currClipCount;
    private int _currClipCap;

    private bool _readyToFire;
    private float _bulletAngle;


    private void Start()
    {
        if (gravitySource == null) gravitySource = GameObject.FindGameObjectWithTag("GravitySource");
        _currClipCount = clipCount;
        _currClipCap = clipCap;
        _readyToFire = true;
    }

    public bool Shoot(float angle)//returns if could start the shoot coroutine
    {
        if (_readyToFire)
        {
            _bulletAngle = angle;
            StartCoroutine(_Fire());
            return true;
        } else
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

        for (int rep = 0; rep < repeats+1; rep++)
        {
            if (rep > 0)//only delay between repeats
            {
                yield return new WaitForSeconds(repeatSeperation);
            }

            int bullets = Mathf.Min(_currClipCap, bulletsPerShot + Random.Range(-bulletsPerShotVarience, bulletsPerShotVarience + 1));
            //Debug.Log(string.Format("bullets: {0}",bullets));
            _currClipCap -= bullets;
            for (int i = 0; i < bullets; i++)
            {
                float latOff, verOff;
                if (bullets > 1)
                {
                    latOff = lateralSeperation * (2 * i - bullets + 1) / (bullets - 1);
                    verOff = verticalSeperation * (1 - Mathf.Abs(2 * ((float)i / (bullets - 1)) - 1));
                } else
                {
                    latOff = verOff = 0;
                }

                Quaternion rot = Quaternion.Euler(transform.rotation.eulerAngles.x,
                    transform.rotation.eulerAngles.x,
                    transform.rotation.eulerAngles.z + _bulletAngle);
                if (Random.Range(0f, 1f) > misfireChance)
                {
                    //Debug.Log("Enemy Spawning Bullet");
                    var bullet = Instantiate(bulletPrefab, transform.position, rot);

                    //Debug.Log("Getting Velocity");
                    bullet.GetComponent<CustomRigidbody2D>().velocity = GetComponent<CustomRigidbody2D>().velocity;
                    bullet.GetComponent<DestroyOffScreen>().playRadius = playRadius;
                    bullet.GetComponent<Gravitatable>().gravitySource = gravitySource;

                    //Debug.Log("Got Velocity");

                    float vertForce = shotForce + Random.Range(-forceVarience, forceVarience) + verOff;
                    float latForce = Random.Range(-forceVarience, forceVarience) + latOff;
                    bullet.GetComponent<CustomRigidbody2D>().AddRelativeForce(new Vector2(latForce, vertForce));
                    bullet.GetComponent<BulletCollision>().dmg = dmgMod;
                    bullet.GetComponent<BulletCollision>().owner = gameObject;
                    bullet.GetComponent<BulletCollision>().shieldMult = shieldMult;
                    bullet.GetComponent<BulletCollision>().bleedPerc = bleedPerc;
                }

            }
        }

        if (_currClipCap <= 0)//should never be less than 0
        {
            yield return new WaitForSeconds(reloadTime);
            _currClipCount -= 1;
            _currClipCap = clipCap;
        }
        

        if (_currClipCount <= 0)//should never be less than 0
        {
            yield return new WaitForSeconds(refillTime);
            _currClipCount = clipCount;
        }

        _readyToFire = true;
    }
}
