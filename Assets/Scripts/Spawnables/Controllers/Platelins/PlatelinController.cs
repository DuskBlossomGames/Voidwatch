using System.Collections.Generic;
using System.Linq;
using LevelPlay;
using Spawnables.Damage;
using UnityEngine;
using Util;
using Random = UnityEngine.Random;

namespace Spawnables.Controllers.Platelins
{
    public class PlatelinController : MonoBehaviour
    {
        private static int _numColonies;
        private static int _numPlatelins;
    
        public int maxColonySize, maxNumColonies;
        public float normalSize, leaderSize;
        public bool isLeader;
        public int maxHealth, leaderMaxHealth;
        public float leaderChance;
        public float explosionScaleMult;
        public float gooSpawnTime;

        public GameObject healthPickup;
    
        public AnimationCurve scaleCurve;
        public float scaleCurveTime;
    
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
        private float _speedCurve, _scaleCurve;
        private readonly Timer _gooTimer = new();
        private float _scale;
        private EnemySpawner _spawner;
        
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
            _spawner = FindAnyObjectByType<EnemySpawner>();
            GetComponent<EnemyDamageable>().maxHealth = isLeader ? leaderMaxHealth : maxHealth;
        
            isSpore = false;
            animationState.SwapState("Maturation");
            _spawntimer = initialDelay;
            //GetComponent<SpriteRenderer>().color = Color.blue;

            _numPlatelins++;
        }

        private void Start()
        {
            transform.localScale = (_scale = isLeader ? leaderSize : normalSize) * Vector3.one;
            if (isLeader)
            {
                _leader = this;
                _numColonies++;
            }
        
            _target = GameObject.FindGameObjectWithTag("Player");
        }


        private Vector2 _dampVel;
        void Update()
        {
            if(isSpore){
                GetComponent<EnemyDamageable>().EnemyHeal(sporeHealingPerSecond*Time.deltaTime);
            } else
            {
                for (int i = 0; i < transform.parent.childCount; i++)
                {
                    var plate = transform.parent.GetChild(i);
                    if ((plate.position - transform.position).sqrMagnitude < tendonLength * tendonLength)
                    {
                        var rigid = GetComponent<CustomRigidbody2D>();
                        var diffVel = plate.GetComponent<CustomRigidbody2D>().linearVelocity - rigid.linearVelocity;
                        rigid.AddForce(diffVel.normalized * diffVel.sqrMagnitude * drag);
                        var diffPos = plate.position - transform.position;
                        rigid.AddForce(diffPos.normalized * suction);

                    }
                }
            }

            if(((Vector2)transform.position).sqrMagnitude > 75 * 75)
            {
                GetComponent<CustomRigidbody2D>().AddForce(-60 * ((Vector2)transform.position).normalized);
            }

            // just hard cap it at some point as a backstop
            if (((Vector2)transform.position).sqrMagnitude > 130 * 130)
            {
                transform.position = transform.position.normalized * 130;
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
                if (((Vector2)transform.position).sqrMagnitude < 80 * 80)
                {
                    var targSpeed = speed * speedCurve.Evaluate(_speedCurve / speedCurveScale) *
                                    ((Vector2)(_target.transform.position - transform.position)).normalized;
                    GetComponent<CustomRigidbody2D>().linearVelocity = Vector2.SmoothDamp(GetComponent<CustomRigidbody2D>().linearVelocity, targSpeed, ref _dampVel, 0.1f);
                    _speedCurve = (_speedCurve + Time.deltaTime) % speedCurveScale;
                }
            
                _gooTimer.Update();
                if (_gooTimer.IsFinished)
                {
                    var newGoo = Instantiate(goo, transform.position, transform.rotation);
                    newGoo.transform.localScale *= _scale/normalSize;
                    newGoo.GetComponent<AreaDamager>().owner = transform.parent.gameObject;
                    _gooTimer.Value = gooSpawnTime;
                }
            }

            if (mode == Mode.Idle || _scaleCurve != 0)
            {
                if (_leader._subjects.Count < maxColonySize && _numPlatelins < maxNumColonies * maxColonySize && _spawner.SpawnedEnemies.Any(e=>e.GetComponentInChildren<PlatelinController>() == null)) _spawntimer -= Time.deltaTime;
                if (_spawntimer <= 0)
                {
                    mode = Mode.Swell;
                    _swelltimer = swelltime;
                }
            
                var scale = _scale * scaleCurve.Evaluate(_scaleCurve / scaleCurveTime);
                transform.localScale = new Vector3(scale, scale, 1);
            
                _scaleCurve += Time.deltaTime;
                if (_scaleCurve >= scaleCurveTime) _scaleCurve = 0; // hard clamp to 0 for the above if statement
            } else if (mode == Mode.Swell)
            {
                _swelltimer -= Time.deltaTime;
                var scale = _scale + (swellsize*_scale/normalSize - _scale) * (swelltime - _swelltimer) / swelltime;
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
                
                    if (isLeader && _numColonies < maxNumColonies && Random.value < leaderChance)
                    {
                        var myRot = UtilFuncs.Angle(transform.position);
                        copy.GetComponent<CustomRigidbody2D>().AddForce(25000 * UtilFuncs.AngleToVector(
                            Random.Range(myRot + Mathf.PI/2, myRot - Mathf.PI/2)));
                        copy._spawntimer += 5;
                        copy.animationState.SwapState("Dormant");
                        copy.isSpore = true;
                        copy.isLeader = true;
                        copy._leader = copy;
                    }
                    else
                    {
                        _leader._subjects.Add(copy.gameObject);
                    }
                
                    copy.GetComponent<EnemyDamageable>().maxHealth = copy.isLeader ? leaderMaxHealth : maxHealth;
                }
            }
        }

        private void OnDestroy()
        {
            if (isLeader) _numColonies--;
            _numPlatelins--;
        
            _leader._subjects.Remove(gameObject);
            var newGoo = Instantiate(goo, transform.position, transform.rotation);
            newGoo.transform.localScale *= explosionScaleMult;
            newGoo.GetComponent<AreaDamager>().owner = transform.parent.gameObject;
            if (transform.parent.childCount == 1)
            {
                GetComponent<EnemyDamageable>().healthPickup = healthPickup;
                GetComponent<EnemyDamageable>().SpawnHealthPickups();
                Destroy(transform.parent.gameObject);
            }
        }
    }
}
