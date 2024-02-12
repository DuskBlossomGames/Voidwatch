using UnityEngine;
using Util;

public class MissleShooter : MonoBehaviour
{
    public GameObject target;
    public GameObject missilePrefab;
    public float shootInterval;
    public float engageDist;
    public int amt = 3;
    public bool isShootin = false;
    public NSpriteAnimation animationState;
    public int startFrames;
    public int endFrames;


    private float _timer;

    private void Start()
    {
        animationState.SwapState("idle");
        if (target == null) target = GameObject.FindGameObjectWithTag("Player");
        _timer = shootInterval;
    }
    void Update()
    {
        _timer -= Time.deltaTime;

        if (_timer < startFrames * animationState.timePerFrame && !isShootin&& (target.transform.position - transform.position).sqrMagnitude < engageDist * engageDist){

            animationState.SwapState("fire");
            isShootin = true;

        }

        if (_timer < 0)
        {
            _timer = shootInterval;
            if ((target.transform.position - transform.position).sqrMagnitude < engageDist * engageDist)
            {
                for (int i = 0; i < amt; i++)
                {
                    GameObject missile = Instantiate(missilePrefab, transform.position, transform.rotation);
                    missile.GetComponent<MissleAim>().target = target;
                    missile.GetComponent<CustomRigidbody2D>().AddForce(1000 * Random.insideUnitCircle);
                }
            }

        }

      if (shootInterval-_timer > endFrames * animationState.timePerFrame && isShootin){
            animationState.SwapState("idle");

            isShootin = false;

        }

    }
}
