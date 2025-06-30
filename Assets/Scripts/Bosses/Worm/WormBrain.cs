using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using Floppy_Bits;
using Player;
using ProgressBars;
using Spawnables;
using Spawnables.Controllers;
using Spawnables.Controllers.Worms;
using Spawnables.Damage;
using Spawnables.Pathfinding;
using UnityEngine;
using UnityEngine.UI;
using Util;
//using Unity.VersionControl.Git.ICSharpCode.SharpZipLib.Zip;
using Random = UnityEngine.Random;

namespace Bosses.Worm
{
    public class WormBrain : HealthHolder
    {
        private float _health;
        public override float Health
        {
            get => _health;
            set
            {
                if (!_isStageTwo) return;
                _health = value;
                bossBar.UpdatePercentage(_health, phaseTwoMaxHealth*2); // should be halfway gone already
            }
        }   
        public override float MaxHealth => phaseTwoMaxHealth;

        public float phaseTwoMaxHealth;
        public ProgressBar bossBar;

        public GameObject head, middle, tail;
        public int middleLength;
        public SnakePathfinder pathfinder;

        public Vector3 targetPosition;
        public GameObject player;

        public float maxTurnAngleDeg;

        private GameObject[] _segments;
        private CustomRigidbody2D _headRigid;
        private float _speed;
        private float _segmentDist;
        private float _ouroborosRadius;
        private Vector2 _currdir;
        private float _swipe;
        public float swipetime;
        public float swipeangle;
        public float tailmomentum;
        public int swipestartindex;

        public float teleportCooldown;
        public float teleportChance;

        public float pursueSpeed;
        public float rushSpeed;
        public float pursueSnakiness;
        public float pursueTurnAngle;

        public float circleSpeed;
        public float ouroborosSpeed;
        public float ouroborosSnakiness;
        public float ouroborosTurnAngle;
        public float ouroborosTime;
        public float ouroborosProgressTime;
        private Timer _ouroborosProgressTimer = null;
        private int _ouroborosProgress;
        private bool _isInCircle = false;

        public float wanderSpeed;
        public float wanderSnakiness;
        public float wanderTurnAngle;

        private float _tarSpeed;
        private float _tarSnakines;
        private float _tarTurnAngle;

        private bool _inCutscene = true;

        private TailController _tailController;
        private SpikeLinkedList _headSpike, _tailSpike;
        private JawGrab _jawGrab;

        public BlackHolePulse bhp;

        public enum MoveMode
        {
            Direct,
            Circle,
            Wander
        }
        public enum ActionGoal
        {
            Rush,
            Tailspike,
            Idle,
            Ouroboros,
            Laser,
        }
        public MoveMode _moveMode;
        public ActionGoal actionGoal;
        private Timer _actionUtilTimer;
        private Timer _teleportTimer = new();
        public bool _isStageTwo = false;
        public bool IsStageTwo => _isStageTwo;

        public float stageTwoSpeedModifier;
        private float _trueTarSpeed => (((Vector2) head.transform.position).magnitude > boundaryCircle.transform.localScale.x/2 ? 2 : 1) * (_isStageTwo && !(_moveMode == MoveMode.Circle && _isInCircle) ? stageTwoSpeedModifier : 1) * _tarSpeed;

        private float _trueTurnAngle => (((Vector2)head.transform.position).magnitude > boundaryCircle.transform.localScale.x / 2 ? 50 : 0) + maxTurnAngleDeg;
        
        struct PortalPair
        {
            public Transform pin;
            public Transform pout;
        };

        private int exportals;
        public int numportals = 0;
        List<PortalPair> _portals = new();
        List<int> _portalIDs = new();

        public float secondsPerSummon;
        private SummonWorm[] _wormSummoners;
        private Timer _summonTimer;

        public Transform portalIn;
        public Transform portalOut;

        private MegaLaserController _mlc;

        private int _eyesAlive, _totalEyes;

        public static WormBrain instance;

        private void Start()
        {
            _health = phaseTwoMaxHealth;
            
            instance = this;
            _mlc = GetComponentInChildren<MegaLaserController>();
            _eyesAlive = _totalEyes = 2 * middleLength;

            _tailController = GetComponentInChildren<TailController>();

            _ouroborosProgressTimer = new Timer();

            _jawGrab = GetComponentInChildren<JawGrab>();

            _actionUtilTimer = new Timer();
            actionGoal = ActionGoal.Idle;
            _segments = new GameObject[middleLength + 2];

            _segments[0] = head;
            _segments[1] = middle;
            _segments[^1] = tail;

            var tailPos = tail.transform.localPosition;
            tailPos.x -= middle.transform.lossyScale.x * (middleLength - 1);

            tail.transform.localPosition = tailPos;


            float totallength = head.transform.lossyScale.x + middle.transform.lossyScale.x + tail.transform.lossyScale.x;
            var prevNode = _headSpike = middle.transform.GetChild(3).GetComponentInChildren<SpikeLinkedList>().previous = middle.transform.GetChild(2).GetComponentInChildren<SpikeLinkedList>();
            prevNode.next = prevNode = middle.transform.GetChild(3).GetComponentInChildren<SpikeLinkedList>();

            for (var i = 2; i < middleLength + 1; i++)
            {
                var segment = (_segments[i] = Instantiate(middle)).transform;
                if (i%2 == 0)
                {
                    Destroy(segment.GetChild(0).gameObject);
                    Destroy(segment.GetChild(1).gameObject);
                    _totalEyes = _eyesAlive -= 2;
                }
                
                segment.SetParent(transform, false);

                segment.GetChild(2).GetComponent<SpikeLinkedList>().previous = prevNode;
                prevNode = prevNode.next = segment.GetChild(2).GetComponent<SpikeLinkedList>();

                segment.GetChild(3).GetComponent<SpikeLinkedList>().previous = prevNode;
                prevNode = prevNode.next = segment.GetChild(3).GetComponent<SpikeLinkedList>();

                var pos = _segments[i - 1].transform.localPosition;

                totallength += _segmentDist = segment.lossyScale.x * 0.9f;
                pos.x -= _segmentDist;
                segment.localPosition = pos;
            }
            
            _summonTimer = new Timer();
            _wormSummoners = GetComponentsInChildren<SummonWorm>();

            var tailLists = tail.GetComponentsInChildren<SpikeLinkedList>();
            prevNode!.next = tailLists[0];
            tailLists[0].previous = prevNode;
            tailLists[0].next = _tailSpike = tailLists[1];
            tailLists[1].previous = tailLists[0];

            _ouroborosRadius = totallength / (2 * Mathf.PI);

            _headRigid = head.GetComponent<CustomRigidbody2D>();
            _moveMode = MoveMode.Wander;

            StartCoroutine(Cutscene());
        }

        public void EyeDead()
        {
            _eyesAlive--;
            if (_eyesAlive == 0)
            {
                _isStageTwo = true;
                _actionUtilTimer.Value = 0;
                
                // TODO: phase two camera pan/roar

                foreach (var dmgable in GetComponentsInChildren<WormDamageable>()) dmgable.enabled = true;
            }

            bossBar.UpdatePercentage(_totalEyes + _eyesAlive, _totalEyes * 2);
        }

        private void Update()
        {
            if (_inCutscene) return;
            
            if (InputManager.GetKeyDown(KeyCode.RightBracket))
            {
                foreach (var dmgable in GetComponentsInChildren<WormEyeDamageable>())
                {
                    dmgable.Damage(float.PositiveInfinity, gameObject); // kill, triggering OnDeath
                }
            }

            UpdateMovement();

            var _snakiness = pathfinder.snakeyness;


            /* Code for managing speeding up and slowing down, too small to really need a function especially since it occurs once */
            {
                _speed = Mathf.Clamp(_speed + 10 * Time.deltaTime * MathF.Sign(_trueTarSpeed - _speed), Mathf.Min(_speed, _trueTarSpeed), Mathf.Max(_speed, _trueTarSpeed));
                pathfinder.snakeyness = Mathf.Clamp(_snakiness + .1f * Time.deltaTime * MathF.Sign(_tarSnakines - _snakiness), Mathf.Min(_snakiness, _tarSnakines), Mathf.Max(_snakiness, _tarSnakines));
                maxTurnAngleDeg = Mathf.Clamp(maxTurnAngleDeg + 40 * Time.deltaTime * MathF.Sign(_tarTurnAngle - maxTurnAngleDeg), Mathf.Min(maxTurnAngleDeg, _tarTurnAngle), Mathf.Max(maxTurnAngleDeg, _tarTurnAngle));
            }
            
            if (!_isStageTwo)
            {
                _summonTimer.Update();
                if (_summonTimer.IsFinished)
                {
                    var summon = _wormSummoners[Random.Range(0, _wormSummoners.Length)];
                    if (summon != null) summon.TrySummon();

                    _summonTimer.Value = secondsPerSummon;
                }
            }

            if (actionGoal == ActionGoal.Ouroboros)
            {
                if (!_isInCircle)
                {
                    _isInCircle = _segments.All(seg => seg == head ||
                        ((Vector2)seg.transform.position).sqrMagnitude - _ouroborosRadius * _ouroborosRadius < 6);
                }

                if (_isInCircle)
                {
                    _ouroborosProgressTimer.Update();
                    if (_ouroborosProgressTimer.IsFinished)
                    {
                        var spike = _tailSpike;
                        for (var i = 0; i < _ouroborosProgress; i++) spike = spike!.previous;

                        if (spike != null)
                        {
                            spike!.isAnim = true;

                            if (_ouroborosProgress >= 2 && _ouroborosProgress % 2 == 1)
                            {
                                var lasers = _segments[^(1 + _ouroborosProgress / 2)].GetComponentsInChildren<MegaLaserController>();

                                if (!lasers[0].IsShooting)
                                {
                                    lasers[0].beamLoopTime = lasers[1].beamLoopTime = _actionUtilTimer.Value + 1 -
                                        (lasers[0].laserBuildupTime - lasers[0].TimeToLightning) -
                                        lasers[0].beamBuildupTime;
                                    lasers[0].Shoot(boundaryCircle.transform.localScale.x/2 - _ouroborosRadius - middle.transform.lossyScale.y / 2, lasers[0].TimeToLightning);
                                    lasers[1].Shoot(_ouroborosRadius - middle.transform.lossyScale.y / 2 - 9.5f, lasers[0].TimeToLightning);
                                }
                            }

                            _ouroborosProgress++;
                        }
                        else
                        {
                            _ouroborosProgress = 0;
                        }

                        _ouroborosProgressTimer.Value = ouroborosProgressTime;
                    }
                }
            }

            _actionUtilTimer.Update();
            if (_actionUtilTimer.IsFinished)
            {
                if (_isStageTwo) StartCoroutine(bhp.Pulse());
                
                if (actionGoal == ActionGoal.Tailspike)
                {
                    TailSwipe();
                }

                StartCoroutine(_headSpike.TriggerDown());
                float rand = Random.value;
                if (!_isStageTwo)
                {
                    bool portalable = false;

                    switch (rand)
                    {
                        case < .25f:
                            /*Do Rush*/
                            actionGoal = ActionGoal.Rush;
                            _actionUtilTimer.Value = Random.Range(12f, 18f);

                            portalable = true;
                            break;
                        case < .75f:
                            /*Do Tailspike*/
                            actionGoal = ActionGoal.Tailspike;
                            _actionUtilTimer.Value = Random.Range(1f, 4f);

                            portalable = true;
                            break;
                        default:
                            actionGoal = ActionGoal.Idle;
                            _actionUtilTimer.Value = Random.Range(5f, 8f);

                            portalable = true;
                            break;
                    }

                    if (portalable && _teleportTimer.IsFinished && Random.value < teleportChance)
                    {
                        _teleportTimer.Value = teleportCooldown;
                        exportals = Random.Range(0, 0);
                        SpawnPortal();
                    }
                }
                else
                {
                    bool portalable = false;
                    switch (rand)
                    {
                        case < .20f:
                            /*Do Central laser*/
                            ShootLaser();
                            break;
                        case < .35f:
                            /*Do Ouroboros*/
                            actionGoal = ActionGoal.Ouroboros;
                            _actionUtilTimer.Value = ouroborosTime;
                            _ouroborosProgressTimer.Value = 0;
                            _ouroborosProgress = 0;
                            _isInCircle = false;

                            _jawGrab.enabled = false;
                            StartCoroutine(EnableJaw());
                            break;
                        case < .75f:
                            /*Do Rush*/
                            actionGoal = ActionGoal.Rush;
                            _actionUtilTimer.Value = Random.Range(12f, 18f);

                            portalable = true;
                            break;

                        case < 1.0f:
                            /*Do Tailspike*/
                            actionGoal = ActionGoal.Tailspike;
                            _actionUtilTimer.Value = Random.Range(1f, 4f);

                            portalable = true;
                            break;

                        default:
                            actionGoal = ActionGoal.Idle;
                            _actionUtilTimer.Value = Random.Range(5f, 8f);

                            portalable = true;
                            break;
                    }

                    if (portalable && _teleportTimer.IsFinished && Random.value < teleportChance)
                    {
                        _teleportTimer.Value = teleportCooldown;
                        exportals = Random.Range(0, 0);
                        SpawnPortal();
                    }
                }
            }
            switch (actionGoal)
            {
                case ActionGoal.Rush:
                    if (player.GetComponent<Movement>().inputBlocked)
                    {
                        actionGoal = ActionGoal.Idle;
                        _actionUtilTimer.Value = _jawGrab.holdTime;
                        goto case ActionGoal.Idle;
                    }
                    _moveMode = MoveMode.Direct;
                    break;

                case ActionGoal.Idle:
                case ActionGoal.Tailspike:
                    _moveMode = MoveMode.Wander;
                    break;

                case ActionGoal.Ouroboros:
                    _moveMode = MoveMode.Circle;
                    break;
                case ActionGoal.Laser:
                    _moveMode = MoveMode.Direct;
                    break;
            }

        }

        private IEnumerator EnableJaw()
        {
            yield return new WaitForSeconds(ouroborosTime);
            _jawGrab.enabled = true;
        }

        private void TailSwipe()
        {
            if (_moveMode == MoveMode.Wander)
            {
                _swipe = 2.5f * swipetime;
            } else
            {
                _swipe = 0;
            }

        }

        private void ShootLaser()
        {
            _mlc.Shoot(4 * boundaryCircle.transform.localScale.x/2);
            _actionUtilTimer.Value = _mlc.laserBuildupTime + _mlc.beamBuildupTime + _mlc.beamLoopTime;

            actionGoal = ActionGoal.Laser;
        }
        
        public IEnumerator BiteStart()
        {
            yield return new WaitForSeconds(0.75f);
            
            if (_isStageTwo && _jawGrab.HasPlayer && actionGoal != ActionGoal.Laser && !_mlc.IsShooting) ShootLaser();
        }
        
        public void BiteFinish()
        {
            actionGoal = _mlc.IsShooting ? ActionGoal.Laser : ActionGoal.Idle;
        }

        private int _spikesReleased;
        private void UpdateMovement()
        {
            if (numportals > 0 && _portalIDs[numportals - 1] <= 3)
            {
                if (_portalIDs[numportals - 1] > 0 || _fakeSegs[numportals-1] != null) // continue straight out
                {
                    targetPosition = FromLocalSpace(_portals[numportals - 1].pout, 3 * _segmentDist * Vector3.left);;
                }
                else
                {
                    targetPosition = _portals[numportals - 1].pin.position;
                }
                _tarSpeed = pursueSpeed;
                _tarTurnAngle = pursueTurnAngle;
                _tarSnakines = 0;
            }
            else
            {
                switch (_moveMode)
                {
                    case MoveMode.Wander:
                        _tarSpeed = wanderSpeed;
                        _tarTurnAngle = wanderTurnAngle;
                        _tarSnakines = wanderSnakiness;
                        
                        randTarg:
                        if ((head.transform.position - targetPosition).sqrMagnitude < 120)
                        {
                            while ((head.transform.position - targetPosition).sqrMagnitude < 4000)
                                targetPosition = Random.Range(20, 70) *
                                                 pathfinder.AngleToVector(Random.Range(0, 6.28f));
                        }

                        break;
                    case MoveMode.Direct:
                        _tarTurnAngle = pursueTurnAngle;
                        _tarSnakines = actionGoal == ActionGoal.Laser ? 0 : pursueSnakiness;

                        if ((player.transform.position - head.transform.position).sqrMagnitude < 30 * 30)
                        {
                            if (_speed < _trueTarSpeed)
                                _speed *= Mathf.Pow(1.10f, Time.deltaTime);
                            _tarSpeed = rushSpeed;
                            
                            goto randTarg;
                        }
                        else
                        {
                            if ((int) _tarSpeed == (int) rushSpeed) // if we had our chance, cancel
                            {
                                actionGoal = ActionGoal.Idle;
                                _actionUtilTimer.Value = 0;
                            }
                            
                            targetPosition = player.transform.position;
                            _tarSpeed = pursueSpeed;
                        }

                        break;
                    case MoveMode.Circle:
                        _tarSpeed = _isInCircle ? circleSpeed : ouroborosSpeed;
                        _tarTurnAngle = _isInCircle ? 360 : ouroborosTurnAngle;
                        _tarSnakines = ouroborosSnakiness;
                        // targetPosition = UtilFuncs.TangentPointOnCircleFromPoint(Vector2.zero, _ouroborosRadius,
                            // head.transform.position);

                        var radPerSec = _trueTarSpeed / _ouroborosRadius; // angular velocity
                        targetPosition = Quaternion.Euler(0, 0, -Mathf.Rad2Deg * (Time.deltaTime * radPerSec + Mathf.Asin(_segmentDist / _ouroborosRadius))) *
                                         ((Vector2) head.transform.position).normalized * _ouroborosRadius;
                        // targetPosition = (Vector3) (((Vector2) (targetPosition - head.transform.position)).normalized * _segmentDist) + head.transform.position;
                        break;
                }
            }

            var dir = pathfinder.PathDirNorm(_segments[0].transform.position, targetPosition);
            var toOrigin = ((Vector2) head.transform.position).normalized;
            var awayFromOrigin = Quaternion.Euler(0, 0, Mathf.Sign(Vector3.SignedAngle(toOrigin, dir, Vector3.forward))*90) * toOrigin;
            dir = Vector3.RotateTowards(dir, awayFromOrigin, Vector2.Dot(dir, toOrigin), 0);
            
            Vector2 prevAngle = pathfinder.AngleToVector(Mathf.Deg2Rad * _segments[0].transform.rotation.eulerAngles.z);
            dir = Vector3.RotateTowards(prevAngle, dir, Mathf.Deg2Rad * _trueTurnAngle * Time.deltaTime, 1);
            // dir = pathfinder.ClampAngle(dir, prevAngle, Mathf.Deg2Rad * maxTurnAngleDeg);

            // _currdir = dir = (.1f / Time.deltaTime * _currdir + dir).normalized;

            //_headRigid.AddForce(_speed * dir, ForceMode.VelocityChange)
            _headRigid.linearVelocity = _speed * dir;

            if (numportals == 0 || _portalIDs[numportals - 1] > 0)
            {
                var angle = Mathf.Atan2(dir.y, dir.x);
                _segments[0].transform.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * angle);
            }

            RippleSegments();
            if (_swipe > swipetime)
            {
                if (_spikesReleased < 3 && _swipe - swipetime < swipetime - swipetime / 6 / 3 * _spikesReleased)
                {
                    _tailController.ReleaseSpike(_spikesReleased++);
                }
            }
            else if (_swipe > 0)
            {
                if (_spikesReleased < 6 && _swipe < swipetime - swipetime / 6 / 3 * (_spikesReleased - 3))
                {
                    _tailController.ReleaseSpike(_spikesReleased++);
                }
            }
            else
            {
                if (_spikesReleased != 0)
                {
                    StartCoroutine(_tailController.RegrowSpikes());
                    _spikesReleased = 0;
                }
            }
            if (_swipe > 0) _swipe -= Time.deltaTime;
        }

        public void SpawnPortal(Vector2? inPos=null, Vector2? outPos=null)
        {
            var newPin = Instantiate(portalIn);
            var newPout = Instantiate(portalOut);
            newPin.gameObject.SetActive(true);
            newPout.gameObject.SetActive(true);

            var oldZ = newPin.position.z;
            var newpos = (Vector3) (inPos ?? (Vector3)(Vector2)head.transform.position + Random.Range(30, 80) * head.transform.right) + oldZ * Vector3.forward;
            newPin.position = newpos;
            newPin.rotation = head.transform.rotation;

            do newpos = (Vector3) (outPos ?? (Vector3)(Random.Range(20, 70) * pathfinder.AngleToVector(Random.Range(0, 6.28f)))) + oldZ * Vector3.forward;
            while ((head.transform.position - newpos).sqrMagnitude < 4000 || (newpos - newPin.position).sqrMagnitude < 4000);
            newPout.position = newpos;
            newPout.rotation = Quaternion.Euler(0, 0, ((Vector2) newpos).sqrMagnitude == 0 ? 210 : -Mathf.Rad2Deg * Mathf.Atan2(newpos.y, newpos.x));

            var pair = new PortalPair { pin = newPin , pout = newPout};
            _portals.Add(pair);
            _portalIDs.Add(0);

            _fakeSegs.Add(null);

            numportals += 1;
            exportals -= 1;

            targetPosition = _portals[numportals - 1].pin.position;
        }

        private readonly List<GameObject> _fakeSegs = new();
        private GameObject _headFake;
        private void IncrementPortalSeg(int portalID)
        {
            if (_fakeSegs[portalID] != null) Destroy(_fakeSegs[portalID]);

            foreach (var summoner in _segments[_portalIDs[portalID]].GetComponentsInChildren<SummonWorm>()) summoner.enabled = true;
            var newSeg = ++_portalIDs[portalID];
            foreach (var summoner in _segments[newSeg].GetComponentsInChildren<SummonWorm>()) summoner.enabled = false;

            _fakeSegs[portalID] = Instantiate(_segments[newSeg], null, true);

            _segments[newSeg].transform.rotation = Quaternion.Euler(0, 0, 180 + _portals[portalID].pout.rotation.eulerAngles.z);
            _segments[newSeg].transform.position = FromLocalSpace(_portals[portalID].pout, _segmentDist/2 * Vector3.right);

            foreach (var rend in _fakeSegs[portalID].GetComponentsInChildren<SpriteRenderer>())
            {
                rend.sortingLayerName = "Teleport In";
            }

            foreach (var mask in _fakeSegs[portalID].GetComponentsInChildren<SpriteMask>())
            {
                mask.backSortingLayerID = mask.frontSortingLayerID = SortingLayer.NameToID("Teleport In");
            }

            foreach (var mmi in _fakeSegs[portalID].GetComponentsInChildren<MiniMapIcon>()) Destroy(mmi);
            foreach (var hb in _fakeSegs[portalID].GetComponentsInChildren<HealthBar>()) Destroy(hb);
            foreach (var dmgable in _fakeSegs[portalID].GetComponentsInChildren<Damageable>()) Destroy(dmgable);
            foreach (var summoner in _fakeSegs[portalID].GetComponentsInChildren<SummonWorm>()) Destroy(summoner);
            foreach (var tail in _fakeSegs[portalID].GetComponentsInChildren<TailController>()) Destroy(tail);
        }

        public void RippleSegments()
        {
            var swipeAngle = swipeangle * Mathf.Pow(-1, (int)(_swipe / swipetime));

            var nextSegment = _segments[0];
            Vector3 currSegmentPos = _segments[0].transform.position;
            bool teled = false;
            for (int idx = 0; idx < numportals; idx++)
            {
                var segID = _portalIDs[idx];
                if (segID == 0)
                {
                    var pair = _portals[idx];

                    var pairHeadIsAt = _fakeSegs[idx] == null ? pair.pin : pair.pout;
                    currSegmentPos = SnapAlongPortal(pairHeadIsAt, currSegmentPos);
                    head.transform.rotation = Quaternion.Lerp(head.transform.rotation, Quaternion.Euler(0, 0, (pairHeadIsAt == pair.pout ? 180 : 0) + pairHeadIsAt.rotation.eulerAngles.z), 5 * Time.deltaTime);
                    float along = ValueAlongPortal(pairHeadIsAt, currSegmentPos);

                    // this has to be set up to correctly instantiate the fake one
                    _segments[0].transform.position = currSegmentPos;

                    if (_fakeSegs[idx] == null)
                    {
                        if (along > -_segmentDist*3/2)
                        {
                            nextSegment = _fakeSegs[idx] = Instantiate(_segments[0], null, true);
                            nextSegment.transform.position = currSegmentPos;

                            foreach (var gen in _fakeSegs[idx].GetComponentsInChildren<GenerateUniformFloppy>()) Destroy(gen);
                            int i = 0;
                            foreach (var fb in _fakeSegs[idx].GetComponentsInChildren<FloppyBrain>())
                            {
                                fb.Clone(head.GetComponentsInChildren<FloppyBrain>()[i++]);
                                for (var j = 0; j < fb.segments.Count; j++) fb.Positions[j] = fb.segments[j].position;
                            }
                            Destroy(_fakeSegs[idx].GetComponent<CustomRigidbody2D>());
                            Destroy(_fakeSegs[idx].GetComponent<Rigidbody2D>());
                            Destroy(_fakeSegs[idx].GetComponentInChildren<JawGrab>());

                            _segments[0].transform.RotateAround(head.transform.position, Vector3.forward, 180 + pair.pout.rotation.eulerAngles.z - head.transform.rotation.eulerAngles.z);
                            _segments[0].transform.position = OutofPortal(pair, currSegmentPos);

                            foreach (var fb in head.GetComponentsInChildren<FloppyBrain>())
                            {
                                for (var j = 0; j < fb.segments.Count; j++) fb.Positions[j] = fb.segments[j].position;
                            }

                            _currdir = pathfinder.AngleToVector(Mathf.Deg2Rad * head.transform.rotation.eulerAngles.z);

                            foreach (var rend in _fakeSegs[idx].GetComponentsInChildren<SpriteRenderer>())
                            {
                                rend.sortingLayerName = "Teleport In";
                            }
                        }
                    }
                    else
                    {
                        nextSegment = _fakeSegs[idx];
                        _fakeSegs[idx].transform.position = IntoPortal(pair, currSegmentPos);

                        if (along < -_segmentDist/2)
                        {
                            foreach (var col in _fakeSegs[idx].GetComponentsInChildren<Collider2D>()) Destroy(col);
                            _headFake = _fakeSegs[idx];
                            _fakeSegs[idx] = null;
                            IncrementPortalSeg(idx);
                            teled = true;
                        }
                    }

                    break;
                }
            }

            if (_headFake != null)
            {
                _headFake.transform.position = SnapAlongPortal(_portals[numportals-1].pin,
                    IntoPortal(_portals[numportals - 1], currSegmentPos));

                var along = ValueAlongPortal(_portals[numportals - 1].pout, currSegmentPos);
                if (along < -_segmentDist * 4)
                {
                    Destroy(_headFake);
                }
            }

            for (var i = 1; i < middleLength + 1; i++)
            {
                Vector3 nextSegmentPos = nextSegment.transform.position;
                currSegmentPos = _segments[i].transform.position;
                Vector3 prevSegmentPos = _segments[i + 1].transform.position;

                nextSegment = _segments[i]; // for next iteration

                bool exitingPortal = false;
                PortalPair? portalPair = null;
                for (int idx = 0; idx < numportals; idx++)
                {
                    var segID = _portalIDs[idx];
                    if (segID == i)
                    {
                        var pair = _portals[idx];

                        portalPair = pair;
                        exitingPortal = true;
                        var oc = currSegmentPos;
                        currSegmentPos = FromLocalSpace(pair.pout, new Vector2(
                            IntoLocalSpace(pair.pout, currSegmentPos).x - Time.deltaTime * _headRigid.linearVelocity.magnitude, 0));

                        _fakeSegs[idx].transform.position = IntoPortal(pair, currSegmentPos);
                        float along = ValueAlongPortal(pair.pin, _fakeSegs[idx].transform.position);

                        if (along > _segmentDist/2)
                        {
                            IncrementPortalSeg(idx);
                        }
                        else
                        {
                            nextSegment = _fakeSegs[idx]; // only set it to fake seg if that's still this one
                        }
                        break;
                    }
                }
                Vector3 curr2next = (nextSegmentPos - currSegmentPos).normalized;

                if (!exitingPortal)
                {
                    if (_swipe > 0 && i >= swipestartindex && _portalIDs.All(j => j < i))
                    {
                        Vector2 next2nnext = (_segments[i - 2].transform.position - nextSegmentPos).normalized;
                        Vector2 tarcurr2next = UtilFuncs.Rot(next2nnext, swipeAngle);
                        curr2next = (tailmomentum * .03f / Time.deltaTime * curr2next + (Vector3)tarcurr2next).normalized;
                    }
                    currSegmentPos = nextSegmentPos + -_segmentDist * curr2next;
                }
                else
                {
                    // if `i` is stuck in a portal, snap everything in front to it
                    for (int j = i - 1; j >= 1; j--)
                    {
                        var curPos = _segments[j].transform.position;
                        var nextPos = _segments[j-1].transform.position;
                        var prevPos = _segments[j+1].transform.position;

                        var curr2prev = (curPos - prevPos).normalized;
                        var next2curr = (nextPos - curPos).normalized;

                        _segments[j].transform.position = prevPos + _segmentDist * curr2prev;

                        var mean = 0.5f * (curr2prev + next2curr);
                        var deg = Mathf.Atan2(mean.y, mean.x) * Mathf.Rad2Deg;
                        _segments[j].transform.rotation = Quaternion.Euler(0, 0, deg);
                    }

                    {
                        var curPos = _segments[0].transform.position;
                        var prevPos = _segments[1].transform.position;

                        var curr2prev = (curPos - prevPos).normalized;

                        _segments[0].transform.position = prevPos + _segmentDist * curr2prev;
                    }
                }

                if (numportals == 0 && _moveMode == MoveMode.Circle && ((Vector2)currSegmentPos).sqrMagnitude <= _ouroborosRadius * _ouroborosRadius)
                {
                    currSegmentPos += .1f * (Vector3)((Vector2)currSegmentPos).normalized;
                }
                _segments[i].transform.position = currSegmentPos;

                Vector3 prev2curr = (currSegmentPos - prevSegmentPos).normalized;
                Vector3 meanDir = .5f * (prev2curr + curr2next);

                var angle = Mathf.Atan2(meanDir.y, meanDir.x);
                _segments[i].transform.rotation = portalPair != null ? Quaternion.Euler(0, 0, 180 + portalPair.Value.pout.rotation.eulerAngles.z) : Quaternion.Euler(0, 0, Mathf.Rad2Deg * angle);
            }

            { //Scope naming stuffs
                Vector3 nextSegmentPos = nextSegment.transform.position;
                currSegmentPos = _segments[^1].transform.position;

                Vector3 curr2next = (nextSegmentPos - currSegmentPos).normalized;

                if (_swipe > 0 && _portalIDs.All(j => j < _segments.Length - 1))
                {
                    Vector2 next2nnext = (_segments[^3].transform.position - nextSegmentPos).normalized;
                    Vector2 tarcurr2next = UtilFuncs.Rot(next2nnext, swipeAngle);
                    curr2next = (tailmomentum * .03f / Time.deltaTime * curr2next + (Vector3)tarcurr2next).normalized;
                }
                currSegmentPos = nextSegmentPos + -_segmentDist * curr2next;

                PortalPair? portalPair = null;
                for (int idx = 0; idx < numportals; idx++)
                {
                    var segID = _portalIDs[idx];
                    if (segID == _segments.Length - 1)
                    {
                        var pair = _portals[idx];

                        portalPair = pair;
                        currSegmentPos = SnapAlongPortal(pair.pout, currSegmentPos);
                        _fakeSegs[idx].transform.position = IntoPortal(pair, currSegmentPos);
                        float along = ValueAlongPortal(pair.pin, _fakeSegs[idx].transform.position);

                        if (along > _segmentDist/2)
                        {
                            Destroy(pair.pin.gameObject);
                            Destroy(pair.pout.gameObject);
                            Destroy(_fakeSegs[idx]);

                            _portals.RemoveAt(idx);
                            _portalIDs.RemoveAt(idx);
                            _fakeSegs.RemoveAt(idx);

                            numportals--;
                        }
                        break;
                    }
                }

                if (_moveMode == MoveMode.Circle && ((Vector2)currSegmentPos).sqrMagnitude <= _ouroborosRadius * _ouroborosRadius)
                {
                    currSegmentPos += .1f * (Vector3)((Vector2)currSegmentPos).normalized;
                }
                _segments[^1].transform.position = currSegmentPos;

                var angle = Mathf.Atan2(curr2next.y, curr2next.x);
                _segments[^1].transform.rotation = portalPair != null ? Quaternion.Euler(0, 0, 180 + portalPair.Value.pout.rotation.eulerAngles.z) : Quaternion.Euler(0, 0, Mathf.Rad2Deg * angle);
            }

            if (teled && exportals > 0) SpawnPortal();

            for (var i = 0; i < middleLength + 2; i++)
            {
                foreach (var rend in _segments[i].GetComponentsInChildren<SpriteRenderer>())
                {
                    rend.sortingLayerName = "Boss";
                    if (i == 0 && _headFake != null) rend.sortingLayerName = "Teleport Out";

                    for (int idx = 0; idx < numportals; idx++)
                    {
                        if (_portalIDs[idx] == i) rend.sortingLayerName = "Teleport Out";
                    }
                }
            }
        }

        Vector2 IntoPortal(PortalPair pair, Vector2 position)
        {
            return FromLocalSpace(pair.pin, -IntoLocalSpace(pair.pout, position));
        }

        Vector2 OutofPortal(PortalPair pair, Vector2 position)
        {
            return FromLocalSpace(pair.pout, -IntoLocalSpace(pair.pin, position));
        }

        float OutofPortalRot(PortalPair pair, float rot)
        {
            float nrot = rot - pair.pin.eulerAngles.z + pair.pout.eulerAngles.z;
            return nrot;
        }

        Vector2 SnapAlongPortal(Transform transform, Vector2 position)
        {
            var local = IntoLocalSpace(transform, position);
            return FromLocalSpace(transform, new Vector2(local.x, 0));
        }

        float ValueAlongPortal(Transform transform, Vector2 position)
        {
            var local = IntoLocalSpace(transform, position);
            return local.x;
        }

        Vector2 IntoLocalSpace(Transform transform, Vector2 position)
        {
            Vector2 delta = position - (Vector2)transform.position;
            Vector2 norm = transform.right;
            Vector2 perp = transform.up;
            float x = Vector2.Dot(delta, norm);
            float y = Vector2.Dot(delta, perp);
            return new Vector2(x, y);
        }

        Vector2 FromLocalSpace(Transform transform, Vector2 position)
        {
            Vector2 norm = transform.right;
            Vector2 perp = transform.up;
            Vector2 x = norm * position.x;
            Vector2 y = perp * position.y;
            return x + y + (Vector2)transform.position;
        }

        public string editorNote = "Cutscene Variables";
        public GameObject fadeIn, portal, planet, gradient, defenses, boundaryCircle;
        public new ParticleSystem particleSystem;
        public Texture2D destructionSprite;
        public float destructionFps;
        public float fadeInTime;
        public float waitForZoomTime;
        public float zoomTime, zoomCamSize;
        public float waitForPortalTime;
        public float waitForLaserTime;
        public float waitForPanTime;
        public float panTime;
        public float zoomOutTime, zoomOutCamSize;
        public Vector2[] endpoints;
        public float[] segmentSpeeds;
        public float maxTurnAngle;
        public float playerMoveVel;
        public AnimationCurve jawWiggle;
        public float wiggleScale;
        public float spitTime;
        private IEnumerator Cutscene()
        {
            var destructionAnim = new UtilFuncs.Anim();
            UtilFuncs.SetupTexture(destructionSprite, destructionAnim, 128/196f);
            
            yield return new WaitForEndOfFrame();
            
            _headRigid.linearVelocity = 7*Vector2.right; // to make sure the floppies look good lol

            var oldCircleScale = boundaryCircle.transform.localScale;
            boundaryCircle.transform.localScale = new Vector3(1000, 1000, 1);

            var emission = particleSystem.emission;
            var shape = particleSystem.shape;
            var main = particleSystem.main;
            emission.rateOverTimeMultiplier *= 5;
            shape.scale *= 5;
            main.maxParticles *= 5;
            
            // setup
            var cam = Camera.main;
            var camFp = cam!.GetComponent<FollowPlayer>();
            camFp.Enabled = false;
            cam.transform.position = (Vector3) (Vector2) player.transform.position + new Vector3(0, 0, cam.transform.position.z);
            cam.transform.rotation = Quaternion.Euler(0, 0, -45);
            cam.orthographicSize = camFp.baseSize;
            for (var i = 0; i < fadeIn.transform.parent.childCount; i++)
            {
                fadeIn.transform.parent.GetChild(i).gameObject.SetActive(false);
            }
            fadeIn.SetActive(true);
            player.GetComponent<Movement>().SetInputBlocked(true);
            var playerDist = player.transform.position.magnitude;
            var playerDir = player.transform.position.normalized;
            
            // fade in
            var fadeImg = fadeIn.GetComponent<Image>();
            for (float t = 0; t < fadeInTime; t += Time.fixedDeltaTime)
            {
                yield return new WaitForFixedUpdate();
                fadeImg.SetAlpha(Mathf.SmoothStep(1, 0, t / fadeInTime));
            }
            fadeIn.SetActive(false);

            yield return new WaitForSeconds(waitForZoomTime);
            
            // zoom out
            for (float t = 0; t < zoomTime; t += Time.fixedDeltaTime)
            {
                yield return new WaitForFixedUpdate();
                cam.orthographicSize = Mathf.SmoothStep(camFp.baseSize, zoomCamSize, t / zoomTime);
            }
            
            yield return new WaitForSeconds(waitForPortalTime);
            
            // spawn portal
            portal.SetActive(true);

            yield return new WaitForSeconds(waitForLaserTime);

            // mega laser sequence
            var mlc = portal.GetComponentInChildren<MegaLaserController>();
            mlc.Shoot(portal.transform.GetChild(1).position.magnitude, mlc.TimeToLightning);

            yield return new WaitForSeconds((mlc.laserBuildupTime - mlc.TimeToLightning)*3/4);
            
            // dodge as mega laser laserifies
            player.GetComponent<Movement>().DodgeOnceDir = Quaternion.Euler(0, 0, 90) * playerDir;
            player.GetComponent<Movement>().DodgeOnceCost = 0;
            
            yield return new WaitForSeconds((mlc.laserBuildupTime - mlc.TimeToLightning)*1/4);
            
            // setup planet sprite
            planet.transform.rotation = Quaternion.Euler(0, 0, -45);
            var planetAnim = planet.GetComponent<NSpriteAnimation>();
            planetAnim.enabled = false;
            var planetSr = planet.GetComponent<SpriteRenderer>();
            var spriteRatio = destructionAnim.Sprites[0].textureRect.width / planetSr.sprite.rect.width;
            
            planetSr.sprite = destructionAnim.Sprites[0];

            camFp.ScreenShake(waitForPanTime+destructionAnim.NumFrames/destructionFps, 0.8f);
            
            yield return new WaitForSeconds(waitForPanTime);

            planet.transform.localScale *= spriteRatio;
            mlc.SetLength(portal.transform.GetChild(1).position.magnitude - planet.transform.localScale.y / 2 - 0.53f);
            
            float timeElapsed = 0;
            
            // camera follows laser to planet
            var start = cam.transform.position;
            var dir = -playerDir;
            for (float t = 0; t < panTime; t += Time.fixedDeltaTime)
            {
                yield return new WaitForFixedUpdate();
                cam.transform.position = start + (Vector3) camFp.ShakeOffset + dir * Mathf.SmoothStep(0, playerDist, t / panTime);

                timeElapsed += Time.fixedDeltaTime;
                planetSr.sprite = destructionAnim.Sprites[(int) (timeElapsed * destructionFps)];
            }
            
            // planet => black hole animation (camera pulses w/ black hole?) (screen shake?)
            while ((int)((timeElapsed+Time.fixedDeltaTime) * destructionFps) < destructionAnim.NumFrames)
            {
                yield return new WaitForFixedUpdate();
                
                timeElapsed += Time.fixedDeltaTime;
                planetSr.sprite = destructionAnim.Sprites[(int) (timeElapsed * destructionFps)];
            }

            planetSr.sprite = planetAnim.states[0].frames[0];
            planetAnim.enabled = true;
            planet.transform.rotation = Quaternion.identity;
            planet.transform.localScale /= spriteRatio;

            // black hole sends out shockwave, camera zooms out, all enemies destroyed, player dodges
            yield return new WaitForSeconds(2.5f);
            
            // worm emerges from black hole, circles
            portal.SetActive(false);
            
            defenses.SetActive(false);
            gradient.SetActive(true);
            
            SpawnPortal(head.transform.position + 10*Vector3.right, Vector2.zero);
            _portals[numportals-1].pout.GetChild(2).gameObject.SetActive(false); // disable portal visual
            StartCoroutine(_headSpike.TriggerDown());

            pathfinder.snakeyness = 0;
            float curSpeed = 0;
            var camStart = cam.transform.position;
            var camDir = ((Vector2)(player.transform.position - camStart)).normalized;
            var camDist = (player.transform.position - camStart).magnitude;
            var camStartRot = cam.transform.rotation;
            var camEndRot = Quaternion.Euler(new Vector3(0,0,-90+Mathf.Rad2Deg*Mathf.Atan2(player.transform.position.y,player.transform.position.x)));

            float time = 0;
            for (var i = 0; i <= endpoints.Length; i++)
            {
                var speed = i < endpoints.Length-1 ? segmentSpeeds[i] : 0;
                if (curSpeed == 0) curSpeed = speed * 3/5;

                float dist;
                var targ = i < endpoints.Length ? endpoints[i] : (Vector2) player.transform.position;
                var startDist = Vector2.Distance(head.transform.position, targ);

                while ((dist = Vector2.Distance(head.transform.position, targ)) > (i == endpoints.Length ? 26 : 4))
                {
                    yield return new WaitForFixedUpdate();
                    time += Time.fixedDeltaTime;

                    // put this at the same time
                    if (time < zoomOutTime)
                    {
                        cam.orthographicSize = Mathf.SmoothStep(zoomCamSize, zoomOutCamSize, time / zoomOutTime);
                    }
                    
                    var toEndpoint = (targ - (Vector2) _segments[0].transform.position).normalized;
                    var prevAngle = pathfinder.AngleToVector(Mathf.Deg2Rad * _segments[0].transform.rotation.eulerAngles.z);
                    var finalDir = Vector3.RotateTowards(prevAngle, toEndpoint, Mathf.Deg2Rad * (i < 2 ? 120 : maxTurnAngle) * Time.deltaTime, 1);
                    
                    var angle = Mathf.Atan2(finalDir.y, finalDir.x);
                    head.transform.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * angle);

                    if (i == endpoints.Length)
                    {
                        var perc = 1 - (dist-15) / startDist;
                        perc = Mathf.Clamp01(1.3f * perc - 0.3f);
                        // perc = 1 / (1 + Mathf.Exp(4 - 7 * perc));
                        
                        curSpeed = Mathf.Lerp(segmentSpeeds[i - 1], 0, perc);
                        
                        cam.orthographicSize = Mathf.SmoothStep(zoomOutCamSize, camFp.baseSize, perc);
                        cam.transform.position = camStart + (Vector3) camDir * Mathf.SmoothStep(0, camDist, perc);
                        cam.transform.rotation = Quaternion.Slerp(camStartRot, camEndRot, perc);
                    }
                    else
                    {
                        curSpeed = Mathf.Clamp(curSpeed + 7 * Time.deltaTime * MathF.Sign(speed - curSpeed), Mathf.Min(curSpeed, speed), Mathf.Max(curSpeed, speed));
                    }

                    _headRigid.linearVelocity = curSpeed * finalDir;
                    
                    RippleSegments();
                }
            }
            
            var origVel = _headRigid.linearVelocity;
            for (float t = 0; t < 0.7f; t += Time.fixedDeltaTime)
            {
                yield return new WaitForFixedUpdate();

                _headRigid.linearVelocity = origVel * Mathf.SmoothStep(1, 0, t / 0.7f);
                RippleSegments();
            }

            yield return new WaitForSeconds(0.7f);
            foreach (var c in GetComponentsInChildren<FloppyBrain>()) c.enabled = false;

            // boss shriek
            camFp.ScreenShake(spitTime, 1.5f);
            GetComponentInChildren<SpitController>().Spit(spitTime);
            
            yield return new WaitForSeconds(0.2f);
            player.GetComponent<CustomRigidbody2D>().linearVelocity = playerMoveVel * ((Vector2) (player.transform.position - head.transform.position)).normalized;
            player.GetComponent<Movement>().autoPilot = true;
            
            for (float t = 0; t < spitTime-0.2f; t += Time.fixedDeltaTime)
            {
                yield return new WaitForFixedUpdate();
                
                var rot = wiggleScale * jawWiggle.Evaluate(t / (spitTime-0.2f));
                _jawGrab.RotTo(rot);
            }
            
            yield return new WaitForSeconds(1);

            camStart = cam.transform.position;
            camDir = ((Vector2)(player.transform.position - camStart)).normalized;
            camDist = ((Vector2)(player.transform.position - camStart)).magnitude;
            for (float t = 0; t < 1; t += Time.fixedDeltaTime)
            {
                yield return new WaitForFixedUpdate();
                
                cam.transform.position = camStart + (Vector3) camDir * Mathf.SmoothStep(0, camDist, t);
            }
            
            // cleanup
            player.GetComponent<Movement>().autoPilot = false;
            for (var i = 0; i < fadeIn.transform.parent.childCount; i++)
            {
                fadeIn.transform.parent.GetChild(i).gameObject.SetActive(true);
            }
            camFp.Enabled = true;
            foreach (var c in GetComponentsInChildren<FloppyBrain>()) c.enabled = true;
            DestroyImmediate(portal);
            player.GetComponent<Movement>().SetInputBlocked(false);
            gradient.SetActive(false);
            defenses.SetActive(true);
            boundaryCircle.transform.localScale = oldCircleScale;
            emission.rateOverTimeMultiplier /= 5;
            shape.scale /= 5;
            main.maxParticles /= 5;
            
            _inCutscene = false;
        }
    }
}
