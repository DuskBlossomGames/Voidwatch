using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UI;
using UnityEngine;
using Util;
using Random = UnityEngine.Random;

public class PlatelinController : MonoBehaviour
{
    public int maxColonySize;
    public float normalSize, leaderSize;
    public bool isLeader;
    public float leaderChance;
    public float explosionScaleMult;
    public float gooSpawnTime;

    public float speed;
    public AnimationCurve speedCurve;
    public float speedCurveScale;
    
    private PlatelinController _leader;
    private readonly List<GameObject> _subjects = new();

    public float initialDelay;
    public float spawndelay;
    public float swellsize;
    public float swelltime;
    public bool isSpore;
    public float sporeHealingPerSecond;
    public GameObject goo;
    public NSpriteAnimation animationState;

    public float tendonLength;
    public float drag;
    public float suction;

    float _spawntimer;
    float _swelltimer;
    private float _speedCurve;
    private readonly Timer _gooTimer = new();
    private float _scale;

    public enum Mode
    {
        Immature,
        Idle,
        Swell,
    };
    
    public Mode mode;

    private GameObject _target;

    void OnEnable()
    {
        isSpore = false;
        animationState.SwapState("Maturation");
        _spawntimer = initialDelay;
        //GetComponent<SpriteRenderer>().color = Color.blue;
    }

    private void Start()
    {
        transform.localScale = (_scale = isLeader ? leaderSize : normalSize) * Vector3.one;
        if (isLeader) _leader = this;
        
        _target = GameObject.FindGameObjectWithTag("Player");
    }


    private Vector2 _dampVel;
    void Update()
    {
        if(isSpore){
          GetComponent<Spawnables.EnemyDamageable>().EnemyHeal(sporeHealingPerSecond*Time.deltaTime);
        } else
        {
            for (int i = 0; i < transform.parent.childCount; i++)
            {
                var plate = transform.parent.GetChild(i);
                if ((plate.position - transform.position).sqrMagnitude < tendonLength * tendonLength)
                {
                    var rigid = GetComponent<CustomRigidbody2D>();
                    var diffVel = plate.GetComponent<CustomRigidbody2D>().velocity - rigid.velocity;
                    rigid.AddForce(diffVel.normalized * diffVel.sqrMagnitude * drag);
                    var diffPos = plate.position - transform.position;
                    rigid.AddForce(diffPos.normalized * suction);

                }
            }
        }

        if(transform.position.sqrMagnitude > 75 * 75)
        {
            GetComponent<CustomRigidbody2D>().AddForce(-30 * ((Vector2)transform.position).normalized);
        }

        if (mode == Mode.Immature)
        {
            _spawntimer -= Time.deltaTime;

            if(_spawntimer <= 1f && isSpore){
                animationState.SwapState("Maturation");
                isSpore = false;
            }

            if (_spawntimer <= 0)
            {
                animationState.SwapState("Idle");
                mode = Mode.Idle;
                _gooTimer.Value = gooSpawnTime;

                _spawntimer = spawndelay + Random.Range(-0.5f,0.5f);
                //GetComponent<SpriteRenderer>().color = Color.white;
            }
        }

        if (mode is Mode.Idle or Mode.Swell)
        {
            var targSpeed = speed * speedCurve.Evaluate(_speedCurve / speedCurveScale) *
                            ((Vector2)(_target.transform.position - transform.position)).normalized;
            GetComponent<CustomRigidbody2D>().velocity = Vector2.SmoothDamp(GetComponent<CustomRigidbody2D>().velocity, targSpeed, ref _dampVel, 0.1f);
            _speedCurve = (_speedCurve + Time.deltaTime) % speedCurveScale;
            
            _gooTimer.Update();
            if (_gooTimer.IsFinished)
            {
                Instantiate(goo, transform.position, transform.rotation).transform.localScale *= _scale / normalSize;
                _gooTimer.Value = gooSpawnTime;
            }
        }

        if (mode == Mode.Idle)
        {
            if (_leader._subjects.Count < maxColonySize) _spawntimer -= Time.deltaTime;
            if (_spawntimer <= 0)
            {
                mode = Mode.Swell;
                _swelltimer = swelltime;
            }
        } else if (mode == Mode.Swell)
        {
            _swelltimer -= Time.deltaTime;
            float scale = _scale + (swellsize*_scale/normalSize - _scale) * (swelltime - _swelltimer) / swelltime;
            transform.localScale = new Vector3(scale, scale, 1);
            if(_swelltimer <= 0)
            {
                mode = Mode.Idle;
                _spawntimer = spawndelay;
                transform.localScale = new Vector3(_scale, _scale, 1);
                var copy = Instantiate(this, transform.parent);
                copy.transform.position += (Vector3) Random.insideUnitCircle;
                copy._leader = _leader;
                copy.isLeader = false;
                copy.mode = Mode.Immature;

                _leader._subjects.Add(copy.gameObject);

                if(Random.value < leaderChance)
                {
                    copy.GetComponent<CustomRigidbody2D>().AddForce(30000 * Random.insideUnitCircle.normalized);
                    copy._spawntimer += 5;
                    copy.animationState.SwapState("Dormant");
                    copy.isSpore = true;
                    copy.isLeader = true;
                }
            }
        }
    }

    private void OnDestroy()
    {
        _leader._subjects.Remove(gameObject);
        Instantiate(goo, transform.position, transform.rotation).transform.localScale *= explosionScaleMult;
    }
}
