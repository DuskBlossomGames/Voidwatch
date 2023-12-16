using UnityEngine;
using Util;

public class MissleShooter : MonoBehaviour
{
    public GameObject target;
    public GameObject missilePrefab;
    public float shootInterval;
    public float engageDist;
    public int amt = 3;
    public AnimationClip shoot;

    private float _timer;
    private Animation _animation;
    private void Start()
    {
        if (target == null) target = GameObject.FindGameObjectWithTag("Player");
        _timer = shootInterval;
        _animation = GetComponent<Animation>();
        _animation.clip = shoot;
    }
    void Update()
    {
        _timer -= Time.deltaTime;
        if (_timer < 0)
        {
            _timer = shootInterval;
            if ((target.transform.position - transform.position).sqrMagnitude < engageDist * engageDist)
            {
                _animation.Play();
                for (int i = 0; i < amt; i++)
                {
                    GameObject missile = Instantiate(missilePrefab, transform.position, transform.rotation);
                    missile.GetComponent<MissleAim>().target = target;
                    missile.GetComponent<CustomRigidbody2D>().AddForce(1000 * Random.insideUnitCircle);
                }
            }
            
        }
    }
}
