using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunHandler : MonoBehaviour
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
    private float _bulletAngle;
    public string status;

    private void Start()
    {
        _currClipCount = clipCount;
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

    IEnumerator _Fire()
    {
        _readyToFire = false;
        status = "Shooting";
        //Debug.Log("started");

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
                    var bullet = Instantiate(bulletPrefab, transform.position, rot);

                    bullet.GetComponent<Rigidbody2D>().velocity = GetComponent<Rigidbody2D>().velocity;
                    bullet.GetComponent<DestroyOffScreen>().playRadius = playRadius;
                    bullet.GetComponent<Gravitatable>().gravitySource = gravitySource;

                    float vertForce = shotForce + Random.Range(-forceVarience, forceVarience) + verOff;
                    float latForce = Random.Range(-forceVarience, forceVarience) + latOff;
                    bullet.GetComponent<Rigidbody2D>().AddRelativeForce(new Vector2(latForce, vertForce));
                    bullet.GetComponent<BulletCollision>().dmg = dmgMod;
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
