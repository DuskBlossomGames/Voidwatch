using Spawnables;
using UnityEngine;
using Util;

public class MissleShooter : Stunnable
{
    public GameObject target;
    public GameObject missilePrefab;
    public float shootInterval;
    public float engageDist;
    public float shootForce = 1000;
    public int amt = 3;
    public NSpriteAnimation animationState;

    private bool _isShooting, _shot;
    private readonly Timer _shoot = new();
    private float _fireAnimDuration;

    private CustomRigidbody2D _rb;
    
    private void Start()
    {
        animationState.SwapState("idle");
        if (target == null) target = GameObject.FindGameObjectWithTag("Player");
        _shoot.Value = shootInterval;

        _fireAnimDuration = (float)animationState.states[1].frames.Length / animationState.framesPerSecond;

        _rb = GetComponent<CustomRigidbody2D>();
    }
    private void Update()
    {
        _shoot.Update();

        if (_shoot.Value < _fireAnimDuration && !_isShooting && (target.transform.position - transform.position).sqrMagnitude < engageDist * engageDist)
        {

            animationState.SwapState("fire");
            _isShooting = true;
            _shot = false;
        }

        if (_shoot.Value <= _fireAnimDuration/2 && !_shot)
        {
            _shot = true;
            
            for (var i = 0; i < amt; i++)
            {
                var missile = Instantiate(missilePrefab, transform.position, transform.rotation);
                missile.GetComponent<MissleAim>().target = target;
                missile.GetComponent<CustomRigidbody2D>().AddForce(shootForce * Random.insideUnitCircle);
            }
        }

        if (_shoot.IsFinished && _isShooting)
        {
            animationState.SwapState("idle");
            _isShooting = false;

            _shoot.Value = _shoot.MaxValue;
        }

    }

    public override void Stun()
    {
        enabled = false;
    }

    public override void UpdateStun()
    {
        _rb.velocity *= Mathf.Pow(0.8f, Time.deltaTime);
    }

    public override void UnStun()
    {
        enabled = true;
    }
}
