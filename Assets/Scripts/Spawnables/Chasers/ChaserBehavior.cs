using UnityEngine;

public class ChaserBehavior : MonoBehaviour
{
    public GameObject target;
    public DirectPathfinding pathfinder;
    public float attackDist;
    public float accel;
    public float maxSpeed;
    public float movVal;
    public AnimationCurve maxSpeedCurve;

    private Vector2 _movDir;
    private Vector2 _oldMovDir;
    private float _currSpeed;
    private GunHandler _gun;

    private void Start()
    {
        if (target == null) target = GameObject.FindGameObjectWithTag("Player");
        _gun = GetComponent<GunHandler>();
    }

    private void Update()
    {
        Vector2 diff = (target.transform.position - transform.position);
        if (diff.sqrMagnitude < attackDist * attackDist)
        {
            _currSpeed *= Mathf.Pow(.9f, Time.deltaTime);
            Vector2 tarmovDir = UtilFuncs.LeadShotNorm(diff, target.GetComponent<Rigidbody2D>().velocity - GetComponent<Rigidbody2D>().velocity, _gun.ExpectedVelocity());
            _movDir = UtilFuncs.LerpSafe(_movDir, tarmovDir, 10 * Time.deltaTime);
            _gun.Shoot(0);
            //Debug.Log(_gun.status);
        } else
        {
            float halfDistV = diff.magnitude * movVal;
            Vector2 e1 = (Vector2)transform.position + halfDistV * GetComponent<Rigidbody2D>().velocity.normalized;
            Vector2 e2 = (Vector2)target.transform.position - halfDistV * target.GetComponent<Rigidbody2D>().velocity.normalized;
            Vector2 em = (e1 + e2) / 2;

            Vector2 tarmovDir = pathfinder.PathDirNorm(transform.position, em);
            _movDir = UtilFuncs.LerpSafe(_movDir, tarmovDir, 10 * Time.deltaTime);
            _currSpeed = Mathf.Min(maxSpeed, _currSpeed + accel * Time.deltaTime);
        }
        GetComponent<Rigidbody2D>().velocity = _movDir * _currSpeed;
        transform.rotation = UtilFuncs.RotFromNorm(_movDir);
    }
}
