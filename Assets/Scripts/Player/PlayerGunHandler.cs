using System;
using System.Collections;
using ProgressBars;
using UnityEngine;
using Util;
using static Static_Info.GunInfo;
using Random = UnityEngine.Random;

public class PlayerGunHandler : MonoBehaviour
{
    [NonSerialized]
    public bool Shootable = true;
    
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
    public AudioSource audioPlayer;
    public AudioClip PlayerShootLaserGeneric;
    private float _AudioPlayerPitchStatic;

    private void Start()
    {
        if (gravitySource == null) gravitySource = GameObject.FindGameObjectWithTag("GravitySource");
        _curAmmo = GunInfoInstance.ammoCount;
        _rb = GetComponent<CustomRigidbody2D>();
        _AudioPlayerPitchStatic = audioPlayer.pitch;
    }

    public void Shoot(Vector2 worldMousePos)//returns if could start the shoot coroutine
    {
        if (_isFiring || _curAmmo <= 0 || _emptyRefilling) return;

        _mPos = worldMousePos;
        StartCoroutine(_Fire());
    }

    private void Update()
    {
        if (Shootable && Input.GetMouseButtonDown(0))
        {
            Shoot(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        }
        
        _emptyRefilling &= _curAmmo < GunInfoInstance.ammoCount;
        
        _noShootRefillTimer.Update();
        _emptyRefillTimer.Update();

        if (_curAmmo < GunInfoInstance.ammoCount && (_noShootRefillTimer.IsFinished || (_emptyRefilling && _emptyRefillTimer.IsFinished)))
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
                    var bullet = Instantiate(bulletPrefab, transform.position, rot);

                    bullet.GetComponent<CustomRigidbody2D>().velocity = GetComponent<CustomRigidbody2D>().velocity;
                    bullet.GetComponent<DestroyOffScreen>().playRadius = playRadius;
                    bullet.GetComponent<Gravitatable>().gravitySource = gravitySource;

                    float vertForce = GunInfoInstance.shotForce + Random.Range(-GunInfoInstance.forceVarience, GunInfoInstance.forceVarience) + verOff;
                    float latForce = Random.Range(-GunInfoInstance.forceVarience, GunInfoInstance.forceVarience) + latOff;
                    bullet.GetComponent<CustomRigidbody2D>().AddRelativeForce(new Vector2(latForce, vertForce));
                    bullet.GetComponent<BulletCollision>().dmg = GunInfoInstance.dmgMod;
                    bullet.GetComponent<BulletCollision>().owner = gameObject;


                }

            }
            audioPlayer.pitch = _AudioPlayerPitchStatic + Random.Range(0.1f,-0.1f); //pitch modulation for sound variance
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
